using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ReportSetting
{
    public class ReportSettingsService : IReportSettingsService
    {
        #region Services Initializations
        private readonly IRepository<ReportSettings> _reportSettingsRepository;
        public ReportSettingsService(IRepository<ReportSettings> reportSettingsRepository)
        {
            _reportSettingsRepository = reportSettingsRepository;
        }
        #endregion

        #region GetBySiteId
        /// <summary>
        /// GetBySiteId
        /// </summary>
        /// <param name="SiteId"></param>
        /// <returns></returns>
        public async Task<ReportSettings> GetBySiteId(string SiteId)
        {
            var query = _reportSettingsRepository.Table;

            query = query.Where(x => !x.Deleted && x.SiteId == SiteId);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion
    }
}
