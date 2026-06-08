using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ExcelDataReader;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.Employees;
using Vsky.Services.Issues;
using Vsky.Services.Notifications;
using Vsky.Services.ProjectActivities;
using Vsky.Services.ProjectModules;
using Vsky.Services.Projects;
using Vsky.Services.ProjectSwimLanes;
using Vsky.Services.ProjectTasks;
using Vsky.Services.Requirements;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.Timesheets;

namespace Vsky.Api.Controllers
{
    [Route("project-tasks")]
    public class ProjectTasksController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IProjectTaskService _taskService;
        private readonly IProjectModuleService _workOrderService;
        private readonly IProjectActivityService _activityService;
        private readonly IProjectService _projectService;
        private readonly IDropDownService _dropDownService;
        private readonly ISiteService _siteService;
        private readonly IProjectModuleService _projectModuleService;
        private readonly ICommonService _commonService;
        private readonly IProjectTaskStatusLogService _projectTaskStatusLogService;
        private readonly ITimesheetLinesService _timesheetLinesService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly ITagService _tagService;
        private readonly IProjectTaskTagService _projectTaskTagService;
        private readonly IProjectTaskFilesService _projectTaskFilesService;
        private readonly INotificationService _notificationService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly IEmployeeService _employeeService;
        private readonly IProjectSwimLanesListsTasksServices _projectSwimLanesListsTasksServices;
        private readonly IMasterNotificationService _masterNotificationService;
        private readonly IProjectTaskRelatedMappingsService _projectTaskRelatedMappingsService;
        private readonly IIssueService _issueService;
        private readonly IRequirementService _requirementService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations
        public ProjectTasksController(
            GlobalVariable globalVariable,
            IMapper mapper,
            IProjectTaskService taskService,
            IProjectActivityService activityService,
            IProjectService projectService,
            IProjectModuleService workOrderService,
            IDropDownService dropDownService,
            ISiteService SiteService,
            IProjectModuleService projectModuleService,
            ICommonService commonService,
            IProjectTaskStatusLogService projectTaskStatusLogService,
            ITimesheetLinesService timesheetLinesService,
            IDropDownTypeService dropDownTypeService,
            ITagService tagService,
            IProjectTaskTagService projectTaskTagService,
            IProjectTaskFilesService projectTaskFilesService,
            INotificationService notificationService,
            ISitesModifiedLogsService sitesModifiedLogsService,
            IEmployeeService employeeService,
            IProjectSwimLanesListsTasksServices projectSwimLanesListsTasksServices,
            IMasterNotificationService masterNotificationService,
            IProjectTaskRelatedMappingsService projectTaskRelatedMappingsService,
            IIssueService issueService,
            IRequirementService requirementService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _taskService = taskService;
            _activityService = activityService;
            _projectService = projectService;
            _workOrderService = workOrderService;
            _dropDownService = dropDownService;
            _siteService = SiteService;
            _projectModuleService = projectModuleService;
            _commonService = commonService;
            _projectTaskStatusLogService = projectTaskStatusLogService;
            _timesheetLinesService = timesheetLinesService;
            _dropDownTypeService = dropDownTypeService;
            _tagService = tagService;
            _projectTaskTagService = projectTaskTagService;
            _projectTaskFilesService = projectTaskFilesService;
            _notificationService = notificationService;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _employeeService = employeeService;
            _projectSwimLanesListsTasksServices = projectSwimLanesListsTasksServices;
            _masterNotificationService = masterNotificationService;
            _projectTaskRelatedMappingsService = projectTaskRelatedMappingsService;
            _issueService = issueService;
            _requirementService = requirementService;
            _azureBlobImageServices = azureBlobImageServices;
        }

        #endregion

        #region GetAllProjectTasks
        // Title: Get All Project Tasks
        // Description: This endpoint fetches a list of project tasks based on the provided search criteria. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllProjectTasks(ProjectTaskSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list
                var list = await _taskService.GetAllProjectTasks(
                    SiteId,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.IsTemplate,
                    searchModel.ProjectTaskNumber,
                    searchModel.ProjectIds,
                    searchModel.ProjectModuleIds,
                    searchModel.ProjectTaskIds,
                    searchModel.ProjectLeadsIds,
                    searchModel.StatusIds,
                    searchModel.PriorityIds,
                    searchModel.CustomerIds,
                    searchModel.CompanyContactIds,
                    searchModel.ActivityOwners,
                    searchModel.Name,
                    searchModel.TaskTagsIds,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                  );
                // Map the fetched list to a model suitable for the response
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

