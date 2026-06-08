using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.Note;
using Vsky.Services.ProjectActivities;
using Vsky.Services.Sites;
using Vsky.Services.SitesItemSubCategoryAttributesMappings;

namespace Vsky.Api.Controllers
{
    [Route("sites_item_subcategory_attributes_mapping")]
    public class SitesItemSubCategoryAttributesMappingController : BaseController
    {

        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISitesItemSubCategoryAttributesMappingService _sitesItemSubCategoryAttributesMappingService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        #endregion

        #region Services Initializations
        public SitesItemSubCategoryAttributesMappingController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISitesItemSubCategoryAttributesMappingService sitesItemSubCategoryAttributesMappingService,
            ICommonService commonService,
            ISiteService siteService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _sitesItemSubCategoryAttributesMappingService = sitesItemSubCategoryAttributesMappingService;
            _commonService = commonService;
            _siteService = siteService;

        }
        #endregion

        #region GetAllSitesItemSubCategoryAttributesListByItemSubCategoryId
        // Title: GetAllSitesItemSubCategoryAttributesListByItemSubCategoryId
        // Description: This endpoint retrieves the list of all site item subcategories attributes associated with the siteId. 
        [HttpGet("sites_item_subcategory_attributes-list")]
        public async Task<IActionResult> GetAllSitesItemSubCategoryAttributesListByItemSubCategoryId(string itemSubCategoryId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _sitesItemSubCategoryAttributesMappingService.GetAllSitesItemSubCategoryAttributesListByItemSubCategoryId(SiteId, itemSubCategoryId);
                var model = _mapper.Map<List<SitesItemSubCategoryAttributesMapping>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateSitesItemSubCategoryAttributesMapping
        // Title: CreateSitesItemSubCategoryAttributesMapping
        // Description: This endpoint handles the creation of site item subcategory attribute mappings. It saves multiple attribute mappings for a selected site and item subcategory.
        [HttpPost]
        public async Task<IActionResult> CreateSitesItemSubCategoryAttributesMapping(SaveItemSubCategoryAttributesMapping model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entities = new List<SitesItemSubCategoryAttributesMapping>();
                    foreach (var item in model.ItemSubCategoryAttributeIds)
                    {
                       
                        var entity = new SitesItemSubCategoryAttributesMapping
                        {
                            SiteId = SiteId,
                            ItemSubCategoryId = model.ItemSubCategoryId,
                            ItemSubCategoryAttributeId = item,
                            CreatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime,
                        };
                        entities.Add(entity);
                        
                    }
                    _sitesItemSubCategoryAttributesMappingService.InsertSitesItemSubCategoryAttributesMappingList(entities);
                    return NoContent();
                }
                // Return model state errors if the model state is not valid
                return ModelStateError(ModelState);
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteSitesItemSubCategoryAttributesMapping
        // Title: DeleteSitesItemSubCategoryAttributesMapping
        // Description: Deletes selected site item subcategory attribute mapping records by Id.
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteSitesItemSubCategoryAttributesMapping(string ids)
        {
            try
            {
                var entityList = new List<SitesItemSubCategoryAttributesMapping>();
                foreach (var id in ids.Split(','))
                {
                    var entity = await _sitesItemSubCategoryAttributesMappingService.GetSitesItemSubCategoryAttributeById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No item subCategory attribute found with the specified id."));
                  
                    entityList.Add(entity);
                }
                _sitesItemSubCategoryAttributesMappingService.DeleteSitesItemSubCategoryAttributesMappingList(entityList);

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

