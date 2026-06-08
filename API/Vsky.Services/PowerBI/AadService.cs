using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MimeKit.Cryptography;

using Microsoft.Identity.Client;
using Vsky.Services.ReportSetting;
using Vsky.Models;
using Microsoft.PowerBI.Api.Models;
using Microsoft.AspNetCore.Http;

namespace Vsky.Services.PowerBI
{
    public class AadService
    {
        private readonly IReportSettingsService _reportSettingsService;
        public AadService(IReportSettingsService reportSettingsService)
        {
            _reportSettingsService = reportSettingsService;
        }
        //private static readonly string m_authorityUrl = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["authorityUrl"];
        //private static readonly string m_authorityUrl = "https://login.microsoftonline.com/organizations/";

        //private static readonly string[] m_scope = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings").GetSection("scope").Get<string[]>();
        //private static readonly string[] m_scope = { "https://analysis.windows.net/powerbi/api/.default"};
        /// <summary>
        /// Get Access token
        /// </summary>
        /// <returns>Access token</returns>
        public async Task<string> GetAccessToken(string SiteId)
        {
            var SettingData = GetReportSettingsForSite(SiteId);

            string m_authorityUrl = SettingData.Result.AuthorityUrl;
            string[] m_scope = { SettingData.Result.Scope };


            AuthenticationResult authenticationResult = null;
            if (SettingData.Result.AuthenticationType.Equals("masteruser", StringComparison.InvariantCultureIgnoreCase))
            {
                IPublicClientApplication clientApp = PublicClientApplicationBuilder
                                                                    .Create(SettingData.Result.ApplicationId)
                                                                    .WithAuthority(m_authorityUrl)
                                                                    .Build();
                var userAccounts = await clientApp.GetAccountsAsync();

                try
                {
                    authenticationResult = await clientApp.AcquireTokenSilent(m_scope, userAccounts.FirstOrDefault()).ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    SecureString secureStringPassword = new SecureString();
                    foreach (var key in ConfigValidatorService.Password)
                    {
                        secureStringPassword.AppendChar(key);
                    }
                    authenticationResult = await clientApp.AcquireTokenByUsernamePassword(m_scope, ConfigValidatorService.Username, secureStringPassword).ExecuteAsync();
                }
            }

            // Service Principal auth is recommended by Microsoft to achieve App Owns Data Power BI embedding
            else if (SettingData.Result.AuthenticationType.Equals("serviceprincipal", StringComparison.InvariantCultureIgnoreCase))
            {
                // For app only authentication, we need the specific tenant id in the authority url
                var tenantSpecificURL = m_authorityUrl.Replace("organizations", SettingData.Result.Tenant);

                IConfidentialClientApplication clientApp = ConfidentialClientApplicationBuilder
                                                                                .Create(SettingData.Result.ApplicationId)
                                                                                .WithClientSecret(SettingData.Result.ApplicationSecret)
                                                                                .WithAuthority(tenantSpecificURL)
                                                                                .Build();

                authenticationResult = await clientApp.AcquireTokenForClient(m_scope).ExecuteAsync();
            }

            return authenticationResult.AccessToken;
        }

        public async Task<ReportSettings> GetReportSettingsForSite(string siteId)
        {
            // Call the ReportSettingsService
            var reportSettings = await _reportSettingsService.GetBySiteId(siteId);

            // Do something with the retrieved report settings
            return reportSettings;
        }


    }
}