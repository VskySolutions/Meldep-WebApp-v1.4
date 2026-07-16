using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ProjectReleaseTrackings
{
    public interface IProjectReleaseTrackingReqPlanTaskIssueMappingService
    {
        #region GetProjectReleaseTrackingReqPlanTaskIssueMappingById
        Task<ProjectReleaseTrackingReqPlanTaskIssueMapping> GetProjectReleaseTrackingReqPlanTaskIssueMappingById(string id);
        #endregion

        #region GetAllReqPlanTaskIssuesByProjectId
        Task<List<ProjectReqPlanTaskIssueItemDto>> GetAllReqPlanTaskIssuesByProjectId(string projectId, string SiteId);
        #endregion

        #region GetAllProjectReleaseTrackingReqPlanTaskIssueMappingByProjectReleaseTrackingId
        Task<List<ProjectReleaseTrackingReqPlanTaskIssueMapping>> GetAllProjectReleaseTrackingReqPlanTaskIssueMappingByProjectReleaseTrackingId(string ProjectReleaseTrackingId);
        #endregion

        #region GetReleaseWiseTestCaseHistory
        Task<List<ReleaseWiseTestCaseHistoryDto>> GetReleaseWiseTestCaseHistory(string testCaseId);
        Task<List<ReleaseWiseTestCaseHistoryDto>> GetReleaseWiseTestCaseHistoryByTestCaseIds(List<string> testCaseIds, string versionNumber);
        #endregion

        #region InsertProjectReleaseTrackingReqPlanTaskIssueMappingList
        void InsertProjectReleaseTrackingReqPlanTaskIssueMapping(ProjectReleaseTrackingReqPlanTaskIssueMapping entity);
        #endregion

        #region DeleteProjectReleaseTrackingReqPlanTaskIssueMapping
        void DeleteProjectReleaseTrackingReqPlanTaskIssueMapping(Models.ProjectReleaseTrackingReqPlanTaskIssueMapping entity);
        #endregion
    }
}
