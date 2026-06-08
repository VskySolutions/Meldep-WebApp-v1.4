using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ItemSubCategoryAttribute
{
    public interface IItemSubCategoryAttributesService
    {
        #region GetAllItemSubCategoryAttributeList
        Task<List<ItemSubCategoryAttributes>> GetAllItemSubCategoryAttributeList(string itemSubCategoryId);
        #endregion

        #region GetAllAttributesWithNullSubCategory
        Task<List<ItemSubCategoryAttributes>> GetAllAttributesWithNullSubCategory();
        #endregion

        #region GetItemSubCategoryAttributeById
        Task<ItemSubCategoryAttributes> GetItemSubCategoryAttributeById(string id);
        #endregion

        #region GetItemSubCategoryAttributeByName
        Task<ItemSubCategoryAttributes> GetItemSubCategoryAttributeByName(string name, string id = null);
        #endregion

        #region GetItemSubCategoryAttributeDetailsById
        Task<ItemSubCategoryAttributes> GetItemSubCategoryAttributeDetailsById(string id);
        #endregion

        #region InsertItemSubCategoryAttribute
        void InsertItemSubCategoryAttribute(Models.ItemSubCategoryAttributes entity);
        #endregion

        #region UpdateItemSubCategoryAttribute
        void UpdateItemSubCategoryAttribute(Models.ItemSubCategoryAttributes entity);
        #endregion

        #region DeleteItemSubCategoryAttribute
        void DeleteItemSubCategoryAttribute(Models.ItemSubCategoryAttributes entity);
        #endregion
    }
}
