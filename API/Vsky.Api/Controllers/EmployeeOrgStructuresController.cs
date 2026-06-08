using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.Departments;
using Vsky.Services.EmployeeOrgStructureDesignationMappings;
using Vsky.Services.EmployeeOrgStructures;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("employee-org-structure")]
    public class EmployeeOrgStructuresController : BaseController
    {

        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IEmployeeOrgStructureService _employeeOrgStructureService;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly IEmployeeOrgStructureDesignationMappingService _employeeOrgStructureDesignationMappingService;
        #endregion

        #region Services Initializations
        public EmployeeOrgStructuresController(
            GlobalVariable globalVariable,
            IMapper mapper,
            IEmployeeOrgStructureService employeeOrgStructureService,
            ISiteService siteService,
            ICommonService commonService,
            IAzureBlobImageServices azureBlobImageServices,
            IEmployeeOrgStructureDesignationMappingService employeeOrgStructureDesignationMappingService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _employeeOrgStructureService = employeeOrgStructureService;
            _siteService = siteService;
            _commonService = commonService;
            _azureBlobImageServices = azureBlobImageServices;
            _employeeOrgStructureDesignationMappingService = employeeOrgStructureDesignationMappingService;
        }
        #endregion

        #region GetAllEmployeeOrgStructures
        // Title: Get All Employee Org Structure
        // Description: This endpoint fetches a list of employee org structure based on the provided search criteria such as sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllEmployeeOrgStructureList(EmployeeOrgStructureSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of employee org structure based on search criteria
                var list = _employeeOrgStructureService.GetAllEmployeeOrgStructureList(
                    SiteId,
                    searchModel.SearchText,
                    searchModel.Years,
                    searchModel.Level,
                    searchModel.DepartmentIds,
                    searchModel.EmployeeDesignationIds,
                    searchModel.ManagerIds,
                    searchModel.EmployeeIds,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                 );
                // Map the fetched list to a model suitable for the response
                var model = new EmployeeOrgStructureListModel
                {
                    Data = _mapper.Map<IList<EmployeeOrgStructureModel>>(list),
                    Total = list.TotalCount
                };
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("preview")]
        public IActionResult GetOrgStructurePreview(int year)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var previewList = _employeeOrgStructureService.GetOrgStructurePreview(SiteId, year);

                return Ok(previewList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region GetEmployeeOrgStructureDetailsById
        // Title: GetEmployeeOrgStructureDetailsById
        // Description: This endpoint retrieves the details of a specific employee org structure based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeOrgStructureDetailsById(string id)
        {
            try
            {
                // Fetch the employee org structure entity by its ID from the service
                var entity = await _employeeOrgStructureService.GetEmployeeOrgStructureDetailsById(id);

                // If the employee org structure entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No employee org structure found with the specified id."));

                // Map the employee org structure entity to a EmployeeOrgStructureModel object
                var model = _mapper.Map<EmployeeOrgStructureModel>(entity);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateEmployeeOrgStructure
        // Title: CreatEemployeeOrgStructure
        // Description: This endpoint handles the creation of a new employee org structureService. it maps the employee org structureService model to the employee org structure entity, sets the creation details, and inserts the employee org structureService into the database.
        [HttpPost]
        public async Task<IActionResult> CreateEmployeeOrgStructure(EmployeeOrgStructureModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    
                    if (model.Level == 1 && string.IsNullOrEmpty(model.ManagerId))
                    {
                        var levelExists = await _employeeOrgStructureService.GetEmployeeOrgStructureByYearAndLevel(SiteId, model.Year, model.Level);
                        if (levelExists != null)
                            return BadRequest(new BadRequestError("Level 1 already exists for the selected year, try with another."));
                    }

                    var exists = await _employeeOrgStructureService.GetEmployeeOrgStructureByManagerAndEmployee(SiteId, model.ManagerId, model.EmployeeId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Employee org structure already exists, try with another."));
                   

                    // Map the eemployee org structure model to the employee org structure entity
                    var entity = _mapper.Map<EmployeeOrgStructure>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    if (!string.IsNullOrEmpty(model.Responsibilities))
                    {
                        entity.Responsibilities = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Responsibilities,
                                SiteData.Name,
                                "employee-org-structure",
                                entity.Id
                            );
                    }

                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _employeeOrgStructureService.InsertEmployeeOrgStructure(entity);

                    if (model.EmployeeDesignationIdsArray != null && model.EmployeeDesignationIdsArray.Count() > 0)
                    {
                        foreach (var item in model.EmployeeDesignationIdsArray)
                        {
                            if (item != null)
                            {
                                var entityTrainingMapping = new EmployeeOrgStructureDesignationMapping
                                {
                                    EmployeeOrgStructureId = entity.Id,
                                    EmployeeDesignationId = item,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };
                                _employeeOrgStructureDesignationMappingService.InsertEmployeeOrgStructureDesignations(entityTrainingMapping);
                            }
                        }
                    }

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

        #region UpdateEmployeeOrgStructure
        // Title: UpdateDepartment
        // Description: This endpoint updates an existing employee org structure by its ID. It validates the employee org structure model, checks for duplicate employee org structure names, updates the employee org structure's details.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployeeOrgStructure(string id, EmployeeOrgStructureModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the department entity by its ID
                    var entity = await _employeeOrgStructureService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No employee org structure found with the specified id."));

                    if (model.Level == 1)
                    {
                        var levelExists = await _employeeOrgStructureService.GetEmployeeOrgStructureByYearAndLevel(SiteId, model.Year, model.Level, id);
                        if (levelExists != null)
                            return BadRequest(new BadRequestError("Level 1 already exists for the selected year, try with another."));
                    }

                    var exists = await _employeeOrgStructureService.GetEmployeeOrgStructureByManagerAndEmployee(SiteId, model.ManagerId, model.EmployeeId, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Employee org structure already exists, try with another."));

                    entity.ManagerId = model.ManagerId;
                    entity.EmployeeId = model.EmployeeId;
                    entity.DepartmentId = model.DepartmentId;
                    entity.RoleId = model.RoleId;
                    entity.Year = model.Year;
                    entity.Level = model.Level;
                   
                    entity.SortOrder = model.SortOrder;
                    entity.Color = model.Color;

                    if (!string.IsNullOrEmpty(model.Responsibilities))
                    {
                        entity.Responsibilities = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Responsibilities,
                                SiteData.Name,
                                "employee-org-structure",
                                entity.Id,
                                entity.Responsibilities
                            );
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _employeeOrgStructureService.UpdateEmployeeOrgStructure(entity);

                    var existingMappings = _employeeOrgStructureDesignationMappingService.GetEmployeeOrgStructureByEmployeeOrgStructureId(SiteId, entity.Id);

                    var existingIds = existingMappings
                        .Select(x => x.EmployeeDesignationId)
                        .ToList();

                    // Normalize incoming IDs
                    var newIds = model.EmployeeDesignationIdsArray?
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()
                        .ToList()
                        ?? new List<string>();

                    //Delete all if newIds are empty
                    if (!newIds.Any())
                    {
                        foreach (var mapping in existingMappings)
                        {
                            _employeeOrgStructureDesignationMappingService.DeleteEmployeeOrgStructureDesignations(mapping);
                        }

                    }

                    // add
                    foreach (var existingId in newIds.Except(existingIds))
                    {
                        var entityTrainingMapping = new EmployeeOrgStructureDesignationMapping
                        {
                            EmployeeOrgStructureId = entity.Id,
                            EmployeeDesignationId = existingId,
                            CreatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime
                        };

                        _employeeOrgStructureDesignationMappingService.InsertEmployeeOrgStructureDesignations(entityTrainingMapping);
                    }

                    // remove deselected mappings
                    if (newIds.Any())
                    {
                        foreach (var mapping in existingMappings.Where(x => !newIds.Contains(x.EmployeeDesignationId)))
                        {
                            _employeeOrgStructureDesignationMappingService.DeleteEmployeeOrgStructureDesignations(mapping);
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

        #region DeleteEmployeeOrgStructure
        // Title: DeleteDepartment
        // Description: This endpoint deletes a department based on the provided department ID. It first retrieves the department entity by ID, checks if it exists, and if so, deletes the department. If the department is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeeOrgStructure(string id)
        {
            try
            {
                // Fetch the department entity by its ID
                var entity = await _employeeOrgStructureService.GetById(id);
                // If no department is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No employee org structure found with the specified id."));

                // Delete the department using the department service
                _employeeOrgStructureService.DeleteEmployeeOrgStructure(entity);
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