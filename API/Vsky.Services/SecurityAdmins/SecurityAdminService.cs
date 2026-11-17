using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Authorization;
using LogLevel = Vsky.Models.LogLevel;

namespace Vsky.Services.SecurityAdmins
{
    /// <summary>
    /// Security administration for one site. Every ID is checked against that site, so one site's
    /// admin can't read or change another site. Admins can't change their own access, act on a
    /// higher-ranked user, or grant what they don't hold.
    /// </summary>
    public class SecurityAdminService : ISecurityAdminService
    {
        private const byte Inherit = 0;
        private const byte Allow = 1;
        private const byte Deny = 2;

        private const string PermissionChange = "Permission change";
        public const string ErrorsCategory = "errors";

        private static readonly Dictionary<string, string[]> AuditCategories = new()
        {
            ["denied"] = new[] { "Authorization denied", "Authentication failed" },
            ["would-deny"] = new[] { "Authorization audit: would deny" },
            ["activity"] = new[] { "User activity", "Record access", "Module access" },
            ["permission"] = new[] { PermissionChange, "Administrative change" },
            ["session"] = new[]
            {
                "Login succeeded", "Login failed", "Login blocked: account locked", "Microsoft login rejected",
                "Login denied: no site access", "Logout", "Sessions revoked",
                "Password reset requested", "Password reset completed", "Site switched"
            }
        };

        private static readonly string[] SessionMessages = AuditCategories["session"];

        #region Define Services
        private readonly IRepository<SecurityGroup> _securityGroupRepository;
        private readonly IRepository<SecurityGroupMember> _securityGroupMemberRepository;
        private readonly IRepository<SecurityGroupMenuPermission> _securityGroupMenuPermissionRepository;
        private readonly IRepository<UserMenuPermission> _userMenuPermissionRepository;
        private readonly IRepository<SitesRoles> _sitesRolesRepository;
        private readonly IRepository<SitesModulesMenusPermissions> _sitesModulesMenusPermissionsRepository;
        private readonly IRepository<PersonSitesMapping> _personSitesMappingRepository;
        private readonly IRepository<Site> _siteRepository;
        private readonly IRepository<Log> _logRepository;
        private readonly ApplicationDbContext _db;
        private readonly AdminAccess _adminAccess;
        private readonly IPermissionService _permissionService;
        private readonly ISiteAccessService _siteAccessService;
        private readonly IEnumerable<IPermissionSource> _permissionSources;
        private readonly ISecurityLog _securityLog;
        private readonly SecurityAuthorizationOptions _options;
        #endregion

        #region Services Initializations
        public SecurityAdminService(
            IRepository<SecurityGroup> securityGroupRepository,
            IRepository<SecurityGroupMember> securityGroupMemberRepository,
            IRepository<SecurityGroupMenuPermission> securityGroupMenuPermissionRepository,
            IRepository<UserMenuPermission> userMenuPermissionRepository,
            IRepository<SitesRoles> sitesRolesRepository,
            IRepository<SitesModulesMenusPermissions> sitesModulesMenusPermissionsRepository,
            IRepository<PersonSitesMapping> personSitesMappingRepository,
            IRepository<Site> siteRepository,
            IRepository<Log> logRepository,
            ApplicationDbContext db,
            AdminAccess adminAccess,
            IPermissionService permissionService,
            ISiteAccessService siteAccessService,
            IEnumerable<IPermissionSource> permissionSources,
            ISecurityLog securityLog,
            IOptions<SecurityAuthorizationOptions> options)
        {
            _securityGroupRepository = securityGroupRepository;
            _securityGroupMemberRepository = securityGroupMemberRepository;
            _securityGroupMenuPermissionRepository = securityGroupMenuPermissionRepository;
            _userMenuPermissionRepository = userMenuPermissionRepository;
            _sitesRolesRepository = sitesRolesRepository;
            _sitesModulesMenusPermissionsRepository = sitesModulesMenusPermissionsRepository;
            _personSitesMappingRepository = personSitesMappingRepository;
            _siteRepository = siteRepository;
            _logRepository = logRepository;
            _db = db;
            _adminAccess = adminAccess;
            _permissionService = permissionService;
            _siteAccessService = siteAccessService;
            _permissionSources = permissionSources;
            _securityLog = securityLog;
            _options = options.Value;
        }
        #endregion

        #region GetOverview
        // Title: GetOverview
        // Description: Returns the enforcement settings and the access counts for the site.
        public async Task<SecurityOverviewDto> GetOverview(string SiteId)
        {
            var users = SiteUsers(SiteId);
            var validRoleUsers = UsersWithValidRole(SiteId);

            return new SecurityOverviewDto
            {
                Mode = _options.Mode.ToString(),
                SiteGateMode = _options.SiteGateMode.ToString(),
                RecordScopeMode = _options.RecordScopeMode.ToString(),
                EnforceAllLinks = _options.EnforceAllLinks,
                LegacyShowOnlyAccess = _options.LegacyShowOnlyAccess,
                UserCount = await users.CountAsync(),
                BlockedUserCount = await users.CountAsync(x => !x.Active || !validRoleUsers.Contains(x.Id)),
                RoleCount = await _sitesRolesRepository.TableNoTracking.CountAsync(x => x.SiteId == SiteId && !x.Deleted),
                GroupCount = await _securityGroupRepository.TableNoTracking.CountAsync(x => x.SiteId == SiteId && !x.Deleted),
                DirectGrantCount = await _userMenuPermissionRepository.TableNoTracking.CountAsync(x => x.SiteId == SiteId && !x.Deleted),
                NotConfiguredRowCount = await _sitesModulesMenusPermissionsRepository.TableNoTracking.CountAsync(x =>
                    x.SiteId == SiteId && !x.Deleted && x.IsShowMenu && !x.IsView && !x.IsManage)
            };
        }
        #endregion

