using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectReleaseTrackings
{
    public interface IProjectReleaseTrackingStatusLogService
    {
        #region GetLatestStatusByProjectReleaseTrackingId
        Task<ProjectReleaseTrackingStatusLog> GetLatestStatusByProjectReleaseTrackingId(string ProjectReleaseTrackingId);
        #endregion

        #region InsertProjectReleaseTrackingStatusLog
        void InsertProjectReleaseTrackingStatusLog(ProjectReleaseTrackingStatusLog entity);
        #endregion
    }
}
