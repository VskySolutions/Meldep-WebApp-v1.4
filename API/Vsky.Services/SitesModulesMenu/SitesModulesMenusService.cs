using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.SitesModulesMenu
{
    public class SitesModulesMenusService : ISitesModulesMenusService
    {
        #region Services Initialiazation
        private readonly IRepository<SitesModulesMenus> _sitesModulesMenusRepository;
        public SitesModulesMenusService(IRepository<SitesModulesMenus> sitesModulesMenusRepository)
        {
            _sitesModulesMenusRepository = sitesModulesMenusRepository;
        }
        #endregion

        #region GetAllModuleMenusForDashboard
        public async Task<IList<SitesModulesMenus>> GetAllModuleMenusForDashboard(string SiteId)
        {
            var query = _sitesModulesMenusRepository.TableNoTracking.Where(x => !x.Deleted && x.Active && x.SiteId == SiteId && x.IsQuickLink);
            query = query.OrderBy(x => x.ModulesMenus.Sortorder).Select(x => new SitesModulesMenus
            {
                Id = x.Id,
                Active = x.Active,
                IsQuickLink = x.IsQuickLink,
                ModulesMenus = new ModulesMenus
                {
                    DisplayName = x.ModulesMenus.DisplayName,
                    Id = x.ModulesMenus.Id,
                    Link = x.ModulesMenus.Link
                },
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetSitesModulesMenusById

        // Title: GetSitesModulesMenusById
        // Description: This method retrieves a SitesModulesMenus from the database by its menuId. 
        public async Task<IList<SitesModulesMenus>> GetSitesModulesMenusByMenuId(string menuId)
        {
            return await _sitesModulesMenusRepository.TableNoTracking.Where(x => !x.Deleted && x.MenuId == menuId).ToListAsync();
            
        }
        #endregion

        #region GetSitesModulesMenusBySiteModuleId
        // Title: GetSitesModulesMenusBySiteModuleId
        // Description: This method retrieves a SitesModulesMenus from the database by its siteModuleId. 
        public async Task<IList<SitesModulesMenus>> GetSitesModulesMenusBySiteModuleId(string siteModuleId)
        {
            return await _sitesModulesMenusRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteModuleId == siteModuleId).ToListAsync();
        }
        public async Task<IList<SitesModulesMenus>> GetSitesModulesMenusBySiteModuleIdForDropdown(string siteId, string siteModuleIds)
        {
            var query = _sitesModulesMenusRepository.TableNoTracking
                .Where(x => !x.Deleted && x.SiteId == siteId);

            if (!string.IsNullOrWhiteSpace(siteModuleIds))
            {
                var ids = siteModuleIds.Split(',');

                query = query.Where(x => ids.Contains(x.SitesModules.ModuleId));
            }

            var list = await query
                .OrderBy(x => x.SortOrder)
                .Select(x => new SitesModulesMenus
                {
                    Id = x.Id,
                    ModulesMenus = new ModulesMenus
                    {
                        Id = x.ModulesMenus.Id,
                        DisplayName = x.ModulesMenus.DisplayName,
                        MenuName = x.ModulesMenus.MenuName
                    }
                })
                .ToListAsync();

            return list;
        }
        #endregion

        #region GetSitesModulesMenusBySiteId
        // Title: GetSitesModulesMenusBySiteId
        // Description: This method retrieves a SitesModulesMenus from the database by its siteId. 
        public async Task<IList<SitesModulesMenus>> GetSitesModulesMenusBySiteId(string siteId)
        {
            return await _sitesModulesMenusRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId).ToListAsync();
          
        }
        #endregion


        #region GetLandingPageBySiteId
        // Title: GetLandingPageBySiteId
        // Description: This method retrieves the landing page link for a site from SitesModulesMenus based on siteId.
        public async Task<string> GetLandingPageBySiteId(string siteId)
        {
            return await _sitesModulesMenusRepository.TableNoTracking
                                .Where(x => !x.Deleted  && x.SiteId == siteId && x.SetAsLanding)
                                .Select(x => x.ModulesMenus.Link)
                                .Where(link => !string.IsNullOrWhiteSpace(link))
                                .FirstOrDefaultAsync()
                                ?? "/dashboard";
        }
       #endregion

        #region GetSiteMenu
        // Title: GetSiteMenu
        // Description: Retrieves a single GetSiteMenu entity matching both SiteId and MenuId.
        public async Task<SitesModulesMenus> GetSiteMenu(string siteId, string menuId)
        {
            return await _sitesModulesMenusRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.MenuId == menuId).FirstOrDefaultAsync();

        }
        #endregion

        #region InsertSitesModulesMenu
        // Title: InsertSitesModulesMenu
        // Description: This method inserts a SitesModulesMenus entity into the repository. 
        public void InsertSitesModulesMenu(SitesModulesMenus entity)
        {
            _sitesModulesMenusRepository.Insert(entity);
        }
        #endregion

        #region UpdateSitesModulesMenu
        // Title: UpdateSitesModulesMenu
        // Description: Updates an existing SitesModulesMenus entity in the repository.
        public void UpdateSitesModulesMenu(SitesModulesMenus entity)
        {
            _sitesModulesMenusRepository.Update(entity);
        }
        #endregion

        #region DeleteSitesModulesMenus
        // Title: DeleteSitesModulesMenus
        // Description: This method performs a soft delete on a SitesModulesMenus entity. It sets the Deleted flag to true and updates the entity in the repository.
        public void DeleteSitesModulesMenus(SitesModulesMenus entity)
        {
            entity.Deleted = true;
            _sitesModulesMenusRepository.Update(entity);
        }
        #endregion

    }
}
