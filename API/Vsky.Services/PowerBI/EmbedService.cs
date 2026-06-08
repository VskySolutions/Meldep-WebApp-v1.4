using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Leads;
using Vsky.Services.ReportSetting;
using Vsky.Services.ReportSettingDetail;
using Vsky.Services.Sites;
namespace Vsky.Services.PowerBI
{
    public class EmbedService
    {
        //private static readonly string urlPowerBiServiceApiRoot = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["urlPowerBiServiceApiRoot"];
        //private static readonly string urlPowerBiServiceApiRoot = "https://api.powerbi.com/";

        private readonly AadService _aadService;
        private readonly ISiteService _siteService;
        private readonly IReportSettingsService _reportSettingsService;
        private readonly IReportSettingDetailService _reportSettingDetailService;

        public EmbedService(
            AadService aadService,
            ISiteService siteService,
            IReportSettingsService reportSettingsService,
            IReportSettingDetailService reportSettingDetailService)
        {
            _aadService = aadService;
            _siteService = siteService;
            _reportSettingsService = reportSettingsService;
            _reportSettingDetailService = reportSettingDetailService;
        }

        public async Task<PowerBIClient> GetPowerBiClient(string SiteId)
        {
            var SettingData = await GetReportSettingsForSite(SiteId);
            string urlPowerBiServiceApiRoot = SettingData.UrlPowerBiServiceApiRoot;

            var tokenCredentials = new TokenCredentials(await _aadService.GetAccessToken(SiteId), "Bearer");
            return new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);
        }