        [HttpPost("project-calender")]
        public async Task<IActionResult> GetAllTaskByProjectIdForCalendar(ProjectTaskSearchModel searchModel)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            if (searchModel.CalendarType.ToLower() == "task")
            {
                var filteredRows = await GenerateTaskRows(searchModel, GetDateTime);
                int totalCount = filteredRows.Count;
                int page = searchModel.Page <= 0 ? 1 : searchModel.Page;
                int pageSize = searchModel.PageSize <= 0 ? 20 : searchModel.PageSize;

                var pagedRows = filteredRows
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                var model = new TaskCalendarModel
                {
                    Columns = GenerateColumns(searchModel, GetDateTime),
                    Rows = pagedRows,
                    Total = totalCount,
                    Page = page,
                    PageSize = pageSize
                };

                return Ok(model);
            }
            else
            {
                var filteredRows = await GenerateModuleRows(searchModel, GetDateTime);
                int totalCount = filteredRows.Count;
                int page = searchModel.Page <= 0 ? 1 : searchModel.Page;
                int pageSize = searchModel.PageSize <= 0 ? 20 : searchModel.PageSize;

                var pagedRows = filteredRows
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var model = new ModuleCalendarModel
                {
                    Columns = GenerateColumns(searchModel, GetDateTime),
                    Rows = pagedRows,
                    Total = filteredRows.Count,
                    Page = searchModel.Page <= 0 ? 1 : searchModel.Page,
                    PageSize = searchModel.PageSize <= 0 ? 20 : searchModel.PageSize
                };

                return Ok(model);
            }
        }

        [HttpPost("taskDetailsList")]
        public IActionResult GetAllProjectTaskDetailsList(ProjectTaskSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list
                var list = _taskService.GetAllProjectTaskDetailsList(SiteId, searchModel.ProjectIds, searchModel.SortByFilterId, searchModel.Name, searchModel.ActivityOwners, searchModel.CustomerIds, searchModel.CompanyContactIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                var sortFilter = searchModel.SortByFilterId?.Trim().ToLowerInvariant();
                // grouping based on SortByFilterId
                var groupedTasks = sortFilter switch
                {
                    "filter by priority" => list.GroupBy(x => x.Priority.Id)
                                .Select(group => new ProjectTaskGroupModel
                                {
                                    GroupId = group.Key.ToString(),
                                    GroupValue = group.First().Priority.DropDownValue, // Get Priority Name
                                    Tasks = _mapper.Map<IList<ProjectTaskModel>>(group.ToList())
                                }).ToList(),

                    "filter by module" => list.GroupBy(x => x.ProjectModule.Id)
                                .Select(group => new ProjectTaskGroupModel
                                {
                                    GroupId = group.Key.ToString(),
                                    GroupValue = group.First().ProjectModule.Name,
                                    Tasks = _mapper.Map<IList<ProjectTaskModel>>(group.ToList())
                                }).ToList(),

                    _ => list.GroupBy(x => x.Status.Id) // Default group by StatusId
                                .Select(group => new ProjectTaskGroupModel
                                {
                                    GroupId = group.Key.ToString(),
                                    GroupValue = group.First().Status.DropDownValue,
                                    Tasks = _mapper.Map<IList<ProjectTaskModel>>(group.ToList())
                                }).ToList()
                };

                // Create the response model
                var model = new ProjectTaskListGroupedModel
                {
                    Data = groupedTasks,
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Description: Get all high priority tasks of current user for dashboard
        [HttpPost("taskListForDashboard")]
        public IActionResult GetAllHighPrioritiesTaskForDashboard(ProjectTaskSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                //find high priorityid 
                var PriorityId = _dropDownTypeService.GetDropDownTypeByType(SiteId, "Task Priorities").GetAwaiter().GetResult();
                var HighPriorityId = _dropDownService.GetDropDownByTypeAndValue(SiteId, PriorityId.Id, "High").GetAwaiter().GetResult();
                //fetch high priority task list
                var list = _taskService.GetAllHighPrioritiesTaskForDashboard(SiteId, searchModel.ActivityOwnerId, HighPriorityId.Id, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

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

        [HttpPost("taskForCopy")]
        public IActionResult GetProjectTaskForCopy(ProjectTaskSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list
                var list = _taskService.GetProjectTaskForCopy(SiteId, searchModel.ProjectTaskId, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
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

        [HttpPost("taskListForNotes")]
        public async Task<IActionResult> GetAllProjectTasksForNotes(ProjectTaskSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list
                var list = await _taskService.GetAllProjectTasksForNotes(
                    SiteId,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.isShowCloseStatus,
                    searchModel.ProjectTaskNumber,
                    searchModel.ProjectIds,
                    searchModel.ProjectModuleIds,
                    searchModel.ProjectLeadsIds,
                    searchModel.StatusIds,
                    searchModel.PriorityIds,
                    searchModel.CustomerIds,
                    searchModel.CompanyContactIds,
                    searchModel.ActivityOwners,
                    searchModel.Name,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                 );

                // Map the fetched list to a model suitable for the response
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

        #region GetAllProjectTaskListForDropdwon        
        [HttpGet("dropdown/list/{projectId}/{projectModuleId}/{employeeId}")]
        public async Task<IActionResult> GetAllProjectTaskListForDropdown(string projectId = null, string projectModuleId = null, string employeeId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _taskService.GetAllProjectTaskListForDropdown(SiteId, projectId, projectModuleId, employeeId);
                var model = _mapper.Map<List<ProjectTaskModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("dropdown/multiTaskList/{isTemplate}/{projectId}/{projectModuleId}")]
        public async Task<IActionResult> GetAllProjectMultiTaskListForDropdown(bool isTemplate, string projectId = null, string projectModuleId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var list = await _taskService.GetAllProjectMultiTaskListForDropdown(SiteId, isTemplate, projectId, projectModuleId);
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("dropdown/task-with-project-list")]
        public async Task<IActionResult> GetAllProjectTaskWithProjectListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var list = await _taskService.GetAllProjectTaskWithProjectListForDropdown(SiteId, LoggedUserId);
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("dropdown/tagslist")]
        public async Task<IActionResult> GetAllTagsListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _tagService.GetAllTagList(SiteId);
                var model = _mapper.Map<List<TagModels>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllProjectTaskTagListForDropdown
        // Title: GetAllProjectTaskTagListForDropdown
        // Description: This endpoint retrieves all tags created by the logged-in user for dropdown display.
        [HttpGet("projectTaskTags/dropdown/list")]
        public async Task<IActionResult> GetAllProjectTaskTagListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();

                var list = await _projectTaskTagService.GetProjectTaskTagByUserId(LoggedUserId);
                var model = _mapper.Map<List<CommonDropDown>>(list);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetTaskByProjectModuleId
        [HttpGet("task")]
        public async Task<IActionResult> GetTaskByProjectModuleId(string projectModuleId, string pageName = "", bool isShowCloseStatus = false)
        {
            try
            {
                var list = await _taskService.GetTaskByProjectModuleId(projectModuleId, pageName, isShowCloseStatus);
                var model = _mapper.Map<IList<ProjectTaskModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetProjectTaskById       
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectTaskById(string id)
        {
            try
            {
                var entity = await _taskService.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No project task found with the specified id."));

                var model = _mapper.Map<ProjectTaskModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetTimesheetByTaskId       
        [HttpGet("task-timesheet/{id}")]
        public async Task<IActionResult> GetTimesheetByTaskId(string id)
        {
            try
            {
                var list = await _timesheetLinesService.GetTimesheetByTaskId(id);
                var model = _mapper.Map<List<TimesheetLinesModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetProjectTaskDetailsById
        // Title: GetProjectTaskDetailsById
        // Description: This endpoint retrieves the details of a specific project task based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetProjectTaskDetailsById(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch the task entity by its ID from the service
                var entity = await _taskService.GetProjectTaskDetailsById(id);
                // If the task entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No project task found with the specified id."));

                // Map the task entity to a ProjectTaskModel object
                var model = _mapper.Map<ProjectTaskModel>(entity);

                // Fetch dropdown data for Project Activities
                var activityDropDownType = await _dropDownTypeService.GetDropDownType(SiteId, "Project Activities");
                var activityDropdowns = await _dropDownService.GetDropDowns(activityDropDownType.Id);

                // Assign description to each activity
                foreach (var activity in model.ProjectActivities)
                {
                    if (!string.IsNullOrEmpty(activity?.Name))
                    {
                        var activityDropdownItem = activityDropdowns.FirstOrDefault(d => d.DropDownValue == activity.Name);
                        if (activityDropdownItem != null)
                        {
                            activity.ActivityNameDescription = activityDropdownItem.Description;
                        }
                    }
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region 
        [HttpGet("projectask-detailsbyids")]
        public async Task<IActionResult> GetProjectTaskDetailsByIds([FromQuery] string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return BadRequest(new BadRequestError("Tasks are missing."));
            }

            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var idArray = ids.Split(','); // Split the comma-separated string into an array
            var entity = await _taskService.GetProjectTaskDetailsByIds(SiteId, idArray);
            if (entity == null)
                return BadRequest(new BadRequestError("No task found with the specified ids."));

            var model = _mapper.Map<List<ProjectTaskModel>>(entity);
            return Ok(model);
        }

        [HttpPost("taskListByProjectId")]
        public IActionResult GetAllTasksByProjectId(ProjectTaskSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _taskService.GetAllTasksByProjectId(SiteId, searchModel.ProjectId, searchModel.SearchTaskText, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
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

        #region CheckTaskCanBeDeleted
        [HttpGet("CheckTaskCanBeDeleted/{taskId}")]
        public async Task<IActionResult> CheckTaskCanBeDeleted(string taskId)
        {
            // Check if any active task activity exists
            var activities = await _activityService.GetProjectActivitiesByTaskId(taskId);

            // Check if any active task activity exists
            var hasActiveActivities = activities.Any(a =>
                !a.Deleted &&
                (a.ActivityStatus.DropDownValue != "Completed" && a.ActivityStatus.DropDownValue != "Close")
            );

            // If either activities are active, disallow delete
            bool canDelete = !(hasActiveActivities);

            return Ok(new { canDelete });
        }
        #endregion

        #region CreateProjectTask
        [HttpPost("task")]
        public async Task<IActionResult> CreateProjectTask(ProjectTaskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = _taskService.GetProjectTaskByName(model.Name, model.ProjectId, model.ProjectModuleId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Task name already exists, try with another."));

                    var entity = _mapper.Map<ProjectTask>(model);
                    entity.SiteId = SiteId;
                    entity.ProjectTaskNumber = await _taskService.GetLastProjectTaskNumber() + 1;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "project-tasks",
                                entity.ProjectTaskNumber.ToString()
                            );
                    }

                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _taskService.InsertProjectTask(entity);

                    var activityEntities = new List<ProjectActivity>();
                    foreach (var activity in model.ProjectActivityModel)
                    {
                        var activityEntity = _mapper.Map<ProjectActivity>(activity);
                        activityEntity.Id = Guid.NewGuid().ToString();
                        activityEntity.TaskId = entity.Id;

                        if (!string.IsNullOrEmpty(activity.Description))
                        {
                            activityEntity.Description = await _azureBlobImageServices
                                .ProcessHtmlAndManageImagesAsync(
                                    activity.Description,
                                    SiteData.Name,
                                    "project-tasks-activities",
                                    activityEntity.Id
                                );
                        }

                        activityEntity.Active = true;
                        activityEntity.CreatedById = LoggedUserId;
                        activityEntity.UpdatedById = LoggedUserId;
                        activityEntity.CreatedOnUtc = GetDateTime;
                        activityEntity.UpdatedOnUtc = GetDateTime;
                        activityEntities.Add(activityEntity);
                    }
                    if (activityEntities.Any())
                        _activityService.InsertProjectActivityList(activityEntities);

                    // Update WorkBoard List Task
                    bool IsWorkboardUpdated = await UpdateProjectSwimlaneListTask(SiteId, entity, LoggedUserId, GetDateTime);

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("tags")]
        public async Task<IActionResult> CreateTags(TagModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    foreach (var taskId in model.TaskIds)
                    {
                        var existingTagList = _projectTaskTagService.GetProjectTaskTagsByTaskIdAndUserId(SiteId, taskId, LoggedUserId);
                        var existingTagNames = existingTagList.Select(x => x.Tags.Name).ToList();
                        var toAdd = model.TagsNameList.Except(existingTagNames).ToList();

                        if (model.Flag != "TG" && model.Flag != "isMultiSelectTask")
                        {
                            var toRemove = existingTagList.Where(x => !model.TagsNameList.Contains(x.Tags.Name)).ToList();

                            foreach (var mapping in toRemove)
                            {
                                if (mapping != null)
                                {
                                    var existingTaskTag = await _projectTaskTagService.GetByNameTaskIdAndUserId(SiteId, mapping.Tags.Name, taskId, LoggedUserId);
                                    if (existingTaskTag != null)
                                        _projectTaskTagService.DeleteTaskTag(existingTaskTag);
                                }
                            }
                        }

                        if (toAdd.Count > 0)
                        {
                            foreach (var tag in toAdd)
                            {
                                if (string.IsNullOrWhiteSpace(tag)) continue;

                                var existingTag = await _tagService.GetTagByName(SiteId, tag);

                                if (existingTag == null)
                                {
                                    existingTag = new Tags
                                    {
                                        Name = tag,
                                        SiteId = SiteId,
                                        Color = model.Color,
                                        CreatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime,
                                        UpdatedById = LoggedUserId,
                                        UpdatedOnUtc = GetDateTime
                                    };

                                    _tagService.InsertTags(existingTag);
                                }

                                var existingTask = await _projectTaskTagService.GetByNameTaskIdAndUserId(SiteId, tag, taskId, LoggedUserId);
                                if (existingTask != null)
                                    continue;

                                var projectTaskTags = new ProjectTask_Tags
                                {
                                    TagId = existingTag.Id,
                                    TaskId = taskId,
                                    AspNetUserId = LoggedUserId,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };

                                _projectTaskTagService.InsertProjectTaskTag(projectTaskTags);
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

        // Title: AddFiles
        [HttpPost("add-task-files")]
        public async Task<IActionResult> AddFiles([FromForm] ProjectTaskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _taskService.GetProjectTaskDetailsById(model.Id);

                    // If no task is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project task found with the specified id."));

                    var moduleData = await _taskService.GetProjectTaskDetailsById(entity.Id);

                    //Add
                    string ProjectTaskId = entity.Id;
                    if (model.ProjectTaskFiles != null && model.ProjectTaskFiles.Any())
                    {
                        int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(ProjectTaskId, "Project Task");

                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "project-tasks", model.ProjectTaskFiles, entity.ProjectTaskNumber.ToString(), existingImagesCount);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ProjectTaskFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = model.ProjectId,
                                Module = moduleData.Project.Name,
                                SubModuleId = ProjectTaskId,
                                Sub_Module = entity.Name,
                                Type = "Project Task",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var ProjectTaskFiles = new ProjectTaskFiles
                            {
                                FileId = picture.Id,
                                ProjectTaskId = ProjectTaskId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _projectTaskFilesService.InsertProjectTaskFile(ProjectTaskFiles);

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

        #region DisplayWarningForSortOrder
        // Title: DisplayWarningForSortOrder
        // Description: This endpoint checks if the sort order exists. 
        // If it does, a warning message is returned.
        [HttpGet("warning")]
        public async Task<IActionResult> DisplayWarningForSortOrder(
            string projectId,
            decimal sortOrder,
            string moduleId,
            string taskId = null)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            string warning = string.Empty;
            bool sortOrderExists = await _taskService.IsProjectTaskSortOrderExists(SiteId, projectId, sortOrder, moduleId, taskId);

            if (sortOrderExists)
            {
                warning = "Sort order already exists";
            }
            else
            {
                warning = null;
            }

            return Ok(new
            {
                Warning = warning
            });
        }
        #endregion

        #region CreateProjectTask
        // Title: CreateProjectModule
        // Description: This endpoint handles the creation of a new project module. It first checks if a project module with the same name already exists for the specified name. If not, it maps the Project Module model to the Project Module entity, sets the creation details, and inserts the Project Module into the database.
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromForm] ProjectTaskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the project module already exists
                    var exists = await _taskService.GetProjectTaskByName(model.Name, model.ProjectId, model.ProjectModuleId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Project task already exists"));

                    // Map the Project Module model to the Project Module entity
                    var entity = _mapper.Map<ProjectTask>(model);

                    var ProjectModuleDetails = await _projectModuleService.GetProjectModuleById(model.ProjectModuleId);
                    if (model.ProjectId == ProjectModuleDetails.ProjectId)
                        entity.ProjectModuleId = model.ProjectModuleId;
                    else
                        return BadRequest(new BadRequestError("select correct module for this project"));

                    entity.SiteId = SiteId;
                    entity.ProjectTaskNumber = await _taskService.GetLastProjectTaskNumber() + 1;
                    if (model.StartDateStr != "" && model.StartDateStr != null)
                        entity.StartDate = DateTime.ParseExact(model.StartDateStr, "MM/dd/yyyy", null);
                    if (model.EndDateStr != "" && model.EndDateStr != null)
                        entity.EndDate = DateTime.ParseExact(model.EndDateStr, "MM/dd/yyyy", null);

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "project-tasks",
                                entity.ProjectTaskNumber.ToString()
                            );
                    }

                    entity.TypeId = (!string.IsNullOrWhiteSpace(model.TypeId) && model.TypeId != "undefined") ? model.TypeId : null;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _taskService.InsertProjectTask(entity);

                    var taskData = await _taskService.GetProjectTaskDetailsById(entity.Id);

                    if (!string.IsNullOrEmpty(model.AssignedToId) && model.AssignedToId != "undefined")
                    {
                        var assignedToName = await _employeeService.GetEmployeeDetailsById(model.AssignedToId);
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", entity.ProjectId, taskData.Project.Name, entity.Id, entity.Name, "Task Owner", assignedToName.Person.FirstName + " " + assignedToName.Person.LastName, LoggedUserId, GetDateTime);
                    }

                    if (!string.IsNullOrEmpty(model.Name))
                    {
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", entity.ProjectId, taskData.Project.Name, entity.Id, entity.Name, "Task Name", model.Name, LoggedUserId, GetDateTime);
                    }

                    if (model.EndDateStr != null)
                    {
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", entity.ProjectId, taskData.Project.Name, entity.Id, entity.Name, "Due Date", model.EndDateStr, LoggedUserId, GetDateTime);
                    }

                    if (!string.IsNullOrEmpty(model.StatusId))
                    {
                        var Status = await _dropDownService.GetDropDownById(model.StatusId);
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", entity.ProjectId, taskData.Project.Name, entity.Id, entity.Name, "Task Status", Status.DropDownValue, LoggedUserId, GetDateTime);
                    }

                    if (!string.IsNullOrEmpty(model.PriorityId))
                    {
                        var Priority = await _dropDownService.GetDropDownById(model.PriorityId);
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", entity.ProjectId, taskData.Project.Name, entity.Id, entity.Name, "Task Priority", Priority.DropDownValue, LoggedUserId, GetDateTime);
                    }

                    string ProjectTaskId = entity.Id;
                    if (model.ProjectTaskFiles != null && model.ProjectTaskFiles.Any())
                    {
                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "project-tasks", model.ProjectTaskFiles, entity.ProjectTaskNumber.ToString());
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ProjectTaskFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = model.ProjectId,
                                Module = taskData.Project.Name,
                                SubModuleId = ProjectTaskId,
                                Sub_Module = entity.Name,
                                Type = "Project Task",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var ProjectTaskFiles = new ProjectTaskFiles
                            {
                                FileId = picture.Id,
                                ProjectTaskId = ProjectTaskId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _projectTaskFilesService.InsertProjectTaskFile(ProjectTaskFiles);

                            index++;
                        }
                    }

                    // Update WorkBoard List Task
                    bool IsWorkboardUpdated = await UpdateProjectSwimlaneListTask(SiteId, entity, LoggedUserId, GetDateTime);

                    // If converted from issue
                    if (model.IsIssueConverted)
                    {
                        var projectTaskRelatedMapping = new ProjectTaskRelatedMapping
                        {
                            TaskId = entity.Id,
                            IssueId = model.IssueId,
                            CreatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime
                        };
                        _projectTaskRelatedMappingsService.InsertProjectTaskRelatedMapping(projectTaskRelatedMapping);

                    }

                    // If converted from Requirement
                    if (model.IsRequirementConverted)
                    {
                        var projectTaskRelatedMapping = new ProjectTaskRelatedMapping
                        {
                            TaskId = entity.Id,
                            RequirementId = model.RequirementId,
                            CreatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime
                        };
                        _projectTaskRelatedMappingsService.InsertProjectTaskRelatedMapping(projectTaskRelatedMapping);
                    }

                    return Ok(entity);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            // Return model state errors if the model state is not valid
            return ModelStateError(ModelState);
        }
        #endregion

        #region TaskAssignToOwner
        [HttpPut("task-assignment/{id}")]
        public async Task<IActionResult> TaskAssignToOwner(string id, ProjectTaskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var status = await _dropDownTypeService.GetDropDownTypeByType(SiteId, "Activity Status");
                    var activityStatus = await _dropDownService.GetDropDownByTypeAndValue(SiteId, status.Id, "New");

                    if (model.ProjectActivities.Count() > 0)
                    {
                        foreach (var activity in model.ProjectActivities)
                        {
                            var exisitingActivityData = await _activityService.GetById(activity.Id);

                            if (exisitingActivityData != null && !activity.Deleted)
                            {
                                var existsActivity = await _activityService.GetProjectActivityByDetails(activity.Name, id, activity.AssignedToId, null, activity.Id);
                                if (existsActivity != null)
                                    continue;

                                exisitingActivityData.ProjectId = model.ProjectId;
                                exisitingActivityData.ProjectModuleId = model.ProjectModuleId;
                                exisitingActivityData.TaskId = id;
                                exisitingActivityData.Name = activity.Name;
                                exisitingActivityData.AssignedToId = !string.IsNullOrEmpty(activity.AssignedToId) && activity.AssignedToId != "undefined" ? activity.AssignedToId : null;
                                exisitingActivityData.EstimateHours = activity.EstimateHours;
                                exisitingActivityData.ActivityStatusId = activity.ActivityStatusId != "undefined" ? activity.ActivityStatusId : activityStatus.Id;

                                if (!string.IsNullOrEmpty(activity.Description))
                                {
                                    exisitingActivityData.Description = await _azureBlobImageServices
                                        .ProcessHtmlAndManageImagesAsync(
                                            activity.Description,
                                            SiteData.Name,
                                            "project-tasks-activities",
                                            activity.Id,
                                            exisitingActivityData.Description
                                        );
                                }

                                exisitingActivityData.Deleted = activity.Deleted;
                                exisitingActivityData.UpdatedById = LoggedUserId;
                                exisitingActivityData.UpdatedOnUtc = GetDateTime;
                                _activityService.UpdateProjectActivity(exisitingActivityData);
                            }
                            else if (exisitingActivityData == null && !activity.Deleted)
                            {
                                var existsActivity = await _activityService.GetProjectActivityByDetails(activity.Name, id, activity.AssignedToId, null);
                                if (existsActivity != null)
                                    continue;

                                ProjectActivity projectActivity = new ProjectActivity();

                                projectActivity.Id = Guid.NewGuid().ToString();
                                projectActivity.SiteId = SiteId;
                                projectActivity.ProjectId = model.ProjectId;
                                projectActivity.ProjectModuleId = model.ProjectModuleId;
                                projectActivity.TaskId = id;
                                projectActivity.Name = activity.Name;
                                projectActivity.AssignedToId = activity.AssignedToId;
                                projectActivity.EstimateHours = activity.EstimateHours;
                                projectActivity.Active = true;
                                projectActivity.ActivityStatusId = activityStatus.Id;
                                projectActivity.Deleted = activity.Deleted;

                                if (!string.IsNullOrEmpty(activity.Description))
                                {
                                    projectActivity.Description = await _azureBlobImageServices
                                        .ProcessHtmlAndManageImagesAsync(
                                            activity.Description,
                                            SiteData.Name,
                                            "project-tasks-activities",
                                            activity.Id
                                        );
                                }

                                projectActivity.CreatedById = LoggedUserId;
                                projectActivity.CreatedOnUtc = GetDateTime;
                                projectActivity.UpdatedById = LoggedUserId;
                                projectActivity.UpdatedOnUtc = GetDateTime;
                                _activityService.InsertProjectActivity(projectActivity);

                                var AssignedTo = _commonService.GetLoggeduserIdByEmployeeId(SiteId, projectActivity.AssignedToId);
                                var MasterNotificationData = await _masterNotificationService.GetMasterNotificationByNumber(SiteId, "Task1", AssignedTo);
                                if (MasterNotificationData != null)
                                {
                                    string Message = MasterNotificationData.Message.Replace("[Activity Name]", projectActivity.Name);
                                    var Notification = _notificationService.AddNotification(SiteId, MasterNotificationData.Title, Message, MasterNotificationData.Type, projectActivity.CreatedById, projectActivity.Id, "/my-task-and-activities", AssignedTo, projectActivity.CreatedById, GetDateTime);
                                }
                            }
                            else if (exisitingActivityData != null && activity.Deleted)
                            {
                                _activityService.DeleteProjectActivity(exisitingActivityData);
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
        #endregion

        #region UpdateProjectTask
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProjectTask(string id, [FromForm] ProjectTaskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _taskService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project task found with the specified id."));

                    var exists = await _taskService.GetProjectTaskByName(model.Name, model.ProjectId, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Task name already exists, try with another."));

                    bool IsAssignedToChanged = (!string.IsNullOrEmpty(model.AssignedToId) && model.AssignedToId != "undefined" && model.AssignedToId != entity.AssignedToId) ? true : false;
                    bool IsTaskStatusChanged = model.StatusId != entity.StatusId;
                    bool IsTaskPriorityChanged = model.PriorityId != entity.PriorityId;
                    bool IsTaskNameChanged = model.Name != entity.Name;
                    bool IsTaskDueDateChanged = !string.IsNullOrEmpty(model.EndDateStr) && DateTime.ParseExact(model.EndDateStr, "MM/dd/yyyy", null) != entity.EndDate;

                    #region Task Files
                    // Retrieve all file IDs from the project task files
                    var allProjectTaskFileIds = (await _projectTaskFilesService.GetAllProjectTaskFilesByProjectTaskId(id)).Select(file => file.Id).ToList();
                    var missingFileIds = allProjectTaskFileIds.ToList();

                    if (model.ExistingFiles != null)
                    {
                        var existingFileIds = model.ExistingFiles.Select(fileJson =>
                        {
                            var file = JsonConvert.DeserializeObject<Picture>(fileJson);
                            return file.Id.Trim().ToLower();
                        })
                        .ToList();

                        // Compare and find missing file IDs
                        missingFileIds = allProjectTaskFileIds.Except(existingFileIds).ToList();
                    }

                    if (missingFileIds.Any())
                    {
                        foreach (var projectTaskFilesId in missingFileIds)
                        {
                            var projectFileDate = await _projectTaskFilesService.GetProjectTaskFileById(projectTaskFilesId);
                            if (projectFileDate != null)
                                _projectTaskFilesService.DeleteProjectTaskFile(projectFileDate);
                        }
                    }
                    #endregion

                    entity.ProjectId = model.ProjectId;
                    entity.ProjectModuleId = model.ProjectModuleId;
                    entity.AreaId = model.AreaId;
                    entity.WorkspaceId = model.WorkspaceId;
                    entity.ActionId = model.ActionId;
                    entity.StatusId = model.StatusId;
                    entity.PriorityId = model.PriorityId;
                    entity.AssignedToId = model.AssignedToId;
                    entity.TypeId = (!string.IsNullOrWhiteSpace(model.TypeId) && model.TypeId != "undefined") ? model.TypeId : null;
                    entity.SortOrder = model.SortOrder;

                    entity.Name = model.Name;
                    entity.EstimateTime = model.EstimateTime;
                    //entity.Description = model.Description;
                    if (model.StartDateStr != "" && model.StartDateStr != null)
                        entity.StartDate = DateTime.ParseExact(model.StartDateStr, "MM/dd/yyyy", null);
                    if (model.EndDateStr != "" && model.EndDateStr != null)
                        entity.EndDate = DateTime.ParseExact(model.EndDateStr, "MM/dd/yyyy", null);

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "project-tasks",
                                entity.ProjectTaskNumber.ToString(),
                                entity.Description
                            );
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _taskService.UpdateProjectTask(entity);

                    // Task Log (new Version)
                    var taskData = await _taskService.GetProjectTaskDetailsById(id);
                    if (IsAssignedToChanged)
                    {
                        var assignedToName = await _employeeService.GetEmployeeDetailsById(model.AssignedToId);
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", entity.ProjectId, taskData.Project.Name, entity.Id, entity.Name, "Task Owner", assignedToName.Person.FirstName + " " + assignedToName.Person.LastName, LoggedUserId, GetDateTime);
                    }

                    if (IsTaskNameChanged)
                    {
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", entity.ProjectId, taskData.Project.Name, entity.Id, entity.Name, "Task Name", model.Name, LoggedUserId, GetDateTime);
                    }

                    if (IsTaskDueDateChanged)
                    {
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", entity.ProjectId, taskData.Project.Name, entity.Id, entity.Name, "Due Date", model.EndDateStr, LoggedUserId, GetDateTime);
                    }

                    if (IsTaskStatusChanged)
                    {
                        var Status = await _dropDownService.GetDropDownById(model.StatusId);
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", entity.ProjectId, taskData.Project.Name, entity.Id, entity.Name, "Task Status", Status.DropDownValue, LoggedUserId, GetDateTime);
                    }

                    if (IsTaskPriorityChanged)
                    {
                        var Priority = await _dropDownService.GetDropDownById(model.PriorityId);
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", entity.ProjectId, taskData.Project.Name, entity.Id, entity.Name, "Task Priority", Priority.DropDownValue, LoggedUserId, GetDateTime);
                    }

                    // Task Files
                    if (model.ProjectTaskFiles != null && model.ProjectTaskFiles.Any())
                    {
                        int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(id, "Project Task");

                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "project-tasks", model.ProjectTaskFiles, entity.ProjectTaskNumber.ToString(), existingImagesCount);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ProjectTaskFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = model.ProjectId,
                                Module = taskData.Project.Name,
                                SubModuleId = id,
                                Sub_Module = entity.Name,
                                Type = "Project Task",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var ProjectTaskFiles = new ProjectTaskFiles
                            {
                                FileId = picture.Id,
                                ProjectTaskId = id,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _projectTaskFilesService.InsertProjectTaskFile(ProjectTaskFiles);

                            index++;
                        }
                    }

                    // Task Activity
                    if (model.ProjectActivities.Count() > 0)
                    {
                        var status = await _dropDownTypeService.GetDropDownTypeByType(SiteId, "Activity Status");
                        var activityStatus = await _dropDownService.GetDropDownByTypeAndValue(SiteId, status.Id, "New");

                        foreach (var activity in model.ProjectActivities)
                        {
                            var exisitingActivityData = await _activityService.GetById(activity.Id);
                            if (exisitingActivityData != null)
                            {
                                var existsActivity = await _activityService.GetProjectActivityByDetails(activity.Name, entity.Id, activity.AssignedToId, null, activity.Id);
                                if (existsActivity != null)
                                    continue;

                                exisitingActivityData.ProjectId = model.ProjectId;
                                exisitingActivityData.ProjectModuleId = model.ProjectModuleId;
                                exisitingActivityData.TaskId = entity.Id;
                                exisitingActivityData.AssignedToId = !string.IsNullOrEmpty(activity.AssignedToId) && activity.AssignedToId != "undefined" ? activity.AssignedToId : null;

                                exisitingActivityData.ActivityStatusId = activity.ActivityStatusId != "undefined" ? activity.ActivityStatusId : activityStatus.Id;
                                exisitingActivityData.Name = activity.Name;
                                //exisitingActivityData.Description = activity.Description;
                                exisitingActivityData.EstimateHours = activity.EstimateHours;

                                if (!string.IsNullOrEmpty(activity.Description))
                                {
                                    exisitingActivityData.Description = await _azureBlobImageServices
                                        .ProcessHtmlAndManageImagesAsync(
                                            activity.Description,
                                            SiteData.Name,
                                            "project-tasks-activities",
                                            activity.Id,
                                            exisitingActivityData.Description
                                        );
                                }

                                exisitingActivityData.UpdatedById = LoggedUserId;
                                exisitingActivityData.UpdatedOnUtc = GetDateTime;
                                exisitingActivityData.Deleted = activity.Deleted;
                                _activityService.UpdateProjectActivity(exisitingActivityData);
                            }
                            else
                            {
                                var existsActivity = await _activityService.GetProjectActivityByDetails(activity.Name, entity.Id, activity.AssignedToId, null);
                                if (existsActivity != null)
                                    continue;

                                ProjectActivity projectActivity = new ProjectActivity();
                                projectActivity.Id = Guid.NewGuid().ToString();
                                projectActivity.SiteId = SiteId;
                                projectActivity.ProjectId = model.ProjectId;
                                projectActivity.ProjectModuleId = model.ProjectModuleId;
                                projectActivity.TaskId = entity.Id;
                                projectActivity.AssignedToId = activity.AssignedToId;
                                projectActivity.ActivityStatusId = activityStatus.Id;

                                projectActivity.Name = activity.Name;
                                //projectActivity.Description = activity.Description;
                                projectActivity.EstimateHours = activity.EstimateHours;
                                projectActivity.Active = true;

                                if (!string.IsNullOrEmpty(activity.Description))
                                {
                                    projectActivity.Description = await _azureBlobImageServices
                                        .ProcessHtmlAndManageImagesAsync(
                                            activity.Description,
                                            SiteData.Name,
                                            "project-tasks-activities",
                                            activity.Id
                                        );
                                }

                                projectActivity.CreatedById = LoggedUserId;
                                projectActivity.CreatedOnUtc = GetDateTime;
                                projectActivity.UpdatedById = LoggedUserId;
                                projectActivity.UpdatedOnUtc = GetDateTime;
                                projectActivity.Deleted = activity.Deleted;
                                _activityService.InsertProjectActivity(projectActivity);

                                var AssignedTo = _commonService.GetLoggeduserIdByEmployeeId(SiteId, projectActivity.AssignedToId);
                                var MasterNotificationData = await _masterNotificationService.GetMasterNotificationByNumber(SiteId, "Task1", AssignedTo);
                                if (MasterNotificationData != null)
                                {
                                    string Message = MasterNotificationData.Message.Replace("[Activity Name]", entity.Name);
                                    var Notification = _notificationService.AddNotification(SiteId, MasterNotificationData.Title, Message, MasterNotificationData.Type, entity.CreatedById, entity.Id, "/my-task-and-activities", AssignedTo, entity.CreatedById, GetDateTime);
                                }
                            }
                        }
                    }

                    // Task Status Log
                    var statusLog = new ProjectTaskStatusLog();
                    statusLog.TaskId = entity.Id;
                    statusLog.StatusId = model.StatusId;
                    //statusLog.StatusChangedBy = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                    statusLog.StatusChangedBy = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                    statusLog.StatusChangedDate = GetDateTime;
                    _projectTaskStatusLogService.InsertProjectTaskStatusLog(statusLog);

                    // Update WorkBoard List Task
                    if (IsTaskStatusChanged)
                        await UpdateProjectSwimlaneListTask(SiteId, entity, LoggedUserId, GetDateTime);

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

        #region UpdateProjectTaskPriority
        [HttpPost("projectTaskPriority")]
        public async Task<IActionResult> UpdateProjectTaskPriority(ProjectTaskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.TaskIds != null && model.TaskIds.Count > 0)
                    {
                        foreach (var taskIds in model.TaskIds)
                        {
                            var taskEntity = await _taskService.GetById(taskIds);
                            if (taskEntity != null)
                            {
                                bool result = await UpdateTaskDetails(taskIds, model.PriorityId, "priority");

                                var taskDetails = await _taskService.GetProjectTaskDetailsById(taskIds);

                                var Priority = await _dropDownService.GetDropDownById(model.PriorityId);
                                _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", taskEntity.ProjectId, taskDetails.Project.Name, taskEntity.Id, taskEntity.Name, "Task Priority", Priority.DropDownValue, LoggedUserId, GetDateTime);
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
        #endregion

        //created for update task status from task activity list page by SN
        #region
        [HttpPut("{id}/{taskId}/{statusId}")]
        public async Task<IActionResult> UpdateTaskStatus(string id, string taskId, string statusId)
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
                    var entity = await _taskService.GetById(taskId);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No task found with the specified id."));

                    bool IsStatusChanged = entity.StatusId != statusId ? true : false;

                    entity.StatusId = statusId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _taskService.UpdateProjectTask(entity);

                    var statusLog = new ProjectTaskStatusLog();
                    statusLog.TaskId = entity.Id;
                    statusLog.StatusId = statusId;
                    //statusLog.StatusChangedBy = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                    statusLog.StatusChangedBy = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                    statusLog.StatusChangedDate = GetDateTime;
                    _projectTaskStatusLogService.InsertProjectTaskStatusLog(statusLog);

                    // Update WorkBoard List Task
                    if (IsStatusChanged)
                        await UpdateProjectSwimlaneListTask(SiteId, entity, LoggedUserId, GetDateTime);

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

        //created for update task status from task list page by SN
        #region
        [HttpPost("projectTaskStatus")]
        public async Task<IActionResult> UpdateProjectTaskStatus(ProjectTaskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.TaskIds != null && model.TaskIds.Count > 0)
                    {
                        foreach (var taskIds in model.TaskIds)
                        {
                            var taskEntity = await _taskService.GetById(taskIds);
                            if (taskEntity != null)
                            {
                                bool result = await UpdateTaskDetails(taskIds, model.StatusId, "status");

                                var taskDetails = await _taskService.GetProjectTaskDetailsById(taskIds);

                                var Status = await _dropDownService.GetDropDownById(model.StatusId);
                                _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", taskEntity.ProjectId, taskDetails.Project.Name, taskEntity.Id, taskEntity.Name, "Task Status", Status.DropDownValue, LoggedUserId, GetDateTime);

                                await UpdateProjectSwimlaneListTask(SiteId, taskEntity, LoggedUserId, GetDateTime);
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
        #endregion

        #region UpdateProjectTaskEndDate
        [HttpPut("end-date/{id}/{endDateStr}/{startDateStr}")]
        public async Task<IActionResult> UpdateProjectTaskEndDate(string id, string endDateStr, string startDateStr = null)
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
                    var entity = await _taskService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project task found with the specified id."));

                    var endDate = endDateStr.Replace("-", "/");
                    bool IsTaskDueDateChanged = !string.IsNullOrEmpty(endDate) && DateTime.ParseExact(endDate, "MM/dd/yyyy", null) != entity.EndDate;

                    entity.EndDate = DateTime.ParseExact(endDate, "MM/dd/yyyy", null);

                    if (startDateStr != "" && startDateStr != null && startDateStr != "null")
                    {
                        var startDate = startDateStr.Replace("-", "/");
                        entity.StartDate = DateTime.ParseExact(startDate, "MM/dd/yyyy", null);
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _taskService.UpdateProjectTask(entity);

                    var taskDetails = await _taskService.GetProjectTaskDetailsById(id);
                    if (IsTaskDueDateChanged)
                    {
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", entity.ProjectId, taskDetails.Project.Name, entity.Id, entity.Name, "Due Date", endDate, LoggedUserId, GetDateTime);
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

        #region UpdateTaskOwner
        [HttpPut("task-owner/{id}/{assignedToId}")]
        public async Task<IActionResult> UpdateTaskOwner(string id, string assignedToId)
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
                    var entity = await _taskService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No task found with the specified id."));

                    entity.AssignedToId = assignedToId != "null" ? assignedToId : null;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _taskService.UpdateProjectTask(entity);
                    
                    if (!string.IsNullOrEmpty(entity.AssignedToId))
                    {
                        var taskDetails = await _taskService.GetProjectTaskDetailsById(id);
                        var assignedTo = await _employeeService.GetEmployeeDetailsById(assignedToId);
                        string Fullname = assignedTo.Person.FirstName + " " + assignedTo.Person.LastName;

                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "ProjectTasks", entity.ProjectId, taskDetails.Project.Name, entity.Id, entity.Name, "Task Owner", Fullname, LoggedUserId, GetDateTime);
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

        #region DeleteProjectTask
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectTask(string id)
        {
            try
            {
                var entity = await _taskService.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No project task found with the specified id."));

                _taskService.DeleteProjectTask(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("task-file/{id}")]
        public async Task<IActionResult> DeleteProjectTaskFile(string id)
        {
            try
            {
                var entity = await _projectTaskFilesService.GetProjectTaskFileById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No file found with the specified id."));

                _projectTaskFilesService.DeleteProjectTaskFile(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region SaveBulkTasks
        [HttpPost("saveBulkTasks")]
        public async Task<IActionResult> SaveBulkTasks(ProjectModuleModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var taskEntities = new List<ProjectTask>();
                    int lastTaskNumber = await _taskService.GetLastProjectTaskNumber() + 1;
                    var newStatusId = "";
                    var lowPriorityId = "";
                    bool isConverted = model.IsIssueConverted || model.IsRequirementConverted;
                    bool isNotConverted = !model.IsIssueConverted && !model.IsRequirementConverted;

                    if (isConverted)
                    {
                        newStatusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Task Status", "New");
                        lowPriorityId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Task Priorities", "Low");
                    }
                    int moduleSortOrder = 0;
                    if (!isConverted)
                    {
                        // Get module sort order
                        var module = await _projectModuleService.GetProjectModuleDetailsById(model.ProjectModuleId);

                        if (module == null)
                            throw new Exception("Module not found");

                        moduleSortOrder = module.SortOrder;
                    }

                    foreach (var item in model.ProjectTaskModel)
                    {
                        if (item.Flag != "Delete")
                        {
                            string projectId = model.ProjectId;
                            string projectModuleId = model.ProjectModuleId;
                            string name = item.Name;
                            string StartDateStr = item.StartDateStr;
                            string EndDateStr = item.EndDateStr;
                            string Description = item.Description;

                            if (isConverted)
                            {
                                if (!string.IsNullOrWhiteSpace(item.IssueId))
                                {
                                    var issueData = await _issueService.GetIssueDetailsById(item.IssueId);
                                    if (issueData == null)
                                        continue;

                                    var module = await _projectModuleService.GetProjectModuleDetailsById(issueData.ProjectModule.Id);
                                    if (module == null)
                                        throw new Exception("Module not found");


                                    moduleSortOrder = module.SortOrder;
                                    projectId = issueData.Project.Id;
                                    projectModuleId = issueData.ProjectModule.Id;
                                    name = issueData.Name;
                                    StartDateStr = issueData.ProjectModule?.StartDate?.ToString("MM/dd/yyyy") ?? issueData.Project?.StartDate?.ToString("MM/dd/yyyy");
                                    EndDateStr = issueData.ProjectModule?.EndDate?.ToString("MM/dd/yyyy") ?? issueData.Project?.GoLiveDate?.ToString("MM/dd/yyyy");
                                    Description = StripHtmlTextOnly(issueData.Description);
                                }
                                else if (!string.IsNullOrWhiteSpace(item.RequirementId))
                                {
                                    var requirementData = await _requirementService.GetRequirementDetailsById(item.RequirementId);
                                    if (requirementData == null)
                                        continue;

                                    var module = await _projectModuleService.GetProjectModuleDetailsById(requirementData.ProjectModule.Id);
                                    if (module == null)
                                        throw new Exception("Module not found");

                                    
                                    moduleSortOrder = module.SortOrder;
                                    projectId = requirementData.Project.Id;
                                    projectModuleId = requirementData.ProjectModule.Id;
                                    name = requirementData.Title;
                                    StartDateStr = requirementData.ProjectModule?.StartDate?.ToString("MM/dd/yyyy") ?? requirementData.Project?.StartDate?.ToString("MM/dd/yyyy");
                                    EndDateStr = requirementData.ProjectModule?.EndDate?.ToString("MM/dd/yyyy") ?? requirementData.Project?.GoLiveDate?.ToString("MM/dd/yyyy");
                                    Description = StripHtmlTextOnly(requirementData.Description);
                                }
                            }

                            var exists = await _taskService.GetProjectTaskByName(name, projectId, projectModuleId);
                            if (exists != null && isNotConverted)
                                continue;

                            var entity = _mapper.Map<ProjectTask>(item);

                            if (exists != null && isConverted)
                            {
                                int counter = 1;
                                string baseName = name;
                                string newName;

                                do
                                {
                                    newName = $"{baseName}_{counter}";
                                    var checkName = await _taskService.GetProjectTaskByName(newName, projectId, projectModuleId);
                                    if (checkName == null)
                                        break;

                                    counter++;
                                } while (true);

                                entity.Name = newName;
                            }
                            else
                                entity.Name = name;

                            decimal NextSortOrderOfProjectTask = 0;
                            // Get last task sort order under this module
                            var CurrentModuleSortOrder = await _taskService.GetLastSortOrderOfProjectTasks(projectModuleId);

                            // If no tasks found, lastTaskSortOrder will be 1.1m
                            if (CurrentModuleSortOrder == 1.1m)
                            {
                                NextSortOrderOfProjectTask = moduleSortOrder + 0.1m;
                            }
                            else
                            {
                                NextSortOrderOfProjectTask = Math.Round(CurrentModuleSortOrder + 0.1m, 1); // increment
                            }

                            entity.SiteId = SiteId;
                            entity.ProjectId = projectId;
                            entity.ProjectModuleId = projectModuleId;
                            entity.SortOrder = NextSortOrderOfProjectTask;
                            entity.ProjectTaskNumber = lastTaskNumber;
                            //entity.Description = Description;
                            if (isConverted)
                            {
                                entity.StatusId = newStatusId;
                                entity.PriorityId = lowPriorityId;
                            }

                            // Set custom properties
                            if (StartDateStr != "" && StartDateStr != null)
                                entity.StartDate = DateTime.ParseExact(StartDateStr, "MM/dd/yyyy", null);
                            if (EndDateStr != "" && EndDateStr != null)
                                entity.EndDate = DateTime.ParseExact(EndDateStr, "MM/dd/yyyy", null);

                            entity.Description = await _azureBlobImageServices
                                 .ProcessHtmlAndManageImagesAsync(
                                     item.Description,
                                     SiteData.Name,
                                     "project-tasks",
                                     lastTaskNumber.ToString()
                                 );

                            // Set the created by and created on properties
                            entity.CreatedById = LoggedUserId;
                            entity.UpdatedById = LoggedUserId;
                            entity.CreatedOnUtc = GetDateTime;
                            entity.UpdatedOnUtc = GetDateTime;

                            //taskEntities.Add(entity);
                            _taskService.InsertProjectTask(entity);
                            // Increment the last task number
                            lastTaskNumber++;

                            // If converted from issue
                            if (model.IsIssueConverted)
                            {
                                var projectTaskRelatedMapping = new ProjectTaskRelatedMapping
                                {
                                    TaskId = entity.Id,
                                    IssueId = item.IssueId,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };
                                _projectTaskRelatedMappingsService.InsertProjectTaskRelatedMapping(projectTaskRelatedMapping);
                            }

                            // If converted from Requirement
                            if (model.IsRequirementConverted)
                            {
                                var projectTaskRelatedMapping = new ProjectTaskRelatedMapping
                                {
                                    TaskId = entity.Id,
                                    RequirementId = item.RequirementId,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };
                                _projectTaskRelatedMappingsService.InsertProjectTaskRelatedMapping(projectTaskRelatedMapping);
                            }
                        }
                    }
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateBulkTasks
        [HttpPost("updateBulkTasks")]
        public async Task<IActionResult> UpdateBulkTasks(ProjectModuleModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (model.ProjectTaskModel.Count() > 0)
                {
                    var deleteList = new List<ProjectTask>();
                    var updateList = new List<ProjectTask>();
                    foreach (var item in model.ProjectTaskModel)
                    {
                        if (item.Flag == "Edit")
                        {
                            var entity = await _taskService.GetById(item.Id);
                            if (entity == null)
                                continue;

                            entity.ProjectId = model.ProjectId;
                            entity.ProjectModuleId = model.ProjectModuleId;
                            entity.Name = item.Name;
                            entity.StatusId = item.StatusId;
                            entity.PriorityId = item.PriorityId;
                            entity.EstimateTime = item.EstimateTime;

                            // Set custom properties
                            if (item.StartDateStr != "" && item.StartDateStr != null)
                                entity.StartDate = DateTime.ParseExact(item.StartDateStr, "MM/dd/yyyy", null);
                            if (item.EndDateStr != "" && item.EndDateStr != null)
                                entity.EndDate = DateTime.ParseExact(item.EndDateStr, "MM/dd/yyyy", null);

                            entity.Description = await _azureBlobImageServices
                                   .ProcessHtmlAndManageImagesAsync(
                                       item.Description,
                                       SiteData.Name,
                                       "project-tasks",
                                       entity.ProjectTaskNumber.ToString(),
                                       entity.Description
                                   );

                            entity.UpdatedById = LoggedUserId;
                            entity.UpdatedOnUtc = GetDateTime;
                            updateList.Add(entity);
                        }
                        else if (item.Flag == "Delete")
                        {
                            var entity = await _taskService.GetById(item.Id);
                            if (entity == null)
                                continue;

                            deleteList.Add(entity);
                        }
                    }

                    if (updateList.Count > 0)
                        _taskService.UpdateProjectTaskList(updateList);

                    if (deleteList.Count > 0)
                        _taskService.DeleteProjectTaskList(deleteList);

                }
                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region Update
        [HttpPut("{id}/task")]
        public async Task<IActionResult> Update(string id, [FromBody] ProjectTaskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();                    
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = _taskService.GetProjectTaskByName(model.Name, model.ProjectId, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Task name already exists, try with another."));

                    var existingTask = await _taskService.GetById(id);
                    if (existingTask == null)
                        return BadRequest(new BadRequestError("No record found with the specified id."));

                    existingTask = _mapper.Map(model, existingTask);

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        existingTask.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "project-tasks",
                                existingTask.ProjectTaskNumber.ToString(),
                                existingTask.Description
                            );
                    }

                    existingTask.UpdatedById = LoggedUserId;
                    existingTask.UpdatedOnUtc = GetDateTime;
                    _taskService.UpdateProjectTask(existingTask);

                    var addList = new List<ProjectActivity>();
                    var deleteList = new List<ProjectActivity>();
                    var updateList = new List<ProjectActivity>();

                    foreach (var activity in model.ProjectActivityModel)
                    {
                        if (activity.Flag == "Edit")
                        {
                            var existingActivity = await _activityService.GetById(activity.Id);

                            if (existingActivity == null)
                            {
                                continue;
                            }
                            existingActivity = _mapper.Map(activity, existingActivity);

                            if (!string.IsNullOrEmpty(activity.Description))
                            {
                                existingActivity.Description = await _azureBlobImageServices
                                    .ProcessHtmlAndManageImagesAsync(
                                        activity.Description,
                                        SiteData.Name,
                                        "project-tasks-activities",
                                        existingActivity.Id,
                                        existingActivity.Description
                                    );
                            }

                            existingActivity.UpdatedOnUtc = GetDateTime;
                            existingActivity.UpdatedById = LoggedUserId;
                            updateList.Add(existingActivity);
                        }

                        else if (activity.Flag == "New")
                        {
                            var existingActivity = await _activityService.GetById(activity.Id);
                            if (existingActivity != null)
                            {
                                continue;
                            }

                            var activityEntity = _mapper.Map<ProjectActivity>(activity);
                            activityEntity.Id = Guid.NewGuid().ToString();

                            if (!string.IsNullOrEmpty(activity.Description))
                            {
                                activityEntity.Description = await _azureBlobImageServices
                                    .ProcessHtmlAndManageImagesAsync(
                                        activity.Description,
                                        SiteData.Name,
                                        "project-tasks-activities",
                                        activityEntity.Id
                                    );
                            }

                            activityEntity.SiteId = SiteId;
                            activityEntity.Active = true;
                            activityEntity.CreatedById = LoggedUserId;
                            activityEntity.CreatedOnUtc = GetDateTime;
                            addList.Add(activityEntity);
                        }
                        else if (activity.Flag == "Delete")
                        {
                            var existingActivity = await _activityService.GetById(activity.Id);
                            if (existingActivity == null)
                            {
                                continue;
                            }
                            deleteList.Add(existingActivity);
                        }
                    }

                    if (addList.Count > 0)
                        _activityService.InsertProjectActivityList(addList);

                    if (updateList.Count > 0)
                        _activityService.UpdateProjectActivityList(updateList);

                    if (deleteList.Count > 0)
                        _activityService.DeleteProjectActivityList(deleteList);

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

        #region LabData
        [HttpPost("project-task")]
        public async Task<IActionResult> LabData([FromForm] TaskUploadModel model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (model.FileXls == null || model.FileXls.Length == 0)
                    return NoContent();

                var timestamp = long.Parse(DateTime.UtcNow.ToString("yyyyMMddHHmmss"));

                using (var reader = ExcelReaderFactory.CreateReader(model.FileXls.OpenReadStream()))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                    });

                    if (dataSet != null && dataSet.Tables.Count > 0)
                    {
                        var dataTable = dataSet.Tables[0];
                        if (dataTable != null)
                        {
                            var taskEntities = new List<ProjectTask>();
                            foreach (DataRow row in dataTable.Rows)
                            {
                                string projectName = row["project"].ToString();
                                var woTitle = row["WoTitle"].ToString();
                                var priority = row["Priority"].ToString();
                                var status = row["Status"].ToString();
                                var taskName = row["TaskName"].ToString();
                                var startDateStr = row["StartDate"].ToString();
                                var endDateStr = row["endDate"].ToString();
                                var dueDateStr = row["DueDate"].ToString();

                                DateTime? startDate = null;
                                DateTime? endDate = null;
                                DateTime? dueDate = null;

                                if (!string.IsNullOrEmpty(startDateStr) && DateTime.TryParse(startDateStr, out DateTime parsedStartDate))
                                {
                                    startDate = parsedStartDate;
                                }

                                if (!string.IsNullOrEmpty(endDateStr) && DateTime.TryParse(endDateStr, out DateTime parsedEndDate))
                                {
                                    endDate = parsedEndDate;
                                }

                                if (!string.IsNullOrEmpty(dueDateStr) && DateTime.TryParse(dueDateStr, out DateTime parsedDueDate))
                                {
                                    dueDate = parsedDueDate;
                                }

                                var project = await _projectService.GetProjectByName(SiteId, projectName, null);
                                var workOrderItem = await _workOrderService.GetProjectModuleByName(woTitle);
                                var priorityItem = await _dropDownService.GetByName(SiteId, priority);
                                var statusItem = await _dropDownService.GetByName(SiteId, status);

                                var item = new ProjectTask
                                {
                                    Name = taskName,
                                    ProjectId = project.Id,
                                    ProjectModuleId = workOrderItem.Id,
                                    PriorityId = priorityItem.Id,
                                    StatusId = statusItem.Id,
                                    StartDate = startDate,
                                    EndDate = endDate,
                                    DueDate = dueDate,
                                    Deleted = false,
                                    Active = true,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };

                                var entity = _mapper.Map<ProjectTask>(item);
                                taskEntities.Add(entity);
                            }
                            if (taskEntities.Count > 0)
                            {
                                _taskService.InsertProjectTaskList(taskEntities);
                            }
                        }
                    }
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CopyModuleToProjects
        [HttpPost("copyModuleToTask")]
        public async Task<IActionResult> CopyModuleToProjects(ProjectTaskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var moduleData = await _projectModuleService.GetProjectModuleById(model.ProjectModuleId);
                    if (moduleData != null)
                    {
                        if (model.ProjectIds != null && model.ProjectIds.Count > 0)
                        {
                            foreach (var projectid in model.ProjectIds)
                            {
                                var entity = new ProjectModule();

                                int NextSortOrderOfProjectModule = 0;
                                if (!string.IsNullOrWhiteSpace(projectid) && projectid != "undefined")
                                {
                                    NextSortOrderOfProjectModule = await _projectModuleService.GetLastSortOrderOfProjectModules(projectid) + 1;
                                }
                                entity.SiteId = SiteId;
                                entity.ProjectId = projectid;
                                entity.SortOrder = NextSortOrderOfProjectModule;
                                entity.ProjectModuleNumber = await _projectModuleService.GetLastProjectModuleNumber() + 1;
                                entity.ProjectModuleStatusId = moduleData.ProjectModuleStatusId;
                                entity.Name = moduleData.Name;
                                entity.Description = moduleData.Description;
                                entity.Notes = moduleData.Notes;
                                if (moduleData.StartDate != null)
                                    entity.StartDate = moduleData.StartDate;
                                if (moduleData.EndDate != null)
                                    entity.EndDate = moduleData.EndDate;
                                entity.CreatedById = LoggedUserId;
                                entity.UpdatedById = LoggedUserId;
                                entity.CreatedOnUtc = GetDateTime;
                                entity.UpdatedOnUtc = GetDateTime;
                                _projectModuleService.InsertProjectModule(entity);

                                // tasks
                                if (model.TaskIds != null && model.TaskIds.Count > 0)
                                {
                                    foreach (var taskIds in model.TaskIds)
                                    {
                                        var taskData = await _taskService.GetById(taskIds);
                                        if (taskData != null)
                                        {
                                            var taskEntity = new ProjectTask();

                                            int moduleSortOrder = entity.SortOrder;
                                            decimal NextSortOrderOfProjectTask = 0;
                                            // Get last task sort order under this module
                                            var CurrentModuleSortOrder = await _taskService.GetLastSortOrderOfProjectTasks(entity.Id);

                                            // If no tasks found, lastTaskSortOrder will be 1.1m
                                            if (CurrentModuleSortOrder == 1.1m)
                                            {
                                                NextSortOrderOfProjectTask = moduleSortOrder + 0.1m;
                                            }
                                            else
                                            {
                                                NextSortOrderOfProjectTask = Math.Round(CurrentModuleSortOrder + 0.1m, 1); // increment
                                            }

                                            taskEntity.SortOrder = NextSortOrderOfProjectTask;
                                            taskEntity.SiteId = SiteId;
                                            taskEntity.ProjectId = projectid;
                                            taskEntity.ProjectModuleId = entity.Id;
                                            taskEntity.StatusId = taskData.StatusId;
                                            taskEntity.PriorityId = taskData.PriorityId;
                                            taskEntity.ProjectTaskNumber = await _taskService.GetLastProjectTaskNumber() + 1;
                                            taskEntity.Name = taskData.Name;
                                            taskEntity.Description = taskData.Description;
                                            if (taskData.StartDate != null)
                                                taskEntity.StartDate = taskData.StartDate;
                                            if (taskData.EndDate != null)
                                                taskEntity.EndDate = taskData.EndDate;
                                            taskEntity.CreatedById = LoggedUserId;
                                            taskEntity.UpdatedById = LoggedUserId;
                                            taskEntity.CreatedOnUtc = GetDateTime;
                                            taskEntity.UpdatedOnUtc = GetDateTime;
                                            _taskService.InsertProjectTask(taskEntity);

                                            bool IsWorkboardUpdated = await UpdateProjectSwimlaneListTask(SiteId, taskEntity, LoggedUserId, GetDateTime);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return BadRequest(new BadRequestError("No module id is found."));
                    }
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return ModelStateError(ModelState);
        }

        [HttpPost("copyTaskToProjects")]
        public async Task<IActionResult> CopyTaskToProjects(ProjectTaskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var moduleData = await _projectModuleService.GetProjectModuleById(model.ProjectModuleId);
                    if (moduleData == null)
                        return BadRequest(new BadRequestError("No module id is found."));

                    if (model.ProjectIds != null && model.ProjectIds.Count > 0)
                    {
                        foreach (var projectid in model.ProjectIds)
                        {
                            var existModule = await _projectModuleService.GetProjectModuleByName(moduleData.Name, projectid);
                            ProjectModule entityModule;
                            // module
                            if (existModule != null)
                            {
                                if (moduleData.StartDate != null)
                                    existModule.StartDate = moduleData.StartDate;
                                if (moduleData.EndDate != null)
                                    existModule.EndDate = moduleData.EndDate;

                                existModule.ProjectId = projectid.ToString();
                                existModule.Name = moduleData.Name;
                                existModule.ProjectModuleStatusId = moduleData.ProjectModuleStatusId;
                                existModule.Description = moduleData.Description;
                                existModule.Notes = moduleData.Notes;
                                existModule.UpdatedById = LoggedUserId;
                                existModule.UpdatedOnUtc = GetDateTime;
                                _projectModuleService.UpdateProjectModule(existModule);
                                entityModule = existModule;
                            }
                            else
                            {
                                int NextSortOrderOfProjectModule = 0;
                                if (!string.IsNullOrWhiteSpace(projectid) && projectid != "undefined")
                                {
                                    NextSortOrderOfProjectModule = await _projectModuleService.GetLastSortOrderOfProjectModules(projectid) + 1;
                                }
                                entityModule = new ProjectModule
                                {
                                    ProjectModuleNumber = await _taskService.GetLastProjectTaskNumber() + 1,
                                    SiteId = SiteId,
                                    SortOrder = NextSortOrderOfProjectModule,
                                    ProjectId = projectid,
                                    StartDate = moduleData.StartDate,
                                    EndDate = moduleData.EndDate,
                                    Name = moduleData.Name,
                                    ProjectModuleStatusId = moduleData.ProjectModuleStatusId,
                                    Description = moduleData.Description,
                                    Notes = moduleData.Notes,
                                    CreatedById = LoggedUserId,
                                    UpdatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime,
                                    UpdatedOnUtc = GetDateTime
                                };

                                _projectModuleService.InsertProjectModule(entityModule);
                            }

                            // tasks
                            if (model.TaskId != null && model.TaskId != "")
                            {
                                var taskData = await _taskService.GetById(model.TaskId);
                                if (taskData != null)
                                {
                                    var existsTask = await _taskService.GetProjectTaskByName(taskData.Name, projectid, entityModule.Id);
                                    if (existsTask != null)
                                        return BadRequest(new BadRequestError("Task name already exists, try with another."));

                                    var taskEntity = new ProjectTask();
                                    int moduleSortOrder = entityModule.SortOrder;
                                    decimal NextSortOrderOfProjectTask = 0;
                                    // Get last task sort order under this module
                                    var CurrentModuleSortOrder = await _taskService.GetLastSortOrderOfProjectTasks(entityModule.Id);

                                    // If no tasks found, lastTaskSortOrder will be 1.1m
                                    if (CurrentModuleSortOrder == 1.1m)
                                    {
                                        NextSortOrderOfProjectTask = moduleSortOrder + 0.1m;
                                    }
                                    else
                                    {
                                        NextSortOrderOfProjectTask = Math.Round(CurrentModuleSortOrder + 0.1m, 1); // increment
                                    }

                                    taskEntity.SortOrder = NextSortOrderOfProjectTask;
                                    taskEntity.SiteId = SiteId;
                                    taskEntity.ProjectId = projectid.ToString();
                                    taskEntity.ProjectModuleId = entityModule.Id;
                                    taskEntity.StatusId = taskData.StatusId;
                                    taskEntity.PriorityId = taskData.PriorityId;
                                    taskEntity.ProjectTaskNumber = await _taskService.GetLastProjectTaskNumber() + 1;
                                    taskEntity.Name = taskData.Name;
                                    taskEntity.Description = taskData.Description;
                                    if (taskData.StartDate != null)
                                        taskEntity.StartDate = taskData.StartDate;
                                    if (taskData.EndDate != null)
                                        taskEntity.EndDate = taskData.EndDate;
                                    taskEntity.CreatedById = LoggedUserId;
                                    taskEntity.UpdatedById = LoggedUserId;
                                    taskEntity.CreatedOnUtc = GetDateTime;
                                    taskEntity.UpdatedOnUtc = GetDateTime;
                                    _taskService.InsertProjectTask(taskEntity);

                                    bool IsWorkboardUpdated = await UpdateProjectSwimlaneListTask(SiteId, taskEntity, LoggedUserId, GetDateTime);
                                }
                            }
                        }
                    }
                    if (model.IsCopyOrMove == "isMove")
                    {
                        var existingTask = await _taskService.GetById(model.TaskId);
                        if (existingTask != null)
                        {
                            existingTask.IsMoved = true;
                            existingTask.UpdatedById = LoggedUserId;
                            existingTask.UpdatedOnUtc = GetDateTime;
                            _taskService.UpdateProjectTask(existingTask);
                        }
                    }

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region Update Project Workboard ListTasks
        private async Task<bool> UpdateProjectSwimlaneListTask(string SiteId, ProjectTask model, string LoggedUserId, DateTime CdateTime)
        {
            bool IsError = true;
            var Data = await _projectService.GettWorkBoardByProjecId(SiteId, model.ProjectId, CdateTime);
            var ProjectTaskStatus = await _dropDownService.GetDropDownById(model.StatusId);

            if (Data != null)
            {
                var DefaultSwimlaneData = Data.ProjectSwimLanes.FirstOrDefault(m => !m.Deleted && m.SwimlaneType.DropDownValue == "Task Status As Lists");
                if (DefaultSwimlaneData != null)
                {
                    var ListData = DefaultSwimlaneData.ProjectSwimLanesList.FirstOrDefault(m => !m.Deleted && m.Name == ProjectTaskStatus.DropDownValue);
                    if (ListData != null)
                    {
                        var ListTaskData = DefaultSwimlaneData.ProjectSwimLanesList.SelectMany(list => list.ProjectSwimLanesListsTasks).FirstOrDefault(task => !task.Deleted && task.ProjectTaskId == model.Id);
                        string ListTaskId = ListTaskData != null ? ListTaskData.Id : "";
                        if (!string.IsNullOrEmpty(ListTaskId))
                        {
                            var ListTask = await _projectSwimLanesListsTasksServices.GetById(ListTaskId);
                            ListTask.ProjectSwimlaneListId = ListData.Id;
                            ListTask.UpdatedById = LoggedUserId;
                            ListTask.UpdatedOnUtc = CdateTime;
                            _projectSwimLanesListsTasksServices.UpdateProjectSwimLaneListsTasks(ListTask);
                            IsError = false;
                        }
                        else
                        {
                            var NewListTask = new ProjectSwimLanesListsTasks();
                            NewListTask.ProjectSwimlaneListId = ListData.Id;
                            NewListTask.ProjectTaskId = model.Id;
                            NewListTask.CreatedById = LoggedUserId;
                            NewListTask.CreatedOnUtc = CdateTime;
                            NewListTask.UpdatedById = LoggedUserId;
                            NewListTask.UpdatedOnUtc = CdateTime;
                            NewListTask.ProjectTask = null;
                            _projectSwimLanesListsTasksServices.InsertProjectSwimLaneListsTasks(NewListTask);
                            IsError = false;
                        }
                    }
                }
            }
            return IsError;
        }
        #endregion

        #region UpdateTaskColor
        [HttpPost("update-task-color")]
        public async Task<IActionResult> UpdateTaskColor(ProjectTaskModel model)
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
                    var entity = await _taskService.GetById(model.Id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No task found with the specified id."));

                    //entity.ProjectColor = model.ProjectColor;
                    if (!string.IsNullOrEmpty(model.Color) && model.Color.StartsWith("#"))
                    {
                        var color = System.Drawing.ColorTranslator.FromHtml(model.Color);
                        entity.Color = $"rgb({color.R}, {color.G}, {color.B})";
                    }
                    else
                    {
                        entity.Color = model.Color; // fallback
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _taskService.UpdateProjectTask(entity);

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

        #region private functions
        private List<TaskCalendarColumnModel> GenerateColumns(ProjectTaskSearchModel searchModel, DateTime CalendarMonth)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var columns = new List<TaskCalendarColumnModel>();
            var now = searchModel.StartDateStr.HasValue ? searchModel.StartDateStr.Value : CalendarMonth;
            var viewTypeData = _dropDownService.GetDropDownById(searchModel.ViewType).GetAwaiter().GetResult();

            switch (viewTypeData?.DropDownValue.ToLower())
            {
                case "day":
                    DateTime startDate = now.Date;
                    DateTime endDate = now.Date;
                    if (searchModel.StartDateStr.HasValue)
                    {
                        // get month end date
                        endDate = now.Date.AddDays(30);
                    }
                    else
                    {
                        startDate = startDate.AddDays(-15);
                        // get month end date
                        endDate = endDate.AddDays(15);
                    }
                    // As per offset display previous or next days
                    if (searchModel.Offset != 0)
                    {
                        startDate = startDate.AddDays(searchModel.Offset);
                        endDate = endDate.AddDays(searchModel.Offset);
                    }

                    int index = 1;
                    for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        columns.Add(new TaskCalendarColumnModel
                        {
                            Index = index++,
                            Date = date,
                            FilterType = "day"
                        });
                    }
                    break;

                case "week":
                    // Get current week's start (Sunday) and end (Saturday)
                    var currentWeekStart = now.Date.AddDays(-(int)now.DayOfWeek);
                    var currentWeekEnd = currentWeekStart.AddDays(6);

                    if (searchModel.StartDateStr.HasValue)
                    {
                        // Align StartDateStr to the start of its week
                        startDate = searchModel.StartDateStr.Value.Date.AddDays(
                            -(int)searchModel.StartDateStr.Value.DayOfWeek
                        );

                        if (searchModel.Offset < 0)
                        {
                            // Previous: include the week that ends exactly before the current start
                            endDate = startDate.AddDays(-1);
                            startDate = endDate.AddDays(-(17 * 7) + 1); // go 17 weeks back (inclusive)
                        }
                        else
                        {
                            // Next or default: show 17 full weeks starting from startDate
                            endDate = startDate.AddDays(17 * 7 - 1);
                        }
                    }
                    else
                    {
                        // Initial load: 8 weeks back and forward
                        startDate = currentWeekStart.AddDays(-8 * 7);
                        endDate = currentWeekEnd.AddDays(8 * 7);

                        if (searchModel.Offset != 0)
                        {
                            startDate = startDate.AddDays(searchModel.Offset * 7);
                            endDate = endDate.AddDays(searchModel.Offset * 7);
                        }
                    }

                    // Build week columns from startDate to endDate
                    var current = startDate;
                    while (current <= endDate)
                    {
                        var start = current;
                        var end = current.AddDays(6); // full week

                        columns.Add(new TaskCalendarColumnModel
                        {
                            Index = columns.Count + 1,
                            Date = start,
                            DisplayDateRange = $"{start:MM/dd} - {end:MM/dd}",
                            DateTooltip = $"{start:MM/dd/yyyy} - {end:MM/dd/yyyy}",
                            FilterType = "week"
                        });

                        current = end.AddDays(1);
                    }
                    break;

                case "month":
                    //columns for month (display all months)
                    for (int i = 1; i <= 12; i++)
                    {
                        var start = new DateTime(now.Year, i, 1);
                        var end = start.AddMonths(1).AddDays(-1);

                        columns.Add(new TaskCalendarColumnModel
                        {
                            Index = i,
                            Date = start,
                            DateTooltip = $"{start:MM/dd/yyyy} - {end:MM/dd/yyyy} ({start:MMM})",
                            FilterType = "month"
                        });
                    }
                    break;
            }

            return columns;
        }
        private async Task<List<TaskCalendarRowModel>> GenerateTaskRows(ProjectTaskSearchModel searchModel, DateTime CalendarMonth)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            if (searchModel.ProjectIds != null && searchModel.ProjectIds.Count > 1)
                searchModel.SortBy = "calendar";

            var fullPageSize = int.MaxValue;
            var tasks = await _taskService.GetAllProjectTasks(SiteId, LoggedUserId, searchModel.SearchText, searchModel.IsTemplate, searchModel.ProjectTaskNumber, searchModel.ProjectIds, searchModel.ProjectModuleIds, null, searchModel.ProjectLeadsIds, searchModel.StatusIds, searchModel.PriorityIds, searchModel.CustomerIds, searchModel.CompanyContactIds, searchModel.ActivityOwners, searchModel.Name, searchModel.TaskTagsIds, searchModel.SortBy, searchModel.Sorts, searchModel.Descending, 1, fullPageSize);

            // selected month 
            //var now = CalendarMonth;
            var now = searchModel.StartDateStr.HasValue ? searchModel.StartDateStr.Value : CalendarMonth;

            var viewTypeData = await _dropDownService.GetDropDownById(searchModel.ViewType);
            if (viewTypeData?.DropDownValue.ToLower() == "day")
            {
                DateTime monthStart = now.Date;
                DateTime monthEnd = now.Date;
                if (searchModel.StartDateStr.HasValue)
                {
                    // get month end date
                    monthEnd = now.Date.AddDays(30);
                }
                else
                {
                    monthStart = monthStart.AddDays(-15);
                    // get month end date
                    monthEnd = monthEnd.AddDays(15);
                }

                // As per offset display previous or next days
                if (searchModel.Offset != 0)
                {
                    monthStart = monthStart.AddDays(searchModel.Offset);
                    monthEnd = monthEnd.AddDays(searchModel.Offset);
                }
                //get task by monthStart and monthEnd date
                var filtered = tasks
                    .Where(task =>
                         task.StartDate.HasValue && task.EndDate.HasValue &&
                         task.StartDate.Value.Date <= monthEnd &&
                         task.EndDate.Value.Date >= monthStart
                    )
                    .Select(task =>
                    {
                        var clampedStart = task.StartDate.Value.Date < monthStart ? monthStart : task.StartDate.Value.Date;
                        var clampedEnd = task.EndDate.Value.Date > monthEnd ? monthEnd : task.EndDate.Value.Date;

                        // Correctly align with inclusive column dates
                        int before = (clampedStart - monthStart).Days;              // Number of days before start
                        int span = (clampedEnd - clampedStart).Days + 1;            // Inclusive range
                        int after = (monthEnd - clampedEnd).Days;                   // Days after the end

                        //return row with data
                        return new TaskCalendarRowModel
                        {
                            Task = task,
                            BeforeStartDateCount = before < 0 ? 0 : before,
                            TaskDurationColspan = span,
                            AfterEndDateCount = after < 0 ? 0 : after
                        };
                    })
                    .ToList();

                return filtered;
            }
            else if (viewTypeData?.DropDownValue.ToLower() == "week")
            {
                var weekRanges = new List<(DateTime Start, DateTime End)>();

                var currentWeekStart = now.Date.AddDays(-(int)now.DayOfWeek);
                var currentWeekEnd = currentWeekStart.AddDays(6);

                DateTime startDate;
                DateTime endDate;

                if (searchModel.StartDateStr.HasValue)
                {
                    // Align StartDateStr to the start of its week
                    startDate = searchModel.StartDateStr.Value.Date.AddDays(
                        -(int)searchModel.StartDateStr.Value.DayOfWeek
                    );

                    if (searchModel.Offset < 0)
                    {
                        // Previous: include the week that ends exactly before the current start
                        endDate = startDate.AddDays(-1);
                        startDate = endDate.AddDays(-(17 * 7) + 1); // go 17 weeks back (inclusive)
                    }
                    else
                    {
                        // Next or default: show 17 full weeks starting from startDate
                        endDate = startDate.AddDays(17 * 7 - 1);
                    }
                }
                else
                {
                    // Initial load: 8 weeks back and forward
                    startDate = currentWeekStart.AddDays(-8 * 7);
                    endDate = currentWeekEnd.AddDays(8 * 7);

                    if (searchModel.Offset != 0)
                    {
                        startDate = startDate.AddDays(searchModel.Offset * 7);
                        endDate = endDate.AddDays(searchModel.Offset * 7);
                    }
                }
                // Build all week ranges from startDate to endDate
                var first = startDate;
                while (first <= endDate)
                {
                    var start = first;
                    var end = first.AddDays(6); // full week
                    weekRanges.Add((start, end));
                    first = end.AddDays(1);
                }

                // Month start and end used for filtering
                var monthStart = startDate;
                var monthEnd = endDate;

                var filtered = tasks
                    .Where(task =>
                        task.StartDate.HasValue && task.EndDate.HasValue &&
                        task.StartDate.Value <= monthEnd &&
                        task.EndDate.Value >= monthStart
                    )
                    .Select(task =>
                    {
                        var clippedStart = task.StartDate.Value < monthStart ? monthStart : task.StartDate.Value;
                        var clippedEnd = task.EndDate.Value > monthEnd ? monthEnd : task.EndDate.Value;

                        int startWeekIndex = weekRanges.FindIndex(r => r.End >= clippedStart);
                        int endWeekIndex = weekRanges.FindLastIndex(r => r.Start <= clippedEnd);

                        int before = startWeekIndex;
                        int span = endWeekIndex - startWeekIndex + 1;
                        int after = weekRanges.Count - endWeekIndex - 1;

                        return new TaskCalendarRowModel
                        {
                            Task = task,
                            BeforeStartDateCount = before > 0 ? before : 0,
                            TaskDurationColspan = span,
                            AfterEndDateCount = after > 0 ? after : 0
                        };
                    })
                    .ToList();

                return filtered;
            }
            else if (viewTypeData?.DropDownValue.ToLower() == "month")
            {
                var monthRanges = new List<(DateTime Start, DateTime End)>();
                for (int i = 1; i <= 12; i++)
                {
                    var start = new DateTime(now.Year, i, 1);
                    var end = start.AddMonths(1).AddDays(-1);
                    monthRanges.Add((start, end));
                }

                var yearStart = new DateTime(now.Year, 1, 1);
                var yearEnd = new DateTime(now.Year, 12, 31);

                var filtered = tasks
                    .Where(task =>
                        task.StartDate.HasValue && task.EndDate.HasValue &&
                        task.StartDate.Value <= yearEnd &&
                        task.EndDate.Value >= yearStart
                    )
                    .Select(task =>
                    {
                        var clippedStart = task.StartDate.Value < yearStart ? yearStart : task.StartDate.Value;
                        var clippedEnd = task.EndDate.Value > yearEnd ? yearEnd : task.EndDate.Value;

                        int startMonthIndex = monthRanges.FindIndex(r => r.End >= clippedStart);
                        int endMonthIndex = monthRanges.FindLastIndex(r => r.Start <= clippedEnd);

                        int before = startMonthIndex;
                        int span = endMonthIndex - startMonthIndex + 1;
                        int after = monthRanges.Count - endMonthIndex - 1;

                        return new TaskCalendarRowModel
                        {
                            Task = task,
                            BeforeStartDateCount = before > 0 ? before : 0,
                            TaskDurationColspan = span,
                            AfterEndDateCount = after > 0 ? after : 0
                        };
                    })
                    .ToList();

                return filtered;
            }

            return new List<TaskCalendarRowModel>();
        }
        private async Task<List<ModuleCalendarRowModel>> GenerateModuleRows(ProjectTaskSearchModel searchModel, DateTime CalendarMonth)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            if (searchModel.ProjectIds != null && searchModel.ProjectIds.Count > 1)
                searchModel.SortBy = "calendar";

            var fullPageSize = int.MaxValue;
            var modules = _projectModuleService.GetAllProjectModules(SiteId, searchModel.SearchText, searchModel.ProjectIds, null, searchModel.ProjectModuleStatusIds, searchModel.ProjectId, searchModel.CustomerIds, searchModel.CompanyContactIds, searchModel.isShowCloseStatus, null, searchModel.SortBy, searchModel.Descending, 1, fullPageSize);
            // selected month 
            //var now = CalendarMonth;
            var now = searchModel.StartDateStr.HasValue ? searchModel.StartDateStr.Value : CalendarMonth;

            var viewTypeData = await _dropDownService.GetDropDownById(searchModel.ViewType);
            if (viewTypeData?.DropDownValue.ToLower() == "day")
            {
                DateTime monthStart = now.Date;
                DateTime monthEnd = now.Date;
                if (searchModel.StartDateStr.HasValue)
                {
                    // get month end date
                    monthEnd = now.Date.AddDays(30);
                }
                else
                {
                    monthStart = monthStart.AddDays(-15);
                    // get month end date
                    monthEnd = monthEnd.AddDays(15);
                }

                // As per offset display previous or next days
                if (searchModel.Offset != 0)
                {
                    monthStart = monthStart.AddDays(searchModel.Offset);
                    monthEnd = monthEnd.AddDays(searchModel.Offset);
                }
                //get task by monthStart and monthEnd date
                var filtered = modules
                    .Where(modules =>
                         modules.StartDate.HasValue && modules.EndDate.HasValue &&
                         modules.StartDate.Value.Date <= monthEnd &&
                         modules.EndDate.Value.Date >= monthStart
                    )
                    .Select(modules =>
                    {
                        var clampedStart = modules.StartDate.Value.Date < monthStart ? monthStart : modules.StartDate.Value.Date;
                        var clampedEnd = modules.EndDate.Value.Date > monthEnd ? monthEnd : modules.EndDate.Value.Date;

                        // Correctly align with inclusive column dates
                        int before = (clampedStart - monthStart).Days;              // Number of days before start
                        int span = (clampedEnd - clampedStart).Days + 1;            // Inclusive range
                        int after = (monthEnd - clampedEnd).Days;                   // Days after the end

                        //return row with data
                        return new ModuleCalendarRowModel
                        {
                            Module = modules,
                            BeforeStartDateCount = before < 0 ? 0 : before,
                            ModuleDurationColspan = span,
                            AfterEndDateCount = after < 0 ? 0 : after
                        };
                    })
                    .ToList();

                return filtered;
            }
            else if (viewTypeData?.DropDownValue.ToLower() == "week")
            {
                var weekRanges = new List<(DateTime Start, DateTime End)>();

                var currentWeekStart = now.Date.AddDays(-(int)now.DayOfWeek);
                var currentWeekEnd = currentWeekStart.AddDays(6);

                DateTime startDate;
                DateTime endDate;

                if (searchModel.StartDateStr.HasValue)
                {
                    // Align StartDateStr to the start of its week
                    startDate = searchModel.StartDateStr.Value.Date.AddDays(
                        -(int)searchModel.StartDateStr.Value.DayOfWeek
                    );

                    if (searchModel.Offset < 0)
                    {
                        // Previous: include the week that ends exactly before the current start
                        endDate = startDate.AddDays(-1);
                        startDate = endDate.AddDays(-(17 * 7) + 1); // go 17 weeks back (inclusive)
                    }
                    else
                    {
                        // Next or default: show 17 full weeks starting from startDate
                        endDate = startDate.AddDays(17 * 7 - 1);
                    }
                }
                else
                {
                    // Initial load: 8 weeks back and forward
                    startDate = currentWeekStart.AddDays(-8 * 7);
                    endDate = currentWeekEnd.AddDays(8 * 7);

                    if (searchModel.Offset != 0)
                    {
                        startDate = startDate.AddDays(searchModel.Offset * 7);
                        endDate = endDate.AddDays(searchModel.Offset * 7);
                    }
                }
                // Build all week ranges from startDate to endDate
                var first = startDate;
                while (first <= endDate)
                {
                    var start = first;
                    var end = first.AddDays(6); // full week
                    weekRanges.Add((start, end));
                    first = end.AddDays(1);
                }

                // Month start and end used for filtering
                var monthStart = startDate;
                var monthEnd = endDate;

                var filtered = modules
                    .Where(modules =>
                        modules.StartDate.HasValue && modules.EndDate.HasValue &&
                        modules.StartDate.Value <= monthEnd &&
                        modules.EndDate.Value >= monthStart
                    )
                    .Select(modules =>
                    {
                        var clippedStart = modules.StartDate.Value < monthStart ? monthStart : modules.StartDate.Value;
                        var clippedEnd = modules.EndDate.Value > monthEnd ? monthEnd : modules.EndDate.Value;

                        int startWeekIndex = weekRanges.FindIndex(r => r.End >= clippedStart);
                        int endWeekIndex = weekRanges.FindLastIndex(r => r.Start <= clippedEnd);

                        int before = startWeekIndex;
                        int span = endWeekIndex - startWeekIndex + 1;
                        int after = weekRanges.Count - endWeekIndex - 1;

                        return new ModuleCalendarRowModel
                        {
                            Module = modules,
                            BeforeStartDateCount = before > 0 ? before : 0,
                            ModuleDurationColspan = span,
                            AfterEndDateCount = after > 0 ? after : 0
                        };
                    })
                    .ToList();

                return filtered;
            }
            else if (viewTypeData?.DropDownValue.ToLower() == "month")
            {
                var monthRanges = new List<(DateTime Start, DateTime End)>();
                for (int i = 1; i <= 12; i++)
                {
                    var start = new DateTime(now.Year, i, 1);
                    var end = start.AddMonths(1).AddDays(-1);
                    monthRanges.Add((start, end));
                }

                var yearStart = new DateTime(now.Year, 1, 1);
                var yearEnd = new DateTime(now.Year, 12, 31);

                var filtered = modules
                    .Where(modules =>
                        modules.StartDate.HasValue && modules.EndDate.HasValue &&
                        modules.StartDate.Value <= yearEnd &&
                        modules.EndDate.Value >= yearStart
                    )
                    .Select(modules =>
                    {
                        var clippedStart = modules.StartDate.Value < yearStart ? yearStart : modules.StartDate.Value;
                        var clippedEnd = modules.EndDate.Value > yearEnd ? yearEnd : modules.EndDate.Value;

                        int startMonthIndex = monthRanges.FindIndex(r => r.End >= clippedStart);
                        int endMonthIndex = monthRanges.FindLastIndex(r => r.Start <= clippedEnd);

                        int before = startMonthIndex;
                        int span = endMonthIndex - startMonthIndex + 1;
                        int after = monthRanges.Count - endMonthIndex - 1;

                        return new ModuleCalendarRowModel
                        {
                            Module = modules,
                            BeforeStartDateCount = before > 0 ? before : 0,
                            ModuleDurationColspan = span,
                            AfterEndDateCount = after > 0 ? after : 0
                        };
                    })
                    .ToList();

                return filtered;
            }

            return new List<ModuleCalendarRowModel>();
        }
        private async Task<bool> UpdateTaskDetails(string taskId, object data, string flag)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            var statusLog = new ProjectTaskStatusLog();
            var entity = await _taskService.GetById(taskId);
            if (entity == null)
                return false;

            switch (flag.ToLower())
            {
                case "status":
                    if (data is string statusId)
                        entity.StatusId = statusId;

                    statusLog.StatusId = data.ToString();
                    statusLog.TaskId = entity.Id;
                    //statusLog.StatusChangedBy = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                    statusLog.StatusChangedBy = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                    statusLog.StatusChangedDate = GetDateTime;
                    _projectTaskStatusLogService.InsertProjectTaskStatusLog(statusLog);

                    break;

                case "priority":
                    if (data is string priorityId)
                        entity.PriorityId = priorityId;
                    break;

                default:
                    return false;
            }

            entity.UpdatedById = LoggedUserId;
            entity.UpdatedOnUtc = GetDateTime;
            _taskService.UpdateProjectTask(entity);

            return true;
        }
        private string StripHtmlTextOnly(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove <img> and <a> entirely (including their content)
            var nodesToRemove = doc.DocumentNode.SelectNodes("//img | //a");
            if (nodesToRemove != null)
            {
                foreach (var node in nodesToRemove)
                {
                    node.Remove(); // Removes the tag and any inner text/links
                }
            }

            // Keep all other tags and structure intact
            return doc.DocumentNode.InnerHtml.Trim();
        }
        #endregion
    }
}