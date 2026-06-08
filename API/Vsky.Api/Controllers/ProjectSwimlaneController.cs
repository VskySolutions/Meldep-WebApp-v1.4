using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.ProjectModules;
using Vsky.Services.Projects;
using Vsky.Services.ProjectSwimLanes;
using Vsky.Services.ProjectTasks;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;

namespace Vsky.Api.Controllers
{
    [Route("project-swimlane")]
    public class ProjectSwimlaneController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly ISiteService _siteService;
        private readonly IDropDownService _dropDownService;
        private readonly ICommonService _commonService;
        private readonly IProjectService _projectService;
        private readonly IProjectModuleService _projectModuleService;
        private readonly IProjectTaskService _taskService;
        private readonly IProjectSwimLanesService _projectSwimLanesService;
        private readonly IProjectSwimLanesListServices _projectSwimLanesListService;
        private readonly IProjectSwimLanesListsTasksServices _projectSwimLanesListsTasksService;
        private readonly IMasterProjectSwimlaneListsServices _masterProjectSwimlaneListsServices;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        #endregion

        #region Services Initializations
        public ProjectSwimlaneController(
            GlobalVariable globalVariable,
            ISiteService siteService,
            IDropDownService dropDownService,
            ICommonService commonService,
            IProjectService projectService,
            IProjectModuleService projectModuleService,
            IProjectTaskService projectTaskService,
            IProjectSwimLanesService projectSwimLanesService,
            IProjectSwimLanesListServices projectSwimLanesListService,
            IProjectSwimLanesListsTasksServices projectSwimLanesListsTasksService,
            IMasterProjectSwimlaneListsServices masterProjectSwimlaneListsServices,
            ISitesModifiedLogsService sitesModifiedLogsService
            )
        {
            _globalVariable = globalVariable;
            _siteService = siteService;
            _dropDownService = dropDownService;
            _commonService = commonService;
            _projectService = projectService;
            _projectModuleService = projectModuleService;
            _taskService = projectTaskService;
            _projectSwimLanesService = projectSwimLanesService;
            _projectSwimLanesListService = projectSwimLanesListService;
            _projectSwimLanesListsTasksService = projectSwimLanesListsTasksService;
            _masterProjectSwimlaneListsServices = masterProjectSwimlaneListsServices;
            _sitesModifiedLogsService = sitesModifiedLogsService;
        }
        #endregion

        #region Project Work Board Ver 2 (NEW)
        [HttpGet("get-workboard")]
        public async Task<IActionResult> GetWorkBoardByProjectId(string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
                return BadRequest(new BadRequestError("Project are missing."));

            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            try
            {
                var entity = await _projectService.GettWorkBoardByProjecId(SiteId, projectId, GetDateTime);
                if (entity == null)
                    return BadRequest(new BadRequestError("No project found with the specified ids."));
                
                ObjectCleaner.RemoveEmptyOrNullProperties(entity);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return Ok($"{ex.Message} :- {ex.InnerException}");
            }
        }

