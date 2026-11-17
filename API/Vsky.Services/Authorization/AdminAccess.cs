using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Authorization
{
    /// <summary>
    /// Phase 2.0 containment checks for administrator-only operations. Roles are always read
    /// per site, because the role claims in the JWT are not site-scoped.
    /// </summary>
    public class AdminAccess
    {
        public const string SystemSuperAdmin = "system-super-admin";
        public const string SiteSuperAdmin = "site-super-admin";
        public const string SiteAdmin = "admin";

        private const int NoAdminRank = 0;
        private const int SiteAdminRank = 1;
        private const int SiteSuperAdminRank = 2;
        private const int SystemSuperAdminRank = 3;

        private readonly ApplicationDbContext _db;
        private readonly GlobalVariable _globalVariable;

        public AdminAccess(ApplicationDbContext db, GlobalVariable globalVariable)
        {
            _db = db;
            _globalVariable = globalVariable;
        }

        public string ActiveSiteId => _globalVariable.SiteId;

        private static int RankOf(string normalizedRoleName)
        {
            if (string.Equals(normalizedRoleName, SystemSuperAdmin, StringComparison.OrdinalIgnoreCase))
                return SystemSuperAdminRank;
            if (string.Equals(normalizedRoleName, SiteSuperAdmin, StringComparison.OrdinalIgnoreCase))
                return SiteSuperAdminRank;
            if (string.Equals(normalizedRoleName, SiteAdmin, StringComparison.OrdinalIgnoreCase))
                return SiteAdminRank;
            return NoAdminRank;
        }

        private static int HighestRank(IEnumerable<string> normalizedRoleNames)
        {
            return normalizedRoleNames.Select(RankOf).DefaultIfEmpty(NoAdminRank).Max();
        }

        /// <summary>Highest admin rank in a site. System Super Admin counts in every site.</summary>
        public async Task<int> GetRankAsync(string userId, string siteId)
        {
            var roleNames = await (
                from userRole in _db.UserRoles
                join role in _db.Roles on userRole.RoleId equals role.Id
                where userRole.UserId == userId
                      && !role.Deleted
                      && (userRole.SiteId == siteId || role.NormalizedName == SystemSuperAdmin)
                select role.NormalizedName).ToListAsync();

            return HighestRank(roleNames);
        }

        public async Task<bool> IsSystemAdminAsync(string userId)
        {
            return await GetRankAsync(userId, null) == SystemSuperAdminRank;
        }

        /// <summary>
        /// True if the caller may administer the target site: a System Super Admin may administer
        /// any site, and any other admin only their active site.
        /// </summary>
        public async Task<bool> CanAdministerSiteAsync(string userId, string targetSiteId)
        {
            var activeSiteId = _globalVariable.SiteId;
            var siteId = IsUnset(targetSiteId) ? activeSiteId : targetSiteId;
            if (IsUnset(siteId))
                return false;

            if (await IsSystemAdminAsync(userId))
                return true;

            return string.Equals(siteId, activeSiteId, StringComparison.OrdinalIgnoreCase)
                   && await GetRankAsync(userId, activeSiteId) >= SiteAdminRank;
        }

        /// <summary>
        /// True if the caller may change the target user in the site. The target must belong to
        /// the site and must not hold a higher admin role than the caller anywhere.
        /// </summary>
        public async Task<bool> CanManageUserAsync(string callerId, string targetUserId, string siteId)
        {
            if (await IsSystemAdminAsync(callerId))
                return true;

            var callerRank = await GetRankAsync(callerId, siteId);
            if (callerRank < SiteAdminRank)
                return false;

            var targetPersonId = await _db.Users
                .Where(x => x.Id == targetUserId)
                .Select(x => x.PersonId)
                .FirstOrDefaultAsync();

            var targetInSite =
                await _db.UserRoles.AnyAsync(x => x.UserId == targetUserId && x.SiteId == siteId)
                || (targetPersonId != null && await _db.PersonSitesMapping.AnyAsync(x =>
                    x.PersonId == targetPersonId && x.SiteId == siteId && !x.Deleted));
            if (!targetInSite)
                return false;

            var targetRoleNames = await (
                from userRole in _db.UserRoles
                join role in _db.Roles on userRole.RoleId equals role.Id
                where userRole.UserId == targetUserId && !role.Deleted
                select role.NormalizedName).ToListAsync();

            return HighestRank(targetRoleNames) <= callerRank;
        }

        /// <summary>
        /// Checks that every requested site role belongs to the site, isn't deleted, and doesn't
        /// outrank the caller. Returns an error message, or null when all are acceptable.
        /// </summary>
        public async Task<string> ValidateGrantableSiteRolesAsync(string callerId, string siteId, IEnumerable<string> siteRoleIds)
        {
            var requestedIds = (siteRoleIds ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
            if (requestedIds.Count == 0)
                return null;

            var roles = await (
                from siteRole in _db.SitesRoles
                join role in _db.Roles on siteRole.RoleId equals role.Id
                where requestedIds.Contains(siteRole.Id)
                      && siteRole.SiteId == siteId
                      && !siteRole.Deleted
                      && !role.Deleted
                select new { siteRole.Id, role.NormalizedName }).ToListAsync();

            if (roles.Select(x => x.Id).Distinct().Count() != requestedIds.Count)
                return "One or more selected roles are not valid for this site.";

            var callerRank = await GetRankAsync(callerId, siteId);
            if (HighestRank(roles.Select(x => x.NormalizedName)) > callerRank)
                return "You cannot assign a role higher than your own.";

            return null;
        }

        /// <summary>True if every site role ID exists in the given site and isn't deleted.</summary>
        public async Task<bool> AreSiteRolesInSiteAsync(string siteId, IEnumerable<string> siteRoleIds)
        {
            var requestedIds = (siteRoleIds ?? Enumerable.Empty<string>()).Distinct().ToList();

            var matchCount = await _db.SitesRoles.CountAsync(x =>
                requestedIds.Contains(x.Id) && x.SiteId == siteId && !x.Deleted);

            return matchCount == requestedIds.Count;
        }

        private static bool IsUnset(string siteId)
        {
            return string.IsNullOrWhiteSpace(siteId) || siteId == "undefined" || siteId == "null";
        }
    }
}
