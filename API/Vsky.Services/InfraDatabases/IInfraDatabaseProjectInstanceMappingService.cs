using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.InfraDatabases
{
    public interface IInfraDatabaseProjectInstanceMappingService
    {
        #region
        Task<InfraDatabaseProjectInstanceMapping> GetInfraDatabaseProjectInstanceMappingById(string id);
        #endregion

        #region InsertInfraDatabaseProjectInstanceMapping
        void InsertInfraDatabaseProjectInstanceMapping(Models.InfraDatabaseProjectInstanceMapping entity);
        #endregion

        #region DeleteInfraDatabaseProjectInstanceMapping
        void DeleteInfraDatabaseProjectInstanceMapping(InfraDatabaseProjectInstanceMapping entity);
        #endregion
    }
}
