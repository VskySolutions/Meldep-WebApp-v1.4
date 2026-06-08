using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.InfraProjectInstances
{
    public interface IInfraProjectInstanceRoleService
    {
        #region
        Task<InfraProjectInstanceRole> GetInfraProjectInstanceRoleById(string id);
        Task<InfraProjectInstanceRole> GetInfraProjectInstanceRoleByRoleName(string SiteId, string projectInstanceId, string roleName, string id = null);
        #endregion

        #region GetInfraProjectInstanceRoleInDetailByInstanceId
        Task<List<InfraProjectInstanceRole>> GetInfraProjectInstanceRoleInDetailByInstanceId(string instanceId);
        #endregion

        #region InsertInfraProjectInstanceRole
        void InsertInfraProjectInstanceRole(InfraProjectInstanceRole entity);
        #endregion

        #region UpdateInfraProjectInstanceRole
        void UpdateInfraProjectInstanceRole(Models.InfraProjectInstanceRole entity);
        #endregion

        #region DeleteInfraProjectInstanceRole
        void DeleteInfraProjectInstanceRole(InfraProjectInstanceRole entity);
        #endregion
    }
}
