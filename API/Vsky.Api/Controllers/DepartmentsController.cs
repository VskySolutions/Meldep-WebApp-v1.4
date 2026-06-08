using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.Departments;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("departments")]
    public class DepartmentsController : BaseController
    {

        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IDepartmentService _departmentService;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations
        public DepartmentsController(
            IMapper mapper,
            GlobalVariable globalVariable,
            IDepartmentService departmentService, 
            ISiteService siteService, 
            ICommonService commonService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _departmentService = departmentService;
            _siteService = siteService;
            _commonService = commonService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllDepartments
        // Title: Get All Departments
        // Description: This endpoint fetches a list of departments based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllDepartments(DepartmentSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of departments based on search criteria
                var list = _departmentService.GetAllDepartments(SiteId, searchModel.SearchText, searchModel.DepartmentIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new DepartmentListModel
                {
                    Data = _mapper.Map<IList<DepartmentModel>>(list),
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

        #region GetAllDepartmentListForDropdown
        // Title: GetAllDepartmentListForDropdown
        // Description: This endpoint retrieves the details of a specific department based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllDepartmentListForDropdown() 
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _departmentService.GetAllDepartmentListForDropdown(SiteId);
            var model = _mapper.Map<List<DepartmentModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetDepartmentById
        // Title: GetDepartmentById
        // Description: This endpoint retrieves the details of a specific department based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(string id)
        {
            try
            {
                // Fetch the department entity by its ID from the service
                var entity = await _departmentService.GetDepartmentDetailsById(id);
                // If the department entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No department found with the specified id."));

                // Map the person entity to a DepartmentModel object
                var model = _mapper.Map<DepartmentModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateDepartment
        // Title: CreateDepartment
        // Description: This endpoint handles the creation of a new department. It first checks if a department with the same name already exists If not, it maps the department model to the department entity, sets the creation details, and inserts the department into the database.
        [HttpPost]
        public async Task<IActionResult> CreateDepartment(DepartmentModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the department already exists
                    var exists = await _departmentService.GetDepartmentByName(SiteId, model.Name);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Department name already exists, try with another."));

                    // Map the department model to the department entity
                    var entity = _mapper.Map<Department>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "departments",
                                entity.Id
                            );
                    }

                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _departmentService.InsertDepartment(entity);

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

        #region UpdateDepartment
        // Title: UpdateDepartment
        // Description: This endpoint updates an existing department by its ID. It validates the department model, checks for duplicate department names, updates the department's details.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(string id, DepartmentModel model)
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
                    var entity = await _departmentService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No department found with the specified id."));

                    entity.Name = model.Name;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "departments",
                                entity.Id,
                                entity.Description
                            );
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _departmentService.UpdateDepartment(entity);

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

        #region DeleteDepartment
        // Title: DeleteDepartment
        // Description: This endpoint deletes a department based on the provided department ID. It first retrieves the department entity by ID, checks if it exists, and if so, deletes the department. If the department is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(string id)
        {
            try
            {
                // Fetch the department entity by its ID
                var entity = await _departmentService.GetById(id);
                // If no department is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No department found with the specified id."));

                // Delete the department using the department service
                _departmentService.DeleteDepartment(entity);
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