using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerBI.Api.Models;
using Org.BouncyCastle.Utilities;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.EmailNotifications;
using Vsky.Services.EmployeeDesignations;
using Vsky.Services.EmployeeLeaves;
using Vsky.Services.Employees;
using Vsky.Services.LeaveCredits;
using Vsky.Services.LeaveSchedule;
using Vsky.Services.Messages;
using Vsky.Services.Notifications;
using Vsky.Services.Sites;
using Vsky.Services.Users;

namespace Vsky.Api.Controllers
{
    [Route("employee-leave")]
    public class EmployeeLeaveController : BaseController
    {
        #region Define Services      
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IEmployeeLeaveService _employeeLeaveService;
        private readonly ICommonService _commonService;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeDesignationService _employeeDesignationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ISiteService _siteService;
        private readonly ILeaveCreditService _leaveCreditService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IMasterNotificationService _masterNotificationService;
        private readonly ILeaveScheduleService _leaveScheduleService;
        private readonly ISitesEmailNotificationsPermissionServices _sitesEmailNotificationsPermissionServices;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations      
        public EmployeeLeaveController(
            GlobalVariable globalVariable,
            IMapper mapper,
            IEmployeeLeaveService employeeLeaveService,
            ICommonService commonService,
            IDropDownService dropDownService,
            IDropDownTypeService dropDownTypeService,
            IWorkflowMessageService workflowMessageService,
            IEmployeeService employeeService,
            IEmployeeDesignationService employeeDesignationService,
            ILeaveCreditService leaveCreditService,
            ISiteService siteService,
            IUserService userService,
            INotificationService notificationService,
            IMasterNotificationService masterNotificationService,
            ILeaveScheduleService leaveScheduleService,
            ISitesEmailNotificationsPermissionServices sitesEmailNotificationsPermissionServices,
            IAzureBlobImageServices azureBlobImageServices
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _employeeLeaveService = employeeLeaveService;
            _commonService = commonService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _workflowMessageService = workflowMessageService;
            _employeeService = employeeService;
            _employeeDesignationService = employeeDesignationService;
            _leaveCreditService = leaveCreditService;
            _siteService = siteService;
            _userService = userService;
            _notificationService = notificationService;
            _masterNotificationService = masterNotificationService;
            _leaveScheduleService = leaveScheduleService;
            _sitesEmailNotificationsPermissionServices = sitesEmailNotificationsPermissionServices;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllEmployeeLeave
        // Title: Get All EmployeeLeave
        // Description: This endpoint fetches a list of EmployeeLeave based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllEmployeeLeave(EmployeeLeaveSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var createdBy = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                //var createdBy = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var list = _employeeLeaveService.GetAllEmployeeLeave(SiteId, LoggedUserId, createdBy, searchModel.SearchText, searchModel.Flag, searchModel.EmployeeIds, searchModel.StatusIds, searchModel.LeaveCategoryId, searchModel.CreatedOnUtc, searchModel.LeaveMonthStr, searchModel.Years, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                // Map the fetched list to a model suitable for the response
                var model = new EmployeeLeaveListModel
                {
                    Data = _mapper.Map<IList<EmployeeLeaveModel>>(list),
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

        #region GetFiveEmployeeLeaveForApprove
        // Title: Get Five EmployeeLeave For Approve
        [HttpPost("dashBoardLeave/list")]
        public async Task<IActionResult> GetFiveEmployeeLeaveForApprove()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var leavestatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Leave Status", "Sent to Approver");
                var activeEmployeeStatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Employee Status", "Current");
                var exEmployeeStatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Employee Status", "Ex-Employee");
                //var employeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);

                var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                var list = await _employeeLeaveService.GetFiveEmployeeLeaveForApprove(SiteId, employeeId, leavestatus, activeEmployeeStatus, exEmployeeStatus);
                var model = _mapper.Map<List<EmployeeLeaveModel>>(list);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllEmployeeLeaveForApprove
        // Title: Get All EmployeeLeave For Approve
        // Description: This endpoint fetches a list of EmployeeLeave for approve based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("forward/list")]
        public IActionResult GetAllEmployeeLeaveForApprove(EmployeeLeaveSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                //var employeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                // Fetch a list of EmployeeLeave on search criteria (name, sorting, pagination)
                var list = _employeeLeaveService.GetAllEmployeeLeaveForApprove(SiteId, employeeId, searchModel.SearchText, searchModel.EmployeeIds, searchModel.StatusIds, searchModel.CreatedOnUtc, searchModel.LeaveMonthStr, searchModel.Years, searchModel.LeaveCategoryId, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                // Map the fetched list to a model suitable for the response
                var model = new EmployeeLeaveListModel
                {
                    Data = _mapper.Map<IList<EmployeeLeaveModel>>(list),
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

        #region GetEmployeeLeaveDetailsById
        //Title: GetEmployeeLeaveDetailsById
        //Description: This endpoint retrieves the details of a specific Leave based on its unique identifier(ID). 
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEmployeeLeaveDetailsById(string id)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    // Fetch the Leave entity by its ID from the service
                    var entity = await _employeeLeaveService.GetEmployeeLeaveDetailsById(id);
                    // If the Leave entity is not found, return a BadRequest response with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Employee Leave found with the specified id."));

                    var leaveapprover = await _employeeDesignationService.GetEmployeeDesignationByEmployeeId(entity.Employee.Id);
                    // Map the Leave entity to a EmployeeLeaveModel object
                    var model = _mapper.Map<EmployeeLeaveModel>(entity);
                    if (leaveapprover != null)
                    {
                        model.LeaveApproverId = leaveapprover.LeaveApproverId;
                        model.ApproverName = leaveapprover.LeaveApprover?.Person.FullName;
                    }
                    return Ok(model);
                }
                else
                {
                    return Ok("Error");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetLeaveBalanceByEmployeeId
        //Title: GetLeaveBalanceByEmployeeId
        //Description: This endpoint retrieves the leave balance of a employee. 
        [HttpGet("leavebalance/{employeeId}")]
        public decimal GetLeaveBalanceByEmployeeId(string employeeId = null)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            int currentYear = DateTime.UtcNow.Year;

            if (string.IsNullOrWhiteSpace(employeeId) || employeeId == "undefined")
            {
                // Fetch the Leave entity by its ID from the service
                //employeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
            }
            var leavecredits = _leaveCreditService.GetLeaveCreditsByEmployeeId(employeeId, currentYear);
            var usedLeaves = _employeeLeaveService.GetUsedLeaveByEmployeeId(employeeId, currentYear);
            var leavebalance = leavecredits - usedLeaves;
            return leavebalance;
        }
        #endregion

        #region GetLeaveBalanceDetailsByEmployeeId
        [HttpGet("leavebalancedetails/{employeeId}/{year}")]
        public async Task<LeaveBalanceDetailsResponse> GetLeaveBalanceDetailsByEmployeeId(string employeeId = null, int year = 0)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
           
            // Determine the logged-in employee ID if not provided
            if (string.IsNullOrWhiteSpace(employeeId) || employeeId == "undefined" || employeeId == "null")
            {
                //employeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
            }

            // Get leave credits for the current year
            var currentYear = year > 0 ? year : DateTime.UtcNow.Year;
            var (totalLeaves, casualLeaves, sickLeaves) = await _leaveCreditService.GetAllLeaveCreditsByEmployeeId(employeeId, currentYear);

            // Get used leaves for the current year
            var usedLeaves = _employeeLeaveService.GetUsedLeaveByEmployeeId(employeeId, currentYear);

            // Calculate the leave balance
            var leaveBalance = totalLeaves - usedLeaves;

            var CasualLeaveId = await _dropDownService.GetByName(SiteId, "Casual");
            var SickLeaveId = await _dropDownService.GetByName(SiteId, "Sick");
            var totalUsedCasualLeaves = _employeeLeaveService.GetUsedLeaveByEmployeeIdAndLeaveCategoryId(employeeId, currentYear, CasualLeaveId.Id);
            var remainingCasualLeavs = casualLeaves - totalUsedCasualLeaves;
            var totalUsedSickLeaves = _employeeLeaveService.GetUsedLeaveByEmployeeIdAndLeaveCategoryId(employeeId, currentYear, SickLeaveId.Id);
            var remainingSickLeaves = sickLeaves - totalUsedSickLeaves;

            var leaveEvents = await _leaveScheduleService.GetAllLeaveEvents(SiteId);
            var officeLeaveDates = leaveEvents.Select(x => x.Date.Value.ToString("yyyy/MM/dd")).ToList();

            var employeeLeavesDates = await _employeeLeaveService.GetAllEmployeeLeaves(employeeId);
            var employeeLeaveFormattedDates = employeeLeavesDates.Select(x => x.FromDate.ToString("yyyy/MM/dd")).ToList();
            var employeeLeaveExcludingOfficeHolidays = employeeLeaveFormattedDates.Except(officeLeaveDates).ToList();

            // Return leave balance ensuring it doesn't go negative
            // Create the response object
            var response = new LeaveBalanceDetailsResponse
            {
                TotalLeaves = totalLeaves.ToString().Replace(".00", " "),
                CasualLeaves = totalUsedCasualLeaves.ToString().Replace(".00", " ") + "/" + casualLeaves.ToString().Replace(".00", " "),
                SickLeaves = totalUsedSickLeaves.ToString().Replace(".00", " ") + "/" + sickLeaves.ToString().Replace(".00", " "),
                LeaveBalance = leaveBalance > 0 ? leaveBalance : 0,
                OfficeLeaves = officeLeaveDates,
                EmployeeLeaves = employeeLeaveExcludingOfficeHolidays
            };
            return response;
        }
        #endregion

        #region GetEmployeeLeaveListForDashboard
        // Title: Get EmployeeLeave List
        [HttpPost("approvedLeave/list")]
        public async Task<IActionResult> GetEmployeeLeaveListForDashboard()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var list = await _employeeLeaveService.GetEmployeeLeaveListForDashboard(SiteId, GetDateTime);
                var model = _mapper.Map<List<EmployeeLeaveModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Title: Get EmployeeLeave List for Movement Register
        [HttpPost("leaveListForMovReg/list/{movRegDateStr}")]
        public async Task<IActionResult> GetEmployeeLeaveListForMovReg(string movRegDateStr)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var list = await _employeeLeaveService.GetEmployeeLeaveListForMovReg(SiteId, movRegDateStr, GetDateTime);
                var model = _mapper.Map<List<EmployeeLeaveModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region IsSandwichLeave
        [HttpGet("is-sandwich-leave/{startDate}/{endDate}")]
        public async Task<IActionResult> IsSandwichLeave(string startDate, string endDate)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
            var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

            bool isPreviousHoliday = false;
            bool isNextHoliday = false;
            List<SandwichLeaveResult> SandwichLeaveData = new List<SandwichLeaveResult>();

            DateTime startDt, endDt;

            if (!string.IsNullOrEmpty(startDate) && (string.IsNullOrEmpty(endDate) || endDate == "null"))
            {
                if (DateTime.TryParse(startDate, out startDt))
                {
                    isPreviousHoliday = await _employeeLeaveService.CheckPreviousHoliday(SiteId, EmployeeId, startDt);
                }
            }
            else if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                if (DateTime.TryParse(startDate, out startDt) && DateTime.TryParse(endDate, out endDt))
                {
                    SandwichLeaveResult result = await _employeeLeaveService.IsSandwichLeave(SiteId, EmployeeId, startDt, endDt);
                    if (result != null)
                    {
                        SandwichLeaveData.Add(result);
                    }
                }
            }

            return Ok(new
            {
                isPreviousHoliday,
                isNextHoliday,
                SandwichLeaveData
            });
        }
        #endregion

        #region CreateEmployeeLeave
        // Title: CreateEmployeeLeave
        // Description: This endpoint handles the creation of a new EmployeeLeave. It maps the EmployeeLeave model to the EmployeeLeave entity, sets the creation details, and inserts the EmployeeLeave into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateEmployeeLeave([FromForm] EmployeeLeaveModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Map the employee model to the employee entity
                    var entity = _mapper.Map<EmployeeLeave>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    //var employeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                    var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                    entity.EmployeeId = employeeId;

                    var employeeDetails = await _employeeService.GetEmployeeDetailsById(employeeId);
                    var currentYear = model.Year > 0 ? model.Year : DateTime.UtcNow.Year;
                    var CasualLeaveId = await _dropDownService.GetByName(SiteId, "Casual");
                    var SickLeaveId = await _dropDownService.GetByName(SiteId, "Sick");
                    var (totalLeaves, casualLeaves, sickLeaves) = await _leaveCreditService.GetAllLeaveCreditsByEmployeeId(employeeId, currentYear);

                    // check total leave credit balance
                    var employeeLeaveCreditExist = _leaveCreditService.GetLeaveCreditsByEmployeeId(employeeId, currentYear);
                    if (employeeLeaveCreditExist == 0)
                        return BadRequest(new BadRequestError("You have an insufficient leave balance."));

                    // check casual leave credit balance
                    if (model.LeaveCategoryId == CasualLeaveId.Id)
                    {
                        var totalUsedCasualLeaves = _employeeLeaveService.GetUsedLeaveByEmployeeIdAndLeaveCategoryId(employeeId, currentYear, CasualLeaveId.Id);
                        var remainingCasualLeavs = casualLeaves - totalUsedCasualLeaves;
                        if (model.NoofLeaves > remainingCasualLeavs || remainingCasualLeavs == 0 || model.TotalDeduction > remainingCasualLeavs)
                            return BadRequest(new BadRequestError("You have an insufficient casual leave balance."));
                    }
                    else
                    {
                        // check sick leave credit balance
                        var totalUsedSickLeaves = _employeeLeaveService.GetUsedLeaveByEmployeeIdAndLeaveCategoryId(employeeId, currentYear, SickLeaveId.Id);
                        var remainingSickLeaves = sickLeaves - totalUsedSickLeaves;
                        if (model.NoofLeaves > remainingSickLeaves || remainingSickLeaves == 0 || model.TotalDeduction > remainingSickLeaves)
                            return BadRequest(new BadRequestError("You have an insufficient sick leave balance."));
                    }

                    var (paid, unpaid) = _leaveCreditService.GetLeaveCreditsByEmployeeIdandType(SiteId, employeeId, currentYear);
                    var usedleaves = _employeeLeaveService.GetUsedLeaveByEmployeeId(employeeId, currentYear);
                    var leaveapproverId = await _employeeDesignationService.GetEmployeeDesignationByEmployeeId(entity.EmployeeId);

                    if (paid > usedleaves)
                        entity.IsPaidLeave = true;
                    else
                        entity.IsPaidLeave = false;

                    if (leaveapproverId != null)
                        entity.LeaveApproverId = leaveapproverId.LeaveApproverId;

                    if (model.FilePic != null && model.FilePic.Length > 0)
                    {
                        var file = model.FilePic;
                        var originalFileName = Path.GetFileName(file.FileName);
                        var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                        var files = new List<IFormFile> { model.FilePic };
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "leave-apply", files, entity.Id);

                        if (urls != null && urls.Any())
                        {
                            foreach (var blobUrl in urls)
                            {
                                var picture = new Picture
                                {
                                    SiteId = SiteId,
                                    Type = "EmployeeLeave",
                                    SeoFilename = originalFileName,
                                    MimeType = mimeType,
                                    VirtualPath = blobUrl,
                                    ModuleId = entity.Id,
                                    Module = employeeDetails.Person.FullName,
                                    SubModuleId = entity.Id,
                                    Sub_Module = employeeDetails.Person.FullName,
                                    CreatedOnUtc = GetDateTime,
                                    CreatedById = LoggedUserId
                                };

                                _commonService.InsertPicture(picture);

                                entity.FileId = picture.Id;
                            }
                        }
                    }
                    else
                    {
                        entity.FileId = null;
                    }

