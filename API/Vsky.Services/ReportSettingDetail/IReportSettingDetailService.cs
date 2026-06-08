using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ReportSettingDetail
{
    public interface IReportSettingDetailService
    {

        #region GetAllReportsDetails
        // Title: GetAllReportsDetails
        Task<IPagedList<ReportSettingsDetails>> GetAllReportsDetails(
            string SiteId, 
            string userId, 
            List<string> allowedReportGroupIds, 
            string SearchText, 
            List<string> reportIds,
            string reportGroupId,
            string sortBy, 
            bool descending, 
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false,
            bool showAllReports = false
       );
        #endregion

        #region GetReportSettingsDetailsById
        // Title: GetReportSettingsDetailsById
        Task<ReportSettingsDetails> GetById(string id);
        #endregion

        #region GetReportByName
        Task<ReportSettingsDetails> GetReportByName(string siteId, string reportName, string id = null);
        #endregion

        #region GetAllReportListForDropdown
        Task<List<ReportSettingsDetails>> GetAllReportListForDropdown(string SiteId, List<string> reportGroupIdsForRoles, string LoggedUserId, bool showAllReports = false);
        #endregion

        #region AddUpdatReportDetails
        Task<bool> AddUpdatReportDetails(string reportSettingId, string reportId, string reportName, string description, DateTime GetDateTime);
        #endregion

        #region IndsertReportSettingDetails
        // Title: IndsertReportSettingDetails
        void IndsertReportSettingDetails(ReportSettingsDetails entity);
        #endregion

        #region UpdateReportSettingsDetails
        // Title: UpdateReportSettingsDetails
        void UpdateReportSettingDetails(ReportSettingsDetails entity);
        #endregion

        #region DeleteReport
        void DeleteReport(ReportSettingsDetails entity);
        #endregion
    }
}
