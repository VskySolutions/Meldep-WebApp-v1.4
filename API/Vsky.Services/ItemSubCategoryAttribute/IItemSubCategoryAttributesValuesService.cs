using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ItemSubCategoryAttribute
{
    public interface IItemSubCategoryAttributesValuesService
    {
        #region GetAllItemSubCategoryAttributes
        Task<List<ItemSubCategoryAttributesValues>> GetAllItemSubCategoryAttributeValues();
        #endregion

        #region GetAllItemSubcategoryAttributeValuesByAttributeId
        Task<List<ItemSubCategoryAttributesValues>> GetAllItemSubcategoryAttributeValuesByAttributeId(string itemSubCategoryAttributeId);
        #endregion

        #region GetItemSubCategoryAttributeById
        Task<ItemSubCategoryAttributesValues> GetItemSubCategoryAttributeValueById(string id);
        #endregion

        #region GetAttributeValueByAttributeIdTextAndSubCategoryId
        Task<ItemSubCategoryAttributesValues> GetAttributeValueByAttributeIdTextAndSubCategoryId(string attributeId, string text, string id = null);
        #endregion

        #region GetAttributeValueByAttributeIdValueAndSubCategoryId
        Task<ItemSubCategoryAttributesValues> GetAttributeValueByAttributeIdValueAndSubCategoryId(string attributeId, string value, string id = null);
        #endregion

        #region GetItemSubCategoryAttributeValueDetailsById
        Task<ItemSubCategoryAttributesValues> GetItemSubCategoryAttributeValueDetailsById(string id);
        #endregion

        #region InsertItemSubCategoryAttributeValue
        void InsertItemSubCategoryAttributeValue(Models.ItemSubCategoryAttributesValues entity);
        #endregion

        #region UpdateItemSubCategoryAttributeValue
        void UpdateItemSubCategoryAttributeValue(Models.ItemSubCategoryAttributesValues entity);
        #endregion

        #region DeleteItemSubCategoryAttributeValue
        void DeleteItemSubCategoryAttributeValue(Models.ItemSubCategoryAttributesValues entity);
        #endregion
    }
}
