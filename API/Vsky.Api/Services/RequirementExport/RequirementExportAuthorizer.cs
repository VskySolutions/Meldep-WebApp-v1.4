using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Api.Services.RequirementExport
{
    public interface IRequirementExportAuthorizer
    {
        /// <summary>The same project rule that GET requirement/details/{id} applies through [RecordScope].</summary>
        Task<bool> CanAccessRequirementAsync(string userId, string siteId, string requirementId);

        /// <summary>View permission on any of the module's menu links in the active site.</summary>
        Task<bool> CanViewModuleAsync(string userId, string siteId, IReadOnlyList<string> menuLinks);
    }

    // Title: RequirementExportAuthorizer
    // Description: Always enforced for export, independent of the global Off/Audit/Enforce rollout mode, so
    // the export cannot reveal module data the user could not open in that module. System admins pass, as
    // in AuthorizationEnforcementFilter.
    public sealed class RequirementExportAuthorizer : IRequirementExportAuthorizer
    {
        private readonly AdminAccess _adminAccess;
        private readonly IPermissionService _permissionService;
        private readonly ProjectRecordAccess _projectRecordAccess;

        public RequirementExportAuthorizer(AdminAccess adminAccess, IPermissionService permissionService, ProjectRecordAccess projectRecordAccess)
        {
            _adminAccess = adminAccess;
            _permissionService = permissionService;
            _projectRecordAccess = projectRecordAccess;
        }

        public async Task<bool> CanAccessRequirementAsync(string userId, string siteId, string requirementId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            if (await _adminAccess.IsSystemAdminAsync(userId))
                return true;

            var snapshot = await _permissionService.GetSnapshotAsync(userId, siteId);
            if (!snapshot.IsSiteMember)
                return false;

            return await _projectRecordAccess.CanAccessAsync<Requirement>(userId, siteId, requirementId);
        }

        public async Task<bool> CanViewModuleAsync(string userId, string siteId, IReadOnlyList<string> menuLinks)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            if (await _adminAccess.IsSystemAdminAsync(userId))
                return true;

            var snapshot = await _permissionService.GetSnapshotAsync(userId, siteId);
            return snapshot.IsSiteMember && snapshot.Allows(menuLinks, AccessLevel.View);
        }
    }
}
