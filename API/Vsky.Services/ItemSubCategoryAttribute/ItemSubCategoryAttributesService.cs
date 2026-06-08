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
    public class ItemSubCategoryAttributesService : IItemSubCategoryAttributesService
    {
        #region Define Services
        private readonly IRepository<ItemSubCategoryAttributes> _itemSubCategoryAttributesRepository;
        #endregion

        #region Services Initializations

        public ItemSubCategoryAttributesService(IRepository<ItemSubCategoryAttributes> itemSubCategoryAttributesRepository)
        {
            _itemSubCategoryAttributesRepository = itemSubCategoryAttributesRepository;
        }
        #endregion

        #region GetAllItemSubCategoryAttributeList
        public async Task<List<ItemSubCategoryAttributes>> GetAllItemSubCategoryAttributeList(string itemSubCategoryId)
        {
            var query = _itemSubCategoryAttributesRepository.TableNoTracking.Where(x => !x.Deleted);
            if (!string.IsNullOrEmpty(itemSubCategoryId))
            {
                query = query.Where(x =>
                    x.ItemSubCategoryAttributesValues.Any(v =>
                        !v.Deleted && v.ItemSubCategoryId == itemSubCategoryId));
            }
            query = query.OrderByDescending(x => x.CreatedOnUtc).Select(x => new ItemSubCategoryAttributes
            {
                Id = x.Id,
                Name = x.Name,
                FieldType = x.FieldType,
                TotalItemSubCategoryAttributesValuesCount = x.ItemSubCategoryAttributesValues.Count(m => !m.Deleted),
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllAttributesWithNullSubCategory
        public async Task<List<ItemSubCategoryAttributes>> GetAllAttributesWithNullSubCategory()
        {
            var query = _itemSubCategoryAttributesRepository.TableNoTracking.Where(x => !x.Deleted && x.ItemSubCategoryAttributesValues.Any(m => !m.Deleted && m.ItemSubCategoryId == null));
            query = query.OrderByDescending(x => x.CreatedOnUtc).Select(x => new ItemSubCategoryAttributes
            {
                Id = x.Id,
                Name = x.Name,
                FieldType = x.FieldType,
                TotalItemSubCategoryAttributesValuesCount = x.ItemSubCategoryAttributesValues.Count(m => !m.Deleted),
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetItemSubCategoryAttributeById
        // Title: GetItemSubCategoryAttributeById
        // Description: This method retrieves a ItemSubCategoryAttributes from the database by its unique identifier (`id`). 
        public async Task<ItemSubCategoryAttributes> GetItemSubCategoryAttributeById(string id)
        {
            var query = _itemSubCategoryAttributesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetItemSubCategoryAttributeDetailsById
        // Title: GetItemSubCategoryAttributeDetailsById
        // Description: The method selects relevant fields from the Item SubCategory Attributes entity
        public async Task<ItemSubCategoryAttributes> GetItemSubCategoryAttributeDetailsById(string id)
        {
            var query = _itemSubCategoryAttributesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id)
             .Select(x => new ItemSubCategoryAttributes
             {
                 Id = x.Id,
                 Name = x.Name,
                 FieldType = x.FieldType,
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
                 },
                 ItemSubCategoryAttributesValues = x.ItemSubCategoryAttributesValues.Where(m => m.ItemSubCategoryAttributeId == x.Id && !m.Deleted)
                    .OrderByDescending(x => x.CreatedOnUtc).Select(d => new ItemSubCategoryAttributesValues
                    {
                        Id = d.Id,
                        Text = d.Text,
                        Value = d.Value,
                        SortOrder = d.SortOrder,
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
                        },
                    }).ToList(),
             });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetItemSubCategoryAttributeByName
        // Title: GetItemSubCategoryAttributeByName
        // Description: This method retrieves a item subcategory attribute based on its name.
        public async Task<ItemSubCategoryAttributes> GetItemSubCategoryAttributeByName(string name, string id = null)
        {
            var query = _itemSubCategoryAttributesRepository.TableNoTracking.Where(x => !x.Deleted && x.Name.ToLower() == name.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertItemSubCategoryAttribute
        public void InsertItemSubCategoryAttribute(Models.ItemSubCategoryAttributes entity)
        {
            _itemSubCategoryAttributesRepository.Insert(entity);
        }
        #endregion

        #region UpdateItemSubCategoryAttribute
        public void UpdateItemSubCategoryAttribute(Models.ItemSubCategoryAttributes entity)
        {
            _itemSubCategoryAttributesRepository.Update(entity);
        }
        #endregion

        #region DeleteItemSubCategoryAttribute
        public void DeleteItemSubCategoryAttribute(Models.ItemSubCategoryAttributes entity)
        {
            entity.Deleted = true;
            _itemSubCategoryAttributesRepository.Update(entity);
        }
        #endregion
    }
}
