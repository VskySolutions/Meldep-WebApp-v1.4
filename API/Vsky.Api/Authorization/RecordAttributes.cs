using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vsky.Api.Extensions;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Authorization;

namespace Vsky.Api.Authorization
{
    /// <summary>
    /// Checks that records named in a request belong to the active site, before the action runs.
    /// Names the route value "id" by default, or a property on the request body with Field, which
    /// may be one ID or a list. Returns 404 for "another site" and "doesn't exist" alike, so a
    /// caller can't test which IDs exist. A record with no SiteId isn't site-owned, so it passes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class SiteOwnedAttribute : Attribute, IAsyncActionFilter
    {
        public SiteOwnedAttribute(Type entityType)
        {
            EntityType = entityType;
        }

        public Type EntityType { get; }

        public string Field { get; set; }

        public string RouteKey { get; set; } = "id";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var services = context.HttpContext.RequestServices;
            var options = services.GetRequiredService<IOptions<SecurityAuthorizationOptions>>().Value;
            var enforce = options.SiteGateMode == AuthorizationMode.Enforce;

            if (options.SiteGateMode != AuthorizationMode.Off)
            {
                var auditLog = services.GetRequiredService<AuthorizationAuditLog>();

                try
                {
                    var siteId = services.GetRequiredService<GlobalVariable>().SiteId;
                    var ids = RecordIds.From(context, Field, RouteKey);

                    var allInSite = await (Task<bool>)typeof(SiteOwnedAttribute)
                        .GetMethod(nameof(AreInSiteAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                        .MakeGenericMethod(EntityType)
                        .Invoke(null, new object[] { services.GetRequiredService<ApplicationDbContext>(), ids, siteId });

                    if (!allInSite)
                    {
                        var endpoint = RecordIds.Endpoint(context);
                        var snapshot = await services.GetRequiredService<IPermissionService>()
                            .GetSnapshotAsync(context.HttpContext.User.GetLoggedInUserId<string>(), siteId);

                        if (snapshot.IsSystemAdmin)
                        {
                            auditLog.Record(context.HttpContext, endpoint, "RECORD_OTHER_SITE", AuthorizationOutcome.Bypassed);
                        }
                        else
                        {
                            var referenceId = auditLog.Record(context.HttpContext, endpoint, "RECORD_OTHER_SITE",
                                enforce ? AuthorizationOutcome.Denied : AuthorizationOutcome.WouldDeny);

                            if (enforce)
                            {
                                context.Result = RecordIds.NotFound(referenceId);
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var referenceId = auditLog.TryWrite("Record site check failed", ex.ToString());
                    if (enforce)
                    {
                        context.Result = RecordIds.NotFound(referenceId);
                        return;
                    }
                }
            }

            await next();
        }

        internal static async Task<bool> AreInSiteAsync<TEntity>(ApplicationDbContext db, List<string> ids, string siteId) where TEntity : class
        {
            if (ids.Count == 0)
                return true;

            var foreignCount = await db.Set<TEntity>()
                .AsNoTracking()
                .Where(x => ids.Contains(EF.Property<string>(x, "Id"))
                            && EF.Property<string>(x, "SiteId") != null
                            && EF.Property<string>(x, "SiteId") != siteId)
                .CountAsync();

            return foreignCount == 0;
        }
    }

    /// <summary>
    /// Applies the existing project rule to a request by ID, so a user can open by ID only what
    /// their own list would show. The entity must have ProjectId and CreatedById.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RecordScopeAttribute : Attribute, IAsyncActionFilter
    {
        public RecordScopeAttribute(Type entityType)
        {
            EntityType = entityType;
        }

        public RecordScopeAttribute(RecordRule rule)
        {
            Rule = rule;
        }

        public Type EntityType { get; }

        public RecordRule? Rule { get; }

        /// <summary>A property on the request body holding the record ID, used instead of the route value.</summary>
        public string Field { get; set; }

        public string RouteKey { get; set; } = "id";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var services = context.HttpContext.RequestServices;
            var options = services.GetRequiredService<IOptions<SecurityAuthorizationOptions>>().Value;
            var enforce = options.RecordScopeMode == AuthorizationMode.Enforce;

            if (options.RecordScopeMode != AuthorizationMode.Off)
            {
                var auditLog = services.GetRequiredService<AuthorizationAuditLog>();

                try
                {
                    var recordId = RecordIds.From(context, Field, RouteKey).FirstOrDefault();
                    var access = services.GetRequiredService<ProjectRecordAccess>();

                    var userId = context.HttpContext.User.GetLoggedInUserId<string>();
                    var siteId = services.GetRequiredService<GlobalVariable>().SiteId;

                    var allowed = recordId == null
                        || (Rule.HasValue
                            ? await access.CanAccessAsync(Rule.Value, userId, siteId, recordId)
                            : await (Task<bool>)typeof(ProjectRecordAccess)
                                .GetMethods().First(m => m.Name == nameof(ProjectRecordAccess.CanAccessAsync) && m.IsGenericMethodDefinition)
                                .MakeGenericMethod(EntityType)
                                .Invoke(access, new object[] { userId, siteId, recordId }));

                    if (!allowed)
                    {
                        var referenceId = auditLog.Record(context.HttpContext, RecordIds.Endpoint(context), "RECORD_OUT_OF_SCOPE",
                            enforce ? AuthorizationOutcome.Denied : AuthorizationOutcome.WouldDeny);

                        if (enforce)
                        {
                            context.Result = RecordIds.NotFound(referenceId);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var referenceId = auditLog.TryWrite("Record scope check failed", ex.ToString());
                    if (enforce)
                    {
                        context.Result = RecordIds.NotFound(referenceId);
                        return;
                    }
                }
            }

            await next();
        }
    }

    internal static class RecordIds
    {
        public static List<string> From(ActionExecutingContext context, string field, string routeKey)
        {
            if (string.IsNullOrEmpty(field))
            {
                var value = context.ActionArguments.TryGetValue(routeKey, out var argument)
                    ? argument?.ToString()
                    : context.RouteData.Values[routeKey]?.ToString();

                return string.IsNullOrWhiteSpace(value) ? new List<string>() : new List<string> { value };
            }

            foreach (var argument in context.ActionArguments.Values)
            {
                var property = argument?.GetType().GetProperty(field);
                if (property == null)
                    continue;

                return property.GetValue(argument) switch
                {
                    string single when !string.IsNullOrWhiteSpace(single) => new List<string> { single },
                    IEnumerable many and not string => many.OfType<string>().Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList(),
                    _ => new List<string>()
                };
            }

            return new List<string>();
        }

        public static string Endpoint(ActionExecutingContext context)
        {
            var action = context.ActionDescriptor as ControllerActionDescriptor;
            return $"{context.HttpContext.Request.Method} {action?.ControllerName}.{action?.ActionName}";
        }

        public static Microsoft.AspNetCore.Mvc.ObjectResult NotFound(string referenceId)
        {
            return new Microsoft.AspNetCore.Mvc.ObjectResult(new { code = "RECORD_NOT_FOUND", message = "The requested record was not found.", referenceId })
            {
                StatusCode = StatusCodes.Status404NotFound
            };
        }
    }
}
