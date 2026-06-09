using System;
using System.Collections.Generic;
using System.Globalization;
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
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.EmployeeClientLocations;
using Vsky.Services.EmployeeDepartments;
using Vsky.Services.EmployeeDesignations;
using Vsky.Services.EmployeeOrgLocations;
using Vsky.Services.Employees;
using Vsky.Services.EmployeeStatuses;
using Vsky.Services.EmployeeTypes;
using Vsky.Services.Issues;
using Vsky.Services.Persons;
using Vsky.Services.ProjectTasks;
using Vsky.Services.Sites;
using Vsky.Services.TestCases;
using Vsky.Services.TestPlans;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Vsky.Api.Models.EmployeeModel;

namespace Vsky.Api.Controllers
{
    [Route("test-case")]
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
        private readonly ApplicationDbContext _db;
        private readonly IAzureBlobImageServices _azureBlobImageServices;

        #endregion

        #region Services Initializations      
        public TestCaseController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ITestCaseService testCaseService, 
            ICommonService commonService,
            IIssueService issueService, 
            ISiteService siteService, 
            ApplicationDbContext db,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _testCaseService = testCaseService;
            _commonService = commonService;
            _issueService = issueService;
            _siteService = siteService;
            _db = db;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllTestCases
        // Title: Get All TestCases
        // Description: This endpoint fetches a list of test cases based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllTestCases(TestCaseSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of test cases on search criteria (name, sorting, pagination)
                var list = await _testCaseService.GetAllTestCases(
                    SiteId,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.TestCaseNumber,
                    searchModel.ProjectIds,
                    searchModel.PlanIds,
                    searchModel.TestedBys,
                    searchModel.StatusIds,
                    searchModel.FromDate,
                    searchModel.ToDate,
                    searchModel.SortBy,
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
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllTestCasesListForDropdown
        // Title: GetAllTestCasesListForDropdown
        // Description: This endpoint retrieves the details of a specific test case based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
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
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetTestCaseDetailsById
        // Title: GetTestCaseDetailsById
        // Description: This endpoint retrieves the details of a specific test case based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
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
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateTestCase
        // Title: CreateTestCase
        // Description: This endpoint handles the creation of a new test case. It maps the test case model to the test case entity, sets the creation details, and inserts the test case into the database. 
        [HttpPost]
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
                        }
                    }
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

        #region UpdateTestCase
        // Title: UpdateTestCase
        // Description: This endpoint updates an existing test case by its ID. 
        [HttpPut("{id}")]
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

                    entity.ProjectId = model.ProjectId;
                    entity.PlanId = model.PlanId;
                    entity.Name = model.Name;
                    entity.StatusId = model.StatusId;
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
                        }
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

        #region UpdateTestCaseStatus
        [HttpPut("updateTestCaseStatus/{id}/{statusId}")]
        public async Task<IActionResult> UpdateTestCaseStatus(string id, string statusId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _testCaseService.GetTestCaseById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No test case found with the specified id."));

                    entity.StatusId = statusId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _testCaseService.UpdateTestCase(entity);

                    var testcasestatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Test Case Status", "Fail");
                    if (statusId == testcasestatus)
                    {
                        //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                        var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                        var newstatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Status", "New from Test Plan");
                        var medium = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Priority", "Medium");
                        var bug = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Type", "Bug");
                        var exists = await _issueService.GetIssueByName(SiteId, entity.Name, entity.ProjectId);
                        if (exists == null)
                        {
                            var issue = new Issue();
                            issue.IssueNumber = await _issueService.GetLastIssueNumber() + 1;
                            issue.SiteId = SiteId;
                            issue.TestCaseId = entity.Id;
                            issue.ProjectId = entity.ProjectId;
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

        #region DeleteTestCase
        // Title: DeleteTestCaseById
        // Description: This endpoint deletes a test case based on the provided testcase ID. It first retrieves the testcase entity by ID, checks if it exists, and if so, deletes the testcase. If the testcase is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
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
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}