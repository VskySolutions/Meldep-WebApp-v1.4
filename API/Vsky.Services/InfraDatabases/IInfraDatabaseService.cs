using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.InfraDatabases
{
    public interface IInfraDatabaseService
    {
        #region GetAllInfraDatabaseForList
        public IPagedList<InfraDatabase> GetAllInfraDatabaseForList(
            string siteId,
            List<string> infraServiceIds,
            string searchText,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetInfraDatabaseById
        Task<InfraDatabase> GetInfraDatabaseById(string id);
        #endregion

        #region GetInfraDatabaseInDetailById
        Task<Models.InfraDatabase> GetInfraDatabaseInDetailById(string id);
        #endregion

        #region InsertInfraDatabase
        void InsertInfraDatabase(Models.InfraDatabase entity);
        #endregion

        #region UpdateInfraDatabase
        void UpdateInfraDatabase(Models.InfraDatabase entity);
        #endregion

        #region DeleteInfraDatabase
        void DeleteInfraDatabase(Models.InfraDatabase entity);
        #endregion
    }
}
