using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using Vsky.Services.LeaveCredits;
using Vsky.Services.LeaveSchedule;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Services.Common;
using AutoMapper;
using Vsky.Api.ApiErrors;
using Vsky.Services.Sites;
using Vsky.Models;
using System.Globalization;
using Vsky.Services.AzureBlobImage;

namespace Vsky.Api.Controllers
{
    [Route("yearly-leave-schedule")]

    public class LeaveScheduleController : BaseController
    {
        private readonly GlobalVariable _globalVariable;
        private readonly ILeaveScheduleService _leaveScheduleService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;

        public LeaveScheduleController(
            GlobalVariable globalVariable,
            ILeaveScheduleService leaveScheduleService,
            ICommonService commonService, 
            IMapper mapper, 
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _leaveScheduleService = leaveScheduleService;
            _commonService = commonService;
            _mapper = mapper;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetAllLeaveEvents()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _leaveScheduleService.GetAllLeaveEvents(SiteId);
                var model = _mapper.Map<IList<LeaveScheduleModels>>(list);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeaveEventById(string id)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            var entity = await _leaveScheduleService.GetLeaveEventById(SiteId, id);
            if (entity == null)
                return BadRequest(new BadRequestError("No event found with the specified id."));

            var model = _mapper.Map<LeaveScheduleModels>(entity);
            return Ok(model);
        }

        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetLeaveEventDetailsById(string id)
        {
            var entity = await _leaveScheduleService.GetLeaveEventDetailsById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No event found with the specified id."));

            var model = _mapper.Map<LeaveScheduleModels>(entity);
            return Ok(model);
        }

        #region create leave event
        [HttpPost]
        public async Task<IActionResult> CreateLeaveEvent(LeaveScheduleModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exist = _leaveScheduleService.GetLeaveScheduleByDate(SiteId, model.Date);
                    if (exist.Result != null)
                        return BadRequest(new BadRequestError("Cann't add more than one leave on the same date"));

                    var entity = _mapper.Map<LeaveSchedules>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "leave-yearly-schedule-event",
                                entity.Id
                            );
                    }

                    entity.Id = Guid.NewGuid().ToString();
                    entity.SiteId = SiteId;
                    entity.LeaveRuleId = model.LeaveRuleId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _leaveScheduleService.InsertLeaveEvent(entity);

                    return Ok(entity);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("yearly-saturdayoff")]
        public async Task<IActionResult> CreateSaturdayOffEvent(LeaveScheduleModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = _mapper.Map<LeaveSchedules>(model);

                    if (model.SelectedDates != null && model.SelectedDates.Count > 0)
                    {
                        foreach (var dateStr in model.SelectedDates)
                        {
                            var date = DateTime.ParseExact(dateStr, "MM/dd/yyyy", null);
                            var exist = _leaveScheduleService.GetLeaveScheduleByDate(SiteId, date);
                            if (exist.Result != null)
                                return BadRequest(new BadRequestError("Can't add more than one leave on the same date"));

                            entity.Id = Guid.NewGuid().ToString();
                            entity.SiteId = SiteId;
                            entity.LeaveRuleId = model.LeaveRuleId;
                            entity.Date = date;
                            entity.CreatedById = LoggedUserId;
                            entity.UpdatedById = LoggedUserId;
                            entity.CreatedOnUtc = GetDateTime;
                            entity.UpdatedOnUtc = GetDateTime;
                            _leaveScheduleService.InsertLeaveEvent(entity);
                        }
                        return Ok(entity);
                    }
                    else
                    {
                        return BadRequest(new BadRequestError("No Saturdays selected"));
                    }                  
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region update leave event
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLeaveEvent(string id, LeaveScheduleModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _leaveScheduleService.GetLeaveEventById(SiteId, id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No leave found with the specified id."));

                    var exist = _leaveScheduleService.GetLeaveScheduleByDate(SiteId, model.Date);
                    if (exist.Result != null)
                        return BadRequest(new BadRequestError("Cann't add more than one leave on the same date"));

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "leave-yearly-schedule-event",
                                entity.Id,
                                entity.Description
                            );
                    }

                    entity.Title = model.Title;
                    entity.SiteId = model.SiteId;
                    entity.LeaveRuleId = model.LeaveRuleId;
                    entity.Date = model.Date;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _leaveScheduleService.UpdateLeaveEvent(entity);

                    return Ok(id);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("yearly-saturdayoff/{id}")]
        public async Task<IActionResult> UpdateSaturdayOffEvent(string id, LeaveScheduleModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _leaveScheduleService.GetLeaveEventById(SiteId, id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No leave found with the specified id."));

                  
                    var exist = _leaveScheduleService.GetLeaveScheduleByDate(SiteId, model.Date);
                    if (exist.Result != null)
                        return BadRequest(new BadRequestError("Can't add more than one leave on the same date"));

                    entity.Title = model.Title;
                    entity.Description = model.Description;
                    entity.SiteId = model.SiteId;
                    //entity.LeaveRuleId = model.LeaveRuleId;
                    entity.LeaveRuleId = model.LeaveRules.Id;
                    entity.Date = model.Date;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _leaveScheduleService.UpdateLeaveEvent(entity);

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

        #region delete event
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeaveEvent(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var entity = await _leaveScheduleService.GetLeaveEventById(SiteId, id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No levae event found with the specified id."));

                _leaveScheduleService.DeleteLeaveEvent(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete-saturdayoff/{id}")]
        public async Task<IActionResult> DeleteSaturdayOffEvent(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var entity = await _leaveScheduleService.GetLeaveEventById(SiteId, id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No levae event found with the specified id."));

                _leaveScheduleService.DeleteLeaveEvent(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetYearlyLeaveListForDashboard
        // Title: Get EmployeeLeave List
        [HttpPost("yealyLeaves/list")]
        public async Task<IActionResult> GetYearlyLeaveListForDashboard()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var list = await _leaveScheduleService.GetEmployeeLeaveListForDashboard(SiteId, GetDateTime);
                var model = _mapper.Map<List<LeaveScheduleModels>>(list);
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
