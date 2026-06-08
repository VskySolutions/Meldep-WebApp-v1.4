using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.SitesModulesMenu
{
    public interface ISitesModulesMenusService
    {
        #region GetAllModuleMenusForDashboard
        Task<IList<SitesModulesMenus>> GetAllModuleMenusForDashboard(string SiteId);
        #endregion

        #region GetSitesModulesMenusById
        Task<IList<SitesModulesMenus>> GetSitesModulesMenusByMenuId(string menuId);
        #endregion

        #region GetSitesModulesMenusBySiteModuleId
        Task<IList<SitesModulesMenus>> GetSitesModulesMenusBySiteModuleId(string siteModuleId);
        Task<IList<SitesModulesMenus>> GetSitesModulesMenusBySiteModuleIdForDropdown(string siteId, string siteModuleIds);
        #endregion

        #region GetSitesModulesMenusBySiteId
        Task<IList<SitesModulesMenus>> GetSitesModulesMenusBySiteId(string siteId);
        #endregion

        #region GetLandingPageBySiteId
        Task<string> GetLandingPageBySiteId(string siteId);
        #endregion

        #region GetSiteMenu
        Task<SitesModulesMenus> GetSiteMenu(string siteId, string menuId);
        #endregion

        #region InsertSitesModulesMenu
        void InsertSitesModulesMenu(SitesModulesMenus entity);
        #endregion

        #region UpdateSitesModulesMenu
        void UpdateSitesModulesMenu(SitesModulesMenus entity);
        #endregion

        #region DeleteSitesModulesMenus
        void DeleteSitesModulesMenus(SitesModulesMenus entity);
        #endregion
    }
}
