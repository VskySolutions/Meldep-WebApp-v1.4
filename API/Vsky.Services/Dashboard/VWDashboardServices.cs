using System;
using System.Collections.Generic;
using System.Linq;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Globalization;
using SendGrid.Helpers.Mail;
using AngleSharp.Dom;

namespace Vsky.Services.Dashboard
{
    public class VWDashboardServices : IVWDashboardServices
    {
        #region Serivces
        private readonly IRepository<VW_Customer> _vwCustomerListRepository;
        private readonly IRepository<VW_Project> _vwCustomerProjectListRepository;
        private readonly IRepository<VW_ProjectSwimLane> _vwProjectSwimLaneRepository;
        private readonly IRepository<VW_ProjectTask> _vwProjectTaskRepository;
        private readonly IRepository<VW_EmployeeTaskActivitySummary> _vwEmployeeTaskActivitySummaryRepository;
        private readonly IRepository<VW_ProjectModules> _vwProjectModulesRepository;
        private readonly IRepository<VW_ProjectTaskActivities> _vwProjectActivitiesRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Tags> _tagsRepository;
        private readonly IRepository<VW_UserTaskTags> _vwUserTaskTagsRepository;
        private readonly IRepository<VW_UserProjectPinned> _vwUserProjectPinnedRepository;
        private readonly IRepository<VW_UserProjectColor> _vwUserProjectColorRepository;

