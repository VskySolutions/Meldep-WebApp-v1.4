using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AutoMapper;
using Ical.Net.DataTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerBI.Api;
using MimeKit;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.EmployeeDesignations;
using Vsky.Services.Employees;
using Vsky.Services.Messages;
using Vsky.Services.MovementRegisters;
using Vsky.Services.Notifications;
using Vsky.Services.Sites;
using Vsky.Services.TestPlans;
using Vsky.Services.TimeInTimeOuts;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Vsky.Api.Models.EmployeeModel;

namespace Vsky.Api.Controllers
{
    [Route("time-in-time-out")]
    public class TimeInTimeOutController : BaseController
    {

        #region Define Services      
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ITimeInTimeOutService _timeInTimeOutService;
        private readonly ITimeInTimeOutBreakDetailService _timeInTimeOutBreakDetailService;
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeDesignationService _employeeDesignationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IMasterNotificationService _masterNotificationService;
        private readonly INotificationService _notificationService;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IMovementRegisterServices _movementRegisterService;
        private readonly IMovementRegisterDetailsService _movementRegisterDetailsService;
        #endregion

        #region Services Initializations      
        public TimeInTimeOutController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ITimeInTimeOutService timeInTimeOutService,
            ITimeInTimeOutBreakDetailService timeInTimeOutBreakDetailService,
            IEmployeeService employeeService,
            IEmployeeDesignationService employeeDesignationService,
            IWorkflowMessageService workflowMessageService,
            IMasterNotificationService masterNotificationService,
            INotificationService notificationService,
            IDropDownService dropDownService,
            IDropDownTypeService dropDownTypeService,
            ISiteService siteService,
            ICommonService commonService,
            IMovementRegisterServices movementRegisterServices,
            IMovementRegisterDetailsService movementRegisterDetailsService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _timeInTimeOutService = timeInTimeOutService;
            _timeInTimeOutBreakDetailService = timeInTimeOutBreakDetailService;
            _employeeService = employeeService;
            _employeeDesignationService = employeeDesignationService;
            _workflowMessageService = workflowMessageService;
            _masterNotificationService = masterNotificationService;
            _notificationService = notificationService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _siteService = siteService;
            _commonService = commonService;
            _movementRegisterService = movementRegisterServices;
            _movementRegisterDetailsService = movementRegisterDetailsService;
        }
        #endregion

        #region GetAllTimeInTimeOuts
        // Title: Get All TimeInTimeOuts
        // Description: This endpoint fetches a list of TimeInTimeOuts based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllTimeInTimeOuts(TimeInTimeOutSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";

