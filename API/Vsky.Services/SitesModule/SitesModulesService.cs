using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.SitesModule
{
    public class SitesModulesService : ISitesModulesService
    {
        #region Services Initialiazation
        private readonly IRepository<SitesModules> _sitesModuleRepository;
        public SitesModulesService(IRepository<SitesModules> sitesModuleRepository)
        {
            _sitesModuleRepository = sitesModuleRepository;
        }
        #endregion

        #region GetSiteModuleById
        // Title: GetSiteModuleById
        // Description: Retrieves a single SitesModules entity by its unique identifier.
        public async Task<SitesModules> GetSiteModuleById(string id)
        {
           return await _sitesModuleRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).FirstOrDefaultAsync();
        }
        #endregion

        #region GetSitesModulesById
        // Title: GetSitesModulesById
        // Description: Retrieves a list of SitesModules entities based on the provided ModuleId.
        public async Task<IList<SitesModules>> GetSitesModulesById(string moduleId)
        {
            return await _sitesModuleRepository.TableNoTracking.Where(x => x.ModuleId == moduleId && !x.Deleted).ToListAsync();
        }
        #endregion

        #region GetSiteModule
        // Title: GetSiteModule
        // Description: Retrieves a single SitesModules entity matching both SiteId and ModuleId.
        public async Task<SitesModules> GetSiteModule(string siteId, string moduleId)
        {
           return await _sitesModuleRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.ModuleId == moduleId).FirstOrDefaultAsync();
           
        }
        #endregion

        #region GetAllSiteModuleListForDropdown
        public async Task<List<SitesModules>> GetAllSiteModuleListForDropdown(string SiteId)
        {
            var query = _sitesModuleRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
         
            query = query.Select(x => new SitesModules
            {
                Id = x.Id,
                Modules = new Modules
                {
                    Id = x.Modules.Id,
                    Name = x.Modules.Name,

                }
            });

            var list = await query.OrderBy(m => m.Modules.Name.Replace(" ", "")).ToListAsync();
            return list;
        }
        #endregion

        #region GetSiteActiveModulesMenus
        // Title: GetSiteActiveModulesMenuss
        // Description: Retrieves a list of active SitesModules and their related menus for a specific SiteId and RoleId.
        public async Task<IList<CustomSiteModule>> GetSiteActiveModulesMenus(string siteId, string[] roleId)
        {
           
            return await _sitesModuleRepository.TableNoTracking
                      .Where(siteModule => !siteModule.Deleted && siteModule.SiteId == siteId)
                      .OrderBy(siteModule => siteModule.SortOrder)
                      .Select(siteModule => new CustomSiteModule
                      {
                          Name = siteModule.Modules.Name,
                          CustomSiteModuleMenuList = siteModule.SitesModulesMenus
                              .Where(siteModuleMenu =>
                                  !siteModuleMenu.Deleted &&
                                  siteModuleMenu.ModulesMenus.Active &&
                                  siteModuleMenu.SitesModulesMenusPermissions
                                      .Any(permission =>
                                          roleId.Contains(permission.SiteRoleId) &&
                                          permission.IsShowMenu))
                              .OrderBy(siteModuleMenu => siteModuleMenu.SortOrder)
                              .Select(siteModuleMenu => new CustomSiteModule.CustomSiteModuleMenu
                              {
                                  DisplayName = siteModuleMenu.ModulesMenus.DisplayName,
                                  Link = siteModuleMenu.ModulesMenus.Link,
                                  Icon = siteModuleMenu.ModulesMenus.Icon,
                                  SetAsLanding = siteModuleMenu.SetAsLanding,
                                  OpenInNewTab = siteModuleMenu.ModulesMenus.OpenInNewTab

                              })
                              .ToList()
                      })
                      .ToListAsync();
        }
        #endregion

        #region InsertSiteModule
        // Title: InsertSiteModule
        // Description: Inserts a new SitesModules entity into the repository.
        public void InsertSiteModule(SitesModules entity)
        {
            _sitesModuleRepository.Insert(entity);
        }
        #endregion

        #region UpdateSiteModule
        // Title: UpdateSiteModule
        // Description: Updates an existing SitesModules entity in the repository.
        public void UpdateSiteModule(SitesModules entity)
        {
            _sitesModuleRepository.Update(entity);
        }
        #endregion

        #region DeleteSitesModule
        // Title: DeleteSitesModule
        // Description: Soft deletes a SitesModules entity by setting its Deleted flag to true.
        public void DeleteSitesModule(SitesModules entity)
        {
            entity.Deleted = true;
            _sitesModuleRepository.Update(entity);
        }
        #endregion
    }
}
