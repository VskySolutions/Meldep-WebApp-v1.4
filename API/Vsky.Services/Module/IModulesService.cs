using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.Module
{
    public interface IModulesService
    {
        #region
        Task<IList<Modules>> GetAllModulesList();
        #endregion

        #region GetAllModules
        Task<IList<Modules>> GetAllModules(string siteId, string[] roleId);
        #endregion

        #region GetModuleById
        Task<Modules> GetModuleById(string id);
        #endregion

        #region InsertModule
        void InsertModule(Modules entity);
        #endregion

        #region UpdateModule
        void UpdateModule(Modules entity);
        #endregion

        #region DeleteModule
        void DeleteModule(Modules entity);
        #endregion
    }
}
