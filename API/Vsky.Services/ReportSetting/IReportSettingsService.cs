using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ReportSetting
{
    public interface IReportSettingsService
    {
        #region GetBySiteId
        Task<ReportSettings> GetBySiteId(string SiteId);
        #endregion
    }
}
