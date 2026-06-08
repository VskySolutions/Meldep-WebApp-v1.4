using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.PowerBI.Api.Models;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.SitesModulesMenusPermission
{
    public interface ISitesModulesMenusPermissionsService
    {
        #region GetSiteModuleMenuPermissionById
        Task<SitesModulesMenusPermissions> GetSiteModuleMenuPermissionById(string id);
        #endregion

        #region GetPermissionsBySiteModuleMenuId
        Task<IList<SitesModulesMenusPermissions>> GetPermissionsBySiteModuleMenuId(string siteModuleMenuId);
        Task<SitesModulesMenusPermissions> GetPermissionByModuleMenuIdAndRoleId(string siteId, string moduleMenuId, string roleId);
        #endregion

        #region GetMenusBySiteIdAndRoleId
        Task<IList<SitesModulesMenusPermissions>> GetMenusBySiteIdAndRoleId(string siteId, string roleId);
        Task<IPagedList<ModuleMenuRoleDto>> GetModuleMenusWithRoles(
            string siteId,
            string searchText,
            List<string> moduleIds,
            List<string> menuIds,
            List<string> roleIds,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue
        );
        #endregion

        #region InsertSiteModuleMenusPermission
        void InsertSiteModuleMenusPermission(SitesModulesMenusPermissions entity);
        #endregion

        #region UpdateSiteModuleMenusPermission
        void UpdateSiteModuleMenusPermission(SitesModulesMenusPermissions entity);
        #endregion

        #region DeleteSiteModuleMenusPermission
        void DeleteSiteModuleMenusPermission(SitesModulesMenusPermissions entity);
        #endregion
    }
}
