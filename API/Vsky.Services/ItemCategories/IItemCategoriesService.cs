using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ItemCategories
{
    public interface IItemCategoriesService
    {

        #region GetAllItemCategoryList
        Task<List<ItemCategory>> GetAllItemCategoryList();
        #endregion

        #region GetItemCategoryById
        Task<ItemCategory> GetItemCategoryById(string id);
        #endregion

        #region GetItemCategoryDetailsById
        Task<Models.ItemCategory> GetItemCategoryDetailsById(string id);
        #endregion

        #region GetItemCategoryByName
        Task<ItemCategory> GetItemCategoryByName(string name, string id = null);
        #endregion

        #region GetItemCategoryByPrefix
        Task<ItemCategory> GetItemCategoryByPrefix(string prefix, string id = null);
        #endregion

        #region InsertItemCategory
        void InsertItemCategory(Models.ItemCategory entity);
        #endregion

        #region UpdateItemCategory
        void UpdateItemCategory(Models.ItemCategory entity);
        #endregion

        #region DeleteItemCategory
        void DeleteItemCategory(Models.ItemCategory entity);
        #endregion
    }
}
