using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Vsky.Api.Extensions;

namespace Vsky.Api.Authorization
{
    /// <summary>
    /// Requires an administrator role (admin, site-super-admin or system-super-admin) in the
    /// active site. When SiteKey is set, the route or query value with that name is the target
    /// site, and only a System Super Admin may name a site other than the active one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireSiteAdminAttribute : Attribute, IAsyncAuthorizationFilter, IEndpointTierAttribute
    {
        public EndpointTier Tier => EndpointTier.Admin;

        public string SiteKey { get; set; }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var access = context.HttpContext.RequestServices.GetRequiredService<AdminAccess>();
            var userId = context.HttpContext.User.GetLoggedInUserId<string>();

            string targetSiteId = null;
            if (!string.IsNullOrEmpty(SiteKey))
            {
                targetSiteId = context.RouteData.Values.TryGetValue(SiteKey, out var routeValue)
                    ? routeValue?.ToString()
                    : context.HttpContext.Request.Query[SiteKey].ToString();
            }

            if (!await access.CanAdministerSiteAsync(userId, targetSiteId))
                context.Result = AdminDenied.Result();
        }
    }

    /// <summary>Requires the System Super Admin role, for data shared by all sites.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireSystemAdminAttribute : Attribute, IAsyncAuthorizationFilter, IEndpointTierAttribute
    {
        public EndpointTier Tier => EndpointTier.Admin;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var access = context.HttpContext.RequestServices.GetRequiredService<AdminAccess>();
            var userId = context.HttpContext.User.GetLoggedInUserId<string>();

            if (!await access.IsSystemAdminAsync(userId))
                context.Result = AdminDenied.Result();
        }
    }

    public static class AdminDenied
    {
        public static ObjectResult Result()
        {
            return new ObjectResult(new
            {
                code = "PERMISSION_DENIED",
                message = "You do not have permission to perform this action."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