                    // Set custom properties
                    if (model.FromDateStr != "" && model.FromDateStr != null)
                        entity.FromDate = DateTime.ParseExact(model.FromDateStr, "MM/dd/yyyy", null);
                    if (model.ToDateStr != "" && model.ToDateStr != null)
                        entity.ToDate = DateTime.ParseExact(model.ToDateStr, "MM/dd/yyyy", null);

                    entity.IsHalfDay = model.IsHalfDay;
                    if (entity.IsHalfDay == true)
                    {
                        if (model.HalfDay == true)
                        {
                            entity.HalfDayType = "1st Half";
                        }
                        else
                        {
                            entity.HalfDayType = "2nd Half";
                        }
                    }

                    var status = await _dropDownTypeService.GetDropDownTypeByType(SiteId, "Leave Status");
                    var leavestatus = await _dropDownService.GetDropDownByTypeAndValue(SiteId, status.Id, "Applied");
                    entity.LeaveStatusId = leavestatus.Id;
                    entity.LeaveCategoryId = model.LeaveCategoryId;
                    entity.NoofLeaves = model.NoofLeaves;
                    entity.Reason = model.Reason;
                    entity.IsSandwich = model.IsSandwich;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _employeeLeaveService.InsertEmployeeLeave(entity);

                    var employeeleave = await _employeeLeaveService.GetEmployeeLeaveDetailsById(entity.Id);
                    var Hr = await _userService.GetUsersByRole(SiteId, "Hr");

