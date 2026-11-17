using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Vsky.Services.Authorization;

namespace Vsky.Api.Authorization
{
    /// <summary>
    /// Clears cached permissions after any administrator write. Doing this in one place means a
    /// new admin endpoint can't forget it. Admin writes are rare, so clearing everything is fine.
    /// </summary>
    public class PermissionCacheInvalidationFilter : IAsyncActionFilter
    {
        private readonly PermissionCache _permissionCache;

        public PermissionCacheInvalidationFilter(PermissionCache permissionCache)
        {
            _permissionCache = permissionCache;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executed = await next();

            var isAdminWrite = context.ActionDescriptor is ControllerActionDescriptor action
                               && EndpointPolicies.For(action).Tier == EndpointTier.Admin
                               && !HttpMethods.IsGet(context.HttpContext.Request.Method);

            if (isAdminWrite && executed.Exception == null)
                _permissionCache.InvalidateAll();
        }
    }
}
