using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.InfraAccounts
{
    public interface IInfraAccountService
    {
        #region GetAllInfraAccountList
        public IPagedList<InfraAccount> GetAllInfraAccountList(
            string siteId,
            string searchText,
            List<string> providerIds,
            List<string> infraAccountIds,
            string CCLast4Digits,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion
        #region GetAllInfraAccountListForDropdown
        Task<List<InfraAccount>> GetAllInfraAccountListForDropdown(string SiteId);
        #endregion

        #region GetInfraAccountById
        Task<InfraAccount> GetInfraAccountById(string id);
        #endregion

        #region GetInfraAccountDetailsById
        Task<Models.InfraAccount> GetInfraAccountDetailsById(string id);
        #endregion

        //#region GetInfraAccountByCustomerId
        //Task<InfraAccount> GetInfraAccountByCustomerId(string SiteId, string customerId, string id = null);
        //#endregion

        #region InsertInfraAccount
        void InsertInfraAccount(Models.InfraAccount entity);
        #endregion

        #region UpdateInfraAccount
        void UpdateInfraAccount(Models.InfraAccount entity);
        #endregion

        #region DeleteInfraAccount
        void DeleteInfraAccount(Models.InfraAccount entity);
        #endregion
    }
}
