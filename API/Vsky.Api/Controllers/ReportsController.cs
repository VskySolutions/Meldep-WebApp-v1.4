using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Rest;
using StackExchange.Profiling.Internal;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.Logging;
using Vsky.Services.PowerBI;
using Vsky.Services.ReportRoleGroupMappings;
using Vsky.Services.ReportSetting;
using Vsky.Services.ReportSettingDetail;
using Vsky.Services.Sites;
using Vsky.Services.SitesRole;

namespace Vsky.Api.Controllers
{
    [Route("reports")]
    public class ReportsController : BaseController
    {
        private string m_errorMessage;
        private readonly GlobalVariable _globalVariable;
        private readonly ConfigValidatorService _configValidatorService ;
        private readonly EmbedService _embedService;
        private readonly IReportSettingsService _reportSettingsService;
        private readonly IReportSettingDetailService _reportSettingDetailService;
        private readonly IReportRoleGroupMappingService _reportRoleGroupMappingService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISitesRolesService _sitesRolesService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;
        private readonly ILogger _loggerService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly IApplicationUserRoleService _applicationUserRoleService;

        public ReportsController(
            IMapper mapper,
            GlobalVariable globalVariable,
            ConfigValidatorService configValidatorService, 
            IReportSettingsService reportSettingsService,
            IReportSettingDetailService reportSettingDetailService,
            IReportRoleGroupMappingService reportRoleGroupMappingService,
            UserManager<ApplicationUser> userManager,
            ISitesRolesService sitesRolesService,
            ICommonService commonService,
            EmbedService embedService,
            ISiteService siteService,
            ILogger loggerService,
            IAzureBlobImageServices azureBlobImageServices,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _configValidatorService = configValidatorService;
            _reportSettingsService = reportSettingsService;
            _reportSettingDetailService = reportSettingDetailService;
            _reportRoleGroupMappingService = reportRoleGroupMappingService;
            _sitesRolesService = sitesRolesService;
            _userManager = userManager;
            m_errorMessage = _configValidatorService.GetWebConfigErrors();
            _commonService = commonService;
            _embedService = embedService;
            _siteService = siteService;
            _loggerService = loggerService;
            _azureBlobImageServices = azureBlobImageServices;
            _applicationUserRoleService = applicationUserRoleService;
        }

        [HttpGet("embed-report")]
        public async Task<ActionResult> EmbedReport(Guid reportId)
        {
            if (!m_errorMessage.IsNullOrWhiteSpace())
            {
                return BadRequest(BuildErrorModel(m_errorMessage));
            }

            try
            {
                //Get LoggedUser Info
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                if ( SiteId != null)
                {
                    var reportSettingData = await _reportSettingsService.GetBySiteId(SiteId);
                    var embedResult = await _embedService.GetEmbedParams(Guid.Parse(reportSettingData.WorkspaceId), reportId, siteId:SiteId);
                    return Ok(embedResult);
                }
                return BadRequest();
            }
            catch (HttpOperationException exc)
            {
                m_errorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}", exc.Response.StatusCode, (int)exc.Response.StatusCode, exc.Response.Content, exc.Response.Headers["RequestId"].FirstOrDefault());
                return BadRequest(m_errorMessage);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("embed-all-reports")]
        public async Task<ActionResult> GetAllReportList()
        {
            if (!m_errorMessage.IsNullOrWhiteSpace())
            {
                return BadRequest(m_errorMessage);
            }

            try
            {
                var model = new ReportEmbedConfig();
                // Fetch embed parameters for all reports in the workspace
                //Get LoggedUser Info
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                if (SiteId == null)
                {
                    _loggerService.Warning("SiteId not found for logged user");
                    return BadRequest();
                }

                var reportSettingData = await _reportSettingsService.GetBySiteId(SiteId);
                //if(reportSettingData == null) return BadRequest("Report Settings Missing");

                if(reportSettingData == null)
                {
                    _loggerService.Warning($"Report Settings Missing for SiteId: {SiteId}");

                    return NoContent();
                }

                var embedReportList = new List<ReportEmbedConfig>();
                var reportIds = await _embedService.GetAllReportData(Guid.Parse(reportSettingData.WorkspaceId), SiteId);
                model.ReportModelList = reportIds;
                return Ok(model);
                
                //return BadRequest();
            }
            catch (HttpOperationException exc)
            {
                m_errorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}", exc.Response.StatusCode, (int)exc.Response.StatusCode, exc.Response.Content, exc.Response.Headers["RequestId"].FirstOrDefault());
                return BadRequest(m_errorMessage);
            }
            catch (Exception ex)
            {
                _loggerService.Error("Error fetching Report Settings details", ex);
                return BadRequest(m_errorMessage);
            }
        }

