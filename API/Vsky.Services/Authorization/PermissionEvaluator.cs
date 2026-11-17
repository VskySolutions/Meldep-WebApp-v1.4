using System;
using System.Collections.Generic;
using System.Linq;

namespace Vsky.Services.Authorization
{
    public sealed record EvaluationResult(IReadOnlyDictionary<string, MenuAccess> Menus, IReadOnlyList<string> Issues);

    /// <summary>
    /// The precedence rules, as a pure function. View and Manage are decided separately, per menu,
    /// and the first matching rule wins:
    ///   1. Conflicting duplicate rows for one role and menu: deny that flag.
    ///   2. User Deny: deny.
    ///   3. User Allow: allow, as the most specific rule.
    ///   4. Deny from any group: deny.
    ///   5. Allow from any role or group: allow.
    ///   6. Nothing matched: deny.
    /// Manage implies View, and an explicit View deny also blocks Manage.
    /// Hard gates such as site membership run before this and can't be overridden.
    /// </summary>
    public static class PermissionEvaluator
    {
        public static EvaluationResult Evaluate(IEnumerable<PermissionGrant> grants)
        {
            var menus = new Dictionary<string, MenuAccess>();
            var issues = new List<string>();

            var byLink = grants
                .Select(grant => (Key: MenuLink.Normalize(grant.Link), Grant: grant))
                .Where(item => item.Key != null)
                .GroupBy(item => item.Key, item => item.Grant);

            foreach (var linkGrants in byLink)
            {
                // A link can sit on more than one menu (/department, /candidate and others do in
                // real data). It is still one page, so each menu's rows are grants for that page.
                var list = linkGrants.ToList();

                var viewConflict = HasRoleConflict(list, g => g.View);
                var manageConflict = HasRoleConflict(list, g => g.Manage);
                if (viewConflict || manageConflict)
                    issues.Add($"Conflicting duplicate permission rows: {linkGrants.Key}");

                var viewExplicitlyDenied = IsExplicitlyDenied(list, g => g.View);
                var manage = Resolve(list, g => g.Manage, manageConflict) && !viewExplicitlyDenied;
                var view = !viewExplicitlyDenied && (Resolve(list, g => g.View, viewConflict) || manage);
                var show = view && list.Any(g => g.Show);

                menus[linkGrants.Key] = new MenuAccess(linkGrants.Key, show, view, manage);
            }

            return new EvaluationResult(menus, issues);
        }

        private static bool Resolve(List<PermissionGrant> grants, Func<PermissionGrant, GrantValue> flag, bool hasConflict)
        {
            if (hasConflict)
                return false;

            if (grants.Any(g => g.Kind == PermissionSourceKind.User && flag(g) == GrantValue.Deny))
                return false;

            if (grants.Any(g => g.Kind == PermissionSourceKind.User && flag(g) == GrantValue.Allow))
                return true;

            if (grants.Any(g => g.Kind == PermissionSourceKind.Group && flag(g) == GrantValue.Deny))
                return false;

            return grants.Any(g => flag(g) == GrantValue.Allow);
        }

        private static bool IsExplicitlyDenied(List<PermissionGrant> grants, Func<PermissionGrant, GrantValue> flag)
        {
            if (grants.Any(g => g.Kind == PermissionSourceKind.User && flag(g) == GrantValue.Deny))
                return true;

            if (grants.Any(g => g.Kind == PermissionSourceKind.User && flag(g) == GrantValue.Allow))
                return false;

            return grants.Any(g => g.Kind == PermissionSourceKind.Group && flag(g) == GrantValue.Deny);
        }

        private static bool HasRoleConflict(List<PermissionGrant> grants, Func<PermissionGrant, GrantValue> flag)
        {
            return grants
                .Where(g => g.Kind == PermissionSourceKind.Role)
                .GroupBy(g => new { g.SourceId, g.MenuId })
                .Any(rows => rows.Select(flag).Distinct().Count() > 1);
        }
    }
}
