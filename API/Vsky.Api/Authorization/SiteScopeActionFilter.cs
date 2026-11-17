using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Vsky.Data;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Vsky.Api.Extensions;
using Vsky.Models;
using Vsky.Services.Authorization;

namespace Vsky.Api.Authorization
{
    /// <summary>
    /// Runs after model binding and rejects a site ID in the body, query or route that differs
    /// from the validated active site. Without this, a body value can override the checked header.
    /// Only a System Super Admin may name another site, and each time is logged. Admin tiers check
    /// their own target site, and Public, Service and Authenticated tiers have no active site.
    /// </summary>
    public class SiteScopeActionFilter : IAsyncActionFilter
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo> SiteIdProperties = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> LinkedProperties = new();

        // IDs in a request or model that point at site-owned records, however an action names them.
        private static readonly Dictionary<string, Type> LinkedFields = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ProjectId"] = typeof(Project), ["ProjectIds"] = typeof(Project),
            ["RequirementId"] = typeof(Requirement), ["RequirementIds"] = typeof(Requirement),
            ["ProjectModuleId"] = typeof(ProjectModule), ["ProjectModuleIds"] = typeof(ProjectModule),
            ["ProjectTaskId"] = typeof(ProjectTask), ["ProjectTaskIds"] = typeof(ProjectTask), ["TaskIds"] = typeof(ProjectTask),
            ["IssueId"] = typeof(Issue), ["IssueIds"] = typeof(Issue),
            ["TestCaseId"] = typeof(TestCase), ["TestCaseIds"] = typeof(TestCase),
            ["PlanId"] = typeof(TestPlan), ["PlanIds"] = typeof(TestPlan), ["TestPlanId"] = typeof(TestPlan),
            ["EmployeeId"] = typeof(Employee), ["EmployeeIds"] = typeof(Employee)
        };

        private readonly GlobalVariable _globalVariable;
        private readonly SecurityAuthorizationOptions _options;
        private readonly IPermissionService _permissionService;
        private readonly AuthorizationAuditLog _auditLog;

        public SiteScopeActionFilter(
            GlobalVariable globalVariable,
            IOptions<SecurityAuthorizationOptions> options,
            IPermissionService permissionService,
            AuthorizationAuditLog auditLog)
        {
            _globalVariable = globalVariable;
            _options = options.Value;
            _permissionService = permissionService;
            _auditLog = auditLog;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var enforce = _options.SiteGateMode == AuthorizationMode.Enforce;

            try
            {
                if (_options.SiteGateMode != AuthorizationMode.Off && context.ActionDescriptor is ControllerActionDescriptor action)
                {
                    var tier = EndpointPolicies.For(action).Tier;
                    var isChecked = tier == null || tier == EndpointTier.SiteMember || tier == EndpointTier.Menu;
                    var requestedSiteId = isChecked ? FindMismatchedSiteId(context) : null;

                    // A linked ID from another site is the same problem as naming that site directly.
                    if (requestedSiteId == null && isChecked && await HasLinkedIdFromOtherSiteAsync(context))
                        requestedSiteId = "(linked record)";

                    if (requestedSiteId != null)
                    {
                        var endpoint = $"{context.HttpContext.Request.Method} {action.ControllerName}.{action.ActionName}";
                        var isSystemAdmin = await IsSystemAdminAsync(context.HttpContext);

                        if (isSystemAdmin)
                        {
                            _auditLog.Record(context.HttpContext, endpoint, "SITE_SCOPE_MISMATCH", AuthorizationOutcome.Bypassed, requestedSiteId: requestedSiteId);
                        }
                        else
                        {
                            var outcome = enforce ? AuthorizationOutcome.Denied : AuthorizationOutcome.WouldDeny;
                            var referenceId = _auditLog.Record(context.HttpContext, endpoint, "SITE_SCOPE_MISMATCH", outcome, requestedSiteId: requestedSiteId);

                            if (enforce)
                            {
                                context.Result = AuthorizationAuditLog.Denied(StatusCodes.Status403Forbidden, "SITE_SCOPE_MISMATCH", referenceId);
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // A failure in the check itself must not break requests while only auditing.
                var referenceId = _auditLog.TryWrite("Site scope check failed", ex.ToString());
                if (enforce)
                {
                    context.Result = AuthorizationAuditLog.Denied(StatusCodes.Status403Forbidden, "SITE_SCOPE_MISMATCH", referenceId);
                    return;
                }
            }

            await next();
        }

        private async Task<bool> HasLinkedIdFromOtherSiteAsync(ActionExecutingContext context)
        {
            var found = new Dictionary<Type, HashSet<string>>();

            void Add(string name, object value)
            {
                if (!LinkedFields.TryGetValue(name, out var entityType))
                    return;

                var ids = value switch
                {
                    string single => new[] { single },
                    IEnumerable<string> many => many,
                    _ => Enumerable.Empty<string>()
                };

                foreach (var id in ids.Where(x => !string.IsNullOrWhiteSpace(x) && x != "undefined" && x != "null"))
                {
                    if (!found.TryGetValue(entityType, out var set))
                        found[entityType] = set = new HashSet<string>();
                    set.Add(id);
                }
            }

            static bool IsRequestModel(Type type) =>
                type?.Namespace != null && type.Namespace.StartsWith("Vsky.Api.Models", StringComparison.Ordinal);

            // Also one level of nested request models, such as copyModuleToTask's Requirements[].TaskIds,
            // so an ID can't reach another site's record by sitting one object deeper.
            void AddFromModel(object model, int depth)
            {
                var type = model?.GetType();
                if (!IsRequestModel(type))
                    return;

                foreach (var property in LinkedProperties.GetOrAdd(type, t => t
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetIndexParameters().Length == 0
                                && (LinkedFields.ContainsKey(p.Name) || IsRequestModel(p.PropertyType) || IsRequestModelList(p.PropertyType)))
                    .ToArray()))
                {
                    var value = property.GetValue(model);
                    if (LinkedFields.ContainsKey(property.Name))
                        Add(property.Name, value);
                    else if (depth > 0)
                        continue;
                    else if (value is IEnumerable items and not string)
                        foreach (var item in items)
                            AddFromModel(item, depth + 1);
                    else
                        AddFromModel(value, depth + 1);
                }
            }

            foreach (var argument in context.ActionArguments)
            {
                Add(argument.Key, argument.Value);
                AddFromModel(argument.Value, 0);
            }

            if (found.Count == 0)
                return false;

            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            foreach (var entry in found)
            {
                var inSite = await (Task<bool>)typeof(SiteOwnedAttribute)
                    .GetMethod(nameof(SiteOwnedAttribute.AreInSiteAsync), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(entry.Key)
                    .Invoke(null, new object[] { db, entry.Value.ToList(), _globalVariable.SiteId });

                if (!inSite)
                    return true;
            }

            return false;
        }

        private static bool IsRequestModelList(Type type)
        {
            if (type == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(type))
                return false;

            var itemType = type.IsArray ? type.GetElementType() : type.GetGenericArguments().FirstOrDefault();
            return itemType?.Namespace != null && itemType.Namespace.StartsWith("Vsky.Api.Models", StringComparison.Ordinal);
        }

        private async Task<bool> IsSystemAdminAsync(HttpContext httpContext)
        {
            if (httpContext.User?.Identity?.IsAuthenticated != true)
                return false;

            var snapshot = await _permissionService.GetSnapshotAsync(httpContext.User.GetLoggedInUserId<string>(), _globalVariable.SiteId);
            return snapshot.IsSystemAdmin;
        }

        private string FindMismatchedSiteId(ActionExecutingContext context)
        {
            var activeSiteId = _globalVariable.SiteId;

            foreach (var argument in context.ActionArguments)
            {
                var requested = string.Equals(argument.Key, "siteId", StringComparison.OrdinalIgnoreCase)
                    ? argument.Value as string
                    : FindSiteIdInModel(argument.Value, activeSiteId);

                if (IsMismatch(requested, activeSiteId))
                    return requested;
            }

            // A query value the action doesn't bind can still be read later, so check it too.
            var queryValue = context.HttpContext.Request.Query["siteId"].ToString();
            return IsMismatch(queryValue, activeSiteId) ? queryValue : null;
        }

        private static string FindSiteIdInModel(object value, string activeSiteId)
        {
            switch (value)
            {
                case null:
                case string:
                    return null;

                case IEnumerable items:
                    foreach (var item in items)
                    {
                        var requested = FindSiteIdInModel(item, activeSiteId);
                        if (IsMismatch(requested, activeSiteId))
                            return requested;
                    }
                    return null;
            }

            // Only this application's own models: framework types such as IFormFile are skipped.
            var type = value.GetType();
            if (type.Namespace == null || !type.Namespace.StartsWith("Vsky", StringComparison.Ordinal))
                return null;

            var property = SiteIdProperties.GetOrAdd(type, t => t
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.PropertyType == typeof(string)
                                     && p.GetIndexParameters().Length == 0
                                     && string.Equals(p.Name, "SiteId", StringComparison.OrdinalIgnoreCase)));

            return property?.GetValue(value) as string;
        }

        private static bool IsMismatch(string requested, string activeSiteId)
        {
            // These are how the Vue pages say "use the active site".
            if (string.IsNullOrWhiteSpace(requested) || requested == "undefined" || requested == "null")
                return false;

            return !string.Equals(requested, activeSiteId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
