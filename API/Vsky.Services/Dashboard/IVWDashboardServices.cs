using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Dashboard
{
    public interface IVWDashboardServices
    {
         Task<IPagedList<VW_Customer>> GetAllCustomerList(
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
            int pageSize = int.MaxValue);

        Task<IPagedList<VW_Project>> GetAllCustomerProjectList(
            string SiteId,
            string filterProject = "",
            string logginuser = "",
            string searchText = "",
            List<string> CustomerIds = null,
            List<string> ProjectCategoryIds = null,
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
            int pageSize = int.MaxValue);

        IPagedList<VW_ProjectSwimLane> GetAllProjectSwimlaneList(string SiteId, string filterProjectSwimlane = "", List<string> ProjectId = null, string SortBy = "",
            bool Descending = false, int page = 1, int pageSize = int.MaxValue);

        VW_EmployeeTaskActivitySummary GetEmployeeTaskActivitySummary(string EmployeeId);

        Task<IPagedList<VW_ProjectModules>> GetAllProjectModulesList(
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
            int pageSize = int.MaxValue);

        Task<IPagedList<VW_ProjectTask>> GetAllProjectTaskList(
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
            int pageSize = int.MaxValue);

        Task<IPagedList<VW_ProjectTaskActivities>> GetAllProjectActivitiesList(
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
        );
    }
}
