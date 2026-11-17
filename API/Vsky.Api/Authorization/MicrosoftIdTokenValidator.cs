using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Vsky.Api.Authorization
{
    /// <summary>
    /// Validates a Microsoft ID token for /auth/mslogin: signature against Microsoft's published
    /// keys, lifetime, audience (this app's client ID) and issuer (an allowed tenant only).
    ///
    /// The tenant allowlist is essential. The SPA signs in through the "organizations" authority,
    /// so a token from any Azure AD tenant is correctly signed, and whoever controls a tenant
    /// controls the names inside its tokens. Fails closed when the configuration is missing.
    ///
    /// Configuration (appsettings.json):
    ///   "MicrosoftLogin": { "ClientId": "...", "AllowedTenantIds": [ "..." ] }
    /// </summary>
    public class MicrosoftIdTokenValidator
    {
        private const string MetadataAddress =
            "https://login.microsoftonline.com/organizations/v2.0/.well-known/openid-configuration";

        private readonly string _clientId;
        private readonly HashSet<string> _allowedTenantIds;
        private readonly ILogger<MicrosoftIdTokenValidator> _logger;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        public MicrosoftIdTokenValidator(IConfiguration configuration, ILogger<MicrosoftIdTokenValidator> logger)
        {
            _logger = logger;
            _clientId = configuration["MicrosoftLogin:ClientId"];
            _allowedTenantIds = new HashSet<string>(
                configuration.GetSection("MicrosoftLogin:AllowedTenantIds").Get<string[]>() ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                MetadataAddress,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = true });
        }

        /// <summary>Returns the sign-in name from a valid token, or null if the token is rejected.</summary>
        public async Task<string> ValidateAndGetEmailAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(_clientId) || _allowedTenantIds.Count == 0)
            {
                _logger.LogError("Microsoft login rejected: MicrosoftLogin:ClientId or AllowedTenantIds is not configured.");
                return null;
            }

            try
            {
                var token = await ValidateAsync(idToken);
                return new[] { "preferred_username", "email", "upn", "unique_name" }
                    .Select(type => token.Claims.FirstOrDefault(c => c.Type == type)?.Value)
                    .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            }
            catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
            {
                _logger.LogWarning("Microsoft login rejected: {Reason}", ex.GetType().Name);
                return null;
            }
        }

        private async Task<JwtSecurityToken> ValidateAsync(string idToken)
        {
            try
            {
                return await ValidateWithCurrentKeysAsync(idToken);
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                // Microsoft rotates its signing keys, so refresh once and retry.
                _configurationManager.RequestRefresh();
                return await ValidateWithCurrentKeysAsync(idToken);
            }
        }

        private async Task<JwtSecurityToken> ValidateWithCurrentKeysAsync(string idToken)
        {
            var configuration = await _configurationManager.GetConfigurationAsync(CancellationToken.None);
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = configuration.SigningKeys,
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
                ValidateAudience = true,
                ValidAudience = _clientId,
                ValidateIssuer = true,
                IssuerValidator = ValidateIssuer
            };

            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            handler.ValidateToken(idToken, parameters, out var validatedToken);
            return (JwtSecurityToken)validatedToken;
        }

        private string ValidateIssuer(string issuer, SecurityToken securityToken, TokenValidationParameters parameters)
        {
            var tenantId = (securityToken as JwtSecurityToken)?.Claims.FirstOrDefault(c => c.Type == "tid")?.Value;

            var issuerMatchesTenant = tenantId != null
                && (string.Equals(issuer, $"https://login.microsoftonline.com/{tenantId}/v2.0", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(issuer, $"https://sts.windows.net/{tenantId}/", StringComparison.OrdinalIgnoreCase));

            if (!issuerMatchesTenant || !_allowedTenantIds.Contains(tenantId))
                throw new SecurityTokenInvalidIssuerException("The token's tenant is not allowed.");

            return issuer;
        }
    }
}
