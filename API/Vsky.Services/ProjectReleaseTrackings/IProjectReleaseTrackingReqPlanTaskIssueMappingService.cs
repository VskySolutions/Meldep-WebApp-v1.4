using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
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

        #region InsertProjectReleaseTrackingReqPlanTaskIssueMappingList
        void InsertProjectReleaseTrackingReqPlanTaskIssueMapping(ProjectReleaseTrackingReqPlanTaskIssueMapping entity);
        #endregion

        #region DeleteProjectReleaseTrackingReqPlanTaskIssueMapping
        void DeleteProjectReleaseTrackingReqPlanTaskIssueMapping(Models.ProjectReleaseTrackingReqPlanTaskIssueMapping entity);
        #endregion
    }
}
