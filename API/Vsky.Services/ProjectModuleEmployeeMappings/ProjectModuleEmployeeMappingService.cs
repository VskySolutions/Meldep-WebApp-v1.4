using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectModuleEmployeeMappings
{
    public class ProjectModuleEmployeeMappingService : IProjectModuleEmployeeMappingService
    {
        #region Define Service
        /// <summary>
        /// Define Service
        /// </summary>
        private readonly IRepository<ProjectModuleEmployeeMapping> _ProjectModuleEmployeeMappingRepository;

        #endregion

        #region Service Initializations
        /// <summary>
        /// Service Initializations
        /// </summary>
        /// <param name="ProjectModuleEmployeeMappingRepository"></param>
        public ProjectModuleEmployeeMappingService(
            IRepository<ProjectModuleEmployeeMapping> ProjectModuleEmployeeMappingRepository
        )
        {
            _ProjectModuleEmployeeMappingRepository = ProjectModuleEmployeeMappingRepository;
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

        #region GetProjectModuleEmployeeById
        // Title : GetProjectModuleEmployeeById
        // Description: This method asynchronously retrieves a `ProjectModuleEmployeeMapping` object from the repository based on the provided unique identifier (ID).
        public async Task<ProjectModuleEmployeeMapping> GetProjectModuleEmployeeById(string id)
        {
            var query = _ProjectModuleEmployeeMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertProjectModuleEmployees
        // Title : InsertProjectModuleEmployees
        // Description: Inserts a new ProjectModuleEmployeeMapping entity into the repository.
        public void InsertProjectModuleEmployees(ProjectModuleEmployeeMapping entity)
        {
            _ProjectModuleEmployeeMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectModuleEmployees
        // Title : UpdateProjectModuleEmployees
        // Description: Updates an existing ProjectModuleEmployeeMapping entity in the repository.
        public void UpdateProjectModuleEmployees(ProjectModuleEmployeeMapping entity)
        {
            _ProjectModuleEmployeeMappingRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectModuleEmployees
        // Title : DeleteProjectModuleEmployees
        // Description: Deletes a ProjectModuleEmployeeMapping entity from the repository.
        public void DeleteProjectModuleEmployees(ProjectModuleEmployeeMapping entity)
        {
            entity.Deleted = true;
            _ProjectModuleEmployeeMappingRepository.Update(entity);
        }
        #endregion
    }
}

