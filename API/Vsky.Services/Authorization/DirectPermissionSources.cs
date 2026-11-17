using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Authorization
{
    public static class ValidSiteMenus
    {
        /// <summary>The same module and menu checks as role permissions, so no grant can revive a deleted menu.</summary>
        public static IQueryable<SitesModulesMenus> Query(ApplicationDbContext db, string siteId, bool enforceSiteFlags)
        {
            return db.Set<SitesModulesMenus>()
                .AsNoTracking()
                .Where(menu => menu.SiteId == siteId
                               && !menu.Deleted
                               && menu.SitesModules.SiteId == siteId
                               && !menu.SitesModules.Deleted
                               && menu.ModulesMenus.Active
                               && !menu.ModulesMenus.Deleted
                               && !menu.ModulesMenus.Modules.Deleted
                               && (!enforceSiteFlags
                                   || (menu.Active && menu.SitesModules.Active && menu.ModulesMenus.Modules.Active)));
        }

        public static GrantValue ToGrant(byte value)
        {
            return value switch
            {
                1 => GrantValue.Allow,
                2 => GrantValue.Deny,
                _ => GrantValue.Inherit
            };
        }
    }

    /// <summary>Grants from active groups the user belongs to in this site. Group, member and grant must all agree on the site.</summary>
    public class GroupPermissionSource : IPermissionSource
    {
        private readonly ApplicationDbContext _db;
        private readonly SecurityAuthorizationOptions _options;

        public GroupPermissionSource(ApplicationDbContext db, IOptions<SecurityAuthorizationOptions> options)
        {
            _db = db;
            _options = options.Value;
        }

        public async Task<IReadOnlyList<PermissionGrant>> GetGrantsAsync(string userId, string siteId)
        {
            var groupIds =
                from member in _db.SecurityGroupMembers
                join securityGroup in _db.SecurityGroups on member.SecurityGroupId equals securityGroup.Id
                where member.UserId == userId
                      && member.SiteId == siteId
                      && !member.Deleted
                      && securityGroup.SiteId == siteId
                      && securityGroup.Active
                      && !securityGroup.Deleted
                select securityGroup.Id;

            var rows = await (
                from permission in _db.SecurityGroupMenuPermissions
                join menu in ValidSiteMenus.Query(_db, siteId, _options.EnforceSiteLevelActiveFlags)
                    on permission.SiteModuleMenuId equals menu.Id
                where permission.SiteId == siteId
                      && !permission.Deleted
                      && groupIds.Contains(permission.SecurityGroupId)
                select new
                {
                    permission.SecurityGroupId,
                    menu.MenuId,
                    menu.ModulesMenus.Link,
                    permission.ShowInMenu,
                    permission.ViewAccess,
                    permission.ManageAccess
                }).ToListAsync();

            return rows.Select(row => new PermissionGrant(
                row.Link,
                row.MenuId,
                PermissionSourceKind.Group,
                row.SecurityGroupId,
                row.ShowInMenu,
                ValidSiteMenus.ToGrant(row.ViewAccess),
                ValidSiteMenus.ToGrant(row.ManageAccess))).ToList();
        }
    }

    /// <summary>Direct grants and denies for this user in this site. An expired row counts as if it were never there.</summary>
    public class UserPermissionSource : IPermissionSource
    {
        private readonly ApplicationDbContext _db;
        private readonly SecurityAuthorizationOptions _options;

        public UserPermissionSource(ApplicationDbContext db, IOptions<SecurityAuthorizationOptions> options)
        {
            _db = db;
            _options = options.Value;
        }

        public async Task<IReadOnlyList<PermissionGrant>> GetGrantsAsync(string userId, string siteId)
        {
            var now = DateTime.UtcNow;

            var rows = await (
                from permission in _db.UserMenuPermissions
                join menu in ValidSiteMenus.Query(_db, siteId, _options.EnforceSiteLevelActiveFlags)
                    on permission.SiteModuleMenuId equals menu.Id
                where permission.UserId == userId
                      && permission.SiteId == siteId
                      && !permission.Deleted
                      && (permission.ExpiresOnUtc == null || permission.ExpiresOnUtc > now)
                select new
                {
                    permission.Id,
                    menu.MenuId,
                    menu.ModulesMenus.Link,
                    permission.ShowInMenu,
                    permission.ViewAccess,
                    permission.ManageAccess
                }).ToListAsync();

            return rows.Select(row => new PermissionGrant(
                row.Link,
                row.MenuId,
                PermissionSourceKind.User,
                row.Id,
                row.ShowInMenu,
                ValidSiteMenus.ToGrant(row.ViewAccess),
                ValidSiteMenus.ToGrant(row.ManageAccess))).ToList();
        }
    }
}
