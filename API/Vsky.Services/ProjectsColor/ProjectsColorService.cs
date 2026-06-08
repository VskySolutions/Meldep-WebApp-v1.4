using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectsColor
{
    public class ProjectsColorService : IProjectsColorService
    {
        #region Define services
        private readonly IRepository<ProjectColor> _projectColorRepository;

        public ProjectsColorService(IRepository<ProjectColor> projectColorRepository)
        {
            _projectColorRepository = projectColorRepository;
        }
        #endregion

        #region GetProjectColorById
        // Title: GetProjectColorById
        // Description: This method retrieves a ProjectColor record for a specific project and user.
        // It queries the ProjectColor table and returns the first matching record if the user has color the project, or null if no record exists
        public async Task<ProjectColor> GetProjectsColorByUser(string projectId, string LoggedUserId)
        {
            var query = _projectColorRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectId == projectId && x.AspNetUserId == LoggedUserId);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertProjectColor
        public void InsertProjectColor(ProjectColor entity)
        {
            _projectColorRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectColor
        public void UpdateProjectColor(ProjectColor entity)
        {
            _projectColorRepository.Update(entity);
        }
        #endregion
    }
}
