using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectModules
{
    public interface IProjectModuleService
    {
        #region GetAllProjectModules
        IPagedList<ProjectModule> GetAllProjectModules(string SiteId, string SearchText, List<string> projectIds, List<string> projectModuleTypeIds, List<string> projectModuleStatusIds, string projectId, List<string> customerIds, List<string> companyContactIds, bool isShowCloseStatus, string pageName, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        IPagedList<ProjectModule> GetAllProjectModulesForDashboard(string SiteId, string projectId, string pageName, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);

        #endregion

        #region GetProjectModuleById
        Task<ProjectModule> GetProjectModuleById(string id);
        #endregion

        Task<int> GetLastProjectModuleNumber();
        Task<int> GetLastSortOrderOfProjectModules(string projectId);
        Task<List<ProjectModule>> GetAllModulesByProjectId(string projectId);
        Task<ProjectModule> GetAllModulesAndTasksById(string projectModuleId);

        #region GetAllProjectModuleListForDropdown
        Task<List<CommonDropDown>> GetAllProjectModuleListForDropdown(string SiteId, bool isTemplate, bool showTaskCount, string projectId = null);
        #endregion

        #region GetProjectModuleDetailsById
        Task<ProjectModule> GetProjectModuleDetailsById(string id);
        #endregion

        #region GetProjectModuleByName
        Task<ProjectModule> GetProjectModuleByName(string name, string ProjectId = null, string id = null);
        #endregion

        #region IsProjectModuleSortOrderExists
        Task<bool> IsProjectModuleSortOrderExists(string siteId, string ProjectId, int sortOrder, string moduleId);
        #endregion

        #region InsertProjectModule
        void InsertProjectModule(ProjectModule entity);
        #endregion

        #region InsertProjectModuleList
        void InsertProjectModuleList(IList<ProjectModule> entities);
        #endregion        

        #region UpdateProjectModule
        void UpdateProjectModule(ProjectModule entity);
        #endregion

        #region UpdateProjectModuleList
        void UpdateProjectModuleList(IList<ProjectModule> entities);
        #endregion        

        #region DeleteProjectModule
        void DeleteProjectModule(ProjectModule entity);
        #endregion
    }
}