using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.InfraProjectInstances
{
    public interface IInfraProjectInstanceRoleUsersService
    {
        #region
        Task<InfraProjectInstanceRoleUsers> GetInfraProjectInstanceRoleUsersById(string id);
        Task<InfraProjectInstanceRoleUsers> GetInfraProjectInstanceRoleUsersByUsername(string projectInstanceId, string username, string id = null);
        #endregion

        #region InsertInfraProjectInstanceRoleUsers
        void InsertInfraProjectInstanceRoleUsers(Models.InfraProjectInstanceRoleUsers entity);
        #endregion

        #region UpdateInfraProjectInstanceRoleUsers
        void UpdateInfraProjectInstanceRoleUsers(Models.InfraProjectInstanceRoleUsers entity);
        #endregion

        #region DeleteInfraProjectInstanceRoleUsers
        void DeleteInfraProjectInstanceRoleUsers(InfraProjectInstanceRoleUsers entity);
        #endregion
    }
}