        public VWDashboardServices(
            IRepository<VW_Customer> vwCustomerListRepository,
            IRepository<VW_Project> vwCustomerProjectListRepositor,
            IRepository<VW_ProjectSwimLane> vwProjectSwimLaneRepository,
            IRepository<VW_ProjectTask> vwProjectTaskRepository,
            IRepository<VW_EmployeeTaskActivitySummary> vwEmployeeTaskActivitySummaryRepository,
            IRepository<VW_ProjectModules> vwProjectModulesRepository,
            IRepository<VW_ProjectTaskActivities> vwProjectActivitiesRepository,
            UserManager<ApplicationUser> userManager,
            IRepository<Tags> tagsRepository,
            IRepository<VW_UserTaskTags> vwUserTaskTagsRepository,
            IRepository<VW_UserProjectPinned> vwUserProjectPinnedRepository,
            IRepository<VW_UserProjectColor> vwUserProjectColorRepository)
        {
            _vwCustomerListRepository = vwCustomerListRepository;
            _vwCustomerProjectListRepository = vwCustomerProjectListRepositor;
            _vwProjectSwimLaneRepository = vwProjectSwimLaneRepository;
            _vwProjectTaskRepository = vwProjectTaskRepository;
            _vwEmployeeTaskActivitySummaryRepository = vwEmployeeTaskActivitySummaryRepository;
            _vwProjectModulesRepository = vwProjectModulesRepository;
            _vwProjectActivitiesRepository = vwProjectActivitiesRepository;
            _userManager = userManager;
            _tagsRepository = tagsRepository;
            _vwUserTaskTagsRepository = vwUserTaskTagsRepository;
            _vwUserProjectPinnedRepository = vwUserProjectPinnedRepository;
            _vwUserProjectColorRepository = vwUserProjectColorRepository;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region
        public async Task<IPagedList<VW_Customer>> GetAllCustomerList(
            string SiteId,
            string SearchText,
            string logginuser = "",
            string projectId = "",
            List<string> CustomerIds = null,
            List<string> CustomerTypeIds = null,
            List<string> CustomerAssignToIds = null,
            List<string> ParentCustomerIds = null,
            List<string> ProjectTypeIds = null,
            List<string> ProjectIds = null,
            List<string> ProjectStatusIds = null,
            List<string> ProjectPriorityIds = null,
            List<string> ProjectCoordinatorIds = null,
            List<string> ProjectLeadsIds = null,
            int Status = 0,
            List<string> CompanyContactIds = null,
            string Year = "",
            string SortBy = "",
            bool Descending = false,
            int page = 1,
            int pageSize = int.MaxValue)
        {
            var query = _vwCustomerListRepository.TableNoTracking
                .Where(m => m.SiteId == SiteId)
                .Include(m => m.CustomerTaskStatusSummary)
                .Include(m => m.Projects.Where(m => !m.IsTemplate)
                    .OrderByDescending(m => m.IsPinned)
                    .ThenByDescending(m => m.CreatedOnUtc))
                .Include(m => m.Projects).ThenInclude(m => m.ProjectTaskStatusSummary)
                .Include(m => m.Projects).ThenInclude(m => m.ProjectIssueStatusSummary)
                .Include(m => m.Projects).ThenInclude(m => m.ProjectRequirementStatusSummary)
                .Include(m => m.Projects).ThenInclude(x => x.ProjectUserMappings.Where(a => a.AspNetUserId == logginuser))
                .AsQueryable();
            var userdata = _userManager.FindByIdAsync(logginuser).GetAwaiter().GetResult();
            var user = _userManager.FindByNameAsync(userdata.UserName).GetAwaiter().GetResult();

            if (user != null && !user.Deleted && user.Active)
            {
                //Get user roles
                var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
                bool IsAdmin = await IsCurrentUserAdmin(logginuser);
                if (!IsAdmin)
                    query = query.Where(p => p.Projects
                                .Any(proj => proj.ProjectUserMappings
                                .Any(m => !m.Deleted && m.AspNetUserId == logginuser && (m.FullAccess || m.ViewOnly || m.Notes))));

                if (!string.IsNullOrEmpty(Year))
                {
                    int YearINT = Convert.ToInt32(Year);
                    query = query.Where(m => m.Projects.Any(m => m.Year == YearINT));
                }

                if (CustomerIds != null && CustomerIds.Any())
                    query = query.Where(m => CustomerIds.Contains(m.Id));

                if (CustomerTypeIds != null && CustomerTypeIds.Any())
                    query = query.Where(m => CustomerTypeIds.Contains(m.CustomerTypeId));

                if (CustomerAssignToIds != null && CustomerAssignToIds.Any())
                    query = query.Where(m => CustomerAssignToIds.Contains(m.AssignToId));

                if (ParentCustomerIds != null && ParentCustomerIds.Any())
                    query = query.Where(m => ParentCustomerIds.Contains(m.ParentCustomerId));

                if (ProjectTypeIds != null && ProjectTypeIds.Any())
                    query = query.Where(m => m.Projects.Any(p => ProjectTypeIds.Contains(p.ProjectTypeId)));
                //query = query.Where(m => m.Projects.Any(m => ProjectTypeIds.Any(x => x ==m.ProjectTypeId )));

                if (ProjectIds != null && ProjectIds.Any())
                    query = query.Where(m => m.Projects.Any(m => ProjectIds.Contains(m.Id)));

                if (ProjectStatusIds != null && ProjectStatusIds.Any())
                    query = query.Where(m => m.Projects.Any(m => ProjectStatusIds.Contains(m.ProjectStatusId)));

                if (ProjectPriorityIds != null && ProjectPriorityIds.Any())
                    query = query.Where(m => m.Projects.Any(m => ProjectPriorityIds.Contains(m.ProjectPriorityId)));

                if (Status == 0)
                    query = query.Where(m => m.Projects.Any(m => m.Active == false));
                else if (Status == 1)
                    query = query.Where(m => m.Projects.Any(m => m.Active == true));

                if (ProjectCoordinatorIds != null && ProjectCoordinatorIds.Any())
                    query = query.Where(m => m.Projects
                                .Any(proj => proj.ProjectEmployeeMappings
                                .Any(m => ProjectCoordinatorIds.Contains(m.EmployeeId) && m.EmployeeRoleDropdown.DropDownValue == "Project Coordinator")));

                if (ProjectCoordinatorIds != null && ProjectCoordinatorIds.Any())
                    query = query.Where(m => m.Projects
                                .Any(proj => proj.ProjectEmployeeMappings
                                .Any(m => ProjectCoordinatorIds.Contains(m.EmployeeId) && m.EmployeeRoleDropdown.DropDownValue == "Project Lead")));

                if (CompanyContactIds != null && CompanyContactIds.Any())
                    query = query.Where(m => m.Projects.Any(m => CompanyContactIds.Contains(m.CompanyContactId)));

                if (!string.IsNullOrEmpty(SearchText))
                {
                    query = query.Where(m =>
                        m.CustomerName.Contains(SearchText.ToLower()) ||
                        m.CustomerType.Contains(SearchText.ToLower()) ||
                        m.AssignTo.Contains(SearchText.ToLower()) ||
                        m.ParentCustomerName.Contains(SearchText.ToLower()) ||
                        m.Projects.Any(n => n.Name.Contains(SearchText.ToLower())) ||
                        m.Projects.Any(n => n.Status.Contains(SearchText.ToLower())) ||
                        m.Projects.Any(n => n.Priority.Contains(SearchText.ToLower()))
                    );
                }

                //if (!string.IsNullOrEmpty(projectId))
                //    query = query.Where(m => m.Projects.Any(x => x.Id == projectId));

                if (!string.IsNullOrWhiteSpace(SortBy))
                {
                    string Sort = SortBy + " " + (Descending ? "desc" : "asc");
                    query = query.OrderBy(Sort);
                }
                else
                    query = query.OrderByDescending(m => m.CreatedOnUtc);

            }

            var list = new PagedList<VW_Customer>(query, page, pageSize);
            return list;
        }
        
        public async Task<IPagedList<VW_Project>> GetAllCustomerProjectList(
            string SiteId,
            string filterProject = "",
            string logginuser = "",
            string searchText = "",
            List<string> CustomerIds = null,
            List<string> ProjectCategoryId = null,
            List<string> ProjectSubCategoryIds = null,
            List<string> ProjectTypeIds = null,
            List<string> ProjectIds = null,
            List<string> ProjectStatusIds = null,
            List<string> ProjectPriorityIds = null,
            List<string> ProjectCoordinatorIds = null,
            List<string> ProjectLeadsIds = null,
            int Status = 0,
            List<string> CompanyContactIds = null,
            string Year = "",
            string SortBy = "",
            bool Descending = false,
            int page = 1,
            int pageSize = int.MaxValue)
        {
            var query = _vwCustomerProjectListRepository.TableNoTracking
                .Include(x => x.ProjectUserMappings.Where(a => a.AspNetUserId == logginuser)).Where(m => m.SiteId == SiteId);

            var userdata = _userManager.FindByIdAsync(logginuser).GetAwaiter().GetResult();
            var user = _userManager.FindByNameAsync(userdata.UserName).GetAwaiter().GetResult();
            var userPinnedProjectIds = await GetUserPinnedProjectIdListByUserId(logginuser);
            var userProjectColors = await GetUserProjectColorListByUserId(logginuser);

            if (user != null && !user.Deleted && user.Active)
            {
                //Get user roles
                var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();

                var isAdmin = roles.Contains("Admin") || roles.Contains("Site Super Admin") || roles.Contains("System Super Admin");
                if (!isAdmin)
                {
                    query = query.Where(p => p.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == logginuser && (m.FullAccess || m.ViewOnly || m.Notes)));
                }

                if (!string.IsNullOrEmpty(Year))
                {
                    int YearINT = Convert.ToInt32(Year);
                    query = query.Where(m => m.Year == YearINT);
                }

                if (Status == 0)
                    query = query.Where(x => x.Active == false);
                else if (Status == 1)
                    query = query.Where(x => x.Active == true);

                if (ProjectTypeIds != null && ProjectTypeIds.Any())
                    query = query.Where(m => ProjectTypeIds.Contains(m.ProjectTypeId));

                var filteredCustomerIds = CustomerIds.Where(id => id != null).ToList();

                if (filteredCustomerIds.Any())
                    query = query.Where(m => m.CustomerId != null && filteredCustomerIds.Contains(m.CustomerId));

                if (ProjectCoordinatorIds != null && ProjectCoordinatorIds.Any())
                    query = query.Where(x => x.ProjectEmployeeMappings.Any(m => ProjectCoordinatorIds.Contains(m.EmployeeId) && !m.Deleted && m.EmployeeRoleDropdown.DropDownValue == "Project Coordinator"));

                if (ProjectLeadsIds != null && ProjectLeadsIds.Any())
                    query = query.Where(x => x.ProjectEmployeeMappings.Any(m => ProjectLeadsIds.Contains(m.EmployeeId) && !m.Deleted && m.EmployeeRoleDropdown.DropDownValue == "Project Lead"));

                if (CompanyContactIds != null && CompanyContactIds.Any())
                    query = query.Where(x => CompanyContactIds.Contains(x.CompanyContactId));

                if (ProjectStatusIds != null && ProjectStatusIds.Any())
                    query = query.Where(m => ProjectStatusIds.Contains(m.ProjectStatusId));

                if (ProjectIds != null && ProjectIds.Any())
                    query = query.Where(m => ProjectIds.Contains(m.Id));

                if (ProjectPriorityIds != null && ProjectPriorityIds.Any())
                    query = query.Where(m => ProjectPriorityIds.Contains(m.ProjectPriorityId));

                if (!string.IsNullOrEmpty(filterProject))
                {
                    query = query.Where(m =>
                        m.CustomerName.Contains(filterProject.ToLower()) ||
                        m.Name.Contains(filterProject.ToLower())
                    );
                }

                if (!string.IsNullOrWhiteSpace(SortBy))
                {
                    var orderBy = $"{GetOrderBy(SortBy)} {(Descending ? "desc" : "asc")}";
                    query = query.OrderByDescending(x => userPinnedProjectIds.Contains(x.Id)).ThenBy(orderBy);
                }
                else
                {
                    query = query.OrderByDescending(x => userPinnedProjectIds.Contains(x.Id)).ThenByDescending(x => x.CreatedOnUtc);
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    searchText = searchText.ToLower();

                    query = query.Where(m =>
                        m.CustomerName.ToLower().Contains(searchText) ||
                        m.Name.ToLower().Contains(searchText) ||
                        m.ProjectTypeName.ToLower().Contains(searchText) ||
                        m.Status.ToLower().Contains(searchText) ||
                        m.Priority.ToLower().Contains(searchText));
                }
            }
            
            var list = new PagedList<VW_Project>(query, page, pageSize);

            foreach (var item in list)
            {
                item.IsPinned = userPinnedProjectIds?.Contains(item.Id) ?? false;
                item.ProjectColor = userProjectColors?
                    .FirstOrDefault(x => x.ProjectId == item.Id)?.Color ?? "";
            }

            return list;
        }

