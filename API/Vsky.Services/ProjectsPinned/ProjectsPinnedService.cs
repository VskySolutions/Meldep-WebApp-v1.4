using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectsPinned
{
    public class ProjectsPinnedService : IProjectsPinnedService
    {
        #region Define services
        private readonly IRepository<ProjectPinned> _projectPinnedRepository;

        public ProjectsPinnedService(IRepository<ProjectPinned> projectPinnedRepository)
        {
            _projectPinnedRepository = projectPinnedRepository;
        }
        #endregion

        #region
        // Title: GetProjectPinnedByUser
        // Description: This method retrieves a ProjectPinned record for a specific project and user.
        // It queries the ProjectPinned table and returns the first matching record if the user has pinned the project, or null if no record exists.
        public async Task<ProjectPinned> GetProjectPinnedByUser(string projectId, string LoggedUserId)
        {
            var query = _projectPinnedRepository.TableNoTracking.Where(x => x.ProjectId == projectId && x.AspNetUserId == LoggedUserId);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertProjectPin
        public void InsertProjectPin(ProjectPinned entity)
        {
            _projectPinnedRepository.Insert(entity);
        }
        #endregion
    }
}
