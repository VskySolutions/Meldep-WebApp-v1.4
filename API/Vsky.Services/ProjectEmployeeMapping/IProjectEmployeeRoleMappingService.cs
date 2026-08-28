using System.Collections.Generic;
using Vsky.Models;

namespace Vsky.Services.ProjectEmployeeMappings
{
    public interface IProjectEmployeeRoleMappingService
    {
        #region GetRoleMappingByProjectEmployeeMappingId
        List<ProjectEmployeeRoleMapping> GetRoleMappingByProjectEmployeeMappingId(string projectEmployeeMappingId);
        #endregion

        #region InsertProjectEmployeeRole
        void InsertProjectEmployeeRole(ProjectEmployeeRoleMapping entity);
        #endregion

        #region UpdateProjectEmployeeRole
        void UpdateProjectEmployeeRole(ProjectEmployeeRoleMapping entity);
        #endregion

        #region DeleteProjectEmployeeRole
        void DeleteProjectEmployeeRole(ProjectEmployeeRoleMapping entity);
        #endregion
    }
}
