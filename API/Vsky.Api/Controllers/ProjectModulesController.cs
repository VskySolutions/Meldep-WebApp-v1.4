using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
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
using Vsky.Services.ProjectActivities;
using Vsky.Services.ProjectModules;
using Vsky.Services.ProjectTasks;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("project-modules")]
    public class ProjectModulesController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IProjectModuleService _projectModuleService;
        private readonly ApplicationDbContext _db;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IProjectModuleFilesService _projectModuleFilesService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IProjectActivityService _projectActivityService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations
        public ProjectModulesController(
            GlobalVariable globalVariable,
            IMapper mapper,
            IProjectModuleService projectModuleService,
            ApplicationDbContext db,
            ISiteService siteService,
            ICommonService commonService,
            IProjectModuleFilesService projectModuleFilesService,
            IProjectTaskService projectTaskService,
            IProjectActivityService projectActivityService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _projectModuleService = projectModuleService;
            _db = db;
            _siteService = siteService;
            _commonService = commonService;
            _projectModuleFilesService = projectModuleFilesService;
            _projectTaskService = projectTaskService;
            _projectActivityService = projectActivityService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllProjectModules
        // Title: Get All ProjectModules
        // Description: This endpoint fetches a list of ProjectModules based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllProjectModules(ProjectModuleSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of project module based on search criteria (name, sorting, pagination)
                var list = _projectModuleService.GetAllProjectModules(SiteId, searchModel.SearchText, searchModel.ProjectIds, searchModel.ProjectModuleTypeIds, searchModel.ProjectModuleStatusIds, searchModel.ProjectId, searchModel.CustomerIds, searchModel.CompanyContactIds, searchModel.isShowCloseStatus, searchModel.pageName, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
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

        #region GetAllProjectModulesByProjectId
        [HttpGet("listbyproject")]
        public async Task<IActionResult> GetAllModulesByProjectId(string projectId)
        {
            var list = await _projectModuleService.GetAllModulesByProjectId(projectId);
            var model = _mapper.Map<IList<ProjectModuleModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetAllProjectModuleListForDropdown
        // Title: GetAllProjectModuleListForDropdown
        // Description: This endpoint retrieves the details of a specific ProjectModule based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllProjectModuleListForDropdown(bool isTemplate, bool showTaskCount, string ProjectId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _projectModuleService.GetAllProjectModuleListForDropdown(SiteId, isTemplate, showTaskCount, ProjectId);
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetProjectModuleById
        // Title: GetProjectModuleById
        // Description: This endpoint retrieves the details of a specific Project Module based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectModuleById(string id)
        {
            try
            {
                // Fetch the project module entity by its ID from the service
                var entity = await _projectModuleService.GetProjectModuleById(id);
                // If the project module entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Project Module found with the specified id."));

                // Map the project module entity to a ProjectModuleModel object
                var model = _mapper.Map<ProjectModuleModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetNextSortOrderOfProjectModuleAndTask
        // Title: GetNextSortOrderOfProjectModuleAndTask
        // Description: This endpoint retrieves the next sort Order of Project Module and task. 
        [HttpGet("sort-order")]
        public async Task<NextSortOrder> GetNextSortOrderOfProjectModuleAndTask(string projectId = null, string moduleId = null)
        {
            try
            {
                // Fetch the project module sort order
                int NextSortOrderOfProjectModule = 0;
                if (!string.IsNullOrWhiteSpace(projectId) && projectId != "undefined" && projectId != "null")
                {
                    NextSortOrderOfProjectModule = await _projectModuleService.GetLastSortOrderOfProjectModules(projectId) + 1;
                }
                decimal NextSortOrderOfProjectTask = 0;
                int SelectedModuleSortOrder = 0;
                decimal CurrentModuleSortOrder = 0;
                if (!string.IsNullOrWhiteSpace(moduleId) && moduleId != "undefined")
                {
                    // Get module sort order
                    var module = await _projectModuleService.GetProjectModuleDetailsById(moduleId);

                    if (module == null)
                        throw new Exception("Module not found");

                    int moduleSortOrder = module.SortOrder;

                    // Get last task sort order under this module
                    CurrentModuleSortOrder = await _projectTaskService.GetLastSortOrderOfProjectTasks(moduleId);

                    // If no tasks found, lastTaskSortOrder will be 1.1m
                    if (CurrentModuleSortOrder == 1.1m)
                    {
                        //NextSortOrderOfProjectTask = moduleSortOrder + 0.1m;
                        NextSortOrderOfProjectTask = moduleSortOrder + 0.001m;
                    }
                    else
                    {
                        //NextSortOrderOfProjectTask = CurrentModuleSortOrder + 1m; // increment
                        NextSortOrderOfProjectTask = Math.Round(CurrentModuleSortOrder + 0.001m, 3);
                    }

                    // Integer part of the task sort order (before decimal)
                    SelectedModuleSortOrder = (int)Math.Floor(NextSortOrderOfProjectTask);
                }

                return new NextSortOrder
                {
                    CurrentModuleSortOrder = CurrentModuleSortOrder,
                    NextSortOrderOfProjectModule = NextSortOrderOfProjectModule,
                    NextSortOrderOfProjectTask = NextSortOrderOfProjectTask,
                    SelectedModuleSortOrder = SelectedModuleSortOrder
                };
            }
            catch (Exception ex)
            {
                return new NextSortOrder
                {
                    NextSortOrderOfProjectModule = 1,
                    NextSortOrderOfProjectTask = 1.1m
                };
            }
        }
        #endregion

        #region GetProjectModuleDetailsById
        // Title: GetProjectModuleDetailsById
        // Description: This endpoint retrieves the details of a specific project module based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetProjectModuleDetailsById(string id)
        {
            try
            {
                // Fetch the project entity by its ID from the service
                var entity = await _projectModuleService.GetProjectModuleDetailsById(id);
                // If the project entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No project module found with the specified id."));

                // Map the project entity to a ProjectModuleModel object
                var model = _mapper.Map<ProjectModuleModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateProjectModule
        // Title: CreateProjectModule
        // Description: This endpoint handles the creation of a new project module. It first checks if a project module with the same name already exists for the specified name. If not, it maps the Project Module model to the Project Module entity, sets the creation details, and inserts the Project Module into the database.
        [HttpPost]
        public async Task<IActionResult> CreateProjectModule([FromForm] ProjectModuleModel model)
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
                    var exists = await _projectModuleService.GetProjectModuleByName(model.Name, model.ProjectId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Project Module already exists"));

                    var record = _db.ProjectModules.OrderByDescending(m => m.ProjectModuleNumber).FirstOrDefault();
                    // Map the Project Module model to the Project Module entity
                    var entity = _mapper.Map<ProjectModule>(model);
                    entity.ProjectModuleNumber = record != null ? record.ProjectModuleNumber + 1 : 1;
                    entity.SiteId = SiteId;
                    // Set custom properties
                    if (model.StartDateStr != "")
                        entity.StartDate = DateTime.ParseExact(model.StartDateStr, "MM/dd/yyyy", null);
                    if (model.EndDateStr != "")
                        entity.EndDate = DateTime.ParseExact(model.EndDateStr, "MM/dd/yyyy", null);

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "project-modules",
                                entity.ProjectModuleNumber.ToString(),
                                entity.Description
                            );
                    }

                    // Set the created by and created on properties
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _projectModuleService.InsertProjectModule(entity);

                    var Warning = "";
                    var sortOrderExist = await _projectModuleService.IsProjectModuleSortOrderExists(SiteId, model.ProjectId, model.SortOrder, entity.Id);
                    if (sortOrderExist)
                    {
                        Warning = "Sort order already exists";
                    }

                    var moduleData = await _projectModuleService.GetProjectModuleDetailsById(entity.Id);

                    string ProjectModuleId = entity.Id;
                    if (model.ProjectModuleFiles != null && model.ProjectModuleFiles.Any())
                    {
                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "project-modules", model.ProjectModuleFiles, entity.ProjectModuleNumber.ToString());
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ProjectModuleFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = model.ProjectId,
                                Module = moduleData.Project.Name,
                                SubModuleId = ProjectModuleId,
                                Sub_Module = entity.Name,
                                Type = "Project Module",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var ProjectModuleFiles = new ProjectModuleFiles
                            {
                                FileId = picture.Id,
                                ProjectModuleId = ProjectModuleId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _projectModuleFilesService.InsertProjectModuleFile(ProjectModuleFiles);

                            index++;
                        }
                    }

                    //return NoContent();
                    return Ok(new
                    {
                        success = true,
                        message = "Project module saved successfully.",
                        Warning
                    });
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

        #region AddFiles
        // Title: AddFiles
        // Description: This endpoint handles the creation of a new file. It first checks if a project with the same name already exists for the specified customer. If not, it maps the project model to the project entity, sets the creation details, and inserts the project into the database. Additionally, it associates team members with the project if provided.
        [HttpPost("add-module-files")]
        public async Task<IActionResult> AddFiles([FromForm] ProjectModuleModel model)
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
                    var entity = await _projectModuleService.GetProjectModuleById(model.Id);

                    // If no project is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project module found with the specified id."));

                    var moduleData = await _projectModuleService.GetProjectModuleDetailsById(entity.Id);
                    //Add Project Employees
                    string ProjectModuleId = entity.Id;
                    if (model.ProjectModuleFiles != null && model.ProjectModuleFiles.Any())
                    {
                        int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(model.Id, "Project Module");

                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "project-modules", model.ProjectModuleFiles, entity.ProjectModuleNumber.ToString(), existingImagesCount);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ProjectModuleFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = model.ProjectId,
                                Module = moduleData.Project.Name,
                                SubModuleId = ProjectModuleId,
                                Sub_Module = entity.Name,
                                Type = "Project Module",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var ProjectModuleFiles = new ProjectModuleFiles
                            {
                                FileId = picture.Id,
                                ProjectModuleId = ProjectModuleId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _projectModuleFilesService.InsertProjectModuleFile(ProjectModuleFiles);

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

        #region UpdateProjectModule
        // Title: UpdateProjectModule
        // Description: This endpoint updates an existing project module by its ID. It validates the project module model, checks for duplicate module names within the same project, updates the modules's details.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProjectModule(string id, [FromForm] ProjectModuleModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Check if there is any project module with the same name.
                    var exists = await _projectModuleService.GetProjectModuleByName(model.Name, model.ProjectId, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Project Module already exists"));

                    // Fetch the project module entity by its ID
                    var entity = await _projectModuleService.GetProjectModuleById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Project Module found with the specified id."));

                    var Warning = "";
                    var sortOrderExist = await _projectModuleService.IsProjectModuleSortOrderExists(SiteId, model.ProjectId, model.SortOrder, id);
                    if (sortOrderExist)
                    {
                        Warning = "Sort order already exists";
                    }

                    // Retrieve all file IDs from the project module files
                    var allProjectModuleFileIds = (await _projectModuleFilesService.GetAllProjectModuleFilesByProjectModuleId(id))
                                             .Select(file => file.Id)  // Extract Id from ProjectFiles
                                             .ToList();

                    var missingFileIds = allProjectModuleFileIds.ToList();
                    if (model.ExistingFiles != null)
                    {
                        var existingFileIds = model.ExistingFiles.Select(fileJson =>
                        {
                            var file = JsonConvert.DeserializeObject<Picture>(fileJson);
                            return file.Id.Trim().ToLower();
                        })
                        .ToList();

                        // Compare and find missing file IDs
                        missingFileIds = allProjectModuleFileIds.Except(existingFileIds).ToList();
                    }

                    if (missingFileIds.Any())
                    {
                        foreach (var projectModuleFilesId in missingFileIds)
                        {
                            var projectFileDate = await _projectModuleFilesService.GetProjectModuleFileById(projectModuleFilesId);
                            if (projectFileDate != null)
                                _projectModuleFilesService.DeleteProjectModuleFile(projectFileDate);
                        }
                    }

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
                                "project-modules",
                                entity.ProjectModuleNumber.ToString(),
                                entity.Description
                            );
                    }

                    entity.ProjectId = model.ProjectId;
                    entity.SortOrder = model.SortOrder;
                    entity.Name = model.Name;
                    entity.ProjectModuleStatusId = model.ProjectModuleStatusId;
                    entity.Description = model.Description;
                    entity.Notes = model.Notes;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _projectModuleService.UpdateProjectModule(entity);

                    var moduleData = await _projectModuleService.GetProjectModuleDetailsById(id);

                    if (model.ProjectModuleFiles != null && model.ProjectModuleFiles.Any())
                    {
                        int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(model.Id, "Project Module");

                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "project-modules", model.ProjectModuleFiles, entity.ProjectModuleNumber.ToString(), existingImagesCount);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ProjectModuleFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = model.ProjectId,
                                Module = moduleData.Project.Name,
                                SubModuleId = id,
                                Sub_Module = entity.Name,
                                Type = "Project Module",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var ProjectModuleFiles = new ProjectModuleFiles
                            {
                                FileId = picture.Id,
                                ProjectModuleId = id,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _projectModuleFilesService.InsertProjectModuleFile(ProjectModuleFiles);

                            index++;
                        }
                    }

                    return Ok(new
                    {
                        success = true,
                        message = "Project module saved successfully.",
                        Warning
                    });
                    //return NoContent();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            // Return model state errors if the model state is not valid
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateModuleColor
        [HttpPost("update-module-color")]
        public async Task<IActionResult> UpdateModuleColor(ProjectModuleModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the project module entity by its ID
                    var entity = await _projectModuleService.GetProjectModuleById(model.Id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No module found with the specified id."));

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
                    _projectModuleService.UpdateProjectModule(entity);

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

        //created for update project module status from list page by SN
        #region
        [HttpPut("{id}/{projectModuleStatusId}")]
        public async Task<IActionResult> UpdateProjectModuleStatus(string id, string projectModuleStatusId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _projectModuleService.GetProjectModuleById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Project Module found with the specified id."));

                    entity.ProjectModuleStatusId = projectModuleStatusId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _projectModuleService.UpdateProjectModule(entity);

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

        #region DeleteProjectModule
        // Title: DeleteProjectModule
        // Description: This endpoint deletes a person module based on the provided project module ID. It first retrieves the project module entity by ID, checks if it exists, and if so, deletes the project module. If the module is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectModule(string id)
        {
            try
            {
                // Fetch the project module entity by its ID
                var entity = await _projectModuleService.GetProjectModuleById(id);
                // If no project module is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Project Module found with the specified id."));

                // Delete the project module using the project module service
                _projectModuleService.DeleteProjectModule(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("module-file/{id}")]
        public async Task<IActionResult> DeleteProjectModuleFile(string id)
        {
            try
            {
                var entity = await _projectModuleFilesService.GetProjectModuleFileById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No file found with the specified id."));

                _projectModuleFilesService.DeleteProjectModuleFile(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        [HttpGet("CheckModuleCanBeDeleted/{moduleId}")]
        public async Task<IActionResult> CheckModuleCanBeDeleted(string moduleId)
        {
            // Check if any active tasks exists
            var tasks = await _projectTaskService.GetTaskByProjectModuleId(moduleId);

            bool hasActiveTasks = tasks.Any(x => !x.Deleted && (x.Status.DropDownValue != "Close" && x.Status.DropDownValue != "Completed"));

            // Get all task IDs in this module
            var taskIds = tasks
                .Where(t => !t.Deleted)
                .Select(t => t.Id)
                .ToList();

            // Check if any active task activity exists
            var activities = await _projectActivityService.GetProjectActivitiesByModuleId(moduleId);

            // Check if any active task activity exists
            var hasActiveActivities = activities.Any(a =>
                taskIds.Contains(a.TaskId) &&
                !a.Deleted &&
                (a.ActivityStatus.DropDownValue != "Completed" && a.ActivityStatus.DropDownValue != "Close")
            );

            // If either tasks or activities are active, disallow delete
            bool canDelete = !(hasActiveTasks || hasActiveActivities);

            return Ok(new { canDelete });
        }

    }
}