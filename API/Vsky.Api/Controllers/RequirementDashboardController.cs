using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MailKit.Search;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Issues;
using Vsky.Services.ProjectActionItem;
using Vsky.Services.ProjectQuestionsAnswer;
using Vsky.Services.ProjectTasks;
using Vsky.Services.TestCases;
using Vsky.Services.Timesheets;

namespace Vsky.Api.Controllers
{
    [Route("requirement-dashboard")]

    public class RequirementDashboardController : BaseController
    {
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ITestCaseService _testCaseService;
        private readonly IIssueService _issueService;
        private readonly IProjectTaskService _taskService;
        private readonly ITimesheetLinesService _timesheetLinesService;
        private readonly IProjectQuestionsAnswersService _projectQuestionsAnswerService;
        private readonly IProjectActionItemsService _projectActionItemsService;

        public RequirementDashboardController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ITestCaseService testCaseService,
            IIssueService issueService,
            IProjectTaskService taskService,
            ITimesheetLinesService timesheetLinesService,
            IProjectQuestionsAnswersService projectQuestionsAnswersService,
            IProjectActionItemsService projectActionItemsService
            )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _testCaseService = testCaseService;
            _issueService = issueService;
            _taskService = taskService;
            _timesheetLinesService = timesheetLinesService;
            _projectQuestionsAnswerService = projectQuestionsAnswersService;
            _projectActionItemsService = projectActionItemsService;
        }

        #region GetTestCasesByRequirementId        
        [HttpPost("test-case-list")]
        public async Task<IActionResult> GetTestCasesByRequirementId(RequirementCenterTestCaseSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _testCaseService.GetTestCasesByRequirementId(
                    SiteId,
                    LoggedUserId,
                    searchModel.RequirementId,
                    searchModel.SearchText,
                    searchModel.TestCaseNumber,
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
                var model = _mapper.Map<List<TestCaseModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetIssuesByRequirementId        
        [HttpPost("issue-list")]
        public async Task<IActionResult> GetIssuesByRequirementId(RequirementCenterIssueSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _issueService.GetIssuesByRequirementId(
                    SiteId,
                    searchModel.RequirementId,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.IssueNumber,
                    searchModel.Name,
                    searchModel.PriorityIds,
                    searchModel.StatusIds,
                    searchModel.IssueTypeIds,
                    searchModel.EmployeeIds,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize);

                var model = _mapper.Map<List<IssueModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetTasksByRequirementId
        [HttpPost("task-list")]
        public async Task<IActionResult> GetTasksByRequirementId(RequirementCenterTaskSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var siteId = _globalVariable.SiteId;

                var list = await _taskService.GetTasksByRequirementId(
                    siteId,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.RequirementId, 
                    searchModel.ProjectTaskNumber, 
                    searchModel.ProjectTaskIds, 
                    searchModel.ActivityOwners, 
                    searchModel.StatusIds, 
                    searchModel.PriorityIds, 
                    searchModel.TaskTagsIds,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = _mapper.Map<List<ProjectTaskModel>>(list);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Get Timesheets
        #region GetTimesheetByRequirementId        
        [HttpGet("timesheet-list")]
        public async Task<IActionResult> GetTimesheetByRequirementId(string requirementId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _timesheetLinesService.GetTimesheetsByRequirementId(SiteId, requirementId);
                var model = _mapper.Map<List<TimesheetLinesModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        [HttpPost("timesheet-details")]
        public async Task<IActionResult> GetTimesheetDetails(RequirementCenterTimesheetSearchModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel.GroupId) || searchModel.GroupId == "undefined")
            {
                return Ok();
            }

            var LoggedUserId = User.GetLoggedInUserId<string>();
            var siteId = _globalVariable.SiteId;

            var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";

            var list = await _timesheetLinesService.GetTimesheetDetails(
                siteId,
                searchModel.RequirementId,
                searchModel.GroupBy,
                searchModel.GroupId,
                createdBy,
                searchModel.SearchText,
                searchModel.EmployeeId,
                searchModel.ProjectTaskId,
                searchModel.ProjectActivityId,
                searchModel.ActivityDate,
                searchModel.FromDate,
                searchModel.ToDate,
                searchModel.ThisWeek,
                searchModel.LastNumberOfWeeks);

            return Ok(_mapper.Map<List<TimesheetLinesModel>>(list));
        }

        #region GetGroupedTimesheetsByRequirementId
        [HttpPost("timesheet-groups")]
        public async Task<IActionResult> GetGroupedTimesheetsByRequirementId(
            RequirementCenterTimesheetSearchModel searchModel
        )
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var siteId = _globalVariable.SiteId;

            if (string.IsNullOrWhiteSpace(searchModel.GroupBy) || searchModel.GroupBy == "undefined")
                return Ok();

            var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";
            var result = await _timesheetLinesService.GetGroupedTimesheetsByRequirementId(
                siteId,
                searchModel.RequirementId,
                searchModel.GroupBy,
                createdBy,
                searchModel.SearchText,
                searchModel.EmployeeId,
                searchModel.ProjectTaskId,
                searchModel.ProjectActivityId,
                searchModel.ActivityDate,
                searchModel.FromDate,
                searchModel.ToDate,
                searchModel.ThisWeek,
                searchModel.LastNumberOfWeeks,
                searchModel.SortBy,
                searchModel.Descending,
                searchModel.Page,
                searchModel.PageSize
            );

            var model = _mapper.Map<List<TimesheetGroupResponse>>(result);
            return Ok(model);
        }

        [HttpPost("timesheet-tabular-list")]
        public async Task<IActionResult> GetAllTimesheetsByRequirementId(RequirementCenterTimesheetSearchModel searchModel)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var siteId = _globalVariable.SiteId;

            var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";
            var result = await _timesheetLinesService.GetAllTimesheetsByRequirementId(
                siteId,
                searchModel.RequirementId,
                createdBy,
                searchModel.SearchText,
                searchModel.EmployeeId,
                searchModel.ProjectTaskId,
                searchModel.ProjectActivityId,
                searchModel.ActivityDate,
                searchModel.FromDate,
                searchModel.ToDate,
                searchModel.ThisWeek,
                searchModel.LastNumberOfWeeks,
                searchModel.SortBy,
                searchModel.Descending,
                searchModel.Page,
                searchModel.PageSize
            );

            var model = _mapper.Map<List<TimesheetLinesModel>>(result);
            return Ok(model);
        }
        #endregion
        #endregion

        #region GetProjectQAByRequirementId
        [HttpPost("project-QA")]
        public async Task<IActionResult> GetProjectQAByRequirementId(ProjectQuestionsAnswersSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var siteId = _globalVariable.SiteId;

                var list = await _projectQuestionsAnswerService.GetProjectQAByRequirementId(
                    siteId,
                    searchModel.SearchText,
                    searchModel.RequirementId,
                    searchModel.Title,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = new ProjectQuestionsAnswersList
                {
                    ProjectQuestionsAnswerList = list
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetProjectActionItemsByRequirementId
        [HttpPost("project-action-items")]
        public async Task<IActionResult> GetProjectActionItemsByRequirementId(ProjectActionItemsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var siteId = _globalVariable.SiteId;

                var list = await _projectActionItemsService.GetProjectActionItemsByRequirementId(
                    siteId,
                    searchModel.SearchText,
                    searchModel.RequirementId,
                    searchModel.Title,
                    searchModel.CustomerIds,
                    searchModel.EmployeeIds,
                    searchModel.PriorityIds,
                    searchModel.DueDate,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = new ProjectActionItemsList
                {
                    ProjectActionItemList = list
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
