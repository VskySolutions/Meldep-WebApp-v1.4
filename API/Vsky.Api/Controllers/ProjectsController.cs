using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AutoMapper;
using Ical.Net.DataTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DailyPlanners;
using Vsky.Services.DropDowns;
using Vsky.Services.HelpDesks;
using Vsky.Services.Issues;
using Vsky.Services.Notifications;
using Vsky.Services.ProjectActivities;
using Vsky.Services.ProjectEmployeeMappings;
using Vsky.Services.ProjectMessage;
using Vsky.Services.ProjectModules;
using Vsky.Services.ProjectModulesUserMappings;
using Vsky.Services.Projects;
using Vsky.Services.ProjectsColor;
using Vsky.Services.ProjectsPinned;
using Vsky.Services.ProjectsTag;
using Vsky.Services.ProjectSwimLanes;
using Vsky.Services.ProjectTasks;
using Vsky.Services.ProjectUserMappings;
using Vsky.Services.ProjectWeeklyPlan;
using Vsky.Services.Requirements;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.Timesheets;

namespace Vsky.Api.Controllers
{
    [Route("projects")]
    public class ProjectsController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IDropDownService _dropDownService;
        private readonly IProjectMessageService _projectMessageService;
        private readonly INotificationService _notificationService;
        private readonly IProjectFilesService _projectFilesService;
        private readonly IProjectModuleFilesService _projectModuleFilesService;
        private readonly IProjectTaskFilesService _projectTaskFilesService;
        private readonly IProjectActivityFilesService _projectActivityFilesService;
        private readonly IProjectWeeklyService _projectWeeklyService;
        private readonly IProjectWeeklyDatesService _projectWeeklyDatesService;
        private readonly IProjectWeeklyDatesLinesService _projectWeeklyDatesLinesService;
        private readonly IProjectWeeklyPlanDatesLinesAssignedToService _projectWeeklyPlanDatesLinesAssignedToService;
        private readonly IProjectWeeklyPlanDatesReqTaskIssueMappingService _projectWeeklyPlanDatesReqTaskIssueMappingService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly IIssueService _issueService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly ITagService _tagService;
        private readonly IProjectsTagService _projectsTagService;
        private readonly IProjectUserMappingService _projectUserMappingService;
        private readonly IProjectModuleService _projectModuleService;
        private readonly IProjectTaskTagService _projectTaskTagService;
        private readonly IProjectActivityService _projectActivityService;
        private readonly IRequirementService _requirementService;
        private readonly IProjectSwimLanesService _projectSwimLanesService;
        private readonly IProjectSwimLanesListServices _projectSwimLanesListServices;
        private readonly IProjectSwimLanesListsTasksServices _projectSwimLanesListsTasksServices;
        private readonly IMasterProjectSwimlaneListsServices _masterProjectSwimlaneListsServices;
        private readonly IMasterNotificationService _masterNotificationService;
        private readonly IProjectModulesUserMappingService _projectModulesUserMappingService;
        private readonly IHelpDeskFilesService _helpDeskFilesService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly IDailyPlannerLineService _dailyPlannerLineService;
        private readonly ITimesheetLinesService _timesheetLinesService;
        private readonly IProjectsPinnedService _projectsPinnedService;
        private readonly IProjectsColorService _projectsColorService;
        #endregion

        #region Services Initializations
        public ProjectsController(
            GlobalVariable globalVariable,
            ApplicationDbContext context,
            IMapper mapper,
            IProjectService projectService,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            ICommonService commonService,
            ISiteService siteService,
            IDropDownService dropDownService,
            IProjectMessageService projectMessageService,
            INotificationService notificationService,
            IProjectFilesService projectFilesService,
            IProjectModuleFilesService projectModuleFilesService,
            IProjectTaskFilesService projectTaskFilesService,
            IProjectActivityFilesService projectActivityFilesService,
            IProjectWeeklyService projectWeeklyService,
            IProjectWeeklyDatesService projectWeeklyDatesService,
            IProjectWeeklyDatesLinesService projectWeeklyDatesLinesService,
            IProjectWeeklyPlanDatesLinesAssignedToService projectWeeklyPlanDatesLinesAssignedToService,
            IProjectWeeklyPlanDatesReqTaskIssueMappingService projectWeeklyPlanDatesReqTaskIssueMappingService,
            ISitesModifiedLogsService sitesModifiedLogsService,
            IIssueService issueService,
            IProjectTaskService projectTaskService,
            IProjectUserMappingService projectUserMappingService,
            ITagService tagService,
            IProjectsTagService projectsTagService,
            IProjectModuleService projectModuleService,
            IProjectTaskTagService projectTaskTagService,
            IProjectActivityService projectActivityService,
            IRequirementService requirementService,
            IProjectSwimLanesService projectSwimLaneService,
            IProjectSwimLanesListServices projectSwimLanesListServices,
            IProjectSwimLanesListsTasksServices projectSwimLanesListsTasksServices,
            IMasterProjectSwimlaneListsServices masterProjectSwimlaneListsServices,
            IMasterNotificationService masterNotificationService,
            IProjectModulesUserMappingService projectModulesUserMappingService,
            IHelpDeskFilesService helpDeskFilesService,
            IAzureBlobImageServices azureBlobImageServices,
            IDailyPlannerLineService dailyPlannerLineService,
            ITimesheetLinesService timesheetLinesService,
            IProjectsPinnedService projectsPinnedService,
            IProjectsColorService projectsColorService
        )
        {
            _globalVariable = globalVariable;
            _context = context;
            _mapper = mapper;
            _projectService = projectService;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _commonService = commonService;
            _siteService = siteService;
            _dropDownService = dropDownService;
            _commonService = commonService;
            _projectFilesService = projectFilesService;
            _projectMessageService = projectMessageService;
            _notificationService = notificationService;
            _projectModuleFilesService = projectModuleFilesService;
            _projectTaskFilesService = projectTaskFilesService;
            _projectActivityFilesService = projectActivityFilesService;
            _projectWeeklyService = projectWeeklyService;
            _projectWeeklyDatesService = projectWeeklyDatesService;
            _projectWeeklyDatesLinesService = projectWeeklyDatesLinesService;
            _projectWeeklyPlanDatesLinesAssignedToService = projectWeeklyPlanDatesLinesAssignedToService;
            _projectWeeklyPlanDatesReqTaskIssueMappingService = projectWeeklyPlanDatesReqTaskIssueMappingService;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _issueService = issueService;
            _projectTaskService = projectTaskService;
            _projectUserMappingService = projectUserMappingService;
            _tagService = tagService;
            _projectsTagService = projectsTagService;
            _projectModuleService = projectModuleService;
            _projectTaskTagService = projectTaskTagService;
            _projectActivityService = projectActivityService;
            _requirementService = requirementService;
            _projectSwimLanesService = projectSwimLaneService;
            _projectSwimLanesListServices = projectSwimLanesListServices;
            _projectSwimLanesListsTasksServices = projectSwimLanesListsTasksServices;
            _masterProjectSwimlaneListsServices = masterProjectSwimlaneListsServices;
            _masterNotificationService = masterNotificationService;
            _projectModulesUserMappingService = projectModulesUserMappingService;
            _helpDeskFilesService = helpDeskFilesService;
            _azureBlobImageServices = azureBlobImageServices;
            _dailyPlannerLineService = dailyPlannerLineService;
            _timesheetLinesService = timesheetLinesService;
            _projectsPinnedService = projectsPinnedService;
            _projectsColorService = projectsColorService;
        }
        #endregion

        #region GetAllProjects
        // Title: Get All Projects
        // Description: This endpoint fetches a list of projects based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllProjects(ProjectSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                int Status = 2;
                if (!string.IsNullOrWhiteSpace(searchModel.StatusId))
                {
                    var activeStatus = _dropDownService.GetDropDownById(searchModel.StatusId).GetAwaiter().GetResult();
                    Status = activeStatus.DropDownValue == "Active" ? 1 : (activeStatus.DropDownValue == "Inactive" ? 0 : 2);
                }

