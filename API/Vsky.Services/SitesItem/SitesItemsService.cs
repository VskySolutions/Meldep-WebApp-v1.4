using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;

namespace Vsky.Services.SitesItem
{
    public class SitesItemsService : ISitesItemsService
    {
        #region Define Services
        private readonly IRepository<SitesItems> _sitesItemsRepository;
        #endregion

        #region Services Initializations
        public SitesItemsService(IRepository<SitesItems> sitesItemsRepository)
        {
            _sitesItemsRepository = sitesItemsRepository;
        }
        #endregion

        #region Private Methods
        // Title: GetOrderBy
        // Description: This method returns the input string as it is, which can be used as the `ORDER BY` clause in a SQL query.
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllSitesItems
        // Title: GetAllSitesItems
        // Description: This method retrieves a paginated list of site items based on various search criteria such as name, It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public async Task<IPagedList<SitesItems>> GetAllSitesItemList(
            string SiteId,
            string SearchText,
            List<string> itemSubcategoryIds,
            string itemName,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
            )
        {
            var query = _sitesItemsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(itemName))
            {
                itemName = itemName.Trim().ToLower();
                query = query.Where(x => x.ItemName.ToLower().Contains(itemName));
            }

            if (itemSubcategoryIds?.Any() == true) query = query.Where(x => itemSubcategoryIds.Contains(x.ItemSubCategoryId));

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.ItemSubcategory.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.ItemName.Contains(SearchText.ToLower()) ||
                     (m.CreatedBy.Person.FirstName + " " + m.CreatedBy.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                     m.CreatedOnUtc.Date == parsedDate.Date
                    );
            }

            query = query.Select(x => new SitesItems
            {
                Id = x.Id,
                ItemName = x.ItemName,
                CreatedOnUtc = x.CreatedOnUtc,
                ItemSubCategoryId = x.ItemSubCategoryId,
                ItemSubcategory = new ItemSubcategory
                {
                    Id = x.ItemSubcategory.Id,
                    Name = x.ItemSubcategory.Name
                },
                CreatedBy = new ApplicationUser
                {
                        Id = x.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = x.CreatedBy.PersonId,
                            FirstName = x.CreatedBy.Person.FirstName,
                            LastName = x.CreatedBy.Person.LastName,
                        }
                },
            });

            var list = new PagedList<SitesItems>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetSitesItemByItemName
        // Title: GetSitesItemByItemName
        // Description: This method retrieves a sites items based on its item name.
        public async Task<SitesItems> GetSitesItemByItemName(string itemSubCategoryId, string itemName, string id = null)
        {
            var query = _sitesItemsRepository.TableNoTracking.Where(x => !x.Deleted && x.ItemSubCategoryId == itemSubCategoryId && x.ItemName.ToLower() == itemName.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetSitesItemDetailsById
        // Title: GetSitesItemDetailsById
        // Description: The method selects relevant fields from the sites item entity,and returns a sites item object with these details. 
        public async Task<SitesItems> GetSitesItemDetailsById(string id)
        {
            var query = _sitesItemsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new SitesItems
            {
                Id = x.Id,
                ItemSubCategoryId = x.ItemSubcategory.Id,
                ItemName = x.ItemName,
                Description = x.Description,
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
                },
                SitesItemsAttributeList = x.SitesItemsAttributeList.Where(m => m.SiteItemId == x.Id && !m.Deleted).OrderByDescending(m => m.CreatedOnUtc).Select(x => new SitesItemsAttributes
                    {
                        Id = x.Id,
                        ItemSubCategoryAttributeId = x.ItemSubCategoryAttributeId,
                        Value = x.Value,
                        CreatedOnUtc = x.CreatedOnUtc,
                        UpdatedOnUtc = x.UpdatedOnUtc,
                        ItemSubCategoryAttributes = new ItemSubCategoryAttributes
                        {
                            Id = x.ItemSubCategoryAttributes.Id,
                            Name = x.ItemSubCategoryAttributes.Name
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
                        },
                    }).ToList(),
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetSitesItemById
        // Title: GetSitesItemById
        // Description: This method retrieves a sites item from the database by its unique identifier (`id`). 
        public async Task<SitesItems> GetSitesItemById(string id)
        {
            var query = _sitesItemsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertSitesItem
        // Title: InsertSitesItem
        // Description: This method inserts a new sites item entity into the repository. It takes a Sites Item object as input and uses the _sitesItemsRepository to handle the insertion operation.
        public void InsertSitesItem(SitesItems entity)
        {
            _sitesItemsRepository.Insert(entity);
        }
        #endregion

        #region UpdateSitesItem
        // Title: UpdateSitesItem
        // Description: This method updates the specified sites item entity in the repository. It takes a sites item object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateSitesItem(SitesItems entity)
        {
            _sitesItemsRepository.Update(entity);
        }
        #endregion

        #region DeleteSitesItem
        // Title: DeleteSitesItem
        // Description: Marks the specified sites items entity as deleted by setting its `Deleted` property to true. 
        public void DeleteSitesItem(SitesItems entity)
        {
            entity.Deleted = true;
            _sitesItemsRepository.Update(entity);
        }
        #endregion
    }
}
