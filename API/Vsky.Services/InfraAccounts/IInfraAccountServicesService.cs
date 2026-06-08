using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.InfraAccounts
{
    public interface IInfraAccountServicesService
    {
        #region GetAllInfraAccountServicesList
        public IPagedList<InfraAccountServices> GetAllInfraAccountServicesList(
            string siteId,
            List<string> projectIds,
            List<string> itemTypeIds,
            List<string> infraAccountIds,
            List<string> ownerShipTypeIds,
            List<string> paymentTermIds,
            string searchText,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetInfraAccountServices
        Task<InfraAccountServices> GetInfraAccountServicesById(string id);
        Task<InfraAccountServices> GetInfraAccountServicesInDetailById(string id);
        Task<List<CommonDropDown>> GetAllInfraServiceListForDropdown(string SiteId, string accountId = null);
        #endregion

        #region HasActiveServices
        Task<bool> HasActiveServices(string accountId);
        #endregion

        #region GetInfraAccountServicesByName
        Task<InfraAccountServices> GetInfraAccountServicesByName(string SiteId, string name, string accountId, string id = null);
        #endregion

        #region UpdateInfraAccountServices
        void UpdateInfraAccountServices(Models.InfraAccountServices entity);
        #endregion

        #region DeleteInfraAccountServices
        void DeleteInfraAccountServices(Models.InfraAccountServices entity);
        #endregion

        #region InsertInfraAccountServicesList
        void InsertInfraAccountServicesList(IList<InfraAccountServices> entities);
        #endregion

        #region UpdateInfraAccountServicesList
        void UpdateInfraAccountServicesList(IList<InfraAccountServices> entities);
        #endregion

        #region DeleteInfraAccountServicesList
        void DeleteInfraAccountServicesList(List<InfraAccountServices> entities);
        #endregion
    }
}
