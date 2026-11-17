using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Common;

namespace Vsky.Services.Authorization
{
    public enum RecordRule
    {
        Project,
        Lead,
        HelpDesk,
        Leave,
        SopProcess
    }

    /// <summary>
    /// The rule the Test Case, Issue and Requirement lists already use, applied to requests by ID:
    /// admins see everything, others see records they or the project's creator made, or projects
    /// they're assigned to with a project role. No new business rule is introduced here.
    /// </summary>
    public class ProjectRecordAccess
    {
        private static readonly string[] AdminRoleNames = { "Admin", "Site Super Admin", "System Super Admin", "Project Admin" };

        private readonly ApplicationDbContext _db;
        private readonly IApplicationUserRoleService _userRoles;
        private readonly ICommonService _commonService;

        public ProjectRecordAccess(ApplicationDbContext db, IApplicationUserRoleService userRoles, ICommonService commonService)
        {
            _db = db;
            _userRoles = userRoles;
            _commonService = commonService;
        }

        /// <summary>Each rule repeats what that module's own list already does for this user.</summary>
        public async Task<bool> CanAccessAsync(RecordRule rule, string userId, string siteId, string recordId)
        {
            var normalizedRoles = await _userRoles.GetNormalizedRoleNamesByUserAndSite(userId, siteId);
            var roleNames = await _userRoles.GetRoleNamesByUserAndSite(userId, siteId);
            bool HasAdminRole() => roleNames.Any(role => AdminRoleNames.Contains(role));
            string EmployeeId() => _commonService.GetEmployeeIdByUserIdAndEmail(siteId, userId);

            switch (rule)
            {
                case RecordRule.Project:
                    if (HasAdminRole() || roleNames.Contains("Project Manager"))
                        return true;
                    var employeeId = EmployeeId();
                    return !await _db.Set<Project>().AsNoTracking().AnyAsync(p => p.Id == recordId)
                           || await _db.Set<Project>().AsNoTracking().AnyAsync(project => project.Id == recordId
                               && (project.CreatedById == userId
                                   || project.ProjectEmployeeMappings.Any(mapping =>
                                       !mapping.Deleted
                                       && mapping.EmployeeId == employeeId
                                       && mapping.ProjectEmployeeRoleMappings.Any(role =>
                                           !role.Deleted
                                           && role.SitesProjectRoles.SitesProjectRolesPermissions.Any(permission =>
                                               !permission.Deleted
                                               && (permission.FullAccess || permission.ViewOnly || permission.Notes))))));

                case RecordRule.Lead:
                    if (HasAdminRole())
                        return true;
                    var lead = await _db.Set<Lead>().AsNoTracking().Where(x => x.Id == recordId).Select(x => new { x.LeadGroupId }).FirstOrDefaultAsync();
                    return lead == null || await _db.Set<LeadUserGroupMapping>().AsNoTracking().AnyAsync(x =>
                        !x.Deleted && x.Active && x.SiteId == siteId && x.UserId == userId && x.LeadGroupId == lead.LeadGroupId);

                case RecordRule.HelpDesk:
                    if (normalizedRoles.Any(role => role == "admin" || role == "support team"))
                        return true;
                    var requester = EmployeeId();
                    var ticket = await _db.Set<HelpDesk>().AsNoTracking().Where(x => x.Id == recordId).Select(x => new { x.RequesterId }).FirstOrDefaultAsync();
                    return ticket == null || (requester != null && ticket.RequesterId == requester);

                case RecordRule.Leave:
                    // The union of the leave list (owner, or admin and HR) and the approval list.
                    if (normalizedRoles.Any(role => role == "admin" || role == "hr"))
                        return true;
                    var employee = EmployeeId();
                    var leave = await _db.Set<EmployeeLeave>().AsNoTracking().Where(x => x.Id == recordId).Select(x => new { x.EmployeeId, x.LeaveApproverId }).FirstOrDefaultAsync();
                    return leave == null || (employee != null && (leave.EmployeeId == employee || leave.LeaveApproverId == employee));

                case RecordRule.SopProcess:
                    if (normalizedRoles.Any(role => role == "sop approver" || role == "sop editor"))
                        return true;
                    if (!await _db.Set<SOPProcess>().AsNoTracking().AnyAsync(x => x.Id == recordId))
                        return true;
                    var latestStatus = await _db.Set<SOPProcess>().AsNoTracking().Where(x => x.Id == recordId)
                        .Select(x => x.SOPProcessStatusLog.OrderByDescending(log => log.CreatedOnUtc).Select(log => log.Status.DropDownValue.ToLower()).FirstOrDefault())
                        .FirstOrDefaultAsync();
                    return latestStatus == "published";

                default:
                    return false;
            }
        }

        /// <summary>A record that doesn't exist returns true, so the action gives its usual response.</summary>
        public async Task<bool> CanAccessAsync<TEntity>(string userId, string siteId, string recordId) where TEntity : class
        {
            var record = await _db.Set<TEntity>()
                .AsNoTracking()
                .Where(x => EF.Property<string>(x, "Id") == recordId)
                .Select(x => new
                {
                    ProjectId = EF.Property<string>(x, "ProjectId"),
                    CreatedById = EF.Property<string>(x, "CreatedById")
                })
                .FirstOrDefaultAsync();

            if (record == null || record.CreatedById == userId)
                return true;

            var roleNames = await _userRoles.GetRoleNamesByUserAndSite(userId, siteId);
            if (roleNames.Any(role => AdminRoleNames.Contains(role)))
                return true;

            var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(siteId, userId);

            return await _db.Set<Project>()
                .AsNoTracking()
                .AnyAsync(project => project.Id == record.ProjectId
                    && (project.CreatedById == userId
                        || project.ProjectEmployeeMappings.Any(mapping =>
                            !mapping.Deleted
                            && mapping.EmployeeId == employeeId
                            && mapping.ProjectEmployeeRoleMappings.Any(role =>
                                !role.Deleted
                                && role.SitesProjectRoles.SitesProjectRolesPermissions.Any(permission =>
                                    !permission.Deleted
                                    && (permission.FullAccess || permission.ViewOnly || permission.Notes))))));
        }
    }
}
