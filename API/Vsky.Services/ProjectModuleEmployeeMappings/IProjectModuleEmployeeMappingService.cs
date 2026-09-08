using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ProjectModuleEmployeeMappings
{
    public interface IProjectModuleEmployeeMappingService
    {
        #region GetProjectModuleEmployeeById
        Task<ProjectModuleEmployeeMapping> GetProjectModuleEmployeeById(string id);
        #endregion

        #region InsertProjectModuleEmployees
        void InsertProjectModuleEmployees(ProjectModuleEmployeeMapping entity);
        #endregion

        #region UpdateProjectModuleEmployees
        void UpdateProjectModuleEmployees(ProjectModuleEmployeeMapping entity);
        #endregion

        #region DeleteProjectModuleEmployees
        void DeleteProjectModuleEmployees(ProjectModuleEmployeeMapping entity);
        #endregion
    }
}
