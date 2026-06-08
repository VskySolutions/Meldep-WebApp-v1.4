using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Projects
{
    public interface IProjectService
    {
        #region GetAllProjects
        Task<IPagedList<Project>> GetAllProjects(
            string siteId,
            bool isTemplate,
            string userId,
            string searchText,
            List<string> projectIds,
            List<string> projectCategoryIds,
            List<string> statusIds,
            List<string> teamMemberIds,
            List<string> coordinatorIds,
            List<string> leadIds,
            List<string> priorityIds,
            List<string> typeIds,
            int status,
            List<string> customerIds,
            List<string> companyContactIds,
            string singleCustomerId,
            List<string> projectTagIds,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false);

        Task<IPagedList<Project>> GetAllProjectsForNotes(
          string siteId,
          string userId,
          string searchText,
          List<string> projectIds,
          List<string> projectCategoryIds,
          List<string> statusIds,
          List<string> teamMemberIds,
          List<string> coordinatorIds,
          List<string> leadIds,
          List<string> priorityIds,
          List<string> typeIds,
          int status,
          List<string> customerIds,
          List<string> companyContactIds,
          string singleCustomerId,
          string sortBy,
          bool descending,
          int page = 1,
          int pageSize = int.MaxValue,
          bool lookup = false);
        #endregion

        #region GetProjectById
        Task<Project> GetById(string id);
        #endregion

        #region GetAllProjectListForDropdown
        Task<List<Project>> GetAllProjectListForDropdown(string SiteId, string LoggedUserId, string[] statuses = null);
        Task<List<CommonDropDown>> GetProjectsListForDropdown(string SiteId, string LoggedUserId, bool isTemplate, string ActiveStatus, bool isAllProject);
        #endregion

        #region GetProjectDetailsById
        Task<Project> GetProjectDetailsById(string id);
        #endregion

        #region GetProjectSummeryInDetails
        Task<Project> GetProjectSummeryInDetails(string projectId, DateTime currentDate, string weeklyTypeId, string monthlyTypeId);
        Task<Project> GetProjectMyWorkSummeryInDetails(string projectId, DateTime currentDate);
        Task<Project> GetProjectSdlcSummeryInDetails(string projectId, DateTime currentDate);
        #endregion

        #region GetProjectByName
        Task<Project> GetProjectByName(string SiteId, string name, string customerId, bool isTemplate = false, string id = null);
        #endregion

        #region GetProjectName
        Task<Project> GetProjectName(string SiteId, string name);
        #endregion

        #region GetProjectByTemplate
        Task<Project> GetProjectByTemplate(string SiteId, string name, string customerId, bool template);
        #endregion

        #region GetProjectDetailsByIds
        Task<List<Project>> GetProjectDetailsByIds(string[] ids, string siteId);
        #endregion

        #region GetProjectsAndCharterListForDashboard
        IPagedList<Project> GetProjectsAndCharterListForDashboard(string SiteId, List<string> projectIds, List<string> projectStatusIds, List<string> projectTeamMemberIds, List<string> projectCoordinatorIds, List<string> projectPriorityIds, List<string> projectTypeIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetAllProjectDataByProjectId
        Task<Project> GetAllProjectDataByProjectId(string SiteId, string projectId);
        #endregion

        #region WorkBoard Ver 3
        Task<Project> GettWorkBoardByProjecId(string SiteId, string ProjectId, DateTime GetDateTime);
        #endregion

        #region InsertProject
        void InsertProject(Project entity);
        #endregion

        #region UpdateProject
        void UpdateProject(Project entity);
        #endregion

        #region DeleteProject
        void DeleteProject(Project entity);
        #endregion

        #region InsertProjectList
        void InsertProjectList(IList<Project> entities);
        #endregion

        #region UpdateProjectList
        void UpdateProjectList(IList<Project> entities);
        #endregion

        #region DeleteProjectList
        void DeleteProjectList(List<Project> entities);
        #endregion

        Task<object> GetProjectsAsync(ProjectListRequest request);
    }
}