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
using Vsky.Services.AzureBlobImage;
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
using Vsky.Services.Sites;
using Vsky.Services.TestPlans;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Vsky.Api.Models.EmployeeModel;

namespace Vsky.Api.Controllers
{
    [Route("test-plan")]
    public class TestPlanController : BaseController
    {
        #region Define Services      
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ITestPlanService _testPlanService;
        private readonly ISiteService _siteService;
        private readonly ApplicationDbContext _db;
        private readonly ICommonService _commonService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations      
        public TestPlanController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ITestPlanService testPlanService,
            ISiteService siteService,
            ApplicationDbContext db,
            ICommonService commonService,
            IAzureBlobImageServices azureBlobImageServices
            )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _testPlanService = testPlanService;
            _siteService = siteService;
            _db = db;
            _commonService = commonService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllTestPlans
        // Title: Get All TestPlans
        // Description: This endpoint fetches a list of test plans based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllTestPlans(TestPlanSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of test plans on search criteria (name, sorting, pagination)
                var list = await _testPlanService.GetAllTestPlans(
                    SiteId,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.TestPlanNumber,
                    searchModel.ProjectIds,
                    searchModel.Name,
                    searchModel.PlanMakerIds,
                    searchModel.PlanReviewerIds,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                    );

                // Map the fetched list to a model suitable for the response
                var model = new TestPlanListModel
                {
                    Data = _mapper.Map<IList<TestPlanModel>>(list),
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

        #region GetAllTestPlansListForDropdown
        // Title: GetAllTestPlansListForDropdown
        // Description: This endpoint retrieves the details of a specific test plan based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllTestPlansListForDropdown(string ProjectId = null)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _testPlanService.GetAllTestPlansListForDropdown(SiteId, ProjectId);
            var model = _mapper.Map<List<CommonDropDown>>(list);
            return Ok(model);
        }
        #endregion

        #region GetTestPlanById
        //Title: GetTestPlanById
        //Description: This endpoint retrieves the details of a specific test plan based on its unique identifier(ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTestPlanById(string id)
        {
            try
            {
                // Fetch the test plan entity by its ID from the service
                var entity = await _testPlanService.GetTestPlanById(id);
                // If the test plan entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No test plan found with the specified id."));

                // Map the test plan entity to a TestPlanModel object
                var model = _mapper.Map<TestPlanModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetTestPlanDetailsById
        // Title: GetTestPlanDetailsById
        // Description: This endpoint retrieves the details of a specific test plan based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetTestPlanDetailsById(string id)
        {
            try
            {
                // Fetch the test plan entity by its ID from the service
                var entity = await _testPlanService.GetTestPlanDetailsById(id);
                // If the test plan entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No test plan found with the specified id."));

                // Map the test plan entity to a TestPlanModel object
                var model = _mapper.Map<TestPlanModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateTestPlan
        // Title: CreateTestPlan
        // Description: This endpoint handles the creation of a new test plan. It maps the test plan model to the test plan entity, sets the creation details, and inserts the test plan into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateTestPlan(TestPlanModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _testPlanService.GetTestPlanByName(SiteId, model.Name, model.ProjectId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("The test plan already exists"));

                    // Map the TestPlan model to the TestPlan entity
                    var entity = _mapper.Map<TestPlan>(model);
                    entity.TestPlanNumber = await _testPlanService.GetLastTestPlanNumber() + 1;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "test-plan",
                                entity.TestPlanNumber.ToString()
                            );
                    }

                    // Set the created by and created on properties
                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _testPlanService.InsertTestPlan(entity);

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

        #region UpdateTestPlan
        // Title: UpdateTestPlan
        // Description: This endpoint updates an existing test plan by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTestPlan(string id, TestPlanModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the test plan entity by its ID
                    var entity = await _testPlanService.GetTestPlanById(id);
                    // If no test plan is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No test plan found with the specified id."));

                    var exists = await _testPlanService.GetTestPlanByName(SiteId, model.Name, model.ProjectId, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("The test plan already exists"));

                    entity.ProjectId = model.ProjectId;
                    entity.Name = model.Name;
                    entity.AreaId = model.AreaId;
                    entity.WorkspaceId = model.WorkspaceId;
                    entity.PlanMakerId = model.PlanMakerId;
                    entity.PlanReviewerId = model.PlanReviewerId;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "test-plan",
                                entity.TestPlanNumber.ToString(),
                                entity.Description
                            );
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _testPlanService.UpdateTestPlan(entity);

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

        #region DeleteTestPlan
        // Title: DeleteTestPlanById
        // Description: This endpoint deletes a test plan based on the provided testplan ID. It first retrieves the testplan entity by ID, checks if it exists, and if so, deletes the testplan. If the testplan is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestPlan(string id)
        {
            try
            {
                // Fetch the testplan entity by its ID
                var entity = await _testPlanService.GetTestPlanById(id);
                // If no testplan is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No testplan found with the specified id."));

                // Delete the testplan using the testplan service
                _testPlanService.DeleteTestPlan(entity);

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