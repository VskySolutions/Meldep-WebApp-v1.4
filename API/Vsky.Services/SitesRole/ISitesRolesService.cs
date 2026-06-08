using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;


namespace Vsky.Services.SitesRole
{
    public interface ISitesRolesService
    {

        #region GetAllSiteRoles
        IPagedList<SitesRoles> GetAllSiteRoles(string siteId, string searchText, List<string> siteRoleIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetRolesBySiteId
        Task<IList<SitesRoles>> GetRolesBySiteId(string SiteId);
        #endregion

        #region GetSitesRoles
        Task<IList<SitesRoles>> GetSitesRoles();
        #endregion

        #region GetRolesBySiteRoleIds
        Task<IList<SitesRoles>> GetRolesBySiteRoleIds(string[] siteRoleIds);
        #endregion

        #region GetRoleIdBySiteRoleId
        Task<string> GetRoleIdBySiteRoleId(string siteRoleId);
        #endregion

        #region GetRolesBySiteRoleIds
        Task<IList<string>> GetSiteRoleIdsByRoleIds(string siteId, string[] siteRoleIds);
        #endregion

        #region InsertSitesRoles
        void InsertSitesRoles(SitesRoles entity);
        #endregion

        #region UpdateSitesRoles
        void UpdateSitesRoles(SitesRoles entity);
        #endregion

        #region DeleteSitesRoles
        void DeleteSitesRoles(SitesRoles entity);
        #endregion
    }
}
