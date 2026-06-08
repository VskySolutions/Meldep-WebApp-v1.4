using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Data;
using Vsky.Services.Module;
using Vsky.Api.Models;
using Vsky.Models;
using System.Linq;
using Vsky.Services.Sites;
using Vsky.Services.SitesModule;
using Vsky.Services.SitesModulesMenu;
using Vsky.Services.SitesModulesMenusPermission;
using Vsky.Services.SitesRole;
using System.Drawing;
using Microsoft.PowerBI.Api.Models;
using Vsky.Services.Common;

namespace Vsky.Api.Controllers
{
    [Route("menus")]
    public class ModulesMenusController : BaseController
    {

        #region Services Initialization
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly IModulesMenusService _menuService;
        private readonly ApplicationDbContext _db;
        private readonly ISiteService _siteService;
        private readonly ISitesModulesService _sitesModulesService;
        private readonly ISitesModulesMenusService _sitesModulesMenusService;
        private readonly ISitesRolesService _sitesRolesService;
        private readonly ISitesModulesMenusPermissionsService _sitesModulesMenusPermissionsService;

        public ModulesMenusController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            ICommonService commonService, 
            IModulesMenusService menuService, 
            ApplicationDbContext db, 
            ISiteService siteService, 
            ISitesModulesService sitesModulesService, 
            ISitesModulesMenusService sitesModulesMenusService, 
            ISitesModulesMenusPermissionsService sitesModulesMenusPermissionsService, 
            ISitesRolesService sitesRolesService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _commonService = commonService;
            _menuService = menuService;
            _db = db;
            _siteService = siteService;
            _sitesModulesService = sitesModulesService;
            _sitesModulesMenusService = sitesModulesMenusService;
            _sitesModulesMenusPermissionsService = sitesModulesMenusPermissionsService;
            _sitesRolesService = sitesRolesService;
        }
        #endregion

        #region Modules Menu Methods 

