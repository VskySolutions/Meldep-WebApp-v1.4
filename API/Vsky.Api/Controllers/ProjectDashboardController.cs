using System.Collections.Generic;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Services.Common;
using Vsky.Services.ProjectModules;
using Vsky.Services.Sites;
using Vsky.Services.ProjectTasks;
using Vsky.Services.Issues;
using Vsky.Services.ProjectActivities;
using System.Threading.Tasks;
using Vsky.Api.ApiErrors;
using Vsky.Services.Note;
using Vsky.Services.TestPlans;
using Vsky.Core;
using Vsky.Models;
using Vsky.Services.TestCases;
using Vsky.Services.Requirements;

namespace Vsky.Api.Controllers
{
    [Route("project-dashboard")]

    public class ProjectDashboardController : BaseController
    {
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IProjectModuleService _projectModuleService;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IProjectTaskService _taskService;
        private readonly IIssueService _issueService;
        private readonly IProjectActivityService _activityService;
        private readonly INoteService _noteService;
        private readonly ITestPlanService _testPlanService;
        private readonly ITestCaseService _testCaseService;
        private readonly IRequirementGroupService _requirementGroupService;
        private readonly IRequirementService _requirementService;

        public ProjectDashboardController(
            GlobalVariable globalVariable,
            IMapper mapper,
            IProjectModuleService projectModuleService,
            ISiteService siteService,
            ICommonService commonService,
            IProjectTaskService taskService,
            IIssueService issueService,
            IProjectActivityService projectActivityService,
            INoteService noteService,
            ITestPlanService testPlanService,
            ITestCaseService testCaseService,
            IRequirementGroupService requirementGroupService,
            IRequirementService requirementService
            )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _projectModuleService = projectModuleService;
            _siteService = siteService;
            _commonService = commonService;
            _taskService = taskService;
            _issueService = issueService;
            _activityService = projectActivityService;
            _noteService = noteService;
            _testPlanService = testPlanService;
            _testCaseService = testCaseService;
            _requirementGroupService = requirementGroupService;
            _requirementService = requirementService;
        }

        #region projectModule
        [HttpPost("projectModulelist")]
        public IActionResult GetAllProjectModulesForDashboard(ProjectModuleSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _projectModuleService.GetAllProjectModulesForDashboard(SiteId, searchModel.ProjectId, searchModel.pageName, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                var model = new ProjectModuleListModel
                {
                    Data = _mapper.Map<IList<ProjectModuleModel>>(list),
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

        #region GetAllProjectTasks
        [HttpPost("projectTaskList")]
        public IActionResult GetAllProjectTasksForDashboard(ProjectTaskSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _taskService.GetAllProjectTasksForDashboard(SiteId,
                    searchModel.ProjectId,
                    searchModel.SortBy,
                    searchModel.Descending, 
                    searchModel.Page, 
                    searchModel.PageSize
                );
                var model = new ProjectTaskListModel
                {
                    Data = _mapper.Map<IList<ProjectTaskModel>>(list),
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

        #region GetAllIssues
        [HttpPost("issueList")]
        public IActionResult GetAllIssuesForDashboard(IssueSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _issueService.GetAllIssuesForDashboard(SiteId, searchModel.ProjectId, searchModel.TargetMonthStr, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                var model = new IssueListModel
                {
                    Data = _mapper.Map<IList<IssueModel>>(list),
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

        #region GetAllProjectActivities
        [HttpPost("projectActivityList")]
        public IActionResult GetAllProjectActivitiesForDashboard(ProjectActivitySearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _activityService.GetAllProjectActivitiesForDashboard(SiteId, 
                    searchModel.ProjectId,
                    searchModel.SortBy, 
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );
                // Map the fetched list to a model suitable for the response
                var model = new ProjectActivityListModel
                {
                    Data = _mapper.Map<IList<ProjectActivityModel>>(list),
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

        #region GetAllNotesForDashboard
        [HttpPost("notesList")]
        public IActionResult GetAllNotesByProjectId(NoteSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _noteService.GetAllNotesByProjectId(SiteId, searchModel.ProjectId, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                var model = new NoteListModel
                {
                    Data = _mapper.Map<IList<NoteModel>>(list),
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

        #region GetAllTestPlanForDashboard
        [HttpPost("testPlanList")]
        public IActionResult GetAllTestPlanForDashboard(TestPlanSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _testPlanService.GetAllTestPlanForDashboard(SiteId, searchModel.ProjectId, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
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

        #region GetAllTestCasesForDashboard
        // Title: Get All TestCases
        // Description: This endpoint fetches a list of test cases based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("testCasesList")]
        public IActionResult GetAllTestCasesForDashboard(TestCaseSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _testCaseService.GetAllTestCasesForDashboard(SiteId, searchModel.ProjectId, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                
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

        #region GetAllRequirementGroupsForDashboard
        // Title: Get All RequirementGroups
        // Description: This endpoint fetches a list of RequirementGroups based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("requirementGroupList")]
        public IActionResult GetAllRequirementGroupsForDashboard(RequirementGroupSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _requirementGroupService.GetAllRequirementGroupsForDashboard(SiteId, searchModel.ProjectId, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                
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

        #region GetAllRequirementsForDashboard
        // Title: Get All Requirements
        // Description: This endpoint fetches a list of Requirements based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("requirementList")]
        public IActionResult GetAllRequirementsForDashboard(RequirementSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _requirementService.GetAllRequirementsForDashboard(SiteId, searchModel.ProjectId, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

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

        #region sites info
        [HttpGet("siteInfobyid/{id}")]
        public async Task<IActionResult> GetOrganizationForDashboard(string id)
        {
            try
            {
                //Find site by id
                var entity = await _siteService.GetSiteDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No site found with the specified id."));

                var model = _mapper.Map<SiteModel>(entity);
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
