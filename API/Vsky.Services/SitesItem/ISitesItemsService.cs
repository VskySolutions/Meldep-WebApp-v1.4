using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.SitesItem
{
    public interface ISitesItemsService
    {
        #region GetAllSitesItems
        Task<IPagedList<SitesItems>> GetAllSitesItemList(
            string SiteId,
            string SearchText,
            List<string> itemSubcategoryIds,
            string itemName,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetItemCategoryByItemName
        Task<SitesItems> GetSitesItemByItemName(string itemSubCategoryId, string itemName, string id = null);
        #endregion

        #region GetSitesItemById
        Task<SitesItems> GetSitesItemById(string id);
        #endregion

        #region GetSitesItemDetailsById
        Task<SitesItems> GetSitesItemDetailsById(string id);
        #endregion

        #region InsertSitesItem
        void InsertSitesItem(SitesItems entity);
        #endregion

        #region UpdateSitesItem
        void UpdateSitesItem(SitesItems entity);
        #endregion

        #region DeleteSitesItem
        void DeleteSitesItem(SitesItems entity);
        #endregion
    }
}
