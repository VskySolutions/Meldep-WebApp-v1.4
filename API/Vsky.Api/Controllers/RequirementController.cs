using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.PowerBI.Api.Models;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.FilePathDetail;
using Vsky.Services.ProjectTasks;
using Vsky.Services.Requirements;
using Vsky.Services.RequirementsColor;
using Vsky.Services.RequirementsPinned;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Api.Controllers
{
    [Route("requirement")]
    public class RequirementController : BaseController
    {

        #region Define Services      
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly IRequirementService _requirementService;
        private readonly IFilePathDetailsService _filePathDetailsService;
        private readonly IRequirementChangeLogService _requirementChangeLogService;
        private readonly ISiteService _siteService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly ApplicationDbContext _db;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly IRequirementTagService _requirementTagService;
        private readonly ITagService _tagService;
        private readonly IRequirementsPinnedService _requirementsPinnedService;
        private readonly IRequirementsColorService _requirementsColorService;
        #endregion

        #region Services Initializations      
        public RequirementController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            ICommonService commonService, 
            IDropDownService dropDownService,
            IDropDownTypeService dropDownTypeService, 
            IRequirementService requirementService, 
            IFilePathDetailsService filePathDetailsService,
            IRequirementChangeLogService requirementChangeLogService,
            ISiteService siteService,
            ISitesModifiedLogsService siteModifiedLogsService,
            ApplicationDbContext db,
            IAzureBlobImageServices azureBlobImageServices,
            IRequirementTagService requirementTagService,
            ITagService tagService,
            IRequirementsPinnedService requirementsPinnedService,
            IRequirementsColorService requirementsColorService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _commonService = commonService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _requirementService = requirementService;
            _filePathDetailsService = filePathDetailsService;
            _siteService = siteService;
            _sitesModifiedLogsService = siteModifiedLogsService;
            _requirementChangeLogService = requirementChangeLogService;
            _db = db;
            _azureBlobImageServices = azureBlobImageServices;
            _requirementTagService = requirementTagService;
            _tagService = tagService;
            _requirementsPinnedService = requirementsPinnedService;
            _requirementsColorService = requirementsColorService;
        }
        #endregion

        #region GetAllRequirements
        // Title: Get All Requirements
        // Description: This endpoint fetches a list of Requirements based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllRequirements(RequirementSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of Requirements on search criteria (name, sorting, pagination)
                var list = await _requirementService.GetAllRequirements(
                    SiteId,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.RequirementNumber,
                    searchModel.ProjectIds,
                    searchModel.ProjectModuleIds,
                    searchModel.RequirementGroupIds,
                    searchModel.Name,
                    searchModel.EditingStatus,
                    searchModel.StatusIds,
                    searchModel.RequirementTypeIds,
                    searchModel.identifiedUserTypeId,
                    searchModel.identifiedCustomerIds,
                    searchModel.identifiedEmployeeIds,
                    searchModel.RequirementTagIds,
                    searchModel.FromDate, 
                    searchModel.ToDate, 
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                    );

                // Map the fetched list to a model suitable for the response
                var model = new RequirementListModel
                {
                    Data = _mapper.Map<IList<RequirementModel>>(list),
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

        #region GetAllRequirementTagListForDropdown
        // Title: GetAllRequirementTagListForDropdown
        // Description: This endpoint retrieves all tags created by the logged-in user for dropdown display.
        [HttpGet("requirementTags/dropdown/list")]
        public async Task<IActionResult> GetAllRequirementTagListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();

                var list = await _requirementTagService.GetRequirementTagByUserId(LoggedUserId);
                var model = _mapper.Map<List<CommonDropDown>>(list);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetRequirementDetailsById
        // Title: GetRequirementDetailsById
        // Description: This endpoint retrieves the details of a specific Requirement based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetRequirementDetailsById(string id)
        {
            try
            {
                // Fetch the Requirement entity by its ID from the service
                var entity = await _requirementService.GetRequirementDetailsById(id);
                // If the Requirement entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Requirement found with the specified id."));

                // Map the Requirement entity to a RequirementModel object
                var model = _mapper.Map<RequirementModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetRequirementDescriptionById
        // Title: GetRequirementDescriptionById
        // Description: Retrieves only the description of a specific Requirement based on its ID.
        [HttpGet("description/{id}")]
        public async Task<IActionResult> GetRequirementDescriptionById(string id)
        {
            try
            {
                var description = await _requirementService.GetRequirementDescriptionById(id);
                return Ok(new
                {
                    description
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateRequirement
        // Title: CreateRequirement
        // Description: This endpoint handles the creation of a new Requirement. It maps the Requirement model to the Requirement entity, sets the creation details, and inserts the Requirement into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateRequirement(RequirementModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the project module already exists
                    var exists = await _requirementService.GetRequirementByName(model.Title, model.ProjectId, model.ProjectModuleId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Requirement already exists"));

                    var entity = new Requirement();
                    entity.RequirementNumber = await _requirementService.GetLastRequirementNumber() + 1;
                    entity.ProjectId = model.ProjectId;
                    entity.ProjectModuleId = model.ProjectModuleId;
                    entity.RequirementGroupId = model.RequirementGroupId;
                    entity.RequirementTypeId = model.RequirementTypeId;
                    entity.Title = model.Title;
                    entity.AreaId = model.AreaId;
                    entity.WorkspaceId = model.WorkspaceId;
                    entity.Notes = model.Notes;
                    entity.StatusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Requirement Status", "New");
                    entity.IdentifiedUserType = model.IdentifiedUserType;
                    entity.EditingStatus = model.EditingStatus;
                    entity.SiteId = SiteId;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "requirements",
                                entity.RequirementNumber.ToString()
                            );
                    }

                    if (model.PriorityId != null)
                        entity.PriorityId = model.PriorityId;

                    if (model.IdentifiedDateStr != "" && model.IdentifiedDateStr != null)
                        entity.IdentifiedDate = DateTime.ParseExact(model.IdentifiedDateStr, "MM/dd/yyyy", null);

                    if (model.CloseDateStr != "" && model.CloseDateStr != null)
                        entity.CloseDate = DateTime.ParseExact(model.CloseDateStr, "MM/dd/yyyy", null);

                    var UserTypeId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Requirement Identifier", "Employee");
                    if (model.IdentifiedUserType == UserTypeId)
                        entity.IdentifiedEmployeeId = model.IdentifiedEmployeeId;
                    else
                        entity.IdentifiedCustomerId = model.IdentifiedCustomerId;

                    if (model.RequirementEnteredBy != null && model.RequirementEnteredBy != "")
                        entity.RequirementEnteredBy = model.RequirementEnteredBy;
                    else
                        entity.RequirementEnteredBy = null;

                    if (model.ApprovalStatus != null && model.ApprovalStatus != "")
                        entity.ApprovalStatus = model.ApprovalStatus;
                    else
                        entity.ApprovalStatus = null;

                    // Set the created by and created on properties
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _requirementService.InsertRequirement(entity);

                    //save File Path Details
                    if (model.FilePathDetailsModel.Count() > 0)
                    {
                        var addList = new List<FilePathDetails>();
                        var deleteList = new List<FilePathDetails>();
                        var updateList = new List<FilePathDetails>();
                        foreach (var item in model.FilePathDetailsModel)
                        {
                            // Fetch the FilePathDetails entity by its ID
                            var type = await _filePathDetailsService.GetFilePathById(item.Id);
                            if (item.Flag == "Edit")
                            {
                                // If no FilePathDetails is found with the given ID, continue
                                if (type == null)
                                    continue;

                                type.ModuleId = entity.Id;
                                type.ModuleName = entity.Title;
                                type.FilePath = item.FilePath;
                                type.FileName = item.FileName;
                                type.Note = item.Note;
                                // Set the Updated by and Updated on properties
                                type.UpdatedById = LoggedUserId;
                                type.UpdatedOnUtc = GetDateTime;
                                updateList.Add(type);
                            }
                            else if (item.Flag == "New")
                            {
                                // If no FilePathDetails is found with the given ID, continue
                                if (type != null)
                                    continue;

                                var data = _mapper.Map<FilePathDetails>(item);
                                data.ModuleId = entity.Id;
                                data.ModuleName = entity.Title;
                                data.FilePath = item.FilePath;
                                data.FileName = item.FileName;
                                data.Note = item.Note;
                                data.CreatedById = LoggedUserId;
                                data.UpdatedById = LoggedUserId;
                                data.CreatedOnUtc = GetDateTime;
                                data.UpdatedOnUtc = GetDateTime;
                                addList.Add(data);
                            }
                            else if (item.Flag == "Delete")
                            {
                                // If no FilePathDetails is found with the given ID, continue
                                if (type == null)
                                    continue;

                                deleteList.Add(type);
                            }
                        }

                        if (addList.Count > 0)
                            _filePathDetailsService.InsertFilePathDetails(addList);

                        if (updateList.Count > 0)
                            _filePathDetailsService.UpdateFilePathDetails(updateList);

                        if (deleteList.Count > 0)
                            _filePathDetailsService.DeleteFilePathDetails(deleteList);
                    }
                    return Ok(entity.Id);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            // Return model state errors if the model state is not valid
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateRequirement
        // Title: UpdateRequirement
        // Description: This endpoint updates an existing Requirement by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequirement(string id, RequirementModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the Requirement entity by its ID
                    var entity = await _requirementService.GetRequirementById(id);
                    // If no Requirement is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Requirement found with the specified id."));

                    entity.ProjectId = model.ProjectId;
                    entity.ProjectModuleId = model.ProjectModuleId;
                    entity.RequirementGroupId = model.RequirementGroupId;
                    entity.RequirementTypeId = model.RequirementTypeId;
                    entity.Title = model.Title;
                    entity.AreaId = model.AreaId;
                    entity.WorkspaceId = model.WorkspaceId;
                    entity.Notes = model.Notes;
                    entity.StatusId = model.StatusId;
                    entity.IdentifiedUserType = model.IdentifiedUserType;
                    entity.EditingStatus = model.EditingStatus;

                    if (model.PriorityId != null)
                        entity.PriorityId = model.PriorityId;

                    if (model.CloseDateStr != "" && model.CloseDateStr != null)
                        entity.CloseDate = DateTime.ParseExact(model.CloseDateStr, "MM/dd/yyyy", null);

                    var UserTypeId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Requirement Identifier", "Employee");
                    if (model.IdentifiedUserType == UserTypeId)
                        entity.IdentifiedEmployeeId = model.IdentifiedEmployeeId;
                    else
                        entity.IdentifiedCustomerId = model.IdentifiedCustomerId;

                    if (model.IdentifiedDateStr != "" && model.IdentifiedDateStr != null)
                        entity.IdentifiedDate = DateTime.ParseExact(model.IdentifiedDateStr, "MM/dd/yyyy", null);

                    if (model.RequirementEnteredBy != null && model.RequirementEnteredBy != "")
                        entity.RequirementEnteredBy = model.RequirementEnteredBy;
                    else
                        entity.RequirementEnteredBy = null;

                    if (model.ApprovalStatus != null && model.ApprovalStatus != "")
                        entity.ApprovalStatus = model.ApprovalStatus;
                    else
                        entity.ApprovalStatus = null;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "requirements",
                                entity.RequirementNumber.ToString(),
                                entity.Description
                            );
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _requirementService.UpdateRequirement(entity);

                    //save Requirement Change Log
                    if (model.FilePathDetailsModel.Count() > 0)
                    {
                        var addList = new List<FilePathDetails>();
                        var deleteList = new List<FilePathDetails>();
                        var updateList = new List<FilePathDetails>();
                        foreach (var item in model.FilePathDetailsModel)
                        {
                            // Fetch the FilePathDetails entity by its ID
                            var type = await _filePathDetailsService.GetFilePathById(item.Id);

                            if (item.Flag == "Edit")
                            {
                                // If no FilePathDetails is found with the given ID, continue
                                if (type == null)
                                    continue;

                                type.ModuleId = entity.Id;
                                type.ModuleName = entity.Title;
                                type.FilePath = item.FilePath;
                                type.FileName = item.FileName;
                                type.Note = item.Note;
                                type.UpdatedById = LoggedUserId;
                                type.UpdatedOnUtc = GetDateTime;
                                updateList.Add(type);
                            }
                            else if (item.Flag == "New")
                            {
                                // If no FilePathDetails is found with the given ID, continue
                                if (type != null)
                                    continue;

                                var data = _mapper.Map<FilePathDetails>(item);

                                data.ModuleId = entity.Id;
                                data.ModuleName = entity.Title;
                                data.FilePath = item.FilePath;
                                data.FileName = item.FileName;
                                data.Note = item.Note;
                                data.CreatedById = LoggedUserId;
                                data.UpdatedById = LoggedUserId;
                                data.CreatedOnUtc = GetDateTime;
                                data.UpdatedOnUtc = GetDateTime;
                                addList.Add(data);
                            }
                            else if (item.Flag == "Delete")
                            {
                                // If no FilePathDetails is found with the given ID, continue
                                if (type == null)
                                    continue;

                                deleteList.Add(type);
                            }
                        }

                        if (addList.Count > 0)
                            _filePathDetailsService.InsertFilePathDetails(addList);

                        if (updateList.Count > 0)
                            _filePathDetailsService.UpdateFilePathDetails(updateList);

                        if (deleteList.Count > 0)
                            _filePathDetailsService.DeleteFilePathDetails(deleteList);
                    }

                    //save Requirement Change Log
                    if (model.RequirementChangeLogModel.Count() > 0)
                    {
                        var addList = new List<RequirementChangeLog>();
                        var deleteList = new List<RequirementChangeLog>();
                        var updateList = new List<RequirementChangeLog>();

                        foreach (var item in model.RequirementChangeLogModel)
                        {
                            // Fetch the RequirementChangeLog entity by its ID
                            var type = await _requirementChangeLogService.GetRequirementChangeLogById(item.Id);
                            if (item.Flag == "Edit")
                            {
                                // If no RequirementChangeLog is found with the given ID, continue
                                if (type == null)
                                    continue;

                                type.RequirementId = entity.Id;
                                type.EmployeeId = item.EmployeeId;
                                type.RequirementName = item.RequirementName;

                                if (!string.IsNullOrEmpty(item.Description))
                                {
                                    type.Description = await _azureBlobImageServices
                                        .ProcessHtmlAndManageImagesAsync(
                                            item.Description,
                                            SiteData.Name,
                                            "requirements",
                                            entity.RequirementNumber.ToString(),
                                            type.Description
                                        );
                                }

                                if (item.RequirementLogDateStr != "" && item.RequirementLogDateStr != null)
                                    type.RequirementLogDate = DateTime.ParseExact(item.RequirementLogDateStr, "MM/dd/yyyy", null);

                                // Set the Updated by and Updated on properties
                                type.UpdatedById = LoggedUserId;
                                type.UpdatedOnUtc = GetDateTime;
                                updateList.Add(type);
                            }
                            else if (item.Flag == "New")
                            {
                                // If no RequirementChangeLog is found with the given ID, continue
                                if (type != null)
                                    continue;

                                var data = _mapper.Map<RequirementChangeLog>(item);
                                data.RequirementId = entity.Id;
                                data.EmployeeId = item.EmployeeId;
                                data.RequirementName = item.RequirementName;

                                if (!string.IsNullOrEmpty(item.Description))
                                {
                                    data.Description = await _azureBlobImageServices
                                        .ProcessHtmlAndManageImagesAsync(
                                            item.Description,
                                            SiteData.Name,
                                            "requirements",
                                            entity.RequirementNumber.ToString()
                                        );
                                }

                                if (item.RequirementLogDateStr != "" && item.RequirementLogDateStr != null)
                                    data.RequirementLogDate = DateTime.ParseExact(item.RequirementLogDateStr, "MM/dd/yyyy", null);

                                // Set the created by and created on properties
                                data.CreatedById = LoggedUserId;
                                data.UpdatedById = LoggedUserId;
                                data.CreatedOnUtc = GetDateTime;
                                data.UpdatedOnUtc = GetDateTime;
                                addList.Add(data);
                            }
                            else if (item.Flag == "Delete")
                            {
                                // If no RequirementChangeLog is found with the given ID, continue
                                if (type == null)
                                    continue;

                                deleteList.Add(type);
                            }
                        }

                        if (addList.Count > 0)
                            _requirementChangeLogService.InsertRequirementChangeLogList(addList);

                        if (updateList.Count > 0)
                            _requirementChangeLogService.UpdateRequirementChangeLogList(updateList);

                        if (deleteList.Count > 0)
                            _requirementChangeLogService.DeleteRequirementChangeLogList(deleteList);
                    }
                    return Ok(entity.Id);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteRequirement
        // Title: DeleteRequirementById
        // Description: This endpoint deletes a test case based on the provided Requirement ID. It first retrieves the Requirement entity by ID, checks if it exists, and if so, deletes the Requirement. If the Requirement is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequirement(string id)
        {
            try
            {
                // Fetch the Requirement entity by its ID
                var entity = await _requirementService.GetRequirementById(id);
                // If no Requirement is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No test case found with the specified id."));

                // Delete the Requirement using the Requirement service
                _requirementService.DeleteRequirement(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateTags
        [HttpPost("tags")]
        public async Task<IActionResult> CreateTags(TagModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    foreach (var requirementId in model.RequirementIds)
                    {
                        var existingTagList = _requirementTagService.GetRequirementTagsByRequirementIdAndUserId(SiteId, requirementId, LoggedUserId);
                        var existingTagNames = existingTagList.Select(x => x.Tags.Name).ToList();
                        var toAdd = model.TagsNameList.Except(existingTagNames).ToList();

                        var toRemove = existingTagList.Where(x => !model.TagsNameList.Contains(x.Tags.Name)).ToList();

                        if(toRemove != null)
                        {
                            foreach (var mapping in toRemove)
                            {
                                if (mapping != null)
                                {
                                    var existingRequirementTag = await _requirementTagService.GetByNameRequirementIdAndUserId(SiteId, mapping.Tags.Name, requirementId, LoggedUserId);
                                    if (existingRequirementTag != null)
                                        _requirementTagService.DeleteRequirementTags(existingRequirementTag);
                                }
                            }
                        }

                        if (toAdd.Count > 0)
                        {
                            foreach (var tag in toAdd)
                            {
                                if (string.IsNullOrWhiteSpace(tag)) continue;

                                var existingTag = await _tagService.GetTagByName(SiteId, tag);

                                if (existingTag == null)
                                {
                                    existingTag = new Tags
                                    {
                                        Name = tag,
                                        SiteId = SiteId,
                                        Color = model.Color,
                                        CreatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime,
                                        UpdatedById = LoggedUserId,
                                        UpdatedOnUtc = GetDateTime
                                    };

                                    _tagService.InsertTags(existingTag);
                                }

                                var existingRequirement = await _requirementTagService.GetByNameRequirementIdAndUserId(SiteId, tag, requirementId, LoggedUserId);
                                if (existingRequirement != null)
                                    continue;

                                var requirementTags = new RequirementTags
                                {
                                    TagId = existingTag.Id,
                                    RequirementId = requirementId,
                                    AspNetUserId = LoggedUserId,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };

                                _requirementTagService.InsertRequirementTags(requirementTags);
                            }
                        }
                    }
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

        #region UpdateRequirementStatus
        [HttpPost("updateRequirementStatus")]
        public async Task<IActionResult> UpdateRequirementStatus(RequirementModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.RequirementIds != null && model.RequirementIds.Count > 0)
                    {
                        foreach (var requirementId in model.RequirementIds)
                        {
                            var entity = await _requirementService.GetRequirementById(requirementId);
                            if (entity != null)
                            {
                                bool result = await UpdateRequirementDetails(requirementId, model.StatusId, "status");

                                var requirementDetails = await _requirementService.GetRequirementDetailsById(requirementId);

                                var Status = await _dropDownService.GetDropDownById(model.StatusId);
                                _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Requirement", entity.ProjectId, requirementDetails.Project.Name, entity.Id, entity.Title, "Requirement Status", Status.DropDownValue, LoggedUserId, GetDateTime);
                            }
                        }
                    }

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

        #region UpdateRequirementPriority
        [HttpPost("updateRequirementPriority")]
        public async Task<IActionResult> UpdateRequirementPriority(RequirementModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.RequirementIds != null && model.RequirementIds.Count > 0)
                    {
                        foreach (var requirementId in model.RequirementIds)
                        {
                            //var entity = await _requirementService.GetRequirementById(requirementId);
                            var entity = await _requirementService.GetRequirementDetailsById(requirementId);
                            if (entity != null)
                            {
                                bool result = await UpdateRequirementDetails(requirementId, model.PriorityId, "priority");

                                var Priority = await _dropDownService.GetDropDownById(model.PriorityId);
                                _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Requirement", entity.ProjectId, entity.Project.Name, entity.Id, entity.Title, "Requirement Priority", Priority.DropDownValue, LoggedUserId, GetDateTime);
                            }
                        }
                    }

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

        #region UpdateRequirementIsPinned
        [HttpPut("pinstatus/{id}/{pinstatus}")]
        public async Task<IActionResult> UpdateRequirementIsPinned(string id, bool pinstatus)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();

                    var entity = await _requirementService.GetRequirementById(id);
                    // If no Requirement is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Requirement found with the specified id."));

                     var existsRequirementPin = await _requirementsPinnedService.GetRequirementPinnedByUser(id, LoggedUserId);
                     if (existsRequirementPin != null)
                     {
                         // Update existing record
                          existsRequirementPin.IsPinned = pinstatus;
                          _requirementsPinnedService.UpdateRequirementPin(existsRequirementPin);
                     }
                     else
                     {
                         // Insert new record
                         var requirementPin = new RequirementPinned
                         {
                             RequirementId = id,
                             AspNetUserId = LoggedUserId,
                             IsPinned = pinstatus
                         };
                         _requirementsPinnedService.InsertRequirementPin(requirementPin);
                     }

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

        #region UpdateRequirementColor
        [HttpPut("requirementColor/{id}/{requirementColor}")]
        public async Task<IActionResult> UpdateRequirementColor(string id, string requirementColor)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _requirementService.GetRequirementById(id);
                    // If no Requirement is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Requirement found with the specified id."));

                    if (!string.IsNullOrEmpty(requirementColor))
                    {

                        // Check if RequirementColor record exists for this requirement & user
                        var existsRequirementColor = await _requirementsColorService.GetRequirementColorByUser(id, LoggedUserId);

                        if (existsRequirementColor != null)
                        {
                            // Update existing record
                            existsRequirementColor.Color = requirementColor;
                            existsRequirementColor.UpdatedById = LoggedUserId;
                            existsRequirementColor.UpdatedOnUtc = GetDateTime;

                            _requirementsColorService.UpdateRequirementColor(existsRequirementColor);
                        }
                        else
                        {
                            // Insert new record
                            var requirementColorEntity = new RequirementColor
                            {
                                RequirementId = id,
                                AspNetUserId = LoggedUserId,
                                Color = requirementColor,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime,
                                UpdatedById = LoggedUserId,
                                UpdatedOnUtc = GetDateTime,
                            };

                            _requirementsColorService.InsertRequirementColor(requirementColorEntity);
                        }
                    }

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

        #region private functions for requirement
        private async Task<bool> UpdateRequirementDetails(string requirementId, object data, string flag)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            var entity = await _requirementService.GetRequirementById(requirementId);
            if (entity == null)
                return false;

            switch (flag.ToLower())
            {
                case "status":
                    if (data is string statusId)
                        entity.StatusId = statusId;
                    break;

                case "priority":
                    if (data is string priorityId)
                        entity.PriorityId = priorityId;
                    break;

                default:
                    return false;
            }

            entity.UpdatedById = LoggedUserId;
            entity.UpdatedOnUtc = GetDateTime;
            _requirementService.UpdateRequirement(entity);

            return true;
        }
        #endregion
    }
}