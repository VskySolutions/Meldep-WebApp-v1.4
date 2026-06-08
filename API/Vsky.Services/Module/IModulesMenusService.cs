using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.Module
{
    public interface IModulesMenusService
    {
        #region GetAllMenus
        Task<IList<ModulesMenus>> GetAllMenus(string roleId);
        #endregion
        #region GetAllMenusList
        Task<IList<ModulesMenus>> GetAllMenusList();
        #endregion
        #region GetAllModuleMenus
        Task<IList<ModulesMenus>> GetAllModuleMenus(string ModuleId);
        #endregion

        #region GetAllParentMenus
        Task<IList<ModulesMenus>> GetAllParentMenus();
        #endregion

        #region GetMenuById
        Task<ModulesMenus> GetMenuById(string id, string SiteId = null);
        #endregion

        #region GetMenuByDisplayName
        Task<ModulesMenus> GetMenuByDisplayName(string displayName, string id = null);
        #endregion

        #region InsertMenu
        void InsertMenu(ModulesMenus entity);
        #endregion

        #region UpdateMenu
        void UpdateMenu(ModulesMenus entity);
        #endregion

        #region DeleteMenu
        void DeleteMenu(ModulesMenus entity);
        #endregion
    }
}
