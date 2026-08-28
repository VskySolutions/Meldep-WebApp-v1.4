using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Models;
using Vsky.Services.Projects;

namespace Vsky.Api.Controllers
{
    [Route("site-project-roles")]
    public class SiteProjectRolesController : BaseController
    {

        #region Define Services      
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISitesProjectRolesService _sitesProjectRolesService;
        #endregion

        #region Services Initializations      
        public SiteProjectRolesController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISitesProjectRolesService sitesProjectRolesService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _sitesProjectRolesService = sitesProjectRolesService;
        }
        #endregion

        #region GetAllSiteProjectRolesListForDropdown
        // Title: GetAllSiteProjectRolesListForDropdown
        // Description: This endpoint retrieves the list of Site Project Roles. 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllSiteProjectRolesListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _sitesProjectRolesService.GetAllSiteProjectRolesListForDropdown(SiteId);
                var model = _mapper.Map<List<SitesProjectRoles>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}