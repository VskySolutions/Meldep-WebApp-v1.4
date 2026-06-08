using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectTasks
{
    public class ProjectTaskRelatedMappingsService : IProjectTaskRelatedMappingsService
    {
        #region Define services
        private readonly IRepository<ProjectTaskRelatedMapping> _projectTaskRelatedMappingRepository;

        public ProjectTaskRelatedMappingsService(IRepository<ProjectTaskRelatedMapping> projectTaskRelatedMappingRepository)
        {
            _projectTaskRelatedMappingRepository = projectTaskRelatedMappingRepository;
        }
        #endregion

        #region InsertProjectTaskRelatedMapping
        public void InsertProjectTaskRelatedMapping(ProjectTaskRelatedMapping entity)
        {
            _projectTaskRelatedMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectTaskRelatedMapping
        public void UpdateProjectTaskRelatedMapping(ProjectTaskRelatedMapping entity)
        {
            _projectTaskRelatedMappingRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectTaskRelatedMapping
        public void DeleteProjectTaskRelatedMapping(ProjectTaskRelatedMapping entity)
        {
            entity.Deleted = true;
            _projectTaskRelatedMappingRepository.Update(entity);
        }
        #endregion
    }
}