                    foreach (var user in Hr)
                    {
                        // check mail notification send or not
                        var canSendEmail = await _sitesEmailNotificationsPermissionServices
                            .ShouldSendNotification(
                                SiteId,
                                user.Id,
                                "Leave.SendToHR"
                            );

                        if (!canSendEmail)
                            continue;

                        //var employee = _commonService.GetEmployeeIdByUserId(SiteId, user.Id);
                        var employee = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, user.Id);
                        var employeeData = await _employeeService.GetEmployeeDetailsById(employee);
                        // Send an email with the new password if the password reset was successful
                        await _workflowMessageService.SendMailToHr(employeeData, employeeleave);

                        var HrEmployee = _commonService.GetLoggeduserIdByEmployeeId(SiteId, employee);
                        var MasterNotificationData = await _masterNotificationService.GetMasterNotificationByNumber(SiteId, "ForwardLeave1", HrEmployee);
                        if (MasterNotificationData != null)
                        {
                            string message = MasterNotificationData.Message
                                            .Replace("[Employee Name ]", employeeleave.Employee.Person.FirstName + " " + employeeleave.Employee.Person.LastName)
                                            .Replace("[Leave Type]", employeeleave.LeaveCategories.DropDownValue);
                            var Notification = _notificationService.AddNotification(SiteId, MasterNotificationData.Title, message, MasterNotificationData.Type, entity.CreatedById, entity.Id, "/forward-leaves", HrEmployee, entity.CreatedById, GetDateTime);
                        }
                    }
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            // Return model state errors if the model state is not valid
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateEmployeeLeave
        // Title: Update EmployeeLeave
        // Description: This endpoint updates an existing EmployeeLeave by its ID. It validates the EmployeeLeave model, updates the EmployeeLeave's details.
        [HttpPut("{ids}")]
        [AllowAnonymous]