                // Fetch a list of projects based on search criterias
                var list = await _projectService.GetAllProjects(
                    SiteId,
                    searchModel.IsTemplate,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.ProjectIds,
                    searchModel.ProjectCategoryIds,
                    searchModel.ProjectStatusIds,
                    searchModel.ProjectTeamMemberIds,
                    searchModel.ProjectCoordinatorIds,
                    searchModel.ProjectLeadsIds,
                    searchModel.ProjectPriorityIds,
                    searchModel.ProjectTypeIds,
                    Status,
                    searchModel.CustomerIds,
                    searchModel.CompanyContactIds,
                    searchModel.CustomerId,
                    searchModel.ProjectTagIds,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                // Get projectIds from issues
                var projectIds = list.Select(x => x.Id).Distinct().ToList();

                // Get summary
                var summaryList = _issueService.GetIssueStatusSummaryByProjectIds(projectIds);
                var taskSummaryList = _projectTaskService.GetTaskStatusSummaryByProjectIds(projectIds);
                var requirementSummaryList = _requirementService.GetRequirementStatusSummaryByProjectIds(projectIds);

                // Prepare final response
                var model = new
                {
                    Data = _mapper.Map<IList<ProjectModel>>(list),
                    Total = list.TotalCount,
                    StatusSummary = summaryList,
                    TaskStatusSummary = taskSummaryList,
                    RequirementStatusSummary = requirementSummaryList
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("projectListForNotes")]
        public async Task<IActionResult> GetAllProjectsForNotes(ProjectSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                int Status = 2;
                if (!string.IsNullOrWhiteSpace(searchModel.StatusId))
                {
                    var activeStatus = _dropDownService.GetDropDownById(searchModel.StatusId).GetAwaiter().GetResult();
                    Status = activeStatus.DropDownValue == "Active" ? 1 : (activeStatus.DropDownValue == "Inactive" ? 0 : 2);
                }
                // Fetch a list of projects based on search criterias
                var list = await _projectService.GetAllProjectsForNotes(
                    SiteId,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.ProjectIds,
                    searchModel.ProjectCategoryIds,
                    searchModel.ProjectStatusIds,
                    searchModel.ProjectTeamMemberIds,
                    searchModel.ProjectCoordinatorIds,
                    searchModel.ProjectLeadsIds,
                    searchModel.ProjectPriorityIds,
                    searchModel.ProjectTypeIds,
                    Status,
                    searchModel.CustomerIds,
                    searchModel.CompanyContactIds,
                    searchModel.CustomerId,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                // Prepare final response
                var model = new
                {
                    Data = _mapper.Map<IList<ProjectModel>>(list),
                    Total = list.TotalCount,
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("listFromSP")]
        public async Task<IActionResult> GetAllProjectListFromSP([FromBody] ProjectListRequest request)
        {
            try
            {
                var result = await _projectService.GetProjectsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException.ToString());
            }
        }
        #endregion

        #region GetAllProjectListForDropdown
        // Title: GetAllProjectListForDropdown
        // Description: This endpoint retrieves the details of a specific project based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllProjectListForDropdown([FromQuery] string[] statuses = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _projectService.GetAllProjectListForDropdown(SiteId, LoggedUserId, statuses);
                var model = _mapper.Map<List<ProjectModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpGet("allDropdown/list/{isTemplate}/{ActiveStatus}/{isAllProject}")]
        [HttpGet("allDropdown/list")]
        public async Task<IActionResult> GetProjectsListForDropdown(bool isTemplate, string ActiveStatus, bool isAllProject)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _projectService.GetProjectsListForDropdown(SiteId, LoggedUserId, isTemplate, ActiveStatus, isAllProject);
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllProjectTagListForDropdown
        // Title: GetAllProjectTagListForDropdown
        // Description: This endpoint retrieves all tags created by the logged-in user for dropdown display.
        [HttpGet("projectTags/dropdown/list")]
        public async Task<IActionResult> GetAllProjectTagListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();

                var list = await _projectsTagService.GetProjectTagByUserId(LoggedUserId);
                var model = _mapper.Map<List<CommonDropDown>>(list);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetProjectById
        // Title: GetProjectById
        // Description: This endpoint retrieves the details of a specific project based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(string id)
        {
            try
            {
                // Fetch the project entity by its ID from the service
                var entity = await _projectService.GetProjectDetailsById(id);
                // If the project entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No project found with the specified id."));

                // Map the project entity to a ProjectModel object
                var model = _mapper.Map<ProjectModel>(entity);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("projectdetailsbyids")]
        public async Task<IActionResult> GetProjectDetailsByIds([FromQuery] string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return BadRequest(new BadRequestError("Project are missing."));

            var idArray = ids.Split(','); // Split the comma-separated string into an array
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var entity = await _projectService.GetProjectDetailsByIds(idArray, SiteId);
            if (entity == null)
                return BadRequest(new BadRequestError("No project found with the specified ids."));

            var model = _mapper.Map<List<ProjectModel>>(entity);
            return Ok(model);
        }
        #endregion

        #region GetProjectSummeryInDetails
        [HttpGet("project-summery-in-details")]
        public async Task<IActionResult> GetProjectSummeryInDetails(string projectId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                string weeklyTypeId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Project Weekly Target Planning", "Weekly");
                string monthlyTypeId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Project Weekly Target Planning", "Monthly");

                // Fetch the project entity by its ID from the service
                var entity = await _projectService.GetProjectSummeryInDetails(projectId, GetDateTime, weeklyTypeId, monthlyTypeId);
                // If the project entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No project found with the specified id."));

                // Get summary
                var ids = new List<string> { projectId };
                var taskSummaryList = _projectTaskService.GetTaskStatusSummaryByProjectIds(ids);

                // Map the project entity to a ProjectModel object
                //var model = _mapper.Map<ProjectModel>(entity);
                var model = new
                {
                    Data = _mapper.Map<ProjectModel>(entity),
                    TaskStatusSummary = taskSummaryList,
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("project-my-work-summery-in-details")]
        public async Task<IActionResult> GetProjectMyWorkSummeryInDetails(string projectId)
        {
            try
            {
                var GetDateTime = _siteService.GetDateTime();

                // Fetch the project entity by its ID from the service
                var entity = await _projectService.GetProjectMyWorkSummeryInDetails(projectId, GetDateTime);
                // If the project entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No project found with the specified id."));

                // Map the project entity to a ProjectModel object
                var model = new
                {
                    Data = _mapper.Map<ProjectModel>(entity)
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("project-sdlc-summery-in-details")]
        public async Task<IActionResult> GetProjectSdlcSummeryInDetails(string projectId)
        {
            try
            {
                var GetDateTime = _siteService.GetDateTime();
                // Fetch the project entity by its ID from the service
                var entity = await _projectService.GetProjectSdlcSummeryInDetails(projectId, GetDateTime);
                // If the project entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No project found with the specified id."));

                // Get summary
                var ids = new List<string> { projectId };
                var summaryList = _issueService.GetIssueStatusSummaryByProjectIds(ids);
                var requirementSummaryList = _requirementService.GetRequirementStatusSummaryByProjectIds(ids);

                // Map the project entity to a ProjectModel object;
                var model = new
                {
                    Data = _mapper.Map<ProjectModel>(entity),
                    StatusSummary = summaryList,
                    RequirementStatusSummary = requirementSummaryList
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetProjectsAndCharterList
        // Title: Get Projects And Charter List
        // Description: This endpoint fetches a list of projects based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("Dashboardlist")]
        public IActionResult GetProjectsAndCharterListForDashboard(ProjectSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of projects based on search criterias
                var list = _projectService.GetProjectsAndCharterListForDashboard(SiteId, searchModel.ProjectIds, searchModel.ProjectStatusIds, searchModel.ProjectTeamMemberIds, searchModel.ProjectCoordinatorIds, searchModel.ProjectPriorityIds, searchModel.ProjectTypeIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                // Map the fetched list to a model suitable for the response
                var model = new ProjectListModel
                {
                    Data = _mapper.Map<IList<ProjectModel>>(list),
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

        #region GetAllFilesByProjectId
        [HttpPost("filesList")]
        public IActionResult GetAllFilesByProjectId(PicturesSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _commonService.GetAllFilesByProjectId(SiteId, searchModel.ProjectId, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                var model = new PicturesListModel
                {
                    Data = _mapper.Map<IList<PicturesModel>>(list),
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

        #region Create
        // Title: CreateProject
        // Description: This endpoint handles the creation of a new project. It first checks if a project with the same name already exists for the specified customer. If not, it maps the project model to the project entity, sets the creation details, and inserts the project into the database. Additionally, it associates team members with the project if provided.
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromForm] ProjectModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the project already exists
                    var exists = await _projectService.GetProjectByName(SiteId, model.Name, model.CustomerId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Project name already exists, try with another."));

                    // Map the project model to the project entity
                    var entity = _mapper.Map<Project>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    // Set custom properties
                    entity.SiteId = SiteId;
                    if (model.StartDateStr != "" && model.StartDateStr != null)
                        entity.StartDate = DateTime.ParseExact(model.StartDateStr, "MM/dd/yyyy", null);
                    if (model.GoLiveDateStr != "" && model.GoLiveDateStr != null)
                        entity.GoLiveDate = DateTime.ParseExact(model.GoLiveDateStr, "MM/dd/yyyy", null);

                    entity.Active = model.Active;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "project",
                                entity.Id
                            );
                    }

                    entity.Year = DateTime.UtcNow.Year;
                    // Set the created by and created on properties
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _projectService.InsertProject(entity);

                    //Add Project Employees
                    string ProjectId = entity.Id;
                    if (model.ProjectFiles != null && model.ProjectFiles.Any())
                    {
                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "project", model.ProjectFiles, entity.Id);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ProjectFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = ProjectId,
                                Module = entity.Name,
                                SubModuleId = ProjectId,
                                Sub_Module = entity.Name,
                                Type = "Projects",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var projectFile = new ProjectFiles
                            {
                                FileId = picture.Id,
                                ProjectId = ProjectId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _projectFilesService.InsertProjectFile(projectFile);

                            index++;
                        }
                    }

                    if (!string.IsNullOrEmpty(model.ProjectStatusId))
                    {
                        var projectStatus = await _dropDownService.GetDropDownById(model.ProjectStatusId);
                        var status = projectStatus.DropDownValue;
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Projects", entity.Id, model.Name, entity.Id, model.Name, "Project Status", status, LoggedUserId, GetDateTime);
                    }

                    if (!string.IsNullOrEmpty(model.GoLiveDateStr))
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Projects", entity.Id, model.Name, entity.Id, model.Name, "Due Date", model.GoLiveDateStr, LoggedUserId, GetDateTime);

                    if (!string.IsNullOrEmpty(model.PlanApproverId))
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Projects", entity.Id, model.Name, entity.Id, model.Name, "Plan Approver", model.PlanApproverId, LoggedUserId, GetDateTime);

                    // Project Security
                    var ProjectSecurity = new ProjectUserMapping();
                    ProjectSecurity.AspNetUserId = LoggedUserId;
                    ProjectSecurity.ProjectId = ProjectId;
                    ProjectSecurity.FullAccess = true;
                    ProjectSecurity.ViewOnly = true;
                    ProjectSecurity.Notes = true;
                    ProjectSecurity.CreatedById = LoggedUserId;
                    ProjectSecurity.CreatedOnUtc = GetDateTime;
                    _projectUserMappingService.InsertProjectUser(ProjectSecurity);

                    // Generate WorkBoard
                    await GenerateDefaultWorkboard(SiteId, entity.Id, LoggedUserId, GetDateTime);

                    return Ok(entity);
                }
                // Return model state errors if the model state is not valid
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost("projectTags")]
        public async Task<IActionResult> CreateProjectTags(TagModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var existingTagList = _projectsTagService.GetProjectTagsByProjectIdAndUserId(SiteId, model.ProjectId, LoggedUserId);
                    var existingTagNames = existingTagList.Select(x => x.Tags.Name).ToList();
                    var toAdd = model.TagsNameList.Except(existingTagNames).ToList();

                    if (model.Flag != "TG")
                    {
                        var toRemove = existingTagList.Where(x => !model.TagsNameList.Contains(x.Tags.Name)).ToList();

                        foreach (var mapping in toRemove)
                        {
                            if (mapping != null)
                            {
                                var existingProjectTag = await _projectsTagService.GetByNameProjectIdAndUserId(SiteId, mapping.Tags.Name, model.ProjectId, LoggedUserId);
                                if (existingProjectTag != null)
                                    _projectsTagService.DeleteProjectTag(existingProjectTag);
                            }
                        }
                    }

                    if (toAdd.Count > 0)
                    {
                        foreach (var tag in toAdd)
                        {
                            if (tag != null)
                            {
                                var existingTags = await _tagService.GetTagByName(SiteId, tag);

                                if (existingTags == null)
                                {
                                    existingTags = new Tags
                                    {
                                        Name = tag,
                                        SiteId = SiteId,
                                        Color = model.Color,
                                        CreatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime,
                                        UpdatedById = LoggedUserId,
                                        UpdatedOnUtc = GetDateTime
                                    };

                                    _tagService.InsertTags(existingTags);
                                }

                                var existingProjectTag = _projectsTagService.GetByNameProjectIdAndUserId(SiteId, tag, model.ProjectId, LoggedUserId);
                                if (existingProjectTag.Result != null)
                                    continue;

                                var projectTags = new ProjectTags
                                {
                                    TagId = existingTags.Id,
                                    ProjectId = model.ProjectId,
                                    AspNetUserId = LoggedUserId,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };

                                _projectsTagService.InsertProjectTag(projectTags);
                            }
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

        private async Task<bool> GenerateDefaultWorkboard(string SiteId, string ProjectId, string LoggedUserId, DateTime GetDateTime)
        {
            string SwimlaneTypeId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Project Swimlane Types", "Task Status As Lists");
            var MastListData = await _masterProjectSwimlaneListsServices.GetMasterProjectSwimlaneBySwimlaneTypeId(SwimlaneTypeId);

            var ProjectSwimlane = new ProjectSwimLanes()
            {
                ProjectId = ProjectId,
                SwimlaneTypeId = SwimlaneTypeId,
                Name = "Default",
                SortOrder = 1,
                CreatedById = LoggedUserId,
                CreatedOnUtc = GetDateTime,
                UpdatedById = LoggedUserId,
                UpdatedOnUtc = GetDateTime
            };
            _projectSwimLanesService.InsertProjectSwimLane(ProjectSwimlane);

            if (MastListData.Any())
            {
                foreach (var item in MastListData)
                {
                    var List = new ProjectSwimLanesList()
                    {
                        ProjectSwimlaneId = ProjectSwimlane.Id,
                        Name = item.Name,
                        SortOrder = item.SortOrder,
                        CreatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedById = LoggedUserId,
                        UpdatedOnUtc = GetDateTime
                    };
                    _projectSwimLanesListServices.InsertProjectSwimLaneList(List);
                }
            }

            return true;
        }
        #endregion

        #region AddFiles
        // Title: AddFiles
        // Description: This endpoint handles the creation of a new project. It first checks if a project with the same name already exists for the specified customer. If not, it maps the project model to the project entity, sets the creation details, and inserts the project into the database. Additionally, it associates team members with the project if provided.
        [HttpPost("add-project-files")]
        public async Task<IActionResult> AddFiles([FromForm] ProjectModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the project entity by its ID
                    var entity = await _projectService.GetById(model.Id);

                    // If no project is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project found with the specified id."));

                    //Add Project Employees
                    string ProjectId = entity.Id;

                    if (model.ProjectFiles != null && model.ProjectFiles.Any())
                    {
                        int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(ProjectId, "Projects");

                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "project", model.ProjectFiles, entity.Id, existingImagesCount);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ProjectFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = ProjectId,
                                Module = entity.Name,
                                SubModuleId = ProjectId,
                                Sub_Module = entity.Name,
                                Type = "Projects",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var projectFile = new ProjectFiles
                            {
                                FileId = picture.Id,
                                ProjectId = ProjectId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _projectFilesService.InsertProjectFile(projectFile);

                            index++;
                        }
                    }
                    return Ok(entity);
                }
                // Return model state errors if the model state is not valid
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region UpdateProject
        // Title: UpdateProject
        // Description: This endpoint updates an existing project by its ID. It validates the project model, checks for duplicate project names within the same customer, updates the project's details, removes existing team members, and adds new team members if provided.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(string id, [FromForm] ProjectModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the project entity by its ID
                    var entity = await _projectService.GetById(id);

                    // If no project is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project found with the specified id."));

                    if (!model.IsCharter)
                    {
                        // Check if there is any project with the same name and customer ID that is not marked as deleted and has a different ID
                        var exists = await _projectService.GetProjectByName(SiteId, model.Name, model.CustomerId, model.IsTemplate, id);

                        // If project exists, return a bad request with an error message
                        if (exists != null)
                            return BadRequest(new BadRequestError("Project name already exists, try with another."));

                        bool IsProjectStatusChanged = model.ProjectStatusId != entity.ProjectStatusId;
                        bool IsProjectDueDateChanged = !string.IsNullOrEmpty(model.GoLiveDateStr) && DateTime.ParseExact(model.GoLiveDateStr, "MM/dd/yyyy", null) != entity.GoLiveDate;
                        bool IsPlanApproverChanged = model.PlanApproverId != entity.PlanApproverId;

                        // Retrieve all file IDs from the project files
                        var allProjectFileIds = (await _projectFilesService.GetAllProjectFileByProjectId(SiteId, id)).Select(file => file.Id).ToList();
                        var missingFileIds = allProjectFileIds.ToList();
                        if (model.ExistingFiles != null)
                        {
                            var existingFileIds = model.ExistingFiles.Select(fileJson =>
                            {
                                var file = JsonConvert.DeserializeObject<Picture>(fileJson);
                                return file.Id.Trim().ToLower();
                            })
                            .ToList();

                            // Compare and find missing file IDs
                            missingFileIds = allProjectFileIds.Except(existingFileIds).ToList();
                        }
                        if (allProjectFileIds.Any())
                        {
                            foreach (var projectFilesId in missingFileIds)
                            {
                                var projectFileDate = await _projectFilesService.GetProjectFileById(projectFilesId);
                                if (projectFileDate != null)
                                    _projectFilesService.DeleteProjectFiles(projectFileDate);
                            }
                        }

                        // Set the user who updated the project and the current UTC time for tracking purposes
                        entity.CustomerId = model.CustomerId;
                        entity.CompanyContactId = model.CompanyContactId;
                        entity.ProjectTypeId = model.ProjectTypeId;
                        entity.ProjectStatusId = model.ProjectStatusId;
                        entity.ProjectPriorityId = model.ProjectPriorityId;
                        entity.ProjectCoordinatorId = model.ProjectCoordinatorId;
                        entity.ProjectCategoryId = !string.IsNullOrEmpty(model.ProjectCategoryId) && model.ProjectCategoryId != "undefined" ? model.ProjectCategoryId : null;
                        entity.ProjectSubcategoryId = !string.IsNullOrEmpty(model.ProjectSubcategoryId) && model.ProjectSubcategoryId != "undefined" ? model.ProjectSubcategoryId : null;
                        entity.PlanApproverId = !string.IsNullOrEmpty(model.PlanApproverId) ? model.PlanApproverId : null;

                        entity.Active = model.Active;
                        entity.Name = model.Name;
                        entity.StartDate = model.StartDateStr != "" && model.StartDateStr != null ? DateTime.ParseExact(model.StartDateStr, "MM/dd/yyyy", null) : null;
                        entity.GoLiveDate = model.GoLiveDateStr != "" && model.GoLiveDateStr != null ? DateTime.ParseExact(model.GoLiveDateStr, "MM/dd/yyyy", null) : null;

                        if (!string.IsNullOrEmpty(model.Description))
                        {
                            entity.Description = await _azureBlobImageServices
                                .ProcessHtmlAndManageImagesAsync(
                                    model.Description,
                                    SiteData.Name,
                                    "project",
                                    entity.Id,
                                    entity.Description
                                );
                        }

                        entity.UpdatedById = LoggedUserId;
                        entity.UpdatedOnUtc = GetDateTime;
                        _projectService.UpdateProject(entity);

                        if (model.ProjectFiles != null && model.ProjectFiles.Any())
                        {
                            int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(id, "Projects");

                            // Upload multiple files to Azure
                            var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "project", model.ProjectFiles, entity.Id, existingImagesCount);
                            int index = 0;

                            foreach (var fileUrl in urls)
                            {
                                var file = model.ProjectFiles[index];

                                var picture = new Picture
                                {
                                    SeoFilename = Path.GetFileName(file.FileName),
                                    MimeType = file.ContentType,
                                    VirtualPath = fileUrl, // Azure URL
                                    ModuleId = id,
                                    Module = entity.Name,
                                    SubModuleId = id,
                                    Sub_Module = entity.Name,
                                    Type = "Projects",
                                    SiteId = SiteId,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };

                                _commonService.InsertPicture(picture);

                                var projectFile = new ProjectFiles
                                {
                                    FileId = picture.Id,
                                    ProjectId = id,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };

                                _projectFilesService.InsertProjectFile(projectFile);

                                index++;
                            }
                        }

                        if (IsProjectStatusChanged)
                        {
                            string projectStatus = _dropDownService.GetDropDownById(model.ProjectStatusId).Result.DropDownValue;
                            _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Projects", entity.Id, model.Name, entity.Id, model.Name, "Project Status", projectStatus, LoggedUserId, GetDateTime);
                        }

                        if (IsProjectDueDateChanged)
                            _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Projects", entity.Id, model.Name, entity.Id, model.Name, "Due Date", model.GoLiveDateStr, LoggedUserId, GetDateTime);

                        if (IsPlanApproverChanged)
                            _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Projects", entity.Id, model.Name, entity.Id, model.Name, "Plan Approver", model.PlanApproverId, LoggedUserId, GetDateTime);
                    }

                    if (model.Tab == "2_tab")
                    {
                        if (model.ProjectEmployeeMappings.Count() > 0)
                        {
                            var managerRoleId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Employee Designation", "Project Manager");
                            var coordinatorRoleId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Employee Designation", "Project Coordinator");

                            // Define role priority
                            var rolePriority = new Dictionary<string, int>
                            {
                                { managerRoleId, 1 },
                                { coordinatorRoleId, 2 },
                            };

                            var employeeGroups = model.ProjectEmployeeMappings
                                                .Where(x =>
                                                    !x.Deleted ||
                                                    (x.Deleted && !string.IsNullOrWhiteSpace(x.Id))
                                                )
                                                .GroupBy(x => x.EmployeeId)
                                                .Select(g =>
                                                    g.OrderBy(x =>
                                                        x.Deleted
                                                            ? 100
                                                            : rolePriority.TryGetValue(x.EmployeeDesignationId, out var priority)
                                                                ? priority
                                                                : 99
                                                    ).First()
                                                )
                                                .ToList();

                            var employeeUserMapping = new Dictionary<(string EmployeeId, string DesignationId), string>();

                            var processedUserIds = new HashSet<string>();
                            foreach (var item in employeeGroups)
                            {
                                var aspNetUserId = _commonService.GetLoggeduserIdByEmployeeId(SiteId, item.EmployeeId);
                                var existingUser = await _projectUserMappingService.GetRecordByUserIdandProjectId(SiteId, aspNetUserId, id);
                                
                                string userMappingId = null;
                                // Get all roles for this employee
                                var allRolesForEmployee = model.ProjectEmployeeMappings
                                    .Where(x => x.EmployeeId == item.EmployeeId && !x.Deleted)
                                    .ToList();

                                // Pick the highest priority active role
                                var activeRole = allRolesForEmployee
                                    .OrderBy(x => rolePriority.ContainsKey(x.EmployeeDesignationId) ? rolePriority[x.EmployeeDesignationId] : 99)
                                    .FirstOrDefault();
                                if (aspNetUserId != null)
                                {
                                    if (activeRole != null)
                                    {
                                        // Employee still has at least one active role
                                        bool hasFullAccess = activeRole.EmployeeDesignationId == managerRoleId ||
                                                             activeRole.EmployeeDesignationId == coordinatorRoleId;

                                        userMappingId = await UpdateOrInsertUserMapping(existingUser, aspNetUserId, id, LoggedUserId, GetDateTime, hasFullAccess, true, true, false, processedUserIds);
                                    }
                                    else
                                    {
                                        userMappingId = await UpdateOrInsertUserMapping(existingUser, aspNetUserId, id, LoggedUserId, GetDateTime, false, true, true, true, processedUserIds, SiteId);
                                    }
                                }
                                employeeUserMapping[(item.EmployeeId, item.EmployeeDesignationId)] = userMappingId;
                            }
                            foreach (var item in model.ProjectEmployeeMappings)
                            {
                                employeeUserMapping.TryGetValue(
                                    (item.EmployeeId, item.EmployeeDesignationId),
                                    out var userMappingId
                                );

                                var type = await _projectEmployeeMappingService.GetProjectEmployeeById(item.Id);
                                if (type != null)
                                {
                                    if (!item.Deleted)
                                    {
                                        if (item.EmployeeDesignationId == managerRoleId)
                                        {
                                            var existEmpForPMRole = await _projectEmployeeMappingService.GetProjectEmployeeByRoleIdAndProjectId(SiteId, id, managerRoleId, null, item.Id);
                                            if (existEmpForPMRole != null)
                                                continue;
                                        }

                                        var exist = await _projectEmployeeMappingService.GetProjectEmployeeByRoleIdAndProjectId(SiteId, id, item.EmployeeDesignationId, item.EmployeeId, item.Id);
                                        if (exist != null)
                                            continue;
                                    }

                                    type.ProjectId = id;
                                    type.EmployeeId = item.EmployeeId;
                                    type.EmployeeDesignationId = item.EmployeeDesignationId;
                                    type.ProductivityFactor = item.ProductivityFactor;
                                    type.ProjectUserMappingId = userMappingId;

                                    // Set the Updated by and Updated on properties
                                    type.UpdatedById = LoggedUserId;
                                    type.UpdatedOnUtc = GetDateTime;
                                    type.Deleted = item.Deleted;
                                    _projectEmployeeMappingService.UpdateProjectEmployees(type);
                                }
                                else
                                {
                                    if (!item.Deleted)
                                    {
                                        if (item.EmployeeDesignationId == managerRoleId)
                                        {
                                            var existEmpForPMRole = await _projectEmployeeMappingService.GetProjectEmployeeByRoleIdAndProjectId(SiteId, id, managerRoleId, null);
                                            if (existEmpForPMRole != null)
                                                continue;
                                        }

                                        var exist = await _projectEmployeeMappingService.GetProjectEmployeeByRoleIdAndProjectId(SiteId, id, item.EmployeeDesignationId, item.EmployeeId);
                                        if (exist != null)
                                            continue;

                                        AddProjectEmployee(id, item.EmployeeId, item.EmployeeDesignationId, item.ProductivityFactor, userMappingId, LoggedUserId, GetDateTime, item.Deleted);
                                    }
                                }
                            }
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

        #region UpdateProjectIsPinned
        [HttpPut("pinstatus/{id}/{pinstatus}")]
        public async Task<IActionResult> UpdateProjectIsPinned(string id, bool pinstatus)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool result = await UpdateProjectDetails(id, pinstatus, "pin");
                    if (!result)
                        BadRequest(new BadRequestError("No project found with the specified id."));

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

        #region UpdateProjectColorandStatus
        [HttpPost("projectColor-projectStatus")]
        public async Task<IActionResult> UpdateProjectColorandStatus(ProjectModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the issue entity by its ID
                    var entity = await _projectService.GetById(model.Id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project found with the specified id."));

                    if (!string.IsNullOrEmpty(model.ProjectColor))
                    {
                       
                        // Check if ProjectColor record exists for this project & user
                        var existsProjectColor = await _projectsColorService.GetProjectsColorByUser(entity.Id, LoggedUserId);

                        if (existsProjectColor != null)
                        {
                            // Update existing record
                            existsProjectColor.Color = model.ProjectColor;
                            existsProjectColor.UpdatedById = LoggedUserId;
                            existsProjectColor.UpdatedOnUtc = GetDateTime;

                            _projectsColorService.UpdateProjectColor(existsProjectColor);
                        }
                        else
                        {
                            // Insert new record
                            var projectColor = new ProjectColor
                            {
                                ProjectId = entity.Id,
                                AspNetUserId = LoggedUserId,
                                Color = model.ProjectColor,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime,
                                UpdatedById = LoggedUserId,
                                UpdatedOnUtc = GetDateTime,
                            };

                            _projectsColorService.InsertProjectColor(projectColor);
                        }
                    }

                    if (model.ActiveStatus != null)
                        entity.Active = model.ActiveStatus == "Active" ? true : false;

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _projectService.UpdateProject(entity);

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

        #region Update Project Status
        [HttpPut("{id}/{statusId}")]
        public async Task<IActionResult> UpdateProjectStatus(string id, string statusId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the issue entity by its ID
                    var entity = await _projectService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project found with the specified id."));

                    bool IsTaskStatusChanged = statusId != entity.ProjectStatusId;

                    bool result = await UpdateProjectDetails(id, statusId, "status");

                    if (IsTaskStatusChanged)
                    {
                        var projectStatus = await _dropDownService.GetDropDownById(statusId);
                        var status = projectStatus.DropDownValue;

                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Projects", entity.Id, entity.Name, entity.Id, entity.Name, "Project Status", status, LoggedUserId, GetDateTime);
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

        #region UpdateProjectPriority
        [HttpPut("priority/{id}/{priorityId}")]
        public async Task<IActionResult> UpdateProjectPriority(string id, string priorityId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool result = await UpdateProjectDetails(id, priorityId, "priority");
                    if (!result)
                        BadRequest(new BadRequestError("No project found with the specified id."));

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

        #region UpdateProjectType
        [HttpPut("type/{id}/{typeId}")]
        public async Task<IActionResult> UpdateProjectType(string id, string typeId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool result = await UpdateProjectDetails(id, typeId, "type");
                    if (!result)
                        BadRequest(new BadRequestError("No project found with the specified id."));

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

        #region UpdateProjectEndDate
        [HttpPut("end-date/{id}/{goLiveDate}")]
        public async Task<IActionResult> UpdateProjectEndDate(string id, string goLiveDate)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the task entity by its ID
                    var entity = await _projectService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project found with the specified id."));

                    var endDate = goLiveDate.Replace("-", "/");
                    bool IsTaskDueDateChanged = !string.IsNullOrEmpty(endDate) && DateTime.ParseExact(endDate, "MM/dd/yyyy", null) != entity.GoLiveDate;

                    if (IsTaskDueDateChanged)
                    {
                        bool result = await UpdateProjectDetails(id, DateTime.ParseExact(endDate, "MM/dd/yyyy", null), "golivedate");

                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Projects", entity.Id, entity.Name, entity.Id, entity.Name, "Due Date", endDate, LoggedUserId, GetDateTime);
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

        #region DeleteProject
        // Title: DeleteProjectById
        // Description: This endpoint deletes a project based on the provided project ID. It first retrieves the project entity by ID, checks if it exists, and if so, deletes the project. If the project is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(string id)
        {
            try
            {
                // Fetch the project entity by its ID
                var entity = await _projectService.GetById(id);
                // If no project is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No project found with the specified id."));

                // Delete the project using the project service
                _projectService.DeleteProject(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteFile
        // Title: DeleteFileById
        // Description: This endpoint deletes a file based on the provided ID. It first retrieves the file entity by ID, checks if it exists, and if so, deletes the file. If the file is not found, it returns a BadRequest response with an error message.
        [HttpDelete("project-file/{id}/{type}")]
        public async Task<IActionResult> DeleteFile(string id, string type)
        {
            try
            {
                switch (type)
                {
                    case "Projects":
                        {
                            // Fetch the file entity by its ID
                            var fileEntity = await _projectFilesService.GetProjectFileByFileId(id);
                            // If no file is found, return a BadRequest response with an error message
                            if (fileEntity == null)
                                return BadRequest(new BadRequestError("No file found with the specified id."));

                            // Delete the file using the project service
                            _projectFilesService.DeleteProjectFiles(fileEntity);
                            break;
                        }
                    case "Project Module":
                        {
                            // Fetch the file entity by its ID
                            var fileEntity = await _projectModuleFilesService.GetProjectModuleFileByFileId(id);
                            // If no file is found, return a BadRequest response with an error message
                            if (fileEntity == null)
                                return BadRequest(new BadRequestError("No file found with the specified id."));

                            // Delete the file using the project service
                            _projectModuleFilesService.DeleteProjectModuleFile(fileEntity);
                            break;
                        }
                    case "Project Task":
                        {
                            // Fetch the file entity by its ID
                            var fileEntity = await _projectTaskFilesService.GetProjectTaskFileByFileId(id);
                            // If no file is found, return a BadRequest response with an error message
                            if (fileEntity == null)
                                return BadRequest(new BadRequestError("No file found with the specified id."));

                            // Delete the file using the project service
                            _projectTaskFilesService.DeleteProjectTaskFile(fileEntity);
                            break;
                        }
                    case "Project Activity":
                        {
                            // Fetch the file entity by its ID
                            var fileEntity = await _projectActivityFilesService.GetProjectActivityFileByFileId(id);
                            // If no file is found, return a BadRequest response with an error message
                            if (fileEntity == null)
                                return BadRequest(new BadRequestError("No file found with the specified id."));

                            // Delete the file using the project service
                            _projectActivityFilesService.DeleteProjectActivityFile(fileEntity);
                            break;
                        }
                    case "Help Desk":
                        {
                            // Fetch the file entity by its ID
                            var fileEntity = await _helpDeskFilesService.GetHelpDeskFileByFileId(id);
                            // If no file is found, return a BadRequest response with an error message
                            if (fileEntity == null)
                                return BadRequest(new BadRequestError("No file found with the specified id."));

                            // Delete the file using the project service
                            _helpDeskFilesService.DeleteHelpDeskFile(fileEntity);
                            break;
                        }
                    default:
                        return BadRequest(new BadRequestError("Invalid type."));
                }
                // Fetch the file entity by its ID
                var entity = await _commonService.GetByPictureId(id);
                // If no file is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No file found with the specified id."));

                // Delete the file using the project service
                _commonService.DeletePicture(entity);


                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Project Messages

        #region GetmessagesByProjectId
        // Title: GetmessagesByProjectId
        // Description: This endpoint retrieves the details of a specific project messages based on its unique identifier (ID). 
        [HttpGet("projectmessages/list/{id}")]
        public async Task<IActionResult> GetmessagesByProjectId(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _projectMessageService.GetProjectMessagesByProjectId(SiteId, id, LoggedUserId);
                var model = _mapper.Map<List<ProjectsMessagesModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region SentMessage
        // Title: SentMessage
        [HttpPost("SentMessage")]
        public async Task<IActionResult> SentMessage(ProjectsMessagesModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Map the project model to the project message entity
                    var entity = _mapper.Map<ProjectsMessages>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    // Set custom properties
                    entity.SiteId = SiteId;

                    // Set the created by and created on properties
                    entity.ProjectId = model.ProjectId;
                    entity.ParentMessageId = entity.Id;
                    entity.SentBy = LoggedUserId;
                    entity.SentDate = GetDateTime;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _projectMessageService.InsertProjectMessages(entity);


                    if (model.EmployeeIds.Any())
                    {
                        foreach (var employee in model.EmployeeIds)
                        {
                            var userdata = _commonService.GetLoggeduserIdByEmployeeId(SiteId, employee);
                            var MasterNotificationData = await _masterNotificationService.GetMasterNotificationByNumber(SiteId, "ProjectMessage1", userdata);
                            if (MasterNotificationData != null)
                            {
                                var projectData = await _projectService.GetById(model.ProjectId);
                                string message = MasterNotificationData.Message.Replace("[Project Name]", projectData.Name);
                                var Notification = _notificationService.AddNotification(SiteId, MasterNotificationData.Title, message, MasterNotificationData.Type, LoggedUserId, entity.Id, "/project", userdata, LoggedUserId, GetDateTime);
                            }

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

        #region UpdateMessage
        // Title: UpdateMessage
        [HttpPost("UpdateMessage/{id}")]
        public async Task<IActionResult> UpdateMessage(string id, ProjectsMessagesModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the project entity by its ID
                    var entity = await _projectMessageService.GetProjectMessagesById(id);

                    // If no project is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No message found with the specified id."));

                    // Set the user who updated the project and the current UTC time for tracking purposes
                    entity.Message = model.Message;
                    entity.Reaction = model.Reaction;
                    entity.ProjectId = model.ProjectId;

                    // Set the updated by and updated on properties
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _projectMessageService.UpdateProjectMessages(entity);

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

        #region DeleteMessage
        // Title: DeleteMessage
        [HttpDelete("DeleteMessage/{id}")]
        public async Task<IActionResult> DeleteMessage(string id)
        {
            try
            {
                // Fetch the project entity by its ID
                var entity = await _projectMessageService.GetProjectMessagesById(id);
                // If no project is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No message found with the specified id."));

                // Delete the project using the project service
                _projectMessageService.DeleteProjectMessages(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetProjectCharterEmployeesWithWeeklyPlanHoursByProjectId
        [HttpGet("ProjectCharterEmployeesWithWeeklyPlanHours/list")]
        public async Task<IActionResult> GetProjectCharterEmployeesWithWeeklyPlanHoursByProjectId(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var list = await _projectEmployeeMappingService.GetProjectCharterEmployeesWithWeeklyPlanHoursByProjectId(id, GetDateTime);
                var model = _mapper.Map<List<ProjectEmployeeMappingModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #endregion

        #region GetProjectEmployeesByProjectId
        [HttpGet("ProjectEmployees/list")]
        public async Task<IActionResult> GetProjectEmployeesByProjectId(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var list = await _projectEmployeeMappingService.GetProjectCharterEmployeeByProjectId(id);
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Copy Project As Template
        [HttpPost("copy-project-as-template")]
        public async Task<IActionResult> CopyProjectAsTemplate(ProjectModel model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var projectData = await _projectService.GetAllProjectDataByProjectId(SiteId, model.Id);
                if (projectData != null)
                {
                    projectData.Name = model.Name;
                    projectData.NewStartDate = !model.IsTemplate ? model.StartDate : projectData.StartDate;

                    string ActionType = model.IsTemplate ? "Project To Template" : "Template To Project";
                    string newProjectId = await AddProject(SiteId, ActionType, false, projectData, LoggedUserId, GetDateTime);

                    // Copy Project Employee Mapping
                    if (projectData.ProjectEmployeeMappings != null && projectData.ProjectEmployeeMappings.Any())
                    {
                        foreach (var mapping in projectData.ProjectEmployeeMappings)
                        {
                            AddProjectEmployee(newProjectId, mapping.EmployeeId, mapping.EmployeeDesignationId, mapping.ProductivityFactor, mapping.ProjectUserMappingId, LoggedUserId, GetDateTime, mapping.Deleted);
                        }
                    }

                    if (projectData.ProjectModules.Any())
                    {
                        foreach (var projectListModel in projectData.ProjectModules)
                        {
                            string newProjectListId = await AddProjectModule(SiteId, newProjectId, ActionType, projectData.StartDate, projectData.NewStartDate.Value, projectListModel, LoggedUserId, GetDateTime);
                            if (projectListModel.ProjectTasks.Any())
                            {
                                foreach (var taskListModel in projectListModel.ProjectTasks)
                                {
                                    string newTaskId = await AddProjectTask(SiteId, newProjectId, newProjectListId, ActionType, projectData.StartDate, projectData.NewStartDate.Value, taskListModel, LoggedUserId, GetDateTime);
                                    if (taskListModel.ProjectActivities.Count() > 0)
                                    {
                                        foreach (var activityListModel in taskListModel.ProjectActivities)
                                        {
                                            bool isActive = activityListModel.Active;
                                            var newActivityId = AddProjectTaskActivity(SiteId, newProjectId, newProjectListId, newTaskId, activityListModel, LoggedUserId, GetDateTime, isActive);
                                        }
                                    }

                                    var existingTagList = _projectTaskTagService.GetProjectTaskTagsByTaskIdAndUserId(SiteId, taskListModel.Id, LoggedUserId);
                                    if (existingTagList.Any())
                                    {
                                        foreach (var tag in existingTagList)
                                        {
                                            AddProjectTaskTags(newTaskId, tag, LoggedUserId, GetDateTime);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Project Weekly/Monthly Plan

        #region Get All List
        [HttpPost("get-project-weekly-plan")]
        public async Task<IActionResult> GetProjectWeeklyPlanAsync(ProjectWeeklyPlanSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                int Status = 2;
                if (!string.IsNullOrWhiteSpace(searchModel.StatusId))
                {
                    var activeStatus = _dropDownService.GetDropDownById(searchModel.StatusId).GetAwaiter().GetResult();
                    Status = activeStatus.DropDownValue == "Active" ? 1 : (activeStatus.DropDownValue == "Inactive" ? 0 : 2);
                }

                var List = await _projectWeeklyService.GetAllProjectWeeklyPlanListAsync(
                    SiteId,
                    LoggedUserId,
                    GetDateTime,
                    searchModel.PlanTypeId,
                    searchModel.SearchText,
                    searchModel.ProjectIds,
                    searchModel.ProjectCoordinatorIds,
                    searchModel.ProjectLeadsIds,
                    searchModel.ProjectStatusIds,
                    Status,
                    searchModel.ProjectPriorityIds,
                    searchModel.ProjectTypeIds,
                    searchModel.CustomerIds,
                    searchModel.CompanyContactIds,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var Data = new ProjectWeeklyPlanList
                {
                    WeeklyPlanList = List,
                    Total = List.TotalCount
                };

                return Ok(Data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost("get-project-weekly-plan-details")]
        public async Task<IActionResult> GetProjectWeeklyPlanAsync(string projectId, string planTypeId, int skipIndex = 0, int takeCount = 4, DateTime? weekEndDate = null)
        {
            try
            {
                var Data = await _projectWeeklyDatesService.GetProjectWeeklyPlanDatesByProjectId(projectId, planTypeId, skipIndex, takeCount, weekEndDate);
                if (Data != null)
                    return Ok(Data);
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost("get-project-weekly-type-id")]
        public async Task<IActionResult> GetProjectWeeklyTypeIdAsync(string type, string value)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            var Id = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, type, value);
            return Ok(Id);
        }

        [HttpPost("get-all-employees-esthrs-as-dropdown-list")]
        public async Task<IActionResult> GetAllEmployeesEstHrsAsDropdownListAsync(Guid projectId, Guid planTypeId, DateTime weekDate)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var List = await _context.EmployeeEstimatedHoursDropdownList.FromSqlRaw("EXEC GetAllEmployeesWithHoursForWeeklyMonthlyPlanning @ProjectId = {0}, @PlanTypeId = {1}, @WeekDate = {2}, @SiteId = {3}", projectId, planTypeId, weekDate, SiteId).ToListAsync();
                return Ok(List);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }

        }

        [HttpPost("get-resource-summary-for-weekPlan-by-id")]
        public async Task<IActionResult> GetResourceSummaryForWeekPlanByIdAsync(string planTypeId, string planDateId)
        {
            try
            {
                var List = await _projectWeeklyDatesService.GetEmployeeHourSummaryByWeekPlanId(planTypeId, planDateId);
                return Ok(List);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region Add Plan Approver To Project & Approve This Week/Month Plan && Add Plan Completion Percentage
        [HttpPost("add-Project-Plan-Approver")]
        public async Task<IActionResult> AddProjectPlanApproverAsync(string projectId, string planApproverId)
        {
            try
            {
                var entity = await _projectService.GetById(projectId);
                if (entity == null)
                    return BadRequest("Project Id Not Found");

                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                entity.PlanApproverId = planApproverId;
                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;

                _projectService.UpdateProject(entity);
                _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Projects", entity.Id, entity.Name, entity.Id, entity.Name, "Plan Approver", entity.PlanApproverId, LoggedUserId, GetDateTime);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost("approve-this-Plan")]
        public async Task<IActionResult> ApproveThisPlanAsync(string planDateId, bool isLock)
        {
            try
            {
                var entity = await _projectWeeklyDatesService.GetById(planDateId);
                if (entity == null)
                    return BadRequest("Plan Id Not Found");

                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                entity.IsApproved = isLock;
                entity.ApprovedById = isLock ? LoggedUserId : null;
                entity.ApprovedOnUtc = isLock ? GetDateTime : null;

                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;

                _projectWeeklyDatesService.UpdateProjectWeeklyPlanDates(entity);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost("add-Plan-Completion-Percentage")]
        public async Task<IActionResult> AddPlanCompletionPercentageAsync(string planDateId, int completionPercentage)
        {
            try
            {
                var entity = await _projectWeeklyDatesService.GetById(planDateId);
                if (entity == null)
                    return BadRequest("Plan Id Not Found");

                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                
                entity.IsCompleted = true;
                entity.CompletionPercentage = completionPercentage;
                entity.CompletedById = LoggedUserId;
                entity.CompletedOnUtc = GetDateTime;

                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;

                _projectWeeklyDatesService.UpdateProjectWeeklyPlanDates(entity);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region Add Project Plan
        [HttpPost("add-project-to-weekly-plan")]
        public async Task<IActionResult> AddProjectToWeeklyPlan(SaveProjectWeeklyPlan model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                // Get Weekly Plan Data
                var WeeklyProjectData = await _projectWeeklyService.GetByProjectId(SiteId, model.ProjectId);

                // Update Weekly Project
                if (WeeklyProjectData != null)
                {
                    WeeklyProjectData.UpdatedById = LoggedUserId;
                    WeeklyProjectData.UpdatedOnUtc = GetDateTime;
                    _projectWeeklyService.UpdateProjectWeeklyPlan(WeeklyProjectData);
                }
                // Add Weekly Project
                else
                {
                    WeeklyProjectData = new ProjectWeeklyPlan();
                    WeeklyProjectData.SiteId = SiteId;
                    WeeklyProjectData.ProjectId = model.ProjectId;
                    WeeklyProjectData.CreatedById = LoggedUserId;
                    WeeklyProjectData.CreatedOnUtc = GetDateTime;
                    WeeklyProjectData.UpdatedById = LoggedUserId;
                    WeeklyProjectData.UpdatedOnUtc = GetDateTime;
                    _projectWeeklyService.InsertProjectWeeklyPlan(WeeklyProjectData);
                }

                // Project Weekly Plan Date
                var WeeklyDate = new ProjectWeeklyPlanDates();
                if (WeeklyProjectData.ProjectWeeklyPlanDates != null)
                {
                    // Check If date already extist
                    WeeklyDate = WeeklyProjectData.ProjectWeeklyPlanDates.FirstOrDefault(m => m.WeekDate == Convert.ToDateTime(model.WeekDate));
                    if (WeeklyDate != null)
                    {
                        WeeklyDate.UpdatedById = LoggedUserId;
                        WeeklyDate.UpdatedOnUtc = GetDateTime;
                        _projectWeeklyDatesService.UpdateProjectWeeklyPlanDates(WeeklyDate);
                    }
                    // Add new date if not found
                    else
                    {
                        WeeklyDate = new ProjectWeeklyPlanDates();
                        WeeklyDate.ProjectWeeklyPlanId = WeeklyProjectData.Id;
                        WeeklyDate.PlanTypeId = model.PlanTypeId;
                        WeeklyDate.WeekDate = Convert.ToDateTime(model.WeekDate);
                        WeeklyDate.CreatedById = LoggedUserId;
                        WeeklyDate.CreatedOnUtc = GetDateTime;
                        WeeklyDate.UpdatedById = LoggedUserId;
                        WeeklyDate.UpdatedOnUtc = GetDateTime;
                        _projectWeeklyDatesService.InsertProjectWeeklyPlanDates(WeeklyDate);
                    }
                }

                // Project Weekly Date Lines
                if (model.weekDateLines.Count() > 0)
                {
                    //Add Lines
                    foreach (var line in model.weekDateLines)
                    {
                        var LineData = new ProjectWeeklyPlanDatesLines();

                        LineData.ProjectWeeklyPlanDatesId = WeeklyDate.Id;

                        if (!string.IsNullOrEmpty(line.Description))
                        {
                            LineData.ExpectedDescription = await _azureBlobImageServices
                                .ProcessHtmlAndManageImagesAsync(
                                    line.Description,
                                    SiteData.Name,
                                    "project-weeklymonthly",
                                    LineData.Id
                                );
                        }

                        LineData.ExpectedHours = line.ExpectedHours;
                        LineData.ExpectedDescriptionCreatedById = LoggedUserId;
                        LineData.ExpectedDescriptionCreatedOnUtc = GetDateTime;
                        LineData.ExpectedDescriptionUpdatedById = LoggedUserId;
                        LineData.ExpectedDescriptionUpdatedOnUtc = GetDateTime;
                        LineData.ActualDescription = "";

                        _projectWeeklyDatesLinesService.InsertProjectWeeklyPlanDatesLines(LineData);

                        if (line.saveWeeklyLinesAssignTos.Any())
                        {
                            foreach (var item in line.saveWeeklyLinesAssignTos)
                            {
                                var AssignTo = new ProjectWeeklyPlanDatesLinesAssignedTo()
                                {
                                    ProjectWeeklyPlanDatesLineId = LineData.Id,
                                    EmployeeId = item.EmployeeId,
                                    EstimatedHours = item.EstimatedHours,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime,
                                };
                                _projectWeeklyPlanDatesLinesAssignedToService.InsertProjectWeeklyPlanDatesLinesAssignedTo(AssignTo);
                            }
                        }
                    }
                }
                return Ok(WeeklyProjectData.Id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region Add Project Plan Week
        [HttpPost("add-project-weeklyplan-date")]
        public async Task<IActionResult> AddProjectWeeklyPlanDates(ProjectWeeklyPlanDates model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var PlanData = await _projectWeeklyService.GetById(SiteId, model.ProjectWeeklyPlanId);
                PlanData.UpdatedById = LoggedUserId;
                PlanData.UpdatedOnUtc = GetDateTime;
                _projectWeeklyService.UpdateProjectWeeklyPlan(PlanData);

                var Data = new ProjectWeeklyPlanDates();
                Data.ProjectWeeklyPlanId = model.ProjectWeeklyPlanId;
                Data.PlanTypeId = model.PlanTypeId;

                Data.WeekDate = model.WeekDate;

                Data.CreatedById = LoggedUserId;
                Data.CreatedOnUtc = GetDateTime;
                Data.UpdatedById = LoggedUserId;
                Data.UpdatedOnUtc = GetDateTime;

                _projectWeeklyDatesService.InsertProjectWeeklyPlanDates(Data);

                var WeekLine = new ProjectWeeklyPlanDatesLines();
                WeekLine.ProjectWeeklyPlanDatesId = Data.Id;
                WeekLine.ExpectedDescription = "";
                WeekLine.ExpectedDescriptionCreatedById = LoggedUserId;
                WeekLine.ExpectedDescriptionCreatedOnUtc = GetDateTime;
                WeekLine.ExpectedDescriptionUpdatedById = LoggedUserId;
                WeekLine.ExpectedDescriptionUpdatedOnUtc = GetDateTime;
                _projectWeeklyDatesLinesService.InsertProjectWeeklyPlanDatesLines(WeekLine);

                var newData = await _projectWeeklyDatesService.GetByIdInDetail(Data.Id);
                return Ok(newData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost("link-RequirementTaskIssue-To-WeeklyPlan-Date")]
        public async Task<IActionResult> LinkRequirementTaskIssueToWeeklyPlanDateAsync(LinkReqTaskIssueToDate model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                string ProjectWeeklyPlanDateId = await _projectWeeklyDatesService.CheckIfProjectWeeklyPlanIsCreated(model.ProjectId, model.PlanTypeId, model.Date);

                if (string.IsNullOrEmpty(ProjectWeeklyPlanDateId))
                {
                    var ProjectWeeklyPlan = await _projectWeeklyService.GetByProjectId(SiteId, model.ProjectId);
                    var newWeekDatemodel = new ProjectWeeklyPlanDates();

                    newWeekDatemodel.ProjectWeeklyPlanId = ProjectWeeklyPlan.Id;
                    newWeekDatemodel.WeekDate = model.Date;
                    newWeekDatemodel.PlanTypeId = model.PlanTypeId;

                    newWeekDatemodel.CreatedById = LoggedUserId;
                    newWeekDatemodel.CreatedOnUtc = GetDateTime;
                    newWeekDatemodel.UpdatedById = LoggedUserId;
                    newWeekDatemodel.UpdatedOnUtc = GetDateTime;
                    _projectWeeklyDatesService.InsertProjectWeeklyPlanDates(newWeekDatemodel);

                    ProjectWeeklyPlanDateId = newWeekDatemodel.Id;
                }

                if (!string.IsNullOrEmpty(ProjectWeeklyPlanDateId) && model.Ids.Any())
                {
                    var GetAllExistingMappings = await _projectWeeklyPlanDatesReqTaskIssueMappingService.GetAllByProjectWeeklyPlanDatesId(ProjectWeeklyPlanDateId);
                    foreach (var id in model.Ids)
                    {
                        if (model.Type == "Requirements" && !GetAllExistingMappings.Any(m => m.RequirementId == id))
                        {
                            var newMapping = new ProjectWeeklyPlanDatesReqTaskIssueMapping();
                            newMapping.ProjectWeeklyPlanDatesId = ProjectWeeklyPlanDateId;
                            newMapping.RequirementId = id;
                            newMapping.CreatedById = LoggedUserId;
                            newMapping.CreatedOnUtc = GetDateTime;
                            _projectWeeklyPlanDatesReqTaskIssueMappingService.InsertProjectWeeklyPlanDatesReqTaskIssue(newMapping);
                        }

                        if (model.Type == "Project Tasks" && !GetAllExistingMappings.Any(m => m.TaskId == id))
                        {
                            var newMapping = new ProjectWeeklyPlanDatesReqTaskIssueMapping();
                            newMapping.ProjectWeeklyPlanDatesId = ProjectWeeklyPlanDateId;
                            newMapping.TaskId = id;
                            newMapping.CreatedById = LoggedUserId;
                            newMapping.CreatedOnUtc = GetDateTime;
                            _projectWeeklyPlanDatesReqTaskIssueMappingService.InsertProjectWeeklyPlanDatesReqTaskIssue(newMapping);
                        }

                        if (model.Type == "Issues" && !GetAllExistingMappings.Any(m => m.IssueId == id))
                        {
                            var newMapping = new ProjectWeeklyPlanDatesReqTaskIssueMapping();
                            newMapping.ProjectWeeklyPlanDatesId = ProjectWeeklyPlanDateId;
                            newMapping.IssueId = id;
                            newMapping.CreatedById = LoggedUserId;
                            newMapping.CreatedOnUtc = GetDateTime;
                            _projectWeeklyPlanDatesReqTaskIssueMappingService.InsertProjectWeeklyPlanDatesReqTaskIssue(newMapping);
                        }
                    }
                    return Ok();
                }
                else
                    return BadRequest("Something Went Wrong....");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost("delete-RequirementTaskIssue-To-WeeklyPlan-Date")]
        public async Task<IActionResult> DeleteRequirementTaskIssueToWeeklyPlanDateAsync(string MappingId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var Data = await _projectWeeklyPlanDatesReqTaskIssueMappingService.GetById(MappingId);
                Data.Deleted = true;
                _projectWeeklyPlanDatesReqTaskIssueMappingService.UpdateProjectWeeklyPlanDatesReqTaskIssue(Data);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region Save & Delete Week Lines
        // Insert
        [HttpPost("save-project-weeklyplan-date-line")]
        public async Task<IActionResult> SaveProjectWeeklyPlanDatesLine(ProjectWeeklyPlanDatesLines model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                // Update Weekly Plan
                var weeklyDate = await _projectWeeklyDatesService.GetById(model.ProjectWeeklyPlanDatesId);
                weeklyDate.UpdatedById = LoggedUserId;
                weeklyDate.UpdatedOnUtc = GetDateTime;
                _projectWeeklyDatesService.UpdateProjectWeeklyPlanDates(weeklyDate);

                var line = await _projectWeeklyDatesLinesService.GetById(model.Id);
                bool isNew = line == null;

                line ??= new ProjectWeeklyPlanDatesLines
                {
                    Id = model.Id,
                    ProjectWeeklyPlanDatesId = model.ProjectWeeklyPlanDatesId,
                    ExpectedDescriptionCreatedById = LoggedUserId,
                    ExpectedDescriptionCreatedOnUtc = GetDateTime
                };

                if (model.IsEditExpectedDescription)
                {
                    if (!string.IsNullOrEmpty(model.ExpectedDescription))
                    {
                        line.ExpectedDescription = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.ExpectedDescription,
                                SiteData.Name,
                                "project-weeklymonthly",
                                model.Id,
                                line.ExpectedDescription
                            );
                    }
                    line.ExpectedHours = model.ExpectedHours;
                    line.ExpectedDescriptionUpdatedById = LoggedUserId;
                    line.ExpectedDescriptionUpdatedOnUtc = GetDateTime;
                }

                if (model.IsEditActualDescription)
                {
                    if (string.IsNullOrEmpty(line.ActualDescription))
                    {
                        line.ActualDescriptionCreatedById = LoggedUserId;
                        line.ActualDescriptionCreatedOnUtc = GetDateTime;
                    }

                    if (!string.IsNullOrEmpty(model.ActualDescription))
                    {
                        line.ActualDescription = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.ActualDescription,
                                SiteData.Name,
                                "project-weeklymonthly",
                                model.Id,
                                line.ActualDescription
                            );
                    }

                    line.ActualDescriptionUpdatedById = LoggedUserId;
                    line.ActualDescriptionUpdatedOnUtc = GetDateTime;
                }

                if (isNew)
                    _projectWeeklyDatesLinesService.InsertProjectWeeklyPlanDatesLines(line);
                else
                    _projectWeeklyDatesLinesService.UpdateProjectWeeklyPlanDatesLines(line);

                var result = await _projectWeeklyDatesLinesService.GetInDetailById(line.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message} :- {ex.InnerException?.Message}");
            }
        }

        [HttpPost("delete-project-weeklyplan-date-line")]
        public async Task<IActionResult> DeleteProjectWeeklyPlanDatesLine(ProjectWeeklyPlanDatesLines model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var Data = await _projectWeeklyDatesLinesService.GetById(model.Id);
                if (Data != null)
                {
                    Data.Deleted = true;
                    Data.DeletedById = LoggedUserId;
                    Data.DeletedOnUtc = GetDateTime;

                    _projectWeeklyDatesLinesService.UpdateProjectWeeklyPlanDatesLines(Data);
                }
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost("delete-project-weeklyplan-date-line-assignTo")]
        public async Task<IActionResult> DeleteProjectWeeklyPlanDatesLineAssignTo(ProjectWeeklyPlanDatesLinesAssignedTo model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var Data = await _projectWeeklyPlanDatesLinesAssignedToService.GetById(model.Id);
                if (Data != null)
                {
                    Data.Deleted = true;
                    Data.CreatedById = LoggedUserId;
                    Data.CreatedOnUtc = GetDateTime;

                    _projectWeeklyPlanDatesLinesAssignedToService.UpdateProjectWeeklyPlanDatesLinesAssignedTo(Data);
                }
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region Add & Delete & Validate Resource To Week Lines
        [HttpPost("add-project-weeklyplan-date-line-resource")]
        public async Task<IActionResult> AddProjectWeeklyPlanDateLineResource(SaveWeeklyLinesAssignTo model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var ResourceData = await _projectWeeklyPlanDatesLinesAssignedToService.GetById(model.Id);
                if (ResourceData != null)
                {
                    ResourceData.EmployeeId = model.EmployeeId;
                    ResourceData.EstimatedHours = model.EstimatedHours;
                    ResourceData.CreatedById = LoggedUserId;
                    ResourceData.CreatedOnUtc = GetDateTime;
                    _projectWeeklyPlanDatesLinesAssignedToService.UpdateProjectWeeklyPlanDatesLinesAssignedTo(ResourceData);

                    var RefreshResourceData = await _projectWeeklyPlanDatesLinesAssignedToService.GetByIdInDetail(ResourceData.Id);
                    return Ok(RefreshResourceData);
                }

                var NewResource = new ProjectWeeklyPlanDatesLinesAssignedTo();
                NewResource.ProjectWeeklyPlanDatesLineId = model.ProjectWeeklyPlanDatesLineId;
                NewResource.EmployeeId = model.EmployeeId;
                NewResource.EstimatedHours = model.EstimatedHours;
                NewResource.CreatedById = LoggedUserId;
                NewResource.CreatedOnUtc = GetDateTime;
                _projectWeeklyPlanDatesLinesAssignedToService.UpdateProjectWeeklyPlanDatesLinesAssignedTo(NewResource);

                var NewResourceData = await _projectWeeklyPlanDatesLinesAssignedToService.GetByIdInDetail(NewResource.Id);
                return Ok(NewResourceData);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost("delete-project-weeklyplan-date-line-resource")]
        public async Task<IActionResult> DeleteProjectWeeklyPlanDateLineResource(string Id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var ResourceData = await _projectWeeklyPlanDatesLinesAssignedToService.GetById(Id);
                if (ResourceData != null)
                {
                    ResourceData.Deleted = true;
                    ResourceData.CreatedById = LoggedUserId;
                    ResourceData.CreatedOnUtc = GetDateTime;
                    _projectWeeklyPlanDatesLinesAssignedToService.UpdateProjectWeeklyPlanDatesLinesAssignedTo(ResourceData);
                }
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost("check-if-employee-already-exists-in-plan-line")]
        public async Task<IActionResult> CheckIfEmployeeAlreadyExistsInPlanLine(string Id, string EmployeeId)
        {
            try
            {
                bool IfExists = await _projectWeeklyPlanDatesLinesAssignedToService.CheckIfEmployeeIdExistsInWeeklyPlanLine(Id, EmployeeId);
                return Ok(IfExists);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #endregion

        #region Move Project Module as New Project
        [HttpPost("move-module-as-project-with-data")]
        public async Task<IActionResult> MoveProjectModuleAsProjectWithData(MoveProjectModuleAsProject model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Get Existing Module
                    var module = await _projectModuleService.GetProjectModuleById(model.Id);
                    if (module == null)
                        return NotFound("Project module not found");

                    // Check if there is any project with the same name and customer ID that is not marked as deleted and has a different ID
                    var exists = await _projectService.GetProjectName(SiteId, model.Name);

                    // If project exists, return a bad request with an error message
                    if (exists != null)
                        return BadRequest(new BadRequestError("Project name already exists, try with another."));

                    var StatusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Project Status", "New");
                    var PriorityId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Project Priorities", "Medium");

                    // Create New Project
                    var projectEntity = new Project();
                    projectEntity.Id = Guid.NewGuid().ToString();
                    projectEntity.SiteId = SiteId;
                    projectEntity.Name = model.Name;
                    projectEntity.Description = module.Description;
                    projectEntity.StartDate = module.StartDate;
                    projectEntity.ProjectStatusId = StatusId;
                    projectEntity.ProjectPriorityId = PriorityId;
                    projectEntity.Active = true;

                    if (!string.IsNullOrEmpty(module.Description))
                    {
                        projectEntity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                module.Description,
                                SiteData.Name,
                                "project",
                                projectEntity.Id
                            );
                    }

                    // Set the created by and created on properties
                    projectEntity.CreatedById = LoggedUserId;
                    projectEntity.UpdatedById = LoggedUserId;
                    projectEntity.CreatedOnUtc = GetDateTime;
                    projectEntity.UpdatedOnUtc = GetDateTime;
                    _projectService.InsertProject(projectEntity);

                    // Insert data into the Charter
                    var projectCharterData = _projectEmployeeMappingService.GetProjectEmployeeByProjectId(module.ProjectId);
                    if (projectCharterData != null && projectCharterData.Count > 0)
                    {
                        foreach (var charter in projectCharterData)
                        {
                            AddProjectEmployee(projectEntity.Id, charter.EmployeeId, charter.EmployeeDesignationId, charter.ProductivityFactor, charter.ProjectUserMappingId, LoggedUserId, GetDateTime, charter.Deleted);
                        }
                    }

                    // Insert data into project security
                    var projectUsersData = await _projectUserMappingService.GetProjectUserByProjectId(SiteId, module.ProjectId);
                    if(projectUsersData != null && projectUsersData.Count > 0)
                    {
                        foreach (var user in projectUsersData)
                        {
                            var ProjectUserMapping = new ProjectUserMapping();
                            ProjectUserMapping.Id = Guid.NewGuid().ToString();
                            ProjectUserMapping.ProjectId = projectEntity.Id;
                            ProjectUserMapping.AspNetUserId = user.AspNetUserId;
                            ProjectUserMapping.FullAccess = user.FullAccess;
                            ProjectUserMapping.ViewOnly = user.ViewOnly;
                            ProjectUserMapping.Notes = user.Notes;

                            ProjectUserMapping.CreatedById = LoggedUserId;
                            ProjectUserMapping.CreatedOnUtc = GetDateTime;
                            ProjectUserMapping.Deleted = user.Deleted;
                            _projectUserMappingService.InsertProjectUser(ProjectUserMapping);
                        }
                    }

                    // Update Project module
                    module.MovedFromProjectId = module.ProjectId;
                    module.ProjectId = projectEntity.Id;
                    module.UpdatedById = LoggedUserId;
                    module.UpdatedOnUtc = GetDateTime;

                    _projectModuleService.UpdateProjectModule(module);

                    // Update Project Tasks
                    var tasksList = await _projectTaskService.GetAllTaskByProjectModuleIdForMoveModuleAsProject(module.Id);
                    if (tasksList != null && tasksList.Count > 0)
                    {
                        foreach (var task in tasksList)
                        {
                            var taskEntity = await _projectTaskService.GetById(task.Id);
                            if (taskEntity != null)
                            {
                                taskEntity.ProjectId = projectEntity.Id;
                                taskEntity.UpdatedById = LoggedUserId;
                                taskEntity.UpdatedOnUtc = GetDateTime;
                                _projectTaskService.UpdateProjectTask(taskEntity);
                            }
                        }
                    }

                    // Update Task Activities
                    var activitiesList = await _projectActivityService.GetAllProjectActivitiesByModuleIdForMoveModuleAsProject(module.Id);
                    if (activitiesList != null && activitiesList.Count > 0)
                    {
                        foreach (var activity in activitiesList)
                        {
                            var activityEntity = await _projectActivityService.GetById(activity.Id);
                            if (activityEntity != null)
                            {
                                activityEntity.ProjectId = projectEntity.Id;
                                activityEntity.ProjectModuleId = model.Id;
                                activityEntity.SiteId = SiteId;
                                activityEntity.Active = true;
                                activityEntity.UpdatedById = LoggedUserId;
                                activityEntity.UpdatedOnUtc = GetDateTime;

                                _projectActivityService.UpdateProjectActivity(activityEntity);
                            }
                        }
                    }

                    // Update Daily Planner
                    var dailyPlannersList = await _dailyPlannerLineService.GetAllDailyPlannerLineByProjectModuleIdForMoveModuleAsProject(module.Id);
                    if (dailyPlannersList != null && dailyPlannersList.Count > 0)
                    {
                        foreach (var planner in dailyPlannersList)
                        {
                            var plannerEntity = await _dailyPlannerLineService.GetDailyPlannerLineById(planner.Id);
                            if (plannerEntity != null)
                            {
                                plannerEntity.ProjectId = projectEntity.Id;
                                plannerEntity.UpdatedById = LoggedUserId;
                                plannerEntity.UpdatedOnUtc = GetDateTime;
                                _dailyPlannerLineService.UpdateDailyPlannerLine(plannerEntity);
                            }
                        }
                    }

                    // Update Timesheets
                    var timesheetsList = await _timesheetLinesService.GetTimesheetLinesByProjectModuleIdForMoveModuleAsProject(module.Id);
                    if (timesheetsList != null && timesheetsList.Count > 0)
                    {
                        foreach (var timesheet in timesheetsList)
                        {
                            var timesheetEntity = await _timesheetLinesService.GetTimesheetLinesById(timesheet.Id);
                            if (timesheetEntity != null)
                            {
                                timesheetEntity.ProjectId = projectEntity.Id;
                                timesheetEntity.UpdatedById = LoggedUserId;
                                timesheetEntity.UpdatedOnUtc = GetDateTime;
                                _timesheetLinesService.UpdateTimesheetLines(timesheetEntity);
                            }
                        }
                    }
                    return Ok();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Functions:- GetUniqueFileName
        private string GetUniqueFileName(string uploadDir, string fileName)
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string finalName = fileName;
            int counter = 1;

            while (System.IO.File.Exists(Path.Combine(uploadDir, finalName)))
            {
                if (counter == 1)
                    finalName = $"{fileNameWithoutExt} - copy{extension}";
                else
                    finalName = $"{fileNameWithoutExt} - copy({counter}){extension}";
                counter++;
            }

            return finalName;
        }
        #endregion

        #region Private Function For Project & Sub-Modules
        private async Task<string> AddProject(string SiteId, string ActionType, bool AddDefaultSwimLane, Project model, string LoggedUserId, DateTime GetDateTime)
        {
            var newProjectCopy = new Project();
            if (ActionType == "Template To Project")
            {
                //Check if the project already exists
                var existingProject = await _projectService.GetProjectByTemplate(SiteId, model.Name, model.CustomerId, false);

                if (existingProject != null)
                {
                    int counter = 1;
                    string baseName = model.Name;
                    string newName;

                    do
                    {
                        newName = $"{baseName} - copy ({counter})";
                        var checkName = await _projectService.GetProjectByTemplate(SiteId, newName, model.CustomerId, false);
                        if (checkName == null)
                            break;

                        counter++;
                    } while (true);

                    newProjectCopy.Name = newName;
                }
                else
                {
                    newProjectCopy.Name = model.Name;
                }
            }
            else if (ActionType == "Project To Template")
            {
                //Check if the project already exists
                var existingProject = await _projectService.GetProjectByTemplate(SiteId, model.Name, model.CustomerId, true);

                if (existingProject != null)
                {
                    int counter = 1;
                    string baseName = model.Name;
                    string newName;

                    do
                    {
                        newName = $"{baseName} - copy ({counter})";
                        var checkName = await _projectService.GetProjectByTemplate(SiteId, newName, model.CustomerId, true);
                        if (checkName == null)
                            break;

                        counter++;
                    } while (true);

                    newProjectCopy.Name = newName;
                }
                else
                {
                    newProjectCopy.Name = model.Name;
                }

            }
            else
            {
                newProjectCopy.Name = model.Name;
            }

            newProjectCopy.SiteId = SiteId;
            newProjectCopy.CustomerId = model.CustomerId;
            newProjectCopy.ProjectCategoryId = model.ProjectCategoryId;
            newProjectCopy.ProjectSubcategoryId = model.ProjectSubcategoryId;
            newProjectCopy.ProjectTypeId = model.ProjectTypeId;
            newProjectCopy.ProjectPriorityId = model.ProjectPriorityId;
            newProjectCopy.ProjectStatusId = model.ProjectStatusId;
            newProjectCopy.ProjectCoordinatorId = model.ProjectCoordinatorId;

            newProjectCopy.Year = model.Year;
            newProjectCopy.Description = model.Description;
            newProjectCopy.Website = model.Website;

            newProjectCopy.StartDate = ActionType == "Template To Project" ? model.NewStartDate : model.StartDate;
            newProjectCopy.EndDate = ActionType == "Template To Project" ? model.NewStartDate : model.EndDate;
            newProjectCopy.GoLiveDate = ActionType == "Template To Project" ? model.NewStartDate : model.GoLiveDate;

            newProjectCopy.IsTemplate = ActionType == "Project To Template" ? true : false;
            newProjectCopy.IsFrom = ActionType != "Create" ? model.Id : "";

            newProjectCopy.Active = true;
            newProjectCopy.CreatedById = LoggedUserId;
            newProjectCopy.UpdatedById = LoggedUserId;
            newProjectCopy.CreatedOnUtc = GetDateTime;
            newProjectCopy.UpdatedOnUtc = GetDateTime;
            _projectService.InsertProject(newProjectCopy);

            ProjectUserMapping ProjectUserMapping = new ProjectUserMapping();
            ProjectUserMapping.ProjectId = newProjectCopy.Id;
            ProjectUserMapping.AspNetUserId = LoggedUserId;
            ProjectUserMapping.FullAccess = true;
            ProjectUserMapping.ViewOnly = true;
            ProjectUserMapping.Notes = true;
            ProjectUserMapping.Deleted = false;

            ProjectUserMapping.CreatedById = LoggedUserId;
            ProjectUserMapping.CreatedOnUtc = GetDateTime;
            _projectUserMappingService.InsertProjectUser(ProjectUserMapping);


            return newProjectCopy.Id;
        }

        private async Task<string> AddProjectModule(string SiteId, string projectId, string ActionType, DateTime? ProjectStartDate, DateTime NewProjectStartDate, ProjectModule model, string LoggedUserId, DateTime GetDateTime)
        {
            var projectNewList = new ProjectModule();
            double Diffint = 0.00;
            if (ActionType == "Template To Project" && ProjectStartDate != null && model.StartDate != null)
            {
                Diffint = (model.StartDate.Value - ProjectStartDate.Value).TotalDays;
                projectNewList.StartDate = NewProjectStartDate.AddDays(Diffint);
                projectNewList.EndDate = NewProjectStartDate.AddDays(Diffint);
            }
            else
            {
                projectNewList.StartDate = model.StartDate;
                projectNewList.EndDate = model.EndDate;
            }

            int NextSortOrderOfProjectModule = 0;
            if (!string.IsNullOrWhiteSpace(projectId) && projectId != "undefined")
            {
                NextSortOrderOfProjectModule = await _projectModuleService.GetLastSortOrderOfProjectModules(projectId) + 1;
            }
            projectNewList.Id = Guid.NewGuid().ToString();
            projectNewList.SiteId = SiteId;
            projectNewList.ProjectId = projectId;
            projectNewList.SortOrder = NextSortOrderOfProjectModule;
            projectNewList.ProjectModuleTypeId = model.ProjectModuleTypeId;
            projectNewList.ProjectModuleStatusId = model.ProjectModuleStatusId;

            projectNewList.ProjectModuleNumber = await _projectModuleService.GetLastProjectModuleNumber() + 1;
            projectNewList.Name = model.Name;
            projectNewList.Description = model.Description;


            projectNewList.CloseDate = model.CloseDate;
            projectNewList.TargetDate = model.TargetDate;

            projectNewList.Notes = model.Notes;
            projectNewList.SortOrder = model.SortOrder;
            projectNewList.Color = model.Color;

            projectNewList.CreatedOnUtc = GetDateTime;
            projectNewList.CreatedById = LoggedUserId;
            projectNewList.UpdatedOnUtc = GetDateTime;
            projectNewList.UpdatedById = LoggedUserId;
            _projectModuleService.InsertProjectModule(projectNewList);

            return projectNewList.Id;
        }

        private async Task<string> AddProjectTask(string SiteId, string projectId, string ProjectModuleId, string ActionType, DateTime? ProjectStartDate, DateTime NewProjectStartDate, ProjectTask model, string LoggedUserId, DateTime GetDateTime)
        {
            var projectNewTask = new ProjectTask();
            // Get module sort order
            var module = await _projectModuleService.GetProjectModuleDetailsById(ProjectModuleId);

            if (module == null)
                throw new Exception("Module not found");

            int moduleSortOrder = module.SortOrder;
            decimal NextSortOrderOfProjectTask = 0;
            // Get last task sort order under this module
            var CurrentModuleSortOrder = await _projectTaskService.GetLastSortOrderOfProjectTasks(ProjectModuleId);

            // If no tasks found, lastTaskSortOrder will be 1.1m
            if (CurrentModuleSortOrder == 1.1m)
            {
                NextSortOrderOfProjectTask = moduleSortOrder + 0.1m;
            }
            else
            {
                NextSortOrderOfProjectTask = Math.Round(CurrentModuleSortOrder + 0.1m, 1); // increment
            }
            projectNewTask.SortOrder = NextSortOrderOfProjectTask;
            double Diffint = 0.00;
            if (ActionType == "Template To Project" && ProjectStartDate != null && model.StartDate != null)
            {
                Diffint = (model.StartDate.Value - ProjectStartDate.Value).TotalDays;
                projectNewTask.StartDate = NewProjectStartDate.AddDays(Diffint);
                projectNewTask.EndDate = NewProjectStartDate.AddDays(Diffint);
                projectNewTask.DueDate = NewProjectStartDate.AddDays(Diffint);
            }
            else
            {
                projectNewTask.StartDate = model.StartDate;
                projectNewTask.EndDate = model.EndDate;
                projectNewTask.DueDate = model.DueDate;
            }

            projectNewTask.SiteId = SiteId;
            projectNewTask.ProjectId = projectId;
            projectNewTask.ProjectModuleId = ProjectModuleId;

            projectNewTask.ProjectTaskNumber = await _projectTaskService.GetLastProjectTaskNumber() + 1;
            projectNewTask.StatusId = model.StatusId;
            projectNewTask.PriorityId = model.PriorityId;
            projectNewTask.AssignedToId = model.AssignedToId;

            projectNewTask.Name = model.Name;
            projectNewTask.Description = model.Description;
            projectNewTask.Instructions = model.Instructions;

            projectNewTask.EstimateTime = model.EstimateTime;
            projectNewTask.TaskMonth = model.TaskMonth;

            projectNewTask.SortOrder = model.SortOrder;
            projectNewTask.Color = model.Color;

            projectNewTask.CreatedOnUtc = GetDateTime;
            projectNewTask.CreatedById = LoggedUserId;
            projectNewTask.UpdatedOnUtc = GetDateTime;
            projectNewTask.UpdatedById = LoggedUserId;
            _projectTaskService.InsertProjectTask(projectNewTask);

            return projectNewTask.Id;
        }

        private async Task<string> AddProjectTaskActivity(string SiteId, string projectId, string ProjectModuleId, string ProjectTaskId, ProjectActivity model, string LoggedUserId, DateTime GetDateTime, bool IsActive = true)
        {
            var newActivity = new ProjectActivity();

            newActivity.SiteId = SiteId;
            newActivity.ProjectId = projectId;
            newActivity.ProjectModuleId = ProjectModuleId;
            newActivity.TaskId = ProjectTaskId;

            newActivity.AssignedToId = model.AssignedToId;
            newActivity.ActivityStatusId = model.ActivityStatusId;

            newActivity.Name = model.Name;
            newActivity.Description = model.Description;

            newActivity.EstimateHours = model.EstimateHours;
            newActivity.TargetMonth = model.TargetMonth;

            newActivity.StartDate = model.StartDate;
            newActivity.EndDate = model.EndDate;
            newActivity.DueDate = model.DueDate;

            newActivity.SortOrder = model.SortOrder;
            newActivity.Active = IsActive;
            newActivity.CreatedOnUtc = GetDateTime;
            newActivity.CreatedById = LoggedUserId;
            newActivity.UpdatedOnUtc = GetDateTime;
            newActivity.UpdatedById = LoggedUserId;
            _projectActivityService.InsertProjectActivity(newActivity);

            return newActivity.Id;
        }
        private void AddProjectEmployee(string projectId, string employeeId, string employeeDesignationId, decimal? productivityFactor, string projectUserMappingId, string LoggedUserId, DateTime GetDateTime, bool deleted = false)
        {
            var projectEmployeeMapping = new ProjectEmployeeMapping();

            projectEmployeeMapping.Id = Guid.NewGuid().ToString();
            projectEmployeeMapping.ProjectId = projectId;
            projectEmployeeMapping.EmployeeId = employeeId;
            projectEmployeeMapping.EmployeeDesignationId = employeeDesignationId;
            projectEmployeeMapping.ProductivityFactor = productivityFactor;
            projectEmployeeMapping.ProjectUserMappingId = projectUserMappingId;

            projectEmployeeMapping.CreatedById = LoggedUserId;
            projectEmployeeMapping.UpdatedById = LoggedUserId;
            projectEmployeeMapping.CreatedOnUtc = GetDateTime;
            projectEmployeeMapping.UpdatedOnUtc = GetDateTime;
            projectEmployeeMapping.Deleted = deleted;
            _projectEmployeeMappingService.InsertProjectEmployees(projectEmployeeMapping);
        }

        private void AddProjectTaskTags(string ProjectTaskId, ProjectTask_Tags model, string LoggedUserId, DateTime GetDateTime)
        {
            var newTag = new ProjectTask_Tags();
            newTag.TaskId = ProjectTaskId;
            newTag.TagId = model.TagId;

            newTag.CreatedById = LoggedUserId;
            newTag.CreatedOnUtc = GetDateTime;
            _projectTaskTagService.InsertProjectTaskTag(newTag);
        }

        private async Task<bool> UpdateProjectDetails(string projectId, object data, string flag)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            var entity = await _projectService.GetById(projectId);
            if (entity == null)
                return false;

            switch (flag.ToLower())
            {
                case "pin":
                    if (data is bool pinValue)
                    {
                        entity.IsPinned = pinValue;

                        if (pinValue)
                        {
                            var existsProjectPin = await _projectsPinnedService.GetProjectPinnedByUser(projectId, LoggedUserId);
                            if (existsProjectPin == null)
                            {
                                var projectPin = new ProjectPinned
                                {
                                    ProjectId = projectId,
                                    AspNetUserId = LoggedUserId
                                };
                                _projectsPinnedService.InsertProjectPin(projectPin);
                            }
                        }
                    }
                    break;

                case "status":
                    if (data is string statusId)
                        entity.ProjectStatusId = statusId;
                    break;

                case "priority":
                    if (data is string priorityId)
                        entity.ProjectPriorityId = priorityId;
                    break;

                case "typeId":
                    if (data is string typeId)
                        entity.ProjectTypeId = typeId;
                    break;

                case "golivedate":
                    if (data is DateTime GoLiveDate)
                        entity.GoLiveDate = GoLiveDate;
                    break;

                default:
                    return false;
            }

            entity.UpdatedById = LoggedUserId;
            entity.UpdatedOnUtc = GetDateTime;
            _projectService.UpdateProject(entity);

            return true;
        }

        private async Task<string> UpdateOrInsertUserMapping(ProjectUserMapping existingUser, string aspNetUserId, string projectId, string LoggedUserId, DateTime GetDateTime, bool fullAccess, bool viewOnly, bool notes, bool deleted, HashSet<string> processedUserIds, string siteId = null)
        {
            if ((existingUser != null && fullAccess) || (existingUser != null && deleted))
            {
                // Skip if this AspNetUserId is already processed
                if (!processedUserIds.Add(existingUser.Id))
                    return existingUser.Id;

                existingUser.ProjectId = projectId;
                existingUser.AspNetUserId = aspNetUserId;
                existingUser.FullAccess = fullAccess;
                existingUser.ViewOnly = viewOnly;
                existingUser.Notes = notes;
                existingUser.Deleted = deleted;
                _projectUserMappingService.UpdateProjectUser(existingUser);

                if (deleted)
                {
                    var moduleIds = (await _projectModuleService.GetAllModulesByProjectId(projectId)).Select(m => m.Id).ToList();

                    if (moduleIds.Any())
                    {
                        var users = await _projectModulesUserMappingService.GetUsersByProjectModuleIds(siteId, moduleIds);
                        var usersToDelete = users.Where(x => x.AspNetUserId == aspNetUserId).ToList();

                        foreach (var user in usersToDelete)
                        {
                            var entity = await _projectModulesUserMappingService.GetProjectModuleUserById(user.Id);
                            _projectModulesUserMappingService.DeleteProjectModuleUser(entity);
                        }
                    }
                }
                return existingUser?.Id;
            }
            else if (existingUser == null && !deleted)
            {
                var newUser = new ProjectUserMapping
                {
                    ProjectId = projectId,
                    AspNetUserId = aspNetUserId,
                    FullAccess = fullAccess,
                    ViewOnly = viewOnly,
                    Notes = notes,
                    Deleted = deleted,
                    CreatedById = LoggedUserId,
                    CreatedOnUtc = GetDateTime
                };
                _projectUserMappingService.InsertProjectUser(newUser);
                processedUserIds.Add(newUser.Id);
                return newUser?.Id;
            }
            return null;
        }
        #endregion
    }
}