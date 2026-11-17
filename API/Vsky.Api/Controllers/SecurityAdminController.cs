using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.SecurityAdmins;

namespace Vsky.Api.Controllers
{
    /// <summary>
    /// Security administration for the active site. Every action needs an admin role in that site.
    /// The rules and data access live in <see cref="ISecurityAdminService"/>. As in IssueController, this
    /// controller prepares the entity, runs the service's checks, and passes the entity back to be saved.
    /// </summary>
    [Route("security-admin")]
    [RequireSiteAdmin]
    public class SecurityAdminController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISecurityAdminService _securityAdminService;
        #endregion

        #region Services Initializations
        public SecurityAdminController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISecurityAdminService securityAdminService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _securityAdminService = securityAdminService;
        }
        #endregion

        #region GetOverview
        // Title: GetOverview
        // Description: This endpoint returns the enforcement settings and access counts for the active site.
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            try
            {
                return Ok(await _securityAdminService.GetOverview(_globalVariable.SiteId));
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetUsers
        // Title: GetUsers
        // Description: This endpoint returns a page of the active site's users.
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(string search = null, bool blockedOnly = false, int page = 1, int pageSize = 20)
        {
            try
            {
                return Ok(await _securityAdminService.GetUsers(_globalVariable.SiteId, search, blockedOnly, page, pageSize));
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetUserSites
        // Title: GetUserSites
        // Description: This endpoint returns the sites of a user in the active site.
        [HttpGet("users/{userId}/sites")]
        public async Task<IActionResult> GetUserSites(string userId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var sites = await _securityAdminService.GetUserSites(_globalVariable.SiteId, LoggedUserId, userId);
                if (sites == null)
                    return NotFound();

                return Ok(sites);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetUserRoles
        // Title: GetUserRoles
        // Description: This endpoint returns a user's roles in the active site.
        [HttpGet("users/{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            try
            {
                var roles = await _securityAdminService.GetUserRoles(_globalVariable.SiteId, userId);
                if (roles == null)
                    return NotFound();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region SetUserRoles
        // Title: SetUserRoles
        // Description: This endpoint replaces a user's roles in the active site.
        [HttpPut("users/{userId}/roles")]
        public async Task<IActionResult> SetUserRoles(string userId, SetUserRolesModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;

                    var rejected = await _securityAdminService.ValidateUserRoles(LoggedUserId, SiteId, userId, model.SiteRoleIds);
                    if (rejected != null)
                        return ToActionResult(rejected);

                    var entities = await _securityAdminService.GetUserRolesBySiteRoleIds(SiteId, userId, model.SiteRoleIds);
                    _securityAdminService.UpdateUserRoles(entities, LoggedUserId);

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetRoles
        // Title: GetRoles
        // Description: This endpoint returns the active site's roles.
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                return Ok(await _securityAdminService.GetRoles(_globalVariable.SiteId));
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetGroups
        // Title: GetGroups
        // Description: This endpoint returns the active site's security groups.
        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups()
        {
            try
            {
                return Ok(await _securityAdminService.GetGroups(_globalVariable.SiteId));
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region CreateGroup
        // Title: CreateGroup
        // Description: This endpoint creates a security group in the active site.
        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup(SecurityGroupModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Map the model to the entity
                    var entity = _mapper.Map<SecurityGroup>(model);
                    entity.Id = Guid.NewGuid().ToString();
                    entity.SiteId = _globalVariable.SiteId;
                    entity.Name = model.Name?.Trim();
                    entity.Description = model.Description?.Trim();
                    entity.Active = true;
                    entity.CreatedById = User.GetLoggedInUserId<string>();
                    entity.CreatedOnUtc = DateTime.UtcNow; 
                    entity.UpdatedById = User.GetLoggedInUserId<string>();
                    entity.UpdatedOnUtc = DateTime.UtcNow;

                    var rejected = await _securityAdminService.ValidateGroup(entity);
                    if (rejected != null)
                        return ToActionResult(rejected);

                    _securityAdminService.InsertGroup(entity);

                    return Ok(new { id = entity.Id });
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region UpdateGroup
        // Title: UpdateGroup
        // Description: This endpoint updates a security group in the active site.
        [HttpPut("groups/{groupId}")]
        public async Task<IActionResult> UpdateGroup(string groupId, SecurityGroupModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;

                    // Fetch the group, which must belong to the active site
                    var entity = await _securityAdminService.GetGroupById(SiteId, groupId);
                    if (entity == null)
                        return NotFound();

                    var rejected = await _securityAdminService.CheckCanChangeGroup(LoggedUserId, SiteId, entity.Id);
                    if (rejected != null)
                        return ToActionResult(rejected);

                    entity.Name = model.Name?.Trim();
                    entity.Description = model.Description?.Trim();
                    entity.Active = model.Active ?? entity.Active;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = DateTime.UtcNow;

                    rejected = await _securityAdminService.ValidateGroup(entity);
                    if (rejected != null)
                        return ToActionResult(rejected);

                    _securityAdminService.UpdateGroup(entity);

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region DeleteGroup
        // Title: DeleteGroup
        // Description: This endpoint soft deletes a security group in the active site.
        [HttpDelete("groups/{groupId}")]
        public async Task<IActionResult> DeleteGroup(string groupId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                // Fetch the group, which must belong to the active site
                var entity = await _securityAdminService.GetGroupById(SiteId, groupId);
                if (entity == null)
                    return NotFound();

                var rejected = await _securityAdminService.CheckCanChangeGroup(LoggedUserId, SiteId, entity.Id);
                if (rejected != null)
                    return ToActionResult(rejected);

                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = DateTime.UtcNow;
                _securityAdminService.DeleteGroup(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetGroupMembers
        // Title: GetGroupMembers
        // Description: This endpoint returns the members of a security group.
        [HttpGet("groups/{groupId}/members")]
        public async Task<IActionResult> GetGroupMembers(string groupId)
        {
            try
            {
                var members = await _securityAdminService.GetGroupMembers(_globalVariable.SiteId, groupId);
                if (members == null)
                    return NotFound();

                return Ok(members);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region AddGroupMembers
        // Title: AddGroupMembers
        // Description: This endpoint adds users to a security group.
        [HttpPost("groups/{groupId}/members")]
        public async Task<IActionResult> AddGroupMembers(string groupId, GroupMembersModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();

                    // Fetch the group, which must belong to the active site
                    var group = await _securityAdminService.GetGroupById(_globalVariable.SiteId, groupId);
                    if (group == null)
                        return NotFound();

                    var createdOn = DateTime.UtcNow;
                    var entities = (model.UserIds ?? new List<string>())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()
                        .Select(userId => new SecurityGroupMember
                        {
                            Id = Guid.NewGuid().ToString(),
                            SecurityGroupId = group.Id,
                            UserId = userId,
                            SiteId = group.SiteId,
                            CreatedById = LoggedUserId,
                            CreatedOnUtc = createdOn,
                            UpdatedById = LoggedUserId,
                            UpdatedOnUtc = createdOn
                        })
                        .ToList();

                    var rejected = await _securityAdminService.ValidateGroupMembers(entities);
                    if (rejected != null)
                        return ToActionResult(rejected);

                    _securityAdminService.InsertGroupMembers(entities);

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region RemoveGroupMember
        // Title: RemoveGroupMember
        // Description: This endpoint removes a user from a security group.
        [HttpDelete("groups/{groupId}/members/{userId}")]
        public async Task<IActionResult> RemoveGroupMember(string groupId, string userId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();

                // Fetch the group, which must belong to the active site
                var group = await _securityAdminService.GetGroupById(_globalVariable.SiteId, groupId);
                if (group == null)
                    return NotFound();

                var rejected = await _securityAdminService.CheckCanChangeUser(LoggedUserId, userId, group.SiteId);
                if (rejected != null)
                    return ToActionResult(rejected);

                var entity = await _securityAdminService.GetGroupMember(group.Id, userId);
                if (entity == null)
                    return NotFound();

                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = DateTime.UtcNow;
                _securityAdminService.DeleteGroupMember(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetGroupPermissions
        // Title: GetGroupPermissions
        // Description: This endpoint returns a security group's permission matrix.
        [HttpGet("groups/{groupId}/permissions")]
        public async Task<IActionResult> GetGroupPermissions(string groupId)
        {
            try
            {
                var matrix = await _securityAdminService.GetGroupPermissions(_globalVariable.SiteId, groupId);
                if (matrix == null)
                    return NotFound();

                return Ok(matrix);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region SetGroupPermission
        // Title: SetGroupPermission
        // Description: This endpoint sets a security group's permission for one menu.
        [HttpPut("groups/{groupId}/permissions")]
        public async Task<IActionResult> SetGroupPermission(string groupId, MenuPermissionModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Fetch the group, which must belong to the active site
                    var group = await _securityAdminService.GetGroupById(_globalVariable.SiteId, groupId);
                    if (group == null)
                        return NotFound();

                    // Map the model to the entity
                    var entity = _mapper.Map<SecurityGroupMenuPermission>(model);
                    entity.SecurityGroupId = group.Id;
                    entity.SiteId = group.SiteId;
                    entity.UpdatedById = User.GetLoggedInUserId<string>();
                    entity.UpdatedOnUtc = DateTime.UtcNow;

                    var rejected = await _securityAdminService.ValidateGroupPermission(entity);
                    if (rejected != null)
                        return ToActionResult(rejected);

                    await _securityAdminService.SaveGroupPermission(entity);

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetUserPermissions
        // Title: GetUserPermissions
        // Description: This endpoint returns a user's individual permission matrix.
        [HttpGet("users/{userId}/permissions")]
        public async Task<IActionResult> GetUserPermissions(string userId)
        {
            try
            {
                var matrix = await _securityAdminService.GetUserPermissions(_globalVariable.SiteId, userId);
                if (matrix == null)
                    return NotFound();

                return Ok(matrix);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region SetUserPermission
        // Title: SetUserPermission
        // Description: This endpoint sets a user's individual permission for one menu.
        [HttpPut("users/{userId}/permissions")]
        public async Task<IActionResult> SetUserPermission(string userId, MenuPermissionModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Map the model to the entity
                    var entity = _mapper.Map<UserMenuPermission>(model);
                    entity.UserId = userId;
                    entity.SiteId = _globalVariable.SiteId;
                    entity.UpdatedById = User.GetLoggedInUserId<string>();
                    entity.UpdatedOnUtc = DateTime.UtcNow;

                    var rejected = await _securityAdminService.ValidateUserPermission(entity);
                    if (rejected != null)
                        return ToActionResult(rejected);

                    await _securityAdminService.SaveUserPermission(entity);

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetEffectivePermissions
        // Title: GetEffectivePermissions
        // Description: This endpoint explains what a user can actually do in the active site.
        [HttpGet("effective/{userId}")]
        public async Task<IActionResult> GetEffectivePermissions(string userId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var effective = await _securityAdminService.GetEffectivePermissions(_globalVariable.SiteId, LoggedUserId, userId);
                if (effective == null)
                    return NotFound();

                return Ok(effective);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetAuditLog
        // Title: GetAuditLog
        // Description: This endpoint returns a page of the security audit log.
        [HttpGet("audit")]
        public async Task<IActionResult> GetAuditLog(
            string category = null,
            string search = null,
            string userId = null,
            DateTime? fromUtc = null,
            DateTime? toUtc = null,
            bool allSites = false,
            int page = 1,
            int pageSize = 20,
            [FromServices] AdminAccess adminAccess = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                if (category == SecurityAdminService.ErrorsCategory && !await adminAccess.IsSystemAdminAsync(LoggedUserId))
                    return AdminDenied.Result();

                return Ok(await _securityAdminService.GetAuditLog(
                    _globalVariable.SiteId,
                    LoggedUserId,
                    category,
                    search,
                    userId,
                    fromUtc,
                    toUtc,
                    allSites,
                    page,
                    pageSize));
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region Private Methods
        private IActionResult ToActionResult(SecurityAdminResult result)
        {
            return result.Status switch
            {
                SecurityAdminStatus.NotFound => NotFound(),
                SecurityAdminStatus.BadRequest => BadRequest(new BadRequestError(result.Message)),
                _ => StatusCode(StatusCodes.Status403Forbidden, new { code = "SECURITY_ADMIN_DENIED", message = result.Message })
            };
        }
        #endregion
    }
}
