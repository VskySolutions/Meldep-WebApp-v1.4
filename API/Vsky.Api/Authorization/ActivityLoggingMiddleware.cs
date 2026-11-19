using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Vsky.Services.Authorization;
using LogLevel = Vsky.Models.LogLevel;

namespace Vsky.Api.Authorization
{
    /// <summary>
    /// Sees the final result of every request, including ones that never reach an action filter:
    /// 401, 403, invalid models and rate limits. Everything here runs after the response, uses memory
    /// only, and never reads request bodies or query strings.
    /// </summary>
    public class ActivityLoggingMiddleware
    {
        private const int MaxResultLength = 500;
        private static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan RecordAccessWindow = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan ModuleAccessWindow = TimeSpan.FromMinutes(30);

        private readonly RequestDelegate _next;
        private readonly SecurityLog _securityLog;
        private readonly ActivityLogQueue _queue;
        private readonly IMemoryCache _cache;
        private readonly SecurityAuthorizationOptions _options;

        public ActivityLoggingMiddleware(
            RequestDelegate next,
            SecurityLog securityLog,
            ActivityLogQueue queue,
            IMemoryCache cache,
            IOptions<SecurityAuthorizationOptions> options)
        {
            _next = next;
            _securityLog = securityLog;
            _queue = queue;
            _cache = cache;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var started = Stopwatch.GetTimestamp();

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Reaches here only when the response had already started, so the handler couldn't reply.
                _securityLog.WriteError(null, ex);
                Record(context, started, context.Response.HasStarted ? context.Response.StatusCode : StatusCodes.Status500InternalServerError);
                throw;
            }

            Record(context, started, context.Response.StatusCode);
        }

        private void Record(HttpContext context, long started, int status)
        {
            try
            {
                if (!_options.ActivityLogging || HttpMethods.IsOptions(context.Request.Method))
                    return;

                var state = RequestLogState.For(context);
                if (state.EventRecorded)
                    return;

                var action = RequestLogState.ActionOf(context);
                var policy = action == null ? null : EndpointPolicies.For(action);
                var userId = SecurityLog.CurrentUserId(context);
                var path = context.Request.Path.ToString();
                var clientKey = userId ?? context.Connection.RemoteIpAddress?.ToString();

                if (status is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
                {
                    if (FirstInWindow($"activity:{status}:{clientKey}:{path}", FailureWindow))
                    {
                        var message = status == StatusCodes.Status401Unauthorized ? "Authentication failed" : "Authorization denied";
                        _securityLog.Write(LogLevel.Warning, message, Details(context, action, policy, state, started, status, userId), userId);
                    }
                    return;
                }

                var failed = status >= StatusCodes.Status400BadRequest || state.ReferenceId != null;

                if (action == null)
                {
                    // Anonymous requests to unknown URLs are mostly scanners.
                    if (failed && userId != null && FirstInWindow($"activity:{status}:{clientKey}:{path}", FailureWindow))
                        Enqueue(context, FailureLevel(status), "User activity", Details(context, action, policy, state, started, status, userId), userId);
                    return;
                }

                var isServiceCall = policy.Tier == EndpointTier.Service;
                var method = context.Request.Method;
                var isRead = !isServiceCall
                             && (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || policy.ExplicitLevel == AccessLevel.View);

                // Browsers cancel reads constantly, for example when the user leaves a page.
                if (context.RequestAborted.IsCancellationRequested && isRead)
                    return;

                if (failed)
                {
                    // Each exception has its own reference, so only plain 4xx responses are limited.
                    var isRepeatable = state.ReferenceId == null && status < StatusCodes.Status500InternalServerError;
                    if (!isRepeatable || FirstInWindow($"activity:{status}:{clientKey}:{path}", FailureWindow))
                        Enqueue(context, FailureLevel(status), "User activity", Details(context, action, policy, state, started, status, userId), userId);
                    return;
                }

                if (isRead && !policy.AlwaysLogActivity)
                {
                    RecordAccess(context, action, policy, state, started, status, userId);
                    return;
                }

                // Runs too often to be useful, and each one only renews the token.
                if (action.ControllerName == "Auth" && action.ActionName == "RefreshToken")
                    return;

                if (policy.Tier == EndpointTier.Admin)
                    _securityLog.Write(LogLevel.Warning, "Administrative change", Details(context, action, policy, state, started, status, userId), userId);
                else
                    Enqueue(context, LogLevel.Information, "User activity", Details(context, action, policy, state, started, status, userId), userId);
            }
            catch (Exception)
            {
                // A logging failure never affects the request.
            }
        }

        private void RecordAccess(HttpContext context, ControllerActionDescriptor action, EndpointPolicy policy, RequestLogState state, long started, int status, string userId)
        {
            if (userId == null)
                return;

            if (_options.LogRecordAccess
                && state.HasRouteRecord
                && FirstInWindow($"activity:record:{userId}:{action.ControllerName}:{string.Join(',', state.RecordIds)}", RecordAccessWindow))
            {
                Enqueue(context, LogLevel.Information, "Record access", Details(context, action, policy, state, started, status, userId), userId);
            }

            if (_options.LogModuleAccess
                && policy.Tier == EndpointTier.Menu
                && FirstInWindow($"activity:module:{userId}:{RequestLogState.SiteIdOf(context)}:{string.Join(',', policy.Links)}", ModuleAccessWindow))
            {
                Enqueue(context, LogLevel.Information, "Module access", Details(context, action, policy, state, started, status, userId), userId);
            }
        }

        private void Enqueue(HttpContext context, LogLevel level, string message, string details, string userId)
        {
            // Copies everything now, because HttpContext isn't valid in the background.
            _queue.Enqueue(_securityLog.CreateRow(level, message, details, userId, context));
        }

        private static LogLevel FailureLevel(int status)
        {
            return status >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning;
        }

        private bool FirstInWindow(string key, TimeSpan window)
        {
            if (_cache.TryGetValue(key, out _))
                return false;

            _cache.Set(key, true, window);
            return true;
        }

        private static string Details(HttpContext context, ControllerActionDescriptor action, EndpointPolicy policy, RequestLogState state, long started, int status, string userId)
        {
            return SecurityLog.ToJson(new
            {
                // Stays first and exact: the audit screen's site filter searches for this text.
                siteId = RequestLogState.SiteIdOf(context),
                requestId = context.TraceIdentifier,
                actor = policy?.Tier == EndpointTier.Service ? "service" : userId == null ? "anonymous" : null,
                method = context.Request.Method,
                controller = action?.ControllerName,
                action = action?.ActionName,
                activity = action?.ActionName.Humanize(LetterCasing.Title),
                module = action?.ControllerName.Humanize(LetterCasing.Title),
                links = policy?.Links.Length > 0 ? policy.Links : null,
                recordType = state.RecordIds.Count > 0 ? action?.ControllerName : null,
                recordIds = state.RecordIds.Count > 0 ? state.RecordIds : null,
                status,
                durationMs = (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds,
                referenceId = state.ReferenceId,
                cancelled = context.RequestAborted.IsCancellationRequested ? true : (bool?)null,
                result = SecurityLog.Sanitize(state.ResultMessage, MaxResultLength),
                invalidFields = state.InvalidFields?.Length > 0 ? state.InvalidFields.Select(field => SecurityLog.Sanitize(field, 100)).ToArray() : null
            });
        }
    }
}
