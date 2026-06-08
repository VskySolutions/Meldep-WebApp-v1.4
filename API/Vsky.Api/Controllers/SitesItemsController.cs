using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.SitesItem;
using Vsky.Services.Timesheets;
using Vsky.Services.Common;
using Vsky.Services.Sites;
using AngleSharp.Dom;
using Vsky.Services.AzureBlobImage;
using System.Linq;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Vsky.Api.Controllers
{
    [Route("sites-items")]
    public class SitesItemsController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISitesItemsService _sitesItemsService;
        private readonly ISitesItemsAttributesService _sitesItemsAttributesService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations
        public SitesItemsController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISitesItemsService sitesItemsService,
            ISitesItemsAttributesService sitesItemsAttributesService,
            ICommonService commonService,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _sitesItemsService = sitesItemsService;
            _sitesItemsAttributesService = sitesItemsAttributesService;
            _commonService = commonService;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllSitesItemList
        // Title: Get All Sites Item List
        // Description: This endpoint fetches a list of issue based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllSitesItemList(SitesItemsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var list = await _sitesItemsService.GetAllSitesItemList(
                    SiteId,
                    searchModel.SearchText,
                    searchModel.ItemSubcategoryIds,
                    searchModel.ItemName,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = new SitesItemList
                {
                    SitesItems = list,
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetSitesItemDetailsById
        // Title: GetSitesItemDetailsById
        // Description: This endpoint retrieves the details of a specific site item based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSitesItemDetailsById(string id)
        {
            try
            {
                var entity = await _sitesItemsService.GetSitesItemDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No site item found with the specified id."));

                // Map the person entity to a DepartmentModel object
                var model = _mapper.Map<SitesItems>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateSitesItem
        [HttpPost]
        public async Task<IActionResult> CreateSitesItem([FromBody] SaveSitesItem model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    
                    var existingSitesItem = await _sitesItemsService.GetSitesItemByItemName(model.ItemSubCategoryId, model.ItemName);
                    if (existingSitesItem != null)
                        return BadRequest(new BadRequestError("Item name already exists, try with another."));

                    var duplicateAttributes = new List<string>();
                    foreach (var item in model.SitesItemsAttributesList)
                    {
                        if (!item.Deleted)
                        {
                            var existingAttribute = await _sitesItemsAttributesService.GetSitesItemsAttributeByValueAndSubCategoryId(model.ItemSubCategoryId, item.Value);

                            if (existingAttribute != null)
                            {
                                duplicateAttributes.Add(item.Value);
                            }
                        }
                    }

                    // If there are duplicates, return a single message
                    if (duplicateAttributes.Any())
                    {
                        var message = duplicateAttributes.Count == 1
                            ? $"Site item attribute '{duplicateAttributes.First()}' already exists. Please try another."
                            : $"The following site item attributes already exist: {string.Join(", ", duplicateAttributes)}. Please try different ones.";

                        return BadRequest(new BadRequestError(message));
                    }

                    var sitesItem = new SitesItems
                    {
                        SiteId = SiteId,
                        ItemSubCategoryId = model.ItemSubCategoryId,
                        ItemName = model.ItemName,
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime,
                    };


                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        sitesItem.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "site-items",
                                sitesItem.Id
                            );
                    }
                    _sitesItemsService.InsertSitesItem(sitesItem);

                    var sitesItemsAttributes = new List<SitesItemsAttributes>();
                    if (model.SitesItemsAttributesList.Count() > 0)
                    {
                        foreach (var item in model.SitesItemsAttributesList)
                        {
                            if (!item.Deleted)
                            {
                                var existingSitesItemsAttribute = await _sitesItemsAttributesService.GetSitesItemsAttributeByValueAndSubCategoryId(model.ItemSubCategoryId, item.Value);
                                if (existingSitesItemsAttribute != null)
                                    return BadRequest(new BadRequestError("Site item attribute already exists, try with another."));

                                var sitesItemsAttribute = new SitesItemsAttributes
                                {
                                    Id = item.Id,
                                    SiteItemId = sitesItem.Id,
                                    ItemSubCategoryAttributeId = item.ItemSubCategoryAttributeId,
                                    Value = item.Value,
                                    CreatedById = LoggedUserId,
                                    UpdatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime,
                                    UpdatedOnUtc = GetDateTime,

                                };
                                sitesItemsAttributes.Add(sitesItemsAttribute);
                            }
                        }
                    }
                    if (sitesItemsAttributes.Any())
                        _sitesItemsAttributesService.InsertSitesItemsAttributeList(sitesItemsAttributes);

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateSitesItem
        // Title: UpdateSitesItem
        // Description: This endpoint updates an existing item category by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSitesItem(string id, [FromBody] SaveSitesItem model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _sitesItemsService.GetSitesItemById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No sites item found with the specified id."));

                    var existingSitesItem = await _sitesItemsService.GetSitesItemByItemName(model.ItemSubCategoryId, model.ItemName, id);
                    if (existingSitesItem != null)
                        return BadRequest(new BadRequestError("Item name already exists, try with another."));

                    var duplicateAttributes = new List<string>();
                    foreach (var item in model.SitesItemsAttributesList)
                    {
                        if (!item.Deleted)
                        {
                            var existingAttribute = await _sitesItemsAttributesService.GetSitesItemsAttributeByValueAndSubCategoryId(model.ItemSubCategoryId, item.Value, item.Id);

                            if (existingAttribute != null)
                            {
                                duplicateAttributes.Add(item.Value);
                            }
                        }
                    }

                    // If there are duplicates, return a single message
                    if (duplicateAttributes.Any())
                    {
                        var message = duplicateAttributes.Count == 1
                            ? $"Site item attribute '{duplicateAttributes.First()}' already exists. Please try another."
                            : $"The following site item attributes already exist: {string.Join(", ", duplicateAttributes)}. Please try different ones.";

                        return BadRequest(new BadRequestError(message));
                    }

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "site-items",
                                entity.Id
                            );
                    }
                    entity.ItemName = model.ItemName;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _sitesItemsService.UpdateSitesItem(entity);
                 
                        if (model.SitesItemsAttributesList.Count() > 0)
                        {
                            var addList = new List<SitesItemsAttributes>();
                            var deleteList = new List<SitesItemsAttributes>();
                            var updateList = new List<SitesItemsAttributes>();

                            foreach (var item in model.SitesItemsAttributesList)
                            {
                                // Fetch the FilePathDetails entity by its ID
                                var existingSitesItemAttribute = await _sitesItemsAttributesService.GetSitesItemsAttributeById(item.Id);

                                if (existingSitesItemAttribute != null && !item.Deleted)
                                {
                                    if (existingSitesItemAttribute == null)
                                        continue;

                                    existingSitesItemAttribute.ItemSubCategoryAttributeId = item.ItemSubCategoryAttributeId;
                                    existingSitesItemAttribute.Value = item.Value;
                                    existingSitesItemAttribute.UpdatedById = LoggedUserId;
                                    existingSitesItemAttribute.UpdatedOnUtc = GetDateTime;
                                    updateList.Add(existingSitesItemAttribute);
                                }
                                else if (existingSitesItemAttribute == null && !item.Deleted)
                                {
                                    var sitesItemsAttribute = new SitesItemsAttributes
                                    {
                                        Id = item.Id,
                                        SiteItemId = entity.Id,
                                        ItemSubCategoryAttributeId = item.ItemSubCategoryAttributeId,
                                        Value = item.Value,
                                        CreatedById = LoggedUserId,
                                        UpdatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime,
                                        UpdatedOnUtc = GetDateTime,

                                    };
                                    addList.Add(sitesItemsAttribute);
                                }
                                else if (existingSitesItemAttribute != null && item.Deleted)
                                {
                                    if (existingSitesItemAttribute == null)
                                        continue;

                                    deleteList.Add(existingSitesItemAttribute);
                                }
                            }

                            if (addList.Count > 0)
                            _sitesItemsAttributesService.InsertSitesItemsAttributeList(addList);

                            if (updateList.Count > 0)
                            _sitesItemsAttributesService.UpdateSitesItemsAttributeList(updateList);

                            if (deleteList.Count > 0)
                            _sitesItemsAttributesService.DeleteSitesItemsAttributeList(deleteList);
                        }
                        return Ok(entity);
                    }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region DeleteSitesItem
        // Title: DeleteSitesItem
        // Description: This endpoint deletes a sites item based on the provided id. It first retrieves the sites item entity by ID, checks if it exists, and if so, deletes the  sites item. If the  sites item is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSitesItem(string id)
        {
            try
            {

                var entity = await _sitesItemsService.GetSitesItemById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No sites item found with the specified id."));

                _sitesItemsService.DeleteSitesItem(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
