using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Api.Models.ExpenseModels;
using Vsky.Data;
using Vsky.Models;
using Vsky.Models.Expens;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.EmailNotifications;
using Vsky.Services.Expences;
using Vsky.Services.ExpenseExpensExpense_Lines;
using Vsky.Services.ExpenseLines;
using Vsky.Services.Messages;
using Vsky.Services.Notifications;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.Users;
using static Vsky.Api.Models.ExpenseModels.Expense_Advance_Requests_Models;


namespace Vsky.Api.Controllers
{
    [Route("advance-expense-request")]
    [ApiController]
    public class ExpenseAdvanceRequestController : BaseController
    {
        #region Fields
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IExpenseService _expensesService;
        private readonly ICommonService _commonService;
        private readonly IDropDownService _dropDownService;
        private readonly ISiteService _siteService;
        private readonly ApplicationDbContext _db;
        private readonly IExpenseFileService _expenseFileService;
        private readonly IExpense_Advance_Requests_Service _expense_Advance_Requests_Service;
        private readonly IMasterNotificationService _masterNotificationService;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly IExpenseAdvanceRequestFilesService _expenseAdvanceRequestFilesService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly ISitesEmailNotificationsPermissionServices _sitesEmailNotificationsPermissionServices;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Ctor
        public ExpenseAdvanceRequestController
        (
            GlobalVariable globalVariable,
            IMapper mapper,
            IExpenseService expensesService,
            ICommonService commonService,
            IDropDownService dropDownService, 
            ISiteService siteService,
            ApplicationDbContext db, 
            IExpenseFileService expenseFileService,
            IExpense_Advance_Requests_Service expense_Advance_Requests_Service, 
            IMasterNotificationService masterNotificationService,
            INotificationService notificationService,
            IUserService userService,
            IWorkflowMessageService workflowMessageService,
            ISitesModifiedLogsService sitesModifiedLogsService,
            IExpenseAdvanceRequestFilesService expenseAdvanceRequestFilesService,
            IDropDownTypeService dropDownTypeService,
            ISitesEmailNotificationsPermissionServices sitesEmailNotificationsPermissionServices,
            IAzureBlobImageServices azureBlobImageServices
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _expensesService = expensesService;
            _commonService = commonService;
            _dropDownService = dropDownService;
            _siteService = siteService;
            _db = db;
            _expenseFileService = expenseFileService;
            _expense_Advance_Requests_Service = expense_Advance_Requests_Service;
            _masterNotificationService = masterNotificationService;
            _notificationService = notificationService;
            _userService = userService;
            _workflowMessageService = workflowMessageService;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _expenseAdvanceRequestFilesService = expenseAdvanceRequestFilesService;
            _dropDownTypeService = dropDownTypeService;
            _sitesEmailNotificationsPermissionServices = sitesEmailNotificationsPermissionServices;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region Get All Advance Expense Requests
        [HttpPost("list")]
        public async Task<IActionResult> GetAllAdvanceExpenseRequests(Expense_Advance_RequestsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var list = await _expense_Advance_Requests_Service.GetAllAdvanceExpenseRequests(
                    SiteId,
                    false,
                    LoggedUserId, 
                    searchModel.SearchText,
                    searchModel.ReferenceId,
                    searchModel.RequestDate,
                    searchModel.StatusId,
                    searchModel.EmployeeId,
                    searchModel.SortBy, 
                    searchModel.Descending,
                    null, 
                    searchModel.Page, 
                    searchModel.PageSize
                );

                var model = new Expense_Advance_Requests_ListModel
                {
                    Data = _mapper.Map<IList<Expense_Advance_Requests_Models>>(list),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region Get All Approve Advance Expense Requests
        [HttpPost("approve-advance-expense-list")]
        public async Task<IActionResult> GetAllApproveAdvanceExpenseRequests(Expense_Advance_RequestsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                string statusId = null;
                if (!string.IsNullOrEmpty(searchModel.StatusName))
                    statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Advance Expense Status", searchModel.StatusName);

                var list = await _expense_Advance_Requests_Service.GetAllAdvanceExpenseRequests(
                    SiteId,
                    true,
                    LoggedUserId, 
                    searchModel.SearchText,
                    searchModel.ReferenceId, 
                    searchModel.RequestDate,
                    searchModel.StatusId, 
                    searchModel.EmployeeId, 
                    searchModel.SortBy, 
                    searchModel.Descending, 
                    statusId, 
                    searchModel.Page, 
                    searchModel.PageSize
                );

                var model = new Expense_Advance_Requests_ListModel
                {
                    Data = _mapper.Map<IList<Expense_Advance_Requests_Models>>(list),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region Get By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var entity = await _expense_Advance_Requests_Service.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No data found with the specified id."));

                var model = _mapper.Map<Expense_Advance_Requests_Models>(entity);

                return Ok(entity);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region Get Advance Expense Details By Id
        [HttpGet("get-advance-expense-details/{id}")]
        public async Task<IActionResult> GetAdvanceExpenseDetailsById(string id)
        {
            try
            {
                var entity = await _expense_Advance_Requests_Service.GetAdvanceExpenseDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No data found with the specified id."));

                var model = _mapper.Map<Expense_Advance_Requests_Models>(entity);

                return Ok(entity);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion
         
        #region Get Advance Expense Request Dropdown List
        [HttpGet("advance-expense-dropdown-list/{statusName}")]
        public async Task<IActionResult> GetAdvanceExpenseRequestList(string statusName = "")
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var statusId = "";

                if(!string.IsNullOrEmpty(statusName) && statusName != "undefined")
                    statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Advance Expense Status", statusName);

                var list = await _expense_Advance_Requests_Service.GetExpenseAdvanceListForDropdown(SiteId, LoggedUserId, statusId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Create Advance Expense Request
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Expense_Advance_Requests_Models model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = _mapper.Map<Expense_Advance_Requests>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    var ReferencId = await _expense_Advance_Requests_Service.GetReferenceId(SiteId);
                    var statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Advance Expense Status", model.StatusType);

                    entity.ReferenceId = ReferencId;
                    entity.StatusId = statusId;
                    entity.SiteId = SiteId;

                    if (!string.IsNullOrEmpty(model.AdvanceDetails))
                    {
                        entity.AdvanceDetails = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.AdvanceDetails,
                                SiteData.Name,
                                "finance-advance-expenses-requests",
                                entity.Id
                            );
                    }

                    entity.CreatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    if (model.StatusId != "Draft")
                    {
                        model.Approver = model.StatusType;
                        await UpdateStatusDetails(model, entity, SiteId, GetDateTime);
                    }

                    _expense_Advance_Requests_Service.InsertExpenseAdvanceRequests(entity);
                    var itemCategory = await _dropDownTypeService.GetDropDownTypeById(model.ItemCategoryId);
                    string ExpenseAdvanceRequestId = entity.Id;

                    if (model.ExpenseAdvanceRequestFiles != null && model.ExpenseAdvanceRequestFiles.Any())
                    {                        
                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "finance-advance-expenses-requests", model.ExpenseAdvanceRequestFiles, entity.Id);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ExpenseAdvanceRequestFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = ExpenseAdvanceRequestId,
                                Module = itemCategory?.Type,
                                SubModuleId = ExpenseAdvanceRequestId,
                                Sub_Module = itemCategory?.Type,
                                Type = "Expense Advance Requests",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var ExpenseAdvanceRequestFiles = new ExpenseAdvanceRequestFiles
                            {
                                FileId = picture.Id,
                                ExpenseAdvanceRequestId = ExpenseAdvanceRequestId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _expenseAdvanceRequestFilesService.InsertExpenseAdvanceRequestFile(ExpenseAdvanceRequestFiles);

                            index++;
                        }
                    }

                    return NoContent();
                }

                return ModelStateError(ModelState);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region Update Advance Expense Request
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] Expense_Advance_Requests_Models model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _expense_Advance_Requests_Service.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No data found with the specified ID."));

                    var statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Advance Expense Status", model.StatusType);

                    if (model.StatusId != "Draft" && !model.IsSubmitted)
                    {
                        model.Approver = model.StatusType;
                        await UpdateStatusDetails(model, entity, SiteId, GetDateTime);
                    }

                    if (!string.IsNullOrEmpty(model.AdvanceDetails))
                    {
                        entity.AdvanceDetails = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.AdvanceDetails,
                                SiteData.Name,
                                "finance-advance-expenses-requests",
                                entity.Id,
                                entity.AdvanceDetails
                            );
                    }

                    entity.RequestedBy = model.RequestedBy;
                    entity.PaymentTypeId = model.PaymentTypeId;
                    entity.LocationId = model.LocationId;
                    entity.RequestDate = model.RequestDate;
                    entity.Balance = model.Amount;
                    entity.ItemCategoryId = model.ItemCategoryId;
                    entity.ItemSubCategoryId = model.ItemSubCategoryId;
                    entity.Amount = model.Amount;
                    entity.ApplyToTrip = model.ApplyToTrip;
                    entity.Notes = model.Notes;
                    entity.IsEdited = model.IsEdited;
                    entity.StatusId = statusId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    _expense_Advance_Requests_Service.UpdateExpenseAdvanceRequests(entity);

                    // Retrieve all file IDs from the expense advance request files
                    var allExpenseAdvanceRequestFileIds = (await _expenseAdvanceRequestFilesService.GetAllExpenseAdvanceRequestFileByExpenseAdvanceRequestId(SiteId, id)).Select(file => file.Id).ToList();
                    var missingFileIds = allExpenseAdvanceRequestFileIds.ToList();
                    if (model.ExistingFiles != null)
                    {
                        var existingFileIds = model.ExistingFiles.Select(fileJson =>
                        {
                            var file = JsonConvert.DeserializeObject<Picture>(fileJson);
                            return file.Id.Trim().ToLower();
                        })
                        .ToList();

                        // Compare and find missing file IDs
                        missingFileIds = allExpenseAdvanceRequestFileIds.Except(existingFileIds).ToList();
                    }
                    if (allExpenseAdvanceRequestFileIds.Any())
                    {
                        foreach (var expenseAdvanceRequestFilesId in missingFileIds)
                        {
                            var expenseAdvanceRequestFile = await _expenseAdvanceRequestFilesService.GetExpenseAdvanceRequestFileById(expenseAdvanceRequestFilesId);
                            if (expenseAdvanceRequestFile != null)
                                _expenseAdvanceRequestFilesService.DeleteExpenseAdvanceRequestFile(expenseAdvanceRequestFile);
                        }
                    }
                    var itemCategory = await _dropDownTypeService.GetDropDownTypeById(model.ItemCategoryId);
                    if (model.ExpenseAdvanceRequestFiles != null && model.ExpenseAdvanceRequestFiles.Any())
                    {
                        int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(id, "Expense Advance Requests");

                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "finance-advance-expenses-requests", model.ExpenseAdvanceRequestFiles, entity.Id, existingImagesCount);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ExpenseAdvanceRequestFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = id,
                                Module = itemCategory?.Type,
                                SubModuleId = id,
                                Sub_Module = itemCategory?.Type,
                                Type = "Expense Advance Requests",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var expenseAdvanceRequestFiles = new ExpenseAdvanceRequestFiles
                            {
                                FileId = picture.Id,
                                ExpenseAdvanceRequestId = id,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _expenseAdvanceRequestFilesService.InsertExpenseAdvanceRequestFile(expenseAdvanceRequestFiles);

                            index++;
                        }
                    }
                    return NoContent();
                }

                return ModelStateError(ModelState);
            }
            catch (Exception e)
            {
                return BadRequest(new BadRequestError($"An error occurred: {e.Message}"));
            }
        }
        #endregion

