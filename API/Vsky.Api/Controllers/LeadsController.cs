using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.LeadActivityLogss;
using Vsky.Services.Leads;
using Vsky.Services.LeadUserGroupMappings;
using Vsky.Services.SetReminders;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("leads")]
    public class LeadsController : BaseController
    {
        #region Services Initializations
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ILeadService _leadService;
        private readonly ILeadActivityLogsService _leadActivityLogsService;
        private readonly ISetReminderService _setReminderService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly ApplicationDbContext _db;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly ILeadUserGroupMappingService _leadUserGroupMappingService;
        public LeadsController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            ILeadService leadService, 
            ILeadActivityLogsService leadActivityLogsService, 
            ISetReminderService setReminderService,
            ICommonService commonService, 
            ApplicationDbContext db, 
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices,
            ILeadUserGroupMappingService leadUserGroupMappingService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _leadService = leadService;
            _leadActivityLogsService = leadActivityLogsService;
            _commonService = commonService;
            _setReminderService = setReminderService;
            _siteService = siteService;
            _db = db;
            _azureBlobImageServices = azureBlobImageServices;
            _leadUserGroupMappingService = leadUserGroupMappingService;
        }
        #endregion

        #region GetAllLeads
        [HttpPost("list")]
        public async Task<IActionResult> GetAllLeads(LeadSearchModels leadModels) 
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            var leadGroupIdsForUser = (await _leadUserGroupMappingService.GetLeadGroupsByUsers(SiteId, LoggedUserId))
                         .Select(x => x.LeadGroupId)
                         .ToList();

            var list = await _leadService.GetAllLeads(SiteId, LoggedUserId, leadGroupIdsForUser, leadModels.SearchText, leadModels.PersonId, leadModels.CompanyId, leadModels.LeadGroupIds, leadModels.LeadSourceId, leadModels.SortBy, leadModels.Descending, leadModels.Page, leadModels.PageSize);
            var model = new LeadsListModel
            {
                Data = _mapper.Map<IList<LeadModels>>(list),
                Total = list.TotalCount
            };

            return Ok(model);
        }
        #endregion

        #region GetAllLeadStages
        [HttpGet("leadstages-list")]
        public async Task<IActionResult> GetAllLeadStages()
        {
            var list = await _leadService.GetAllLeadStages();
            var model = _mapper.Map<IList<LeadStagesModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetAllActivities
        [HttpGet("leadactivities-list")]
        public async Task<IActionResult> GetAllActivities()
        {
            var list = await _leadService.GetAllActivities();
            var model = _mapper.Map<IList<LeadActivities>>(list);
            return Ok(model);
        }
        #endregion

        #region GetAllLeadActivityListForDropdown
        // Title: GetAllLeadActivityListForDropdown
        // Description: This endpoint retrieves the list of lead activities. 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllLeadActivityListForDropdown()
        {
            try
            {
                var list = await _leadService.GetAllLeadActivityListForDropdown();
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllLeadListForDropdown
        [HttpGet("lead-dropdown")]
        public async Task<IActionResult> GetAllLeadListForDropdown()
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _leadService.GetAllLeadListForDropdown(SiteId);
            var model = _mapper.Map<IList<Lead>>(list);
            return Ok(model);
        }
        #endregion

        #region GetLeadById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeadById(string id)
        {
            var entity = await _leadService.GetById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No lead found with the specified id."));

            var model = _mapper.Map<LeadModels>(entity);
            return Ok(model);
        }
        #endregion

        #region GetcompanydetailsById
        [HttpGet("{id}/leaddetails")]
        public async Task<IActionResult> GetLeadDetailsById(string id)
        {
            var entity = await _leadService.GetLeadDetailsById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No company found with the specified id."));

            var model = _mapper.Map<LeadModels>(entity);
            return Ok(model);
        }
        #endregion

        #region CreateLead
        [HttpPost]
        public async Task<IActionResult> CreateLead(LeadModels model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var exists = _db.Leads.Any(x => x.PersonId == model.PersonId && x.CompanyId == model.CompanyId && !x.Deleted);
                if (exists)
                    return BadRequest(new BadRequestError("Lead already exists."));

                var entity = _mapper.Map<Lead>(model);
                entity.Id = Guid.NewGuid().ToString();

                if (!string.IsNullOrEmpty(model.LeadNote))
                {
                    entity.LeadNote = await _azureBlobImageServices
                        .ProcessHtmlAndManageImagesAsync(
                            model.LeadNote,
                            SiteData.Name,
                            "lead",
                            entity.Id
                        );
                }

                // Set custom properties
                entity.SiteId = SiteId;
                entity.CreatedById = LoggedUserId;
                entity.UpdatedById = LoggedUserId;
                entity.CreatedOnUtc = GetDateTime;
                entity.UpdatedOnUtc = GetDateTime;
                _leadService.InsertLead(entity);

                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateLead
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLead(string id, LeadModels model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);

                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var exists = _db.Leads.Any(x => x.PersonId == model.PersonId && x.ClientId == model.ClientId && !x.Deleted && x.Id != model.Id);
                if (exists)
                    return BadRequest(new BadRequestError("Email already exists."));

                var entity = await _leadService.GetLeadDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No lead found with the specified id."));

                entity = _mapper.Map(model, entity);

                if (!string.IsNullOrEmpty(model.LeadNote))
                {
                    entity.LeadNote = await _azureBlobImageServices
                        .ProcessHtmlAndManageImagesAsync(
                            model.LeadNote,
                            SiteData.Name,
                            "lead",
                            entity.Id,
                            entity.LeadNote
                        );
                }

                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;
                _leadService.UpdateLead(entity);

                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteLead
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLead(string id)
        {
            var entity = await _leadService.GetById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No lead found with the specified id."));

            _leadService.DeleteLead(entity);

            return NoContent();
        }
        #endregion

        #region GetLeadActivityById
        [HttpGet("{id}/leadActivity")]
        public async Task<IActionResult> GetLeadActivityLogById(string id)
        {
            var entity = await _leadActivityLogsService.GetById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No lead activity found with the specified id."));

            // var model = _mapper.Map<LeadActivityLogsModel>(entity);
            var model = _mapper.Map<List<LeadActivityLogsModel>>(entity);
            return Ok(model);       
        }
        #endregion

        #region GetActivityById
        [HttpGet("{id}/Activitylog")]
        public async Task<IActionResult> GetActivityLogById(string id)
        {
            var entity = _leadActivityLogsService.GetByLeadId(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No lead activity log found with the specified id."));

            var model = _mapper.Map<List<LeadActivityLogsModel>>(entity);
            return Ok(model);
        }
        #endregion

        #region CreateLeadActivityLogs
        [HttpPost("leadActivityLogs")]
        public async Task<IActionResult> CreateLeadActivityLogs(LeadActivityLogsModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (model.Id == null)
                {
                    var entity = _mapper.Map<LeadActivityLogs>(model);
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _leadActivityLogsService.InsertLeadActivityLogs(entity);

                    if (model.SetReminderModels != null && model.SetReminderModels.Count() > 0)
                    {
                        foreach (var activity in model.SetReminderModels)
                        {
                            var reminderEntity = _mapper.Map<SetReminder>(activity);
                            reminderEntity.LeadActivityLogId = entity.Id;

                            string hour = string.Empty;

                            if (!string.IsNullOrWhiteSpace(activity.Time))
                            {
                                int charLocation = activity.Time.IndexOf(":", StringComparison.Ordinal);

                                if (charLocation > 0)
                                {
                                    hour = activity.Time.Substring(0, charLocation);
                                }
                            }

                            if (hour.Length == 1)
                            {
                                activity.Time = "0" + activity.Time;
                            }

                            // Define the possible formats of the time string
                            string[] formats = { "HH:mm", "hh:mm tt" };
                            // Parse the time string to a DateTime object
                            DateTime dateTime;
                            bool isValidTime = DateTime.TryParseExact(activity.Time, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                            if (!isValidTime)
                            {
                                throw new FormatException($"String '{activity.Time}' was not recognized as a valid DateTime.");
                            }

                            // Extract the hours and minutes
                            int hours = dateTime.Hour;
                            int minutes = dateTime.Minute;
                            var ActivityDate = Convert.ToDateTime(entity.ActivityDate);
                            // Calculate the new date and time for the reminder
                            DateTime newDateTime = ActivityDate.AddDays(activity.ReminderAfterDays);
                            // Create a new DateTime with the current date and the new time
                            newDateTime = new DateTime(
                                newDateTime.Year,
                                newDateTime.Month,
                                newDateTime.Day,
                                hours,
                                minutes,
                                0 // seconds
                            );

                            reminderEntity.ReminderDateTime = newDateTime;
                            reminderEntity.CreatedById = LoggedUserId;
                            reminderEntity.UpdatedById = LoggedUserId;
                            reminderEntity.CreatedOnUtc = GetDateTime;
                            reminderEntity.UpdatedOnUtc = GetDateTime;
                            _setReminderService.InsertReminder(reminderEntity);
                        }
                    }
                }
                else
                {
                    var entity = await _leadActivityLogsService.GetById(model.Id);
                    if (entity == null)
                    {
                        return BadRequest(new BadRequestError("No lead activity found with the specified id."));
                    }

                    entity.LeadsId = model.LeadsId;
                    entity.LeadActivityId = model.LeadActivityId;
                    entity.LeadStageId = model.LeadStageId;
                    entity.ActivityDate = Convert.ToDateTime(model.ActivityDate);
                    entity.ActivityNote = model.ActivityNote;
                    entity.IsFutureActivity = model.IsFutureActivity;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _leadActivityLogsService.UpdateLeadActivityLogs(entity);
                }
                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateLeadActivity
        [HttpPut("{id}/leadactivity")]
        public async Task<IActionResult> UpdateLeadActivity(string id, LeadActivityLogsModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var entity = await _leadActivityLogsService.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No lead activity found with the specified id."));

                entity = _mapper.Map(model, entity);
                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;
                _leadActivityLogsService.UpdateLeadActivityLogs(entity);

                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteLeadActivityLogs
        [HttpDelete("{Activityid}/leadactivity")]
        public async Task<IActionResult> DeleteLeadActivityLogs(string Activityid)
        {
            var entity = await _leadActivityLogsService.GetById(Activityid);
            if (entity == null)
                return BadRequest(new BadRequestError("No lead activity found with the specified id."));

            _leadActivityLogsService.DeleteLeadActivityLogs(entity);

            return NoContent();
        }
        #endregion
    }
}
