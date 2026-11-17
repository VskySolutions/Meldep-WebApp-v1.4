using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vsky.Data;

namespace Vsky.Services.Authorization
{
    /// <summary>
    /// Grants from the user's roles in this site. Every table in the path must agree on the site,
    /// so a role held in another site can never count here.
    /// </summary>
    public class RolePermissionSource : IPermissionSource
    {
        private readonly ApplicationDbContext _db;
        private readonly SecurityAuthorizationOptions _options;

        public RolePermissionSource(ApplicationDbContext db, IOptions<SecurityAuthorizationOptions> options)
        {
            _db = db;
            _options = options.Value;
        }

        public async Task<IReadOnlyList<PermissionGrant>> GetGrantsAsync(string userId, string siteId)
        {
            var siteRoleIds =
                from userRole in _db.UserRoles
                join role in _db.Roles on userRole.RoleId equals role.Id
                join siteRole in _db.SitesRoles on userRole.RoleId equals siteRole.RoleId
                where userRole.UserId == userId
                      && userRole.SiteId == siteId
                      && !role.Deleted
                      && siteRole.SiteId == siteId
                      && !siteRole.Deleted
                select siteRole.Id;

            var enforceSiteFlags = _options.EnforceSiteLevelActiveFlags;

            var rows = await _db.SitesModulesMenusPermissions
                .AsNoTracking()
                .Where(p => p.SiteId == siteId
                            && !p.Deleted
                            && siteRoleIds.Contains(p.SiteRoleId)
                            && p.SitesModulesMenus.SiteId == siteId
                            && !p.SitesModulesMenus.Deleted
                            && p.SitesModulesMenus.SitesModules.SiteId == siteId
                            && !p.SitesModulesMenus.SitesModules.Deleted
                            && p.SitesModulesMenus.ModulesMenus.Active
                            && !p.SitesModulesMenus.ModulesMenus.Deleted
                            && !p.SitesModulesMenus.ModulesMenus.Modules.Deleted
                            && (!enforceSiteFlags
                                || (p.SitesModulesMenus.Active
                                    && p.SitesModulesMenus.SitesModules.Active
                                    && p.SitesModulesMenus.ModulesMenus.Modules.Active)))
                .Select(p => new
                {
                    p.SiteRoleId,
                    p.SitesModulesMenus.MenuId,
                    p.SitesModulesMenus.ModulesMenus.Link,
                    p.IsShowMenu,
                    p.IsView,
                    p.IsManage
                })
                .ToListAsync();

            return rows.Select(row =>
            {
                var isLegacyRow = _options.LegacyShowOnlyAccess && row.IsShowMenu && !row.IsView && !row.IsManage;

                // false means "not granted", never deny, so adding a role can't remove access.
                return new PermissionGrant(
                    row.Link,
                    row.MenuId,
                    PermissionSourceKind.Role,
                    row.SiteRoleId,
                    row.IsShowMenu,
                    row.IsView || isLegacyRow ? GrantValue.Allow : GrantValue.Inherit,
                    row.IsManage || isLegacyRow ? GrantValue.Allow : GrantValue.Inherit,
                    isLegacyRow);
            }).ToList();
        }
    }
}
