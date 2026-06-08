using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Services.Module
{
    public class ModulesMenusService : IModulesMenusService
    {
        #region Fields
        private readonly IRepository<ModulesMenus> _modulesMenusRepository;
        #endregion

        #region Ctor
        public ModulesMenusService(IRepository<ModulesMenus> modulesMenusRepository)
        {
            _modulesMenusRepository = modulesMenusRepository;
        }
        #endregion

        #region GetMenuById
        public async Task<ModulesMenus> GetMenuById(string id, string SiteId = null)
        {
            //return await _modulesMenusRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).FirstOrDefaultAsync();

            var query = _modulesMenusRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            var item = await query.Include(m => m.SitesModulesMenus.Where(m => m.SiteId == SiteId)).FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetMenuByDisplayName
        public async Task<ModulesMenus> GetMenuByDisplayName(string displayName, string id = null)
        {
            var query = _modulesMenusRepository.TableNoTracking.Where(x => !x.Deleted && !x.Modules.Deleted && x.DisplayName.ToLower() == displayName.ToLower());
            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllMenusList
        public async Task<IList<ModulesMenus>> GetAllMenusList()
        {
            return await _modulesMenusRepository.TableNoTracking.Where(x => !x.Deleted).OrderBy(x => x.Sortorder).ToListAsync();
        }
        #endregion

        #region GetAllMenus - By RoleId
        public async Task<IList<ModulesMenus>> GetAllMenus(string roleId)
        {
            var query = _modulesMenusRepository.TableNoTracking.Where(x => !x.Deleted);
            query = query.OrderBy(x => x.Sortorder).Select(x => new ModulesMenus
            {
                Id = x.Id,
                MenuName = x.MenuName,
                DisplayName = x.DisplayName,
                Link = x.Link,
                Icon = x.Icon,
                Sortorder = x.Sortorder,
                Active = x.Active,
                ModuleId = x.ModuleId
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllModuleMenus
        public async Task<IList<ModulesMenus>> GetAllModuleMenus(string ModuleId)
        {
            var query = _modulesMenusRepository.TableNoTracking.Where(x => !x.Deleted && x.ModuleId == ModuleId);
            query = query.OrderBy(x => x.Sortorder).Select(x => new ModulesMenus
            {
                Id = x.Id,
                MenuName = x.MenuName,
                DisplayName = x.DisplayName,
                Link = x.Link,
                Icon = x.Icon,
                Sortorder = x.Sortorder,
                Active = x.Active,
                ModuleId = x.ModuleId,
                Modules = new Modules
                {
                    Name = x.Modules.Name,
                    Id = x.Modules.Id
                },
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllParentMenus
        public async Task<IList<ModulesMenus>> GetAllParentMenus()
        {
            var query = _modulesMenusRepository.TableNoTracking.Where(x => !x.Deleted && x.ParentMenuId == null);
            query = query.OrderBy(x => x.Sortorder).Select(x => new ModulesMenus
            {
                Id = x.Id,
                ModuleId = x.ModuleId,
                Active = x.Active,
                MenuName = x.MenuName,
                DisplayName = x.DisplayName,
                Link = x.Link,
                Icon = x.Icon,
                Sortorder = x.Sortorder
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region InsertMenu
        public void InsertMenu(ModulesMenus entity)
        {
            _modulesMenusRepository.Insert(entity);
        }
        #endregion

        #region UpdateMenu
        public void UpdateMenu(ModulesMenus entity)
        {
            _modulesMenusRepository.Update(entity);
        }
        #endregion

        #region DeleteMenu
        public void DeleteMenu(ModulesMenus entity)
        {
            entity.Deleted = true;
            _modulesMenusRepository.Update(entity);
        }
        #endregion
    }
}
