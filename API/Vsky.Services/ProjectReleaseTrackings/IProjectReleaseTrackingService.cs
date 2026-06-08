using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectReleaseTrackings
{
    public interface IProjectReleaseTrackingService
    {
        #region GetAllProjectReleaseTrackingList
        Task<IPagedList<ProjectReleaseTracking>> GetAllProjectReleaseTrackingList(
            string siteId,
            string searchText,
            string LoggedUserId,
            List<string> projectIds,
            List<string> infraInstanceIds,
            List<string> deploymentOwnerIds,
            List<string> approverIds,
            List<string> testerIds,
            List<string> releaseTypeIds,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetProjectReleaseTrackingById
        Task<ProjectReleaseTracking> GetProjectReleaseTrackingById(string id);
        #endregion

        #region GenerateVersionNumber
        Task<string> GenerateVersionNumber(string projectId, string releaseType);
        #endregion

        #region GetProjectReleaseTrackingInDetailsById
        Task<Models.ProjectReleaseTracking> GetProjectReleaseTrackingInDetailsById(string id);
        #endregion

        #region InsertProjectReleaseTracking
        void InsertProjectReleaseTracking(Models.ProjectReleaseTracking entity);
        #endregion

        #region UpdateProjectReleaseTracking
        void UpdateProjectReleaseTracking(Models.ProjectReleaseTracking entity);
        #endregion

        #region DeleteProjectReleaseTracking
        void DeleteProjectReleaseTracking(Models.ProjectReleaseTracking entity);
        #endregion
    }
}
