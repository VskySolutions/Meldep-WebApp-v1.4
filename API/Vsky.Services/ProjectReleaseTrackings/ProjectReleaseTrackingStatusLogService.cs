using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectReleaseTrackings
{
    public class ProjectReleaseTrackingStatusLogService : IProjectReleaseTrackingStatusLogService
    {
        #region Define Services
        private readonly IRepository<ProjectReleaseTrackingStatusLog> _projectReleaseTrackingStatusLogRepository;
        #endregion

        #region Services Initializations

        public ProjectReleaseTrackingStatusLogService(IRepository<ProjectReleaseTrackingStatusLog> projectReleaseTrackingStatusLogRepository)
        {
            _projectReleaseTrackingStatusLogRepository = projectReleaseTrackingStatusLogRepository;
        }

        #endregion

        #region Private Methods
        // Title: GetOrderBy
        // Description: This method returns the input string as it is, which can be used as the `ORDER BY` clause in a SQL query.
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetLatestStatusByProjectReleaseTrackingId
        // Title: GetLatestStatusByProjectReleaseTrackingId
        // Description: This method retrieves a get latest status by ProjectReleaseTrackingId from the database by ProjectReleaseTrackingId. 
        public async Task<ProjectReleaseTrackingStatusLog> GetLatestStatusByProjectReleaseTrackingId(string ProjectReleaseTrackingId)
        {
            return await _projectReleaseTrackingStatusLogRepository.TableNoTracking
            .Where(x => x.ReleaseTrackingId == ProjectReleaseTrackingId)
            .Include(x => x.Status)
            .OrderByDescending(x => x.CreatedOnUtc)
            .FirstOrDefaultAsync();
        }
        #endregion

        #region InsertProjectReleaseTrackingStatusLog
        // Title: InsertProjectReleaseTrackingStatusLog
        // Description: This method inserts a new InsertProjectReleaseTrackingStatusLog entity into the repository. It takes a InsertProjectReleaseTrackingStatusLog object as input and uses the _projectReleaseTrackingStatusLogRepository to handle the insertion operation.
        public void InsertProjectReleaseTrackingStatusLog(ProjectReleaseTrackingStatusLog entity)
        {
            _projectReleaseTrackingStatusLogRepository.Insert(entity);
        }
        #endregion
    }
}
