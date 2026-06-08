using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.Notifications;
using Vsky.Services.ProjectActivities;
using Vsky.Services.ProjectEmployeeMappings;
using Vsky.Services.ProjectMessage;
using Vsky.Services.ProjectModules;
using Vsky.Services.ProjectModulesUserMappings;
using Vsky.Services.Projects;
using Vsky.Services.ProjectTasks;
using Vsky.Services.ProjectUserMappings;
using Vsky.Services.ProjectWeeklyPlan;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Api.Controllers
{
    [Route("project-users")]
    public class ProjectUserMappingController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly ISiteService _siteService;
        private readonly IProjectUserMappingService _projectUserMappingService;
        private readonly ICommonService _commonService;
        private readonly IDropDownService _dropDownService;
        private readonly IProjectModuleService _projectModuleService;
        private readonly IProjectModulesUserMappingService _projectModulesUserMappingService;
        #endregion

        #region Services Initializations
        public ProjectUserMappingController(
            GlobalVariable globalVariable,
            IMapper mapper,
            IProjectService projectService,
            ISiteService siteService, IProjectUserMappingService projectUserMappingService,
            ICommonService commonService, IDropDownService dropDownService,
            IProjectModuleService projectModuleService,
            IProjectModulesUserMappingService projectModulesUserMappingService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _projectService = projectService;
            _siteService = siteService;
            _projectUserMappingService = projectUserMappingService;
            _commonService = commonService;
            _dropDownService = dropDownService;
            _projectModuleService = projectModuleService;
            _projectModulesUserMappingService = projectModulesUserMappingService;
        }
        #endregion

        #region GetAllProjectsForUserPermission
        // Title: Get All Projects For User Permission
        // Description: This endpoint fetches a list of projects based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllProjectsForUserPermission(ProjectSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of projects based on search criterias
                var list = _projectUserMappingService.GetAllProjectsForUserPermission(SiteId, searchModel.IsTemplate, searchModel.SearchText, searchModel.ProjectIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
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

        #region GetProjectUserByProjectId
        // Title: GetProjectUserByProjectId
        // Description: This endpoint retrieves the details of a specific project activity based on its unique identifier (ID). 
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetProjectUserByProjectId(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _projectUserMappingService.GetProjectUserByProjectId(SiteId, id);
                var model = _mapper.Map<List<ProjectUserMappingModel>>(list);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region AssignUsersToProject
        [HttpPut("{id}")]
        public async Task<IActionResult> AssignUsersToProject(string id, ProjectModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.ProjectUserMappings.Count() > 0)
                    {
                        var processedUserIds = new HashSet<string>();
                        foreach (var projectUser in model.ProjectUserMappings)
                        {
                            if (!string.IsNullOrWhiteSpace(projectUser.AspNetUserId))
                            {
                                var existUserUnderProject = await _projectUserMappingService.GetRecordByUserIdandProjectId(SiteId, projectUser.AspNetUserId, id);
                                if (projectUser.Deleted)
                                {
                                    var exisitingProjectUserData = await _projectUserMappingService.GetProjectUserById(projectUser.Id);
                                    if (exisitingProjectUserData != null)
                                    {
                                        exisitingProjectUserData.ProjectId = id;
                                        exisitingProjectUserData.AspNetUserId = projectUser.AspNetUserId;
                                        exisitingProjectUserData.FullAccess = projectUser.FullAccess;
                                        exisitingProjectUserData.ViewOnly = projectUser.ViewOnly;
                                        exisitingProjectUserData.Notes = projectUser.Notes;
                                        exisitingProjectUserData.Deleted = projectUser.Deleted;
                                        _projectUserMappingService.UpdateProjectUser(exisitingProjectUserData);

                                        var moduleIds = (await _projectModuleService.GetAllModulesByProjectId(id))
                                                            .Select(m => m.Id).ToList();

                                        if (moduleIds.Any())
                                        {
                                            var users = await _projectModulesUserMappingService.GetUsersByProjectModuleIds(SiteId, moduleIds);
                                            var usersToDelete = users.Where(x => x.AspNetUserId == projectUser.AspNetUserId).ToList();

                                            foreach (var user in usersToDelete)
                                            {
                                                var entity = await _projectModulesUserMappingService.GetProjectModuleUserById(user.Id);
                                                _projectModulesUserMappingService.DeleteProjectModuleUser(entity);
                                            }
                                        }
                                    }
                                }
                                else if (existUserUnderProject != null)
                                {
                                    // Skip if this AspNetUserId is already processed
                                    if (!processedUserIds.Add(existUserUnderProject.Id))
                                        continue;

                                    existUserUnderProject.FullAccess = projectUser.FullAccess;
                                    existUserUnderProject.ViewOnly = projectUser.ViewOnly;
                                    existUserUnderProject.Notes = projectUser.Notes;
                                    _projectUserMappingService.UpdateProjectUser(existUserUnderProject);
                                }
                                else
                                {
                                    ProjectUserMapping ProjectUserMapping = new ProjectUserMapping();
                                    ProjectUserMapping.ProjectId = id;
                                    ProjectUserMapping.AspNetUserId = projectUser.AspNetUserId;
                                    ProjectUserMapping.FullAccess = projectUser.FullAccess;
                                    ProjectUserMapping.ViewOnly = projectUser.ViewOnly;
                                    ProjectUserMapping.Notes = projectUser.Notes;
                                    ProjectUserMapping.Deleted = projectUser.Deleted;

                                    ProjectUserMapping.CreatedById = LoggedUserId;
                                    ProjectUserMapping.CreatedOnUtc = GetDateTime;
                                    _projectUserMappingService.InsertProjectUser(ProjectUserMapping);
                                }
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

        #region AssignBulkProject
        [HttpPut("savebulk/{ids}")]
        public async Task<IActionResult> AssignBulkProject(string ids, ProjectModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    List<string> notFoundUsers = new List<string>();
                    if (!string.IsNullOrEmpty(ids))
                    {
                        var idArray = ids.Split(',');
                        foreach (var user in idArray)
                        {
                            var AspNetUserId = _commonService.GetLoggeduserIdByEmployeeId(SiteId, user);
                            if (string.IsNullOrWhiteSpace(AspNetUserId))
                            {
                                notFoundUsers.Add(user);
                                continue;
                            }
                            if (!string.IsNullOrWhiteSpace(AspNetUserId))
                            {
                                if (model.ProjectUserMappings.Count() > 0)
                                {
                                    foreach (var projectUser in model.ProjectUserMappings)
                                    {
                                        if (!string.IsNullOrWhiteSpace(projectUser.ProjectId))
                                        {
                                            var existUserUnderProject = await _projectUserMappingService.GetRecordByUserIdandProjectId(SiteId, AspNetUserId, projectUser.ProjectId);
                                            if (existUserUnderProject != null)
                                            {
                                                existUserUnderProject.FullAccess = projectUser.FullAccess;
                                                existUserUnderProject.ViewOnly = projectUser.ViewOnly;
                                                existUserUnderProject.Notes = projectUser.Notes;
                                                _projectUserMappingService.UpdateProjectUser(existUserUnderProject);
                                            }
                                            else
                                            {
                                                ProjectUserMapping ProjectUserMapping = new ProjectUserMapping();
                                                ProjectUserMapping.ProjectId = projectUser.ProjectId;
                                                ProjectUserMapping.AspNetUserId = AspNetUserId;
                                                ProjectUserMapping.FullAccess = projectUser.FullAccess;
                                                ProjectUserMapping.ViewOnly = projectUser.ViewOnly;
                                                ProjectUserMapping.Notes = projectUser.Notes;
                                                ProjectUserMapping.Deleted = projectUser.Deleted;

                                                ProjectUserMapping.CreatedById = LoggedUserId;
                                                ProjectUserMapping.CreatedOnUtc = GetDateTime;
                                                _projectUserMappingService.InsertProjectUser(ProjectUserMapping);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (notFoundUsers.Any())
                    {
                        return NotFound(new { message = "Something went wrong." });
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

        #region GetProjectUserSettingProjectId
        [HttpGet("get-project-user-setting-by-projectid/{id}")]
        public async Task<IActionResult> GetProjectUserSettingProjectId(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var data = await _projectUserMappingService.GetRecordByUserIdandProjectId(SiteId, LoggedUserId, id);
                var model = _mapper.Map<ProjectUserMappingModel>(data);

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