using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectEmployeeMappings
{
    public class ProjectEmployeeRoleMappingService : IProjectEmployeeRoleMappingService
    {
        #region Define Service
        /// <summary>
        /// Define Service
        /// </summary>

        private readonly IRepository<ProjectEmployeeRoleMapping> _projectEmployeeRoleMappingRepository;

        #endregion

        #region Service Initializations
        /// <summary>
        /// Service Initializations
        /// </summary>
        /// <param name="projectEmployeeMappingRepository"></param>
        public ProjectEmployeeRoleMappingService(IRepository<ProjectEmployeeRoleMapping> projectEmployeeRoleMappingRepository)
        {
            _projectEmployeeRoleMappingRepository = projectEmployeeRoleMappingRepository;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Private Methods
        /// </summary>
        /// <param name="orderBy"></param>
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetRoleMappingByProjectEmployeeMappingId
        // Title : GetRoleMappingByProjectEmployeeMappingId
        // Description: Retrieves a list of ProjectEmployeeRoleMapping entities associated with a projectEmployeeMappingId.
        public List<ProjectEmployeeRoleMapping> GetRoleMappingByProjectEmployeeMappingId(string projectEmployeeMappingId)
        {
            var query = _projectEmployeeRoleMappingRepository.TableNoTracking.Where(x => x.ProjectEmployeeMappingId == projectEmployeeMappingId);
            var list = query.ToList();
            return list;
        }
        #endregion

        #region InsertProjectEmployeeRole
        // Title : InsertProjectEmployeeRole
        // Description: Inserts a new ProjectEmployeeRoleMapping entity into the repository.
        public void InsertProjectEmployeeRole(ProjectEmployeeRoleMapping entity)
        {
            _projectEmployeeRoleMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectEmployeeRole
        // Title : UpdateProjectEmployeeRole
        // Description: Updates an existing ProjectEmployeeRoleMapping entity in the repository.
        public void UpdateProjectEmployeeRole(ProjectEmployeeRoleMapping entity)
        {
            _projectEmployeeRoleMappingRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectEmployeeRole
        // Title : DeleteProjectEmployeeRole
        // Description: Deletes a ProjectEmployeeRoleMapping entity from the repository.
        public void DeleteProjectEmployeeRole(ProjectEmployeeRoleMapping entity)
        {
            entity.Deleted = true;
            _projectEmployeeRoleMappingRepository.Update(entity);
        }
        #endregion
    }
}
