using System.Threading.Tasks;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Models;
using Vsky.Services.ItemCategories;
using Vsky.Services.Sites;
using Vsky.Services.Common;
using System.Linq;
using Vsky.Api.ApiErrors;
using System.Collections.Generic;

namespace Vsky.Api.Controllers
{
    [Route("item-category")]
    public class ItemCategoryController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IItemCategoriesService _itemCategoriesService;
        private readonly IItemSubcategoriesService _itemSubcategoriesService;
        #endregion

        #region Services Initializations
        public ItemCategoryController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISiteService siteService,
            ICommonService commonService,
            IItemCategoriesService itemCategoriesServices,
            IItemSubcategoriesService itemSubcategoriesService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _siteService = siteService;
            _commonService = commonService;
            _itemCategoriesService = itemCategoriesServices;
            _itemSubcategoriesService = itemSubcategoriesService;
        }
        #endregion

        #region GetAllItemCategoryList
        // Title: GetAllItemCategories
        // Description: This endpoint retrieves the list of item categories.
        [HttpGet("item-category-list")]
        public async Task<IActionResult> GetAllItemCategoryList()
        {
            try
            {
                var list = await _itemCategoriesService.GetAllItemCategoryList();
                var model = _mapper.Map<List<ItemCategory>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllItemSubcategoryList
        // Title: GetAllItemSubcategoryList
        // Description: This endpoint retrieves the list of all item subcategories associated with the specified item category. If an itemCategoryId is provided, only subcategories belonging to that category are returned.
        [HttpGet("item-subcategory-list")]
        public async Task<IActionResult> GetAllItemSubcategoryList(string itemCategoryId = null)
        {
            try
            {
                var list = await _itemSubcategoriesService.GetAllItemSubcategoryList(itemCategoryId);
                var model = _mapper.Map<List<ItemSubcategory>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GeneratePrefixForSubcategoryName
        // Title: GeneratePrefixForSubcategoryName
        // Description: This endpoint generates a unique prefix for a subcategory name based on defined rules.It ensures the generated prefix does not already exist for the current site.
        [HttpGet("prefix")]
        public async Task<string> GeneratePrefixForSubcategoryName(string subCategoryName, string subCategoryId = null)
        {
            if (string.IsNullOrWhiteSpace(subCategoryName))
                return string.Empty;

            string prefix = string.Empty;
            var words = subCategoryName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 1)
            {
                // Single-word subcategory
                var firstWord = words[0].ToUpper();
                // Start with first two letters
                prefix = firstWord.Length >= 2 ? $"VS/{firstWord[0]}{firstWord[1]}" : $"VS/{firstWord[0]}";

                // Check if the initial prefix already exists
                bool prefixExists = await _itemSubcategoriesService.GetItemSubcategoryByPrefix(prefix, subCategoryId) != null;

                // Try alternative letters if initial prefix is taken
                if (prefixExists)
                {
                    for (int i = 2; i < firstWord.Length; i++)
                    {
                        prefix = $"VS/{firstWord[0]}{firstWord[i]}";
                        prefixExists = await _itemSubcategoriesService.GetItemSubcategoryByPrefix(prefix, subCategoryId) != null;

                        if (!prefixExists)
                            break;
                    }
                }
            }
            else
            {
                // Multi-word subcategory: use first letter of first word + letters of second word
                var firstLetter = char.ToUpper(words[0][0]);
                var secondWord = words[1].ToUpper();

                foreach (var letter in secondWord)
                {
                    prefix = $"VS/{firstLetter}{letter}";

                    bool prefixExists = await _itemSubcategoriesService.GetItemSubcategoryByPrefix(prefix, subCategoryId) != null;

                    if (!prefixExists)
                        break;
                }
            }
            return prefix;
        }
        #endregion

        #region GetItemCategoryDetailsById
        // Title: GetItemCategoryDetailsById
        // Description: This endpoint retrieves the details of a specific item category based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemCategoryDetailsById(string id)
        {
            try
            {
                var entity = await _itemCategoriesService.GetItemCategoryDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No item category found with the specified id."));

                var model = _mapper.Map<ItemCategory>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetItemSubcategoryDetailsById
        // Title: GetItemSubcategoryDetailsById
        // Description: This endpoint retrieves the details of a specific item sub category based on its unique identifier (ID). 
        [HttpGet("item-subcategory/{id}")]
        public async Task<IActionResult> GetItemSubcategoryDetailsById(string id)
        {
            try
            {
                var entity = await _itemSubcategoriesService.GetItemSubcategoryDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No item subcategory found with the specified id."));

                var model = _mapper.Map<ItemSubcategory>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region SaveBulkItemSubcategories
        // Title: SaveBulkItemSubcategories
        // Description: This endpoint creates a new Item Category along with its subcategories in bulk.
        [HttpPost("saveBulkItemSubcategories")]
        public async Task<IActionResult> SaveBulkItemSubcategories(SaveItemCategory model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var existingItemCategoryName = await _itemCategoriesService.GetItemCategoryByName(model.Name);
                if (existingItemCategoryName != null)
                    return BadRequest(new BadRequestError("Item Category name already exists, try with another."));

                var duplicateMessages = new List<string>();
                foreach (var item in model.ItemSubcategoryList)
                {
                    if (!item.Deleted)
                    {
                        var existingItemSubcategory = await _itemSubcategoriesService.GetItemSubcategoryByPrefixOrName(item.Prefix, item.Name);
                        if (existingItemSubcategory != null)
                        {
                            if (existingItemSubcategory.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                                duplicateMessages.Add($"Subcategory name '{item.Name}'");

                            if (existingItemSubcategory.Prefix == item.Prefix)
                                duplicateMessages.Add($"Prefix '{item.Prefix}'");

                        }
                    }

                }
                if (duplicateMessages.Any())
                {
                    var message = duplicateMessages.Count == 1
                  ? $"{duplicateMessages.First()} already exists. Please try another."
                  : $"The following already exist: {string.Join(", ", duplicateMessages)}. Please try different ones.";
                    return BadRequest(new BadRequestError(message));
                }

                var itemCategory = new ItemCategory
                {
                    Name = model.Name,
                    Prefix = model.Prefix,
                    GroupName = model.GroupName,
                    CreatedById = LoggedUserId,
                    UpdatedById = LoggedUserId,
                    CreatedOnUtc = GetDateTime,
                    UpdatedOnUtc = GetDateTime,
                };
                _itemCategoriesService.InsertItemCategory(itemCategory);

                foreach (var item in model.ItemSubcategoryList)
                {
                    if (!item.Deleted)
                    {
                        var itemSubcategoryEntity = new ItemSubcategory
                        {
                            Id = item.Id,
                            ItemCategoryId = itemCategory.Id,
                            Name = item.Name,
                            Prefix = item.Prefix,
                            CreatedById = LoggedUserId,
                            UpdatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime,
                            UpdatedOnUtc = GetDateTime,

                        };
                        _itemSubcategoriesService.InsertItemSubcategory(itemSubcategoryEntity);
                    }

                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region CreateItemCategory
        // Title: CreateItemCategory
        // Description: This endpoint handles the creation of a new item category. It first checks if a item category with the same name already exists. If not, it maps the item category model to the item category entity, sets the creation details, and inserts the item category into the database.
        [HttpPost("createItemCategory")]
        public async Task<IActionResult> CreateItemCategory(SaveItemCategory model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var existingItemCategory = await _itemCategoriesService.GetItemCategoryByName(model.Name);
                    if (existingItemCategory != null)
                        return BadRequest(new BadRequestError("Item category name already exists, try with another."));
                    var existingItemCategoryPrefix = await _itemCategoriesService.GetItemCategoryByPrefix(model.Prefix);
                    if (existingItemCategoryPrefix != null)
                        return BadRequest(new BadRequestError("Item category prefix already exists, try with another."));

                    var itemCategory = new ItemCategory
                    {
                        Name = model.Name,
                        GroupName = model.GroupName,
                        Prefix = model.Prefix,
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime,
                    };
                    _itemCategoriesService.InsertItemCategory(itemCategory);

                    return Ok(new { itemCategory.Id, itemCategory.Name });
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateItemSubcategory
        // Title: CreateItemSubcategory
        // Description: This endpoint handles the creation of a new item subcategory. It first checks if a item subcategory with the same name already exists. If not, it maps the item subcategory model to the item subcategory entity, sets the creation details, and inserts the item sub category into the database.
        [HttpPost("createItemSubcategory")]
        public async Task<IActionResult> CreateItemSubCategory(SaveItemSubcategory model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var existingItemSubcategory = await _itemSubcategoriesService.GetItemSubcategoryByItemSubcategoryName(model.Name);

                    if (existingItemSubcategory != null)
                        return BadRequest(new BadRequestError("Item subcategory name already exists, try with another."));

                    var existingItemSubcategoryPrefix = await _itemSubcategoriesService.GetItemSubcategoryByPrefix(model.Prefix);
                    if (existingItemSubcategoryPrefix != null)
                        return BadRequest(new BadRequestError("Item subcategory prefix already exists, try with another."));

                    var itemSubcategory = new ItemSubcategory
                    {
                        ItemCategoryId = model.ItemCategoryId,
                        Name = model.Name,
                        Prefix = model.Prefix,
                        SortOrder = model.SortOrder,
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime,
                    };
                    _itemSubcategoriesService.InsertItemSubcategory(itemSubcategory);

                    return Ok(new { itemSubcategory.Id, itemSubcategory.Name });
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateItemCategory
        // Title: UpdateItemCategory
        // Description: This endpoint updates an existing item category by its ID. 
        [HttpPut("updateItemCategory/{id}")]
        public async Task<IActionResult> UpdateItemCategory(string id, SaveItemCategory model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _itemCategoriesService.GetItemCategoryById(id);
                    // If no Requirement is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No item category found with the specified id."));

                    var existingItemCategoryName = await _itemCategoriesService.GetItemCategoryByName(model.Name, id);
                    if (existingItemCategoryName != null)
                        return BadRequest(new BadRequestError("Item category name already exists, try with another."));

                    var existingItemCategoryPrefix = await _itemCategoriesService.GetItemCategoryByPrefix(model.Prefix, id);
                    if (existingItemCategoryPrefix != null)
                        return BadRequest(new BadRequestError("Item category prefix already exists, try with another."));

                    entity.Name = model.Name;
                    entity.Prefix = model.Prefix;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _itemCategoriesService.UpdateItemCategory(entity);
                   
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region UpdateItemSubcategory
        //Title: UpdateItemSubcategory
        //Description: This endpoint updates an existing item subcategory by its ID.

       [HttpPut("updateItemSubcategory/{id}")]
        public async Task<IActionResult> UpdateItemSubcategory(string id, SaveItemSubcategory model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _itemSubcategoriesService.GetItemSubcategoryById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No item subcategory found with the specified id."));

                    var existingItemSubcategory = await _itemSubcategoriesService.GetItemSubcategoryByItemSubcategoryName(model.Name, id);

                    if (existingItemSubcategory != null)
                        return BadRequest(new BadRequestError("Item subcategory name already exists, try with another."));

                    var existingItemSubcategoryPrefix = await _itemSubcategoriesService.GetItemSubcategoryByPrefix(model.Prefix, id);
                    if (existingItemSubcategoryPrefix != null)
                        return BadRequest(new BadRequestError("Item subcategory prefix already exists, try with another."));

                    entity.Name = model.Name;
                    entity.Prefix = model.Prefix;
                    entity.SortOrder = model.SortOrder;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _itemSubcategoriesService.UpdateItemSubcategory(entity);

                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region DeleteItemCategory
        // Title: DeleteItemCategory
        // Description: This endpoint deletes a item category based on the provided item category ID. It first retrieves the item category entity by ID, checks if it exists, and if so, deletes the item category. If the item category is not found, it returns a BadRequest response with an error message.
        [HttpDelete("deleteItemCategory/{id}")]
        public async Task<IActionResult> DeleteItemCategory(string id)
        {
            try
            {
                // Fetch the item category entity by its ID
                var entity = await _itemCategoriesService.GetItemCategoryById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No item category found with the specified id."));

                _itemCategoriesService.DeleteItemCategory(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteItemSubcategory
        //Title: DeleteItemSubcategory
        //Description: This endpoint deletes a item subcategory based on the provided item subcategory ID.It first retrieves the item subcategory entity by ID, checks if it exists, and if so, deletes the item subcategory.If the item subcategory is not found, it returns a BadRequest response with an error message.

       [HttpDelete("deleteItemSubcategory/{id}")]
        public async Task<IActionResult> DeleteItemSubcategory(string id)
        {
            try
            {
                // Fetch the item subcategory entity by its ID
                var entity = await _itemSubcategoriesService.GetItemSubcategoryById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No item subcategory found with the specified id."));

                _itemSubcategoriesService.DeleteItemSubcategory(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region checkItemCategoryCanBeDeleted
        // Title: checkItemCategoryCanBeDeleted
        // Description: A item category cannot be deleted if it has any active (non-deleted) item subcategory.
        [HttpGet("checkItemCategoryCanBeDeleted/{itemCategoryId}")]
        public async Task<IActionResult> CheckItemCategoryCanBeDeleted(string itemCategoryId)
        {
          
            // Fetch all item subcategories under this item category
            var itemSubcategories = await _itemSubcategoriesService.GetAllItemSubcategoryList(itemCategoryId);

            // Check if any a item subcategory exist
            bool hasActiveItemSubcategories = itemSubcategories.Any(q => !q.Deleted);

            // If any active item subcategory exist, we cannot delete
            bool canDelete = !hasActiveItemSubcategories;

            return Ok(new { canDelete });
        }
        #endregion
    }
}
