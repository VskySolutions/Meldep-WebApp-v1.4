using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Common;
using Vsky.Services.Module;
using Vsky.Services.Sites;
using Vsky.Services.SitesModule;
using Vsky.Services.SitesModulesMenu;
using Vsky.Services.SitesModulesMenusPermission;
using Vsky.Services.SitesRole;

namespace Vsky.Api.Controllers
{
    [Route("modules")]
    public class ModulesController : BaseController
    {
        #region Services Initialization
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IModulesService _moduleService;
        private readonly IModulesMenusService _menuService;
        private readonly ICommonService _commonService;
        private readonly ISitesModulesService _sitesModulesService;
        private readonly ISiteService _siteService;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ISitesRolesService _sitesRolesService;
        private readonly ISitesModulesMenusService _sitesModulesMenusService;
        private readonly ISitesModulesMenusPermissionsService _sitesModulesMenusPermissionsService;
        private readonly IApplicationUserRoleService _applicationUserRoleService;

        public ModulesController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            IModulesService moduleService, 
            ISiteService siteService, 
            ICommonService commonService, 
            ISitesModulesService sitesModulesService, 
            IModulesMenusService modulesMenusService,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext db,
            ISitesRolesService sitesRolesService,
            ISitesModulesMenusService sitesModulesMenusService,
            ISitesModulesMenusPermissionsService sitesModulesMenusPermissionsService,
            IApplicationUserRoleService applicationUserRoleService
            )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _moduleService = moduleService;
            _commonService = commonService;
            _userManager = userManager;
            _roleManager = roleManager;
            _sitesModulesService = sitesModulesService;
            _menuService = modulesMenusService;
            _siteService = siteService;
            _db = db;
            _sitesRolesService = sitesRolesService;
            _sitesModulesMenusService = sitesModulesMenusService;
            _sitesModulesMenusPermissionsService = sitesModulesMenusPermissionsService;
            _applicationUserRoleService = applicationUserRoleService;
        }
        #endregion

        #region GetAllModules
        [AllowAnonymous]
        [HttpGet("moduleslist")]
        public async Task<IActionResult> GetModules()
        {
            try
            {
                var list = await _moduleService.GetAllModulesList();
                var model = _mapper.Map<IList<ModulesModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllModules
        [HttpGet]
        public async Task<IActionResult> GetAllModules(string siteId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
            var siteRoles = await _sitesRolesService.GetRolesBySiteId(SiteId);
            var siteRoleIds = siteRoles.Select(sr => sr.Id).ToArray();
            var list = await _moduleService.GetAllModules(SiteId, siteRoleIds);
            var model = _mapper.Map<IList<ModulesModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetSiteActiveModulesMenu
        [HttpGet("activemodulesmenusbysite")]
        public async Task<IActionResult> GetSiteActiveModulesMenus()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                //var loggedUser = await _userManager.FindByIdAsync(LoggedUserId);
                //var roleNames = await _userManager.GetRolesAsync(loggedUser);
                //var siteRoleIds = siteRoles.Where(sr => roleNames.Contains(sr.ApplicationRole.Name)).Select(sr => sr.Id).ToArray();

                var roleIds = await _applicationUserRoleService.GetRoleIdsByUserAndSite(LoggedUserId, SiteId);
                var siteRoles = await _sitesRolesService.GetRolesBySiteId(SiteId);
                var siteRoleIds = siteRoles
                    .Where(sr => roleIds.Contains(sr.RoleId))
                    .Select(sr => sr.Id)
                    .ToArray();

                var rolesArray = siteRoleIds.Length > 0 ? siteRoleIds : Array.Empty<string>();
                var list = await _sitesModulesService.GetSiteActiveModulesMenus(SiteId, rolesArray);
                var model = _mapper.Map<IList<CustomSiteModule>>(list);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetModuleById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetModuleById(string id)
        {
            var entity = await _moduleService.GetModuleById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No Module found found with the specified id."));

            var model = _mapper.Map<ModulesModel>(entity);
            return Ok(model);
        }
        #endregion

        #region CreateModule
        [HttpPost]
        public async Task<IActionResult> CreateModule(ModulesModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var entity = _mapper.Map<Vsky.Models.Modules>(model);
                entity.CreatedById = LoggedUserId;
                entity.UpdatedById = LoggedUserId;
                entity.CreatedOnUtc = GetDateTime;
                entity.UpdatedOnUtc = GetDateTime;
                _moduleService.InsertModule(entity);

                var sites = await _siteService.GetAllSitesList();
                foreach (var site in sites)
                {
                    var sitesModule = new SitesModules
                    {
                        SiteId = site.Id,
                        ModuleId = entity.Id,
                        SortOrder = entity.Sortorder,
                        Active = false,
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime
                    };
                    _sitesModulesService.InsertSiteModule(sitesModule);
                }

               return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateModule
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModule(string id, ModulesModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var entity = await _moduleService.GetModuleById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No Module found found with the specified id."));

                entity = _mapper.Map(model, entity);
                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;
                _moduleService.UpdateModule(entity);

                // update related SitesModules sort order
                var SiteModule = _sitesModulesService.GetSiteModule(SiteId, entity.Id).Result;
                var siteModuleEntity = await _sitesModulesService.GetSiteModuleById(SiteModule.Id);
                if (siteModuleEntity == null)
                    return BadRequest(new BadRequestError("No Site Module found found with the specified id."));

                siteModuleEntity.SortOrder = entity.Sortorder;
                siteModuleEntity.UpdatedById = LoggedUserId;
                siteModuleEntity.UpdatedOnUtc = GetDateTime;
                _sitesModulesService.UpdateSiteModule(siteModuleEntity);

                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteModule
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModule(string id)
        {
            var entity = await _moduleService.GetModuleById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No Module found found with the specified id."));

            var siteModules = await _sitesModulesService.GetSitesModulesById(id);
            foreach (var siteModule in siteModules)
            {
                // Get all SiteModuleMenus for each SiteModule
                var siteModuleMenus = await _sitesModulesMenusService.GetSitesModulesMenusBySiteModuleId(siteModule.Id);
                foreach (var siteModuleMenu in siteModuleMenus)
                {
                    // Get and delete all Permissions for each menu
                    var permissions = await _sitesModulesMenusPermissionsService.GetPermissionsBySiteModuleMenuId(siteModuleMenu.Id);
                    foreach (var permission in permissions)
                    {
                        _sitesModulesMenusPermissionsService.DeleteSiteModuleMenusPermission(permission);
                    }

                    _sitesModulesMenusService.DeleteSitesModulesMenus(siteModuleMenu); // delete menu
                }

                _sitesModulesService.DeleteSitesModule(siteModule); // delete sitemodule
            }

            _moduleService.DeleteModule(entity);

            return NoContent();
        }
        #endregion

        #region GetMenuPermission
        //for toggle functionality
        [HttpGet("sitesmenusmanage-permission/{siteId}/{siteRoleId}")]
        public async Task<IActionResult> GetSiteModuleMenuPermission(string siteId, string siteRoleId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
                var permissionsList = new List<SitesModulesMenusPermissionsModel>();
                var siteModuleMenusPermissions = await _sitesModulesMenusPermissionsService.GetMenusBySiteIdAndRoleId(SiteId, siteRoleId);
                if (siteModuleMenusPermissions != null && siteModuleMenusPermissions.Any())
                {
                     foreach (var entity in siteModuleMenusPermissions)
                     {
                        var model = _mapper.Map<SitesModulesMenusPermissionsModel>(entity);
                        permissionsList.Add(model);
                     }
                 }
               return Ok(permissionsList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("menu-role-permissions/list")]
        public async Task<IActionResult> GetSiteMenuRolePermissions(SitesModulesMenusPermissionsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = searchModel.SiteId == "undefined" ? _globalVariable.SiteId : searchModel.SiteId;
                var result = await _sitesModulesMenusPermissionsService.GetModuleMenusWithRoles(
                    SiteId,
                    searchModel.SearchText,
                    searchModel.ModuleIds,
                    searchModel.MenuIds,
                    searchModel.RoleIds,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                return Ok(new
                {
                    siteMenuRolePermissionsList = result,
                    total = result.TotalCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region SaveSitesModules
        [HttpPost("savesitemodules")]
        public async Task<IActionResult> SaveSitesModules(SitesModulesModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var entity = _mapper.Map<SitesModules>(model);
                var exists = _db.SitesModules.Any(x => x.SiteId == model.SiteId && x.ModuleId == model.ModuleId && !x.Deleted);
                if (exists) //Update permission
                {
                    var SiteModule = _sitesModulesService.GetSiteModule(model.SiteId, model.ModuleId);
                    entity = await _sitesModulesService.GetSiteModuleById(SiteModule.Result.Id);
                    entity.Active = model.ModuleStatus;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _sitesModulesService.UpdateSiteModule(entity);
                }
                else //Create permission
                {
                    entity = _mapper.Map(model, entity);
                    entity.Active = true;
                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _sitesModulesService.InsertSiteModule(entity);
                }
                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion
    }
}
