using System.Collections.Generic;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using System.Threading.Tasks;
using Vsky.Models;
using Vsky.Services.TestCases;
using Vsky.Services.Issues;
using Vsky.Services.ProjectTasks;
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

        public RequirementDashboardController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ITestCaseService testCaseService,
            IIssueService issueService,
            IProjectTaskService taskService,
            ITimesheetLinesService timesheetLinesService
            )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _testCaseService = testCaseService;
            _issueService = issueService;
            _taskService = taskService;
            _timesheetLinesService = timesheetLinesService;
        }

        #region GetTestCasesByRequirementId        
        [HttpGet("test-case-list")]
        public async Task<IActionResult> GetTestCasesByRequirementId(string requirementId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _testCaseService.GetTestCasesByRequirementId(SiteId, requirementId);
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
        [HttpGet("issue-list")]
        public async Task<IActionResult> GetIssuesByRequirementId(string requirementId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _issueService.GetIssuesByRequirementId(SiteId, requirementId);
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
        [HttpGet("task-list")]
        public async Task<IActionResult> GetTasksByRequirementId(string requirementId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _taskService.GetTasksByRequirementId(SiteId, requirementId);
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

        [HttpGet("timesheet-details")]
        public async Task<IActionResult> GetTimesheetDetails(
            string requirementId,
            string groupBy,
            string groupId
        )
        {

            if (string.IsNullOrWhiteSpace(groupId) || groupId == "undefined")
                return Ok();

            var siteId = _globalVariable.SiteId;

            var list = await _timesheetLinesService.GetTimesheetDetails(
                siteId,
                requirementId,
                groupBy,
                groupId);

            return Ok(_mapper.Map<List<TimesheetLinesModel>>(list));
        }

        #region GetGroupedTimesheetsByRequirementId
        [HttpGet("timesheet-groups")]
        public async Task<IActionResult> GetGroupedTimesheetsByRequirementId(
            string requirementId,
            string groupBy = "date"
        )
        {
            var siteId = _globalVariable.SiteId;

            if (string.IsNullOrWhiteSpace(groupBy) || groupBy == "undefined")
                return Ok();

            var result = await _timesheetLinesService.GetGroupedTimesheetsByRequirementId(
                siteId,
                requirementId,
                groupBy);

            return Ok(result);
        }
        #endregion
        #endregion
    }
}
