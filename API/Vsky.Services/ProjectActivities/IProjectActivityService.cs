using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectActivities
{
    public interface IProjectActivityService
    {
        #region GetAllProjectActivities
        Task<IPagedList<ProjectActivity>> GetAllProjectActivities(string SiteId,
            string userId, 
            string createdBy,
            string SearchText,
            string activeStatus,
            List<string> projectIds, 
            List<string> projectModuleIds,
            List<string> assignedToId, 
            List<string> activityNameIds,
            List<string> activityStatusIds, 
            List<string> statusIds, 
            List<string> customerIds,
            List<string> companyContactIds,
            DateTime? SprintWeekEndDate,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        Task<IPagedList<object>> GetAllProjectActivitiesForExpandCollapse(string SiteId,
           string userId,
           string createdBy,
           string SearchText,
           string activeStatus,
           List<string> projectIds,
           List<string> projectModuleIds,
           List<string> assignedToId,
           List<string> activityNameIds,
           List<string> activityStatusIds,
           List<string> statusIds,
           List<string> customerIds,
           List<string> companyContactIds,
           DateTime? SprintWeekEndDate,
           string sortBy,
           Dictionary<string, string> sorts,
           bool descending,
           int page = 1,
           int pageSize = int.MaxValue,
           bool lookup = false
       );
        IPagedList<ProjectActivity> GetAllProjectActivitiesForDashboard(string SiteId,
            string projectId,
            string sortBy, 
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetProjectActivityById
        Task<ProjectActivity> GetById(string id);
        #endregion

        #region GetAllProjectActivityListForDropdown
        Task<List<ProjectActivity>> GetAllProjectActivityListForDropdown(string SiteId);
        #endregion

        #region GetProjectActivityDetailsById GetProjectActivityForTimerById
        Task<ProjectActivity> GetProjectActivityDetailsById(string id);
        #endregion

        #region GetProjectActivityByTaskId
        Task<List<ProjectActivity>> GetProjectActivityByTaskId(string SiteId, string taskId, DateTime? TargetMonth = null);
        #endregion

        #region GetProjectActivityById
        Task<ProjectActivity> GetProjectActivityById(string id);
        #endregion

        #region GetProjectActivityByName
        Task<ProjectActivity> GetProjectActivityByName(string SiteId, string name, string taskId, string id = null);
        #endregion

        #region GetProjectActivityByDetails
        Task<ProjectActivity> GetProjectActivityByDetails(string name, string taskId, string AssignedToId, DateTime? TargetMonth, string id = null);
        #endregion

        #region InsertProjectActivity
        void InsertProjectActivity(ProjectActivity entity);
        #endregion

        #region UpdateProjectActivity
        void UpdateProjectActivity(ProjectActivity entity);
        #endregion

        #region DeleteProjectActivity
        void DeleteProjectActivity(ProjectActivity entity);
        #endregion

        #region GetProjectActivitiesByTaskId
        Task<List<ProjectActivity>> GetProjectActivitiesByTaskId(string taskId, string pageName = "", bool isShowCloseStatus = false);
        #endregion

        #region GetProjectActivitiesByModuleId
        Task<List<ProjectActivity>> GetProjectActivitiesByModuleId(string moduleId);
        #endregion

        #region GetAllProjectActivitiesByModuleIdForMoveModuleAsProject
        Task<List<ProjectActivity>> GetAllProjectActivitiesByModuleIdForMoveModuleAsProject(string moduleId);
        #endregion

        #region GetProjectTaskActivityListForDropdown
        Task<List<ProjectActivity>> GetProjectTaskActivityListForDropdown(string SiteId, 
            string projectId = null,
            string projectModuleId = null,
            string taskId = null, 
            string employeeId = null
        );
        #endregion

        #region GetProjectTasksActivitiesDetailsByIds
        Task<List<ProjectActivity>> GetProjectTasksActivitiesDetailsByIds(string[] ids);
        #endregion

        #region InsertProjectActivityList
        void InsertProjectActivityList(IList<ProjectActivity> entities);
        #endregion

        #region UpdateProjectActivityList
        void UpdateProjectActivityList(IList<ProjectActivity> entities);
        #endregion

        #region DeleteProjectActivityList
        void DeleteProjectActivityList(List<ProjectActivity> activityEntities);
        #endregion

        #region GetProjectActivityForTimerById 
        Task<ProjectActivity> GetProjectActivityForTimerById(string id);
        #endregion

        #region GetProjectActivityDescriptionById
        Task<string> GetProjectActivityDescriptionById(string id);
        #endregion
    }
}