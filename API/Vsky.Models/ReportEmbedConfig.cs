using Microsoft.PowerBI.Api.Models;
using System.Collections.Generic;
using System;
using Microsoft.PowerBI.Api.Models;

namespace Vsky.Api.Models
{
    public class ReportEmbedConfig
    {
        public ReportEmbedConfig()
        {
            EmbedToken = new EmbedToken();
            ReportModelList = new List<ReportEmbedConfig>();
        }
        public List<EmbedReport> EmbedReports { get; set; }

        // Embed Token for the Power BI report
        public EmbedToken EmbedToken { get; set; }

        public string ReportName { get; set; }
        public Guid ReportIds { get; set; }
        public string Description { get; set; }

        public List<ReportEmbedConfig> ReportModelList { get; set; }
    }
}
