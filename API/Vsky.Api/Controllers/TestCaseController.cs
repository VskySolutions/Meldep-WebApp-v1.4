using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerBI.Api.Models;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Api.Services.TestCases;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.Issues;
using Vsky.Services.ProjectReleaseTrackings;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.TestCases;

namespace Vsky.Api.Controllers
{
    [Route("test-case")]
    [RequireMenu(MenuLinks.TestCase)]
    public class TestCaseController : BaseController
    {

        #region Define Services      
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        //private readonly ITestPlanService _testPlanService;
        private readonly ITestCaseService _testCaseService;
        private readonly ICommonService _commonService;
        private readonly IIssueService _issueService;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly IProjectReleaseTrackingReqPlanTaskIssueMappingService _projectReleaseTrackingReqPlanTaskIssueMappingService;
        private readonly ITestCaseExecutionLogExecutionLogService _testCaseExecutionLogExecutionLogService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly IDropDownService _dropDownService;
        private readonly ApplicationDbContext _db;
        private readonly ITestCaseImportService _testCaseImportService;

        #endregion

        #region Services Initializations      
        public TestCaseController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ITestCaseService testCaseService,
            ICommonService commonService,
            IIssueService issueService,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices,
            IProjectReleaseTrackingReqPlanTaskIssueMappingService projectReleaseTrackingReqPlanTaskIssueMappingService,
            ITestCaseExecutionLogExecutionLogService testCaseExecutionLogExecutionLogService,
            ISitesModifiedLogsService sitesModifiedLogsService,
            IDropDownService dropDownService,
            ApplicationDbContext db,
            ITestCaseImportService testCaseImportService)
        {
            _db = db;
            _testCaseImportService = testCaseImportService;
            _globalVariable = globalVariable;
            _mapper = mapper;
            _testCaseService = testCaseService;
            _commonService = commonService;
            _issueService = issueService;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
            _projectReleaseTrackingReqPlanTaskIssueMappingService = projectReleaseTrackingReqPlanTaskIssueMappingService;
            _testCaseExecutionLogExecutionLogService = testCaseExecutionLogExecutionLogService;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _dropDownService = dropDownService;
        }
        #endregion

