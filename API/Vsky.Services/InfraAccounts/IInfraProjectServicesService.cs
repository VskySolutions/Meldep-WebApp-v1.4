using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.InfraAccounts
{
    public interface IInfraProjectServicesService
    {
        #region GetInfraAccountProjectServicesById
        Task<InfraProjectServices> GetInfraAccountProjectServicesById(string id);
        #endregion

        #region HasActiveInfraProjects
        Task<bool> HasActiveInfraProjects(string infraServiceId);
        #endregion

        #region InsertInfraProjectServices
        void InsertInfraProjectServices(Models.InfraProjectServices entity);
        #endregion

        #region DeleteInfraProjectServices
        void DeleteInfraProjectServices(InfraProjectServices entity);
        #endregion
    }
}
