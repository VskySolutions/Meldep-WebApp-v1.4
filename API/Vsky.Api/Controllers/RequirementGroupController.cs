using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.EmployeeClientLocations;
using Vsky.Services.EmployeeDepartments;
using Vsky.Services.EmployeeDesignations;
using Vsky.Services.EmployeeOrgLocations;
using Vsky.Services.Employees;
using Vsky.Services.EmployeeStatuses;
using Vsky.Services.EmployeeTypes;
using Vsky.Services.Persons;
using Vsky.Services.ProjectTasks;
using Vsky.Services.Requirements;
using Vsky.Services.Sites;
using Vsky.Services.TestPlans;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Vsky.Api.Models.EmployeeModel;

namespace Vsky.Api.Controllers
{
    [Route("requirement-group")]
    public class RequirementGroupController : BaseController
    {

        #region Define Services   
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IRequirementGroupService _requirementGroupService;
        private readonly ISiteService _siteService;
        private readonly ApplicationDbContext _db;
        private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations      
        public RequirementGroupController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            IRequirementGroupService requirementGroupService, 
            ISiteService siteService, 
            ApplicationDbContext db, 
            ICommonService commonService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _requirementGroupService = requirementGroupService;
            _siteService = siteService;
            _db = db;
            _commonService = commonService;
        }
        #endregion

        #region GetAllRequirementGroups
        // Title: Get All RequirementGroups
        // Description: This endpoint fetches a list of RequirementGroups based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllRequirementGroups(RequirementGroupSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of RequirementGroups on search criteria (name, sorting, pagination)
                var list = await _requirementGroupService.GetAllRequirementGroups(SiteId, LoggedUserId, searchModel.SearchText, searchModel.RequirementGroupNumber, searchModel.ProjectIds, searchModel.Name, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new RequirementGroupListModel
                {
                    Data = _mapper.Map<IList<RequirementGroupModel>>(list),
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

        #region GetAllRequirementGroupsListForDropdown
        // Title: GetAllRequirementGroupsListForDropdown
        // Description: This endpoint retrieves the details of a specific RequirementGroup based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllRequirementGroupsListForDropdown(string ProjectId = null)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _requirementGroupService.GetAllRequirementGroupsListForDropdown(SiteId, ProjectId);
            var model = _mapper.Map<List<CommonDropDown>>(list);
            return Ok(model);
        }
        #endregion

        #region GetRequirementGroupById
        //Title: GetRequirementGroupById
        //Description: This endpoint retrieves the details of a specific RequirementGroup based on its unique identifier(ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequirementGroupById(string id)
        {
            try
            {
                // Fetch the RequirementGroup entity by its ID from the service
                var entity = await _requirementGroupService.GetRequirementGroupById(id);
                // If the RequirementGroup entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Requirement Group found with the specified id."));

                // Map the RequirementGroup entity to a RequirementGroupModel object
                var model = _mapper.Map<RequirementGroupModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetRequirementGroupDetailsById
        // Title: GetRequirementGroupDetailsById
        // Description: This endpoint retrieves the details of a specific Requirement Group based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetRequirementGroupDetailsById(string id)
        {
            try
            {
                // Fetch the Requirement Group entity by its ID from the service
                var entity = await _requirementGroupService.GetRequirementGroupDetailsById(id);
                // If the Requirement Group entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Requirement Group found with the specified id."));

                // Map the Requirement Group entity to a RequirementGroupModel object
                var model = _mapper.Map<RequirementGroupModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateRequirementGroup
        // Title: CreateRequirementGroup
        // Description: This endpoint handles the creation of a new Requirement Group. It maps the Requirement Group model to the Requirement Group entity, sets the creation details, and inserts the Requirement Group into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateRequirementGroup(RequirementGroupModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _requirementGroupService.GetRequirementGroupByName(model.Name, model.ProjectId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("The Requirement Group already exists"));

                    var record = _db.RequirementGroup.OrderByDescending(m => m.RequirementGroupNumber).FirstOrDefault();
                    // Map the RequirementGroup model to the RequirementGroup entity
                    var entity = _mapper.Map<RequirementGroup>(model);
                    entity.RequirementGroupNumber = record != null ? record.RequirementGroupNumber + 1 : 1;

                    // Set the created by and created on properties
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _requirementGroupService.InsertRequirementGroup(entity);

                    return Ok(entity);
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

        #region UpdateRequirementGroup
        // Title: UpdateRequirementGroup
        // Description: This endpoint updates an existing Requirement Group by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequirementGroup(string id, RequirementGroupModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the Requirement Group entity by its ID
                    var entity = await _requirementGroupService.GetRequirementGroupById(id);
                    // If no Requirement Group is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Requirement Group found with the specified id."));

                    var exists = await _requirementGroupService.GetRequirementGroupByName(model.Name, model.ProjectId, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("The Requirement Group already exists"));

                    entity.ProjectId = model.ProjectId;
                    entity.Name = model.Name;
                    entity.Description = model.Description;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _requirementGroupService.UpdateRequirementGroup(entity);

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

        #region DeleteRequirementGroup
        // Title: DeleteRequirementGroupById
        // Description: This endpoint deletes a Requirement Group based on the provided RequirementGroup ID. It first retrieves the RequirementGroup entity by ID, checks if it exists, and if so, deletes the RequirementGroup. If the RequirementGroup is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequirementGroup(string id)
        {
            try
            {
                // Fetch the RequirementGroup entity by its ID
                var entity = await _requirementGroupService.GetRequirementGroupById(id);
                // If no RequirementGroup is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No RequirementGroup found with the specified id."));

                // Delete the RequirementGroup using the RequirementGroup service
                _requirementGroupService.DeleteRequirementGroup(entity);

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