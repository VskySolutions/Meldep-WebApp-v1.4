using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.InfraFTPs
{
    public interface IInfraFTPsProjectInstanceMappingService
    {
        #region
        Task<InfraFTPsProjectInstanceMapping> GetInfraFTPsProjectInstanceMappingById(string id);
        #endregion

        #region InsertInfraFTPsProjectInstanceMapping
        void InsertInfraFTPsProjectInstanceMapping(Models.InfraFTPsProjectInstanceMapping entity);
        #endregion

        #region DeleteInfraFTPsProjectInstanceMapping
        void DeleteInfraFTPsProjectInstanceMapping(InfraFTPsProjectInstanceMapping entity);
        #endregion
    }
}
