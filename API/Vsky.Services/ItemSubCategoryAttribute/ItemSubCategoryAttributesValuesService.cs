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

namespace Vsky.Services.ItemSubCategoryAttribute
{
    public class ItemSubCategoryAttributesValuesService : IItemSubCategoryAttributesValuesService
    {
        #region Define Services
        private readonly IRepository<ItemSubCategoryAttributesValues> _itemSubCategoryAttributesValuesRepository;
        #endregion

        #region Services Initializations

        public ItemSubCategoryAttributesValuesService(IRepository<ItemSubCategoryAttributesValues> itemSubCategoryAttributesValuesRepository)
        {
            _itemSubCategoryAttributesValuesRepository = itemSubCategoryAttributesValuesRepository;
        }
        #endregion

        #region GetAllItemSubCategoryAttributeValues
        public async Task<List<ItemSubCategoryAttributesValues>> GetAllItemSubCategoryAttributeValues()
        {
            var query = _itemSubCategoryAttributesValuesRepository.TableNoTracking.Where(x => !x.Deleted && !x.ItemSubCategoryAttributes.Deleted);
            query = query.Select(x => new ItemSubCategoryAttributesValues
            {
                Id = x.Id,
                ItemSubCategoryAttributeId = x.ItemSubCategoryAttributeId,
                Text = x.Text,
                Value = x.Value,
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllItemSubcategoryAttributeValuesByAttributeId
        public async Task<List<ItemSubCategoryAttributesValues>> GetAllItemSubcategoryAttributeValuesByAttributeId(string itemSubCategoryAttributeId)
        {
            var query = _itemSubCategoryAttributesValuesRepository.TableNoTracking.Where(x => !x.Deleted && x.ItemSubCategoryAttributeId == itemSubCategoryAttributeId);

            query = query.OrderByDescending(x => x.CreatedOnUtc).Select(x => new ItemSubCategoryAttributesValues
            {
                Id = x.Id,
                ItemSubCategoryId = x.ItemSubCategoryId,
                Text = x.Text,
                Value = x.Value,
                SortOrder = x.SortOrder,
                ItemSubcategory = new ItemSubcategory
                {
                    Id = x.ItemSubcategory.Id,
                    Name = x.ItemSubcategory.Name,
                }
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetItemSubCategoryAttributeValueById
        // Title: GetItemSubCategoryAttributeValueById
        // Description: This method retrieves a item subcategory attribute value from the database by its unique identifier (`id`). 
        public async Task<ItemSubCategoryAttributesValues> GetItemSubCategoryAttributeValueById(string id)
        {
            var query = _itemSubCategoryAttributesValuesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAttributeValueByAttributeIdTextAndSubCategoryId
        // Title: GetAttributeValueByAttributeIdTextAndSubCategoryId
        // Description: This method retrieves a item subcategory attribute value  based on attribute id, itemSubCategoryId and text.
        public async Task<ItemSubCategoryAttributesValues> GetAttributeValueByAttributeIdTextAndSubCategoryId(string attributeId, string text, string id = null)
        {
            var query = _itemSubCategoryAttributesValuesRepository.TableNoTracking.Where(x => !x.Deleted && x.ItemSubCategoryAttributeId == attributeId && x.Text.ToLower() == text.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAttributeValueByAttributeIdValueAndSubCategoryId
        // Title: GetAttributeValueByAttributeIdValueAndSubCategoryId
        // Description: This method retrieves a item subcategory attribute value  based on its value and attribute id.
        public async Task<ItemSubCategoryAttributesValues> GetAttributeValueByAttributeIdValueAndSubCategoryId(string attributeId, string value, string id = null)
        {
            var query = _itemSubCategoryAttributesValuesRepository.TableNoTracking.Where(x => !x.Deleted && x.ItemSubCategoryAttributeId == attributeId && x.Value.ToLower() == value.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetItemSubCategoryAttributeValueDetailsById
        // Title: GetItemSubCategoryAttributeValueDetailsById
        // Description: The method selects relevant fields from the item subCategory attributes value entity
        public async Task<ItemSubCategoryAttributesValues> GetItemSubCategoryAttributeValueDetailsById(string id)
        {
            var query = _itemSubCategoryAttributesValuesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id)
                .Select(x => new ItemSubCategoryAttributesValues
                {
                    Id = x.Id,
                    ItemSubCategoryId = x.ItemSubCategoryId,
                    Text = x.Text,
                    Value = x.Value,
                    SortOrder = x.SortOrder,
                    CreatedOnUtc = x.CreatedOnUtc,
                    UpdatedOnUtc = x.UpdatedOnUtc,
                    ItemSubcategory = new ItemSubcategory
                    {
                        Id = x.ItemSubcategory.Id,
                        Name = x.ItemSubcategory.Name,
                    },
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

        #region InsertItemSubCategoryAttributeValue
        public void InsertItemSubCategoryAttributeValue(Models.ItemSubCategoryAttributesValues entity)
        {
            _itemSubCategoryAttributesValuesRepository.Insert(entity);
        }
        #endregion

        #region UpdateItemSubCategoryAttributeValue
        public void UpdateItemSubCategoryAttributeValue(Models.ItemSubCategoryAttributesValues entity)
        {
            _itemSubCategoryAttributesValuesRepository.Update(entity);
        }
        #endregion

        #region DeleteItemSubCategoryAttributeValue
        public void DeleteItemSubCategoryAttributeValue(Models.ItemSubCategoryAttributesValues entity)
        {
            entity.Deleted = true;
            _itemSubCategoryAttributesValuesRepository.Update(entity);
        }
        #endregion

    }
}