        #region GetAllMenus
        [HttpGet]
        public async Task<IActionResult> GetAllMenus(string roleId = null)
        {
            var list = await _menuService.GetAllMenus(roleId);
            var model = _mapper.Map<IList<ModulesMenusModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetAllModuleMenus
        [HttpGet("modulemenus")]
        public async Task<IActionResult> GetAllModuleMenus(string moduleId = null)
        {
            var list = await _menuService.GetAllModuleMenus(moduleId);
            var model = _mapper.Map<IList<ModulesMenusModel>>(list);
            return Ok(model);
        }

        [HttpGet("dashboard-module-menus")]
        public async Task<IActionResult> GetAllModuleMenusForDashboard()
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            var list = await _sitesModulesMenusService.GetAllModuleMenusForDashboard(SiteId);
            var model = _mapper.Map<IList<SitesModulesMenusModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetAllParentMenus
        [HttpGet("parent")]
        public async Task<IActionResult> GetAllParentMenus()
        {
            var list = await _menuService.GetAllParentMenus();
            var model = _mapper.Map<IList<ModulesMenusModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetMenuById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuById(string id)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            var entity = await _menuService.GetMenuById(id, SiteId);
            if (entity == null)
                return BadRequest(new BadRequestError("No Menu found with the specified id."));

            var model = _mapper.Map<ModulesMenusModel>(entity);
            return Ok(model);
        }
        #endregion

        #region CreateMenu
        [HttpPost]
        public async Task<IActionResult> CreateMenu(ModulesMenusModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var exists = await _menuService.GetMenuByDisplayName(model.DisplayName);
                if (exists != null)
                    return BadRequest(new BadRequestError("Menu name already exists."));

                var entity = _mapper.Map<ModulesMenus>(model);
                entity.ParentMenuId = model.ParentMenuId != "" ? model.ParentMenuId : null;
                entity.CreatedById = LoggedUserId;
                entity.UpdatedById = LoggedUserId;
                entity.CreatedOnUtc = GetDateTime;
                entity.UpdatedOnUtc = GetDateTime;
                _menuService.InsertMenu(entity);


                var siteModules = await _sitesModulesService.GetSitesModulesById(entity.ModuleId);
                var siteRoles = await _sitesRolesService.GetSitesRoles();
                foreach (var siteModule in siteModules)
                {
                    var sitesModulesMenus = new SitesModulesMenus
                    {
                        SiteId = siteModule.SiteId,
                        SiteModuleId = siteModule.Id,
                        MenuId = entity.Id,
                        SortOrder = entity.Sortorder,
                        IsQuickLink = model.IsQuickLink,
                        Active = false,
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime
                    };

                    _sitesModulesMenusService.InsertSitesModulesMenu(sitesModulesMenus);

                    var roleIds = siteRoles.Where(x => x.SiteId == sitesModulesMenus.SiteId).Select(x => x.Id).ToList();
                    foreach (var roleId in roleIds)
                    {
                        var sitesModulesMenusPermissions = new SitesModulesMenusPermissions
                        {
                            SiteId = sitesModulesMenus.SiteId,
                            SiteRoleId = roleId,
                            SiteModuleMenuId = sitesModulesMenus.Id,
                            IsShowMenu = false,
                            IsManage = false,
                            IsView = false,
                            CreatedById = LoggedUserId,
                            UpdatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime,
                            UpdatedOnUtc = GetDateTime
                        };
                        _sitesModulesMenusPermissionsService.InsertSiteModuleMenusPermission(sitesModulesMenusPermissions);
                    }
                }
                return NoContent();

            }

            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateMenu
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenu(string id, ModulesMenusModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                
                var exists = await _menuService.GetMenuByDisplayName(model.DisplayName, id);
                if (exists != null)
                    return BadRequest(new BadRequestError("Menu name already exists."));

                var entity = await _menuService.GetMenuById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No menu found with the specified id."));

                entity.ModuleId = model.ModuleId;
                entity.MenuName = model.MenuName;
                entity.DisplayName = model.DisplayName;
                entity.Sortorder = model.Sortorder;
                entity.Active = model.Active;
                entity.Link = model.Link;
                entity.Icon = model.Icon;
                entity.ParentMenuId = model.ParentMenuId != "" ? model.ParentMenuId : null;
                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;
                _menuService.UpdateMenu(entity);

                // update related SiteMenu sort order
                var SiteMenu = _sitesModulesMenusService.GetSiteMenu(SiteId, id).Result;
                if (SiteMenu == null)
                    return BadRequest(new BadRequestError("No Site menu found found with the specified id."));
                SiteMenu.Active = model.Active;
                SiteMenu.SortOrder = model.Sortorder;
                SiteMenu.IsQuickLink = model.IsQuickLink;
                SiteMenu.UpdatedById = LoggedUserId;
                SiteMenu.UpdatedOnUtc = GetDateTime;
                _sitesModulesMenusService.UpdateSitesModulesMenu(SiteMenu);

                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteMenu
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(string id)
        {
            var entity = await _menuService.GetMenuById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No menu found with the specified id."));

            _menuService.DeleteMenu(entity);
            // Delete from SitesModulesMenus
            var siteModuleMenus = await _sitesModulesMenusService.GetSitesModulesMenusByMenuId(id);
            foreach (var siteModuleMenu in siteModuleMenus)
            {
                _sitesModulesMenusService.DeleteSitesModulesMenus(siteModuleMenu);

                var sitesModulesMenusPermissions = await _sitesModulesMenusPermissionsService.GetPermissionsBySiteModuleMenuId(siteModuleMenu.Id);

                foreach (var permission in sitesModulesMenusPermissions)
                {
                    _sitesModulesMenusPermissionsService.DeleteSiteModuleMenusPermission(permission);
                }
            }
            return NoContent();
        }
        #endregion

        #region Menu Permissions Methods
        [HttpPost("savesitemenupermission")]
        public async Task<IActionResult> CreateSiteModuleMenuPermission(SitesModulesMenusPermissionsModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var entity = await _sitesModulesMenusPermissionsService.GetSiteModuleMenuPermissionById(model.Id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No menu found with the specified id."));

                entity.IsShowMenu = model.IsShowMenu;
                entity.IsView = model.IsView;
                entity.IsManage = model.IsManage;
                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;
                _sitesModulesMenusPermissionsService.UpdateSiteModuleMenusPermission(entity);

                return NoContent();
            }

            return ModelStateError(ModelState);
        }

        [HttpPost("assignRolesToMenu")]
        public async Task<IActionResult> SaveRolesToSiteModuleMenu(SitesModulesMenusPermissionsModel model)
        {
            var loggedUserId = User.GetLoggedInUserId<string>();
            var siteId = _globalVariable.SiteId;
            var siteData = await _siteService.GetById(siteId);
            var dateTime = _siteService.GetDateTime(siteData.TimeZone);

            var roleIds = model.SiteRoleIds.Split(',');

            foreach (var roleId in roleIds)
            {
                var entity = await _sitesModulesMenusPermissionsService
                    .GetPermissionByModuleMenuIdAndRoleId(
                        model.SiteId,
                        model.SiteModuleMenuId,
                        roleId);

                if (entity == null)
                {
                    entity = new SitesModulesMenusPermissions
                    {
                        SiteId = model.SiteId,
                        SiteModuleMenuId = model.SiteModuleMenuId,
                        SiteRoleId = roleId,
                        IsShowMenu = true,
                        IsView = false,
                        IsManage = false,
                        CreatedById = loggedUserId,
                        CreatedOnUtc = dateTime,
                        UpdatedById = loggedUserId,
                        UpdatedOnUtc = dateTime
                    };

                    _sitesModulesMenusPermissionsService
                        .InsertSiteModuleMenusPermission(entity);
                } else
                {
                    entity.IsShowMenu = true;
                    entity.IsView = false;
                    entity.IsManage = false;
                    entity.UpdatedById = loggedUserId;
                    entity.UpdatedOnUtc = dateTime;
                    _sitesModulesMenusPermissionsService.UpdateSiteModuleMenusPermission(entity);
                }
            }

            return NoContent();
        }
        #endregion

        #region DeleteModuleMenuRoleAccess
        [HttpDelete("delete-role-access/{siteId}/{moduleMenuId}/{roleId}")]
        public async Task<IActionResult> DeleteModuleMenuRoleAccess(string siteId, string moduleMenuId, string roleId)
        {
            var permission = await _sitesModulesMenusPermissionsService
                .GetPermissionByModuleMenuIdAndRoleId(siteId, moduleMenuId, roleId);

            if (permission == null)
                return BadRequest(new BadRequestError("Role access not found."));

            _sitesModulesMenusPermissionsService.DeleteSiteModuleMenusPermission(permission);

            return NoContent();
        }
        #endregion

        #endregion

        #region GetIsLandingPage
        #endregion

    }
}
