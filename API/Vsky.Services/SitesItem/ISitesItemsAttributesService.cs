using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.SitesItem
{
    public interface ISitesItemsAttributesService
    {
        #region GetSitesItemsAttributeById
        Task<SitesItemsAttributes> GetSitesItemsAttributeById(string id);
        #endregion

        #region GetSitesItemsAttributeByName
        Task<SitesItemsAttributes> GetSitesItemsAttributeByValueAndSubCategoryId(string itemSubCategoryId, string value, string id = null);
        #endregion

        #region InsertSitesItemsAttributeList
        void InsertSitesItemsAttributeList(IList<SitesItemsAttributes> entity);
        #endregion

        #region UpdateSitesItemsAttributeList
        void UpdateSitesItemsAttributeList(IList<SitesItemsAttributes> entity);
        #endregion

        #region DeleteSitesItemsAttributeList
        void DeleteSitesItemsAttributeList(List<SitesItemsAttributes> entity);
        #endregion
    }
}