        public async Task<IActionResult> UpdateEmployeeLeave(string ids, EmployeeLeaveModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var status = await _dropDownTypeService.GetDropDownTypeByType(SiteId, "Leave Status");
                    var leavestatus = await _dropDownService.GetDropDownByTypeAndValue(SiteId, status.Id, "Sent to Approver");
                    var declineleave = await _dropDownService.GetDropDownByTypeAndValue(SiteId, status.Id, "Decline");
                    var approvedleave = await _dropDownService.GetDropDownByTypeAndValue(SiteId, status.Id, "Approved");

                    var leaveIdsList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

                    foreach (var leaveId in leaveIdsList)
                    {
                        // Fetch the EmployeeLeave entity by its ID
                        var entity = await _employeeLeaveService.GetEmployeeLeaveById(leaveId);
                        var employeeleave = await _employeeLeaveService.GetEmployeeLeaveDetailsById(leaveId);
                        var employee = await _employeeService.GetEmployeeDetailsById(model.LeaveApproverId);

                        // If no EmployeeLeave is not found with the given ID, return a bad request with an error message
                        if (entity == null)
                            return BadRequest(new BadRequestError("No Employee Leave found with the specified id."));                       

                        if (model.Flag == "FD")
                        {
                            entity.LeaveStatusId = leavestatus.Id;
                            employeeleave.HRNote = model.HRNote;
                            entity.IsPaidLeave = model.IsPaidLeave;
                            entity.LeaveApproverId = model.LeaveApproverId;
                            entity.HRNote = model.HRNote;
                            entity.UpdatedById = LoggedUserId;
                            entity.UpdatedOnUtc = GetDateTime;
                            _employeeLeaveService.UpdateEmployeeLeave(entity);

                            // check email send or not
                            var approverUserId = _commonService.GetLoggeduserIdByEmployeeId(SiteId, model.LeaveApproverId);
                            bool canSendApproverMail = true;

                            if (!string.IsNullOrEmpty(approverUserId))
                            {
                                canSendApproverMail = await _sitesEmailNotificationsPermissionServices
                                    .ShouldSendNotification(
                                        SiteId,
                                        approverUserId,
                                        "Leave.Forward"
                                    );

                                if(canSendApproverMail)
                                {
                                    // Send an email with the new password if the password reset was successful
                                    await _workflowMessageService.SendMailToApprover(employee, employeeleave);
                                }
                            }

                            var ToUserId = _commonService.GetLoggeduserIdByEmployeeId(SiteId, model.LeaveApproverId);
                            var MasterNotificationData = await _masterNotificationService.GetMasterNotificationByNumber(SiteId, "ApproveLeave1", ToUserId);
                            if (MasterNotificationData != null)
                            {
                                string message = MasterNotificationData.Message
                                                .Replace("[Employee Name]", employeeleave.Employee.Person.FirstName + " " + employeeleave.Employee.Person.LastName);
                                var Notification = _notificationService.AddNotification(SiteId, MasterNotificationData.Title, message, MasterNotificationData.Type, LoggedUserId, entity.Id, "/approve-leaves", ToUserId, LoggedUserId, GetDateTime);
                            }

                        }
                        else if (model.Flag == "AV")
                        {
                            bool isDeclined = model.LeaveStatusFlag == "DC";

                            if (model.LeaveStatusFlag == "DC")
                            {
                                entity.LeaveStatusId = declineleave.Id;
                            }
                            else
                                entity.LeaveStatusId = approvedleave.Id;

                            entity.IsPaidLeave = model.IsPaidLeave;
                            entity.ApproverNote = model.ApproverNote;
                            entity.UpdatedById = LoggedUserId;
                            entity.UpdatedOnUtc = GetDateTime;
                            _employeeLeaveService.UpdateEmployeeLeave(entity);

                            if (model.LeaveStatusFlag == "DC")
                                await AdjustLeaveBalanceOnStatusChange(entity);

                            var employeeleaveData = await _employeeLeaveService.GetEmployeeLeaveDetailsById(entity.Id);
                            var employeeData = await _employeeService.GetEmployeeDetailsById(employeeleaveData.Employee.Id);

                            // check email send or not
                            var employeeUserId = _commonService.GetLoggeduserIdByEmployeeId(SiteId, employeeleaveData.Employee.Id);
                            bool canSendEmployeeMail = true;

                            if (!string.IsNullOrEmpty(employeeUserId))
                            {
                                string templateName =
                                    isDeclined
                                        ? "Leave.SendLeaveStatusToEmployee"
                                        : "Leave.SendLeaveApprovalToEmployee";

                                canSendEmployeeMail = await _sitesEmailNotificationsPermissionServices
                                    .ShouldSendNotification(
                                        SiteId,
                                        employeeUserId,
                                        templateName
                                    );
                            }

                            if (canSendEmployeeMail)
                            {
                                await _workflowMessageService.SendLeaveStatusEmailToEmployee(employeeData, employeeleaveData);
                            }

                            var NotificationNumber = "";
                            if (employeeleaveData.LeaveStatuses.DropDownValue == "Approved")
                                NotificationNumber = "LeaveConfirmation1";
                            else
                                NotificationNumber = "LeaveConfirmation2";

                            var ToUserId = _commonService.GetLoggeduserIdByEmployeeId(SiteId, employeeleaveData.Employee.Id);
                            var MasterNotificationData = await _masterNotificationService.GetMasterNotificationByNumber(SiteId, NotificationNumber, ToUserId);
                            if (MasterNotificationData != null)
                            {
                                var Notification = _notificationService.AddNotification(SiteId, MasterNotificationData.Title, MasterNotificationData.Message, MasterNotificationData.Type, LoggedUserId, entity.Id, "/apply-leave", ToUserId, LoggedUserId, GetDateTime);
                            }
                        }
                    }

                    return NoContent();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            // Return model state errors if the model state is not valid
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteEmployeeLeave
        // Title: DeleteEmployeeLeaveById
        // Description: This endpoint deletes a employee leave based on the provided employee leave ID. It first retrieves the employee leave entity by ID, checks if it exists, and if so, deletes the employee leave. If the employee leave is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeeLeave(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();

                // Fetch the employee leave entity by its ID
                var entity = await _employeeLeaveService.GetEmployeeLeaveById(id);
                // If no employee leave is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No employee leave found with the specified id."));

                await AdjustLeaveBalanceOnStatusChange(entity);

                // Delete the employee leave using the employee leave service
                _employeeLeaveService.DeleteEmployeeLeave(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CancelEmployeeLeave
        // Title: CancelEmployeeLeaveById
        // Description: This endpoint cancel a employee leave based on the provided employee leave ID. It first retrieves the employee leave entity by ID, checks if it exists, and if so, cancels the employee leave. If the employee leave is not found, it returns a BadRequest response with an error message.
        [HttpPost("cancelleave/{id}")]
        public async Task<IActionResult> CancelEmployeeLeave(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                // Fetch the EmployeeLeave entity by its ID
                var entity = await _employeeLeaveService.GetEmployeeLeaveById(id);
                // If no EmployeeLeave is not found with the given ID, return a bad request with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Employee Leave found with the specified id."));

                var status = await _dropDownTypeService.GetDropDownTypeByType(SiteId, "Leave Status");
                var cancelledleave = await _dropDownService.GetDropDownByTypeAndValue(SiteId, status.Id, "Cancelled");

                await AdjustLeaveBalanceOnStatusChange(entity);

                entity.LeaveStatusId = cancelledleave.Id;
                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;
                _employeeLeaveService.UpdateEmployeeLeave(entity);

                var employeeleaveData = await _employeeLeaveService.GetEmployeeLeaveDetailsById(id);
                var employeeData = await _employeeService.GetEmployeeDetailsById(employeeleaveData.Employee.Id);

                // check email send or not
                var employeeUserId = _commonService.GetLoggeduserIdByEmployeeId(SiteId, employeeleaveData.Employee.Id);
                bool canSendEmail = true;
                if (!string.IsNullOrEmpty(employeeUserId))
                {
                    canSendEmail = await _sitesEmailNotificationsPermissionServices
                        .ShouldSendNotification(
                            SiteId,
                            employeeUserId,
                            "Leave.SendLeaveCancellationToEmployee"
                        );
                }

                if (canSendEmail)
                {
                    await _workflowMessageService.SendLeaveStatusEmailToEmployee(employeeData, employeeleaveData);
                }

                var ToUserId = _commonService.GetLoggeduserIdByEmployeeId(SiteId, employeeleaveData.Employee.Id);
                var MasterNotificationData = await _masterNotificationService.GetMasterNotificationByNumber(SiteId, "LeaveCancellation1", ToUserId);
                if (MasterNotificationData != null)
                {
                    var Notification = _notificationService.AddNotification(SiteId, MasterNotificationData.Title, MasterNotificationData.Message, MasterNotificationData.Type, LoggedUserId, entity.Id, "/apply-leave", ToUserId, LoggedUserId, GetDateTime);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region private methods
        private static List<DateTime> GetDateRange(DateTime fromDate, DateTime toDate)
        {
            fromDate = fromDate.Date;
            toDate = toDate.Date;

            if (fromDate > toDate)
                throw new ArgumentException("fromDate cannot be greater than toDate.");

            var dates = new List<DateTime>();
            for (var d = fromDate; d <= toDate; d = d.AddDays(1))
                dates.Add(d);

            return dates;
        }
        private async Task AdjustLeaveBalanceOnStatusChange(EmployeeLeave entity)
        {
            //try
            //{
            //var officeLeaves = await _employeeLeaveService.GetAllOfficeLeaves(SiteId, entity.FromDate, entity.ToDate);
            var currentLeaveDates = GetDateRange(entity.FromDate, entity.ToDate);

            // Find sandwich leaves that include this date
            var matchingSandwichLeaves =
                await _employeeLeaveService.GetEmployeeLeavesThatIncludeDates(
                    entity.EmployeeId,
                    currentLeaveDates, entity.Id
                );

            //foreach (var sandwich in matchingSandwichLeaves)
            //{
            if (matchingSandwichLeaves?.Count > 0)
            {
                var sandwich = matchingSandwichLeaves[0];
                // Recalculate date range of sandwich leave
                var sandwichDates = GetDateRange(sandwich.FromDate, sandwich.ToDate);

                // Sandwich actual deducted = total days - excluded holidays
                int actualSandwichDays = sandwichDates.Count;
                // Check how many current leave days are found in the sandwich leave
                var overlappingDays = currentLeaveDates.Intersect(sandwichDates).Count();
                // Combine the current leave balance and the sandwich leave balance for that sandwich.
                var totalDays = overlappingDays + sandwich.NoofLeaves;
                // If all current-leave days are found inside the sandwich leave, then return true.
                bool isFullyContained = currentLeaveDates.All(d => sandwichDates.Contains(d));

                if (actualSandwichDays > sandwich.NoofLeaves && actualSandwichDays >= totalDays)
                {
                    var employeeLeave = await _employeeLeaveService.GetEmployeeLeaveById(sandwich.Id);

                    employeeLeave.NoofLeaves += isFullyContained ? entity.NoofLeaves : overlappingDays;
                    _employeeLeaveService.UpdateEmployeeLeave(employeeLeave);
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest(ex.Message);
            //}
        }
        #endregion
    }
}
