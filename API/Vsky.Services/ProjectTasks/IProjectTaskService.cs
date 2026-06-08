using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectTasks
{
    public interface IProjectTaskService
    {
        #region GetAllProjectTasks
        Task<IPagedList<ProjectTask>> GetAllProjectTasks(
            string SiteId, 
            string LoggedUserId,
            string SearchText,
            bool isTemplate,
            int projectTaskNumber,
            List<string> projectIds, 
            List<string> projectModuleIds,
            List<string> projectTaskIds,
            List<string> leadIds,
            List<string> statusIds,
            List<string> priorityIds,
            List<string> customerIds,
            List<string> companyContactIds,
            List<string> activityOwners, 
            string taskName, 
            List<string> TaskTagsIds, 
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending, 
            int page = 1,
            int pageSize = int.MaxValue
        );
        IPagedList<ProjectTask> GetAllProjectTasksForDashboard(string SiteId,
            string projectId,
            string sortBy, 
            bool descending,
            int page = 1, 
            int pageSize = int.MaxValue
        );
        IPagedList<ProjectTask> GetAllProjectTaskDetailsList(string SiteId, 
            List<string> projectIds,
            string sortByFilterId,
            string taskName, 
            List<string> activityOwners,
            List<string> customerIds, 
            List<string> companyContactIds,
            string sortBy, 
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue
        );
        IPagedList<ProjectTask> GetAllHighPrioritiesTaskForDashboard(string SiteId, 
            string activityOwnerId,
            string highPriorityId, 
            string sortBy, 
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue
        );
        IPagedList<ProjectTask> GetProjectTaskForCopy(string SiteId, string projectTaskId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue);

        Task<IPagedList<ProjectTask>> GetAllProjectTasksForNotes(
            string siteId,
            string userId,
            string searchText,
            bool isShowCloseStatus,
            int projectTaskNumber,
            List<string> projectIds,
            List<string> projectModuleIds,
            List<string> projectLeadsIds,
            List<string> statusIds,
            List<string> priorityIds,
            List<string> customerIds,
            List<string> companyContactIds,
            List<string> activityOwners,
            string taskName,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue
       );
        #endregion
        List<VWProjectTaskStatusSummary> GetTaskStatusSummaryByProjectIds(List<string> projectIds);

        #region GetAllProjectTaskListForDropdwon
        Task<List<ProjectTask>> GetAllProjectTaskListForDropdown(string SiteId, 
            string projectId = null, 
            string projectModuleId=null, 
            string employeeId = null
        );
        Task<List<CommonDropDown>> GetAllProjectMultiTaskListForDropdown(string siteId, bool isTemplate, string projectId = null, string projectModuleId = null);
        Task<List<CommonDropDown>> GetAllProjectTaskWithProjectListForDropdown(string siteId, string LoggedUserId);
        #endregion

        Task<int> GetLastProjectTaskNumber();
        Task<decimal> GetLastSortOrderOfProjectTasks(string listId);

        #region GetById
        Task<ProjectTask> GetById(string id);
        #endregion

        #region GetProjectTaskDetailsById
        Task<ProjectTask> GetProjectTaskDetailsById(string id);
        #endregion

        #region GetProjectDetailsByIds
        Task<List<ProjectTask>> GetProjectTaskDetailsByIds(string SiteId, string[] ids);
        #endregion

        #region GetTaskLevelTimesheetDetailsById
        Task<ProjectTask> GetTimesheetByTaskId(string id);
        #endregion

        #region GetProjectIdAndProjectModuleIdByTaskId
        Task<(string ProjectId, string ProjectModuleId)> GetProjectIdAndProjectModuleIdByTaskId(string id);
        #endregion

        #region GetAllTasksByProjectId
        IPagedList<ProjectTask> GetAllTasksByProjectId(string SiteId, string projectId, string searchTaskText, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetProjectByName
        Task<ProjectTask> GetProjectTaskByName(string name, string projectId, string projectModuleId, string id = null);
        #endregion
        #region IsProjectTaskSortOrderExists
        Task<bool> IsProjectTaskSortOrderExists(string siteId, string ProjectId, decimal sortOrder, string moduleId, string id = null);
        #endregion

        #region GetTaskByProjectModuleId
        Task<List<ProjectTask>> GetTaskByProjectModuleId(string projectModuleId, string pageName="", bool isShowCloseStatus = false);
        #endregion

        #region GetAllTaskByProjectModuleIdForMoveModuleAsProject
        Task<List<ProjectTask>> GetAllTaskByProjectModuleIdForMoveModuleAsProject(string projectModuleId);
        #endregion   

        #region InsertProjectTask
        void InsertProjectTask(ProjectTask entity);
        #endregion

        #region UpdateProjectTask
        void UpdateProjectTask(ProjectTask entity);
        #endregion

        #region DeleteProjectTask
        void DeleteProjectTask(ProjectTask entity);
        #endregion     

        #region InsertProjectTaskList
        void InsertProjectTaskList(IList<ProjectTask> entities);
        #endregion

        #region UpdateProjectTaskList
        void UpdateProjectTaskList(IList<ProjectTask> entities);
        #endregion

        #region DeleteProjectTask
        void DeleteProjectTaskList(List<ProjectTask> entities);
        #endregion
    }
}
