using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.ProjectModules;
using Vsky.Services.ProjectModulesUserMappings;
using Vsky.Services.Projects;
using Vsky.Services.ProjectUserMappings;
using Vsky.Services.Sites;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Api.Controllers
{
    [Route("project-module-users")]
    public class ProjectModulesUserMappingController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IProjectModuleService _projectModuleService;
        private readonly ISiteService _siteService;
        private readonly IProjectModulesUserMappingService _projectModulesUserMappingService;
        private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations
        public ProjectModulesUserMappingController(
            GlobalVariable globalVariable,
            IMapper mapper,
            IProjectModuleService projectModuleService,
            ISiteService siteService, 
            IProjectModulesUserMappingService projectModulesUserMappingService,
            ICommonService commonService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _projectModuleService = projectModuleService;
            _siteService = siteService;
            _projectModulesUserMappingService = projectModulesUserMappingService;
            _commonService = commonService;
        }
        #endregion

        #region GetUsersByProjectModuleIds
        // Title: GetUsersByProjectModuleIds
        // Description: This endpoint retrieves the project users by project id. 
        [HttpGet("user/{projectId}")]
        public async Task<IActionResult> GetUsersByProjectModuleIds(string projectId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var moduleList = await _projectModuleService.GetAllModulesByProjectId(projectId);

                if (moduleList.Any()) // check if count > 0
                {
                    var moduleIds = moduleList.Select(m => m.Id).ToList();
                    var list = await _projectModulesUserMappingService.GetUsersByProjectModuleIds(SiteId, moduleIds);
                    var model = _mapper.Map<List<ProjectModulesUserMapping>>(list);

                    return Ok(model);
                }

                // If no modules, return empty list
                return Ok(new List<ProjectModulesUserMapping>());

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region AssignBulkUsersToProjectModule
        [HttpPut("savebulk/{ids}")]
        public async Task<IActionResult> AssignBulkUsersToProjectModule(string ids, ProjectModuleModel model)
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
                            if (string.IsNullOrWhiteSpace(user))
                            {
                                notFoundUsers.Add(user);
                                continue;
                            }
                            if (!string.IsNullOrWhiteSpace(user))
                            {
                                if (model.ProjectModulesUserMappings.Count() > 0)
                                {
                                    foreach (var projectModuleUser in model.ProjectModulesUserMappings)
                                    {
                                        if (!string.IsNullOrWhiteSpace(projectModuleUser.ProjectModuleId))
                                        {
                                            var existUserUnderProjectModule = await _projectModulesUserMappingService.GetRecordByUserIdandProjectModuleId(SiteId, user, projectModuleUser.ProjectModuleId);
                                            if (existUserUnderProjectModule != null)
                                            {
                                                existUserUnderProjectModule.FullAccess = projectModuleUser.FullAccess;
                                                existUserUnderProjectModule.ViewOnly = projectModuleUser.ViewOnly;
                                                existUserUnderProjectModule.Notes = projectModuleUser.Notes;

                                                existUserUnderProjectModule.UpdatedById = LoggedUserId;
                                                existUserUnderProjectModule.UpdatedOnUtc = GetDateTime;
                                                _projectModulesUserMappingService.UpdateProjectModuleUser(existUserUnderProjectModule);
                                            }
                                            else
                                            {
                                                ProjectModulesUserMapping ProjectModulesUserMapping = new ProjectModulesUserMapping();
                                                ProjectModulesUserMapping.ProjectModuleId = projectModuleUser.ProjectModuleId;
                                                ProjectModulesUserMapping.AspNetUserId = user;
                                                ProjectModulesUserMapping.FullAccess = projectModuleUser.FullAccess;
                                                ProjectModulesUserMapping.ViewOnly = projectModuleUser.ViewOnly;
                                                ProjectModulesUserMapping.Notes = projectModuleUser.Notes;
                                                ProjectModulesUserMapping.Deleted = projectModuleUser.Deleted;

                                                ProjectModulesUserMapping.CreatedById = LoggedUserId;
                                                ProjectModulesUserMapping.CreatedOnUtc = GetDateTime;
                                                ProjectModulesUserMapping.UpdatedById = LoggedUserId;
                                                ProjectModulesUserMapping.UpdatedOnUtc = GetDateTime;
                                                _projectModulesUserMappingService.InsertProjectModuleUser(ProjectModulesUserMapping);
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

        #region UpdateUserProjectModuleAccess
        // Title: UpdateUserProjectModuleAccess
        // Description: This endpoint updates an existing user in project module by its mapping ID.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserProjectModuleAccess(string id, SaveProjectModulesUser model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var existUserUnderProjectModule = await _projectModulesUserMappingService.GetProjectModuleUserById(id);
                    if (existUserUnderProjectModule != null)
                    {
                        existUserUnderProjectModule.FullAccess = model.FullAccess;
                        existUserUnderProjectModule.ViewOnly = model.ViewOnly;
                        existUserUnderProjectModule.Notes = model.Notes;

                        existUserUnderProjectModule.UpdatedById = LoggedUserId;
                        existUserUnderProjectModule.UpdatedOnUtc = GetDateTime;
                        _projectModulesUserMappingService.UpdateProjectModuleUser(existUserUnderProjectModule);
                    }

                    return Ok(existUserUnderProjectModule);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteProjectModuleUser
        // Title: DeleteProjectModuleUserById
        // Description: This endpoint deletes a project module user based on the provided project ID. It first retrieves the project module user entity by ID, checks if it exists, and if so, deletes the project module user. If the project module user is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectModuleUser(string id)
        {
            try
            {
                // Fetch the project module user entity by its ID
                var entity = await _projectModulesUserMappingService.GetProjectModuleUserById(id);
                // If no project module user is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No project module user found with the specified id."));

                // Delete the project module user using the project module mapping service
                _projectModulesUserMappingService.DeleteProjectModuleUser(entity);

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
