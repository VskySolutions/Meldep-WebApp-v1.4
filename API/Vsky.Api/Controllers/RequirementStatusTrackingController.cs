using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.RequirementStatusTrackings;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("requirement-status-tracking")]
    public class RequirementStatusTrackingController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IRequirementStatusTrackingService _requirementStatusTrackingService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        #endregion

        #region Services Initializations
        public RequirementStatusTrackingController(
            GlobalVariable globalVariable,
            IRequirementStatusTrackingService requirementStatusTrackingService,
            ICommonService commonService,
            ISiteService siteService
        )
        {
            _globalVariable = globalVariable;
            _requirementStatusTrackingService = requirementStatusTrackingService;
            _commonService = commonService;
            _siteService = siteService;
        }
        #endregion

        #region Private Methods
        private (string SiteId, string LoggedUserId, string EmployeeId) GetRequestContext()
        {
            var loggedUserId = User.GetLoggedInUserId<string>();
            var siteId = _globalVariable.SiteId;
            var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(siteId, loggedUserId);
            return (siteId, loggedUserId, employeeId);
        }

        private async Task<DateTime> GetSiteDateTime(string siteId)
        {
            var siteData = await _siteService.GetById(siteId);
            return _siteService.GetDateTime(siteData.TimeZone);
        }

        private IActionResult SaveResult(RequirementStatusTrackingResult result)
        {
            if (result.NotFound)
                return NotFound(new NotFoundError("No tracking record found with the specified id."));
            if (result.Errors.Any())
                return BadRequest(new BadRequestError(result.Errors));
            return Ok(result.Record);
        }
        #endregion

        #region GetAllRequirementStatusTrackingRecords
        // Title: Get All Requirement Status Tracking Records
        // Description: Returns complete tracking records (with all their rows), paged by record.
        [HttpPost("list")]
        public async Task<IActionResult> GetAllRecords(RequirementStatusTrackingSearchModel searchModel)
        {
            try
            {
                var (siteId, loggedUserId, employeeId) = GetRequestContext();

                var result = await _requirementStatusTrackingService.GetAllRecords(siteId, loggedUserId, employeeId, new RequirementStatusTrackingFilter
                {
                    SearchText = searchModel.SearchText,
                    ProjectIds = searchModel.ProjectIds,
                    ProjectModuleIds = searchModel.ProjectModuleIds,
                    RequirementIds = searchModel.RequirementIds,
                    FromDate = searchModel.FromDate,
                    ToDate = searchModel.ToDate,
                    SortBy = searchModel.SortBy,
                    Descending = searchModel.Descending,
                    Sorts = searchModel.Sorts,
                    Page = searchModel.Page,
                    PageSize = searchModel.PageSize
                });

                if (result.Errors.Any())
                    return BadRequest(new BadRequestError(result.Errors));

                return Ok(result.List);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetRequirementStatusTrackingRecordById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecordById(string id)
        {
            try
            {
                var (siteId, loggedUserId, employeeId) = GetRequestContext();

                var record = await _requirementStatusTrackingService.GetRecordById(siteId, loggedUserId, employeeId, id);
                if (record == null)
                    return NotFound(new NotFoundError("No tracking record found with the specified id."));

                return Ok(record);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region BulkAdd
        // Creates one complete project/module/date record with all its requirement rows in a single request.
        [HttpPost("bulk-add")]
        public async Task<IActionResult> BulkAdd(RequirementStatusTrackingSaveModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ModelStateError(ModelState);

                var (siteId, loggedUserId, employeeId) = GetRequestContext();
                var now = await GetSiteDateTime(siteId);

                var result = await _requirementStatusTrackingService.SaveRecord(siteId, loggedUserId, employeeId, model, true, now);
                return SaveResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region BulkSave
        // Updates one complete record: row additions, changes and staged removals are committed together.
        [HttpPost("bulk-save")]
        public async Task<IActionResult> BulkSave(RequirementStatusTrackingSaveModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ModelStateError(ModelState);

                var (siteId, loggedUserId, employeeId) = GetRequestContext();
                var now = await GetSiteDateTime(siteId);

                var result = await _requirementStatusTrackingService.SaveRecord(siteId, loggedUserId, employeeId, model, false, now);
                return SaveResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var (siteId, loggedUserId, employeeId) = GetRequestContext();
                var now = await GetSiteDateTime(siteId);

                var result = await _requirementStatusTrackingService.DeleteRecords(siteId, loggedUserId, employeeId, new List<string> { id }, now);
                if (result.NotFound)
                    return NotFound(new NotFoundError("No tracking record found with the specified id."));
                if (result.Errors.Any())
                    return BadRequest(new BadRequestError(result.Errors));

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region BulkDelete
        [HttpPost("bulk-delete")]
        public async Task<IActionResult> BulkDelete(RequirementStatusTrackingRecordIdsModel model)
        {
            try
            {
                var (siteId, loggedUserId, employeeId) = GetRequestContext();
                var now = await GetSiteDateTime(siteId);

                var result = await _requirementStatusTrackingService.DeleteRecords(siteId, loggedUserId, employeeId, model?.RecordIds, now);
                if (result.NotFound)
                    return NotFound(new NotFoundError("No tracking record found with the specified id."));
                if (result.Errors.Any())
                    return BadRequest(new BadRequestError(result.Errors));

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region BulkEdit
        // Contract reserved; disabled until the editable field set and row-target rules are agreed (WO-66 gate).
        [HttpPost("bulk-edit")]
        public IActionResult BulkEdit(RequirementStatusTrackingBulkEditModel model)
        {
            return BadRequest(new BadRequestError("Bulk Edit is not enabled yet: the editable fields and row-target rules have not been confirmed. No records were changed."));
        }
        #endregion
    }
}
