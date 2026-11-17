using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vsky.Services.Authorization
{
    public enum PermissionSourceKind
    {
        Role,
        Group,
        User
    }

    public enum GrantValue
    {
        Inherit,
        Allow,
        Deny
    }

    public sealed record PermissionGrant(
        string Link,
        string MenuId,
        PermissionSourceKind Kind,
        string SourceId,
        bool Show,
        GrantValue View,
        GrantValue Manage,
        bool IsLegacy = false);

    /// <summary>
    /// One place permissions come from: roles, security groups or direct user grants. The evaluator
    /// combines every source, so the filter and controllers don't need to know which one applied.
    /// </summary>
    public interface IPermissionSource
    {
        Task<IReadOnlyList<PermissionGrant>> GetGrantsAsync(string userId, string siteId);
    }
}
