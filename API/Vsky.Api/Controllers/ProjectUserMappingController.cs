using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.ProjectUserMappings;

namespace Vsky.Api.Controllers
{
    [Route("project-users")]
    public class ProjectUserMappingController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IProjectUserMappingService _projectUserMappingService;
        private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations
        public ProjectUserMappingController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            IProjectUserMappingService projectUserMappingService,
            ICommonService commonService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _projectUserMappingService = projectUserMappingService;
            _commonService = commonService;
        }
        #endregion

        #region GetAllProjectsForUserPermission
        // Title: Get All Projects For User Permission
        // Description: This endpoint fetches a list of projects based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllProjectsForUserPermission(ProjectSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var employeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);

                // Fetch a list of projects based on search criterias
                var list = await _projectUserMappingService.GetAllProjectsForUserPermission(SiteId, searchModel.IsTemplate, LoggedUserId, employeeId, searchModel.SearchText, searchModel.ProjectIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new ProjectListModel
                {
                    Data = _mapper.Map<IList<ProjectModel>>(list),
                    Total = list.TotalCount
                };

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