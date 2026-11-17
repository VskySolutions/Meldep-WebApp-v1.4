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
using Vsky.Api.Services.TestPlans;
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
    [RequireMenu(MenuLinks.TestPlan + "|" + MenuLinks.TestCase)]
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
        private readonly ITestPlanImportService _testPlanImportService;
        #endregion

        #region Services Initializations      
        public TestPlanController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ITestPlanService testPlanService,
            ISiteService siteService,
            ApplicationDbContext db,
            ICommonService commonService,
            IAzureBlobImageServices azureBlobImageServices,
            ITestPlanImportService testPlanImportService
            )
        {
            _testPlanImportService = testPlanImportService;
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
        [ReadAccess]
        public async Task<IActionResult> GetAllTestPlans(TestPlanSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                //var employeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                // Fetch a list of test plans on search criteria (name, sorting, pagination)
                var list = await _testPlanService.GetAllTestPlans(
                    SiteId,
                    LoggedUserId,
                    employeeId,
                    searchModel.SearchText,
                    searchModel.TestPlanNumber,
                    searchModel.ProjectIds,
                    searchModel.Name,
                    searchModel.PlanMakerIds,
                    searchModel.PlanReviewerIds,
                    searchModel.SortBy,
                    searchModel.Sorts,
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
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetAllTestPlansListForDropdown
        // Title: GetAllTestPlansListForDropdown
        // Description: This endpoint retrieves the details of a specific test plan based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
        [SiteMember]
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
        [RecordScope(typeof(Vsky.Models.TestPlan))]
        [SiteOwned(typeof(Vsky.Models.TestPlan))]
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
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetTestPlanDetailsById
        // Title: GetTestPlanDetailsById
        // Description: This endpoint retrieves the details of a specific test plan based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        [RecordScope(typeof(Vsky.Models.TestPlan))]
        [SiteOwned(typeof(Vsky.Models.TestPlan))]
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
                return BadRequest(SafeErrorMessage(ex));
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

                    var entity = await CreateTestPlanEntity(model, LoggedUserId, SiteId, SiteData, GetDateTime);

                    return Ok(entity);
                }
                // Return model state errors if the model state is not valid
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region UpdateTestPlan
        // Title: UpdateTestPlan
        // Description: This endpoint updates an existing test plan by its ID. 
        [HttpPut("{id}")]
        [RecordScope(typeof(Vsky.Models.TestPlan))]
        [SiteOwned(typeof(Vsky.Models.TestPlan))]
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
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region DeleteTestPlan
        // Title: DeleteTestPlanById
        // Description: This endpoint deletes a test plan based on the provided testplan ID. It first retrieves the testplan entity by ID, checks if it exists, and if so, deletes the testplan. If the testplan is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        [RecordScope(typeof(Vsky.Models.TestPlan))]
        [SiteOwned(typeof(Vsky.Models.TestPlan))]
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
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region CreateTestPlanEntity
        // Title: CreateTestPlanEntity
        // Description: Shared manual-creation logic (mapping, plan numbering, description image processing,
        // site ownership and audit stamping, insert). Used by CreateTestPlan and by Excel import confirmation
        // so imported plans are persisted exactly like manually created ones.
        private async Task<TestPlan> CreateTestPlanEntity(TestPlanModel model, string LoggedUserId, string SiteId, Site SiteData, DateTime GetDateTime)
        {
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

            return entity;
        }
        #endregion

        #region DownloadImportTemplate
        // Title: DownloadImportTemplate
        // Description: Generates the Test Plan Excel import template (editable form fields only) with
        // ID/label guidance sheets scoped to the current site and user. No writes.
        [HttpGet("import/template")]
        public async Task<IActionResult> DownloadImportTemplate()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var bytes = await _testPlanImportService.BuildTemplateAsync(SiteId, LoggedUserId);
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TestPlanImportTemplate.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region PreviewImport
        // Title: PreviewImport
        // Description: Parses an uploaded workbook in memory and returns canonical rows, resolved labels,
        // row/field errors and counts. Never inserts, updates, allocates numbers or processes images.
        [HttpPost("import/preview")]
        [RequestSizeLimit(TestPlanImportField.MaxFileSizeBytes + 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = TestPlanImportField.MaxFileSizeBytes + 1024 * 1024)]
        public async Task<IActionResult> PreviewImport([FromForm] TestPlanImportUploadModel model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var parsed = _testPlanImportService.ParseWorkbook(model?.File);
                if (parsed.FileErrors.Any())
                {
                    return Ok(new TestPlanImportPreviewModel { FileErrors = parsed.FileErrors, CanConfirm = false });
                }

                var preview = await _testPlanImportService.ValidateAsync(parsed.Rows, SiteId, LoggedUserId, parsed.CellErrors);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region ConfirmImport
        // Title: ConfirmImport
        // Description: Revalidates the whole submitted batch against the current site/user context (client
        // validity flags are ignored). Any invalid row rejects the batch with no writes. A valid batch is
        // created through CreateTestPlanEntity inside one database transaction; on any failure the
        // transaction is rolled back and no imported plans are reported.
        [HttpPost("import/confirm")]
        public async Task<IActionResult> ConfirmImport(TestPlanImportConfirmModel model)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            TestPlanImportPreviewModel validation;
            try
            {
                validation = await _testPlanImportService.ValidateAsync(model?.Rows, SiteId, LoggedUserId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (!validation.CanConfirm)
            {
                return BadRequest(new TestPlanImportResultModel
                {
                    Success = false,
                    ImportedCount = 0,
                    Message = "Import failed. No test plans were imported. Please correct the errors and try again.",
                    Validation = validation
                });
            }

            var created = new List<TestPlanImportCreatedItem>();
            try
            {
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                await using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    foreach (var row in validation.Rows.OrderBy(r => r.RowNumber))
                    {
                        // Only allowlisted, revalidated fields; site/number/audit are server-owned.
                        var planModel = new TestPlanModel
                        {
                            ProjectId = row.ProjectId,
                            Name = row.Name,
                            AreaId = row.AreaId,
                            WorkspaceId = row.WorkspaceId,
                            PlanMakerId = row.PlanMakerId,
                            PlanReviewerId = row.PlanReviewerId,
                            Description = _testPlanImportService.ToDescriptionHtml(row.Description)
                        };

                        var entity = await CreateTestPlanEntity(planModel, LoggedUserId, SiteId, SiteData, GetDateTime);
                        created.Add(new TestPlanImportCreatedItem
                        {
                            RowNumber = row.RowNumber,
                            Id = entity.Id,
                            TestPlanNumber = entity.TestPlanNumber,
                            Name = entity.Name
                        });
                    }

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new TestPlanImportResultModel
                {
                    Success = false,
                    ImportedCount = 0,
                    Message = $"Import failed. No test plans were imported. {ex.Message}",
                    Validation = validation
                });
            }

            return Ok(new TestPlanImportResultModel
            {
                Success = true,
                ImportedCount = created.Count,
                Message = $"{created.Count} test plan(s) imported successfully.",
                Created = created,
                Validation = validation
            });
        }
        #endregion
    }
}