using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ItemCategories
{
    public interface IItemSubcategoriesService
    {
        #region GetAllItemSubcategoryList
        Task<List<ItemSubcategory>> GetAllItemSubcategoryList(string itemCategoryId);
        #endregion

        #region GetItemSubcategoryById
        Task<ItemSubcategory> GetItemSubcategoryById(string id);
        #endregion

        #region GetItemSubcategoryDetailsById
        Task<ItemSubcategory> GetItemSubcategoryDetailsById(string id);
        #endregion

        #region GetItemSubcategoryByPrefix
        Task<ItemSubcategory> GetItemSubcategoryByPrefix(string prefix, string id = null);
        #endregion

        #region GetItemSubcategoryByPrefixOrName
        Task<ItemSubcategory> GetItemSubcategoryByPrefixOrName(string prefix, string name = null, string id = null);
        #endregion

        #region GetItemSubcategoryByItemSubcategoryName
        Task<ItemSubcategory> GetItemSubcategoryByItemSubcategoryName(string itemSubcategory, string id = null);
        #endregion

        #region InsertItemSubcategory
        void InsertItemSubcategory(ItemSubcategory entity);
        #endregion

        #region UpdateItemSubcategory
        void UpdateItemSubcategory(ItemSubcategory entity);
        #endregion

        #region DeleteItemSubcategory
        void DeleteItemSubcategory(ItemSubcategory entity);
        #endregion

    }
}
