using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using Vsky.Services.EmailNotifications;
using Vsky.Services.Expences;
using Vsky.Services.Messages;
using Vsky.Services.Notifications;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.Users;
using static Vsky.Api.Models.ExpenseModels.Expense_Purchase_Requests_Models;

namespace Vsky.Api.Controllers
{
    [Route("purchase-expense-request")]
    public class ExpensePurchaseRequestController : BaseController
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
        private readonly IExpense_Purchase_Requests_Service _expense_Purchase_Requests_Service;
        private readonly IMasterNotificationService _masterNotificationService;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly IExpensePurchaseRequestFilesService _expensePurchaseRequestFilesService;
        private readonly ISitesEmailNotificationsPermissionServices _sitesEmailNotificationsPermissionServices;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Ctor
        public ExpensePurchaseRequestController
        (
            GlobalVariable globalVariable,
            IMapper mapper,
            IExpenseService expensesService,
            ICommonService commonService,
            IDropDownService dropDownService,
            ISiteService siteService,
            ApplicationDbContext db,
            IExpenseFileService expenseFileService,
            IExpense_Purchase_Requests_Service expense_Purchase_Requests_Service,
            IMasterNotificationService masterNotificationService,
            INotificationService notificationService,
            IUserService userService,
            IWorkflowMessageService workflowMessageService,
            ISitesModifiedLogsService sitesModifiedLogsService,
            IExpensePurchaseRequestFilesService expensePurchaseRequestFilesService,
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
            _expense_Purchase_Requests_Service = expense_Purchase_Requests_Service;
            _masterNotificationService = masterNotificationService;
            _notificationService = notificationService;
            _userService = userService;
            _workflowMessageService = workflowMessageService;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _expensePurchaseRequestFilesService = expensePurchaseRequestFilesService;
            _sitesEmailNotificationsPermissionServices = sitesEmailNotificationsPermissionServices;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region Get All Purchase Expense Requests
        [HttpPost("list")]
        public async Task<IActionResult> GetAllPurchaseExpenseRequests(Expense_Purchase_RequestsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var list = await _expense_Purchase_Requests_Service.GetAllPurchaseExpenseRequests(
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

                var model = new Expense_Purchase_Requests_ListModel
                {
                    Data = _mapper.Map<IList<Expense_Purchase_Requests_Models>>(list),
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

        #region Get All Approve Purchase Expense Requests
        [HttpPost("approve-purchase-expense-list")]
        public async Task<IActionResult> GetAllApprovePurchaseExpenseRequests(Expense_Purchase_RequestsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                string statusId = null;
                if (!string.IsNullOrEmpty(searchModel.StatusName))
                    statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Purchase Expense Status", searchModel.StatusName);

                var list = await _expense_Purchase_Requests_Service.GetAllPurchaseExpenseRequests(
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

                var model = new Expense_Purchase_Requests_ListModel
                {
                    Data = _mapper.Map<IList<Expense_Purchase_Requests_Models>>(list),
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
                var entity = await _expense_Purchase_Requests_Service.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No data found with the specified id."));

                var model = _mapper.Map<Expense_Purchase_Requests_Models>(entity);

                return Ok(entity);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region Get Purchase Expense Details By Id
        [HttpGet("get-purchase-expense-details/{id}")]
        public async Task<IActionResult> GetPurchaseExpenseDetailsById(string id)
        {
            try
            {
                var entity = await _expense_Purchase_Requests_Service.GetPurchaseExpenseDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No data found with the specified id."));

                var model = _mapper.Map<Expense_Purchase_Requests_Models>(entity);

                return Ok(entity);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region Get Purchase Expense Request Dropdown List
        [HttpGet("purchase-expense-dropdown-list/{statusName}")]
        public async Task<IActionResult> GetPurchaseExpenseRequestList(string statusName = "")
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var statusId = "";

                if (!string.IsNullOrEmpty(statusName) && statusName != "undefined")
                    statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Purchase Expense Status", statusName);

                var list = await _expense_Purchase_Requests_Service.GetExpensePurchaseListForDropdown(SiteId, LoggedUserId, statusId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Create Purchase Expense Request
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Expense_Purchase_Requests_Models model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = _mapper.Map<Expense_Purchase_Requests>(model);
                    var ReferencId = await _expense_Purchase_Requests_Service.GetReferenceId(SiteId);
                    var statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Purchase Expense Status", model.StatusType);

                    entity.Id = Guid.NewGuid().ToString();
                    entity.ReferenceId = ReferencId;
                    entity.StatusId = statusId;
                    entity.SiteId = SiteId;

                    entity.CreatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    if (model.StatusId != "Draft")
                    {
                        model.Approver = model.StatusType;
                        await UpdateStatusDetails(model, entity, SiteId, GetDateTime);
                    }

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "finance-expense-purchase-request",
                                entity.Id
                            );
                    }

                    _expense_Purchase_Requests_Service.InsertExpensePurchaseRequests(entity);
                    string ExpensePurchaseRequestId = entity.Id;

                    if (model.ExpensePurchaseRequestFiles != null && model.ExpensePurchaseRequestFiles.Any())
                    {
                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "finance-expense-purchase-request", model.ExpensePurchaseRequestFiles, entity.Id);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ExpensePurchaseRequestFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl,
                                ModuleId = ExpensePurchaseRequestId,
                                Module = entity.ItemName,
                                SubModuleId = ExpensePurchaseRequestId,
                                Sub_Module = entity.ItemName,
                                Type = "Expense Purchase Requests",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var ExpensePurchaseRequestFiles = new ExpensePurchaseRequestFiles
                            {
                                FileId = picture.Id,
                                ExpensePurchaseRequestId = ExpensePurchaseRequestId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _expensePurchaseRequestFilesService.InsertExpensePurchaseRequestFile(ExpensePurchaseRequestFiles);

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

        #region Update Purchase Expense Request
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] Expense_Purchase_Requests_Models model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    var entity = await _expense_Purchase_Requests_Service.GetById(id);

                    if (entity == null)
                        return BadRequest(new BadRequestError("No data found with the specified ID."));

                    var statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Purchase Expense Status", model.StatusType);

                    if (model.StatusId != "Draft" && !model.IsSubmitted)
                    {
                        model.Approver = model.StatusType;
                    }

                    // Retrieve all file IDs from the expense purchase request files
                    var allExpensePurchaseRequestFileIds = (await _expensePurchaseRequestFilesService.GetAllExpensePurchaseRequestFileByExpensePurchaseRequestId(SiteId, id)).Select(file => file.Id).ToList();
                    var missingFileIds = allExpensePurchaseRequestFileIds.ToList();
                    if (model.ExistingFiles != null)
                    {
                        var existingFileIds = model.ExistingFiles.Select(fileJson =>
                        {
                            var file = JsonConvert.DeserializeObject<Picture>(fileJson);
                            return file.Id.Trim().ToLower();
                        })
                        .ToList();

                        // Compare and find missing file IDs
                        missingFileIds = allExpensePurchaseRequestFileIds.Except(existingFileIds).ToList();
                    }
                    if (allExpensePurchaseRequestFileIds.Any())
                    {
                        foreach (var expensePurchaseRequestFilesId in missingFileIds)
                        {
                            var expensePurchaseRequestFile = await _expensePurchaseRequestFilesService.GetExpensePurchaseRequestFileById(expensePurchaseRequestFilesId);
                            if (expensePurchaseRequestFile != null)
                                _expensePurchaseRequestFilesService.DeleteExpensePurchaseRequestFile(expensePurchaseRequestFile);
                        }
                    }

                    entity.RequestedById = model.RequestedById;
                    entity.PurchaserId = model.PurchaserId;
                    entity.ItemName = model.ItemName;
                    entity.RequestDate = model.RequestDate;
                    entity.ItemCategoryId = model.ItemCategoryId;
                    entity.ItemSubCategoryId = model.ItemSubCategoryId;

                    entity.VendorId = model.VendorId;
                    entity.Quantity = model.Quantity;
                    entity.EstimatedRate = model.EstimatedRate;
                    entity.Discount = model.Discount;
                    entity.EstimatedAmount = model.EstimatedAmount;
                    entity.IsEdited = model.IsEdited;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "finance-expense-purchase-request",
                                entity.Id,
                                entity.Description
                            );
                    }

                    entity.StatusId = statusId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    _expense_Purchase_Requests_Service.UpdateExpensePurchaseRequests(entity);

                    if (model.ExpensePurchaseRequestFiles != null && model.ExpensePurchaseRequestFiles.Any())
                    {
                        int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(id, "Expense Purchase Requests");

                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "finance-expense-purchase-request", model.ExpensePurchaseRequestFiles, id, existingImagesCount);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ExpensePurchaseRequestFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = id,
                                Module = entity.ItemName,
                                SubModuleId = id,
                                Sub_Module = entity.ItemName,
                                Type = "Expense Purchase Requests",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var expensePurchaseRequestFiles = new ExpensePurchaseRequestFiles
                            {
                                ExpensePurchaseRequestId = id,
                                FileId = picture.Id,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _expensePurchaseRequestFilesService.InsertExpensePurchaseRequestFile(expensePurchaseRequestFiles);

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

        #region DeletePurchaseExpenseRequest
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchaseExpenseRequest(string id)
        {
            try
            {
                var entity = await _expense_Purchase_Requests_Service.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No data found with the specified ID."));

                // Delete the ExpenseVendor using the ExpenseVendor service
                _expense_Purchase_Requests_Service.DeleteExpensePurchaseRequests(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Forward Purchase Expense To Approvers
        [HttpPost("forward-purchase-expense-to-approvers")]
        public async Task<IActionResult> ForwardPurchaseExpenseToApprovers(Expense_Purchase_Requests_Models model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var entity = await _expense_Purchase_Requests_Service.GetById(model.Id);
                if (entity == null)
                {
                    return BadRequest(new BadRequestError("No data found with the specified ID."));
                }

                var statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Purchase Expense Status", model.Approver);
                if (!string.IsNullOrEmpty(statusId))
                {
                    entity.StatusId = statusId;
                    if (model.Approver == "Approved")
                        entity.PostApproverNote = model.PostApproverNote;
                    if (model.Approver == "Pre-Approved")
                        entity.PreApproverNote = model.PreApproverNote;
                    if (model.Approver == "Paid")
                        entity.PaidByNote = model.PaidByNote;
                    if (model.Approver == "Declined")
                        entity.PreApproverNote = model.PreApproverNote;
                        entity.PostApproverNote = model.PostApproverNote;
                    if (model.Approver == "Cancelled")
                        entity.PreApproverNote = model.PreApproverNote;

                    await UpdateStatusDetails(model, entity, SiteId, GetDateTime);
                }
                entity.UpdatedById = User.GetLoggedInUserId<string>();
                entity.UpdatedOnUtc = GetDateTime;

                _expense_Purchase_Requests_Service.UpdateExpensePurchaseRequests(entity);
                _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Expense_Purchase_Requests", entity.Id, entity.ReferenceId, entity.Id, entity.ReferenceId, "Purchase Expense Status", model.Approver, LoggedUserId, GetDateTime);
                return Ok(entity);
            }
            catch (Exception e)
            {
                return BadRequest(new BadRequestError(e.Message));
            }
        }
        #endregion

        #region Update Status & Send Notifications
        private async Task<bool> UpdateStatusDetails(Expense_Purchase_Requests_Models model, Expense_Purchase_Requests entity, string SiteId, DateTime GetDateTime)
        {
            string approverRole = model.Approver == "Submitted" ? "Finance-Preapprove"
                             : model.Approver == "Pre-Approved" ? "Finance-Approver"
                             : model.Approver == "Request For Cancellation" ? "Finance-Approver"
                             : model.Approver == "Cancelled" ? "Finance-Preapprove"
                             : model.Approver == "Approved" ? "Finance-PaidBy"
                             : null;
            string ApproverNotificationNumber = model.Approver == "Submitted" ? "PurchaseExpenseSubmitted1"
                                        : model.Approver == "Pre-Approved" ? "PurchaseExpensePreApproved1"
                                        : model.Approver == "Request For Cancellation" ? "PurchaseExpenseCancelRequest1"
                                        : model.Approver == "Approved" ? "PurchaseExpenseApproved1"
                                        : null;


            string userRole = model.Approver == "Declined"
                              || model.Approver == "Cancelled"
                              || model.Approver == "Submitted"
                              || model.Approver == "Approved"
                              || model.Approver == "Paid"
                              || model.Approver == "Pre-Approved" ? "Finance" : null;

            string userNotificationNumber = model.Approver == "Submitted" ? "PurchaseExpenseSubmitted2"
                                            : model.Approver == "Declined" ? "PurchaseExpenseDeclined2"
                                            : model.Approver == "Cancelled" ? "PurchaseExpenseCancelled2"
                                            : model.Approver == "Approved" ? "PurchaseExpenseApproved2"
                                            : model.Approver == "Pre-Approved" ? "PurchaseExpensePreApproved2"
                                            : model.Approver == "Paid" ? "PurchaseExpensePaid2"
                                            : null;

            if (!string.IsNullOrEmpty(approverRole))
                await SendNotification(SiteId, ApproverNotificationNumber, approverRole, "/approve-purchase-expense", null, entity.Id, model.Approver, entity.EstimatedAmount, entity.ReferenceId, entity.PreApproverNote, entity.PostApproverNote, entity.PaidByNote, GetDateTime);

            if (!string.IsNullOrEmpty(userRole))
                await SendNotification(SiteId, userNotificationNumber, userRole, "/finance-expense-purchase-request", entity.CreatedById, entity.Id, model.Approver, entity.EstimatedAmount, entity.ReferenceId, entity.PreApproverNote, entity.PostApproverNote, entity.PaidByNote, GetDateTime);

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
                    string userId = !string.IsNullOrEmpty(createdBy) ? createdBy : user.Id;
                    var MasterNotificationData = await _masterNotificationService.GetMasterNotificationByNumber(SiteId, Number, userId);
                    if (MasterNotificationData != null)
                    {
                        string Message = MasterNotificationData.Message.Replace("[Purchase Expense Number]", expenseNumber);

                        var userData = await _userService.GetById(SiteId, userId);

                        role = Approver == "Submitted" ? "Finance"
                              : Approver == "Pre-Approved" ? "Finance-Preapprove"
                              : Approver == "Approved" ? "Finance-Approver"
                              : Approver == "Paid" ? "Finance-PaidBy" : null;

                        // get correct template name
                        string templateName = null;
                        bool canSendEmail = true;

                        if (string.IsNullOrEmpty(createdBy))
                        {
                            templateName =
                                role == "Finance" && Approver == "Submitted"
                                    ? "PurchaseExpense.PurchaseExpensePreApproveRequest"
                                : role == "Finance-Preapprove" && Approver == "Pre-Approved"
                                    ? "PurchaseExpense.PurchaseExpenseApproveRequest"
                                : role == "Finance-Approver" && Approver == "Approved"
                                    ? "PurchaseExpense.PurchaseExpensePayRequest"
                                : null;
                        }
                        else
                        {
                            templateName =
                                role == "Finance-Preapprove" && Approver == "Pre-Approved"
                                    ? "PurchaseExpense.PurchaseExpensePreApproved"
                                : role == "Finance-Approver" && Approver == "Approved"
                                    ? "PurchaseExpense.PurchaseExpenseApproved"
                                : role == "Finance-PaidBy" && Approver == "Paid"
                                    ? "PurchaseExpense.PurchaseExpensePaid"
                                : null;
                        }

                        if (!string.IsNullOrEmpty(templateName))
                        {
                            canSendEmail = await _sitesEmailNotificationsPermissionServices
                                .ShouldSendNotification(
                                    SiteId,
                                    userId,
                                    templateName
                                );
                        }

                        if (!string.IsNullOrEmpty(role) && string.IsNullOrEmpty(createdBy))
                        {
                            _notificationService.AddNotification(SiteId, MasterNotificationData.Title, Message, MasterNotificationData.Type, userId, recordId, redirectUrl, userId, userId, GetDateTime);

                            //Send Email ONLY if allowed
                            if(canSendEmail)
                            {
                                await _workflowMessageService.SendPurchaseExpenseMailToApprovers(userData, amount, redirectionUrl, role, Approver, PreApproverNote, SiteId);
                            }
                        }

                        if (!string.IsNullOrEmpty(role) && !string.IsNullOrEmpty(createdBy) && user.Id == createdBy)
                        {
                            _notificationService.AddNotification(SiteId, MasterNotificationData.Title, Message, MasterNotificationData.Type, userId, recordId, redirectUrl, userId, userId, GetDateTime);

                            //Send Email ONLY if allowed
                            if (canSendEmail)
                            {
                                await _workflowMessageService.SendPurchaseExpenseMailToUsers(userData, amount, redirectionUrl, role, Approver, PostApproverNote, paidByNote, SiteId);
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
