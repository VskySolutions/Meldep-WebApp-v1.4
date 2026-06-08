using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("drop-downs-type")]
    public class DropDownTypesController : BaseController
    {
        #region Define Services
        private readonly IMapper _mapper;
        private readonly GlobalVariable _globalVariable;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations
        public DropDownTypesController(
            IMapper mapper,
            GlobalVariable globalVariable,
            IDropDownService dropDownService, 
            IDropDownTypeService dropDownTypeService, 
            ISiteService siteService, 
            ICommonService commonService)
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _siteService = siteService;
            _commonService = commonService;
        }
        #endregion

        #region GetAllDropDownTypes
        // Title: Get All DropDownTypes
        // Description: This endpoint fetches a list of DropDowns based on the provided search criteria such as dropdown type, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllDropDownTypes(DropDownTypeSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var list = _dropDownTypeService.GetAllDropDownTypes(SiteId,
                    searchModel.SearchText,
                    searchModel.ModuleName,
                    searchModel.GroupName,
                    searchModel.DropDownTypeIds,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );
                // Map the fetched list to a model suitable for the response
                var model = new DropDownTypeListModel
                {
                    Data = _mapper.Map<IList<DropDownTypeModel>>(list),
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

        #region GetAllDropDownTypeListForDropdown
        // Title: GetAllDropDownTypeListForDropdown
        // Description: This endpoint retrieves the list of dropDown Types. 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllDropDownTypeListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _dropDownTypeService.GetAllDropDownTypeListForDropdown(SiteId);
                var model = _mapper.Map<List<DropDownTypeModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetDropDownTypeById
        // Title: GetDropDownTypeById
        // Description: This endpoint retrieves the details of a specific Dropdown type based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDropDownTypeById(string id)
        {
            
            var entity = await _dropDownTypeService.GetDropDownTypeDetailsById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No drop down type found with the specified id."));

            var model = _mapper.Map<DropDownTypeModel>(entity);
            return Ok(model);
        }
        #endregion

        #region GetDropdownTypeByGroupName
        // Title: GetDropdownTypeByGroupName
        // Description: This endpoint retrieves all dropdown types under the specified group name. 
        [HttpGet("dropDownTypeByGroupName")]
        public async Task<IActionResult> GetDropdownTypeByGroupName(string groupName)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            var list = await _dropDownTypeService.GetDropDownTypeListByGroupName(SiteId, groupName);
            var model = _mapper.Map<IList<DropDownTypeViewModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetDropdownByTypeId
        // Title: GetDropdownByTypeId
        // Description: This endpoint retrieves all dropdown values for the specified dropdown type ID.
        [HttpGet("dropDownValueByTypeId")]
        public async Task<IActionResult> GetDropdownByTypeId(string typeId)
        {
            var list = await _dropDownService.GetDropDowns(typeId);
            if (list != null)
            {
                var model = _mapper.Map<IList<DropDownViewModel>>(list);
                return Ok(model);
            }
            return Ok();
        }
        #endregion

        #region GetDropdownTypeByModuleName
        // Title: GetDropdownTypeByModuleName
        // Description: This endpoint retrieves all dropdown types associated with a given module name.
        [HttpGet("dropDownTypeByModuleName")]
        public async Task<IActionResult> GetDropdownTypeByModuleName(string moduleName)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            var list = await _dropDownTypeService.GetDropdownTypeListByModuleName(SiteId, moduleName);
            var model = _mapper.Map<IList<DropDownTypeViewModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetDropdownTypesByIdAndGroupName
        // Title: GetDropdownTypesByIdAndGroupName
        // Description: This endpoint retrieves dropdown types by group name or a single type by ID. 
        [HttpGet("{id}/{groupName}")]
        public async Task<IActionResult> GetDropdownTypesByIdAndGroupName(string id, string groupName)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            List<DropDownType> dropdownTypes;
            if (groupName != "undefined")
            {
                dropdownTypes = await _dropDownTypeService.GetDropDownTypeListByGroupName(SiteId, groupName);
            }
            else
            {
                var dropdownType = await _dropDownTypeService.GetDropDownTypeById(id);
                dropdownTypes = new List<DropDownType> { dropdownType };
            }
            return Ok(dropdownTypes);
        }
        #endregion

        #region Get
        // Title: Get
        // Description: This endpoint retrieves the details of a specific Dropdown value based on its unique identifier (ID). 
        [AllowAnonymous]
        [HttpGet("list")]
        public async Task<IActionResult> Get(string type)
        {
           
            var LoggedUserId = User.GetLoggedInUserId<string>();
            string SiteId = type == "Business Size" ? "04086f10-8f09-4e0c-b5c0-c827a244addd" : _globalVariable.SiteId;
            var item = await _dropDownTypeService.GetDropDownType(SiteId,type);
            if(item != null)
            {
                var list = await _dropDownService.GetDropDowns(item.Id);
                if(list != null)
                {
                    var model = _mapper.Map<IList<DropDownViewModel>>(list);
                    return Ok(model);
                }
            }
            return Ok();
        }
        #endregion

        #region GetDropdownForSite
        // Title: GetDropdownForSite
        // Description: This endpoint retrieves the details of a specific Dropdown value based on its  on the siteId and type.
        [HttpGet("dropdownlist")]
        public async Task<IActionResult> GetDropdownForSite(string siteId, string type)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
            var item = await _dropDownTypeService.GetDropDownTypeBySite(SiteId, type);
            if (item != null)
            {
                var list = await _dropDownService.GetDropDowns(item.Id);

                if (list != null)
                {
                    var model = _mapper.Map<IList<DropDownViewModel>>(list);
                    return Ok(model);
                }
            }
            else
            {
                List<DropDown> list = null;
                if (type == "Gender" || type == "Address Type")
                {
                    var firstSite = await _dropDownTypeService.GetFirstCreatedDropDownType();
                    if (firstSite != null && !string.IsNullOrEmpty(firstSite.SiteId))
                    {
                        var firstSiteItem = await _dropDownTypeService.GetDropDownTypeBySite(firstSite.SiteId, type);
                        if (firstSiteItem != null)
                        {
                            list = (await _dropDownService.GetDropDowns(firstSiteItem.Id)).ToList();
                            if (list != null)
                            {
                                var model = _mapper.Map<IList<DropDownViewModel>>(list);
                                return Ok(model);
                            }
                        }
                    }
                }
            }
            return Ok();
        }
        #endregion


        [HttpGet("dropdown-list/by-button")]
        public async Task<IActionResult> GetDropdownByButton(string buttonType, string type)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var item = await _dropDownTypeService.GetDropDownType(SiteId, type);
            if (item == null) return Ok(new List<DropDownViewModel>());

            // Call service method to get dropdown values based on button type
            var result = await _dropDownService.GetDropdownByButton(buttonType, item.Id);

            return Ok(result);
        }

        #region CreateDropDownType
        // Title: CreateDropDownType
        // Description: This endpoint handles the creation of a new dropdown type. It first checks if a dropdown type with the same name already exists for the specified type. If not, it maps the dropdown type model to the dropdown type entity, sets the creation details, and inserts the dropdown type into the database.
        [HttpPost]
        public async Task<IActionResult> CreateDropDownType(DropDownTypeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the dropdown type already exists
                    var exists = await _dropDownTypeService.GetDropDownTypeByType(SiteId, model.Type);
                    if (exists != null)
                        return BadRequest(new BadRequestError("DropDown Type already exists, Please try with another."));

                   
                    var entity = _mapper.Map<DropDownType>(model);
                    entity.SiteId = SiteId;
                    entity.DisplayName = model.Type;
                    entity.IsAlphabeticalOrNumerical = model.IsAlphabeticalOrNumerical;
                    entity.CreatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _dropDownTypeService.InsertDropDownType(entity);

                    return Ok(new { entity.Id, entity.Type,entity.SortOrder });
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region saveBulkDropDownTypes
        [HttpPost("save-bulk-dropDown-types")]
        public async Task<IActionResult> SaveBulkDropDownTypes(DropDownTypeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var addDropDownTypesList = new List<DropDownType>();

                    if (!string.IsNullOrEmpty(model.GroupName))
                    {
                        var existGroupName = await _dropDownTypeService.GetDropDownTypeByGroupName(SiteId, model.GroupName);
                        if (existGroupName != null)
                            return BadRequest(new BadRequestError("Group Name already exists, Please try with another."));
                    }

                    if (model.DropDownTypeList.Count() > 0)
                    {
                        foreach (var item in model.DropDownTypeList)
                        {
                            if (!item.Deleted)
                            {
                                var exists = await _dropDownTypeService.GetDropDownTypeByType(SiteId, item.Type);
                                if (exists != null)
                                    continue;

                                var dropDownTypeEntity = new DropDownType
                                {
                                    SiteId = SiteId,
                                    Type = item.Type,
                                    DisplayName = item.Type,
                                    SortOrder = item.SortOrder,
                                    GroupName = model.GroupName,
                                    ModuleName = model.ModuleName,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime,
                                    UpdatedById = LoggedUserId,
                                    UpdatedOnUtc = GetDateTime,

                                };
                                addDropDownTypesList.Add(dropDownTypeEntity);
                            }
                        }

                        if (addDropDownTypesList.Count > 0)
                        {
                            _dropDownTypeService.InsertDropDownTypeList(addDropDownTypesList);
                        }

                    }

                    return Ok();
                }

                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateDropDownType
        // Title: UpdateDropDownType
        // Description: This endpoint updates an existing dropdown type by its ID. It validates the dropdown type model, checks for duplicate dropdown type, updates the dropdown's details.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDropDownType(string id, DropDownTypeModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                //Check if the dropdown type already exists
                var exists = await _dropDownTypeService.GetDropDownTypeByType(SiteId, model.Type, id);
                if (exists != null)
                    return BadRequest(new BadRequestError("DropDown Type already exists, Please try with another."));

                var entity = await _dropDownTypeService.GetDropDownTypeById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No drop down type found with the specified id."));

                entity.Type = model.Type;
                entity.DisplayName = model.Type;
                entity.SortOrder = model.SortOrder;
                entity.GroupName = model.GroupName;
                entity.ModuleName = model.ModuleName;
                entity.IsAlphabeticalOrNumerical = model.IsAlphabeticalOrNumerical;
                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;
                _dropDownTypeService.UpdateDropDownType(entity);

                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region saveBulkDropDownTypes
        [HttpPut("save-bulk-dropDown-types/{id}")]
        public async Task<IActionResult> UpdateBulkDropDownTypes(string id, DropDownTypeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _dropDownTypeService.GetDropDownTypeById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No drop down type found with the specified id."));

                    var addDropDownTypesList = new List<DropDownType>();
                    var updateDropDownTypesList = new List<DropDownType>();
                    var deleteDropDownTypesList = new List<DropDownType>();

                    if (model.DropDownTypeList.Count() > 0)
                    {
                        foreach (var item in model.DropDownTypeList)
                        {
                            var exisitingDropDownTypeData = await _dropDownTypeService.GetDropDownTypeById(item.Id);

                            if (exisitingDropDownTypeData != null && !item.Deleted)
                            {
                                var exists = await _dropDownTypeService.GetDropDownTypeByType(SiteId, item.Type, item.Id);
                                if (exists != null)
                                    continue;

                                exisitingDropDownTypeData.Type = item.Type;
                                exisitingDropDownTypeData.DisplayName = item.Type;
                                exisitingDropDownTypeData.SortOrder = item.SortOrder;
                                exisitingDropDownTypeData.ModuleName = model.ModuleName;
                                exisitingDropDownTypeData.UpdatedById = LoggedUserId;
                                exisitingDropDownTypeData.UpdatedOnUtc = GetDateTime;
                                updateDropDownTypesList.Add(exisitingDropDownTypeData);
                            }
                            else if (exisitingDropDownTypeData == null && !item.Deleted)
                            {
                                var exists = await _dropDownTypeService.GetDropDownTypeByType(SiteId, item.Type);
                                if (exists != null)
                                    continue;

                                var dropDownTypeEntity = new DropDownType
                                {
                                    SiteId = SiteId,
                                    Type = item.Type,
                                    DisplayName = item.Type,
                                    SortOrder = item.SortOrder,
                                    GroupName = model.GroupName,
                                    ModuleName = model.ModuleName,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime,
                                    UpdatedById = LoggedUserId,
                                    UpdatedOnUtc = GetDateTime,

                                };
                                addDropDownTypesList.Add(dropDownTypeEntity);
                            }
                            else if (exisitingDropDownTypeData != null && item.Deleted)
                            {
                                var dropDownTypeData = await _dropDownTypeService.GetDropDownTypeById(item.Id);
                                if (dropDownTypeData == null)
                                    continue;
                                deleteDropDownTypesList.Add(dropDownTypeData);
                            }
                        }
                    }
                    if (addDropDownTypesList.Count > 0)
                    {
                        _dropDownTypeService.InsertDropDownTypeList(addDropDownTypesList);
                    }

                    if (updateDropDownTypesList.Count > 0)
                    {
                        _dropDownTypeService.UpdateDropDownTypeList(updateDropDownTypesList);
                    }

                    if (deleteDropDownTypesList.Count > 0)
                    {
                        _dropDownTypeService.DeleteDropDownTypeList(deleteDropDownTypesList);
                    }

                    return Ok(entity);
                }

                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteDropDown
        // Title: DeleteDropDownById
        // Description: This endpoint deletes a dropdown based on the provided dropdown ID. It first retrieves the dropdown entity by ID, checks if it exists, and if so, deletes the dropdown. If the dropdown is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDropDown(string id)
        {
            var entity = await _dropDownTypeService.GetDropDownTypeById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No drop down type found with the specified id."));

            _dropDownTypeService.DeleteDropDownType(entity);

            return NoContent();
        }
        #endregion
    }
}