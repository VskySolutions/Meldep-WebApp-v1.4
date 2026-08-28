using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;

namespace Vsky.Services.ProjectWeeklyPlan
{
    public class ProjectWeeklyService : IProjectWeeklyService
    {
        #region Initialization
        private readonly IRepository<Models.ProjectWeeklyPlan> _projectWeeklyPlanRepository;
        private readonly IRepository<Notes> _notesRepository;
        private readonly IRepository<DropDown> _dropdownRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        public ProjectWeeklyService(
            IRepository<Models.ProjectWeeklyPlan> projectWeeklyPlanRepository,
            IRepository<Notes> notesRepository,
            IRepository<DropDown> dropdownRepository,
            UserManager<ApplicationUser> userManager,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _projectWeeklyPlanRepository = projectWeeklyPlanRepository;
            _notesRepository = notesRepository;
            _dropdownRepository = dropdownRepository;
            _userManager = userManager;
            _applicationUserRoleService = applicationUserRoleService;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetById
        public async Task<Models.ProjectWeeklyPlan> GetById(string SiteId, string Id)
        {
            var query = _projectWeeklyPlanRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Id == Id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<Models.ProjectWeeklyPlan> GetByProjectId(string SiteId, string ProjectId)
        {
            var query = _projectWeeklyPlanRepository.TableNoTracking
                .Where(x => !x.Deleted && x.SiteId == SiteId && x.ProjectId == ProjectId)
                .Include(x => x.ProjectWeeklyPlanDates.Where(m => !m.Deleted))
                .Include(x => x.ProjectWeeklyPlanDates).ThenInclude(m => m.ProjectWeeklyPlanDatesLines.Where(m => !m.Deleted));

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllList
        public async Task<IPagedList<Models.ProjectWeeklyPlan>> GetAllProjectWeeklyPlanListAsync(
            string SiteId,
            string LoggedUserId,
            string employeeId,
            DateTime GetDateTime,
            string PlanTypeId,
            string SearchText,
            List<string> ProjectIds,
            List<string> ProjectCoordinatorIds,
            List<string> ProjectLeadsIds,
            List<string> ProjectStatusIds,
            int Status,
            List<string> ProjectPriorityIds,
            List<string> ProjectTypeIds,
            List<string> CustomerIds,
            List<string> CompanyContactIds,
            string SortBy,
            bool Descending,
            int PageIndex = 1,
            int PageSize = int.MaxValue)
        {
            var query = _projectWeeklyPlanRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
            var dropdownData = _dropdownRepository.TableNoTracking.FirstOrDefault(x => x.Id == PlanTypeId);
            var NotesType = "Project " + dropdownData.DropDownValue + " Plan";
            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, SiteId);
            int diff = (7 + (GetDateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = GetDateTime.AddDays(-1 * diff);

            if (!IsAdmin)
            {
                query = query.Where(x =>
                    x.Project.CreatedById == LoggedUserId ||
                    x.CreatedById == LoggedUserId ||
                    x.Project.ProjectEmployeeMappings.Any(m =>
                        !m.Deleted &&
                        m.EmployeeId == employeeId &&
                        m.ProjectEmployeeRoleMappings.Any(r =>
                            !r.Deleted &&
                            r.SitesProjectRoles.SitesProjectRolesPermissions.Any(p =>
                                !p.Deleted &&
                                (p.FullAccess || p.ViewOnly || p.Notes)
                            )
                        )
                    )
                );
            }

            // project filter
            if (ProjectIds != null && ProjectIds.Any())
                query = query.Where(x => ProjectIds.Contains(x.ProjectId));

            if (ProjectStatusIds != null && ProjectStatusIds.Any())
                query = query.Where(x => ProjectStatusIds.Contains(x.Project.ProjectStatusId));

            if (Status == 0)
                query = query.Where(x => x.Project.Active == false);
            else if (Status == 1)
                query = query.Where(x => x.Project.Active == true);

            if (ProjectCoordinatorIds != null && ProjectCoordinatorIds.Any())
            {
                query = query.Where(x =>
                    x.Project.ProjectEmployeeMappings.Any(m =>
                        ProjectCoordinatorIds.Contains(m.EmployeeId) &&
                        !m.Deleted &&
                        m.ProjectEmployeeRoleMappings.Any(r =>
                            !r.Deleted &&
                            r.SitesProjectRoles.MasterProjectRoles.Name == "Project Coordinator"
                        )
                    )
                );
            }

            if (ProjectLeadsIds != null && ProjectLeadsIds.Any())
            {
                query = query.Where(x =>
                    x.Project.ProjectEmployeeMappings.Any(m =>
                        ProjectLeadsIds.Contains(m.EmployeeId) &&
                        !m.Deleted &&
                        m.ProjectEmployeeRoleMappings.Any(r =>
                            !r.Deleted &&
                            r.SitesProjectRoles.MasterProjectRoles.Name == "Project Lead"
                        )
                    )
                );
            }

            if (ProjectPriorityIds != null && ProjectPriorityIds.Any())
                query = query.Where(x => ProjectPriorityIds.Contains(x.Project.ProjectPriorityId));

            if (ProjectTypeIds != null && ProjectTypeIds.Any())
                query = query.Where(x => ProjectTypeIds.Contains(x.Project.ProjectTypeId));

            if (CustomerIds != null && CustomerIds.Any())
                query = query.Where(x => CustomerIds.Contains(x.Project.CustomerId));

            if (CompanyContactIds != null && CompanyContactIds.Any())
                query = query.Where(x => CompanyContactIds.Contains(x.Project.CompanyContactId));

            if (!string.IsNullOrEmpty(SearchText))
                query = query.Where(m => m.Project.Name.ToLower().Contains(SearchText.ToLower()));

            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                var orderBy = $"{GetOrderBy(SortBy)} {(Descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.Project.Name);

            query = query.Select(x => new Models.ProjectWeeklyPlan
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    PlanApproverId = x.Project.PlanApproverId,
                    PlanApprover = new Employee
                    {
                        Id = x.Project.PlanApprover.Id,
                        Person = new Person
                        {
                            Id = x.Project.PlanApprover.Person.Id,
                            FirstName = x.Project.PlanApprover.Person.FirstName,
                            LastName = x.Project.PlanApprover.Person.LastName,
                            PrimaryEmailAddress = x.Project.PlanApprover.Person.PrimaryEmailAddress,
                            PrimaryPhoneNumber = x.Project.PlanApprover.Person.PrimaryPhoneNumber,
                        }
                    },
                    ProjectCharterGroupByList = x.Project.ProjectEmployeeMappings
                        .Where(m => !m.Deleted)
                        .SelectMany(m => m.ProjectEmployeeRoleMappings
                            .Where(r => !r.Deleted)
                            .Select(r => new
                            {
                                EmployeeId = m.EmployeeId,

                                EmployeeIdValue = m.Employee.Id,

                                PersonId = m.Employee.Person.Id,
                                FirstName = m.Employee.Person.FirstName,
                                LastName = m.Employee.Person.LastName,
                                PrimaryEmailAddress = m.Employee.Person.PrimaryEmailAddress,
                                PrimaryPhoneNumber = m.Employee.Person.PrimaryPhoneNumber,

                                RoleId = r.SitesProjectRoles.Id,
                                RoleName = r.SitesProjectRoles.MasterProjectRoles.Name
                            }))
                        .GroupBy(x => new
                        {
                            Id = x.RoleId,
                            Value = x.RoleName
                        })
                        .Select(group => new ProjectCharterGroupBy
                        {
                            GroupId = group.Key.Id.ToString(),
                            GroupValue = group.Key.Value,

                            EmployeeMappingList = group
                                .OrderBy(m => m.FirstName)
                                .Select(g => new ProjectEmployeeMapping
                                {
                                    EmployeeId = g.EmployeeId,

                                    Employee = new Employee
                                    {
                                        Id = g.EmployeeIdValue,

                                        Person = new Person
                                        {
                                            Id = g.PersonId,
                                            FirstName = g.FirstName,
                                            LastName = g.LastName,
                                            PrimaryEmailAddress = g.PrimaryEmailAddress,
                                            PrimaryPhoneNumber = g.PrimaryPhoneNumber
                                        }
                                    }
                                })
                                .ToList()
                        })
                        .OrderBy(group => group.GroupValue)
                        .ToList(),
                    CurrentUserManage =
                    x.Project.CreatedById == LoggedUserId ||
                    x.CreatedById == LoggedUserId ||
                    x.Project.ProjectEmployeeMappings
                        .Where(m =>
                            !m.Deleted &&
                            m.EmployeeId == employeeId)
                        .Any(m =>
                            m.ProjectEmployeeRoleMappings
                                .Where(r => !r.Deleted)
                                .Any(r =>
                                    r.SitesProjectRoles
                                        .SitesProjectRolesPermissions
                                        .Any(p =>
                                            !p.Deleted &&
                                            p.FullAccess))),

                    CurrentUserView =
                    x.Project.ProjectEmployeeMappings
                        .Where(m =>
                            !m.Deleted &&
                            m.EmployeeId == employeeId)
                        .Any(m =>
                            m.ProjectEmployeeRoleMappings
                                .Where(r => !r.Deleted)
                                .Any(r =>
                                    r.SitesProjectRoles
                                        .SitesProjectRolesPermissions
                                        .Any(p =>
                                            !p.Deleted &&
                                            p.ViewOnly))),

                    CurrentUserNotes =
                    x.Project.ProjectEmployeeMappings
                        .Where(m =>
                            !m.Deleted &&
                            m.EmployeeId == employeeId)
                        .Any(m =>
                            m.ProjectEmployeeRoleMappings
                                .Where(r => !r.Deleted)
                                .Any(r =>
                                    r.SitesProjectRoles
                                        .SitesProjectRolesPermissions
                                        .Any(p =>
                                            !p.Deleted &&
                                            p.Notes))),
                    ProjectMessageCount = x.Project.ProjectsMessages.Count(m => !m.Deleted),
                    ProjectNotesCount = _notesRepository.TableNoTracking.Count(m => !m.Deleted && m.SubModuleId == x.ProjectId && m.Type == NotesType)
                },
                ProjectWeeklyPlanDates = x.ProjectWeeklyPlanDates.Where(m => !m.Deleted && m.PlanTypeId == PlanTypeId && m.WeekDate >= startOfWeek && (IsAdmin || !m.IsApproved && !m.IsCompleted))
                .Select(d => new ProjectWeeklyPlanDates
                {
                    Id = d.Id
                })
                .ToList()
            });

            var list = new PagedList<Models.ProjectWeeklyPlan>(query, PageIndex, PageSize);
            return list;
        }

        private async Task<bool> IsCurrentUserAdmin(string CId, string SiteId)
        {
            var userdata = await _userManager.FindByIdAsync(CId);
            var user = await _userManager.FindByNameAsync(userdata.UserName);
            //var roles = await _userManager.GetRolesAsync(user);
            var roles = await _applicationUserRoleService.GetRoleNamesByUserAndSite(user.Id, SiteId);
            var isAdmin = roles.Contains("Admin") || roles.Contains("Site Super Admin") || roles.Contains("System Super Admin") || roles.Contains("Project Admin");

            return isAdmin;
        }
        #endregion

        #region Insert & Update & Delete
        public void InsertProjectWeeklyPlan(Models.ProjectWeeklyPlan entity)
        {
            _projectWeeklyPlanRepository.Insert(entity);
        }
        public void UpdateProjectWeeklyPlan(Models.ProjectWeeklyPlan entity)
        {
            _projectWeeklyPlanRepository.Update(entity);
        }
        public void DeleteProjectWeeklyPlan(Models.ProjectWeeklyPlan entity)
        {
            entity.Deleted = true;
            _projectWeeklyPlanRepository.Update(entity);
        }
        #endregion
    }
}
