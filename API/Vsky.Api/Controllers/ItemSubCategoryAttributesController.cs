using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.ItemSubCategoryAttribute;
using Vsky.Services.Sites;
using Vsky.Api.ApiErrors;
using System.Linq;
using Vsky.Services.SitesItemSubCategoryAttributesMappings;

namespace Vsky.Api.Controllers
{
    [Route("item-subcategory-attributes")]
    public class ItemSubCategoryAttributesController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IItemSubCategoryAttributesService _itemSubCategoryAttributesService;
        private readonly IItemSubCategoryAttributesValuesService _itemSubCategoryAttributesValuesService;
        private readonly ISitesItemSubCategoryAttributesMappingService _sitesItemSubCategoryAttributesMappingService;
        #endregion

        #region Services Initializations
        public ItemSubCategoryAttributesController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISiteService siteService,
            ICommonService commonService,
            IItemSubCategoryAttributesService itemSubCategoryAttributesService,
            IItemSubCategoryAttributesValuesService itemSubCategoryAttributesValuesService,
            ISitesItemSubCategoryAttributesMappingService sitesItemSubCategoryAttributesMappingService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _siteService = siteService;
            _commonService = commonService;
            _itemSubCategoryAttributesService = itemSubCategoryAttributesService;
            _itemSubCategoryAttributesValuesService = itemSubCategoryAttributesValuesService;
            _sitesItemSubCategoryAttributesMappingService = sitesItemSubCategoryAttributesMappingService;
        }
        #endregion

        #region GetAllItemSubCategoryAttributeList
        // Title: GetAllItemSubCategoryAttributeList
        // Description: This endpoint retrieves the list of item subcategory attribute list.
        [HttpGet("item-subcategory-attribute-list")]
        public async Task<IActionResult> GetAllItemSubCategoryAttributeList()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _itemSubCategoryAttributesService.GetAllItemSubCategoryAttributeList(null);
                var model = _mapper.Map<List<ItemSubCategoryAttributes>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        [HttpGet("item-subcategory-attribute-list/{itemSubCategoryId}")]
        public async Task<IActionResult> GetItemAttributeListNotInMappingAsync(string itemSubCategoryId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Get data
                var allItemAttributes = await _itemSubCategoryAttributesService.GetAllItemSubCategoryAttributeList(itemSubCategoryId);

                var nullSubCategoryAttributes = await _itemSubCategoryAttributesService.GetAllAttributesWithNullSubCategory();

                allItemAttributes.AddRange(nullSubCategoryAttributes);

                var mappedAttributes = await _sitesItemSubCategoryAttributesMappingService.GetAttributeMappingByItemSubCategoryId(SiteId, itemSubCategoryId);

                List<ItemSubCategoryAttributes> itemSubCategoryAttributesList;

                if (mappedAttributes == null || !mappedAttributes.Any())
                {
                    itemSubCategoryAttributesList = allItemAttributes;
                }
                else
                {
                    var mappedAttributeIds = mappedAttributes.Select(x => x.ItemSubCategoryAttributeId).ToList();

                    itemSubCategoryAttributesList = allItemAttributes
                        .Where(x => !mappedAttributeIds.Contains(x.Id))
                        .ToList();
                }

                var model = new
                {
                    itemSubcategoryAttributes = itemSubCategoryAttributesList,
                    Total = itemSubCategoryAttributesList.Count
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        #region GetAllItemSubcategoryAttributeValuesByAttributeId
        // Title: GetAllItemSubcategoryAttributeValuesByAttributeId
        // Description: This endpoint retrieves the details of a specific item subcategory attribute value based on its unique identifier (ID). 
        [HttpGet("item-subcategory-attribute-value-list/{itemSubcategoryAttributeId}")]
        public async Task<IActionResult> GetAllItemSubcategoryAttributeValuesByAttributeId(string itemSubcategoryAttributeId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _itemSubCategoryAttributesValuesService.GetAllItemSubcategoryAttributeValuesByAttributeId(itemSubcategoryAttributeId);
                var model = _mapper.Map<List<ItemSubCategoryAttributesValues>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetItemSubCategoryAttributeDetailsById
        // Title: GetItemSubCategoryAttributeDetailsById
        // Description: This endpoint retrieves the details of a specific item subcategory attribute based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemSubCategoryAttributeDetailsById(string id)
        {
            try
            {
                var entity = await _itemSubCategoryAttributesService.GetItemSubCategoryAttributeDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No item subcategory attribute found with the specified id."));

                var model = _mapper.Map<ItemSubCategoryAttributes>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetItemSubCategoryAttributeValueDetailsById
        // Title: GetItemSubCategoryAttributeValueDetailsById
        // Description: This endpoint retrieves the details of a specific item subcategory attribute value based on its unique identifier (ID). 
        [HttpGet("item-subcategory-attribute-value/{id}")]
        public async Task<IActionResult> GetItemSubCategoryAttributeValueDetailsById(string id)
        {
            try
            {
                var entity = await _itemSubCategoryAttributesValuesService.GetItemSubCategoryAttributeValueDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No item subcategory attribute value found with the specified id."));

                var model = _mapper.Map<ItemSubCategoryAttributesValues>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateItemSubCategoryAttribute
        // Title: CreateItemSubCategoryAttribute
        // Description: This endpoint handles the creation of a new item subcategory attributes. It first checks if a item subcategory attribute with the same name already exists. If not, it maps the item subcategory attributes model to the item subcategory attributes entity, sets the creation details, and inserts the item subcategory attributes into the database.
        [HttpPost("createItemSubCategoryAttribute")]
        public async Task<IActionResult> CreateItemSubCategoryAttribute(SaveItemSubCategoryAttributes model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var existingItemCategory = await _itemSubCategoryAttributesService.GetItemSubCategoryAttributeByName(model.Name);
                    if (existingItemCategory != null)
                        return BadRequest(new BadRequestError("Item attribute already exists, try with another."));

                    var itemSubCategoryAttributes = new ItemSubCategoryAttributes
                    {
                        Name = model.Name,
                        FieldType = model.FieldType,
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime,
                    };
                    _itemSubCategoryAttributesService.InsertItemSubCategoryAttribute(itemSubCategoryAttributes);

                    return Ok(new { itemSubCategoryAttributes.Id, itemSubCategoryAttributes.Name });
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateItemSubCategoryAttributeValue
        // Title: CreateItemSubCategoryAttributeValue
        // Description: This endpoint handles the creation of a new item subcategory attribute value. It first checks if a item subcategory attribute value with the same name already exists. If not, it maps the item subcategory attributes value model to the item subcategory attribute value entity, sets the creation details, and inserts the item subcategory attribute value into the database.
        [HttpPost("createItemSubCategoryAttributeValue")]
        public async Task<IActionResult> CreateItemSubCategoryAttributeValue(SaveItemSubCategoryAttributesValues model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var existingItemCategoryText = await _itemSubCategoryAttributesValuesService.GetAttributeValueByAttributeIdTextAndSubCategoryId(model.ItemSubCategoryAttributeId, model.Text);
                    if (existingItemCategoryText != null)
                        return BadRequest(new BadRequestError("Item attribute text already exists, try with another."));

                    var existingItemCategoryValue = await _itemSubCategoryAttributesValuesService.GetAttributeValueByAttributeIdValueAndSubCategoryId(model.ItemSubCategoryAttributeId, model.Value);
                    if (existingItemCategoryValue != null)
                        return BadRequest(new BadRequestError("Item attribute value already exists, try with another."));

                    var itemSubCategoryAttributesValue = new ItemSubCategoryAttributesValues
                    {
                        ItemSubCategoryAttributeId = model.ItemSubCategoryAttributeId,
                        ItemSubCategoryId = model.ItemSubCategoryId,
                        Text = model.Text,
                        Value = model.Value,
                        SortOrder = model.SortOrder,
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime,
                    };
                    _itemSubCategoryAttributesValuesService.InsertItemSubCategoryAttributeValue(itemSubCategoryAttributesValue);

                    return Ok(new { itemSubCategoryAttributesValue.Id, itemSubCategoryAttributesValue.ItemSubCategoryId, itemSubCategoryAttributesValue.Text, itemSubCategoryAttributesValue.Value });
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateItemSubCategoryAttribute
        // Title: UpdateItemSubCategoryAttribute
        // Description: This endpoint updates an existing item category by its ID. 
        [HttpPut("updateItemSubCategoryAttribute/{id}")]
        public async Task<IActionResult> UpdateItemSubCategoryAttribute(string id, SaveItemSubCategoryAttributes model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _itemSubCategoryAttributesService.GetItemSubCategoryAttributeById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No item attribute found with the specified id."));

                    var existingItemCategory = await _itemSubCategoryAttributesService.GetItemSubCategoryAttributeByName(model.Name, id);
                    if (existingItemCategory != null)
                        return BadRequest(new BadRequestError("Item attribute name already exists, try with another."));

                    entity.Name = model.Name;
                    entity.FieldType = model.FieldType;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _itemSubCategoryAttributesService.UpdateItemSubCategoryAttribute(entity);

                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region UpdateItemSubCategoryAttributeValue
        // Title: UpdateItemSubCategoryAttributeValue
        // Description: This endpoint updates an existing item category by its ID. 
        [HttpPut("updateItemSubCategoryAttributeValue/{id}")]
        public async Task<IActionResult> UpdateItemSubCategoryAttributeValue(string id, SaveItemSubCategoryAttributesValues model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _itemSubCategoryAttributesValuesService.GetItemSubCategoryAttributeValueById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No item attribute value found with the specified id."));

                    var existingItemCategoryText = await _itemSubCategoryAttributesValuesService.GetAttributeValueByAttributeIdTextAndSubCategoryId(model.ItemSubCategoryAttributeId, model.Text, id);
                    if (existingItemCategoryText != null)
                        return BadRequest(new BadRequestError("Item attribute text already exists, try with another."));

                    var existingItemCategoryValue = await _itemSubCategoryAttributesValuesService.GetAttributeValueByAttributeIdValueAndSubCategoryId(model.ItemSubCategoryAttributeId, model.Value, id);
                    if (existingItemCategoryValue != null)
                        return BadRequest(new BadRequestError("Item  attribute value already exists, try with another."));

                    entity.ItemSubCategoryId = model.ItemSubCategoryId;
                    entity.Text = model.Text;
                    entity.Value = model.Value;
                    entity.SortOrder = model.SortOrder;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _itemSubCategoryAttributesValuesService.UpdateItemSubCategoryAttributeValue(entity);

                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region DeleteItemSubCategoryAttribute
        //Title: DeleteItemSubCategoryAttribute
        //Description: This endpoint deletes a item subCategory attribute based on the provided item subCategory attribute ID.It first retrieves the item subCategory attribute entity by ID, checks if it exists, and if so, deletes the item subCategory attribute.If the item subCategory attribute is not found, it returns a BadRequest response with an error message.

        [HttpDelete("deleteItemSubCategoryAttribute/{id}")]
        public async Task<IActionResult> DeleteItemSubCategoryAttribute(string id)
        {
            try
            {
                // Fetch the item subCategory attribute entity by its ID
                var entity = await _itemSubCategoryAttributesService.GetItemSubCategoryAttributeById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No item attribute found with the specified id."));

                _itemSubCategoryAttributesService.DeleteItemSubCategoryAttribute(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteItemSubCategoryAttributeValue
        //Title: DeleteItemSubCategoryAttributeValue
        //Description: This endpoint deletes a item subCategory attribute value based on the provided item subCategory attribute value ID.It first retrieves the item subCategory attribute value entity by ID, checks if it exists, and if so, deletes the item subCategory attribute value.If the item subCategory attribute value is not found, it returns a BadRequest response with an error message.

        [HttpDelete("deleteItemSubCategoryAttributeValue/{id}")]
        public async Task<IActionResult> DeleteItemSubCategoryAttributeValue(string id)
        {
            try
            {
                var entity = await _itemSubCategoryAttributesValuesService.GetItemSubCategoryAttributeValueById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No item attribute value found with the specified id."));

                _itemSubCategoryAttributesValuesService.DeleteItemSubCategoryAttributeValue(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region checkItemSubcategoryAttributeCanBeDeleted
        // Title: checkItemSubcategoryAttributeCanBeDeleted
        // Description: A item category cannot be deleted if it has any active (non-deleted) item subcategory.
        [HttpGet("checkItemSubcategoryAttributeCanBeDeleted/{itemSubcategoryAttributeId}")]
        public async Task<IActionResult> CheckItemSubcategoryAttributeCanBeDeleted(string itemSubcategoryAttributeId)
        {
            // Fetch all item subcategories attributes values under this item subcategories attribute
            var itemSubcategoriesAttributesValues = await _itemSubCategoryAttributesValuesService.GetAllItemSubcategoryAttributeValuesByAttributeId(itemSubcategoryAttributeId);

            // Check if any a attributes value exist
            bool hasActiveItemSubcategoriesAttributesValues = itemSubcategoriesAttributesValues.Any(q => !q.Deleted);

            // If any active attributes value exist, we cannot delete
            bool canDelete = !hasActiveItemSubcategoriesAttributesValues;

            return Ok(new { canDelete });
        }
        #endregion

    }
}
