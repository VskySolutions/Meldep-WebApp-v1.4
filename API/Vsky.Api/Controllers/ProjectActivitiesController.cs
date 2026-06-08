using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
using Vsky.Services.Notifications;
using Vsky.Services.ProjectActivities;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("project-activities")]
    public class ProjectActivitiesController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IProjectActivityService _activityService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly INotificationService _notificationService;
        private readonly IEmployeeService _employeeService;
        private readonly IProjectActivityFilesService _projectActivityFilesService;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly IMasterNotificationService _masterNotificationService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations
        public ProjectActivitiesController(
            GlobalVariable globalVariable,
            IMapper mapper,
            IProjectActivityService activityService,
            ICommonService commonService,
            ISiteService siteService,
            INotificationService notificationService,
            IEmployeeService employeeService,
            IProjectActivityFilesService projectActivityFilesService,
            IDropDownService dropDownService,
            IDropDownTypeService dropDownTypeService,
            IMasterNotificationService masterNotificationService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _activityService = activityService;
            _commonService = commonService;
            _siteService = siteService;
            _notificationService = notificationService;
            _employeeService = employeeService;
            _projectActivityFilesService = projectActivityFilesService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _masterNotificationService = masterNotificationService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllProjectActivities
        // Title: Get All Activities
        // Description: This endpoint fetches a list of Activities based on the provided search criteria such as name, sorting, and pagination.

        [HttpPost("list")]
        public async Task<IActionResult> GetAllProjectActivities(ProjectActivitySearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                //var createdBy = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var createdBy = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                if (searchModel.Flag == "DS")
                {
                    searchModel.AssignedToIds = new List<string> { createdBy };
                    searchModel.ActiveStatus = "Active";
                }
                
                // Fetch a list of project activities  based on search criteria
                var list = await _activityService.GetAllProjectActivities(SiteId,
                    LoggedUserId,
                    createdBy,
                    searchModel.SearchText,
                    searchModel.ActiveStatus,
                    searchModel.ProjectIds,
                    searchModel.ProjectModuleIds,
                    searchModel.AssignedToIds,
                    searchModel.ActivityNameIds,
                    searchModel.ActivityStatusIds,
                    searchModel.StatusIds,
                    searchModel.CustomerIds,
                    searchModel.CompanyContactIds,
                    searchModel.SprintWeekEndDate,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var activityDropDownType = await _dropDownTypeService.GetDropDownType(SiteId, "Project Activities");
                var activityDropdowns = await _dropDownService.GetDropDowns(activityDropDownType.Id);
                var projectAcivityList = _mapper.Map<IList<ProjectActivityModel>>(list);

                foreach (var item in projectAcivityList)
                {
                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        var activityDropdownItem = activityDropdowns.FirstOrDefault(d => d.DropDownValue == item.Name);
                        if (activityDropdownItem != null)
                        {
                            item.ActivityNameDescription = activityDropdownItem.Description;
                        }
                    }
                }

                var model = new ProjectActivityListModel
                {
                    Data = projectAcivityList,
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

        #region GetAllProjectActivitiesForExpandCollapse
        [HttpPost("list-expand-collapse")]
        public async Task<IActionResult> GetAllProjectActivitiesForExpandCollapse(ProjectActivitySearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                //var createdBy = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var createdBy = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                if (searchModel.Flag == "DS")
                {
                    searchModel.AssignedToIds = new List<string> { createdBy };
                    searchModel.ActiveStatus = "Active";
                }
                //if (searchModel.IsSprintWeek)
                //{
                //    var planType = await _dropDownTypeService.GetDropDownType(SiteId, "Project Activities");
                //}
                // Fetch a list of project activities  based on search criteria
                var list = await _activityService.GetAllProjectActivitiesForExpandCollapse(SiteId,
                    LoggedUserId,
                    createdBy,
                    searchModel.SearchText,
                    searchModel.ActiveStatus,
                    searchModel.ProjectIds,
                    searchModel.ProjectModuleIds,
                    searchModel.AssignedToIds,
                    searchModel.ActivityNameIds,
                    searchModel.ActivityStatusIds,
                    searchModel.StatusIds,
                    searchModel.CustomerIds,
                    searchModel.CompanyContactIds,
                    searchModel.SprintWeekEndDate,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                //var activityDropDownType = await _dropDownTypeService.GetDropDownType(SiteId, "Project Activities");
                //var activityDropdowns = await _dropDownService.GetDropDowns(activityDropDownType.Id);
                //var projectAcivityList = _mapper.Map<IList<ProjectActivityModel>>(list);

                //foreach (var item in projectAcivityList)
                //{
                //    if (!string.IsNullOrEmpty(item.Name))
                //    {
                //        var activityDropdownItem = activityDropdowns.FirstOrDefault(d => d.DropDownValue == item.Name);
                //        if (activityDropdownItem != null)
                //        {
                //            item.ActivityNameDescription = activityDropdownItem.Description;
                //        }
                //    }
                //}

                return Ok(new
                {
                    Data = list,
                    Total = list.TotalCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllProjectActivityListForDropdown
        // Title: GetAllProjectActivitiesListForDropdown
        // Description: This endpoint retrieves the details of a specific project activity based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllProjectActivityListForDropdown()
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _activityService.GetAllProjectActivityListForDropdown(SiteId);
            var model = _mapper.Map<List<LeadActivitiesModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetProjectTaskActivityListForDropdown        
        [HttpGet("dailytimesheetdropdown/list/{projectId}/{projectModuleId}/{taskId}")]
        public async Task<IActionResult> GetProjectTaskActivityListForDropdown(string projectId = null, string projectModuleId = null, string taskId = null /*string flag = null,*/ /*string date = null*/ )
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                //var employeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                var list = await _activityService.GetProjectTaskActivityListForDropdown(SiteId,
                    projectId,
                    projectModuleId,
                    taskId,
                    employeeId
                );
                var activityDropDownType = await _dropDownTypeService.GetDropDownType(SiteId, "Project Activities");
                var activityDropdowns = await _dropDownService.GetDropDowns(activityDropDownType.Id);
                var model = _mapper.Map<IList<ProjectActivityModel>>(list);

                foreach (var item in model)
                {
                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        var activityDropdownItem = activityDropdowns.FirstOrDefault(d => d.DropDownValue == item.Name);
                        if (activityDropdownItem != null)
                        {
                            item.ActivityNameDescription = activityDropdownItem.Description;
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

        #region GetProjectActivitiesByTaskId
        // Title: GetProjectActivitiesByTaskId
        // Description:The GetProjectActivitiesByTaskId method is an asynchronous HTTP GET endpoint that retrieves project activities based on a given task ID. It accepts a taskId parameter, which is used to fetch the related activities from the _activityService.
        [HttpGet("task")]
        public async Task<IActionResult> GetProjectActivitiesByTaskId(string taskId, string pageName = "", bool isShowCloseStatus = false)
        {
            var list = await _activityService.GetProjectActivitiesByTaskId(taskId, pageName, isShowCloseStatus);
            var model = _mapper.Map<IList<ProjectActivityModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetProjectActivityById
        // Title: GetProjectActivityById
        // Description: This endpoint retrieves the details of a specific project activity based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectActivityById(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch the project activity entity by its ID from the service
                var entity = await _activityService.GetProjectActivityDetailsById(id);
                // If the project activity entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No project activity found with the specified id."));

                var activityDropDownType = await _dropDownTypeService.GetDropDownType(SiteId, "Project Activities");
                var activityDropDown = await _dropDownService.GetDropDownByTypeAndValue(SiteId, activityDropDownType.Id, entity.Name);

                // Map the project activity entity to a ProjectActivityModel object
                var model = _mapper.Map<ProjectActivityModel>(entity);
                model.ActivityNameDescription = activityDropDown.Description;
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetProjectActivityByTaskId
        // Title: GetProjectActivityByTaskId
        // Description: This endpoint retrieves the details of a specific project activity based on its unique identifier (ID). 
        [HttpGet("task/{id}")]
        public async Task<IActionResult> GetProjectActivityByTaskId(string id, string TargetMonthStr = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                //Set target month
                //DateTime? TargetMonth = null;
                //if (!string.IsNullOrWhiteSpace(TargetMonthStr) && TargetMonthStr != "undefined")
                //{
                //    var targetMonth = TargetMonthStr;
                //    if (targetMonth != null)
                //    {
                //        var targetMonthArr = targetMonth.Split('-');
                //        int month = DateTimeFormatInfo.CurrentInfo.MonthNames.ToList().IndexOf(targetMonthArr[0]) + 1;
                //        if (month > 0)
                //        {
                //            string monthStr = month >= 10 ? month.ToString() : "0" + month.ToString();
                //            string targetMonthDate = "01/" + monthStr + "/" + targetMonthArr[1];
                //            TargetMonth = DateTime.ParseExact(targetMonthDate, "dd/MM/yyyy", null);
                //        }
                //    }
                //}

                //var list = await _activityService.GetProjectActivityByTaskId(SiteId, id, TargetMonth);
                var list = await _activityService.GetProjectActivityByTaskId(SiteId, id, null);
                var model = _mapper.Map<List<ProjectActivityModel>>(list);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateProjectActivity
        // Title: CreateProjectActivity
        // Description: This endpoint handles the creation of a new project activity. It first checks if a project activity with the same name already exists.If not, it maps the project activity model to the project activity entity, sets the creation details, and inserts the project activity into the database.
        [HttpPost]
        public async Task<IActionResult> CreateProjectActivity([FromForm] ProjectActivityModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the project activity already exists
                    var exists = await _activityService.GetProjectActivityByDetails(model.Name, model.TaskId, model.AssignedToId, null);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Activity name already exists, try with another."));

                    // Map the project activity model to the project activity entity
                    var entity = _mapper.Map<ProjectActivity>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    entity.SiteId = SiteId;
                    entity.ActivityStatusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Activity Status", "New");
                    // Set custom properties
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
                                "project-tasks-activities",
                                entity.Id
                            );
                    }

                    // Set the created by and created on properties
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _activityService.InsertProjectActivity(entity);

                    var taskActivityData = await _activityService.GetProjectActivityDetailsById(entity.Id);

                    string ProjectTaskActivityId = entity.Id;
                    if (model.ProjectTaskActivityFiles != null && model.ProjectTaskActivityFiles.Any())
                    {
                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "project-tasks-activities", model.ProjectTaskActivityFiles, entity.Id);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ProjectTaskActivityFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = model.ProjectId,
                                Module = taskActivityData.Project.Name,
                                SubModuleId = ProjectTaskActivityId,
                                Sub_Module = entity.Name,
                                Type = "Project Activity",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var ProjectActivityFiles = new ProjectActivityFiles
                            {
                                FileId = picture.Id,
                                ProjectActivityId = ProjectTaskActivityId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _projectActivityFilesService.InsertProjectActivityFile(ProjectActivityFiles);

                            index++;
                        }
                    }

                    var AssignedTo = _commonService.GetLoggeduserIdByEmployeeId(SiteId, entity.AssignedToId);
                    var MasterNotificationData = await _masterNotificationService.GetMasterNotificationByNumber(SiteId, "Task1", AssignedTo);
                    if (MasterNotificationData != null)
                    {
                        string Message = MasterNotificationData.Message.Replace("[Activity Name]", entity.Name);
                        var Notification = _notificationService.AddNotification(SiteId, MasterNotificationData.Title, Message, MasterNotificationData.Type, entity.CreatedById, entity.Id, "/my-task-and-activities", AssignedTo, entity.CreatedById, GetDateTime);
                    }
                    return NoContent();
                }
                // Return model state errors if the model state is not valid
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Title: AddFiles
        [HttpPost("add-activity-files")]
        public async Task<IActionResult> AddFiles([FromForm] ProjectActivityModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _activityService.GetProjectActivityDetailsById(model.Id);

                    // If no task is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project activity found with the specified id."));

                    var moduleData = await _activityService.GetProjectActivityDetailsById(entity.Id);

                    //Add
                    string ProjectActivityId = entity.Id;
                    if (model.ProjectTaskActivityFiles != null && model.ProjectTaskActivityFiles.Any())
                    {
                        int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(ProjectActivityId, "Project Activity");

                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "project-tasks-activities", model.ProjectTaskActivityFiles, entity.Id, existingImagesCount);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ProjectTaskActivityFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = model.ProjectId,
                                Module = moduleData.Project.Name,
                                SubModuleId = ProjectActivityId,
                                Sub_Module = entity.Name,
                                Type = "Project Activity",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var ProjectActivityFiles = new ProjectActivityFiles
                            {
                                FileId = picture.Id,
                                ProjectActivityId = ProjectActivityId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _projectActivityFilesService.InsertProjectActivityFile(ProjectActivityFiles);

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

        #region CloneProjectTaskActivity
        // Title: CloneProjectTaskActivity
        [HttpPost("clone-task-activities")]
        public async Task<IActionResult> CloneProjectTaskActivity(ProjectActivityModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (model.ProjectActivityLines != null && model.ProjectActivityLines.Count() > 0)
                {
                    // Loop through each plan
                    foreach (var plan in model.ProjectActivityLines)
                    {
                        if (!string.IsNullOrWhiteSpace(plan.Id))
                        {
                            // Fetch the project activity entity by its ID from the service
                            var activityDetails = await _activityService.GetProjectActivityDetailsById(plan.Id);
                            // If no project activity is found with the given ID, return a bad request with an error message
                            if (activityDetails == null)
                                continue;

                            // Map the project activity model to the project activity entity
                            var entity = _mapper.Map<ProjectActivity>(model);
                            entity.Id = Guid.NewGuid().ToString();
                            entity.SiteId = SiteId;

                            //Check if the project activity already exists
                            var exists = await _activityService.GetProjectActivityByDetails(activityDetails.Name, activityDetails.TaskId, activityDetails.AssignedToId, null);
                            if (exists != null)
                                continue;

                            var exEmployeeStatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Employee Status", "Ex-Employee");
                            var employee = await _employeeService.GetActiveEmployeeByStatusId(SiteId, exEmployeeStatus, activityDetails.AssignedToId);
                            if (employee != null)
                                continue;

                            // Set the user who updated the project activity and the current UTC time for tracking purposes
                            entity.Name = activityDetails.Name;
                            entity.TaskId = activityDetails.TaskId;
                            entity.ProjectId = activityDetails.ProjectId;
                            entity.ProjectModuleId = activityDetails.ProjectModuleId;
                            entity.ActivityStatusId = activityDetails.ActivityStatusId;
                            entity.AssignedToId = activityDetails.AssignedToId;
                            entity.Description = activityDetails.Description;
                            entity.EstimateHours = plan.EstimateHours;
                            entity.CreatedById = LoggedUserId;
                            entity.CreatedOnUtc = GetDateTime;
                            _activityService.InsertProjectActivity(entity);
                        }
                    }
                }
                return NoContent();
            }
            // Return model state errors if the model state is not valid
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateProjectActivity
        // Title: UpdateProjectActivity
        // Description: This endpoint updates an existing project activity by its ID. It validates the project activity model, checks for duplicate project activity names, updates the  project activity's details.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProjectActivity(string id, [FromForm] ProjectActivityModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the project activity already exists
                    var exists = await _activityService.GetProjectActivityByDetails(model.Name, model.TaskId, model.AssignedToId, null, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Activity name already exists, try with another."));

                    // Fetch the project activity entity by its ID
                    var entity = await _activityService.GetById(id);
                    // If no project activity is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project activity found with the specified id."));

                    // Retrieve all file IDs from the project task files
                    var allProjectTaskActivityFileIds = (await _projectActivityFilesService.GetAllProjectActivityFilesByProjectActivityId(id))
                                             .Select(file => file.Id)  // Extract Id from ProjectFiles
                                             .ToList();

                    var missingFileIds = allProjectTaskActivityFileIds.ToList();
                    if (model.ExistingFiles != null)
                    {
                        var existingFileIds = model.ExistingFiles.Select(fileJson =>
                        {
                            var file = JsonConvert.DeserializeObject<Picture>(fileJson);
                            return file.Id.Trim().ToLower();
                        })
                        .ToList();

                        // Compare and find missing file IDs
                        missingFileIds = allProjectTaskActivityFileIds.Except(existingFileIds).ToList();
                    }

                    if (missingFileIds.Any())
                    {
                        foreach (var projectTaskActivityFilesId in missingFileIds)
                        {
                            var projectTaskActivityFileDate = await _projectActivityFilesService.GetProjectActivityFileById(projectTaskActivityFilesId);
                            if (projectTaskActivityFileDate != null)
                                _projectActivityFilesService.DeleteProjectActivityFile(projectTaskActivityFileDate);
                        }
                    }
                    // Set custom properties
                    if (model.StartDateStr != "" && model.StartDateStr != null)
                        entity.StartDate = DateTime.ParseExact(model.StartDateStr, "MM/dd/yyyy", null);
                    if (model.EndDateStr != "" && model.EndDateStr != null)
                        entity.EndDate = DateTime.ParseExact(model.EndDateStr, "MM/dd/yyyy", null);

                    // Set the user who updated the project activity and the current UTC time for tracking purposes
                    entity.Name = model.Name;
                    entity.TaskId = model.TaskId;
                    entity.ProjectId = model.ProjectId;
                    entity.ProjectModuleId = model.ProjectModuleId;
                    entity.AssignedToId = model.AssignedToId;
                    //entity.Description = model.Description;
                    entity.EstimateHours = model.EstimateHours;
                    entity.ActivityStatusId = model.ActivityStatusId;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "project-tasks-activities",
                                entity.Id,
                                entity.Description
                            );
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _activityService.UpdateProjectActivity(entity);

                    var taskActivityData = await _activityService.GetProjectActivityDetailsById(id);

                    string ProjectTaskActivityId = entity.Id;
                    if (model.ProjectTaskActivityFiles != null && model.ProjectTaskActivityFiles.Any())
                    {
                        int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(ProjectTaskActivityId, "Project Activity");

                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "project-tasks-activities", model.ProjectTaskActivityFiles, entity.Id, existingImagesCount);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ProjectTaskActivityFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = model.ProjectId,
                                Module = taskActivityData.Project.Name,
                                SubModuleId = ProjectTaskActivityId,
                                Sub_Module = entity.Name,
                                Type = "Project Activity",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var ProjectActivityFiles = new ProjectActivityFiles
                            {
                                FileId = picture.Id,
                                ProjectActivityId = ProjectTaskActivityId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _projectActivityFilesService.InsertProjectActivityFile(ProjectActivityFiles);

                            index++;
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

        //created for update task activity status from list page- by SN
        #region
        [HttpPut("updateTaskActivityStatus")]
        public async Task<IActionResult> UpdateTaskActivityStatus(ProjectActivityModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.ActivityIds != null && model.ActivityIds.Count > 0)
                    {
                        foreach (var activityId in model.ActivityIds)
                        {
                            var activityEntity = await _activityService.GetById(activityId);
                            if (activityEntity != null)
                            {
                                bool result = await UpdateActivityDetails(activityId, model.ActivityStatusId, "status");
                               
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

        #region DeleteProjectActivity
        // Title: DeleteProjectActivity
        // Description: This endpoint deletes a project activity based on the provided project ID. It first retrieves the project activity entity by ID, checks if it exists, and if so, deletes the project activity. If the project activity is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectActivity(string id)
        {
            try
            {
                // Fetch the project activity entity by its ID
                var entity = await _activityService.GetById(id);
                // If no project activity is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No project activity found with the specified id."));

                // Delete the project activity using the project activity service
                _activityService.DeleteProjectActivity(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("taskActivity-file/{id}")]
        public async Task<IActionResult> DeleteProjectTaskActivityFile(string id)
        {
            try
            {
                var entity = await _projectActivityFilesService.GetProjectActivityFileById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No file found with the specified id."));

                _projectActivityFilesService.DeleteProjectActivityFile(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region SaveBulkActivities
        // Title: SaveBulkActivities
        // Description:This method, SaveBulkActivities, handles the bulk saving of project activities. It takes a ProjectTaskModel as input and processes each activity inside the model. The method maps the model data to the corresponding ProjectActivity entity and populates details like ProjectId, ProjectModuleId, TaskId, and TargetMonth.

        [HttpPost("saveBulkActivities")]
        public async Task<IActionResult> SaveBulkActivities(ProjectTaskModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                
                var status = await _dropDownTypeService.GetDropDownTypeByType(SiteId, "Activity Status");
                var activityStatus = await _dropDownService.GetDropDownByTypeAndValue(SiteId, status.Id, "New");

                foreach (var activity in model.ProjectActivityModel)
                {
                    if (activity.Flag != "Delete")
                    {
                        var exists = await _activityService.GetProjectActivityByDetails(activity.Name, model.TaskId, activity.AssignedToId, null);
                        if (exists != null)
                            continue;

                        var activityEntity = _mapper.Map<ProjectActivity>(activity);
                        activityEntity.SiteId = SiteId;
                        activityEntity.ProjectId = model.ProjectId;
                        activityEntity.ProjectModuleId = model.ProjectModuleId;
                        activityEntity.TaskId = model.TaskId;
                        activityEntity.ActivityStatusId = activityStatus.Id;

                        // Set custom properties
                        if (activity.StartDateStr != "" && activity.StartDateStr != null)
                            activityEntity.StartDate = DateTime.ParseExact(activity.StartDateStr, "MM/dd/yyyy", null);
                        if (activity.EndDateStr != "" && activity.EndDateStr != null)
                            activityEntity.EndDate = DateTime.ParseExact(activity.EndDateStr, "MM/dd/yyyy", null);

                        // Set the created by and created on properties
                        activityEntity.CreatedById = LoggedUserId;
                        activityEntity.UpdatedById = LoggedUserId;
                        activityEntity.CreatedOnUtc = GetDateTime;
                        activityEntity.UpdatedOnUtc = GetDateTime;
                        _activityService.InsertProjectActivity(activityEntity);
                        //activityEntities.Add(activityEntity);
                    }
                }

                //if (activityEntities.Count > 0)
                //    _activityService.InsertProjectActivityList(activityEntities);

                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateBulkActivities
        // Title: UpdateBulkActivities
        // Description:The UpdateBulkActivities method is designed to handle the updating or deletion of multiple project activities within a project task. It accepts a ProjectTaskModel and checks the validity of the model. The activities within the model are processed based on their Flag values—if the Flag is set to "Edit," the corresponding activity is updated with new data such as ProjectId, TaskId, Name, AssignedToId, Description, and EstimateHours.
        [HttpPost("updateBulkActivities")]
        public async Task<IActionResult> UpdateBulkActivities(ProjectTaskModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (model.ProjectActivityModel.Count() > 0)
                {
                    var deleteList = new List<ProjectActivity>();
                    var updateList = new List<ProjectActivity>();
                    foreach (var item in model.ProjectActivityModel)
                    {
                        if (item.Flag == "Edit")
                        {
                            var activityEntity = await _activityService.GetById(item.Id);
                            if (activityEntity == null)
                                continue;

                            var exists = await _activityService.GetProjectActivityByDetails(item.Name, model.TaskId, item.AssignedToId, null, item.Id);
                            if (exists != null)
                                continue;

                            activityEntity.ProjectId = model.ProjectId;
                            activityEntity.ProjectModuleId = model.ProjectModuleId;
                            activityEntity.TaskId = model.TaskId;
                            activityEntity.Name = item.Name;
                            activityEntity.AssignedToId = item.AssignedToId;
                            //activityEntity.ActivityStatusId = item.ActivityStatusId;
                            activityEntity.EstimateHours = item.EstimateHours;

                            if (!string.IsNullOrEmpty(item.Description))
                            {
                                activityEntity.Description = await _azureBlobImageServices
                                    .ProcessHtmlAndManageImagesAsync(
                                        item.Description,
                                        SiteData.Name,
                                        "project-tasks-activities",
                                        activityEntity.Id,
                                        activityEntity.Description
                                    );
                            }

                            //// Set custom properties
                            //if (item.StartDateStr != "" && item.StartDateStr != null)
                            //    activityEntity.StartDate = DateTime.ParseExact(item.StartDateStr, "MM/dd/yyyy", null);
                            //if (item.EndDateStr != "" && item.EndDateStr != null)
                            //    activityEntity.EndDate = DateTime.ParseExact(item.EndDateStr, "MM/dd/yyyy", null);

                            activityEntity.UpdatedById = LoggedUserId;
                            activityEntity.UpdatedOnUtc = GetDateTime;
                            updateList.Add(activityEntity);
                        }
                        else if (item.Flag == "Delete")
                        {
                            var activityEntity = await _activityService.GetById(item.Id);
                            if (activityEntity == null)
                                continue;

                            deleteList.Add(activityEntity);
                        }
                    }

                    if (updateList.Count > 0)
                        _activityService.UpdateProjectActivityList(updateList);

                    if (deleteList.Count > 0)
                        _activityService.DeleteProjectActivityList(deleteList);

                }
                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        [HttpGet("project-activity-detailsbyids")]
        public async Task<IActionResult> GetProjectTasksActivitiesDetailsByIds([FromQuery] string ids)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            if (string.IsNullOrEmpty(ids))
                return BadRequest(new BadRequestError("Activities are missing."));

            var idArray = ids.Split(','); // Split the comma-separated string into an array
            var entity = await _activityService.GetProjectTasksActivitiesDetailsByIds(idArray);
            if (entity == null)
                return BadRequest(new BadRequestError("No activity found with the specified ids."));

            var model = _mapper.Map<List<ProjectActivityModel>>(entity);

            // Fetch dropdown descriptions
            var activityDropDownType = await _dropDownTypeService.GetDropDownType(SiteId, "Project Activities");
            var activityDropdowns = await _dropDownService.GetDropDowns(activityDropDownType.Id);

            // Match and assign descriptions
            foreach (var item in model)
            {
                if (!string.IsNullOrEmpty(item.Name))
                {
                    var activityDropdownItem = activityDropdowns.FirstOrDefault(d => d.DropDownValue == item.Name);
                    if (activityDropdownItem != null)
                    {
                        item.ActivityNameDescription = activityDropdownItem.Description;
                    }
                }
            }
            return Ok(model);
        }

        #region GetActivitiesDetailsByIds
        // Title: GetActivitiesDetailsByIds
        [HttpGet("activity-detailsbyids")]
        public async Task<IActionResult> GetActivitiesDetailsByIds(string ids, string targetMonthStr = null)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return BadRequest(new BadRequestError("Activities are missing."));
            }

            var idArray = ids.Split(','); // Split the comma-separated string into an array
            var entity = await _activityService.GetProjectTasksActivitiesDetailsByIds(idArray);
            var modelList = new List<ProjectActivityModel>();
            // Iterate through the entity list and update the status
            if (entity != null)
            {
                foreach (var item in entity)
                {
                    // Determine the state status based on the condition
                    string stateStatus;
                    // Check if the project activity already exists
                    var exists = await _activityService.GetProjectActivityByDetails(item.Name, item.Task.Id, item.AssignedTo.Id, null);
                    if (exists != null)
                    {
                        stateStatus = "Invalid"; // Set status as Invalid
                    }
                    else
                    {
                        stateStatus = "Valid"; // Set status as Valid
                    }

                    var activityModelList = new ProjectActivityModel
                    {
                        Id = item.Id,
                        StateStatus = stateStatus,
                        ProjectId = item.Project.Id,
                        ProjectName = item.Project.Name,
                        ProjectModuleName = item.ProjectModule.Name,
                        TaskName = item.Task.Name,
                        Name = item.Name,
                        AssignedToName = item.AssignedTo.Person.FirstName + " " + item.AssignedTo.Person.LastName,
                        EstimateHours = item.EstimateHours

                    };
                    modelList.Add(activityModelList);
                }
            }

            if (entity == null)
            {
                return BadRequest(new BadRequestError("No activity found with the specified ids."));
            }
            var model = _mapper.Map<List<ProjectActivityModel>>(modelList);
            return Ok(model);
        }
        #endregion        


        #region All Project Planner -> AddProjectActivity
        // Title: Add Project Activity
        [HttpPost("add-single-activity")]
        public async Task<IActionResult> AddProjectActivity(ProjectActivityModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the project activity already exists
                    var exists = await _activityService.GetById(model.Id);
                    if (exists != null)
                    {
                        exists.Name = model.ActivityName;
                        if (!string.IsNullOrEmpty(model.AssignedToId))
                            exists.AssignedToId = model.AssignedToId;
                        exists.EstimateHours = model.EstimateHours;
                        exists.ProjectModuleId = model.ProjectModuleId;
                        exists.UpdatedById = LoggedUserId;
                        exists.UpdatedOnUtc = GetDateTime;
                        _activityService.UpdateProjectActivity(exists);
                    }
                    else
                    {
                        //Check if the project activity already exists
                        var alreadyExist = await _activityService.GetProjectActivityByDetails(model.ActivityName, model.TaskId, model.AssignedToId, null);
                        if (alreadyExist != null)
                            return BadRequest(new BadRequestError("Activity name already exists, try with another."));

                        // Map the project activity model to the project activity entity
                        ProjectActivity projectActivity = new ProjectActivity();
                        projectActivity.SiteId = SiteId;
                        projectActivity.ProjectId = model.ProjectId;
                        projectActivity.ProjectModuleId = model.ProjectModuleId;
                        projectActivity.TaskId = model.TaskId;
                        projectActivity.Name = model.ActivityName;
                        projectActivity.EstimateHours = model.EstimateHours;
                        projectActivity.AssignedToId = model.AssignedToId;
                        projectActivity.ActivityStatusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Activity Status", "New");

                        projectActivity.Active = true;
                        projectActivity.CreatedById = LoggedUserId;
                        projectActivity.CreatedOnUtc = GetDateTime;
                        projectActivity.UpdatedById = LoggedUserId;
                        projectActivity.UpdatedOnUtc = GetDateTime;
                        _activityService.InsertProjectActivity(projectActivity);
                    }

                    return NoContent();
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

        #region Update activeStatus
        [HttpPut("updateActivityActiveStatus")]
        public async Task<IActionResult> UpdateActivityActiveStatus(ProjectActivityModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.ActivityIds != null && model.ActivityIds.Count > 0)
                    {
                        foreach (var activityId in model.ActivityIds)
                        {
                            var activityEntity = await _activityService.GetById(activityId);
                            if (activityEntity != null)
                            {
                                bool result = await UpdateActivityDetails(activityId, model.Active, "active");

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

        #region Private Function For Activity
        private async Task<bool> UpdateActivityDetails(string activityId, object data, string flag)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            var entity = await _activityService.GetById(activityId);
            if (entity == null)
                return false;

            switch (flag.ToLower())
            {
                case "status":
                    if (data is string statusId)
                        entity.ActivityStatusId = statusId;
                    break;

                case "active":
                    if (data is bool isActive)
                        entity.Active = isActive ? true : false;
                    break;

                default:
                    return false;
            }

            entity.UpdatedById = LoggedUserId;
            entity.UpdatedOnUtc = GetDateTime;
            _activityService.UpdateProjectActivity(entity);

            return true;
        }
        #endregion

        #region GetProjectActivityByIdForTimer
        // Title: GetProjectActivityById
        // Description: This endpoint retrieves the details of a specific project activity based on its unique identifier (ID). 
        [HttpPost("{id}/timer")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProjectActivityForTimerById(string id)
        {
            try
            {
                // Fetch the project activity entity by its ID from the service
                var entity = await _activityService.GetProjectActivityForTimerById(id);
                // If the project activity entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No project activity found with the specified id."));

                // Map the project activity entity to a ProjectActivityModel object
                var model = _mapper.Map<ProjectActivityModel>(entity);
                
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetProjectActivityDescriptionById
        // Title: GetProjectActivityDescriptionById
        // Description: Retrieves only the description of a specific Project Activity based on its ID.
        [HttpGet("description/{id}")]
        public async Task<IActionResult> GetProjectActivityDescriptionById(string id)
        {
            try
            {
                var description = await _activityService.GetProjectActivityDescriptionById(id);
                return Ok(new
                {
                    description
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateDescription
        //created for update description from list page
        [HttpPut("description/{id}")]
        public async Task<IActionResult> UpdateDescription(string id, ProjectActivityModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _activityService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project activity found with the specified id."));


                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "project-tasks-activities",
                                entity.Id,
                                entity.Description
                            );
                    }
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _activityService.UpdateProjectActivity(entity);

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
    }
}