        #region GetUsers
        // Title: GetUsers
        // Description: Returns a page of the site's users with their roles, group and grant counts, and whether they can reach the site.
        public async Task<SecurityUserListDto> GetUsers(string SiteId, string search, bool blockedOnly, int page, int pageSize)
        {
            var validRoleUsers = UsersWithValidRole(SiteId);
            var query = SiteUsers(SiteId);

            if (blockedOnly)
                query = query.Where(x => !x.Active || !validRoleUsers.Contains(x.Id));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var text = search.Trim();
                query = query.Where(x => x.Id == text
                                         || x.UserName.Contains(text)
                                         || x.Email.Contains(text)
                                         || x.Person.FirstName.Contains(text)
                                         || x.Person.LastName.Contains(text));
            }

            var total = await query.CountAsync();
            var (skip, take) = PageOf(page, pageSize);

            var users = await query
                .OrderBy(x => x.UserName)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    x.Id,
                    x.UserName,
                    x.Email,
                    x.Active,
                    x.PersonId,
                    x.Person.FirstName,
                    x.Person.LastName
                })
                .ToListAsync();

            var userIds = users.Select(x => x.Id).ToList();
            var personIds = users.Select(x => x.PersonId).ToList();

            var roles = await RolesOfUsers(SiteId, userIds);

            var mappings = await _personSitesMappingRepository.TableNoTracking
                .Where(x => x.SiteId == SiteId && !x.Deleted && personIds.Contains(x.PersonId))
                .Select(x => new { x.PersonId, x.IsSharedUser, x.LastUsed })
                .ToListAsync();

            var groupCounts = await _securityGroupMemberRepository.TableNoTracking
                .Where(x => x.SiteId == SiteId && !x.Deleted && userIds.Contains(x.UserId))
                .GroupBy(x => x.UserId)
                .Select(x => new { UserId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var grantCounts = await _userMenuPermissionRepository.TableNoTracking
                .Where(x => x.SiteId == SiteId && !x.Deleted && userIds.Contains(x.UserId))
                .GroupBy(x => x.UserId)
                .Select(x => new { UserId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            return new SecurityUserListDto
            {
                Total = total,
                Items = users.Select(user =>
                {
                    var userRoles = roles.Where(x => x.UserId == user.Id).ToList();
                    var mapping = mappings.FirstOrDefault(x => x.PersonId == user.PersonId);

                    return new SecurityUserDto
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Name = $"{user.FirstName} {user.LastName}".Trim(),
                        Active = user.Active,
                        IsSharedUser = mapping?.IsSharedUser ?? false,
                        LastUsed = mapping?.LastUsed ?? false,
                        Roles = userRoles.Select(x => new SecurityUserRoleDto { SiteRoleId = x.SiteRoleId, Name = x.Name }).ToList(),
                        GroupCount = groupCounts.GetValueOrDefault(user.Id),
                        DirectGrantCount = grantCounts.GetValueOrDefault(user.Id),
                        CanAccessSite = user.Active && userRoles.Count > 0,
                        AccessIssue = !user.Active ? "User is inactive" : userRoles.Count == 0 ? "No valid role in this site" : null
                    };
                }).ToList()
            };
        }
        #endregion

        #region GetUserSites
        // Title: GetUserSites
        // Description: A System Super Admin sees every site the user is mapped to. A site admin sees only this one.
        public async Task<List<SecurityUserSiteDto>> GetUserSites(string SiteId, string LoggedUserId, string userId)
        {
            if (!await IsUserInSite(userId, SiteId))
                return null;

            var isSystemAdmin = await _adminAccess.IsSystemAdminAsync(LoggedUserId);
            var personId = await _db.Users.Where(x => x.Id == userId).Select(x => x.PersonId).FirstOrDefaultAsync();
            var accessible = await _siteAccessService.GetAccessibleSiteIdsAsync(userId);

            var sites = await (
                from mapping in _personSitesMappingRepository.TableNoTracking
                join site in _siteRepository.TableNoTracking on mapping.SiteId equals site.Id
                where mapping.PersonId == personId
                      && !mapping.Deleted
                      && !site.Deleted
                      && (isSystemAdmin || mapping.SiteId == SiteId)
                select new { site.Id, site.Name, mapping.IsSharedUser, mapping.LastUsed })
                .ToListAsync();

            return sites.Select(x => new SecurityUserSiteDto
            {
                SiteId = x.Id,
                Name = x.Name,
                IsSharedUser = x.IsSharedUser,
                LastUsed = x.LastUsed,
                CanAccess = accessible.Contains(x.Id)
            }).ToList();
        }
        #endregion

        #region GetUserRoles
        // Title: GetUserRoles
        // Description: Returns the user's valid roles in the site.
        public async Task<List<SecurityUserRoleDto>> GetUserRoles(string SiteId, string userId)
        {
            if (!await IsUserInSite(userId, SiteId))
                return null;

            return (await RolesOfUsers(SiteId, new List<string> { userId }))
                .Select(x => new SecurityUserRoleDto { SiteRoleId = x.SiteRoleId, Name = x.Name })
                .ToList();
        }
        #endregion

        #region SetUserRoles
        // Title: ValidateUserRoles
        // Description: Checks a change to the user's roles in the active site only. Removing someone from a site
        // stays with site sharing, so at least one role is required here.
        public async Task<SecurityAdminResult> ValidateUserRoles(string LoggedUserId, string SiteId, string userId, List<string> siteRoleIds)
        {
            var denied = await CheckCanChangeUser(LoggedUserId, userId, SiteId);
            if (denied != null)
                return denied;

            var requestedIds = (siteRoleIds ?? new List<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            if (requestedIds.Count == 0)
                return SecurityAdminResult.BadRequest("Select at least one role.");

            var error = await _adminAccess.ValidateGrantableSiteRolesAsync(LoggedUserId, SiteId, requestedIds);
            return error == null ? null : SecurityAdminResult.BadRequest(error);
        }

        // Title: GetUserRolesBySiteRoleIds
        // Description: Builds the user role entities for the given site roles.
        public async Task<List<ApplicationUserRole>> GetUserRolesBySiteRoleIds(string SiteId, string userId, List<string> siteRoleIds)
        {
            var roleIds = await _sitesRolesRepository.TableNoTracking
                .Where(x => siteRoleIds.Contains(x.Id) && x.SiteId == SiteId && !x.Deleted)
                .Select(x => x.RoleId)
                .ToListAsync();

            return roleIds.Select(roleId => new ApplicationUserRole { UserId = userId, RoleId = roleId, SiteId = SiteId }).ToList();
        }

        // Title: UpdateUserRoles
        // Description: Replaces the user's roles in the site with the given entities, in one save. The entities
        // have no "changed by" column, so the audit log needs the logged-in user passed separately.
        public void UpdateUserRoles(List<ApplicationUserRole> entities, string LoggedUserId)
        {
            var userId = entities[0].UserId;
            var siteId = entities[0].SiteId;
            var wantedRoleIds = entities.Select(x => x.RoleId).ToList();

            var existing = _db.UserRoles.Where(x => x.UserId == userId && x.SiteId == siteId).ToList();
            var before = existing.Select(x => x.RoleId).ToList();

            _db.UserRoles.RemoveRange(existing.Where(x => !wantedRoleIds.Contains(x.RoleId)));
            _db.UserRoles.AddRange(entities.Where(x => !before.Contains(x.RoleId)));
            _db.SaveChanges();

            LogChange(siteId, LoggedUserId, "user-roles", "user", userId, null, before, wantedRoleIds);
        }
        #endregion

        #region GetRoles
        // Title: GetRoles
        // Description: Returns the site's roles with how many users and menus each one has.
        public async Task<List<SecurityRoleDto>> GetRoles(string SiteId)
        {
            var roles = await (
                from siteRole in _sitesRolesRepository.TableNoTracking
                join role in _db.Roles on siteRole.RoleId equals role.Id
                where siteRole.SiteId == SiteId && !siteRole.Deleted && !role.Deleted
                orderby role.Name
                select new { siteRole.Id, siteRole.RoleId, role.Name, role.NormalizedName })
                .ToListAsync();

            var siteUsers = SiteUsers(SiteId).Select(x => x.Id);
            var userCounts = await _db.UserRoles
                .Where(x => x.SiteId == SiteId && siteUsers.Contains(x.UserId))
                .GroupBy(x => x.RoleId)
                .Select(x => new { RoleId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.RoleId, x => x.Count);

            var rows = await _sitesModulesMenusPermissionsRepository.TableNoTracking
                .Where(x => x.SiteId == SiteId && !x.Deleted)
                .GroupBy(x => x.SiteRoleId)
                .Select(x => new
                {
                    SiteRoleId = x.Key,
                    Granted = x.Count(p => p.IsShowMenu || p.IsView || p.IsManage),
                    NotConfigured = x.Count(p => p.IsShowMenu && !p.IsView && !p.IsManage)
                })
                .ToDictionaryAsync(x => x.SiteRoleId);

            return roles.Select(role => new SecurityRoleDto
            {
                SiteRoleId = role.Id,
                Name = role.Name,
                NormalizedName = role.NormalizedName,
                UserCount = userCounts.GetValueOrDefault(role.RoleId),
                MenuCount = rows.GetValueOrDefault(role.Id)?.Granted ?? 0,
                NotConfiguredCount = rows.GetValueOrDefault(role.Id)?.NotConfigured ?? 0
            }).ToList();
        }
        #endregion

        #region GetGroups
        // Title: GetGroups
        // Description: Returns the site's groups with their member and permission counts.
        public async Task<List<SecurityGroupDto>> GetGroups(string SiteId)
        {
            var members = _securityGroupMemberRepository.TableNoTracking;
            var permissions = _securityGroupMenuPermissionRepository.TableNoTracking;

            return await _securityGroupRepository.TableNoTracking
                .Where(x => x.SiteId == SiteId && !x.Deleted)
                .OrderBy(x => x.Name)
                .Select(x => new SecurityGroupDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Active = x.Active,
                    MemberCount = members.Count(m => m.SecurityGroupId == x.Id && !m.Deleted),
                    PermissionCount = permissions.Count(p => p.SecurityGroupId == x.Id && !p.Deleted)
                })
                .ToListAsync();
        }
        #endregion

        #region GetGroupById
        // Title: GetGroupById
        // Description: Returns the group only if it belongs to the site, so one site can't reach another site's group.
        public async Task<SecurityGroup> GetGroupById(string SiteId, string groupId)
        {
            return await _securityGroupRepository.TableNoTracking
                .FirstOrDefaultAsync(x => x.Id == groupId && x.SiteId == SiteId && !x.Deleted);
        }
        #endregion

        #region CreateGroup
        // Title: ValidateGroup
        // Description: Checks the group's name, which must be unique within the site.
        public async Task<SecurityAdminResult> ValidateGroup(SecurityGroup entity)
        {
            if (string.IsNullOrEmpty(entity.Name) || entity.Name.Length > 200)
                return SecurityAdminResult.BadRequest("Enter a group name of up to 200 characters.");

            if (await _securityGroupRepository.TableNoTracking.AnyAsync(x => x.SiteId == entity.SiteId && !x.Deleted && x.Id != entity.Id && x.Name == entity.Name))
                return SecurityAdminResult.BadRequest("A group with this name already exists in this site.");

            return null;
        }

        // Title: InsertGroup
        // Description: Inserts a new SecurityGroup entity and records it in the audit log.
        public void InsertGroup(SecurityGroup entity)
        {
            _securityGroupRepository.Insert(entity);

            LogChange(entity.SiteId, entity.CreatedById, "group-created", "group", entity.Id, null, null, new { entity.Name, entity.Description });
        }
        #endregion

        #region UpdateGroup
        // Title: UpdateGroup
        // Description: Updates the SecurityGroup entity and records the old and new values in the audit log.
        public void UpdateGroup(SecurityGroup entity)
        {
            var before = _securityGroupRepository.TableNoTracking
                .Where(x => x.Id == entity.Id)
                .Select(x => new { x.Name, x.Description, x.Active })
                .FirstOrDefault();

            _securityGroupRepository.Update(entity);

            LogChange(entity.SiteId, entity.UpdatedById, "group-updated", "group", entity.Id, null, before, new { entity.Name, entity.Description, entity.Active });
        }
        #endregion

        #region DeleteGroup
        // Title: DeleteGroup
        // Description: Soft delete, so the audit trail can still name the group and nothing is lost. The members and
        // permissions are tracked, so the repository's single save covers them too and none can be left behind.
        public void DeleteGroup(SecurityGroup entity)
        {
            entity.Deleted = true;

            var members = _securityGroupMemberRepository.Table.Where(x => x.SecurityGroupId == entity.Id && !x.Deleted).ToList();
            foreach (var member in members)
            {
                member.Deleted = true;
                member.UpdatedById = entity.UpdatedById;
                member.UpdatedOnUtc = entity.UpdatedOnUtc;
            }

            var permissions = _securityGroupMenuPermissionRepository.Table.Where(x => x.SecurityGroupId == entity.Id && !x.Deleted).ToList();
            foreach (var permission in permissions)
            {
                permission.Deleted = true;
                permission.UpdatedById = entity.UpdatedById;
                permission.UpdatedOnUtc = entity.UpdatedOnUtc;
            }

            _securityGroupRepository.Update(entity);

            LogChange(entity.SiteId, entity.UpdatedById, "group-deleted", "group", entity.Id, null, new { entity.Name, memberCount = members.Count, permissionCount = permissions.Count }, null);
        }
        #endregion

        #region GetGroupMembers
        // Title: GetGroupMembers
        // Description: Returns the group's members and whether each one can reach the site.
        public async Task<List<SecurityGroupMemberDto>> GetGroupMembers(string SiteId, string groupId)
        {
            var securityGroup = await GetGroupById(SiteId, groupId);
            if (securityGroup == null)
                return null;

            var validRoleUsers = UsersWithValidRole(securityGroup.SiteId);

            return await (
                from member in _securityGroupMemberRepository.TableNoTracking
                join user in _db.Users on member.UserId equals user.Id
                where member.SecurityGroupId == securityGroup.Id && !member.Deleted
                orderby user.UserName
                select new SecurityGroupMemberDto
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Name = user.Person.FirstName + " " + user.Person.LastName,
                    // A group never replaces the site check, so a member with no role gets nothing.
                    CanAccessSite = user.Active && !user.Deleted && validRoleUsers.Contains(user.Id)
                }).ToListAsync();
        }
        #endregion

        #region AddGroupMembers
        // Title: ValidateGroupMembers
        // Description: Checks new members of one group. Adding someone gives them the group's rights, so the caller must hold those rights too.
        public async Task<SecurityAdminResult> ValidateGroupMembers(List<SecurityGroupMember> entities)
        {
            if (entities.Count == 0)
                return SecurityAdminResult.BadRequest("Select at least one user.");

            var groupId = entities[0].SecurityGroupId;
            var siteId = entities[0].SiteId;
            var callerId = entities[0].CreatedById;

            foreach (var entity in entities)
            {
                var denied = await CheckCanChangeUser(callerId, entity.UserId, siteId);
                if (denied != null)
                    return denied;
            }

            var groupAllows = await _securityGroupMenuPermissionRepository.TableNoTracking
                .Where(x => x.SecurityGroupId == groupId && !x.Deleted && (x.ViewAccess == Allow || x.ManageAccess == Allow))
                .ToListAsync();
            foreach (var permission in groupAllows)
            {
                var error = await CheckGrantable(callerId, siteId, permission.SiteModuleMenuId, permission.ViewAccess, permission.ManageAccess);
                if (error != null)
                    return SecurityAdminResult.Forbidden(error);
            }

            return null;
        }

        // Title: InsertGroupMembers
        // Description: Inserts the SecurityGroupMember entities for one group, skipping users who are already members.
        public void InsertGroupMembers(List<SecurityGroupMember> entities)
        {
            var first = entities[0];
            var userIds = entities.Select(x => x.UserId).ToList();

            var existing = _securityGroupMemberRepository.TableNoTracking
                .Where(x => x.SecurityGroupId == first.SecurityGroupId && !x.Deleted && userIds.Contains(x.UserId))
                .Select(x => x.UserId)
                .ToList();

            var added = entities.Where(x => !existing.Contains(x.UserId)).ToList();
            _securityGroupMemberRepository.Insert(added);

            LogChange(first.SiteId, first.CreatedById, "group-members-added", "group", first.SecurityGroupId, null, null, added.Select(x => x.UserId).ToList());
        }
        #endregion

        #region RemoveGroupMember
        // Title: GetGroupMember
        // Description: Returns the user's active membership of the group, or null.
        public async Task<SecurityGroupMember> GetGroupMember(string groupId, string userId)
        {
            return await _securityGroupMemberRepository.TableNoTracking
                .FirstOrDefaultAsync(x => x.SecurityGroupId == groupId && x.UserId == userId && !x.Deleted);
        }

        // Title: DeleteGroupMember
        // Description: Marks the SecurityGroupMember entity as deleted and records it in the audit log.
        public void DeleteGroupMember(SecurityGroupMember entity)
        {
            entity.Deleted = true;

            _securityGroupMemberRepository.Update(entity);

            LogChange(entity.SiteId, entity.UpdatedById, "group-member-removed", "group", entity.SecurityGroupId, null, entity.UserId, null);
        }
        #endregion

        #region GetGroupPermissions
        // Title: GetGroupPermissions
        // Description: Returns the site's menus with this group's value for each one.
        public async Task<List<SecurityMenuPermissionDto>> GetGroupPermissions(string SiteId, string groupId)
        {
            var group = await GetGroupById(SiteId, groupId);
            if (group == null)
                return null;

            var rows = await _securityGroupMenuPermissionRepository.TableNoTracking
                .Where(x => x.SecurityGroupId == group.Id && !x.Deleted)
                .Select(x => new MatrixValue(x.SiteModuleMenuId, x.ShowInMenu, x.ViewAccess, x.ManageAccess, null, null))
                .ToListAsync();

            return await BuildMatrix(SiteId, rows);
        }
        #endregion

        #region SetGroupPermission
        // Title: ValidateGroupPermission
        // Description: Checks that the caller may change the group, and may grant this value on this menu.
        public async Task<SecurityAdminResult> ValidateGroupPermission(SecurityGroupMenuPermission entity)
        {
            var denied = await CheckCanChangeGroup(entity.UpdatedById, entity.SiteId, entity.SecurityGroupId);
            if (denied != null)
                return denied;

            var error = ValidateValues(entity.SiteModuleMenuId, entity.ViewAccess, entity.ManageAccess);
            if (error != null)
                return SecurityAdminResult.BadRequest(error);

            var menu = await FindMenu(entity.SiteId, entity.SiteModuleMenuId);
            if (menu == null)
                return SecurityAdminResult.NotFound();

            error = await CheckGrantable(entity.UpdatedById, entity.SiteId, menu.Id, entity.ViewAccess, entity.ManageAccess);
            return error == null ? null : SecurityAdminResult.Forbidden(error);
        }

        // Title: SaveGroupPermission
        // Description: Inserts the entity, or updates the group's existing row for that menu. A row left with nothing in it is soft deleted.
        public async Task SaveGroupPermission(SecurityGroupMenuPermission entity)
        {
            var menu = await FindMenu(entity.SiteId, entity.SiteModuleMenuId);

            var row = await _securityGroupMenuPermissionRepository.Table
                .FirstOrDefaultAsync(x => x.SecurityGroupId == entity.SecurityGroupId && x.SiteModuleMenuId == entity.SiteModuleMenuId && !x.Deleted);

            var before = row == null ? null : new { row.ShowInMenu, row.ViewAccess, row.ManageAccess };
            var isEmpty = !entity.ShowInMenu && entity.ViewAccess == Inherit && entity.ManageAccess == Inherit;

            if (row == null && !isEmpty)
            {
                entity.Id = Guid.NewGuid().ToString();
                entity.CreatedById = entity.UpdatedById;
                entity.CreatedOnUtc = entity.UpdatedOnUtc.Value;
                _securityGroupMenuPermissionRepository.Insert(entity);
            }
            else if (row != null)
            {
                row.ShowInMenu = entity.ShowInMenu;
                row.ViewAccess = entity.ViewAccess;
                row.ManageAccess = entity.ManageAccess;
                row.Deleted = isEmpty;
                row.UpdatedById = entity.UpdatedById;
                row.UpdatedOnUtc = entity.UpdatedOnUtc;
                _securityGroupMenuPermissionRepository.Update(row);
            }

            LogChange(entity.SiteId, entity.UpdatedById, "group-permission", "group", entity.SecurityGroupId, menu.Link, before, new { entity.ShowInMenu, entity.ViewAccess, entity.ManageAccess });
        }
        #endregion

        #region GetUserPermissions
        // Title: GetUserPermissions
        // Description: Returns the site's menus with this user's individual value for each one.
        public async Task<List<SecurityMenuPermissionDto>> GetUserPermissions(string SiteId, string userId)
        {
            if (!await IsUserInSite(userId, SiteId))
                return null;

            var rows = await _userMenuPermissionRepository.TableNoTracking
                .Where(x => x.UserId == userId && x.SiteId == SiteId && !x.Deleted)
                .Select(x => new MatrixValue(x.SiteModuleMenuId, x.ShowInMenu, x.ViewAccess, x.ManageAccess, x.Reason, x.ExpiresOnUtc))
                .ToListAsync();

            return await BuildMatrix(SiteId, rows);
        }
        #endregion

        #region SetUserPermission
        // Title: ValidateUserPermission
        // Description: Checks that the caller may change the user and grant this value. An individual permission needs a reason.
        public async Task<SecurityAdminResult> ValidateUserPermission(UserMenuPermission entity)
        {
            var denied = await CheckCanChangeUser(entity.UpdatedById, entity.UserId, entity.SiteId);
            if (denied != null)
                return denied;

            var error = ValidateValues(entity.SiteModuleMenuId, entity.ViewAccess, entity.ManageAccess);
            if (error != null)
                return SecurityAdminResult.BadRequest(error);

            var isEmpty = !entity.ShowInMenu && entity.ViewAccess == Inherit && entity.ManageAccess == Inherit;

            if (!isEmpty && string.IsNullOrWhiteSpace(entity.Reason))
                return SecurityAdminResult.BadRequest("Enter a reason for this individual permission.");
            if (entity.Reason?.Length > 500)
                return SecurityAdminResult.BadRequest("The reason can be up to 500 characters.");
            if (entity.ExpiresOnUtc.HasValue && entity.ExpiresOnUtc.Value <= DateTime.UtcNow)
                return SecurityAdminResult.BadRequest("The end date must be in the future.");

            var menu = await FindMenu(entity.SiteId, entity.SiteModuleMenuId);
            if (menu == null)
                return SecurityAdminResult.NotFound();

            error = await CheckGrantable(entity.UpdatedById, entity.SiteId, menu.Id, entity.ViewAccess, entity.ManageAccess);
            return error == null ? null : SecurityAdminResult.Forbidden(error);
        }

        // Title: SaveUserPermission
        // Description: Inserts the entity, or updates the user's existing row for that menu. A row left with nothing in it is soft deleted.
        public async Task SaveUserPermission(UserMenuPermission entity)
        {
            var menu = await FindMenu(entity.SiteId, entity.SiteModuleMenuId);

            var row = await _userMenuPermissionRepository.Table
                .FirstOrDefaultAsync(x => x.UserId == entity.UserId && x.SiteModuleMenuId == entity.SiteModuleMenuId && !x.Deleted);

            var before = row == null ? null : new { row.ShowInMenu, row.ViewAccess, row.ManageAccess, row.Reason, row.ExpiresOnUtc };
            var isEmpty = !entity.ShowInMenu && entity.ViewAccess == Inherit && entity.ManageAccess == Inherit;
            var after = new { entity.ShowInMenu, entity.ViewAccess, entity.ManageAccess, entity.Reason, entity.ExpiresOnUtc };

            if (row == null && !isEmpty)
            {
                entity.Id = Guid.NewGuid().ToString();
                entity.Reason = entity.Reason?.Trim();
                entity.CreatedById = entity.UpdatedById;
                entity.CreatedOnUtc = entity.UpdatedOnUtc.Value;
                _userMenuPermissionRepository.Insert(entity);
            }
            else if (row != null)
            {
                row.ShowInMenu = entity.ShowInMenu;
                row.ViewAccess = entity.ViewAccess;
                row.ManageAccess = entity.ManageAccess;
                row.Reason = entity.Reason?.Trim();
                row.ExpiresOnUtc = entity.ExpiresOnUtc;
                row.Deleted = isEmpty;
                row.UpdatedById = entity.UpdatedById;
                row.UpdatedOnUtc = entity.UpdatedOnUtc;
                _userMenuPermissionRepository.Update(row);
            }

            LogChange(entity.SiteId, entity.UpdatedById, "user-permission", "user", entity.UserId, menu.Link, before, after);
        }
        #endregion

        #region GetEffectivePermissions
        // Title: GetEffectivePermissions
        // Description: Reads the sources directly rather than the cache, and uses the same evaluator as the
        // enforcement filter, so this screen can't disagree with what the API decides.
        public async Task<EffectivePermissionsDto> GetEffectivePermissions(string SiteId, string LoggedUserId, string userId)
        {
            if (!await IsUserInSite(userId, SiteId))
                return null;

            var user = await _db.Users.AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new { x.UserName, x.Active })
                .FirstOrDefaultAsync();

            var isSiteMember = await _siteAccessService.IsSiteMemberAsync(userId, SiteId);

            var grants = new List<PermissionGrant>();
            foreach (var source in _permissionSources)
                grants.AddRange(await source.GetGrantsAsync(userId, SiteId));

            var result = PermissionEvaluator.Evaluate(grants);

            var roleNames = await (
                from siteRole in _sitesRolesRepository.TableNoTracking
                join role in _db.Roles on siteRole.RoleId equals role.Id
                where siteRole.SiteId == SiteId
                select new { siteRole.Id, role.Name }).ToDictionaryAsync(x => x.Id, x => x.Name);

            var groupNames = await _securityGroupRepository.TableNoTracking
                .Where(x => x.SiteId == SiteId)
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var grantsByLink = grants
                .Where(x => MenuLink.Normalize(x.Link) != null)
                .ToLookup(x => MenuLink.Normalize(x.Link));

            var menus = (await SiteMenus(SiteId))
                .GroupBy(x => MenuLink.Normalize(x.Link))
                .Select(group =>
                {
                    var menu = group.First();
                    var linkGrants = grantsByLink[group.Key].ToList();
                    result.Menus.TryGetValue(group.Key, out var access);

                    return new EffectiveMenuDto
                    {
                        SiteModuleMenuId = menu.Id,
                        Module = menu.Module,
                        Menu = menu.DisplayName,
                        Link = group.Key,
                        // A failed site check overrides every grant, as it does in the filter.
                        Show = isSiteMember && (access?.Show ?? false),
                        View = isSiteMember && (access?.View ?? false),
                        Manage = isSiteMember && (access?.Manage ?? false),
                        ViewReason = Explain(linkGrants, x => x.View, roleNames, groupNames, isSiteMember),
                        ManageReason = Explain(linkGrants, x => x.Manage, roleNames, groupNames, isSiteMember),
                        Sources = linkGrants.Select(grant => new EffectiveSourceDto
                        {
                            Kind = grant.Kind.ToString(),
                            Name = SourceName(grant, roleNames, groupNames),
                            Show = grant.Show,
                            View = grant.View.ToString(),
                            Manage = grant.Manage.ToString()
                        }).ToList()
                    };
                })
                .OrderBy(x => x.Module)
                .ThenBy(x => x.Menu)
                .ToList();

            var callerSnapshot = await _permissionService.GetSnapshotAsync(LoggedUserId, SiteId);

            return new EffectivePermissionsDto
            {
                UserId = userId,
                UserName = user?.UserName,
                SiteId = SiteId,
                IsSiteMember = isSiteMember,
                SiteIssue = isSiteMember ? null : user?.Active == false ? "User is inactive" : "No valid role in this site, so nothing below applies",
                Mode = _options.Mode.ToString(),
                ViewerIsSystemAdmin = callerSnapshot.IsSystemAdmin,
                Menus = menus
            };
        }
        #endregion

        #region GetAuditLog
        // Title: GetAuditLog
        // Description: A site admin sees only rows for their site. The log has no site column, so that comes from
        // the JSON the security writers store, plus sign-in events of this site's users.
        // A System Super Admin can also see security rows across all sites.
        public async Task<SecurityAuditListDto> GetAuditLog(
            string SiteId,
            string LoggedUserId,
            string category,
            string search,
            string userId,
            DateTime? fromUtc,
            DateTime? toUtc,
            bool allSites,
            int page,
            int pageSize)
        {
            var isSystemAdmin = await _adminAccess.IsSystemAdminAsync(LoggedUserId);

            // Stack traces can reveal internals, so only a System Super Admin can read error rows.
            var errorsOnly = category == ErrorsCategory;
            if (errorsOnly && !isSystemAdmin)
                return new SecurityAuditListDto { Total = 0, Items = new List<SecurityAuditLogDto>(), CanSeeAllSites = false };

            var messages = !string.IsNullOrEmpty(category) && AuditCategories.TryGetValue(category, out var selected)
                ? selected
                : AuditCategories.Values.SelectMany(x => x).ToArray();

            var query = errorsOnly
                ? _logRepository.TableNoTracking.Where(x => x.LogLevelId >= (int)LogLevel.Error && x.ShortMessage != "User activity") // The activity row for a failed request isn't a second error.
                : _logRepository.TableNoTracking.Where(x => messages.Contains(x.ShortMessage));

            if (!(isSystemAdmin && allSites))
            {
                var siteMarker = $"\"siteId\":\"{SiteId}\"";
                var siteUserIds = SiteUsers(SiteId).Select(x => x.Id);

                query = query.Where(x =>
                    x.FullMessage.Contains(siteMarker)
                    || (x.ShortMessage == "Site switched" && x.FullMessage == SiteId)
                    || (SessionMessages.Contains(x.ShortMessage) && x.ShortMessage != "Site switched" && siteUserIds.Contains(x.UserId)));
            }

            if (!string.IsNullOrWhiteSpace(userId))
                query = query.Where(x => x.UserId == userId);
            if (fromUtc.HasValue)
                query = query.Where(x => x.CreatedOnUtc >= fromUtc.Value);
            if (toUtc.HasValue)
                query = query.Where(x => x.CreatedOnUtc <= toUtc.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var text = search.Trim();
                query = query.Where(x => x.Id == text || x.FullMessage.Contains(text) || x.PageUrl.Contains(text) || x.User.UserName.Contains(text));
            }

            var total = await query.CountAsync();
            var (skip, take) = PageOf(page, pageSize);

            var items = await query
                .OrderByDescending(x => x.CreatedOnUtc)
                .Skip(skip)
                .Take(take)
                .Select(x => new SecurityAuditLogDto
                {
                    Id = x.Id,
                    CreatedOnUtc = x.CreatedOnUtc,
                    Message = x.ShortMessage,
                    Details = x.FullMessage,
                    UserId = x.UserId,
                    UserName = x.User.UserName,
                    IpAddress = x.IpAddress,
                    PageUrl = x.PageUrl
                })
                .ToListAsync();

            return new SecurityAuditListDto { Total = total, Items = items, CanSeeAllSites = isSystemAdmin };
        }
        #endregion

        #region Access rules
        // Title: CheckCanChangeUser
        // Description: The caller can't change their own access or that of a higher-ranked user.
        public async Task<SecurityAdminResult> CheckCanChangeUser(string LoggedUserId, string userId, string SiteId)
        {
            if (!await IsUserInSite(userId, SiteId))
                return SecurityAdminResult.NotFound();

            if (string.Equals(userId, LoggedUserId, StringComparison.OrdinalIgnoreCase))
                return SecurityAdminResult.Forbidden("You can't change your own access. Ask another administrator.");

            if (!await _adminAccess.CanManageUserAsync(LoggedUserId, userId, SiteId))
                return SecurityAdminResult.Forbidden("You can't change the access of a user with a higher role than yours.");

            return null;
        }

        // Title: CheckCanChangeGroup
        // Description: A group change affects every member, so the same rules apply to each of them.
        public async Task<SecurityAdminResult> CheckCanChangeGroup(string LoggedUserId, string SiteId, string groupId)
        {
            var memberIds = await _securityGroupMemberRepository.TableNoTracking
                .Where(x => x.SecurityGroupId == groupId && !x.Deleted)
                .Select(x => x.UserId)
                .ToListAsync();

            if (memberIds.Contains(LoggedUserId, StringComparer.OrdinalIgnoreCase))
                return SecurityAdminResult.Forbidden("You can't change a group you belong to. Ask another administrator.");

            foreach (var memberId in memberIds)
            {
                if (!await _adminAccess.CanManageUserAsync(LoggedUserId, memberId, SiteId))
                    return SecurityAdminResult.Forbidden("This group has a member with a higher role than yours.");
            }

            return null;
        }
        #endregion

        #region Private Methods
        private static string Explain(
            List<PermissionGrant> grants,
            Func<PermissionGrant, GrantValue> flag,
            IReadOnlyDictionary<string, string> roleNames,
            IReadOnlyDictionary<string, string> groupNames,
            bool isSiteMember)
        {
            if (!isSiteMember)
                return "Denied: no access to this site";

            string Names(PermissionSourceKind kind, GrantValue value) => string.Join(", ", grants
                .Where(x => x.Kind == kind && flag(x) == value)
                .Select(x => SourceName(x, roleNames, groupNames))
                .Distinct());

            var hasRoleConflict = grants
                .Where(x => x.Kind == PermissionSourceKind.Role)
                .GroupBy(x => new { x.SourceId, x.MenuId })
                .Any(rows => rows.Select(flag).Distinct().Count() > 1);

            if (hasRoleConflict)
                return "Denied: conflicting duplicate role rows";

            if (grants.Any(x => x.Kind == PermissionSourceKind.User && flag(x) == GrantValue.Deny))
                return "Denied: individual Deny";
            if (grants.Any(x => x.Kind == PermissionSourceKind.User && flag(x) == GrantValue.Allow))
                return "Allowed: individual Allow";

            var groupDeny = Names(PermissionSourceKind.Group, GrantValue.Deny);
            if (groupDeny.Length > 0)
                return $"Denied: group {groupDeny}";

            var groupAllow = Names(PermissionSourceKind.Group, GrantValue.Allow);
            var roleAllow = Names(PermissionSourceKind.Role, GrantValue.Allow);
            if (groupAllow.Length > 0 && roleAllow.Length > 0)
                return $"Allowed: role {roleAllow}; group {groupAllow}";
            if (groupAllow.Length > 0)
                return $"Allowed: group {groupAllow}";
            if (roleAllow.Length > 0)
                return $"Allowed: role {roleAllow}";

            return "Not granted";
        }

        private static string SourceName(PermissionGrant grant, IReadOnlyDictionary<string, string> roleNames, IReadOnlyDictionary<string, string> groupNames)
        {
            return grant.Kind switch
            {
                PermissionSourceKind.Role => roleNames.GetValueOrDefault(grant.SourceId) ?? "Role",
                PermissionSourceKind.Group => groupNames.GetValueOrDefault(grant.SourceId) ?? "Group",
                _ => "Individual"
            };
        }

        private IQueryable<ApplicationUser> SiteUsers(string siteId)
        {
            var mappedPersons = _personSitesMappingRepository.TableNoTracking
                .Where(x => x.SiteId == siteId && !x.Deleted)
                .Select(x => x.PersonId);

            return _db.Users.AsNoTracking().Where(x => !x.Deleted && mappedPersons.Contains(x.PersonId));
        }

        private IQueryable<string> UsersWithValidRole(string siteId)
        {
            return from userRole in _db.UserRoles
                   join role in _db.Roles on userRole.RoleId equals role.Id
                   join siteRole in _sitesRolesRepository.TableNoTracking on userRole.RoleId equals siteRole.RoleId
                   where userRole.SiteId == siteId && !role.Deleted && siteRole.SiteId == siteId && !siteRole.Deleted
                   select userRole.UserId;
        }

        private sealed record UserRoleRow(string UserId, string SiteRoleId, string Name);

        private async Task<List<UserRoleRow>> RolesOfUsers(string siteId, List<string> userIds)
        {
            return await (
                from userRole in _db.UserRoles
                join role in _db.Roles on userRole.RoleId equals role.Id
                join siteRole in _sitesRolesRepository.TableNoTracking on userRole.RoleId equals siteRole.RoleId
                where userRole.SiteId == siteId
                      && userIds.Contains(userRole.UserId)
                      && !role.Deleted
                      && siteRole.SiteId == siteId
                      && !siteRole.Deleted
                select new UserRoleRow(userRole.UserId, siteRole.Id, role.Name))
                .ToListAsync();
        }

        private Task<bool> IsUserInSite(string userId, string siteId)
        {
            return SiteUsers(siteId).AnyAsync(x => x.Id == userId);
        }

        private sealed record SiteMenuRow(string Id, string Module, string DisplayName, string Link);

        // External links aren't pages in this application, so no permission can apply to them.
        private async Task<List<SiteMenuRow>> SiteMenus(string siteId)
        {
            var rows = await ValidSiteMenus.Query(_db, siteId, _options.EnforceSiteLevelActiveFlags)
                .OrderBy(x => x.ModulesMenus.Modules.Name)
                .ThenBy(x => x.ModulesMenus.DisplayName)
                .Select(x => new SiteMenuRow(x.Id, x.ModulesMenus.Modules.Name, x.ModulesMenus.DisplayName, x.ModulesMenus.Link))
                .ToListAsync();

            return rows.Where(x => MenuLink.Normalize(x.Link) != null).ToList();
        }

        private async Task<SiteMenuRow> FindMenu(string siteId, string siteModuleMenuId)
        {
            return (await SiteMenus(siteId)).FirstOrDefault(x => x.Id == siteModuleMenuId);
        }

        private sealed record MatrixValue(string SiteModuleMenuId, bool Show, byte View, byte Manage, string Reason, DateTime? ExpiresOnUtc);

        private async Task<List<SecurityMenuPermissionDto>> BuildMatrix(string siteId, List<MatrixValue> values)
        {
            var byMenu = values.ToDictionary(x => x.SiteModuleMenuId);

            return (await SiteMenus(siteId)).Select(menu =>
            {
                byMenu.TryGetValue(menu.Id, out var value);
                return new SecurityMenuPermissionDto
                {
                    SiteModuleMenuId = menu.Id,
                    Module = menu.Module,
                    Menu = menu.DisplayName,
                    Link = MenuLink.Normalize(menu.Link),
                    ShowInMenu = value?.Show ?? false,
                    ViewAccess = value?.View ?? Inherit,
                    ManageAccess = value?.Manage ?? Inherit,
                    Reason = value?.Reason,
                    ExpiresOnUtc = value?.ExpiresOnUtc
                };
            }).ToList();
        }

        private static string ValidateValues(string siteModuleMenuId, byte viewAccess, byte manageAccess)
        {
            if (string.IsNullOrWhiteSpace(siteModuleMenuId))
                return "Select a menu.";
            if (viewAccess > Deny || manageAccess > Deny)
                return "Use Inherit, Allow or Deny.";
            if (viewAccess == Deny && manageAccess == Allow)
                return "Manage can't be allowed while View is denied, because Manage includes View.";
            return null;
        }

        /// <summary>
        /// Admins can only hand out what they hold, unless they are System Super Admin. Deny and
        /// Inherit never add access, so they need no such check.
        /// </summary>
        private async Task<string> CheckGrantable(string callerId, string siteId, string siteModuleMenuId, byte view, byte manage)
        {
            if (view != Allow && manage != Allow)
                return null;

            var snapshot = await _permissionService.GetSnapshotAsync(callerId, siteId);
            if (snapshot.IsSystemAdmin)
                return null;

            var menu = await FindMenu(siteId, siteModuleMenuId);
            if (menu == null)
                return "This menu isn't available in this site.";

            var links = new[] { menu.Link };
            if (manage == Allow && !snapshot.Allows(links, AccessLevel.Manage))
                return $"You can't grant Manage on {menu.DisplayName}, because you don't have it.";
            if (view == Allow && !snapshot.Allows(links, AccessLevel.View))
                return $"You can't grant View on {menu.DisplayName}, because you don't have it.";

            return null;
        }

        private static (int Skip, int Take) PageOf(int page, int pageSize)
        {
            var take = Math.Clamp(pageSize, 1, 200);
            return ((Math.Max(page, 1) - 1) * take, take);
        }

        private void LogChange(string siteId, string callerId, string action, string targetType, string targetId, string link, object before, object after)
        {
            _securityLog.Write(LogLevel.Warning, PermissionChange, JsonSerializer.Serialize(new
            {
                action,
                siteId,
                actorId = callerId,
                targetType,
                targetId,
                link,
                before,
                after
            }));
        }
        #endregion
    }
}
