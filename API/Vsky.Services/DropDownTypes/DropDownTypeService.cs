using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Services.DropDownTypes
{
    public class DropDownTypeService : IDropDownTypeService
    {
        #region Define Services
        /// <summary>
        /// Define Services
        /// </summary>
        private readonly IRepository<DropDownType> _dropdownTypeRepository;
        #endregion

        #region Services Initializations
        /// <summary>
        /// Services Initializations
        /// </summary>
        /// <param name="dropdownTypeRepository"></param>
        public DropDownTypeService(IRepository<DropDownType> dropdownTypeRepository)
        {
            _dropdownTypeRepository = dropdownTypeRepository;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllDropDownTypes
        // Title: GetAllDropDownTypes
        // Description: This method retrieves a paginated list of dropdown type based on various search criteria such as dropdown type, 
        // mappings. The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<DropDownType> GetAllDropDownTypes(
            string SiteId,
            string SearchText,
            string moduleName,
            string groupName,
            List<string> dropdownTypeIds,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _dropdownTypeRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(moduleName))
                query = query.Where(x => x.ModuleName.ToLower().Contains(moduleName.ToLower()));

            if (!string.IsNullOrWhiteSpace(groupName))
                query = query.Where(x => x.GroupName.ToLower().Contains(groupName.ToLower()));

            if (dropdownTypeIds != null && dropdownTypeIds.Any())
                query = query.Where(x => dropdownTypeIds.Contains(x.Id));

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                    m.ModuleName.ToLower().Contains(SearchText.ToLower()) ||
                    m.GroupName.ToLower().Contains(SearchText.ToLower()) ||
                    m.Type.ToLower().Contains(SearchText.ToLower())
                );
            }
            
            query = query.OrderBy(x => x.GroupName).ThenBy(x => x.Type).Select(x => new DropDownType
            {
                Id = x.Id,
                Type = x.Type,
                GroupName = x.GroupName,
                ModuleName = x.ModuleName,

            });

            var list = new PagedList<DropDownType>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetAllDropDownTypeListForDropdown
        public async Task<List<DropDownType>> GetAllDropDownTypeListForDropdown(string SiteId)
        {
            var query = _dropdownTypeRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
            query = query.Select(x => new DropDownType
            {
                Id = x.Id,
                Type = x.Type
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllDropDownTypeListBySiteId
        public async Task<List<DropDownType>> GetAllDropDownTypeListBySiteId(string SiteId)
        {
            var query = _dropdownTypeRepository.TableNoTracking.Where(x => !x.Deleted);
            //var firstSiteId = await query.OrderBy(x => x.CreatedOnUtc).Select(x => x.SiteId).FirstOrDefaultAsync();
            var list = await query.Where(x => x.SiteId == SiteId).ToListAsync();
            return list;
        }
        #endregion
        #region GetFirstCreatedSiteId
        public async Task<DropDownType> GetFirstCreatedDropDownType()
        {
            return await _dropdownTypeRepository.Table .Where(x => !x.Deleted).OrderBy(x => x.CreatedOnUtc).FirstOrDefaultAsync();
        }
        #endregion

        #region GetDropDownTypeById
        // Title: GetDropDownTypeById
        // Description: This method retrieves a dropdown type from the database by its unique identifier (`id`). 
        public async Task<DropDownType> GetDropDownTypeById(string id)
        {
            var query = _dropdownTypeRepository.TableNoTracking.Where(x => x.Id == id);
            var item = await query.OrderBy(x => x.SortOrder).FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDropDownTypeDetailsById
        // Title: GetDropDownTypeDetailsById
        // Description:  The method selects relevant fields from the dropdown type entity, including related entities and returns a `DropDownType` object with these details. 
        public async Task<DropDownType> GetDropDownTypeDetailsById(string id)
        {
            var item = await _dropdownTypeRepository.TableNoTracking
                .Where(x => !x.Deleted && x.Id == id)
                .Select(x => new DropDownType
                {
                    Id = x.Id,
                    Type = x.Type,
                    SiteId = x.SiteId,
                    GroupName = x.GroupName,
                    ModuleName = x.ModuleName,
                    IsAlphabeticalOrNumerical = x.IsAlphabeticalOrNumerical 
                })
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(item.GroupName))
            {
                item.DropDownTypeList = await _dropdownTypeRepository.TableNoTracking
                    .Where(d => !d.Deleted && d.GroupName == item.GroupName && d.ModuleName == item.ModuleName && d.SiteId == item.SiteId)
                    .OrderByDescending(d => d.CreatedOnUtc)
                    .Select(d => new DropDownType
                    {
                        Id = d.Id,
                        Type = d.Type,
                        SortOrder = d.SortOrder
                    })
                    .ToListAsync();
            }
            else
            {
                // No group, just return empty list
                item.DropDownTypeList = new List<DropDownType>();
            }

            return item;
        }
        #endregion

        #region GetDropDownType
        // Title: GetDropDownType
        // Description: This method retrieves a dropdown type from the database by type.
        public async Task<DropDownType> GetDropDownType(string SiteId, string type)
        {
            var query = _dropdownTypeRepository.TableNoTracking.Where(x => x.Type == type && !x.Deleted && x.SiteId == SiteId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDropDownTypeForSite
        // Title: GetDropDownTypeForSite
        // Description: This method retrieves a dropdown type from the database by type and siteId.
        public async Task<DropDownType> GetDropDownTypeBySite(string siteId, string type)
        {
            var query = _dropdownTypeRepository.TableNoTracking.Where(x => x.Type == type && !x.Deleted && x.SiteId == siteId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDropDownTypeByType
        // Title: GetDropDownTypeByType
        // Description: This method retrieves a DropDown Type based on its type. It allows an optional exclusion of a DropDownType by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific DropDownType. The method returns the first matching DropDownType or null if no match is found.
        public async Task<DropDownType> GetDropDownTypeByType(string SiteId, string Type, string id = null)
        {
            var query = _dropdownTypeRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Type.Replace(" ", "").ToLower() == Type.Replace(" ", "").ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDropDownTypeByGroupName
        public async Task<DropDownType> GetDropDownTypeByGroupName(string SiteId, string groupName)
        {
            var query = _dropdownTypeRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.GroupName.Replace(" ", "").ToLower() == groupName.Replace(" ", "").ToLower());

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDropDownTypeListByGroupName
        public async Task<List<DropDownType>> GetDropDownTypeListByGroupName(string SiteId, string groupName)
        {
            var query = _dropdownTypeRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.GroupName == groupName);

            var item = await query.OrderBy(x => x.Type).ToListAsync();
            return item;
        }
        #endregion

        #region GetDropdownTypeByModuleName
        public async Task<List<DropDownType>> GetDropdownTypeListByModuleName(string SiteId, string moduleName)
        {
            var query = await _dropdownTypeRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.ModuleName == moduleName).ToListAsync();
            var result = query
                        .Select(x => 
                        { 
                               x.Type = string.IsNullOrEmpty(x.GroupName) ? x.Type : x.GroupName;
                                return x;
                        })
                        .GroupBy(x => string.IsNullOrWhiteSpace(x.GroupName) ? x.Type.ToLower() : x.GroupName.Trim().ToLower())
                        .Select(g => g.First())    
                        .OrderBy(x => x.Type)    
                        .ToList();

            return result;
        }
        #endregion

        #region InsertDropDownType
        // Title: InsertDropDownType
        // Description: This method inserts a new dropdown type entity into the repository. It takes a dropdown type object as input and uses the _dropdownTypeRepository to handle the insertion operation.
        public void InsertDropDownType(DropDownType entity)
        {
            _dropdownTypeRepository.Insert(entity);
        }
        #endregion

        #region InsertDropDownTypeList
        public void InsertDropDownTypeList(IList<DropDownType> entities)
        {
            _dropdownTypeRepository.Insert(entities);
        }
        #endregion

        #region UpdateDropDownType
        // Title: UpdateDropDownType
        // Description: This method updates the specified dropdown type entity in the repository. It takes a dropdown type object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateDropDownType(DropDownType entity)
        {
            _dropdownTypeRepository.Update(entity);
        }
        #endregion

        #region UpdateDropDownTypeList
        public void UpdateDropDownTypeList(IList<DropDownType> entities)
        {
            _dropdownTypeRepository.Update(entities);
        }
        #endregion

        #region DeleteDropDownType
        // Title: DeleteDropDownType
        // Description: Marks the specified dropdown type entity as deleted by setting its `Deleted` property to true.
        public void DeleteDropDownType(DropDownType entity)
        {
            entity.Deleted = true;

            _dropdownTypeRepository.Update(entity);
        }
        #endregion

        #region DeleteDropDownTypeList
        public void DeleteDropDownTypeList(List<DropDownType> entities)
        {
            var companyContacts = new List<DropDownType>();
            foreach (var items in entities)
            {
                items.Deleted = true;
                companyContacts.Add(items);
            }
            _dropdownTypeRepository.Update(companyContacts);
        }
        #endregion
    }
}