        [HttpPost("add-project-swimlane")]
        public async Task<IActionResult> AddProjectSwimlane(ProjectSwimLanes model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var Swimlane = new ProjectSwimLanes();
                var MastListData = await _masterProjectSwimlaneListsServices.GetMasterProjectSwimlaneBySwimlaneTypeId(model.SwimlaneTypeId);

                if (model != null)
                {
                    Swimlane.ProjectId = model.ProjectId;
                    Swimlane.SwimlaneTypeId = model.SwimlaneTypeId;
                    Swimlane.Name = model.Name;
                    Swimlane.SortOrder = model.SortOrder;
                    Swimlane.CreatedById = LoggedUserId;
                    Swimlane.CreatedOnUtc = GetDateTime;
                    Swimlane.UpdatedById = LoggedUserId;
                    Swimlane.UpdatedOnUtc = GetDateTime;
                    _projectSwimLanesService.InsertProjectSwimLane(Swimlane);

                    if (MastListData.Any())
                    {
                        foreach (var item in MastListData)
                        {
                            var List = new ProjectSwimLanesList();
                            List.ProjectSwimlaneId = Swimlane.Id;
                            List.Name = item.Name;
                            List.SortOrder = item.SortOrder;
                            List.CreatedById = LoggedUserId;
                            List.CreatedOnUtc = GetDateTime;
                            List.UpdatedById = LoggedUserId;
                            List.UpdatedOnUtc = GetDateTime;
                            _projectSwimLanesListService.InsertProjectSwimLaneList(List);
                        }
                    }
                }

                var NewData = await _projectSwimLanesService.GetSwimlaneWithListsById(Swimlane.Id);
                return Ok(NewData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        #region Duplicate Action Methods
        [HttpPost("duplicate-swimlane")]
        public async Task<IActionResult> DuplicateProjectSwimalne(string projectId, string projectSwimLaneId)
        {
            if (string.IsNullOrEmpty(projectId))
                return BadRequest(new BadRequestError("Project are missing."));

            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            var entity = await _projectService.GettWorkBoardByProjecId(SiteId, projectId, GetDateTime);
            if (entity == null)
                return BadRequest(new BadRequestError("No project found with the specified ids."));

            var sourceSwimlane = entity.ProjectSwimLanes.FirstOrDefault(m => m.Id == projectSwimLaneId);
            if (sourceSwimlane == null) return Ok();

            var newSwimlane = CreateDuplicateSwimlane(sourceSwimlane, projectId, LoggedUserId, GetDateTime);
            _projectSwimLanesService.InsertProjectSwimLane(newSwimlane);

            foreach (var list in sourceSwimlane.ProjectSwimLanesList.Where(l => !l.Deleted))
            {
                var newList = CreateDuplicateList(list, newSwimlane.Id, LoggedUserId, GetDateTime);
                _projectSwimLanesListService.InsertProjectSwimLaneList(newList);

                foreach (var listTask in list.ProjectSwimLanesListsTasks)
                {
                    var newTask = await CreateDuplicateTask(listTask.ProjectTask, SiteId, LoggedUserId, GetDateTime);
                    _taskService.InsertProjectTask(newTask);

                    var newListTask = CreateDuplicateListTask(listTask, newList.Id, newTask.Id, LoggedUserId, GetDateTime);
                    _projectSwimLanesListsTasksService.InsertProjectSwimLaneListsTasks(newListTask);
                }
            }

            return Ok();
        }

        [HttpPost("duplicate-list")]
        public async Task<IActionResult> DuplicateProjectList(string projectId, string projectSwimLaneId, string listId)
        {
            if (string.IsNullOrEmpty(projectId))
                return BadRequest(new BadRequestError("Project are missing."));

            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            var sourceList = await _projectSwimLanesListService.GetById(listId);
            if (sourceList == null)
                return BadRequest(new BadRequestError("No list found with the specified ids."));

            var newList = CreateDuplicateList(sourceList, projectSwimLaneId, LoggedUserId, GetDateTime);
            _projectSwimLanesListService.InsertProjectSwimLaneList(newList);

            foreach (var listTask in sourceList.ProjectSwimLanesListsTasks)
            {
                var newTask = await CreateDuplicateTask(listTask.ProjectTask, SiteId, LoggedUserId, GetDateTime);
                _taskService.InsertProjectTask(newTask);

                var newListTask = CreateDuplicateListTask(listTask, newList.Id, newTask.Id, LoggedUserId, GetDateTime);
                _projectSwimLanesListsTasksService.InsertProjectSwimLaneListsTasks(newListTask);
            }

            return Ok();
        }

        [HttpPost("duplicate-task")]
        public async Task<IActionResult> DuplicateProjectTask(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
                return BadRequest(new BadRequestError("TaskId are missing."));

            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            var sourceTask = await _taskService.GetById(taskId);
            if (sourceTask == null)
                return BadRequest(new BadRequestError("No task found with the specified ids."));

            var newTask = await CreateDuplicateTask(sourceTask, SiteId, LoggedUserId, GetDateTime);
            newTask.Id = Guid.NewGuid().ToString();
            _taskService.InsertProjectTask(newTask);

            return Ok();
        }
        #endregion

        #region Private Function For Duplicate
        private ProjectSwimLanes CreateDuplicateSwimlane(ProjectSwimLanes source, string projectId, string LoggedUserId, DateTime GetDateTime)
        {
            return new ProjectSwimLanes
            {
                ProjectId = projectId,
                Name = GetNextCopyName(source.Name),
                IsDuplicate = true,
                IsDuplicateFromId = source.Id,
                SortOrder = source.SortOrder + 1,
                CreatedById = LoggedUserId,
                CreatedOnUtc = GetDateTime,
                UpdatedById = LoggedUserId,
                UpdatedOnUtc = GetDateTime
            };
        }
        private ProjectSwimLanesList CreateDuplicateList(ProjectSwimLanesList source, string swimlaneId, string LoggedUserId, DateTime GetDateTime)
        {
            return new ProjectSwimLanesList
            {
                ProjectSwimlaneId = swimlaneId,
                Name = GetNextCopyName(source.Name),
                Color = source.Color,
                SortOrder = source.SortOrder,
                CreatedById = LoggedUserId,
                CreatedOnUtc = GetDateTime,
                UpdatedById = LoggedUserId,
                UpdatedOnUtc = GetDateTime
            };
        }
        private async Task<ProjectTask> CreateDuplicateTask(ProjectTask source, string siteId, string LoggedUserId, DateTime GetDateTime)
        {
            // Get module sort order
            var module = await _projectModuleService.GetProjectModuleDetailsById(source.ProjectModuleId);

            if (module == null)
                throw new Exception("Module not found");

            int moduleSortOrder = module.SortOrder;
            decimal NextSortOrderOfProjectTask = 0;
            // Get last task sort order under this module
            var CurrentModuleSortOrder = await _taskService.GetLastSortOrderOfProjectTasks(source.ProjectModuleId);

            // If no tasks found, lastTaskSortOrder will be 1.1m
            if (CurrentModuleSortOrder == 1.1m)
            {
                NextSortOrderOfProjectTask = moduleSortOrder + 0.1m;
            }
            else
            {
                NextSortOrderOfProjectTask = Math.Round(CurrentModuleSortOrder + 0.1m, 1); // increment
            }

            return new ProjectTask
            {
                SiteId = siteId,
                ProjectId = source.ProjectId,
                SortOrder = NextSortOrderOfProjectTask,
                ProjectModuleId = source.ProjectModuleId,
                AssignedToId = source.AssignedToId,
                StatusId = source.StatusId,
                PriorityId = source.PriorityId,
                ProjectTaskNumber = await _taskService.GetLastProjectTaskNumber() + 1,
                Name = source.Name + " (Duplicated)",
                EstimateTime = source.EstimateTime,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                IsDuplicate = true,
                IsDuplicateFromId = source.Id,
                Color = source.Color,
                CreatedById = LoggedUserId,
                CreatedOnUtc = GetDateTime,
                UpdatedById = LoggedUserId,
                UpdatedOnUtc = GetDateTime
            };
        }
        private ProjectSwimLanesListsTasks CreateDuplicateListTask(ProjectSwimLanesListsTasks source, string listId, string taskId, string LoggedUserId, DateTime GetDateTime)
        {
            return new ProjectSwimLanesListsTasks
            {
                ProjectSwimlaneListId = listId,
                ProjectTaskId = taskId,
                Color = source.Color,
                SortOrder = source.SortOrder,
                CreatedById = LoggedUserId,
                CreatedOnUtc = GetDateTime,
                UpdatedById = LoggedUserId,
                UpdatedOnUtc = GetDateTime
            };
        }
        #endregion

        #region Save WorkBoard
        [HttpPost("save-project-workboard")]
        public async Task<IActionResult> SaveProjectWorkboard(Project model)
        {
            if (!ModelState.IsValid)
                return ModelStateError(ModelState);

            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var projectId = model.Id;
                if (model.ProjectSwimLanes?.Any() == true)
                {
                    foreach (var swimlane in model.ProjectSwimLanes.Where(s => s != null))
                    {
                        var swimlaneId = await AddUpdateProjectSwimLane(SiteId, projectId, swimlane, LoggedUserId, GetDateTime);

                        foreach (var list in swimlane.ProjectSwimLanesList?.Where(m => m != null) ?? Enumerable.Empty<ProjectSwimLanesList>())
                        {
                            var listId = await AddUpdateProjectSwimlaneList(swimlaneId, list, LoggedUserId, GetDateTime);

                            foreach (var listTask in list.ProjectSwimLanesListsTasks?.Where(t => t != null) ?? Enumerable.Empty<ProjectSwimLanesListsTasks>())
                            {
                                await AddUpdateProjectSwimlaneListTask(model.Name, swimlane.SwimlaneType.DropDownValue, list, listTask, SiteId, LoggedUserId, GetDateTime);
                            }
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message} :- {ex.InnerException?.Message}");
            }
        }
        #endregion

        #region Private Function For Save
        private static string GetNextCopyName(string baseName)
        {
            var regex = new Regex(@"\s-\sCopy\s\((\d+)\)$");
            var match = regex.Match(baseName);

            return match.Success
                ? regex.Replace(baseName, $" - Copy ({int.Parse(match.Groups[1].Value) + 1}")
                : $"{baseName} - Copy (1)";
        }
        private async Task<string> AddUpdateProjectSwimLane(string siteId, string projectId, ProjectSwimLanes model, string LoggedUserId, DateTime GetDateTime)
        {
            if (model.Name != "Untitled")
            {
                var exist = await _projectSwimLanesService.GetProjectSwimLaneByName(siteId, model.Name, projectId);
                if (exist != null && exist.Id != model.Id)
                    throw new Exception("A swimlane with the same name already exists.");
            }

            var swimlane = await _projectSwimLanesService.GetById(model.Id);

            if (swimlane != null)
            {
                swimlane.Name = model.Name;
                swimlane.SortOrder = model.SortOrder;
                swimlane.Color = model.Color;
                swimlane.UpdatedById = LoggedUserId;
                swimlane.UpdatedOnUtc = GetDateTime;
                swimlane.Deleted = model.Deleted;
                _projectSwimLanesService.UpdateProjectSwimLane(swimlane);
                return swimlane.Id;
            }

            var newSwimlane = new ProjectSwimLanes
            {
                ProjectId = projectId,
                SwimlaneTypeId = model.SwimlaneTypeId,
                Name = model.Name,
                SortOrder = model.SortOrder,
                Color = model.Color,
                CreatedById = LoggedUserId,
                CreatedOnUtc = GetDateTime,
                UpdatedById = LoggedUserId,
                UpdatedOnUtc = GetDateTime,
                Deleted = model.Deleted
            };

            _projectSwimLanesService.InsertProjectSwimLane(newSwimlane);
            return newSwimlane.Id;
        }
        private async Task<string> AddUpdateProjectSwimlaneList(string swimlaneId, ProjectSwimLanesList list, string LoggedUserId, DateTime GetDateTime)
        {
            var listData = await _projectSwimLanesListService.GetById(list.Id);

            if (listData != null)
            {
                listData.ProjectSwimlaneId = swimlaneId;
                listData.Name = list.Name;
                listData.Color = list.Color;
                listData.SortOrder = list.SortOrder;
                listData.UpdatedById = LoggedUserId;
                listData.UpdatedOnUtc = GetDateTime;
                listData.Deleted = list.Deleted;
                _projectSwimLanesListService.UpdateProjectSwimLaneList(listData);
                return listData.Id;
            }

            var newList = new ProjectSwimLanesList
            {
                ProjectSwimlaneId = swimlaneId,
                Name = list.Name,
                Color = list.Color,
                SortOrder = list.SortOrder,
                CreatedById = LoggedUserId,
                CreatedOnUtc = GetDateTime,
                UpdatedById = LoggedUserId,
                UpdatedOnUtc = GetDateTime,
                Deleted = list.Deleted
            };

            _projectSwimLanesListService.InsertProjectSwimLaneList(newList);
            return newList.Id;
        }
        private async Task<string> AddUpdateProjectSwimlaneListTask(string projectName, string swimlaneTypeName, ProjectSwimLanesList list, ProjectSwimLanesListsTasks taskModel, string siteId, string LoggedUserId, DateTime GetDateTime)
        {
            var taskData = await _projectSwimLanesListsTasksService.GetById(taskModel.Id);
            if (taskData != null)
            {
                var existingTask = await _taskService.GetById(taskModel.ProjectTaskId);
                if (existingTask != null)
                {
                    string NewStatusId = swimlaneTypeName == "Task Status As Lists" ? await _dropDownService.GetDropDownByTypeNameAndName(siteId, "Task Status", list.Name) : taskModel.ProjectTask.StatusId;
                    bool IsStatusChanged = existingTask.StatusId != NewStatusId;

                    existingTask.ProjectModuleId = taskModel.ProjectTask.ProjectModuleId;
                    existingTask.AssignedToId = taskModel.ProjectTask.AssignedToId;
                    existingTask.StatusId = NewStatusId;
                    existingTask.PriorityId = taskModel.ProjectTask.PriorityId;
                    existingTask.Name = taskModel.ProjectTask.Name;
                    existingTask.EstimateTime = taskModel.ProjectTask.EstimateTime;
                    existingTask.StartDate = taskModel.ProjectTask.StartDate;
                    existingTask.EndDate = taskModel.ProjectTask.EndDate;
                    existingTask.Color = taskModel.ProjectTask.Color;
                    existingTask.SortOrder = taskModel.ProjectTask.SortOrder;
                    existingTask.UpdatedById = LoggedUserId;
                    existingTask.UpdatedOnUtc = GetDateTime;
                    _taskService.UpdateProjectTask(existingTask);

                    taskData.ProjectSwimlaneListId = list.Id;
                    taskData.SortOrder = taskModel.SortOrder;
                    taskData.Color = taskModel.Color;
                    taskData.UpdatedById = LoggedUserId;
                    taskData.UpdatedOnUtc = GetDateTime;
                    taskData.Deleted = taskModel.Deleted;
                    _projectSwimLanesListsTasksService.UpdateProjectSwimLaneListsTasks(taskData);

                    if (IsStatusChanged)
                    {
                        var Status = await _dropDownService.GetDropDownById(NewStatusId);
                        _sitesModifiedLogsService.AddSiteModifiedLogs(siteId, "ProjectTasks", taskModel.ProjectTask.ProjectId, projectName, existingTask.Id, existingTask.Name, "Task Status", Status.DropDownValue, LoggedUserId, GetDateTime);
                    }
                    return taskData.Id;
                }
            }

            var ProjectModule = await _projectModuleService.GetProjectModuleByName("Default", taskModel.ProjectTask.ProjectId);
            string ProjectModuleId = "";
            if (ProjectModule == null)
            {
                int NextSortOrderOfProjectModule = 0;
                if (!string.IsNullOrWhiteSpace(taskModel.ProjectTask.ProjectId) && taskModel.ProjectTask.ProjectId != "undefined")
                {
                    NextSortOrderOfProjectModule = await _projectModuleService.GetLastSortOrderOfProjectModules(taskModel.ProjectTask.ProjectId) + 1;
                }
                var NewProjectModule = new ProjectModule
                {
                    SiteId = siteId,
                    SortOrder = NextSortOrderOfProjectModule,
                    ProjectId = taskModel.ProjectTask.ProjectId,
                    ProjectModuleStatusId = await _dropDownService.GetDropDownByTypeNameAndName(siteId, "WO Status", "New"),
                    ProjectModuleNumber = await _projectModuleService.GetLastProjectModuleNumber() + 1,
                    Name = "Default",
                    StartDate = GetDateTime,
                    CreatedById = LoggedUserId,
                    CreatedOnUtc = GetDateTime,
                    UpdatedById = LoggedUserId,
                    UpdatedOnUtc = GetDateTime
                };
                _projectModuleService.InsertProjectModule(NewProjectModule);
                ProjectModuleId = NewProjectModule.Id;
            }
            else
                ProjectModuleId = ProjectModule.Id;

            // Get module sort order
            var module = await _projectModuleService.GetProjectModuleDetailsById(ProjectModuleId);

            if (module == null)
                throw new Exception("Module not found");

            int moduleSortOrder = module.SortOrder;
            decimal NextSortOrderOfProjectTask = 0;
            // Get last task sort order under this module
            var CurrentModuleSortOrder = await _taskService.GetLastSortOrderOfProjectTasks(ProjectModuleId);

            // If no tasks found, lastTaskSortOrder will be 1.1m
            if (CurrentModuleSortOrder == 1.1m)
            {
                NextSortOrderOfProjectTask = moduleSortOrder + 0.1m;
            }
            else
            {
                NextSortOrderOfProjectTask = Math.Round(CurrentModuleSortOrder + 0.1m, 1); // increment
            }

            var newTask = new ProjectTask
            {
                SiteId = siteId,
                ProjectId = taskModel.ProjectTask.ProjectId,
                ProjectModuleId = ProjectModuleId,
                AssignedToId = taskModel.ProjectTask.AssignedToId,
                StatusId = swimlaneTypeName == "Task Status As Lists" ? await _dropDownService.GetDropDownByTypeNameAndName(siteId, "Task Status", list.Name) : taskModel.ProjectTask.StatusId,
                PriorityId = taskModel.ProjectTask.PriorityId,
                ProjectTaskNumber = await _taskService.GetLastProjectTaskNumber() + 1,
                Name = taskModel.ProjectTask.Name,
                EstimateTime = taskModel.ProjectTask.EstimateTime,
                StartDate = taskModel.ProjectTask.StartDate,
                EndDate = taskModel.ProjectTask.EndDate,
                Color = taskModel.ProjectTask.Color,
                //SortOrder = taskModel.ProjectTask.SortOrder,
                SortOrder = NextSortOrderOfProjectTask,
                Active = true,
                CreatedById = LoggedUserId,
                CreatedOnUtc = GetDateTime,
                UpdatedById = LoggedUserId,
                UpdatedOnUtc = GetDateTime
            };

            _taskService.InsertProjectTask(newTask);

            var newListTask = new ProjectSwimLanesListsTasks
            {
                ProjectSwimlaneListId = list.Id,
                ProjectTaskId = newTask.Id,
                Color = taskModel.Color,
                SortOrder = taskModel.SortOrder,
                CreatedById = LoggedUserId,
                CreatedOnUtc = GetDateTime,
                UpdatedById = LoggedUserId,
                UpdatedOnUtc = GetDateTime
            };

            _projectSwimLanesListsTasksService.InsertProjectSwimLaneListsTasks(newListTask);
            return taskModel.Id;
        }
        #endregion

        #endregion
    }
}
