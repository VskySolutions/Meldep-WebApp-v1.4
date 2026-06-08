using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Module
{
    public class ModulesService : IModulesService
    {
        #region Services Initialization
        private readonly IRepository<Modules> _modulesRepository;

        public ModulesService(IRepository<Modules> moduleRepository)
        {
            _modulesRepository = moduleRepository;
        }
        #endregion

        #region GetAllModules (OLD 1.0) - REMOVED
        public async Task<IList<Modules>> GetAllModules(string siteId, string[] roleId)
        {
            var query = _modulesRepository.TableNoTracking.Where(x => !x.Deleted);
            query = query.OrderBy(x => x.Sortorder);
            query = _modulesRepository.TableNoTracking.Where(x => !x.Deleted).OrderBy(x => x.Sortorder).Select(x => new Modules
            {
                Id = x.Id,
                Name = x.Name,
                Sortorder = x.Sortorder,
                Active = x.Active,
                IsModuleActivatedForSite = x.SiteModules.Any(m => m.SiteId == siteId && m.Active),
                ModulesMenus = (ICollection<ModulesMenus>)x.ModulesMenus.Where(m => !m.Deleted && m.SitesModulesMenus.Any(s => s.SiteId == siteId && s.SitesModulesMenusPermissions.Any(b => roleId.Contains(b.SiteRoleId) && b.IsShowMenu == true))).OrderBy(m => m.Sortorder)
                .Select(a => new ModulesMenus
                {
                    // IsModuleMenuActivatedForSite = true,
                    MenuName = a.MenuName,
                    Id = a.Id,
                    Active = a.Active,
                    Sortorder = a.Sortorder,
                    DisplayName = a.DisplayName,
                    ModuleId = a.Modules.Id,
                }),
            });
            var list = await query.ToListAsync();
            return list;
        }
        #endregion



        #region GetModuleById (NEW 2.0)
        public async Task<Modules> GetModuleById(string id)
        {
            return await _modulesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).FirstOrDefaultAsync();
        }
        #endregion

        #region GetAllModulesList  (NEW 2.0)
        public async Task<IList<Modules>> GetAllModulesList()
        {
            return await _modulesRepository.TableNoTracking.Where(x => !x.Deleted).OrderBy(x => x.Sortorder).ToListAsync();
        }
        #endregion

        #region InsertModule
        public void InsertModule(Modules entity)
        {
            _modulesRepository.Insert(entity);
        }
        #endregion

        #region UpdateModule
        public void UpdateModule(Modules entity)
        {
            _modulesRepository.Update(entity);
        }
        #endregion

        #region DeleteModule
        public void DeleteModule(Modules entity)
        {
            entity.Deleted = true;

            _modulesRepository.Update(entity);
        }
        #endregion
    }
}