        public IPagedList<VW_ProjectSwimLane> GetAllProjectSwimlaneList(string SiteId, string filterProjectSwimlane = "", List<string> ProjectId = null, string SortBy = "",
            bool Descending = false, int page = 1, int pageSize = int.MaxValue)
        {
            var query = _vwProjectSwimLaneRepository.TableNoTracking.Where(m => m.SiteId == SiteId);

            if (ProjectId != null && ProjectId.Any())
                query = query.Where(m => ProjectId.Contains(m.ProjectId));

            if (!string.IsNullOrEmpty(filterProjectSwimlane))
            {
                query = query.Where(m =>
                    m.Name.Contains(filterProjectSwimlane.ToLower()) ||
                    m.ProjectName.Contains(filterProjectSwimlane.ToLower())
                );
            }

            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                string Sort = SortBy + " " + (Descending ? "desc" : "asc");
                query = query.OrderBy(Sort);
            }
            else
                query = query.OrderBy(m => m.Name);

            var list = new PagedList<VW_ProjectSwimLane>(query, page, pageSize);
            return list;
        }

        public async Task<IPagedList<VW_ProjectModules>> GetAllProjectModulesList(
            string SiteId,
            bool isShowCloseStatus,
            string filterModule = "",
            string LoggedUserId = "",
            List<string> ProjectModuleIds = null,
            List<string> ProjectId = null,
            List<string> ProjectModuleStatusIds = null,
            string SortBy = "",
            bool Descending = false,
            int page = 1,
            int pageSize = int.MaxValue)
        {
            var query = _vwProjectModulesRepository.TableNoTracking.Include(x => x.Project.ProjectUserMappings.Where(m => m.AspNetUserId == LoggedUserId)).Where(x => x.SiteId == SiteId);
            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId);
            if (!IsAdmin)
                query = query.Where(p => p.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId && (m.FullAccess || m.ViewOnly || m.Notes)));

            if (ProjectModuleIds != null && ProjectModuleIds.Any())
                query = query.Where(m => ProjectModuleIds.Contains(m.Id));

            if (ProjectId != null && ProjectId.Any())
                query = query.Where(m => ProjectId.Contains(m.ProjectId));

            if (!string.IsNullOrEmpty(filterModule))
            {
                query = query.Where(m =>
                    m.Name.Contains(filterModule.ToLower()) ||
                    m.ProjectName.Contains(filterModule.ToLower()));
            }

            //if (isShowCloseStatus)
            //    query = query.Where(x => x.ProjectModuleStatus == "Close");
            //else
            //    query = query.Where(x => x.ProjectModuleStatus != "Close");

            if (ProjectModuleStatusIds != null && ProjectModuleStatusIds.Any())
                query = query.Where(m => ProjectModuleStatusIds.Contains(m.ProjectModuleStatusId));
            else if (!isShowCloseStatus)
            {
                // Only exclude Close when no specific status filter is selected
                query = query.Where(x => x.ProjectModuleStatus != "Close");
            }

            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                string Sort = SortBy + " " + (Descending ? "desc" : "asc");
                query = query.OrderBy(Sort);
            }
            else
                query = query.OrderBy(m => m.Name);

            var list = new PagedList<VW_ProjectModules>(query, page, pageSize);
            return list;
        }

        public async Task<IPagedList<VW_ProjectTask>> GetAllProjectTaskList(
            string SiteId,
            bool isShowCloseStatus,
            string taskName,
            string filterTask = "",
            string LoggedUserId = "",
            List<string> ProjectId = null,
            List<string> ProjectSwimlaneId = null,
            List<string> ProjectModuleId = null,
            List<string> StatusId = null,
            List<string> PriorityId = null,
            List<string> AssignedToId = null,
            List<string> tagIds = null,
            string SortBy = "",
            bool Descending = false,
            int page = 1,
            int pageSize = int.MaxValue)
        {
            var query = _vwProjectTaskRepository.TableNoTracking
                        .Include(x => x.Project.ProjectUserMappings.Where(m => m.AspNetUserId == LoggedUserId))
                        .Include(x => x.ProjectActivities.Where(a => a.Active))
                       .Where(m => m.SiteId == SiteId);

            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId);
            if (!IsAdmin)
                query = query.Where(p => p.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId && (m.FullAccess || m.ViewOnly || m.Notes)));

            if (!string.IsNullOrWhiteSpace(taskName))
            {
                taskName = taskName.Trim().ToLower();
                query = query.Where(x => x.TaskName.ToLower().Contains(taskName));
            }

            if (ProjectId != null && ProjectId.Any())
                query = query.Where(m => ProjectId.Contains(m.ProjectId));

            //if (ProjectSwimlaneId != null && ProjectSwimlaneId.Any())
            //    query = query.Where(m => ProjectSwimlaneId.Contains(m.ProjectSwimlaneId));

            if (ProjectModuleId != null && ProjectModuleId.Any())
                query = query.Where(m => ProjectModuleId.Contains(m.ProjectModuleId));

            //if (StatusId != null && StatusId.Any())
            //    query = query.Where(m => StatusId.Contains(m.StatusId));

            if (PriorityId != null && PriorityId.Any())
                query = query.Where(m => PriorityId.Contains(m.PriorityId));

            if (AssignedToId != null && AssignedToId.Any())
                query = query.Where(m => AssignedToId.Contains(m.AssignedToId));

            //if (isShowCloseStatus)
            //    query = query.Where(x => x.Status == "Close");
            //else if (StatusId != null && StatusId.Any())
            //    query = query.Where(m => StatusId.Contains(m.StatusId));
            //else
            //    query = query.Where(x => x.Status != "Close");

            // Status Logic
            if (StatusId != null && StatusId.Any())
            {
                query = query.Where(m => StatusId.Contains(m.StatusId));
            }
            else if (!isShowCloseStatus)
            {
                // Only exclude Close when no specific status filter is selected
                query = query.Where(x => x.Status != "Close");
            }

            if (!string.IsNullOrEmpty(filterTask))
            {
                query = query.Where(m =>
                    m.TaskName.Contains(filterTask.ToLower()) ||
                    m.ProjectName.Contains(filterTask.ToLower()) ||
                    m.AssignTo.Contains(filterTask.ToLower()) ||
                    m.ProjectTask_Tags.Contains(filterTask.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                string Sort = SortBy + " " + (Descending ? "desc" : "asc");
                query = query.OrderBy(Sort);
            }
            else
                query = query.OrderByDescending(m => m.CreatedOnUtc);

            var userTaskTagList = await GetUserTaskTagListByUserId(LoggedUserId);
            var list = new PagedList<VW_ProjectTask>(query, page, pageSize);
            var taskList = list.ToList();

            if (tagIds?.Any() == true)
            {
                taskList = taskList
                    .Where(task =>
                        userTaskTagList.Any(t =>
                            t.TaskId == task.Id && tagIds.Contains(t.TagId)
                        )
                    )
                    .ToList();
            }

            foreach (var task in taskList)
            {
                task.ProjectTask_Tags = string.Join(",",
                    userTaskTagList
                        .Where(x => x.TaskId == task.Id)
                        .OrderBy(x => x.TagName)
                        .Select(x => $"{x.TagId}:{x.TagName}:{x.Color}:{x.BgColor}")
                );
            }

            return new PagedList<VW_ProjectTask>(taskList, page, pageSize);
        }

        public async Task<IPagedList<VW_ProjectTaskActivities>> GetAllProjectActivitiesList(
            string SiteId,
            bool isShowCloseStatus,
            bool isShowCompletedStatus,
            string filterActivity,
            string projectId,
            string LoggedUserId,
            string projectSwimlaneId,
            string projectModuleId,
            string projectTaskId,
            string SortBy,
            bool Descending,
            int Page = 1,
            int PageSize = int.MaxValue
        )
        {
            var query = _vwProjectActivitiesRepository.TableNoTracking
                .Include(x => x.Project.ProjectUserMappings.Where(m => m.AspNetUserId == LoggedUserId))
                .Where(x => x.SiteId == SiteId);

            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId);
            if (!IsAdmin)
                query = query.Where(p => p.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId && (m.FullAccess || m.ViewOnly || m.Notes)));

            if (!string.IsNullOrEmpty(projectId))
                query = query.Where(m => m.ProjectId == projectId);

            if (!string.IsNullOrEmpty(projectModuleId))
                query = query.Where(m => m.ProjectModuleId == projectModuleId);

            if (!string.IsNullOrEmpty(projectTaskId))
                query = query.Where(m => m.ProjectTaskId == projectTaskId);

            //if (isShowCloseStatus)
            //    query = query.Where(x => x.Status == "Close");
            //else
            //    query = query.Where(x => x.Status != "Close");
            // Handle Close + Completed filters for all combinations
            if (!isShowCloseStatus && !isShowCompletedStatus)
            {
                // Hide both Close and Completed
                query = query.Where(x => x.Status != "Close" && x.Status != "Completed");
            }
            else if (!isShowCloseStatus)
            {
                // Hide only Close
                query = query.Where(x => x.Status != "Close");
            }
            else if (!isShowCompletedStatus)
            {
                // Hide only Completed
                query = query.Where(x => x.Status != "Completed");
            }

            if (!string.IsNullOrEmpty(filterActivity))
            {
                query = query.Where(m =>
                    m.ActivityName.Contains(filterActivity.ToLower()) ||
                    m.ActivityOwner.Contains(filterActivity.ToLower()) ||
                    m.Status.Contains(filterActivity.ToLower()));
            }

            //var targetMonth = ParseTargetMonth(targetMonthStr);
            //if (!string.IsNullOrWhiteSpace(targetMonthStr))
            //{
            //    query = query.Where(x => x.TargetMonth == targetMonth);
            //}

            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                string Sort = SortBy + " " + (Descending ? "desc" : "asc");
                query = query.OrderBy(Sort);
            }
            else
                query = query.OrderByDescending(m => m.CreatedOnUtc);

            var list = new PagedList<VW_ProjectTaskActivities>(query, Page, PageSize);
            return list;
        }

        public VW_EmployeeTaskActivitySummary GetEmployeeTaskActivitySummary(string EmployeeId)
        {
            return _vwEmployeeTaskActivitySummaryRepository.TableNoTracking.FirstOrDefault(m => m.AssignToId == EmployeeId);
        }

        private async Task<bool> IsCurrentUserAdmin(string CId)
        {
            var userdata = await _userManager.FindByIdAsync(CId);
            var user = await _userManager.FindByNameAsync(userdata.UserName);
            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains("Admin") || roles.Contains("Site Super Admin") || roles.Contains("System Super Admin");

            return isAdmin;
        }
        #endregion


        private async Task<List<VW_UserTaskTags>> GetUserTaskTagListByUserId(string userId)
        {
            var userTaskTagList = await _vwUserTaskTagsRepository.TableNoTracking
                                .Where(x => x.AspNetUserId == userId)
                                .Select(x => new VW_UserTaskTags
                                {
                                    TaskId = x.TaskId,
                                    TagId = x.TagId,
                                    TagName = x.TagName,
                                    Color = x.Color,
                                    BgColor = x.BgColor,
                                })
                                .ToListAsync();

            return userTaskTagList;
        }
        private async Task<List<string>> GetUserPinnedProjectIdListByUserId(string userId)
        {
            return await _vwUserProjectPinnedRepository.TableNoTracking
                .Where(x => x.AspNetUserId == userId)
                .Select(x => x.ProjectId)
                .ToListAsync();
        }
        private async Task<List<VW_UserProjectColor>> GetUserProjectColorListByUserId(string userId)
        {
            return await _vwUserProjectColorRepository.TableNoTracking
                .Where(x => x.AspNetUserId == userId)
                .Select(x => new VW_UserProjectColor
                {
                    ProjectId = x.ProjectId,
                    Color = x.Color
                })
                .ToListAsync();
        }
    }
}