        #region GetAllTestCases
        // Title: Get All TestCases
        // Description: This endpoint fetches a list of test cases based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        [ReadAccess]
        public async Task<IActionResult> GetAllTestCases(TestCaseSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                //var employeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                // Fetch a list of test cases on search criteria (name, sorting, pagination)
                var list = await _testCaseService.GetAllTestCases(
                    SiteId,
                    LoggedUserId,
                    employeeId,
                    searchModel.SearchText,
                    searchModel.TestCaseNumber,
                    searchModel.ProjectIds,
                    searchModel.ProjectModuleIds,
                    searchModel.RequirementIds,
                    searchModel.PlanIds,
                    searchModel.TestedBys,
                    searchModel.StatusIds,
                    searchModel.VersionNumber,
                    searchModel.FromDate,
                    searchModel.ToDate,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                    );
                // Map the fetched list to a model suitable for the response
                var model = new TestCaseListModel
                {
                    Data = _mapper.Map<IList<TestCaseModel>>(list),
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

        #region GetAllTestCasesListForDropdown
        // Title: GetAllTestCasesListForDropdown
        // Description: This endpoint retrieves the details of a specific test case based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
        [SiteMember]
        public async Task<IActionResult> GetAllTestCasesListForDropdown()
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _testCaseService.GetAllTestCasesListForDropdown(SiteId);
            var model = _mapper.Map<List<TestCaseModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetTestCaseById
        //Title: GetTestCaseById
        //Description: This endpoint retrieves the details of a specific test case based on its unique identifier(ID). 
        [HttpGet("{id}")]
        [SiteOwned(typeof(Vsky.Models.TestCase))]
        [RecordScope(typeof(Vsky.Models.TestCase))]
        public async Task<IActionResult> GetTestCaseById(string id)
        {
            try
            {
                // Fetch the test case entity by its ID from the service
                var entity = await _testCaseService.GetTestCaseById(id);
                // If the test case entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No test case found with the specified id."));

                // Map the test case entity to a TestCaseModel object
                var model = _mapper.Map<TestCaseModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetTestCaseDetailsById
        // Title: GetTestCaseDetailsById
        // Description: This endpoint retrieves the details of a specific test case based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        [SiteOwned(typeof(Vsky.Models.TestCase))]
        [RecordScope(typeof(Vsky.Models.TestCase))]
        public async Task<IActionResult> GetTestCaseDetailsById(string id)
        {
            try
            {
                // Fetch the test case entity by its ID from the service
                var entity = await _testCaseService.GetTestCaseDetailsById(id);
                // If the test case entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No test case found with the specified id."));

                // Map the test case entity to a TestCaseModel object
                var model = _mapper.Map<TestCaseModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetReleaseWiseTestCaseHistory
        // Title: GetReleaseWiseTestCaseHistory
        // Description: This endpoint retrieves the details of a specific test case based on its unique identifier (ID). 
        [HttpGet("history/{id}")]
        [SiteOwned(typeof(Vsky.Models.TestCase))]
        [RecordScope(typeof(Vsky.Models.TestCase))]
        public async Task<IActionResult> GetReleaseWiseTestCaseHistory(string id)
        {
            try
            {
                // Fetch the test case entity by its ID from the service
                var list = await _projectReleaseTrackingReqPlanTaskIssueMappingService.GetReleaseWiseTestCaseHistory(id);
                // If the test case entity is not found, return a BadRequest response with an error message
                if (list == null)
                    return BadRequest(new BadRequestError("No test case found with the specified id."));

                // Map the test case entity to a ReleaseWiseTestCaseHistoryDto object
                var model = _mapper.Map<List<ReleaseWiseTestCaseHistoryDto>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetReleaseWiseTestCaseHistory
        [HttpPost("history-by-testCaseIds")]
        [ReadAccess]
        [RequireMenu(MenuLinks.TestCase + "|" + MenuLinks.ProjectRelease)]
        [SiteOwned(typeof(Vsky.Models.TestCase), Field = "TestCaseIds")]
        public async Task<IActionResult> GetReleaseWiseTestCaseHistoryByTestCaseIds([FromBody] ReleaseHistoryRequest request)
        {
            var result = await _projectReleaseTrackingReqPlanTaskIssueMappingService
                .GetReleaseWiseTestCaseHistoryByTestCaseIds(request.TestCaseIds,
            request.VersionNumber);

            return Ok(result);
        }
        #endregion

        #region GetStatusChangeLog
        // Title: GetStatusChangeLog
        // Description: This endpoint retrieves the list of statuses of a specific test case based on its unique identifier (ID). 
        [HttpGet("change-log")]
        public async Task<IActionResult> GetStatusChangeLog(string mappingId)
        {
            return Ok(await _testCaseService.GetStatusChangeLog(mappingId, _globalVariable.SiteId));
        }
        #endregion

        #region CreateTestCase
        // Title: CreateTestCase
        // Description: This endpoint handles the creation of a new test case. It maps the test case model to the test case entity, sets the creation details, and inserts the test case into the database. 
        [HttpPost]
        [SiteOwned(typeof(Vsky.Models.Project), Field = "ProjectId")]
        [SiteOwned(typeof(Vsky.Models.TestPlan), Field = "PlanId")]
        [SiteOwned(typeof(Vsky.Models.ProjectModule), Field = "ProjectModuleId")]
        public async Task<IActionResult> CreateTestCase(TestCaseModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var testCaseExists = await _testCaseService.GetTestCaseByName(SiteId, model.ProjectId, model.PlanId, model.ProjectModuleId, model.Name);
                    if (testCaseExists != null)
                        return BadRequest(new BadRequestError("The test case already exists"));

                    var (entity, _) = await CreateTestCaseEntity(model, LoggedUserId, SiteId, SiteData, GetDateTime);
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

        #region CreateTestCaseEntity
        // Title: CreateTestCaseEntity
        // Description: Shared manual base-case creation (mapping, tested date, numbering, rich-content image
        // processing, requirement normalization, attribution, insert) followed by the existing Fail rule: when
        // the status is Fail and no active issue matches site/project/case-insensitive name, create a
        // Bug / Medium / New from Test Plan issue linked to the saved case. Used by CreateTestCase and by Excel
        // import confirmation. Returns the created issue (null when none was created).
        private async Task<(TestCase Entity, Issue CreatedIssue)> CreateTestCaseEntity(TestCaseModel model, string LoggedUserId, string SiteId, Site SiteData, DateTime GetDateTime)
        {
            Issue createdIssue = null;
            string TestCaseId = null;
            var entity = _mapper.Map<TestCase>(model);

            if (model.TestedDateStr != "" && model.TestedDateStr != null)
                entity.TestedDate = DateTime.ParseExact(model.TestedDateStr, "MM/dd/yyyy", null);

            if (!string.IsNullOrWhiteSpace(model.EmployeeId))
            {
                entity.EmployeeId = model.EmployeeId;
            }
            entity.TestCaseNumber = await _testCaseService.GetLastTestCaseNumber() + 1;
            entity.SiteId = SiteId;

            entity.Description = await _azureBlobImageServices
                   .ProcessHtmlAndManageImagesAsync(
                       model.Description,
                       SiteData.Name,
                       "test-case",
                       entity.TestCaseNumber.ToString()
                   );

            entity.Steps = await _azureBlobImageServices
                .ProcessHtmlAndManageImagesAsync(
                    model.Steps,
                    SiteData.Name,
                    "test-case",
                    entity.TestCaseNumber.ToString()
                );

            entity.ExpectedResult = await _azureBlobImageServices
                .ProcessHtmlAndManageImagesAsync(
                    model.ExpectedResult,
                    SiteData.Name,
                    "test-case",
                    entity.TestCaseNumber.ToString()
                );

            entity.ActualResult = await _azureBlobImageServices
                .ProcessHtmlAndManageImagesAsync(
                    model.ActualResult,
                    SiteData.Name,
                    "test-case",
                    entity.TestCaseNumber.ToString()
                );


            entity.RequirementId =
                !string.IsNullOrWhiteSpace(model.RequirementId) &&
                model.RequirementId != "undefined" &&
                model.RequirementId != "null"
                    ? model.RequirementId
                    : null;

            entity.CreatedById = LoggedUserId;
            entity.UpdatedById = LoggedUserId;
            entity.CreatedOnUtc = GetDateTime;
            entity.UpdatedOnUtc = GetDateTime;
            _testCaseService.InsertTestCase(entity);

            TestCaseId = entity.Id;

            var testcasestatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Test Case Status", "Fail");

            if (model.StatusId == testcasestatus)
            {
                //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                var newstatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Status", "New from Test Plan");
                var medium = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Priority", "Medium");
                var bug = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Type", "Bug");
                var exists = await _issueService.GetIssueByName(SiteId, model.Name, model.ProjectId);
                if (exists == null)
                {
                    var issue = new Issue();
                    issue.IssueNumber = await _issueService.GetLastIssueNumber() + 1;
                    issue.SiteId = SiteId;
                    issue.TestCaseId = TestCaseId;
                    issue.ProjectId = model.ProjectId;
                    issue.ProjectModuleId = model.ProjectModuleId;

                    issue.RequirementId =
                        !string.IsNullOrWhiteSpace(model.RequirementId) &&
                        model.RequirementId != "undefined" &&
                        model.RequirementId != "null"
                            ? model.RequirementId
                            : null;
                    issue.Name = model.Name;
                    issue.PriorityId = medium;
                    issue.StatusId = newstatus;
                    issue.EmployeeId = model.EmployeeId;
                    issue.TypeId = bug;
                    issue.ReportedById = EmployeeId;
                    issue.AreaId = model.AreaId;
                    issue.WorkspaceId = model.WorkspaceId;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        issue.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "test-case",
                                issue.IssueNumber.ToString()
                            );
                    }

                    issue.CreatedById = LoggedUserId;
                    issue.UpdatedById = LoggedUserId;
                    issue.CreatedOnUtc = GetDateTime;
                    issue.UpdatedOnUtc = GetDateTime;
                    _issueService.InsertIssue(issue);
                    createdIssue = issue;
                }
            }
            return (entity, createdIssue);
        }
        #endregion

        #region DownloadImportTemplate
        // Title: DownloadImportTemplate
        // Description: Generates the Test Case Excel import template (15 editable form fields) with dropdowns
        // and option/ID guidance scoped to the current site and user. No writes.
        [HttpGet("import/template")]
        public async Task<IActionResult> DownloadImportTemplate()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var bytes = await _testCaseImportService.BuildTemplateAsync(SiteId, LoggedUserId);
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TestCaseImportTemplate.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region PreviewImport
        // Title: PreviewImport
        // Description: Parses an uploaded workbook in memory and returns canonical rows, labels, errors, counts and
        // the predicted Fail-issue outcome. Never writes cases/issues, allocates numbers or processes images.
        [HttpPost("import/preview")]
        [RequestSizeLimit(TestCaseImportField.MaxFileSizeBytes + 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = TestCaseImportField.MaxFileSizeBytes + 1024 * 1024)]
        public async Task<IActionResult> PreviewImport([FromForm] TestCaseImportUploadModel model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var parsed = _testCaseImportService.ParseWorkbook(model?.File);
                if (parsed.FileErrors.Any())
                    return Ok(new TestCaseImportPreviewModel { FileErrors = parsed.FileErrors, CanConfirm = false });

                return Ok(await _testCaseImportService.ValidateAsync(parsed.Rows, SiteId, LoggedUserId, parsed.CellErrors));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region ConfirmImport
        // Title: ConfirmImport
        // Description: Revalidates the whole submitted batch (client flags ignored). Any invalid row rejects the
        // batch with no writes. A valid batch is created through CreateTestCaseEntity (incl. the manual Fail-issue
        // rule) inside one database transaction; any failure rolls back all cases and issues and reports 0.
        [HttpPost("import/confirm")]
        public async Task<IActionResult> ConfirmImport(TestCaseImportConfirmModel model)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            TestCaseImportPreviewModel validation;
            try
            {
                validation = await _testCaseImportService.ValidateAsync(model?.Rows, SiteId, LoggedUserId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (!validation.CanConfirm)
            {
                return BadRequest(new TestCaseImportResultModel
                {
                    Success = false,
                    Message = "Import failed. No test cases were imported. Please correct the errors and try again.",
                    Validation = validation
                });
            }

            var created = new List<TestCaseImportCreatedItem>();
            try
            {
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                await using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    foreach (var row in validation.Rows.OrderBy(r => r.RowNumber))
                    {
                        // Only allowlisted, revalidated fields; site/number/audit/release data are server-owned.
                        var caseModel = new TestCaseModel
                        {
                            ProjectId = row.ProjectId,
                            PlanId = row.PlanId,
                            ProjectModuleId = row.ProjectModuleId,
                            RequirementId = row.RequirementId,
                            Name = row.Name,
                            AreaId = row.AreaId,
                            WorkspaceId = row.WorkspaceId,
                            StatusId = row.StatusId,
                            EmployeeId = row.EmployeeId,
                            TestedBy = row.TestedBy,
                            TestedDateStr = row.TestedDate,
                            Description = _testCaseImportService.ToRichTextHtml(row.Description),
                            Steps = _testCaseImportService.ToRichTextHtml(row.Steps),
                            ExpectedResult = _testCaseImportService.ToRichTextHtml(row.ExpectedResult),
                            ActualResult = _testCaseImportService.ToRichTextHtml(row.ActualResult)
                        };

                        var (entity, issue) = await CreateTestCaseEntity(caseModel, LoggedUserId, SiteId, SiteData, GetDateTime);
                        created.Add(new TestCaseImportCreatedItem
                        {
                            RowNumber = row.RowNumber,
                            Id = entity.Id,
                            TestCaseNumber = entity.TestCaseNumber,
                            Name = entity.Name,
                            IssueCreated = issue != null,
                            IssueNumber = issue?.IssueNumber
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
                return BadRequest(new TestCaseImportResultModel
                {
                    Success = false,
                    Message = $"Import failed. No test cases were imported. {ex.Message}",
                    Validation = validation
                });
            }

            var issuesCreated = created.Count(c => c.IssueCreated);
            return Ok(new TestCaseImportResultModel
            {
                Success = true,
                ImportedCount = created.Count,
                IssuesCreated = issuesCreated,
                Message = $"{created.Count} test case(s) imported successfully." + (issuesCreated > 0 ? $" {issuesCreated} issue(s) created for failed test cases." : ""),
                Created = created,
                Validation = validation
            });
        }
        #endregion

        #region UpdateTestCase
        // Title: UpdateTestCase
        // Description: This endpoint updates an existing test case by its ID. 
        [HttpPut("{id}")]
        [SiteOwned(typeof(Vsky.Models.TestCase))]
        [RecordScope(typeof(Vsky.Models.TestCase))]
        [SiteOwned(typeof(Vsky.Models.Project), Field = "ProjectId")]
        [SiteOwned(typeof(Vsky.Models.TestPlan), Field = "PlanId")]
        [SiteOwned(typeof(Vsky.Models.ProjectModule), Field = "ProjectModuleId")]
        public async Task<IActionResult> UpdateTestCase(string id, TestCaseModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the test case entity by its ID
                    var entity = await _testCaseService.GetTestCaseById(id);
                    // If no test case is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No test case found with the specified id."));

                    var testCaseExists = await _testCaseService.GetTestCaseByName(SiteId, model.ProjectId, model.PlanId, model.ProjectModuleId, model.Name, id);
                    if (testCaseExists != null)
                        return BadRequest(new BadRequestError("The test case already exists"));

                    bool hasMapping = !string.IsNullOrWhiteSpace(model.ProjectReleaseTrackingReqPlanTaskIssueMappingId) &&
                     !model.ProjectReleaseTrackingReqPlanTaskIssueMappingId.Equals("undefined", StringComparison.OrdinalIgnoreCase);

                    if (!hasMapping)
                    {
                        bool IsTestCaseStatusChanged = model.StatusId != entity.StatusId;
                        if (IsTestCaseStatusChanged)
                        {
                            var testCaseStatus = await _dropDownService.GetDropDownById(model.StatusId);
                            var status = testCaseStatus.DropDownValue;

                            _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "TestCase", entity.Id, entity.Name, entity.Id, entity.Name, "Test Case Status", status, LoggedUserId, GetDateTime);
                        }

                        entity.StatusId = model.StatusId;
                    }

                    entity.ProjectId = model.ProjectId;
                    entity.ProjectModuleId = model.ProjectModuleId;

                    entity.RequirementId =
                        !string.IsNullOrWhiteSpace(model.RequirementId) &&
                        model.RequirementId != "undefined" &&
                        model.RequirementId != "null"
                            ? model.RequirementId
                            : null;
                    entity.PlanId = model.PlanId;
                    entity.Name = model.Name;
                    entity.TestedBy = model.TestedBy;
                    entity.EmployeeId = model.EmployeeId;

                    if (model.TestedDateStr != "" && model.TestedDateStr != null)
                        entity.TestedDate = DateTime.ParseExact(model.TestedDateStr, "MM/dd/yyyy", null);

                    entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "test-case",
                                entity.TestCaseNumber.ToString(),
                                entity.Description
                    );
                    entity.Steps = await _azureBlobImageServices
                             .ProcessHtmlAndManageImagesAsync(
                                 model.Steps,
                                 SiteData.Name,
                                 "test-case",
                                 entity.TestCaseNumber.ToString(),
                                 entity.Steps
                             );
                    entity.ExpectedResult = await _azureBlobImageServices
                              .ProcessHtmlAndManageImagesAsync(
                                  model.ExpectedResult,
                                  SiteData.Name,
                                  "test-case",
                                  entity.TestCaseNumber.ToString(),
                                  entity.ExpectedResult
                              );
                    entity.ActualResult = await _azureBlobImageServices
                             .ProcessHtmlAndManageImagesAsync(
                                 model.ActualResult,
                                 SiteData.Name,
                                 "test-case",
                                 entity.TestCaseNumber.ToString(),
                                 entity.ActualResult
                             );
                    entity.AreaId = model.AreaId;
                    entity.WorkspaceId = model.WorkspaceId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _testCaseService.UpdateTestCase(entity);

                    string issueId = null;
                    var testcasestatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Test Case Status", "Fail");
                    if (model.StatusId == testcasestatus)
                    {
                        //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                        var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                        var newstatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Status", "New from Test Plan");
                        var medium = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Priority", "Medium");
                        var bug = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Type", "Bug");
                        var exists = await _issueService.GetIssueByName(SiteId, model.Name, model.ProjectId);
                        if (exists == null)
                        {
                            var issue = new Issue();
                            issue.IssueNumber = await _issueService.GetLastIssueNumber() + 1;
                            issue.SiteId = SiteId;
                            issue.TestCaseId = model.Id;
                            issue.ProjectId = model.ProjectId;
                            issue.ProjectModuleId = entity.ProjectModuleId;
                            issue.RequirementId = !string.IsNullOrWhiteSpace(entity.RequirementId) && entity.RequirementId != "undefined" && entity.RequirementId != "null" ? entity.RequirementId : null;
                            issue.Name = model.Name;
                            issue.PriorityId = medium;
                            issue.StatusId = newstatus;
                            issue.EmployeeId = model.EmployeeId;
                            issue.TypeId = bug;
                            issue.ReportedById = EmployeeId;
                            //issue.Description = issue.Description = model.Description + "<br><br>Steps:" + model.Steps + "<br><br>Expected Result:<br>" + model.ExpectedResult + "<br><br>Actual Result:<br>" + model.ActualResult;

                            if (!string.IsNullOrEmpty(model.Description))
                            {
                                var formattedDescription = issue.Description = model.Description + "<br><br>Steps:" + model.Steps + "<br><br>Expected Result:<br>" + model.ExpectedResult + "<br><br>Actual Result:<br>" + model.ActualResult;

                                issue.Description = await _azureBlobImageServices
                                    .ProcessHtmlAndManageImagesAsync(
                                        formattedDescription,
                                        SiteData.Name,
                                        "test-case",
                                        issue.IssueNumber.ToString()
                                    );
                            }

                            issue.AreaId = model.AreaId;
                            issue.WorkspaceId = model.WorkspaceId;
                            issue.CreatedById = LoggedUserId;
                            issue.UpdatedById = LoggedUserId;
                            issue.CreatedOnUtc = GetDateTime;
                            issue.UpdatedOnUtc = GetDateTime;
                            _issueService.InsertIssue(issue);

                            issueId = issue.Id;
                        }
                    }
                    if (hasMapping)
                    {
                        _testCaseExecutionLogExecutionLogService.InsertTestCaseExecutionLog(
                            new TestCaseExecutionLog
                            {
                                Id = Guid.NewGuid().ToString(),
                                ProjectReleaseTracking_ReqPlanTaskIssueMappingId = model.ProjectReleaseTrackingReqPlanTaskIssueMappingId,
                                StatusId = model.StatusId,
                                Comment = null,
                                IssueId = issueId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime,
                                UpdatedById = LoggedUserId,
                                UpdatedOnUtc = GetDateTime
                            });
                    }
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

        #region UpdateTestCaseStatus
        [HttpPut("updateTestCaseStatus/{id}/{statusId}/{mappingId}")]
        [SiteOwned(typeof(Vsky.Models.TestCase))]
        [RecordScope(typeof(Vsky.Models.TestCase))]
        public async Task<IActionResult> UpdateTestCaseStatus(string id, string statusId, string mappingId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ModelStateError(ModelState);

                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var entity = await _testCaseService.GetTestCaseById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No test case found with the specified id."));

                bool hasMapping = !string.IsNullOrWhiteSpace(mappingId) &&
                      !mappingId.Equals("undefined", StringComparison.OrdinalIgnoreCase);

                if (!hasMapping)
                {
                    bool IsTestCaseStatusChanged = statusId != entity.StatusId;

                    entity.StatusId = statusId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _testCaseService.UpdateTestCase(entity);

                    if (IsTestCaseStatusChanged)
                    {
                        var testCaseStatus = await _dropDownService.GetDropDownById(statusId);
                        var status = testCaseStatus.DropDownValue;

                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "TestCase", entity.Id, entity.Name, entity.Id, entity.Name, "Test Case Status", status, LoggedUserId, GetDateTime);
                    }
                }

                string issueId = null;
                var failStatusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Test Case Status", "Fail");
                if (statusId == failStatusId)
                {
                    var newstatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Status", "New from Test Plan");
                    var medium = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Priority", "Medium");
                    var bug = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Type", "Bug");
                    var issue = await _issueService.GetIssueByName(SiteId, entity.Name, entity.ProjectId);
                    if (issue == null)
                    {
                        var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                        issue = new Issue();
                        issue.IssueNumber = await _issueService.GetLastIssueNumber() + 1;
                        issue.SiteId = SiteId;
                        issue.TestCaseId = entity.Id;
                        issue.ProjectId = entity.ProjectId;
                        issue.ProjectModuleId = entity.ProjectModuleId;

                        issue.RequirementId =
                            !string.IsNullOrWhiteSpace(entity.RequirementId) &&
                            entity.RequirementId != "undefined" &&
                            entity.RequirementId != "null"
                                ? entity.RequirementId
                                : null;
                        issue.Name = entity.Name;
                        issue.PriorityId = medium;
                        issue.StatusId = newstatus;
                        issue.EmployeeId = entity.EmployeeId;
                        issue.TypeId = bug;
                        issue.ReportedById = EmployeeId;
                        //issue.Description = issue.Description = model.Description + "<br><br>Steps:" + model.Steps + "<br><br>Expected Result:<br>" + model.ExpectedResult + "<br><br>Actual Result:<br>" + model.ActualResult;

                        if (!string.IsNullOrEmpty(entity.Description))
                        {
                            var formattedDescription = issue.Description = entity.Description + "<br><br>Steps:" + entity.Steps + "<br><br>Expected Result:<br>" + entity.ExpectedResult + "<br><br>Actual Result:<br>" + entity.ActualResult;

                            issue.Description = await _azureBlobImageServices
                                .ProcessHtmlAndManageImagesAsync(
                                    formattedDescription,
                                    SiteData.Name,
                                    "test-case",
                                    issue.IssueNumber.ToString()
                                );
                        }

                        issue.AreaId = entity.AreaId;
                        issue.WorkspaceId = entity.WorkspaceId;
                        issue.CreatedById = LoggedUserId;
                        issue.UpdatedById = LoggedUserId;
                        issue.CreatedOnUtc = GetDateTime;
                        issue.UpdatedOnUtc = GetDateTime;
                        _issueService.InsertIssue(issue);
                    }

                    issueId = issue.Id;
                }
                if (hasMapping)
                {
                    _testCaseExecutionLogExecutionLogService.InsertTestCaseExecutionLog(
                        new TestCaseExecutionLog
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProjectReleaseTracking_ReqPlanTaskIssueMappingId = mappingId,
                            StatusId = statusId,
                            Comment = null,
                            IssueId = issueId,
                            CreatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime,
                            UpdatedById = LoggedUserId,
                            UpdatedOnUtc = GetDateTime
                        });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region DeleteTestCase
        // Title: DeleteTestCaseById
        // Description: This endpoint deletes a test case based on the provided testcase ID. It first retrieves the testcase entity by ID, checks if it exists, and if so, deletes the testcase. If the testcase is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        [SiteOwned(typeof(Vsky.Models.TestCase))]
        [RecordScope(typeof(Vsky.Models.TestCase))]
        public async Task<IActionResult> DeleteTestCase(string id)
        {
            try
            {
                // Fetch the testcase entity by its ID
                var entity = await _testCaseService.GetTestCaseById(id);
                // If no testcase is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No test case found with the specified id."));

                // Delete the testcase using the testcase service
                _testCaseService.DeleteTestCase(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion
    }
}