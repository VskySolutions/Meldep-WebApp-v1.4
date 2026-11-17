using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Authorization;
using Vsky.Services.Persons;
using Vsky.Services.Sites;
using Vsky.Services.SitesModulesMenu;

namespace Vsky.Api.Controllers
{
    /// <summary>
    /// Site selection for the logged-in user. These actions ignore X-Site-Id, so a user whose
    /// current site was revoked can still move to a site they do belong to.
    /// </summary>
    [Route("authorization")]
    [AuthenticatedOnly]
    public class AuthorizationController : BaseController
    {
        private readonly IPersonSitesMappingService _personSitesMappingService;
        private readonly ISiteAccessService _siteAccessService;
        private readonly ISiteService _siteService;
        private readonly ISitesModulesMenusService _sitesModulesMenusService;
        private readonly IApplicationUserRoleService _applicationUserRoleService;

        public AuthorizationController(
            IPersonSitesMappingService personSitesMappingService,
            ISiteAccessService siteAccessService,
            ISiteService siteService,
            ISitesModulesMenusService sitesModulesMenusService,
            IApplicationUserRoleService applicationUserRoleService)
        {
            _personSitesMappingService = personSitesMappingService;
            _siteAccessService = siteAccessService;
            _siteService = siteService;
            _sitesModulesMenusService = sitesModulesMenusService;
            _applicationUserRoleService = applicationUserRoleService;
        }

        // Vue uses this for route and button decisions, from the same logic the API enforces with.
        // The mode tells Vue whether a denial is real yet, so the UI and API change together.
        [HttpGet("my-permissions")]
        public async Task<IActionResult> GetMyPermissions(
            [FromServices] IPermissionService permissionService,
            [FromServices] GlobalVariable globalVariable,
            [FromServices] IOptions<SecurityAuthorizationOptions> options)
        {
            var snapshot = await permissionService.GetSnapshotAsync(User.GetLoggedInUserId<string>(), globalVariable.SiteId);

            return Ok(new
            {
                siteId = snapshot.SiteId,
                isSiteMember = snapshot.IsSiteMember,
                isSystemAdmin = snapshot.IsSystemAdmin,
                isSiteAdmin = snapshot.IsSiteAdmin,
                mode = options.Value.Mode.ToString(),
                siteGateMode = options.Value.SiteGateMode.ToString(),
                enforcedLinks = (options.Value.EnforcedLinks ?? System.Array.Empty<string>()).Select(MenuLink.Normalize),
                enforceAllLinks = options.Value.EnforceAllLinks,
                menus = snapshot.Menus.Values.Select(menu => new { link = menu.Link, show = menu.Show, view = menu.View, manage = menu.Manage })
            });
        }

        // Shows which actions still need a declared rule, without needing access to server logs.
        [HttpGet("coverage")]
        [RequireSystemAdmin]
        public IActionResult GetCoverage([FromServices] EndpointClassificationScan scan)
        {
            return Ok(scan.GetCoverage());
        }

        // Signs the user out everywhere, by making all their tokens stop working.
        [HttpPost("revoke-sessions")]
        public async Task<IActionResult> RevokeMySessions([FromServices] Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.FindByIdAsync(User.GetLoggedInUserId<string>());
            await userManager.UpdateSecurityStampAsync(user);
            HttpContext.RequestServices.GetService<SecurityLog>()?.Write(Vsky.Models.LogLevel.Warning, "Sessions revoked");
            return NoContent();
        }

        [HttpGet("my-sites")]
        public async Task<IActionResult> GetMySites()
        {
            var siteIds = await _siteAccessService.GetAccessibleSiteIdsAsync(User.GetLoggedInUserId<string>());
            return Ok(siteIds.Select(siteId => new { siteId }));
        }

        [HttpPost("switch-site")]
        public async Task<IActionResult> SwitchSite(SwitchSiteModel model)
        {
            var userId = User.GetLoggedInUserId<string>();

            if (!await _siteAccessService.IsSiteMemberAsync(userId, model?.SiteId))
            {
                // Not SITE_NOT_MEMBER: the user's current site is still fine, so they stay on it.
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    code = "SITE_SWITCH_DENIED",
                    message = "You do not have access to this site."
                });
            }

            var site = await _siteService.GetById(model.SiteId);
            var now = _siteService.GetDateTime(site.TimeZone);

            var mappings = await _personSitesMappingService.GetAllSitesByUserId(userId);
            foreach (var mapping in mappings)
            {
                mapping.LastUsed = mapping.SiteId == model.SiteId;
                mapping.UpdatedById = userId;
                mapping.UpdatedOnUtc = now;
            }

            // One save, so the user can't end up with no last-used site or with two.
            _personSitesMappingService.UpdatePersonSites(mappings);

            HttpContext.RequestServices.GetService<SecurityLog>()?.Write(Vsky.Models.LogLevel.Information, "Site switched", site.Id);

            return Ok(new
            {
                siteId = site.Id,
                timeZone = site.TimeZone,
                name = site.Name,
                landingPage = await _sitesModulesMenusService.GetLandingPageBySiteId(site.Id),
                roles = await _applicationUserRoleService.GetNormalizedRoleNamesByUserAndSite(userId, site.Id),
                isFileUploadOrExternal = site.IsFileUploadOrExternal
            });
        }
    }
}