                var list = _timeInTimeOutService.GetAllTimeInTimeOuts(
                    SiteId,
                    createdBy,
                     searchModel.EmployeeId,
                    searchModel.SearchText,
                    searchModel.FromDate,
                    searchModel.ToDate,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                // Map the fetched list to a model suitable for the response
                var model = new TimeInTimeOutListModel
                {
                    Data = _mapper.Map<IList<TimeInTimeOutModel>>(list),
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

        #region GetTimeInTimeOutDetailsByEmployeeId
        [HttpGet("{employeeId}")]
        public async Task<IActionResult> GetTimeInTimeOutDetailsByEmployeeId(string employeeId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var entity = await _timeInTimeOutService.GetTimeInTimeOutByEmployeeId(SiteId,employeeId);
                var model = _mapper.Map<TimeInTimeOutModel>(entity);
               
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetTimeInTimeOutDetailsById
        // Title: GetTimeInTimeOutDetailsById
        // Description: This endpoint retrieves the details of a specific TimeInTimeOut based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetTimeInTimeOutDetailsById(string id)
        {
            try
            {
                // Fetch the TimeInTimeOut entity by its ID from the service
                var entity = await _timeInTimeOutService.GetTimeInTimeOutDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No time in time out found with the specified id."));

                // Map the TimeInTimeOut entity to a TimeInTimeOutModel object
                var model = _mapper.Map<TimeInTimeOutModel>(entity);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateTimeInTimeOut
        // Title: CreateTimeInTimeOut
        // Description: This endpoint handles the creation of a new time in time out. It maps the time in time out model to the time in time out entity, sets the creation details, and inserts the time in time out into the database. 
        [HttpPost]
        public async Task<IActionResult> InsertTimeIn()
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                    var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                    if (EmployeeId == null)
                        return BadRequest("Employee not found. Please create employee.");

                    var entity = new TimeInTimeOut
                    {
                        EmployeeId = EmployeeId,
                        SiteId = SiteId,
                        TimeIn = GetDateTime.TimeOfDay,        // current time in 24-hour format
                        TimeInDate = GetDateTime.Date,         // current date
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime
                    };
                    _timeInTimeOutService.InsertTimeInTimeOut(entity);

                    return Ok();
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

        #region UpdateTimeInTimeOut
        // Title: UpdateTimeInTimeOut
        // Description: This endpoint updates an existing time in time out by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimeOut(string id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the time in time out entity by its ID
                    var entity = await _timeInTimeOutService.GetTimeInTimeOutById(id);
                    
                    // If no time in time out is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No time in time out found with the specified id."));
                   
                    var breakInOutList = await _timeInTimeOutBreakDetailService.GetBreakInOutByTimeInOutId(entity.Id);
                    TimeSpan totalBreak = TimeSpan.Zero;

                    if (breakInOutList != null && breakInOutList.Any())
                    {
                        foreach (var item in breakInOutList)
                        {
                            if (item.BreakIn != TimeSpan.Zero && item.BreakOut != TimeSpan.Zero)
                            {
                                totalBreak += item.BreakOut - item.BreakIn;
                            }
                        }
                    }

                    entity.TimeOut = GetDateTime.TimeOfDay;
                    entity.TimeOutDate = GetDateTime.Date;
                    entity.TotalBreak = totalBreak;

                    //TimeSpan timeOut = entity.TimeOut;

                    //// Handle overnight shift
                    //if (timeOut < entity.TimeIn)
                    //{
                    //    timeOut = timeOut.Add(TimeSpan.FromHours(24));
                    //}
                    //// Calculate total hours
                    //entity.TotalHours = TimeSpan.FromSeconds(
                    //    Math.Round((timeOut - entity.TimeIn).TotalSeconds)
                    //);

                    //// Prevent negative actual hours
                    //entity.ActualHours = entity.TotalHours > entity.TotalBreak
                    //    ? TimeSpan.FromSeconds(
                    //        Math.Round((entity.TotalHours - entity.TotalBreak).TotalSeconds)
                    //      )
                    //    : TimeSpan.Zero;

                    DateTime timeInDateTime = entity.TimeInDate.Value.Date + entity.TimeIn;

                    DateTime timeOutDateTime = entity.TimeOutDate.Value.Date + entity.TimeOut;

                    // Calculate duration
                    TimeSpan totalHours = timeOutDateTime - timeInDateTime;

                   
                    // If duration exceeds 24 hours
                    if (totalHours >= TimeSpan.FromDays(1))
                    {
                        totalHours = new TimeSpan(23, 59, 59);
                    }

                    // Store TotalHours
                    entity.TotalHours = totalHours;

                    // Calculate ActualHours
                    TimeSpan actualHours =
                        totalHours > entity.TotalBreak
                            ? totalHours - entity.TotalBreak
                            : TimeSpan.Zero;

                    // Prevent ActualHours exceeding SQL time limit
                    if (actualHours >= TimeSpan.FromDays(1))
                    {
                        actualHours = new TimeSpan(23, 59, 59); ;
                    }

                    entity.ActualHours = actualHours;


                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    _timeInTimeOutService.UpdateTimeInTimeOut(entity);

                    return Ok();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region AddUpdateBreak
        [HttpPut("{id}/addUpdateBreak")]
        public async Task<IActionResult> AddUpdateBreak(string id, TimeInTimeOutModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                    var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                    TimeSpan totalBreak = TimeSpan.Zero;
                    var entity = await _timeInTimeOutService.GetTimeInTimeOutById(id);
                    if (entity == null)
                    {
                        entity = new TimeInTimeOut
                        {
                            Id = Guid.NewGuid().ToString(),
                            EmployeeId = EmployeeId,
                            TimeInDate = GetDateTime,
                            CreatedById = LoggedUserId,
                            UpdatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime,
                            UpdatedOnUtc = GetDateTime
                        };

                        _timeInTimeOutService.InsertTimeInTimeOut(entity);
                    }
                 
                        // Manage Break Details
                        if (model.TimeInTimeOutBreakDetailModel != null && model.TimeInTimeOutBreakDetailModel.Count() > 0)
                        {
                            var addList = new List<TimeInTimeOutBreakDetail>();
                            var deleteList = new List<TimeInTimeOutBreakDetail>();
                            var updateList = new List<TimeInTimeOutBreakDetail>();

                            // Loop through each Daily Planner Line
                            foreach (var time in model.TimeInTimeOutBreakDetailModel)
                            {
                                var existingtime = await _timeInTimeOutBreakDetailService.GetTimeInTimeOutBreakById(time.Id);

                                if (time.Flag == "Edit")
                                {
                                    if (existingtime == null)
                                        continue;

                                    existingtime.BreakIn = GetDateTime.TimeOfDay;
                                    existingtime.BreakReason = existingtime.BreakReason;


                                    if (existingtime.BreakIn != TimeSpan.Zero && existingtime.BreakOut != TimeSpan.Zero)
                                    {
                                        // Handle overnight breaks (crossing midnight)
                                        if (existingtime.BreakOut > existingtime.BreakIn)
                                            existingtime.BreakOut = existingtime.BreakOut.Add(new TimeSpan(24, 0, 0));

                                        existingtime.TotalBreak = TimeSpan.FromSeconds(Math.Round((existingtime.BreakIn - existingtime.BreakOut).TotalSeconds));
                                        totalBreak += existingtime.TotalBreak;

                                    }
                                    existingtime.UpdatedById = LoggedUserId;
                                    existingtime.UpdatedOnUtc = GetDateTime;
                                    updateList.Add(existingtime);
                                }
                                else if (time.Flag == "New")
                                {
                                    if (existingtime != null)
                                        continue;

                                    var timeEntity = _mapper.Map<TimeInTimeOutBreakDetail>(time);
                                    timeEntity.TimeInTimeOutId = entity.Id;

                                    timeEntity.BreakOut = GetDateTime.TimeOfDay;
                                    timeEntity.BreakReason = time.BreakReason;

                                    var movementRegister = await _movementRegisterService.GetMovementRegisterByDate(SiteId, GetDateTime.Date);
                                    if (movementRegister == null)
                                    {
                                        movementRegister = new MovementRegister
                                        {
                                            SiteId = SiteId,
                                            Date = GetDateTime.Date,
                                            CreatedById = LoggedUserId,
                                            CreatedOnUtc = GetDateTime
                                        };

                                        _movementRegisterService.InsertMovementRegister(movementRegister);
                                    }
                                    
                                    // Insert into MovementRegisterDetails
                                    var movementRegisterDetails = new MovementRegisterDetails
                                    {

                                        MomentRegisterId = movementRegister.Id,
                                        ApproverById = time.ApproverById,
                                        TypeId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Type", time.Type),
                                        NotifyToStakeholders = time.NotifyToStakeholders,
                                        EmployeeId = entity.EmployeeId,
                                        Message = time.BreakReason,

                                        CreatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime,
                                        UpdatedById = LoggedUserId,
                                        UpdatedOnUtc = GetDateTime
                                    };

                                    _movementRegisterDetailsService.InsertMovementRegisterDetails(movementRegisterDetails);

                                    timeEntity.MovementRegisterId = movementRegister.Id;
                                    timeEntity.CreatedById = LoggedUserId;
                                    timeEntity.CreatedOnUtc = GetDateTime;
                                    timeEntity.UpdatedById = LoggedUserId;
                                    timeEntity.UpdatedOnUtc = GetDateTime;
                                    addList.Add(timeEntity);
                                }
                                else if (time.Flag == "Delete")
                                {
                                    if (existingtime == null)
                                        continue;

                                    deleteList.Add(existingtime);
                                }
                            }

                            if (addList.Count > 0)
                            {
                                _timeInTimeOutBreakDetailService.InsertTimeInTimeOutBreakDetailList(addList);
                            }

                            if (updateList.Count > 0)
                            {
                                _timeInTimeOutBreakDetailService.UpdateTimeInTimeOutBreakDetailList(updateList);
                            }

                            if (deleteList.Count > 0)
                            {
                                _timeInTimeOutBreakDetailService.DeleteTimeInTimeOutBreakDetailList(deleteList);
                            }
                        }

                        // Calculate ActualHours = TotalHours - TotalBreak
                        entity.TotalBreak = entity.TotalBreak.Add(totalBreak);
                        //entity.ActualHours = entity.TotalHours - totalBreak;
                        entity.UpdatedById = LoggedUserId;
                        entity.UpdatedOnUtc = GetDateTime;
                        _timeInTimeOutService.UpdateTimeInTimeOut(entity);

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


        #region private functions
       

        private TimeSpan ParseTime(string timeOutStr)
        {

            if (string.IsNullOrWhiteSpace(timeOutStr))
                return TimeSpan.Zero;

            // Parse strictly as 12-hour format (e.g., "01:05 AM", "11:30 PM")
            if (DateTime.TryParseExact(timeOutStr, "hh:mm tt",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
            {
                // Extract only the time portion as 24-hour TimeSpan (no date info)
                return new TimeSpan(dateTime.Hour, dateTime.Minute, 0);
            }

            return TimeSpan.Zero;
        }

        #endregion

    }
}