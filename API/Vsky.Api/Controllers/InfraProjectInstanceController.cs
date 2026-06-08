using System.Threading.Tasks;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Models;
using Vsky.Services.Sites;
using Vsky.Services.Common;
using System.Linq;
using Vsky.Api.ApiErrors;
using Vsky.Services.InfraProjectInstances;
using Vsky.Api.Models;
using Microsoft.PowerBI.Api.Models;

namespace Vsky.Api.Controllers
{
    [Route("infra-project-instance")]
    public class InfraProjectInstanceController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IInfraProjectInstanceService _infraProjectInstanceService;
        private readonly IInfraProjectInstanceRoleService _infraProjectInstanceRoleService;
        private readonly IInfraProjectInstanceRoleUsersService _infraProjectInstanceRoleUsersService;
        #endregion

        #region Services Initializations
        public InfraProjectInstanceController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISiteService siteService,
            ICommonService commonService,
            IInfraProjectInstanceService infraProjectInstanceService,
            IInfraProjectInstanceRoleService infraProjectInstanceRoleService,
            IInfraProjectInstanceRoleUsersService infraProjectInstanceRoleUsersService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _siteService = siteService;
            _commonService = commonService;
            _infraProjectInstanceService = infraProjectInstanceService;
            _infraProjectInstanceRoleService = infraProjectInstanceRoleService;
            _infraProjectInstanceRoleUsersService = infraProjectInstanceRoleUsersService;
        }
        #endregion

        #region GetAllInfraProjectInstanceForList
        // Title: GetAllInfraProjectInstanceForList
        // Description: This endpoint retrieves the list of infra project instance.
        [HttpPost("list")]
        public IActionResult GetAllInfraProjectInstanceForList(InfraProjectInstanceSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of infra project instance based on search criteria (name, sorting, pagination)
                var list = _infraProjectInstanceService.GetAllInfraProjectInstanceForList(
                    SiteId,
                    searchModel.InfraProjectIds,
                    searchModel.PlatformIds,
                    searchModel.InstanceTypeIds,
                    searchModel.SearchText,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new InfraProjectInstanceList
                {
                    InfraProjectInstancesList = list,
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

        #region GetAllInfraProjectInstanceListForDropdown
        // Title: GetAllInfraProjectInstanceListForDropdown
        // Description: This endpoint retrieves the list of Infra Account for dropdown. 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllInfraProjectInstanceListForDropdown(string projectId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _infraProjectInstanceService.GetAllInfraProjectInstanceListForDropdown(SiteId, projectId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetInfraProjectInstanceInDetailById
        // Title: GetInfraProjectInstanceInDetailById
        // Description: This endpoint retrieves the details of a specific infra project instance based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetInfraProjectInstanceInDetailById(string id)
        {
            try
            {
                var entity = await _infraProjectInstanceService.GetInfraProjectInstanceInDetailById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra project instance found with the specified id."));

                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetInfraProjectInstanceRoleInDetailByInstanceId
        // Title: GetInfraProjectInstanceRoleInDetailByInstanceId
        // Description: This endpoint retrieves the details of a specific Infra Project Instance Role based on its unique identifier (ID). 
        [HttpGet("role-details/{instanceId}")]
        public async Task<IActionResult> GetInfraProjectInstanceRoleInDetailByInstanceId(string instanceId)
        {
            try
            {
                var list = await _infraProjectInstanceRoleService.GetInfraProjectInstanceRoleInDetailByInstanceId(instanceId);
                if (list == null)
                    return BadRequest(new BadRequestError("No infra project instance role found with the specified id."));

                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Add & Update infra project instance
        [HttpPost()]
        public async Task<IActionResult> AddUpdateInfraProjectInstance(SaveInfraProjectInstanceList model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.InfraProjectInstanceLines != null && model.InfraProjectInstanceLines.Count() > 0)
                    {
                        foreach (var InfraProjectInstance in model.InfraProjectInstanceLines)
                        {
                            var existing = await _infraProjectInstanceService.GetInfraProjectInstanceById(InfraProjectInstance.Id);
                            if (existing != null && !InfraProjectInstance.Deleted)
                            {
                                // update existing
                                existing.InfraProjectId = InfraProjectInstance.InfraProjectId;
                                existing.InstanceTypeId = InfraProjectInstance.InstanceTypeId;
                                existing.PlatformId = InfraProjectInstance.PlatformId;
                                existing.URL = InfraProjectInstance.URL;
                                existing.Instructions = !string.IsNullOrEmpty(InfraProjectInstance.Instructions) ? InfraProjectInstance.Instructions : null;
                                existing.UpdatedOnUtc = GetDateTime;
                                existing.UpdatedById = LoggedUserId;

                                _infraProjectInstanceService.UpdateInfraProjectInstance(existing);
                            }
                            else if (existing == null && !InfraProjectInstance.Deleted)
                            {
                                // insert new
                                var ftp = new InfraProjectInstance
                                {
                                    Id = InfraProjectInstance.Id ?? Guid.NewGuid().ToString(),
                                    SiteId = SiteId,
                                    InfraProjectId = InfraProjectInstance.InfraProjectId,
                                    InstanceTypeId = InfraProjectInstance.InstanceTypeId,
                                    PlatformId = InfraProjectInstance.PlatformId,
                                    URL = InfraProjectInstance.URL,
                                    Instructions = !string.IsNullOrEmpty(InfraProjectInstance.Instructions) ? InfraProjectInstance.Instructions : null,
                                    CreatedOnUtc = GetDateTime,
                                    CreatedById = LoggedUserId,
                                    UpdatedOnUtc = GetDateTime,
                                    UpdatedById = LoggedUserId
                                };
                                _infraProjectInstanceService.InsertInfraProjectInstance(ftp);
                            }
                        }
                    }

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region Save Infra Project Instance Roles
        [HttpPost("save-project-instance-roles")]
        public async Task<IActionResult> SaveInfraProjectInstanceRoles(SaveInfraProjectInstanceRoleRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ModelStateError(ModelState);

                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (model.InfraProjectInstanceRoleList == null || !model.InfraProjectInstanceRoleList.Any())
                    return Ok();

                foreach (var role in model.InfraProjectInstanceRoleList)
                {
                    if (string.IsNullOrWhiteSpace(role.RoleName) && role.Flag != "Delete")
                        continue;

                    InfraProjectInstanceRole roleEntity = null;

                    if (role.Flag == "Edit" || role.Flag == "Delete")
                    {
                        // check exist
                        roleEntity = await _infraProjectInstanceRoleService.GetInfraProjectInstanceRoleById(role.Id);
                        if (roleEntity == null)
                            continue;
                    }

                    // delete role
                    if (role.Flag == "Delete")
                    {
                        _infraProjectInstanceRoleService.DeleteInfraProjectInstanceRole(roleEntity);
                        continue;
                    }

                    // add new role
                    if (role.Flag == "New")
                    {
                        //Check if the infra Project Instance Role already exists
                        var exists = await _infraProjectInstanceRoleService.GetInfraProjectInstanceRoleByRoleName(SiteId, model.ProjectInstanceId, role.RoleName);
                        if (exists != null)
                            continue;

                        roleEntity = new InfraProjectInstanceRole
                        {
                            ProjectInstanceId = model.ProjectInstanceId,
                            RoleName = role.RoleName,
                            CreatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime,
                            UpdatedById = LoggedUserId,
                            UpdatedOnUtc = GetDateTime
                        };

                        _infraProjectInstanceRoleService.InsertInfraProjectInstanceRole(roleEntity);
                    }

                    // edit role
                    if (role.Flag == "Edit")
                    {
                        //Check if the infra Project Instance Role already exists
                        var exists = await _infraProjectInstanceRoleService.GetInfraProjectInstanceRoleByRoleName(SiteId, model.ProjectInstanceId, role.RoleName, role.Id);
                        if (exists != null)
                            continue;

                        roleEntity.RoleName = role.RoleName;
                        roleEntity.UpdatedById = LoggedUserId;
                        roleEntity.UpdatedOnUtc = GetDateTime;

                        _infraProjectInstanceRoleService.UpdateInfraProjectInstanceRole(roleEntity);
                    }

                    if (role.InfraProjectInstanceRoleUserList == null)
                        continue;

                    foreach (var user in role.InfraProjectInstanceRoleUserList)
                    {
                        if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Password))
                            continue;

                        InfraProjectInstanceRoleUsers userEntity = null;

                        // check exist
                        if (user.Flag == "Edit" || user.Flag == "Delete")
                        {
                            userEntity = await _infraProjectInstanceRoleUsersService
                                .GetInfraProjectInstanceRoleUsersById(user.Id);

                            if (userEntity == null)
                                continue;
                        }

                        // delete user
                        if (user.Flag == "Delete")
                        {
                            _infraProjectInstanceRoleUsersService.DeleteInfraProjectInstanceRoleUsers(userEntity);
                            continue;
                        }

                        // new user
                        if (user.Flag == "New")
                        {
                            //Check if the infra Project Instance Role User already exists
                            var userExists = await _infraProjectInstanceRoleUsersService.GetInfraProjectInstanceRoleUsersByUsername(model.ProjectInstanceId, user.UserName);
                            if (userExists != null)
                                continue;

                            userEntity = new InfraProjectInstanceRoleUsers
                            {
                                ProjectInstanceRoleId = roleEntity.Id,
                                UserName = user.UserName,
                                Password = user.Password,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime,
                                UpdatedById = LoggedUserId,
                                UpdatedOnUtc = GetDateTime
                            };

                            _infraProjectInstanceRoleUsersService.InsertInfraProjectInstanceRoleUsers(userEntity);
                        }

                        // edit user
                        if (user.Flag == "Edit")
                        {
                            //Check if the infra Project Instance Role User already exists
                            var userExists = await _infraProjectInstanceRoleUsersService.GetInfraProjectInstanceRoleUsersByUsername(model.ProjectInstanceId, user.UserName, user.Id);
                            if (userExists != null)
                                continue;

                            userEntity.UserName = user.UserName;
                            userEntity.Password = user.Password;
                            userEntity.UpdatedById = LoggedUserId;
                            userEntity.UpdatedOnUtc = GetDateTime;

                            _infraProjectInstanceRoleUsersService.UpdateInfraProjectInstanceRoleUsers(userEntity);
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + " :- " + ex.InnerException);
            }
        }
        #endregion

        #region AddorUpdateInstructions
        //created for update Instructions from list page
        [HttpPut("instructions/{id}")]
        public async Task<IActionResult> AddorUpdateInstructions(string id, SaveInfraProjectInstance model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the infra instance entity by its ID
                    var entity = await _infraProjectInstanceService.GetInfraProjectInstanceById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Infra instance found with the specified id."));

                    entity.Instructions = model.Instructions;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _infraProjectInstanceService.UpdateInfraProjectInstance(entity);

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

        #region DeleteInfraProjectInstance
        // Title: DeleteInfraProjectInstance
        // Description: This endpoint deletes a infra project instance based on the provided infra project instance ID. It first retrieves the infra project instance entity by ID, checks if it exists, and if so, deletes the infra project instance. If the infra project instance is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInfraProjectInstance(string id)
        {
            try
            {
                // Fetch the infra project instance entity by its ID
                var entity = await _infraProjectInstanceService.GetInfraProjectInstanceById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra project instance found with the specified id."));

                _infraProjectInstanceService.DeleteInfraProjectInstance(entity);

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
