using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ApplicationUserRoles
{
    public interface IApplicationUserRoleService
    {
        #region GetUserRoles
        Task<List<ApplicationUserRole>> GetUserRoles(string userId, string siteId);
        #endregion

        #region IsRoleExists
        Task<bool> IsRoleExists(string userId, string roleId, string siteId);
        #endregion

        #region GetRoleIdsByUserAndSite
        Task<List<string>> GetRoleIdsByUserAndSite(string userId, string siteId);
        #endregion

        #region GetNormalizedRoleNamesByUserAndSite
        Task<List<string>> GetNormalizedRoleNamesByUserAndSite(string userId, string siteId);
        #endregion


        #region GetRoleNamesByUserAndSite
        Task<List<string>> GetRoleNamesByUserAndSite(string userId, string siteId);
        #endregion

        #region AddUserRoleAsync
        Task AddUserRoleAsync(string userId, string roleId, string siteId);
        #endregion

        #region RemoveAllUserRolesAsync
        Task RemoveAllUserRolesAsync(string userId, string siteId);
        #endregion

        #region InsertApplicationUserRole
        void InsertApplicationUserRole(ApplicationUserRole entity);
        #endregion

        #region UpdateApplicationUserRole
        void UpdateApplicationUserRole(ApplicationUserRole entity);
        #endregion

        #region DeleteApplicationUserRole
        void DeleteApplicationUserRole(ApplicationUserRole entity);
        #endregion
    }
}
