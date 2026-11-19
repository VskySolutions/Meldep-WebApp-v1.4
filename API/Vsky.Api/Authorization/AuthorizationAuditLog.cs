using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Vsky.Api.Extensions;
using Vsky.Models;
using LogLevel = Vsky.Models.LogLevel;

namespace Vsky.Api.Authorization
{
    public enum AuthorizationOutcome
    {
        Denied,
        WouldDeny,
        Bypassed
    }

    /// <summary>
    /// Records authorization decisions in the existing log table. Rows are limited per user, site,
    /// endpoint and reason, so a busy page can't flood the table.
    /// </summary>
    public class AuthorizationAuditLog
    {
        private static readonly TimeSpan AuditLogInterval = TimeSpan.FromHours(1);
        private static readonly TimeSpan DenyLogInterval = TimeSpan.FromMinutes(1);

        private readonly SecurityLog _securityLog;
        private readonly IMemoryCache _cache;
        private readonly GlobalVariable _globalVariable;

        public AuthorizationAuditLog(SecurityLog securityLog, IMemoryCache cache, GlobalVariable globalVariable)
        {
            _securityLog = securityLog;
            _cache = cache;
            _globalVariable = globalVariable;
        }

        /// <summary>Returns the log row's ID as a reference, or null if this one wasn't written.</summary>
        public string Record(HttpContext httpContext, string endpoint, string code, AuthorizationOutcome outcome, string[] links = null, string level = null, string requestedSiteId = null)
        {
            var userId = httpContext.User?.Identity?.IsAuthenticated == true
                ? httpContext.User.GetLoggedInUserId<string>()
                : null;

            // Before the limit below: a denial skipped by the limit still needs no other row.
            if (outcome == AuthorizationOutcome.Denied)
                RequestLogState.For(httpContext).EventRecorded = true;

            var logKey = $"auth-log:{outcome}:{code}:{userId}:{_globalVariable.SiteId}:{requestedSiteId}:{endpoint}";
            if (_cache.TryGetValue(logKey, out _))
                return null;

            _cache.Set(logKey, true, outcome == AuthorizationOutcome.Denied ? DenyLogInterval : AuditLogInterval);

            var message = outcome switch
            {
                AuthorizationOutcome.Denied => "Authorization denied",
                AuthorizationOutcome.Bypassed => "Authorization bypass",
                _ => "Authorization denied: Unauthorized module access"
            };

            return TryWrite(message, JsonSerializer.Serialize(new
            {
                decision = outcome.ToString(),
                code,
                userId,
                siteId = _globalVariable.SiteId,
                requestedSiteId,
                endpoint,
                links,
                level
            }));
        }

        public string TryWrite(string message, string details)
        {
            return _securityLog.Write(LogLevel.Warning, message, details);
        }

        public static ObjectResult Denied(int status, string code, string referenceId, string message = null)
        {
            message ??= code switch
            {
                "SITE_NOT_MEMBER" => "You do not have access to this site.",
                "SITE_SCOPE_MISMATCH" => "This request doesn't match your active site.",
                _ => "You do not have permission to perform this action."
            };

            return new ObjectResult(new { code, message, referenceId }) { StatusCode = status };
        }
    }
}
