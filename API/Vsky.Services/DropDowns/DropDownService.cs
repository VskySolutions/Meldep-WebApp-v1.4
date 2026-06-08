using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.DropDowns
{
    public class DropDownService : IDropDownService
    {
        #region Define Services
        /// <summary>
        /// Define Services
        /// </summary>
        private readonly IRepository<DropDown> _dropdownRepository;
        #endregion

        #region Services Initializations
        /// <summary>
        /// Services Initializations
        /// </summary>
        /// <param name="dropdownTypeRepository"></param>
        public DropDownService(IRepository<DropDown> dropDownRepository)
        {
            _dropdownRepository = dropDownRepository;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllDropDowns
        // Title: GetAllDropDowns
        // Description: This method retrieves a paginated list of dropdown based on various search criteria such as dropdown type, 
        // mappings. The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<DropDown> GetAllDropDowns(string SiteId, string SearchText, List<string> dropdownTypeIds, string sortBy,
            bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _dropdownRepository.TableNoTracking.Where(x => !x.Deleted && !x.DropDownType.Deleted && x.DropDownType.SiteId == SiteId);

            if (dropdownTypeIds != null && dropdownTypeIds.Any())
                query = query.Where(x => dropdownTypeIds.Contains(x.DropDownTypeId));

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.DropDownValue);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                    m.DropDownType.Type.ToLower().Contains(SearchText.ToLower()) ||
                    m.DropDownValue.ToLower().Contains(SearchText.ToLower())
                );
            }

                if (lookup)
            {
                query = query.Select(x => new DropDown
                {
                    Id = x.Id,
                    DropDownValue = x.DropDownValue,
                });
            }
            else
            {
                // projection
                query = query.Select(x => new DropDown
                {
                    Id = x.Id,
                    DropDownValue = x.DropDownValue,
                    Active = x.Active,
                    BgColor = x.BgColor,
                    Color = x.Color,
                    DropDownType = new DropDownType
                    {
                        Id = x.DropDownType.Id,
                        Type = x.DropDownType.Type
                    }
                });
            }

            var list = new PagedList<DropDown>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetByName
        // Title: GetByName
        // Description: This method retrieves a dropdown from the database by name. 
        public async Task<DropDown> GetByName(string SiteId, string name)
        {
            var query = _dropdownRepository.TableNoTracking.Where(x => !x.Deleted && x.DropDownValue == name && x.DropDownType.SiteId == SiteId && !x.DropDownType.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<DropDown> GetDropdownValueByName(string SiteId, string name)
        {
            var query = _dropdownRepository.TableNoTracking.Where(x => !x.Deleted && x.DropDownValue.Contains(name) && x.DropDownType.SiteId == SiteId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDropDowns
        // Title: GetDropDowns
        // Description: This method retrieves a dropdown from the database by its type. 
        public async Task<IList<DropDown>> GetDropDowns(string typeId)
        {
            var query = _dropdownRepository.TableNoTracking.Where(x => x.DropDownTypeId == typeId && !x.Deleted && !x.DropDownType.Deleted);

            query = query
                     .OrderBy(x => x.DropDownType.IsAlphabeticalOrNumerical ? x.SortOrder : 0)
                     .ThenBy(x => x.DropDownType.IsAlphabeticalOrNumerical ? "" : x.DropDownValue);
            return await query.ToListAsync();
        }
        #endregion

        #region GetAllDropDowns
        // Title: GetAllDropDowns
        // Description: This method retrieves a dropdown from the database. 
        public async Task<IList<DropDown>> GetAllDropDowns()
        {
            var query = _dropdownRepository.TableNoTracking.Where(x => !x.Deleted && !x.DropDownType.Deleted);
            var list = await query.OrderBy(x => x.DropDownValue).ToListAsync();
            return list;
        }
        #endregion

        #region GetDropDownById
        // Title: GetDropDownById
        // Description: This method retrieves a dropdown from the database by its unique identifier (`id`). 
        public async Task<DropDown> GetDropDownById(string id)
        {
            var query = _dropdownRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        public async Task<string> GetDropDownByTypeNameAndName(string SiteId, string TypeName, string Name)
        {
            var query = _dropdownRepository.TableNoTracking.Where(x => !x.DropDownType.Deleted && !x.Deleted && x.DropDownType.SiteId == SiteId && x.DropDownType.Type == TypeName && x.DropDownValue == Name);
            var item = await query.FirstOrDefaultAsync();
            return item?.Id ?? "";
        }
        #endregion

        #region GetDropDownByTypeAndValue
        // Title: GetDropDownByTypeAndValue
        // Description: This method retrieves a DropDown based on its value. It allows an optional exclusion of a DropDown by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific DropDown. The method returns the first matching DropDown or null if no match is found.
        public async Task<DropDown> GetDropDownByTypeAndValue(string SiteId, string dropDownTypeId, string value, string id = null)
        {
            var query = _dropdownRepository.TableNoTracking.Where(x => x.DropDownTypeId == dropDownTypeId && x.DropDownValue == value && !x.Deleted && x.DropDownType.SiteId == SiteId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        public async Task<IList<DropDown>> GetDropdownByButton(string buttonType, string dropDownTypeId)
        { 
            var list = await _dropdownRepository.TableNoTracking
                .Where(x => x.DropDownTypeId == dropDownTypeId && !x.Deleted && !x.DropDownType.Deleted)
                .ToListAsync();

            if (!list.Any())
                return new List<DropDown>();

            int total = list.Count;

            if (buttonType == "Break" || buttonType == "Time Adjustment")
            {
                    list = list.OrderBy(x => x.SortOrder).Take(total - 3).ToList();
            }
            else if (buttonType == "Work From Home")
            {
                list = list.OrderByDescending(x => x.SortOrder).Take(3).ToList();
                list = list.OrderBy(x => x.SortOrder).ToList();
            }

            return list;
        }


        #region InsertDropDown
        // Title: InsertDropDown
        // Description: This method inserts a new dropdown entity into the repository. It takes a dropdown object as input and uses the _dropdownRepository to handle the insertion operation.
        public void InsertDropDown(DropDown entity)
        {
            _dropdownRepository.Insert(entity);
        }
        #endregion

        #region UpdateDropDown
        // Title: UpdateDropDown
        // Description: This method updates the specified dropdown entity in the repository. It takes a dropdown object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateDropDown(DropDown entity)
        {
            _dropdownRepository.Update(entity);
        }
        #endregion

        #region DeleteDropDown
        // Title: DeleteDropDown
        // Description: Marks the specified dropdown entity as deleted by setting its `Deleted` property to true.
        public void DeleteDropDown(DropDown entity)
        {
            entity.Deleted = true;

            _dropdownRepository.Update(entity);
        }
        #endregion
    }
}
