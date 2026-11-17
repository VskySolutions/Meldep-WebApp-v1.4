using System;
using System.Collections.Generic;
using System.Linq;

namespace Vsky.Services.Authorization
{
    public enum AccessLevel
    {
        View,
        Manage
    }

    public sealed record MenuAccess(string Link, bool Show, bool View, bool Manage);

    /// <summary>What one user may do in one site. Built by PermissionService and cached briefly.</summary>
    public sealed class PermissionSnapshot
    {
        public string UserId { get; init; }
        public string SiteId { get; init; }
        public bool IsSiteMember { get; init; }
        public bool IsSystemAdmin { get; init; }
        public bool IsSiteAdmin { get; init; }
        public IReadOnlyDictionary<string, MenuAccess> Menus { get; init; } = new Dictionary<string, MenuAccess>();

        /// <summary>Links with a group or individual grant, whose menu visibility comes from Menus.</summary>
        public IReadOnlySet<string> DirectGrantLinks { get; init; } = new HashSet<string>();

        /// <summary>Links where Manage was deliberately set, not just implied by a Show-only legacy row.</summary>
        public IReadOnlySet<string> ConfiguredManageLinks { get; init; } = new HashSet<string>();

        /// <summary>
        /// For decisions about other people's records. A Show-only legacy row keeps a page usable,
        /// but it wasn't a decision to let that role approve for others.
        /// </summary>
        public bool HasConfiguredManage(IEnumerable<string> links)
        {
            return links.Any(link =>
            {
                var key = MenuLink.Normalize(link);
                return key != null
                       && ConfiguredManageLinks.Contains(key)
                       && Menus.TryGetValue(key, out var access)
                       && access.Manage;
            });
        }

        /// <summary>True if any of the links grants the level. A link with no entry grants nothing.</summary>
        public bool Allows(IEnumerable<string> links, AccessLevel level)
        {
            return links.Any(link =>
            {
                var key = MenuLink.Normalize(link);
                return key != null
                       && Menus.TryGetValue(key, out var access)
                       && (level == AccessLevel.Manage ? access.Manage : access.View);
            });
        }
    }

    public static class MenuLink
    {
        /// <summary>
        /// Exact-match key for ModulesMenus.Link. External links return null, because they aren't
        /// pages in this application. Prefix matching is never used, so /project can't match
        /// /project-tasks.
        /// </summary>
        public static string Normalize(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
                return null;

            var value = link.Trim();
            if (value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return null;

            var end = value.IndexOfAny(new[] { '?', '#' });
            if (end >= 0)
                value = value.Substring(0, end);

            value = value.TrimEnd('/').ToLowerInvariant();
            return value.Length == 0 ? null : value;
        }
    }
}
