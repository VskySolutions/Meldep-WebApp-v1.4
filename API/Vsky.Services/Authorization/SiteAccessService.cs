using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;

namespace Vsky.Services.Authorization
{
    /// <summary>
    /// A user may act in a site only if the site is active and not deleted, a non-deleted
    /// PersonSitesMapping row links their person to it, and they hold at least one role there
    /// whose AspNetRoles and SitesRoles rows aren't deleted. IsSharedUser plays no part: a shared
    /// user passes the same gate as any other member, using only their roles in that site.
    /// </summary>
    public class SiteAccessService : ISiteAccessService
    {
        private readonly ApplicationDbContext _db;

        public SiteAccessService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> IsSiteMemberAsync(string userId, string siteId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(siteId))
                return false;

            var accessibleSiteIds = await GetAccessibleSiteIdsAsync(userId);
            return accessibleSiteIds.Contains(siteId);
        }

        public async Task<string> GetDefaultSiteIdAsync(string userId)
        {
            var accessibleSiteIds = await GetAccessibleSiteIdsAsync(userId);
            if (accessibleSiteIds.Count == 0)
                return null;

            var lastUsedSiteId = await (
                from user in _db.Users
                join mapping in _db.PersonSitesMapping on user.PersonId equals mapping.PersonId
                where user.Id == userId && mapping.LastUsed && !mapping.Deleted
                select mapping.SiteId).FirstOrDefaultAsync();

            return lastUsedSiteId != null && accessibleSiteIds.Contains(lastUsedSiteId)
                ? lastUsedSiteId
                : accessibleSiteIds[0];
        }

        public async Task<List<string>> GetAccessibleSiteIdsAsync(string userId)
        {
            var personId = await _db.Users
                .Where(x => x.Id == userId && x.Active && !x.Deleted)
                .Select(x => x.PersonId)
                .FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(personId))
                return new List<string>();

            var sitesWithValidRole =
                from userRole in _db.UserRoles
                join role in _db.Roles on userRole.RoleId equals role.Id
                join siteRole in _db.SitesRoles on userRole.RoleId equals siteRole.RoleId
                where userRole.UserId == userId
                      && !role.Deleted
                      && siteRole.SiteId == userRole.SiteId
                      && !siteRole.Deleted
                select userRole.SiteId;

            return await (
                from mapping in _db.PersonSitesMapping
                join site in _db.Sites on mapping.SiteId equals site.Id
                where mapping.PersonId == personId
                      && !mapping.Deleted
                      && site.Active
                      && !site.Deleted
                      && sitesWithValidRole.Contains(mapping.SiteId)
                orderby mapping.CreatedOnUtc, mapping.Id
                select mapping.SiteId).ToListAsync();
        }
    }
}
