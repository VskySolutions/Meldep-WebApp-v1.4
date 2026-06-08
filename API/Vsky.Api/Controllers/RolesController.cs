using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Logging;
using AutoMapper;
using Vsky.Data;
using Vsky.Services.Common;
using Microsoft.Extensions.DependencyInjection;
using Vsky.Services.Sites;
using StackExchange.Profiling.Internal;
using Vsky.Services.SitesModulesMenusPermission;
using Vsky.Services.SitesModulesMenu;
using Vsky.Services.SitesRole;
using Microsoft.EntityFrameworkCore;

namespace Vsky.Api.Controllers
{
    [Route("roles")]
    [AllowAnonymous]
    public class RolesController : BaseController
    {
        #region Fields
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICommonService _commonService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISiteService _siteService;
        private readonly ISitesModulesMenusPermissionsService _siteModulesMenusPermissionsService;
        private readonly ISitesRolesService _sitesRolesService;
        public RolesController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ApplicationDbContext db,
            RoleManager<ApplicationRole> roleManager,
            ICommonService commonService,
            UserManager<ApplicationUser> userManager,
            IServiceProvider serviceProvider,
            ISiteService siteService,
            ISitesModulesMenusPermissionsService sitesModulesMenusPermissionsService,
            ISitesRolesService sitesRolesService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _db = db;
            _roleManager = roleManager;
            _commonService = commonService;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
            _siteService = siteService;
            _siteModulesMenusPermissionsService = sitesModulesMenusPermissionsService;
            _sitesRolesService = sitesRolesService;
        }
        #endregion

        #region Methods
        [HttpGet]
        public IActionResult GetAllMasterRoles()
        {
             var list = _roleManager.Roles.Where(x => !x.Deleted).OrderBy(m => m.Name).ToList();
             var model = new RoleModel
             {
                 Data = _mapper.Map<IList<RoleModel>>(list),
                 Total = list.Count
             };
             return Ok(model);
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetAllSiteRoles(SitesRolesSearchModel searchModel)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            var list = _sitesRolesService.GetAllSiteRoles(SiteId, searchModel.SearchText, searchModel.SiteRoleIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

            var model = new SitesRolesListModel
            {
                Data = _mapper.Map<IList<SitesRolesModels>>(list),
                Total = list.TotalCount
            };

            return Ok(model);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMasterRoleById(string id)
        {
            var entity = await _roleManager.FindByIdAsync(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No role found with the specified id."));

            var model = _mapper.Map<RoleModel>(entity);
            return Ok(model);
        }

        [HttpGet("user-count/{siteRoleId}")]
        public async Task<int> GetUserCountBySiteRoleId(string siteRoleId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var roleId = await _sitesRolesService.GetRoleIdBySiteRoleId(siteRoleId);

            var totalUser = await _userManager.Users
                .Where(x => !x.Deleted
                    && x.UserRoles.Any(r => r.RoleId == roleId && r.SiteId == SiteId)
                    && x.Person.PersonSitesMapping.Any(psm => psm.SiteId == SiteId && !psm.Deleted))
                .CountAsync();

            return totalUser;
        }


        [HttpPost]
        public async Task<IActionResult> CreateMasterRole(RoleModel model)
        {
           
            var exists = _db.Roles.Any(x => x.Name == model.Name && !x.Deleted);            
            if (exists)
                return BadRequest(new BadRequestError("Role Name already exists."));

            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            var newRole = new ApplicationRole();
            newRole.Id = Guid.NewGuid().ToString();
            newRole.Name = model.Name;
            newRole.NormalizedName = model.Name.ToLower();
            newRole.ConcurrencyStamp = Guid.NewGuid().ToString();
            newRole.SiteId = SiteId;
            newRole.CreatedById = LoggedUserId;
            newRole.CreatedOnUtc = GetDateTime;

            var roleCreated = await _roleManager.CreateAsync(newRole);
            if (roleCreated.Succeeded)
            {
                return NoContent();
            }
            else
            {
                // Handle the error
                return BadRequest(new BadRequestError(string.Join(",", roleCreated.Errors.Select(e => e.Description))));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMasterRole(string id, RoleModel model)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            var exists = _db.Roles.Any(x => x.Name == model.Name && x.Id != id);
            if (exists)
                return BadRequest(new BadRequestError("Role Name already exists."));

            var entity = await _roleManager.FindByIdAsync(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No role found with the specified id."));

            entity = _mapper.Map(model, entity);
            entity.Name = model.Name;
            entity.UpdatedById = LoggedUserId;
            entity.UpdatedOnUtc = GetDateTime;
            await _roleManager.UpdateAsync(entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMasterRole(string id)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            var entity = await _roleManager.FindByIdAsync(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No role found with the specified id."));

            entity.Deleted = true;
            entity.UpdatedById = LoggedUserId;
            entity.UpdatedOnUtc = GetDateTime;
            await _roleManager.UpdateAsync(entity);

            return NoContent();
        }
        #endregion
    }
}