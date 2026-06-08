using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ReportRoleGroupMappings
{
    public interface IReportRoleGroupMappingService
    {
        #region GetAllReportGroupRoles
        Task<IPagedList<ReportRoleGroupMapping>> GetAllReportGroupRoles(string SiteId, string SearchText, List<string> siteRoleIds, List<string> reportGroupIds, string sortBy,
           bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetReportGroupRoleById
        Task<ReportRoleGroupMapping> GetReportGroupRoleById(string id);
        #endregion

        #region GetAssignedReportGroupNames
        Task<List<string>> GetAssignedReportGroupNames(string siteId, string siteRoleId, List<string> reportGroupIds);
        #endregion

        #region GetReportGroupsByRoles
        Task<List<ReportRoleGroupMapping>> GetReportGroupsByRoles(string siteId, string[] siteRoleIds);
        #endregion

        #region InsertReportGroupRole
        void InsertReportGroupRole(ReportRoleGroupMapping entity);
        #endregion

        #region UpdateReportGroupRole
        void UpdateReportGroupRole(ReportRoleGroupMapping entity);
        #endregion

        #region DeleteReportGroupRole
        void DeleteReportGroupRole(ReportRoleGroupMapping entities);
        #endregion
    }
}
