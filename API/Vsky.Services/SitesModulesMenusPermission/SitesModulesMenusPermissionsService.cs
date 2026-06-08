using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.SitesModulesMenusPermission
{
    public class SitesModulesMenusPermissionsService : ISitesModulesMenusPermissionsService
    {
        #region Define Services
        private readonly IRepository<SitesModulesMenusPermissions> _SitesModulesMenusPermissionsRepository;
        #endregion

        #region Services Initializations
        public SitesModulesMenusPermissionsService(IRepository<SitesModulesMenusPermissions> SitesModulesMenusPermissionsRepository)
        {
            _SitesModulesMenusPermissionsRepository = SitesModulesMenusPermissionsRepository;
        }
        #endregion

        #region GetPermissionsById
        // Title: GetSiteModuleMenuPermissionById
        // Description: This method retrieves a SitesModulesMenusPermissions entity by its unique Id.
        public async Task<SitesModulesMenusPermissions> GetSiteModuleMenuPermissionById(string id)
        {
            return await _SitesModulesMenusPermissionsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).FirstOrDefaultAsync();
        }
        #endregion

        #region GetPermissionsBySiteModuleMenuId
        // Title: GetPermissionsBySiteModuleMenuId
        // Description: This method retrieves a list of SitesModulesMenusPermissions based on a given SiteModuleMenuId.
        public async Task<IList<SitesModulesMenusPermissions>> GetPermissionsBySiteModuleMenuId(string siteModuleMenuId)
        {
            return await _SitesModulesMenusPermissionsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteModuleMenuId == siteModuleMenuId).ToListAsync();
           
        }
        #endregion
        public async Task<SitesModulesMenusPermissions> GetPermissionByModuleMenuIdAndRoleId(string siteId, string moduleMenuId, string roleId)
        {
            return await _SitesModulesMenusPermissionsRepository.TableNoTracking
                .FirstOrDefaultAsync(x =>
                    !x.Deleted &&
                    x.SiteId == siteId &&
                    x.SiteModuleMenuId == moduleMenuId &&
                    x.SiteRoleId == roleId);
        }

        #region GetPermissionsByRoleId
        // Title: GetMenusBySiteIdAndRoleId
        // Description: This method retrieves a list of active SitesModulesMenusPermissions based on the given SiteId and RoleId.
        public async Task<IList<SitesModulesMenusPermissions>> GetMenusBySiteIdAndRoleId(string siteId, string roleId)
        {
            var query = _SitesModulesMenusPermissionsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.SiteRoleId == roleId && !x.SitesModulesMenus.SitesModules.Modules.Deleted && x.SitesModulesMenus.ModulesMenus.Active);

            query = query.OrderBy(x => x.SitesModulesMenus.SitesModules.SortOrder).ThenBy(x => x.SitesModulesMenus.SitesModules.Modules.Name).ThenBy(x => x.SitesModulesMenus.SortOrder).Select(x => new SitesModulesMenusPermissions
            {
                Id = x.Id,
                IsShowMenu = x.IsShowMenu,
                IsManage = x.IsManage,
                IsView = x.IsView,
                SitesModulesMenus = new SitesModulesMenus
                {
                    Id = x.Id,
                    MenuId = x.SitesModulesMenus.MenuId,
                    ModulesMenus = new ModulesMenus
                    {
                        Id = x.SitesModulesMenus.ModulesMenus.Id,
                        DisplayName = x.SitesModulesMenus.ModulesMenus.DisplayName,
                    },
                    SitesModules = new SitesModules
                    {
                        ModuleId = x.SitesModulesMenus.SitesModules.ModuleId,
                        Modules = new Modules
                        {
                            Id = x.SitesModulesMenus.SitesModules.Modules.Id,
                            Name = x.SitesModulesMenus.SitesModules.Modules.Name
                        }
                    }
                }
            });
            var list = await query.ToListAsync();
            return list;
        }
        public async Task<IPagedList<ModuleMenuRoleDto>> GetModuleMenusWithRoles(
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
        )
        {
            var query = _SitesModulesMenusPermissionsRepository.TableNoTracking
                .Where(x =>
                    !x.Deleted &&
                    x.SiteId == siteId &&
                    !x.SitesModulesMenus.SitesModules.Modules.Deleted &&
                    x.SitesModulesMenus.ModulesMenus.Active);

            if (moduleIds?.Any() == true)
                query = query.Where(x => moduleIds.Contains(x.SitesModulesMenus.SitesModules.ModuleId));

            if (menuIds?.Any() == true)
                query = query.Where(x => menuIds.Contains(x.SitesModulesMenus.MenuId));

            //if (roleIds?.Any() == true)
            //    query = query.Where(x => roleIds.Contains(x.SiteRoleId));
            if (roleIds?.Any() == true)
            {
                var menuIdsWithRoles = await query
                    .Where(x => roleIds.Contains(x.SiteRoleId) && x.IsShowMenu)
                    .Select(x => x.SiteModuleMenuId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(x => menuIdsWithRoles.Contains(x.SiteModuleMenuId));
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();

                var matchedMenuIds = await query
                    .Where(x =>
                        x.SitesModulesMenus.SitesModules.Modules.Name.ToLower().Contains(searchText) ||
                        x.SitesModulesMenus.ModulesMenus.DisplayName.ToLower().Contains(searchText) ||
                        x.SitesRoles.ApplicationRole.Name.ToLower().Contains(searchText))
                    .Select(x => x.SiteModuleMenuId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(x => matchedMenuIds.Contains(x.SiteModuleMenuId));
            }

            var groupedQuery = query
                .GroupBy(x => new
                {
                    x.SitesModulesMenus.Id,
                    x.SitesModulesMenus.MenuId,
                    ModuleName = x.SitesModulesMenus.SitesModules.Modules.Name,
                    MenuName = x.SitesModulesMenus.ModulesMenus.DisplayName,
                    ModuleSortOrder = x.SitesModulesMenus.SitesModules.Modules.Sortorder,
                    MenuSortOrder = x.SitesModulesMenus.SortOrder
                })
                .Select(g => new ModuleMenuRoleDto
                {
                    SiteModuleMenuId = g.Key.Id,
                    MenuId = g.Key.MenuId,
                    ModuleName = g.Key.ModuleName,
                    MenuName = g.Key.MenuName,
                    Roles = g.Where(x => x.IsShowMenu).Select(x => new RoleDto
                    {
                        Id = x.SiteRoleId,
                        Name = x.SitesRoles.ApplicationRole.Name
                    }).Distinct().ToList()
                });

            // Default sorting
            groupedQuery = groupedQuery
                .OrderBy(x => x.ModuleName)
                .ThenBy(x => x.MenuName);
            
            var pagedList = new PagedList<ModuleMenuRoleDto>(
                groupedQuery,
                page,
                pageSize
            );

            return pagedList;
        }
        #endregion

        #region InsertSiteModuleMenusPermission
        // Title: InsertSiteModuleMenusPermission
        // Description: This method inserts a  SitesModulesMenusPermissions entity into the repository. It takes a SitesModulesMenusPermissions object as input and uses the _SitesModulesMenusPermissionsRepository to handle the insertion operation.
        public void InsertSiteModuleMenusPermission(SitesModulesMenusPermissions entity)
        {
            _SitesModulesMenusPermissionsRepository.Insert(entity);
        }
        #endregion

        #region UpdateSiteModuleMenusPermission
        // Title: UpdateSiteModuleMenusPermission
        // Description: This method updates an existing SitesModulesMenusPermissions entity in the repository.It takes a SitesModulesMenusPermissions object as input and uses the _SitesModulesMenusPermissionsRepository to perform the update operation.
        public void UpdateSiteModuleMenusPermission(SitesModulesMenusPermissions entity)
        {
            _SitesModulesMenusPermissionsRepository.Update(entity);
        }
        #endregion

        #region DeleteSiteModuleMenusPermission
        // Title: DeleteSiteModuleMenusPermission
        // Description: This method performs a soft delete on a SitesModulesMenusPermissions entity by setting its Deleted flag to true. It uses the _SitesModulesMenusPermissionsRepository to update the entity.
        public void DeleteSiteModuleMenusPermission(SitesModulesMenusPermissions permission)
        {
            permission.Deleted = true;
            _SitesModulesMenusPermissionsRepository.Update(permission);
        }
        #endregion
    }
}
