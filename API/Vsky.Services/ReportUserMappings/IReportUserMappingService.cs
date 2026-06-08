using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;


namespace Vsky.Services.ReportUserMappings
{
    public interface IReportUserMappingService
    {
        #region GetAllReportsForUserPermission
        IPagedList<ReportSettingsDetails> GetAllReportsForUserPermission(string SiteId, string SearchText, List<string> reportIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetReportUserByReportSettingsDetailId
        Task<List<ReportUserMapping>> GetReportUserByReportSettingsDetailId(string SiteId, string reportSettingsDetailId);
        #endregion

        #region GetReportUserById
        Task<ReportUserMapping> GetReportUserById(string id);
        #endregion

        #region GetReportByUserIdandReportSettingsDetailId
        Task<ReportUserMapping> GetReportByUserIdandReportSettingsDetailId(string SiteId, string userId, string reportSettingsDetailId);
        #endregion

        #region InsertReportUser
        void InsertReportUser(ReportUserMapping entity);
        #endregion

        #region UpdateProjectUser
        void UpdateReportUser(ReportUserMapping entity);
        #endregion

    }

}
