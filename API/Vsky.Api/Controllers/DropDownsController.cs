using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("drop-downs")]
    public class DropDownsController : BaseController
    {
        #region Define Services
        private readonly IMapper _mapper;
        private readonly GlobalVariable _globalVariable;
        private readonly IDropDownService _dropDownService;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations
        public DropDownsController(
            IMapper mapper,
            GlobalVariable globalVariable,
            IDropDownService dropDownService, 
            ISiteService siteService, 
            ICommonService commonService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _siteService = siteService;
            _dropDownService = dropDownService;
            _commonService = commonService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllDropDowns
        // Title: Get All DropDowns
        // Description: This endpoint fetches a list of DropDowns based on the provided search criteria such as dropdown type, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllDropDowns(DropDownSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of DropDowns based on search criteria (dropdown type, sorting, pagination)
                var list = _dropDownService.GetAllDropDowns(SiteId, searchModel.SearchText, searchModel.DropDownTypeIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new DropDownListModel
                {
                    Data = _mapper.Map<IList<DropDownModel>>(list),
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

        #region GetDropDownById
        // Title: GetDropDownById
        // Description: This endpoint retrieves the details of a specific Dropdown value based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDropDownById(string id)
        {
            var entity = await _dropDownService.GetDropDownById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No drop down found with the specified id."));

            var model = _mapper.Map<DropDownModel>(entity);
            return Ok(model);
        }

        [HttpGet("GetByTypeNameandName/{TypeName}/{Name}")]
        public async Task<IActionResult> GetDropDownIdByTypeNameAndName(string TypeName, string Name)
        {
            if (string.IsNullOrEmpty(TypeName) || string.IsNullOrEmpty(Name))
                return BadRequest(new BadRequestError("No Data Found."));

            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var id = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, TypeName, Name);

            return Ok(id);
        }
        #endregion

        #region CreateDropDown
        // Title: CreateDropDown
        // Description: This endpoint handles the creation of a new dropdown. It first checks if a dropdown with the same name already exists for the specified type. If not, it maps the dropdown model to the dropdown entity, sets the creation details, and inserts the dropdown into the database.
        [HttpPost]
        public async Task<IActionResult> CreateDropDown(DropDownModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the dropdown already exists
                    var exists = await _dropDownService.GetDropDownByTypeAndValue(SiteId, model.DropDownTypeId, model.DropDownValue);
                    if (exists != null)
                        return BadRequest(new BadRequestError("DropDown value already exists, Please try with another."));

                    var entity = _mapper.Map<DropDown>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "dropdown",
                                entity.Id
                            );
                    }

                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _dropDownService.InsertDropDown(entity);

                    return Ok(new { id = entity.Id });
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateDropDown
        // Title: UpdateDropDown
        // Description: This endpoint updates an existing dropdown by its ID. It validates the dropdown model, checks for duplicate dropdown value within the same dropdown type, updates the dropdown's details.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDropDown(string id, DropDownModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                //Check if the dropdown already exists
                var exists = await _dropDownService.GetDropDownByTypeAndValue(SiteId, model.DropDownTypeId, model.DropDownValue, id);
                if (exists != null)
                    return BadRequest(new BadRequestError("DropDown value already exists, Please tye with another."));

                var entity = await _dropDownService.GetDropDownById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No drop down found with the specified id."));

                
                if (!string.IsNullOrEmpty(model.Description))
                {
                    entity.Description = await _azureBlobImageServices
                        .ProcessHtmlAndManageImagesAsync(
                            model.Description,
                            SiteData.Name,
                            "dropdown",
                            entity.Id,
                            entity.Description
                        );
                }

                entity.DropDownValue = model.DropDownValue;
                entity.DropDownText = model.DropDownText;
                entity.SortOrder = model.SortOrder;
                entity.Active = model.Active;
                entity.Description = model.Description;
                entity.BgColor = model.BgColor;
                entity.Color = model.Color;
                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;
                _dropDownService.UpdateDropDown(entity);

                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteDropDown
        // Title: DeleteDropDownById
        // Description: This endpoint deletes a dropdown based on the provided dropdown ID. It first retrieves the dropdown entity by ID, checks if it exists, and if so, deletes the dropdown. If the dropdown is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDropDown(string id)
        {
            var entity = await _dropDownService.GetDropDownById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No drop down found with the specified id."));

            _dropDownService.DeleteDropDown(entity);

            return NoContent();
        }
        #endregion
    }
}