using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;


namespace Vsky.Services.ProjectUserMappings
{
    public interface IProjectUserMappingService
    {
        #region GetAllProjectsForUserPermission
        IPagedList<Project> GetAllProjectsForUserPermission(string SiteId, bool isTemplate, string SearchText, List<string> projectIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetProjectUserByProjectId
        Task<List<ProjectUserMapping>> GetProjectUserByProjectId(string SiteId, string projectId);
        #endregion

        #region GetProjectUserById
        Task<ProjectUserMapping> GetProjectUserById(string id);
        #endregion

        #region GetRecordByUserIdandProjectId
        Task<ProjectUserMapping> GetRecordByUserIdandProjectId(string SiteId, string userId, string projectId);
        #endregion

        #region InsertProjectUser
        void InsertProjectUser(ProjectUserMapping entity);
        #endregion

        #region UpdateProjectUser
        void UpdateProjectUser(ProjectUserMapping entity);
        #endregion

        #region DeleteProjectUser
        void DeleteProjectUser(ProjectUserMapping entity);
        #endregion
    }
}