        /// <summary>
        /// Get embed params for a report
        /// </summary>
        /// <returns>Wrapper object containing Embed token, Embed URL, Report Id, and Report name for single report</returns>
        public  async Task<ReportEmbedConfig> GetEmbedParams(Guid workspaceId, Guid reportId, [Optional] Guid additionalDatasetId, string siteId)
        {
            using (var pbiClient = await GetPowerBiClient(siteId))
            {
                var pbiReport = await pbiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);

                var datasetIds = new List<Guid>();
                if(pbiReport.DatasetId != null)
                    datasetIds.Add(Guid.Parse(pbiReport.DatasetId));
                var dataSourceIds = new List<Datasource>();

                if (pbiReport.ReportType == "PaginatedReport")
                {
                    try
                    {
                        var datasources = await pbiClient.Reports.GetDatasourcesInGroupAsync(workspaceId, reportId);
                        if (datasources?.Value != null && datasources.Value.Any())
                        {
                            foreach (var datasource in datasources.Value)
                            {
                                dataSourceIds.Add(datasource);
                            }
                        }
                    }
                    catch (Microsoft.Rest.HttpOperationException ex)
                    {
                        // Log the error or handle it gracefully
                        Console.WriteLine($"No datasources found or an error occurred: {ex.Response.Content}");
                    }
                }

                // Append additional dataset to the list to achieve dynamic binding later
                if (additionalDatasetId != Guid.Empty)
                {
                    datasetIds.Add(additionalDatasetId);
                }

                // Add report data for embedding
                var embedReports = new List<EmbedReport>() {
                    new EmbedReport
                    {
                        ReportId = pbiReport.Id, ReportName = pbiReport.Name, EmbedUrl = pbiReport.EmbedUrl
                    }
                };

                // Get Embed token multiple resources

                EmbedToken embedToken = null;
                if (pbiReport.ReportType == "PaginatedReport")
                {
                    embedToken = await GetEmbedTokenForPaginatedReport(reportId, datasetIds, dataSourceIds, workspaceId, siteId);
                }
                else
                {
                    embedToken = await GetEmbedTokenWithoutDataSource(reportId, datasetIds, workspaceId, siteId);
                }

                // Capture embed params
                var embedParams = new ReportEmbedConfig
                {
                    EmbedReports = embedReports,
                    EmbedToken = embedToken
                };
                return embedParams;

            }
        }

        /// <summary>
        /// Get embed params for multiple reports for a single workspace
        /// </summary>
        /// <returns>Wrapper object containing Embed token, Embed URL, Report Id, and Report name for multiple reports</returns>
        public  async Task<ReportEmbedConfig> GetEmbedParams(Guid workspaceId, IList<Guid> reportIds, [Optional] IList<Guid> additionalDatasetIds, string siteId)
        {
            // Note: This method is an example and is not consumed in this sample app

            using (var pbiClient = await GetPowerBiClient(siteId))
            {
                // Create mapping for reports and Embed URLs
                var embedReports = new List<EmbedReport>();

                // Create list of datasets
                var datasetIds = new List<Guid>();

                // Get datasets and Embed URLs for all the reports
                foreach (var reportId in reportIds)
                {
                    // Get report info
                    var pbiReport = await pbiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);

                    // Append to existing list of datasets to achieve dynamic binding later
                    datasetIds.Add(Guid.Parse(pbiReport.DatasetId));

                    // Add report data for embedding
                    embedReports.Add(new EmbedReport { ReportId = pbiReport.Id, ReportName = pbiReport.Name, EmbedUrl = pbiReport.EmbedUrl });
                }

                // Append to existing list of datasets to achieve dynamic binding later
                if (additionalDatasetIds != null)
                {
                    datasetIds.AddRange(additionalDatasetIds);
                }

                // Get Embed token multiple resources
                var embedToken = await GetEmbedTokenForGroup(workspaceId, reportIds, datasetIds, siteId);

                // Capture embed params
                var embedParams = new ReportEmbedConfig
                {
                    EmbedReports = embedReports,
                    EmbedToken = embedToken
                };

                return embedParams;
            }
        }

        /// <summary>
        /// Get Embed token for single report, multiple datasets, and an optional target workspace
        /// </summary>
        /// <returns>Embed token</returns>
        public async Task<EmbedToken> GetEmbedToken(Guid reportId, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId, string siteId)
        {
            using (var pbiClient = await GetPowerBiClient(siteId))
            {
                // Create a request for getting Embed token 
                // This method works only with new Power BI V2 workspace experience

                var tokenRequest = new GenerateTokenRequestV2(

                reports: new List<GenerateTokenRequestV2Report>() { new GenerateTokenRequestV2Report(reportId) },

                datasets: datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList(),

                targetWorkspaces: targetWorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId) } : null
                );

                // Generate Embed token
                var embedToken = await pbiClient.EmbedToken.GenerateTokenAsync(tokenRequest);

                return embedToken;
            }
        }

        /// <summary>
        /// Get Embed token for multiple reports, datasets, and an optional target workspace
        /// </summary>
        /// <returns>Embed token</returns>
        public async Task<EmbedToken> GetEmbedToken(IList<Guid> reportIds, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId, string siteId)
        {
            // Note: This method is an example and is not consumed in this sample app

            using (var pbiClient = await GetPowerBiClient(siteId))
            {
                // Convert reports to required types
                var reports = reportIds.Select(reportId => new GenerateTokenRequestV2Report(reportId)).ToList();

                // Convert datasets to required types
                var datasets = datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList();

                // Create a request for getting Embed token 
                // This method works only with new Power BI V2 workspace experience
                var tokenRequest = new GenerateTokenRequestV2(

                    datasets: datasets,

                    reports: reports,

                    targetWorkspaces: targetWorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId) } : null
                );

                // Generate Embed token
                var embedToken = await pbiClient.EmbedToken.GenerateTokenAsync(tokenRequest);

                return embedToken;
            }
        }


        /// <summary>
        /// Get Embed token for multiple reports, datasets, and optional target workspaces
        /// </summary>
        /// <returns>Embed token</returns>
        public async Task<EmbedToken> GetEmbedToken(IList<Guid> reportIds, IList<Guid> datasetIds, [Optional] IList<Guid> targetWorkspaceIds, string siteId)
        {
            // Note: This method is an example and is not consumed in this sample app

            using (var pbiClient = await GetPowerBiClient(siteId))
            {
                // Convert report Ids to required types
                var reports = reportIds.Select(reportId => new GenerateTokenRequestV2Report(reportId)).ToList();

                // Convert dataset Ids to required types
                var datasets = datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList();

                // Convert target workspace Ids to required types
                IList<GenerateTokenRequestV2TargetWorkspace> targetWorkspaces = null;
                if (targetWorkspaceIds != null)
                {
                    targetWorkspaces = targetWorkspaceIds.Select(targetWorkspaceId => new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId)).ToList();
                }

                // Create a request for getting Embed token 
                // This method works only with new Power BI V2 workspace experience
                var tokenRequest = new GenerateTokenRequestV2(

                    datasets: datasets,

                    reports: reports,

                    targetWorkspaces: targetWorkspaceIds != null ? targetWorkspaces : null
                );

                // Generate Embed token
                var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

                return embedToken;
            }
        }


        public async Task<EmbedToken> GetEmbedTokenForGroup(Guid workspaceId, IList<Guid> reportIds, IList<Guid> datasetIds, string siteId)
        {
            using (var pbiClient = await GetPowerBiClient(siteId))
            {
                // Map reports
                var reports = reportIds.Select(id => new GenerateTokenRequestV2Report(id)).ToList();

                // Map datasets
                var datasets = datasetIds.Select(id => new GenerateTokenRequestV2Dataset(id.ToString())).ToList();

                // Create the token request
                var tokenRequest = new GenerateTokenRequestV2
                {
                    Reports = reports,
                    Datasets = datasets,
                    TargetWorkspaces = new List<GenerateTokenRequestV2TargetWorkspace>
            {
                new GenerateTokenRequestV2TargetWorkspace(workspaceId)
            }
                };
                Console.WriteLine($"Error: {tokenRequest}");
                try
                {
                    // Use GenerateTokenAsync
                    return await pbiClient.EmbedToken.GenerateTokenAsync(tokenRequest);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error generating token: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Get embed params for a dashboard
        /// </summary>
        /// <returns>Wrapper object containing Embed token, Embed URL for single dashboard</returns>
        public async Task<DashboardEmbedConfig> EmbedDashboard(Guid workspaceId, string siteId)
        {
            // Create a Power BI Client object. It will be used to call Power BI APIs.
            using (var client = await GetPowerBiClient(siteId))
            {
                // Get a list of dashboards.
                var dashboards = await client.Dashboards.GetDashboardsInGroupAsync(workspaceId);

                // Get the first report in the workspace.
                var dashboard = dashboards.Value.FirstOrDefault();

                if (dashboard == null)
                {
                    throw new NullReferenceException("Workspace has no dashboards");
                }

                // Generate Embed Token.
                var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                var tokenResponse = await client.Dashboards.GenerateTokenInGroupAsync(workspaceId, dashboard.Id, generateTokenRequestParameters);

                if (tokenResponse == null)
                {
                    throw new NullReferenceException("Failed to generate embed token");
                }

                // Generate Embed Configuration.
                var dashboardEmbedConfig = new DashboardEmbedConfig
                {
                    EmbedToken = tokenResponse,
                    EmbedUrl = dashboard.EmbedUrl,
                    DashboardId = dashboard.Id
                };

                return dashboardEmbedConfig;
            }
        }

        public async Task<ReportEmbedConfig> GetEmbedParamsForAllReports(Guid workspaceId, string siteId)
        {
            using (var pbiClient = await GetPowerBiClient(siteId))
            {
                // Get all reports in the workspace
                var pbiReports = pbiClient.Reports.GetReports(workspaceId);

                // Create a list to hold embed report configurations
                var embedReports = new List<EmbedReport>();
                var datasetIds = new List<Guid>();
                var reportIds = new List<Guid>();

                foreach (var pbiReport in pbiReports.Value)
                {
                    // Add report details
                    embedReports.Add(new EmbedReport
                    {
                        ReportId = pbiReport.Id,
                        ReportName = pbiReport.Name,
                        EmbedUrl = pbiReport.EmbedUrl
                    });

                    // Collect dataset IDs for embedding
                    datasetIds.Add(Guid.Parse(pbiReport.DatasetId));
                    reportIds.Add(pbiReport.Id);
                }

                // Get an embed token for all reports and datasets
                var embedToken = await GetEmbedToken(reportIds, datasetIds, workspaceId, siteId);

                // Create embed parameters
                var embedParams = new ReportEmbedConfig
                {
                    EmbedReports = embedReports,
                    EmbedToken = embedToken
                };

                return embedParams;
            }
        }

        public async Task<EmbedToken> GetEmbedAllToken(List<Guid> datasetIds, Guid workspaceId, string siteId)
        {
            using (var pbiClient = await GetPowerBiClient(siteId))
            {
                // Create embed token request parameters
                var embedTokenRequestParameters = new GenerateTokenRequestV2
                {
                    Datasets = datasetIds.Select(id => new GenerateTokenRequestV2Dataset { Id = id.ToString() }).ToList(), // Pass GUID directly
                    TargetWorkspaces = workspaceId != Guid.Empty
                        ? new List<GenerateTokenRequestV2TargetWorkspace> { new GenerateTokenRequestV2TargetWorkspace { Id = workspaceId } }
                        : null
                };

                try
                {
                    // Generate token
                    return await pbiClient.EmbedToken.GenerateTokenAsync(embedTokenRequestParameters);
                }
                catch (HttpOperationException exc)
                {
                    // Log the detailed error and rethrow
                    var errorDetails = $"Status: {exc.Response.StatusCode} ({(int)exc.Response.StatusCode})\r\n" +
                                       $"Response: {exc.Response.Content}\r\n" +
                                       $"RequestId: {exc.Response.Headers["RequestId"].FirstOrDefault()}";
                    throw new Exception($"Power BI Embed Token Error: {errorDetails}");
                }
            }
        }

        public async Task<List<Guid>> GetAllReportIds(Guid WorkspaceId, string siteId)
        {
            using (var pbiClient = await GetPowerBiClient(siteId))
            {
                var reportIds = new List<Guid>();
                var pbiReports = pbiClient.Reports.GetReports(WorkspaceId);
                foreach (var pbiReport in pbiReports.Value)
                {
                    // Collect dataset IDs for embedding
                    reportIds.Add(pbiReport.Id);
                }
                return reportIds;
            }
        }

        public async Task<List<ReportEmbedConfig>> GetAllReportData(Guid WorkspaceId, string siteId)
        {
            var reportList = new List<ReportEmbedConfig>(); // Initialize the list to store report data
            using (var pbiClient = await GetPowerBiClient(siteId))
            {
                // Asynchronously fetch the reports for the specified workspace
                var pbiReports = await pbiClient.Reports.GetReportsInGroupAsync(WorkspaceId);
                var reportSettingDetails = await GetReportSettingsForSite(siteId);
                var GetDateTime = _siteService.GetDateTime(siteId);

                // Iterate through the reports and populate the list
                foreach (var pbiReport in pbiReports.Value)
                {
                    var reportEmbedConfig = new ReportEmbedConfig
                    {
                        ReportName = pbiReport.Name,
                        ReportIds = pbiReport.Id,
                        Description = pbiReport.Description,
                    };
                    await _reportSettingDetailService.AddUpdatReportDetails(reportSettingDetails.Id, pbiReport.Id.ToString(), pbiReport.Name, pbiReport.Description, GetDateTime);
                    reportList.Add(reportEmbedConfig); // Add the report to the list
                }
            }

            return reportList; // Return the populated list
        }

        public async Task<EmbedToken> GetEmbedTokenForPaginatedReport(Guid reportId, IList<Guid> datasetIds, IList<Datasource> dataSourceIds, Guid workspaceId, string siteId)
        {
            using (var pbiClient = await GetPowerBiClient(siteId))
            {
                // Prepare the DatasourceIdentities
                var datasourceIdentities = dataSourceIds.Select(datasource =>
                {
                    // Map DatasourceIdentity from Datasource
                    return new DatasourceIdentity(
                        identityBlob: datasource.DatasourceId.ToString(), // Set the identityBlob to the DatasourceId
                        datasources: new List<DatasourceSelector>
                        {
                    new DatasourceSelector(
                        datasourceType: datasource.DatasourceType, // e.g., SQL, Oracle
                        connectionDetails: datasource.ConnectionDetails // Connection details (server, database, etc.)
                    )
                        }
                    );
                }).ToList();

                // Create token request for Paginated Report
                var tokenRequest = new GenerateTokenRequestV2(
                    reports: new List<GenerateTokenRequestV2Report>{
                        new GenerateTokenRequestV2Report(reportId)
                    },
                    datasets: datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList(),
                    targetWorkspaces: new List<GenerateTokenRequestV2TargetWorkspace>
                    {
                        new GenerateTokenRequestV2TargetWorkspace(workspaceId)
                    },
                    datasourceIdentities: null
                );

                // Generate the embed token
                try
                {
                    var embedToken = await pbiClient.EmbedToken.GenerateTokenAsync(tokenRequest);
                    return embedToken;
                }
                catch (Microsoft.Rest.HttpOperationException ex)
                {
                    Console.WriteLine($"Error generating token: {ex.Response.Content}");
                    throw; // Rethrow the exception for further handling
                }
            }
        }

        /// <summary>
        /// Get Embed token for single report, multiple datasets, and an optional target workspace
        /// </summary>
        /// <returns>Embed token</returns>
        public async Task<EmbedToken> GetEmbedTokenWithoutDataSource(Guid reportId, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId, string siteId)
        {
            using (var pbiClient = await GetPowerBiClient(siteId))
            {
                // Create a request for getting Embed token 
                // This method works only with new Power BI V2 workspace experience
                var tokenRequest = new GenerateTokenRequestV2(

                reports: new List<GenerateTokenRequestV2Report>() { new GenerateTokenRequestV2Report(reportId) },

                datasets: datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList(),

                targetWorkspaces: targetWorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId) } : null
                );

                // Generate Embed token
                var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

                return embedToken;
            }
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
