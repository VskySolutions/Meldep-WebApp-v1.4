using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
    /// The central check, run on every controller action before model binding, so a denied request
    /// never reaches action code. In Audit mode it only logs what it would deny.
    /// [RequireSiteAdmin] and [RequireSystemAdmin] enforce themselves and are always on.
    /// </summary>
    public class AuthorizationEnforcementFilter : IAsyncAuthorizationFilter
    {
        private const string ServiceKeyHeader = "X-Service-Key";

        private readonly IPermissionService _permissionService;
        private readonly GlobalVariable _globalVariable;
        private readonly SecurityAuthorizationOptions _options;
        private readonly AuthorizationAuditLog _auditLog;
        private readonly AdminAccess _adminAccess;

        public AuthorizationEnforcementFilter(
            IPermissionService permissionService,
            GlobalVariable globalVariable,
            IOptions<SecurityAuthorizationOptions> options,
            AuthorizationAuditLog auditLog,
            AdminAccess adminAccess)
        {
            _permissionService = permissionService;
            _globalVariable = globalVariable;
            _options = options.Value;
            _auditLog = auditLog;
            _adminAccess = adminAccess;

        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (_options.Mode == AuthorizationMode.Off || context.ActionDescriptor is not ControllerActionDescriptor action)
                return;

            var policy = EndpointPolicies.For(action);

            try
            {
                await CheckAsync(context, action, policy);
            }
            catch (Exception ex)
            {
                // A failure in the check itself must not break requests while only auditing.
                var failClosed = IsEnforced(policy) || _options.SiteGateMode == AuthorizationMode.Enforce;
                var referenceId = _auditLog.TryWrite(failClosed ? "Authorization check failed" : "Authorization audit check failed", ex.ToString());
                if (failClosed)
                    context.Result = AuthorizationAuditLog.Denied(StatusCodes.Status403Forbidden, "PERMISSION_DENIED", referenceId);
            }
        }

        private async Task CheckAsync(AuthorizationFilterContext context, ControllerActionDescriptor action, EndpointPolicy policy)
        {
            switch (policy.Tier)
            {
                case EndpointTier.Public:
                    // Anonymous callers must not choose a site by header once everything is enforced.
                    if (IsEverythingEnforced)
                        _globalVariable.SiteId = string.Empty;
                    return;

                case EndpointTier.Service:
                    if (!HasValidServiceKey(context.HttpContext))
                        Decide(context, action, policy, StatusCodes.Status403Forbidden, "SERVICE_KEY_INVALID", IsEverythingEnforced);
                    else if (IsEverythingEnforced)
                        _globalVariable.SiteId = string.Empty;
                    return;

                case EndpointTier.Admin:
                    return;
            }

            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                Decide(context, action, policy, StatusCodes.Status401Unauthorized, "UNAUTHENTICATED", IsEverythingEnforced);
                return;
            }

            if (policy.Tier == EndpointTier.Authenticated)
                return;

            var userId = context.HttpContext.User.GetLoggedInUserId<string>();
            if (await _adminAccess.IsSystemAdminAsync(userId))
            {
                return;
            }
            var snapshot = await _permissionService.GetSnapshotAsync(userId, _globalVariable.SiteId);

            if (policy.Tier == null)
            {
                // Unclassified: the scan reports these, so only the site check is recorded per request.
                if (!snapshot.IsSiteMember)
                    Decide(context, action, policy, StatusCodes.Status403Forbidden, "SITE_NOT_MEMBER", _options.SiteGateMode == AuthorizationMode.Enforce);

                if (context.Result == null && IsEverythingEnforced)
                    Decide(context, action, policy, StatusCodes.Status403Forbidden, "UNCLASSIFIED_ACTION", true);
                return;
            }

            if (!snapshot.IsSiteMember)
            {
                Decide(context, action, policy, StatusCodes.Status403Forbidden, "SITE_NOT_MEMBER", _options.SiteGateMode == AuthorizationMode.Enforce);
                return;
            }

            if (policy.Tier == EndpointTier.Menu
                && !snapshot.Allows(policy.Links, policy.ResolveLevel(context.HttpContext.Request.Method)))
            {
                Decide(context, action, policy, StatusCodes.Status403Forbidden, "PERMISSION_DENIED", IsEnforced(policy));
            }
        }

        private bool IsEverythingEnforced => _options.Mode == AuthorizationMode.Enforce && _options.EnforceAllLinks;

        private bool IsEnforced(EndpointPolicy policy)
        {
            if (_options.Mode != AuthorizationMode.Enforce)
                return false;

            if (_options.EnforceAllLinks)
                return true;

            var enforced = (_options.EnforcedLinks ?? Array.Empty<string>()).Select(MenuLink.Normalize);
            return policy.Links.Select(MenuLink.Normalize).Any(link => enforced.Contains(link));
        }

        private bool HasValidServiceKey(HttpContext httpContext)
        {
            var expected = _options.ServiceKey;
            // Some inbound webhooks can't set headers, so those may pass it on their URL instead.
            var supplied = httpContext.Request.Headers[ServiceKeyHeader].ToString();
            if (string.IsNullOrEmpty(supplied))
                supplied = httpContext.Request.Query["serviceKey"].ToString();
            if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(supplied))
                return false;

            // Compares hashes, so the check takes the same time wherever the values differ.
            return CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(expected)),
                SHA256.HashData(Encoding.UTF8.GetBytes(supplied)));
        }

        private void Decide(AuthorizationFilterContext context, ControllerActionDescriptor action, EndpointPolicy policy, int status, string code, bool enforce)
        {
            var request = context.HttpContext.Request;

            var referenceId = _auditLog.Record(
                context.HttpContext,
                $"{request.Method} {action.ControllerName}.{action.ActionName}",
                code,
                enforce ? AuthorizationOutcome.Denied : AuthorizationOutcome.WouldDeny,
                policy.Links,
                policy.Tier == EndpointTier.Menu ? policy.ResolveLevel(request.Method).ToString() : null);

            if (enforce)
                context.Result = AuthorizationAuditLog.Denied(status, code, referenceId, DescribeMissingSite(code));
        }

        // Says why the site check failed, so a client such as Swagger or Postman can tell a wrong header
        // from a real lack of access. The code stays SITE_NOT_MEMBER so the web app reacts as before.
        private string DescribeMissingSite(string code)
        {
            if (code != "SITE_NOT_MEMBER")
                return null;

            // Empty only when no header was sent and the user can act in no site (see OnTokenValidated).
            if (string.IsNullOrWhiteSpace(_globalVariable.SiteId))
                return "You do not have access to any site.";

            // Site IDs are GUIDs. Anything else, such as the token pasted into the site field, can't be one.
            if (!Guid.TryParse(_globalVariable.SiteId, out _))
                return "The X-Site-Id header doesn't contain a site ID. Send a site ID from GET /authorization/my-sites, or leave the header out to use your current site.";

            return null;
        }
    }
}
