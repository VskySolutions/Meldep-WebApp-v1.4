using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Vsky.Models;
using Vsky.Services.ReportSetting;

namespace Vsky.Services.PowerBI
{
    public class ConfigValidatorService
    {
        private readonly IReportSettingsService _reportSettingsService;
        public ConfigValidatorService(IReportSettingsService reportSettingsService)
        {
            _reportSettingsService = reportSettingsService;
        }
        
        public static readonly string Username = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["WEB_Domain"];
        public static readonly string Password = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["WEB_Domain"];

        /// <summary>
        /// Check if web.config embed parameters have valid values.
        /// </summary>
        /// <returns>Null if web.config parameters are valid, otherwise returns specific error string.</returns>
        public  string GetWebConfigErrors()
        {
            var SettingData =  GetReportSettingsForSite("04086f10-8f09-4e0c-b5c0-c827a244addd");

            string ApplicationId = SettingData.Result.ApplicationId;
            Guid WorkspaceId = Guid.Parse(SettingData.Result.WorkspaceId);
            string AuthenticationType = SettingData.Result.AuthenticationType;
            var ApplicationSecret = SettingData.Result.ApplicationSecret;
            var Tenant = SettingData.Result.Tenant;

            string message = null;
            Guid result;

            // Application Id must have a value.
            if (string.IsNullOrWhiteSpace(ApplicationId))
            {
                message = "ApplicationId is empty. please register your application as Native app in https://dev.powerbi.com/apps and fill client Id in web.config.";
            }
            // Application Id must be a Guid object.
            else if (!Guid.TryParse(ApplicationId, out result))
            {
                message = "ApplicationId must be a Guid object. please register your application as Native app in https://dev.powerbi.com/apps and fill application Id in web.config.";
            }
            // Workspace Id must have a value.
            else if (WorkspaceId == Guid.Empty)
            {
                message = "WorkspaceId is empty or not a valid Guid. Please fill its Id correctly in web.config";
            }
            // Report Id must have a value.
            //else if (ReportId == Guid.Empty)
            //{
            //    message = "ReportId is empty or not a valid Guid. Please fill its Id correctly in web.config";
            //}
            else if (AuthenticationType.Equals("masteruser", StringComparison.InvariantCultureIgnoreCase))
            {
                //// Username must have a value.
                //if (string.IsNullOrWhiteSpace(Username))
                //{
                //    message = "Username is empty. Please fill Power BI username in web.config";
                //}

                //// Password must have a value.
                //if (string.IsNullOrWhiteSpace(Password))
                //{
                //    message = "Password is empty. Please fill password of Power BI username in web.config";
                //}
            }
            else if (AuthenticationType.Equals("serviceprincipal", StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(ApplicationSecret))
                {
                    message = "ApplicationSecret is empty. please register your application as Web app and fill appSecret in web.config.";
                }
                // Must fill tenant Id
                else if (string.IsNullOrWhiteSpace(Tenant))
                {
                    message = "Invalid Tenant. Please fill Tenant ID in Tenant under web.config";
                }
            }
            else
            {
                message = "Invalid authentication type";
            }

            return message;
        }

        private static Guid GetParamGuid(string param)
        {
            Guid paramGuid = Guid.Empty;
            Guid.TryParse(param, out paramGuid);
            return paramGuid;
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