        private ErrorModel BuildErrorModel(string errorMessage)
        {
            return new ErrorModel(errorMessage);
        }

        #region GetAllReportsDetails
        [HttpPost("list")]
        public async Task<IActionResult> GetAllReportsDetails(ReportSettingsDetailsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                //var loggedUser = await _userManager.FindByIdAsync(LoggedUserId);
                //var roleNames = await _userManager.GetRolesAsync(loggedUser);
                //var siteRoles = await _sitesRolesService.GetRolesBySiteId(SiteId);
                //var siteRoleIds = siteRoles.Where(sr => roleNames.Contains(sr.ApplicationRole.Name)).Select(sr => sr.Id).ToArray();
                var roleIds = await _applicationUserRoleService.GetRoleIdsByUserAndSite(LoggedUserId, SiteId);
                var siteRoles = await _sitesRolesService.GetRolesBySiteId(SiteId);
                var siteRoleIds = siteRoles
                    .Where(sr => roleIds.Contains(sr.RoleId))
                    .Select(sr => sr.Id)
                    .ToArray();

                var reportGroupIdsForRoles = (await _reportRoleGroupMappingService.GetReportGroupsByRoles(SiteId, siteRoleIds))
                             .Select(x => x.ReportGroupId)
                             .ToList();

                var list = await  _reportSettingDetailService.GetAllReportsDetails(
                                SiteId,
                                LoggedUserId,
                                reportGroupIdsForRoles,
                                searchModel.SearchText, 
                                searchModel.ReportIds, 
                                searchModel.ReportGroupId, 
                                searchModel.SortBy, 
                                searchModel.Descending, 
                                searchModel.Page, 
                                searchModel.PageSize,
                                false,
                                true
                                );

                // Map the fetched list to a model suitable for the response
                var model = new ReportSettingsDetailsListModel
                {
                    Data = _mapper.Map<IList<ReportSettingsDetailsModel>>(list),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(new BadRequestError("No data found."));
            }

        }
        #endregion


        #region GetAllReportListForDropdown
        // Title: GetAllReportListForDropdown
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllReportListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                //var loggedUser = await _userManager.FindByIdAsync(LoggedUserId);
                //var roleNames = await _userManager.GetRolesAsync(loggedUser);
                //var siteRoles = await _sitesRolesService.GetRolesBySiteId(SiteId);
                //var siteRoleIds = siteRoles.Where(sr => roleNames.Contains(sr.ApplicationRole.Name)).Select(sr => sr.Id).ToArray();

                var roleIds = await _applicationUserRoleService.GetRoleIdsByUserAndSite(LoggedUserId, SiteId);
                var siteRoles = await _sitesRolesService.GetRolesBySiteId(SiteId);
                var siteRoleIds = siteRoles
                    .Where(sr => roleIds.Contains(sr.RoleId))
                    .Select(sr => sr.Id)
                    .ToArray();

                var reportGroupIdsForRoles = (await _reportRoleGroupMappingService.GetReportGroupsByRoles(SiteId, siteRoleIds))
                             .Select(x => x.ReportGroupId)
                             .ToList();

