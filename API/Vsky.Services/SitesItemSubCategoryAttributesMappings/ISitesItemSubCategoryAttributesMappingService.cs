using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.SitesItemSubCategoryAttributesMappings
{
    public interface ISitesItemSubCategoryAttributesMappingService
    {
        #region GetAllSitesItemSubCategoryAttributesList
        Task<List<SitesItemSubCategoryAttributesMapping>> GetAllSitesItemSubCategoryAttributesListByItemSubCategoryId(string SiteId, string itemSubCategoryId);
        #endregion

        #region GetSitesItemSubCategoryAttributeById
        Task<SitesItemSubCategoryAttributesMapping> GetSitesItemSubCategoryAttributeById(string id);
        #endregion

        #region GetAttributeMappingByItemSubCategoryId
        Task<List<SitesItemSubCategoryAttributesMapping>> GetAttributeMappingByItemSubCategoryId(string siteId, string itemSubCategoryId);
        #endregion

        #region InsertSitesItemSubCategoryAttributesMappingList
        void InsertSitesItemSubCategoryAttributesMappingList(IList<SitesItemSubCategoryAttributesMapping> entities);
        #endregion

        #region DeleteSitesItemSubCategoryAttributesMappingList
        void DeleteSitesItemSubCategoryAttributesMappingList(IList<SitesItemSubCategoryAttributesMapping> entities);
       #endregion
    }
}
