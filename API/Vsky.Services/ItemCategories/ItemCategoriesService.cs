using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using MailKit.Search;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ItemCategories
{
    public class ItemCategoriesService : IItemCategoriesService
    {
        #region Define Services
        private readonly IRepository<ItemCategory> _itemCategoryRepository;
        #endregion

        #region Services Initializations
        public ItemCategoriesService(IRepository<ItemCategory> itemCategoryRepository)
        {
            _itemCategoryRepository = itemCategoryRepository;
        }
        #endregion

        #region GetAllItemCategoryList
        public async Task<List<ItemCategory>> GetAllItemCategoryList()
        {
            var query = _itemCategoryRepository.TableNoTracking.Where(x => !x.Deleted && x.GroupName == "Inventory");
            query = query.OrderByDescending(x => x.CreatedOnUtc).Select(x => new ItemCategory
            {
                Id = x.Id,
                Name = x.Name,
                Prefix = x.Prefix,
                TotalItemSubcategoryCount = x.ItemSubcategory.Count(m => !m.Deleted),
                TotalSitesItemSubCategoryAttributesMappingCount = x.ItemSubcategory.Where(m => !m.Deleted).SelectMany(m => m.SitesItemSubCategoryAttributesMapping).Count(sm => !sm.Deleted &&
                  !sm.ItemSubCategoryAttributes.Deleted)
            });
           
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetItemCategoryById
        // Title: GetItemCategoryById
        // Description: This method retrieves a ItemCategory from the database by its unique identifier (`id`). 
        public async Task<ItemCategory> GetItemCategoryById(string id)
        {
            var query = _itemCategoryRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetItemCategoryByName
        // Title: GetItemCategoryByName
        // Description: This method retrieves a item category based on its name.
        public async Task<ItemCategory> GetItemCategoryByName(string name, string id = null)
        {
            var query = _itemCategoryRepository.TableNoTracking.Where(x => !x.Deleted && x.Name.ToLower() == name.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetItemCategoryByPrefix
        // Title: GetItemCategoryByPrefix
        // Description: This method retrieves a item category based on its prefix.
        public async Task<ItemCategory> GetItemCategoryByPrefix(string prefix, string id = null)
        {
            var query = _itemCategoryRepository.TableNoTracking.Where(x => !x.Deleted && x.Prefix.ToLower() == prefix.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetItemCategoryDetailsById
        // Title: GetItemCategoryDetailsById
        // Description: The method selects relevant fields from the Item Category entity
        public async Task<ItemCategory> GetItemCategoryDetailsById(string id)
        {
            var query = _itemCategoryRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id)
                .Select(x => new ItemCategory
                {
                    Id = x.Id,
                    Name = x.Name,
                    Prefix = x.Prefix,
                    CreatedOnUtc = x.CreatedOnUtc,
                    UpdatedOnUtc = x.UpdatedOnUtc,
                    CreatedBy = new ApplicationUser
                    {
                        Id = x.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = x.CreatedBy.PersonId,
                            FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                        }
                    },
                    UpdatedBy = new ApplicationUser
                    {
                        Id = x.UpdatedBy.Id,
                        Person = new Person
                        {
                            Id = x.UpdatedBy.PersonId,
                            FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName
                        }
                    }
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertItemCategory
        public void InsertItemCategory(Models.ItemCategory entity)
        {
            _itemCategoryRepository.Insert(entity);
        }
        #endregion

        #region UpdateItemCategory
        public void UpdateItemCategory(Models.ItemCategory entity)
        {
            _itemCategoryRepository.Update(entity);
        }
        #endregion

        #region DeleteItemCategory
        public void DeleteItemCategory(Models.ItemCategory entity)
        {
            entity.Deleted = true;
            _itemCategoryRepository.Update(entity);
        }
        #endregion

    }
}
