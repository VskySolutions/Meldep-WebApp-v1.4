using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.ReportSettingDetail;
using Vsky.Services.ReportUserMappings;
using Vsky.Services.Sites;
using Vsky.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Vsky.Services.DropDowns;
using Vsky.Services.Projects;
using Vsky.Services.ProjectUserMappings;
using System.Linq;
using Microsoft.PowerBI.Api.Models;

namespace Vsky.Api.Controllers
{
    [Route("report-users")]
    public class ReportUserMappingController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;
        private readonly IReportUserMappingService _reportUserMappingService;
        private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations
        public ReportUserMappingController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISiteService siteService, IReportUserMappingService reportUserMappingService,
            ICommonService commonService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _siteService = siteService;
            _reportUserMappingService = reportUserMappingService;
            _commonService = commonService;
        }
        #endregion

        #region GetAllReportsForUserPermission
        // Title: Get All Reports For User Permission
        // Description: This endpoint fetches a list of reports based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllReportsForUserPermission(ReportSettingsDetailsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of reports based on search criterias
                var list = _reportUserMappingService.GetAllReportsForUserPermission(SiteId, searchModel.SearchText, searchModel.ReportIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
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
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetReportUserByReportSettingsDetailId
        // Title: GetReportUserByReportSettingsDetailId
        // Description: This endpoint retrieves the details of a specific report based on its reportSettingsDetailId. 
        [HttpGet("user/{reportSettingsDetailId}")]
        public async Task<IActionResult> GetReportUserByReportSettingsDetailId(string reportSettingsDetailId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _reportUserMappingService.GetReportUserByReportSettingsDetailId(SiteId, reportSettingsDetailId);
                var model = _mapper.Map<List<ReportUserMapping>>(list);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region AssignUsersToReport
        [HttpPut("{reportSettingsDetailId}")]
        public async Task<IActionResult> AssignUsersToReport(string reportSettingsDetailId, ReportSettingsDetailsModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.ReportUserMapping.Count() > 0)
                    {
                        foreach (var reportUser in model.ReportUserMapping)
                        {
                            if (!string.IsNullOrWhiteSpace(reportUser.AspNetUserId))
                            {
                                var existUserUnderReport = await _reportUserMappingService.GetReportByUserIdandReportSettingsDetailId(SiteId, reportUser.AspNetUserId, reportSettingsDetailId);
                                if (reportUser.Deleted)
                                {
                                    var exisitingReportUserData = await _reportUserMappingService.GetReportUserById(reportUser.Id);
                                    if (exisitingReportUserData != null)
                                    {
                                        exisitingReportUserData.ReportSettingsDetailId = reportSettingsDetailId;
                                        exisitingReportUserData.AspNetUserId = reportUser.AspNetUserId;
                                        exisitingReportUserData.FullAccess = reportUser.FullAccess;
                                        exisitingReportUserData.ViewOnly = reportUser.ViewOnly;
                                        exisitingReportUserData.Deleted = reportUser.Deleted;
                                        _reportUserMappingService.UpdateReportUser(exisitingReportUserData);
                                    }
                                }
                                else if (existUserUnderReport != null)
                                {
                                    existUserUnderReport.FullAccess = reportUser.FullAccess;
                                    existUserUnderReport.ViewOnly = reportUser.ViewOnly;
                                    _reportUserMappingService.UpdateReportUser(existUserUnderReport);
                                }
                                else
                                {
                                    ReportUserMapping ReportUserMapping = new ReportUserMapping();
                                    ReportUserMapping.ReportSettingsDetailId = reportSettingsDetailId;
                                    ReportUserMapping.AspNetUserId = reportUser.AspNetUserId;
                                    ReportUserMapping.FullAccess = reportUser.FullAccess;
                                    ReportUserMapping.ViewOnly = reportUser.ViewOnly;
                                    ReportUserMapping.Deleted = reportUser.Deleted;

                                    ReportUserMapping.CreatedById = LoggedUserId;
                                    ReportUserMapping.CreatedOnUtc = GetDateTime;
                                    _reportUserMappingService.InsertReportUser(ReportUserMapping);
                                }
                            }
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

        #region AssignBulkReport
        [HttpPut("savebulk/{employeeIds}")]
        public async Task<IActionResult> AssignBulkReport(string employeeIds, ReportSettingsDetailsModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    List<string> notFoundUsers = new List<string>();
                    if (!string.IsNullOrEmpty(employeeIds))
                    {
                        var idArray = employeeIds.Split(',');
                        foreach (var user in idArray)
                        {
                            var AspNetUserId = _commonService.GetLoggeduserIdByEmployeeId(SiteId, user);
                            if (string.IsNullOrWhiteSpace(AspNetUserId))
                            {
                                notFoundUsers.Add(user);
                                continue;
                            }
                            if (!string.IsNullOrWhiteSpace(AspNetUserId))
                            {
                                if (model.ReportUserMapping.Count() > 0)
                                {
                                    foreach (var reportUser in model.ReportUserMapping)
                                    {
                                        if (!string.IsNullOrWhiteSpace(reportUser.ReportSettingsDetailId))
                                        {
                                            var existUserUnderReport = await _reportUserMappingService.GetReportByUserIdandReportSettingsDetailId(SiteId, AspNetUserId, reportUser.ReportSettingsDetailId);
                                            if (existUserUnderReport != null)
                                            {
                                                existUserUnderReport.FullAccess = reportUser.FullAccess;
                                                existUserUnderReport.ViewOnly = reportUser.ViewOnly;
                                                _reportUserMappingService.UpdateReportUser(existUserUnderReport);
                                            }
                                            else
                                            {
                                                ReportUserMapping ReportUserMapping = new ReportUserMapping();
                                                ReportUserMapping.ReportSettingsDetailId = reportUser.ReportSettingsDetailId;
                                                ReportUserMapping.AspNetUserId = AspNetUserId;
                                                ReportUserMapping.FullAccess = reportUser.FullAccess;
                                                ReportUserMapping.ViewOnly = reportUser.ViewOnly;
                                                ReportUserMapping.Deleted = reportUser.Deleted;

                                                ReportUserMapping.CreatedById = LoggedUserId;
                                                ReportUserMapping.CreatedOnUtc = GetDateTime;
                                                _reportUserMappingService.InsertReportUser(ReportUserMapping);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (notFoundUsers.Any())
                    {
                        return NotFound(new { message = "Something went wrong." });
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
    }
}