        #region Forward Advance Expense To Approvers
        [HttpPost("forward-advance-expense-to-approvers")]
        public async Task<IActionResult> ForwardAdvanceExpenseToApprovers(Expense_Advance_Requests_Models model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var entity = await _expense_Advance_Requests_Service.GetById(model.Id);
                if (entity == null)
                {
                    return BadRequest(new BadRequestError("No data found with the specified ID."));
                }

                var statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Advance Expense Status", model.Approver);
                if (!string.IsNullOrEmpty(statusId))
                {
                    entity.StatusId = statusId;
                    if (model.Approver == "Approved")
                        entity.PostApproverNote = model.PostApproverNote;
                    if (model.Approver == "Pre-Approved")
                        entity.PreApproverNote = model.PreApproverNote;
                    if (model.Approver == "Paid")
                        entity.PaidByNote = model.PaidByNote;
                    if (model.Approver == "Cancelled")
                        entity.PreApproverNote = model.PreApproverNote;
                    if (model.Approver == "Declined")
                        entity.PreApproverNote = model.PreApproverNote;
                        entity.PostApproverNote = model.PostApproverNote;

                    await UpdateStatusDetails(model, entity, SiteId, GetDateTime);
                }
                entity.UpdatedById = User.GetLoggedInUserId<string>();
                entity.UpdatedOnUtc = GetDateTime;

                _expense_Advance_Requests_Service.UpdateExpenseAdvanceRequests(entity);
                _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Expense_Advance_Requests", entity.Id, entity.ReferenceId, entity.Id, entity.ReferenceId, "Advance Expense Status", model.Approver, LoggedUserId, GetDateTime);

                return Ok(entity);
            }
            catch (Exception e)
            {
                return BadRequest(new BadRequestError(e.Message));
            }
        }
        #endregion

        #region Delete Advance Expense
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteAdvanceExpense(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    
                    var entity = await _expense_Advance_Requests_Service.GetById(id);
                    if (entity != null)
                        _expense_Advance_Requests_Service.DeleteExpenseAdvanceRequests(entity);

                }
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region Update Status & Send Notifications
        private async Task<bool> UpdateStatusDetails(Expense_Advance_Requests_Models model, Expense_Advance_Requests entity, string SiteId, DateTime GetDateTime)
        {
            string approverRole = model.Approver == "Submitted" ? "Finance-Preapprove"
                             : model.Approver == "Pre-Approved" ? "Finance-Approver"
                             : model.Approver == "Request For Cancellation" ? "Finance-Approver"
                             : model.Approver == "Cancelled" ? "Finance-Preapprove"
                             : model.Approver == "Approved" ? "Finance-PaidBy"
                             : null;
            string ApproverNotificationNumber = model.Approver == "Submitted" ? "AdvanceExpenseSubmitted1"
                                        : model.Approver == "Pre-Approved" ? "AdvanceExpensePreApproved1"
                                        : model.Approver == "Request For Cancellation" ? "AdvanceExpenseCancelRequest1"
                                        : model.Approver == "Approved" ? "AdvanceExpenseApproved1"
                                        : null;


            string userRole = model.Approver == "Declined"
                              || model.Approver == "Cancelled"
                              || model.Approver == "Submitted"
                              || model.Approver == "Approved"
                              || model.Approver == "Paid"
                              || model.Approver == "Pre-Approved" ? "Finance" : null;

            string userNotificationNumber = model.Approver == "Submitted" ? "AdvanceExpenseSubmitted2"
                                            : model.Approver == "Declined" ? "AdvanceExpenseDeclined2"
                                            : model.Approver == "Cancelled" ? "AdvanceExpenseCancelled2"
                                            : model.Approver == "Approved" ? "AdvanceExpenseApproved2"
                                            : model.Approver == "Pre-Approved" ? "AdvanceExpensePreApproved2"
                                            : model.Approver == "Paid" ? "AdvanceExpensePaid2"
                                            : null;

            if (!string.IsNullOrEmpty(approverRole))
                await SendNotification(SiteId, ApproverNotificationNumber, approverRole, "/approve-advance-expense", null, entity.Id, model.Approver, entity.Amount, entity.ReferenceId, entity.PreApproverNote, entity.PostApproverNote, entity.PaidByNote, GetDateTime);

            if (!string.IsNullOrEmpty(userRole))
                await SendNotification(SiteId, userNotificationNumber, userRole, "/finance-expense-advance", entity.CreatedById, entity.Id, model.Approver, entity.Amount, entity.ReferenceId, entity.PreApproverNote, entity.PostApproverNote, entity.PaidByNote, GetDateTime);

            return true;
        }

        private async Task<bool> SendNotification(string SiteId, string Number, string role, string redirectUrl, string createdBy, string recordId, string Approver, decimal amount, string expenseNumber, string PreApproverNote, string PostApproverNote, string paidByNote, DateTime GetDateTime)
        {
            var host = HttpContext.Request.Host.Host;
            var redirectionUrl = host == "dev4-0api.meldep.com" ? "https://dev4-0.meldep.com" + redirectUrl : "https://meldep.com" + redirectUrl;
            var Users = await _userService.GetUsersByRole(SiteId, role);
            if (Users.Any() && Number != null)
            {
                foreach (var user in Users)
                {
                    string LoggedUserId = !string.IsNullOrEmpty(createdBy) ? createdBy : user.Id;
                    var MasterNotificationData = await _masterNotificationService.GetMasterNotificationByNumber(SiteId, Number, LoggedUserId);
                    if (MasterNotificationData != null)
                    {
                        string Message = MasterNotificationData.Message.Replace("[Advance Expense Number]", expenseNumber);

                        var userData = await _userService.GetById(SiteId, LoggedUserId);

                        string expenseRole = Approver == "Submitted" ? "Finance"
                              : Approver == "Pre-Approved" ? "Finance-Preapprove"
                              : Approver == "Approved" ? "Finance-Approver"
                              : Approver == "Paid" ? "Finance-PaidBy" : null;

                        // get correct template name
                        string templateName = null;
                        bool canSendEmail = true;

                        if (string.IsNullOrEmpty(createdBy))
                        {
                            templateName =
                                expenseRole == "Finance" && Approver == "Submitted"
                                    ? "AdvanceExpense.AdvanceExpensePreApproveRequest"
                                : expenseRole == "Finance-Preapprove" && Approver == "Pre-Approved"
                                    ? "AdvanceExpense.AdvanceExpenseApproveRequest"
                                : expenseRole == "Finance-Approver" && Approver == "Approved"
                                    ? "AdvanceExpense.AdvanceExpensePayRequest"
                                : null;
                        }
                        else
                        {
                            templateName =
                                expenseRole == "Finance-Preapprove" && Approver == "Pre-Approved"
                                    ? "AdvanceExpense.AdvanceExpensePreApproved"
                                : expenseRole == "Finance-Approver" && Approver == "Approved"
                                    ? "AdvanceExpense.AdvanceExpenseApproved"
                                : expenseRole == "Finance-PaidBy" && Approver == "Paid"
                                    ? "AdvanceExpense.AdvanceExpensePaid"
                                : null;
                        }

                        if (!string.IsNullOrEmpty(templateName))
                        {
                            canSendEmail = await _sitesEmailNotificationsPermissionServices
                                .ShouldSendNotification(
                                    SiteId,
                                    LoggedUserId,
                                    templateName
                                );
                        }

                        if (!string.IsNullOrEmpty(expenseRole) && string.IsNullOrEmpty(createdBy))
                        {
                            _notificationService.AddNotification(SiteId, MasterNotificationData.Title, Message, MasterNotificationData.Type, LoggedUserId, recordId, redirectUrl, LoggedUserId, LoggedUserId, GetDateTime);

                            //Send Email ONLY if allowed
                            if (canSendEmail)
                            {
                                await _workflowMessageService.SendAdvanceExpenseMailToApprovers(userData, amount, redirectionUrl, expenseRole, Approver, PreApproverNote, SiteId);
                            }
                        }

                        if (!string.IsNullOrEmpty(expenseRole) && !string.IsNullOrEmpty(createdBy) && user.Id == createdBy)
                        {
                            _notificationService.AddNotification(SiteId, MasterNotificationData.Title, Message, MasterNotificationData.Type, LoggedUserId, recordId, redirectUrl, LoggedUserId, LoggedUserId, GetDateTime);

                            //Send Email ONLY if allowed
                            if (canSendEmail)
                            {
                                await _workflowMessageService.SendAdvanceExpenseMailToUsers(userData, amount, redirectionUrl, expenseRole, Approver, PostApproverNote, paidByNote, SiteId);
                            }
                        }
                    }
                }
            }
            return true;
        }
        #endregion

        #region Functions:- GetUniqueFileName
        private string GetUniqueFileName(string uploadDir, string fileName)
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string finalName = fileName;
            int counter = 1;

            while (System.IO.File.Exists(Path.Combine(uploadDir, finalName)))
            {
                if (counter == 1)
                    finalName = $"{fileNameWithoutExt} - copy{extension}";
                else
                    finalName = $"{fileNameWithoutExt} - copy({counter}){extension}";
                counter++;
            }

            return finalName;
        }
        #endregion
    }
}
