using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Vsky.Data;

namespace Vsky.Services.Authorization
{
    /// <summary>
    /// Snapshots are cached per user and site, so one site's rights can never be read for another.
    /// Admin changes clear the cache on this server. With several servers, 60 seconds is the
    /// longest a removed permission can still apply.
    /// </summary>
    public sealed class PermissionCache
    {
        private long _generation;

        public long Generation => Interlocked.Read(ref _generation);

        public void InvalidateAll()
        {
            Interlocked.Increment(ref _generation);
        }
    }

    public class PermissionService : IPermissionService
    {
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromSeconds(60);
        private const string SystemSuperAdmin = "system-super-admin";
        private const string SiteSuperAdmin = "site-super-admin";
        private const string SiteAdmin = "admin";

        private readonly ApplicationDbContext _db;
        private readonly ISiteAccessService _siteAccessService;
        private readonly IEnumerable<IPermissionSource> _sources;
        private readonly IMemoryCache _cache;
        private readonly PermissionCache _permissionCache;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(
            ApplicationDbContext db,
            ISiteAccessService siteAccessService,
            IEnumerable<IPermissionSource> sources,
            IMemoryCache cache,
            PermissionCache permissionCache,
            ILogger<PermissionService> logger)
        {
            _db = db;
            _siteAccessService = siteAccessService;
            _sources = sources;
            _cache = cache;
            _permissionCache = permissionCache;
            _logger = logger;
        }

        public async Task<PermissionSnapshot> GetSnapshotAsync(string userId, string siteId)
        {
            var cacheKey = $"permissions:{_permissionCache.Generation}:{userId}:{siteId}";
            if (_cache.TryGetValue(cacheKey, out PermissionSnapshot cached))
                return cached;

            var snapshot = await BuildSnapshotAsync(userId, siteId);
            _cache.Set(cacheKey, snapshot, CacheLifetime);
            return snapshot;
        }

        private async Task<PermissionSnapshot> BuildSnapshotAsync(string userId, string siteId)
        {
            var roleNames = await (
                from userRole in _db.UserRoles
                join role in _db.Roles on userRole.RoleId equals role.Id
                where userRole.UserId == userId
                      && !role.Deleted
                      && (userRole.SiteId == siteId || role.NormalizedName == SystemSuperAdmin)
                select new { userRole.SiteId, role.NormalizedName }).ToListAsync();

            bool HasRole(string name) => roleNames.Any(x => string.Equals(x.NormalizedName, name, StringComparison.OrdinalIgnoreCase));
            bool HasRoleInSite(string name) => roleNames.Any(x => x.SiteId == siteId && string.Equals(x.NormalizedName, name, StringComparison.OrdinalIgnoreCase));

            var isSystemAdmin = HasRole(SystemSuperAdmin);
            var isSiteAdmin = isSystemAdmin || HasRoleInSite(SiteSuperAdmin) || HasRoleInSite(SiteAdmin);

            if (!await _siteAccessService.IsSiteMemberAsync(userId, siteId))
            {
                return new PermissionSnapshot
                {
                    UserId = userId,
                    SiteId = siteId,
                    IsSiteMember = false,
                    IsSystemAdmin = isSystemAdmin,
                    IsSiteAdmin = isSiteAdmin
                };
            }

            var grants = new List<PermissionGrant>();
            foreach (var source in _sources)
                grants.AddRange(await source.GetGrantsAsync(userId, siteId));

            var result = PermissionEvaluator.Evaluate(grants);
            foreach (var issue in result.Issues)
                _logger.LogWarning("Permission data issue for site {SiteId}: {Issue}", siteId, issue);

            return new PermissionSnapshot
            {
                UserId = userId,
                SiteId = siteId,
                IsSiteMember = true,
                IsSystemAdmin = isSystemAdmin,
                IsSiteAdmin = isSiteAdmin,
                Menus = result.Menus,
                DirectGrantLinks = grants
                    .Where(x => x.Kind != PermissionSourceKind.Role)
                    .Select(x => MenuLink.Normalize(x.Link))
                    .Where(x => x != null)
                    .ToHashSet(),
                ConfiguredManageLinks = grants
                    .Where(x => !x.IsLegacy && x.Manage == GrantValue.Allow)
                    .Select(x => MenuLink.Normalize(x.Link))
                    .Where(x => x != null)
                    .ToHashSet()
            };
        }
    }
}