                var list = await _reportSettingDetailService.GetAllReportListForDropdown(SiteId, reportGroupIdsForRoles, LoggedUserId, true);
                var model = _mapper.Map<List<ReportSettingsDetailsModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetReportById
        // Title: GetReportById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(string id)
        {
            try
            {
                // Fetch the project entity by its ID from the service
                var entity = await _reportSettingDetailService.GetById(id);
                // If the project entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No report found with the specified id."));

                // Map the project entity to a ProjectModel object
                var model = _mapper.Map<ReportSettingsDetailsModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Create Report

        [HttpPost]
        public async Task<IActionResult> CreateReport(ReportSettingsDetailsModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    
                    var exists = await _reportSettingDetailService.GetReportByName(SiteId, model.ReportName);
                    if (exists != null)
                        return BadRequest(new BadRequestError("The report name already exists"));

                    var entity = _mapper.Map<ReportSettingsDetails>(model);

                    entity.Id = Guid.NewGuid().ToString();
                    entity.SiteId = SiteId;

                    if (!string.IsNullOrEmpty(model.ReportDescription))
                    {
                        entity.ReportDescription = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.ReportDescription,
                                SiteData.Name,
                                "reports",
                                entity.Id
                            );
                    }

                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _reportSettingDetailService.IndsertReportSettingDetails(entity);

                    return Ok(entity);
                }

                // Return model state errors if the model state is not valid
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Update Report
        // Title: GetReportById
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReport(string id, ReportSettingsDetailsModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the project entity by its ID
                    var entity = await _reportSettingDetailService.GetById(id);

                    // If no project is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No report found with the specified id."));

                    var exists = await _reportSettingDetailService.GetReportByName(SiteId, model.ReportName, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Report name already exists, try with another."));

                    // Set the user who updated the project and the current UTC time for tracking purposes
                    entity.ReportName = model.ReportName;
                    entity.ReportGroupId = model.ReportGroupId;
                    entity.Url = model.Url;

                    entity.ReportDescription = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.ReportDescription,
                                SiteData.Name,
                                "reports",
                                entity.Id,
                                entity.ReportDescription
                            );

                    // Set the updated by and updated on properties
                    entity.UpdatedOnUtc = GetDateTime;
                    entity.UpdatedById = LoggedUserId;
                    _reportSettingDetailService.UpdateReportSettingDetails(entity);

                    return Ok(entity);

                }
                return ModelStateError(ModelState);
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Delete Report
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var entity = await _reportSettingDetailService.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No report found with the specified id."));

                _reportSettingDetailService.DeleteReport(entity);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllReportGroupRoles
        [HttpPost("report-group-roles/list")]
        public async Task<IActionResult> GetAllReportGroupRoles(ReportRoleGroupMappingSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _reportRoleGroupMappingService.GetAllReportGroupRoles(
                    SiteId,
                    searchModel.SearchText,
                    searchModel.SiteRoleIds,
                    searchModel.ReportGroupIds,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize);

                // Map the fetched list to a model suitable for the response
                var model = new ReportRoleGroupMappingListModel
                {
                    Data = _mapper.Map<IList<ReportRoleGroupMappingModel>>(list),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(new BadRequestError("No data found."));
            }

        }
        #endregion

        #region CreateReportGroupsRole
        [HttpPost("reportgroupsrole")]
        public async Task<IActionResult> CreateReportGroupsRole(ReportRoleGroupMappingModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.ReportGroupIds != null && model.ReportGroupIds.Any())
                    {
                        var existGroupNames = await _reportRoleGroupMappingService.GetAssignedReportGroupNames(SiteId, model.SiteRoleId, model.ReportGroupIds.ToList());

                        if (existGroupNames.Any())
                        {
                            return BadRequest(new BadRequestError($"Report group {string.Join(", ", existGroupNames)} already exist for the selected role."));
                        }

                        foreach (var groupId in model.ReportGroupIds)
                        {
                            
                            var entity = _mapper.Map<ReportRoleGroupMapping>(model);

                            entity.SiteId = SiteId;
                            entity.SiteRoleId = model.SiteRoleId;
                            entity.ReportGroupId = groupId;
                            entity.CreatedOnUtc = GetDateTime;
                            entity.UpdatedOnUtc = GetDateTime;
                            entity.CreatedById = LoggedUserId;
                            entity.UpdatedById = LoggedUserId;

                            _reportRoleGroupMappingService.InsertReportGroupRole(entity);
                        }
                    }

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteReportGroupsRole
        [HttpDelete("reportgroupsrole/{id}")]
        public async Task<IActionResult> DeleteReportGroupsRole(string id)
        {
            try
            {
                var entity = await _reportRoleGroupMappingService.GetReportGroupRoleById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No report group role found with the specified id."));

                _reportRoleGroupMappingService.DeleteReportGroupRole(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion


        [HttpPut("updateGroupsRoleStatus/{id}")]
        public async Task<IActionResult> UpdateReportGroupsRoleStatus(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (ModelState.IsValid)
                {
                    var entity = await _reportRoleGroupMappingService.GetReportGroupRoleById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No report group role found with the specified id."));

                    entity.Active = !entity.Active;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _reportRoleGroupMappingService.UpdateReportGroupRole(entity);

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
