using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.SitesModifiedLog
{
    public interface ISitesModifiedLogsService
    {
        IPagedList<SitesModifiedLogs> GetAllSitesModifiedLogs(string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);

        #region GetSitesModifiedLogsById
        Task<SitesModifiedLogs> GetSitesModifiedLogsById(string id);
        #endregion

        #region GetSitesModifiedLogDetailsById
        Task<SitesModifiedLogs> GetSitesModifiedLogDetailsById(string id);
        Task<List<SitesModifiedLogs>> GetAllSitesModifiedLogDetailsById(string SiteId, string subModuleId, string columnName);
        #endregion

        void AddSiteModifiedLogs(string SiteId, string TableName, string ModuleId, string ModuleName, string SubModuleId, string SubModule, string ColumnName, string ColumnValue, string LoggedUserId, DateTime GetDateTime);

        #region InsertSitesModifiedLog
        void InsertSitesModifiedLog(SitesModifiedLogs entity);
        #endregion

        #region UpdateSitesModifiedLog
        void UpdateSitesModifiedLog(SitesModifiedLogs entity);
        #endregion
    }
}
