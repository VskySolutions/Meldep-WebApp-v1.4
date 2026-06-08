using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.SitesModule
{
    public interface ISitesModulesService
    {
        #region GetSiteModuleById
        Task<SitesModules> GetSiteModuleById(string id);
        #endregion

        #region GetSiteModuleById
        Task<IList<SitesModules>> GetSitesModulesById(string moduleId);
        #endregion

        #region GetSiteModule
        Task<SitesModules> GetSiteModule(string siteId, string moduleId);
        #endregion

        #region GetAllSiteModuleListForDropdown
        Task<List<SitesModules>> GetAllSiteModuleListForDropdown(string SiteId);
        #endregion

        #region GetSiteActiveModulesMenu
        Task<IList<CustomSiteModule>> GetSiteActiveModulesMenus(string siteId, string[] roleId);
        #endregion

        #region InsertSiteModule
        void InsertSiteModule(SitesModules entity);
        #endregion

        #region UpdateSiteModule
        void UpdateSiteModule(SitesModules entity);
        #endregion

        #region DeleteSitesModule
        void DeleteSitesModule(SitesModules entity);
        #endregion
        
    }
}
