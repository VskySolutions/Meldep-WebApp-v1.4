using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ItemCategories
{
    public class ItemSubcategoriesService : IItemSubcategoriesService
    {
        #region Define Services
        private readonly IRepository<Models.ItemSubcategory> _itemSubcategoryRepository;
        #endregion

        #region Services Initializations
        public ItemSubcategoriesService(IRepository<Models.ItemSubcategory> itemSubcategoryRepository)
        {
            _itemSubcategoryRepository = itemSubcategoryRepository;
        }
        #endregion

        #region GetAllItemSubcategoryList
        public async Task<List<ItemSubcategory>> GetAllItemSubcategoryList(string itemCategoryId)
        {
            var query = _itemSubcategoryRepository.TableNoTracking.Where(x => !x.Deleted && !x.ItemCategory.Deleted);

            if (!string.IsNullOrEmpty(itemCategoryId) && itemCategoryId != "undefined")
            {
                query = query.Where(x => x.ItemCategoryId == itemCategoryId);
            }

            query = query.OrderByDescending(x => x.CreatedOnUtc).Select(x => new ItemSubcategory
            {
                Id = x.Id,
                Name = x.Name,
                Prefix = x.Prefix,
                SortOrder = x.SortOrder,
                TotalSitesItemSubCategoryAttributesMappingCount = x.SitesItemSubCategoryAttributesMapping.Count(m =>
                 !m.Deleted &&
                 !m.ItemSubcategory.Deleted &&
                 !m.ItemSubCategoryAttributes.Deleted)
            });

            var list = await query.ToListAsync();
            return list;
        }
       #endregion

        #region GetItemSubcategoryById
        // Title: GetItemSubcategoryById
        // Description: This method retrieves a item subcategory from the database by its unique identifier (`id`). 
        public async Task<ItemSubcategory> GetItemSubcategoryById(string id)
        {
            var query = _itemSubcategoryRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion
        #region GetItemSubcategoryDetailsById
        // Title: GetItemSubcategoryDetailsById
        // Description: The method selects relevant fields from the Item Subcategory entity
        public async Task<ItemSubcategory> GetItemSubcategoryDetailsById(string id)
        {
            var query = _itemSubcategoryRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id)
                .Select(x => new ItemSubcategory
                {
                    Id = x.Id,
                    Name = x.Name,
                    Prefix = x.Prefix,
                    SortOrder = x.SortOrder,
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

        #region GetItemSubcategoryByPrefixOrName
        // Title: GetItemSubcategoryByPrefixOrName
        // Description: This method retrieves a item subcategory based on its prefix and name.
        public async Task<ItemSubcategory> GetItemSubcategoryByPrefixOrName(string prefix, string name = null, string id = null)
        {
            var query = _itemSubcategoryRepository.TableNoTracking
                         .Where(x => !x.Deleted && (x.Prefix.ToLower() == prefix.ToLower() || (name != null && x.Name.ToLower() == name.ToLower())));

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetItemSubcategoryByItemSubcategoryName
        // Title: GetItemSubcategoryByItemSubcategoryName
        // Description: This method retrieves a ItemSubcategory based on its value. It allows an optional exclusion of a ItemSubcategory by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific ItemSubcategory. The method returns the first matching ItemSubcategory or null if no match is found.
        public async Task<ItemSubcategory> GetItemSubcategoryByItemSubcategoryName(string itemSubcategory, string id = null)
        {
            var query = _itemSubcategoryRepository.TableNoTracking.Where(x => x.Name == itemSubcategory && !x.Deleted);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion


        #region GetItemSubcategoryByPrefix
        // Title: GetItemSubcategoryByPrefix
        // Description: This method retrieves a item subcategory based on its prefix.
        public async Task<ItemSubcategory> GetItemSubcategoryByPrefix(string prefix, string id = null)
        {
            var query = _itemSubcategoryRepository.TableNoTracking
                         .Where(x => !x.Deleted && (x.Prefix.ToLower() == prefix.ToLower()));

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertItemSubcategory
        public void InsertItemSubcategory(ItemSubcategory entity)
        {
            _itemSubcategoryRepository.Insert(entity);
        }
        #endregion

        #region UpdateItemSubcategory
        public void UpdateItemSubcategory(ItemSubcategory entity)
        {
            _itemSubcategoryRepository.Update(entity);
        }
        #endregion

        #region DeleteItemSubcategory
        public void DeleteItemSubcategory(ItemSubcategory entity)
        {
            entity.Deleted = true;
            _itemSubcategoryRepository.Update(entity);
        }
        #endregion
    }
}
