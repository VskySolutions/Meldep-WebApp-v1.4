using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
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
using static Vsky.Api.Models.ExpenseModel;

namespace Vsky.Api.Controllers
{
    [Route("Expense")]
    public class ExpenseController : BaseController
    {
        #region Services Initialization
        private readonly IMapper _mapper;
        private readonly GlobalVariable _globalVariable;
        private readonly IExpenseService _expensesService;
        private readonly IExpense_LinesService _expenselinesService;
        private readonly ICommonService _commonService;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly ISiteService _siteService;
        private readonly ApplicationDbContext _db;
        private readonly IExpenseFileService _expenseFileService;
        private readonly IExpense_Advance_Requests_Service _expense_Advance_Requests_Service;
        private readonly IMasterNotificationService _masterNotificationService;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly ISitesEmailNotificationsPermissionServices _sitesEmailNotificationsPermissionServices;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Ctor
        public ExpenseController
        (
            IMapper mapper,
            GlobalVariable globalVariable,
            IExpenseService expensesService,
            IExpense_LinesService expenselinesService,
            ICommonService commonService,
            IDropDownService dropDownService,
            IDropDownTypeService dropDownTypeService,
            ISiteService siteService,
            ApplicationDbContext db, 
            IExpenseFileService expenseFileService,
            IExpense_Advance_Requests_Service expense_Advance_Requests_Service,
            IMasterNotificationService masterNotificationService,
            INotificationService notificationService,
            IUserService userService,
            IWorkflowMessageService workflowMessageService,
            ISitesModifiedLogsService sitesModifiedLogsService,
            ISitesEmailNotificationsPermissionServices sitesEmailNotificationsPermissionServices,
            IAzureBlobImageServices azureBlobImageServices
        )
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _expensesService = expensesService;
            _expenselinesService = expenselinesService;
            _commonService = commonService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _siteService = siteService;
            _db = db;
            _expenseFileService = expenseFileService;
            _expense_Advance_Requests_Service = expense_Advance_Requests_Service;
            _masterNotificationService = masterNotificationService;
            _notificationService = notificationService;
            _userService = userService;
            _workflowMessageService = workflowMessageService;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _sitesEmailNotificationsPermissionServices = sitesEmailNotificationsPermissionServices;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllExpenses
        [HttpPost("list")]
        public async Task<IActionResult> GetAllExpenses(ExpensesSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";

                string statusId = null;
                if(!string.IsNullOrEmpty(searchModel.StatusName))
                    statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Expense Status", searchModel.StatusName);

                var list =await _expensesService.GetAllExpenses(
                    SiteId, 
                    false, 
                    LoggedUserId, 
                    searchModel.SearchText, 
                    searchModel.ExpenseNumber, 
                    searchModel.BankAccountIds,
                    searchModel.PayeeIds, 
                    searchModel.ExpenseDate, 
                    statusId,
                    createdBy,
                    searchModel.SortBy, 
                    searchModel.Descending, 
                    searchModel.Page, 
                    searchModel.PageSize
                );

                var model = new ExpensesListModel
                {
                    Data = _mapper.Map<IList<ExpenseModel>>(list),
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

        #region GetAllApproverExpenses
        [HttpPost("approve-expense-list")]
        public async Task<IActionResult> GetAllApproverExpenses(ExpensesSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";

                string statusId = null;
                if (!string.IsNullOrEmpty(searchModel.StatusName))
                    statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Expense Status", searchModel.StatusName);

                var list =await _expensesService.GetAllExpenses(SiteId, 
                    true, 
                    LoggedUserId,  
                    searchModel.SearchText,
                    searchModel.ExpenseNumber,
                    searchModel.BankAccountIds,
                    searchModel.PayeeIds,
                    searchModel.ExpenseDate,
                    statusId,
                    createdBy,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page, 
                    searchModel.PageSize);

                var model = new ExpensesListModel
                {
                    Data = _mapper.Map<IList<ExpenseModel>>(list),
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

        #region GetById 
        [HttpGet("GetExpenseId/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var entity = await _expensesService.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No expense data found with the specified id."));

                var model = _mapper.Map<ExpenseModel>(entity);

                return Ok(entity);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region GetByExpenseLinesId
        [HttpGet("GetSubcategoryId/{id}")]
        public async Task<IActionResult> GetByExpenseLinesId(string categoryId)
        {
            try
            {
                var entity = await _expenselinesService.GetByExpenseLinesId(categoryId);

                if (entity == null)
                {
                    return BadRequest(new BadRequestError("No expense line data found with the specified id."));
                }

                var model = _mapper.Map<Expense_Lines>(entity);

                return Ok(model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region GetBankAccountNoList
        [HttpGet("GetBankAccountNoBySelectItemList")]
        public async Task<IActionResult> GetBankAccountNoList()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var list = await _expensesService.GetBankAccountNoList(SiteId);
                if (list == null)
                {
                    return BadRequest(new BadRequestError("No bank account number found with the specified id "));
                }
                var model = _mapper.Map<List<Expense_BankAccountsModel>>(list);
                return Ok(model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region DeleteExpense
        [HttpDelete("{ExpenseId}")]

        public async Task<IActionResult> DeleteExpenses(string ExpenseId)
        {
            try
            {
                if (!string.IsNullOrEmpty(ExpenseId))
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;

                    var entity = await _expenselinesService.GetByExpenseId(SiteId, ExpenseId);

                    if (entity.Any()) // More efficient than Count() > 0
                    {
                        // Update each item's Deleted flag
                        foreach (var expenseLine in entity)
                        {
                            expenseLine.Deleted = true;
                        }
                        _expenselinesService.DeleteExpensExpense_Lines(entity);
                    }

                    var ExpenseObj = await _expensesService.GetExpenseById(ExpenseId);
                    if (ExpenseObj != null)
                    {
                        _expensesService.DeleteExpenses(ExpenseObj);
                    }
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region Create
        [HttpPost("Create")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ExpenseModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = _mapper.Map<Expense>(model);
                    var Number = await _expensesService.GetExpenseNumber(SiteId);

                    entity.Id = Guid.NewGuid().ToString();
                    entity.Ref_no = model.IsReImbursement ? null : model.Ref_no;
                    entity.ExpenseNumber = Number;

                    entity.CreatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    var statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Expense Status", model.StatusId);

                    if (model.StatusId != "Draft")
                    {
                        model.Approver = model.StatusId;
                        await UpdateStatusDetails(model, entity, SiteId, GetDateTime);
                    }

                    entity.StatusId = statusId;
                    entity.SiteId = SiteId;
                    if (model.ExpenseVendorBankAccountId != "cash")
                        entity.ExpenseVendorBankAccountId = model.ExpenseVendorBankAccountId;
                    else
                        entity.ExpenseVendorBankAccountId = null;

                    if (!string.IsNullOrEmpty(model.Memo))
                    {
                        entity.Memo = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Memo,
                                SiteData.Name,
                                "finance-expenses",
                                entity.ExpenseNumber.ToString()
                            );
                    }

                    _expensesService.InsertExpenses(entity);

                    // Retrieve the generated Id
                    string expenseId = entity.Id;

                    if (model.ExpenseFiles != null && model.ExpenseFiles.Any())
                    {
                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "finance-expenses", model.ExpenseFiles, entity.ExpenseNumber.ToString());
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ExpenseFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = expenseId,
                                Module = entity.ExpenseNumber,
                                SubModuleId = expenseId,
                                Sub_Module = entity.ExpenseNumber,
                                Type = "Expense",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var expenseFiles = new Expense_Files
                            {
                                ExpenseId = expenseId,
                                FileId = picture.Id,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _expenseFileService.InsertExpenseFile(expenseFiles);

                            index++;
                        }
                    }

                    if (model.ExpensesListModelProps.Count > 0)
                    {
                        var addList = new List<Expense_Lines>();

                        foreach (var item in model.ExpensesListModelProps)
                        {
                            var existsLine = await _expenselinesService.GetByExpenseLinesId(item.Id);
                            if (existsLine != null)
                                continue;

                            var newLine = _mapper.Map<Expense_Lines>(item);
                            newLine.ExpenseId = expenseId; // Use retrieved Id
                            newLine.CreatedById = LoggedUserId;
                            newLine.CreatedOnUtc = GetDateTime;
                            newLine.UpdatedById = LoggedUserId;
                            newLine.UpdatedOnUtc = GetDateTime;

                            // Handle Expense Lines attachment
                            string line_pic_id = string.Empty;
                            if (item.FilePic != null && item.FilePic.Length > 0)
                            {
                                // Direct logic to save the expense line file
                                var allowedFileTypes = new[] {
                                    "image/jpeg", "image/png", "application/pdf",
                                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                    "application/vnd.openxmlformats-officedocument.presentationml.presentation"
                                };

                                var file = item.FilePic;
                                if (!allowedFileTypes.Contains(file.ContentType))
                                    return BadRequest(new BadRequestError("Invalid file type."));

                                var originalFileName = Path.GetFileName(file.FileName);
                                var sanitizedFileName = Regex.Replace(originalFileName.Trim(), "[^A-Za-z0-9_.]+", "").Replace(" ", "_");
                                var mimeType = file.ContentType.Length > 15 ? file.ContentType.Substring(0, 15) : file.ContentType;
                                var uploadDirectory = Path.Combine("wwwroot", "uploads/expenselines", expenseId.ToString());
                                var filePath = Path.Combine(uploadDirectory, sanitizedFileName);
                                var virtualPath = $"/uploads/expenselines/{expenseId}/{sanitizedFileName}";

                                if (!Directory.Exists(uploadDirectory))
                                    Directory.CreateDirectory(uploadDirectory);

                                try
                                {
                                    using (var stream = System.IO.File.Create(filePath))
                                    {
                                        await file.CopyToAsync(stream);
                                    }
                                }
                                catch (Exception)
                                {
                                    return BadRequest(new BadRequestError("Expense line file upload failed"));
                                }

                                var picture = new Picture
                                {
                                    SeoFilename = originalFileName,
                                    MimeType = mimeType,
                                    VirtualPath = virtualPath
                                };

                                _commonService.InsertPicture(picture);
                                line_pic_id = picture.Id;
                            }

                            if (!string.IsNullOrEmpty(line_pic_id))
                            {
                                newLine.Attachment = line_pic_id;
                            }

                            addList.Add(newLine);
                        }

                        if (addList.Count > 0)
                            _expenselinesService.InsertExpensExpense_Lines(addList);
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

        #region Update
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] ExpenseModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    var entity = await _expensesService.GetExpenseById(id);

                    if (entity == null)
                        return BadRequest(new BadRequestError("No expense found with the specified ID."));

                    // Retrieve all file IDs from the project files
                    var allExpenseFileIds = (await _expenseFileService.GetAllExpenseFilesByExpenseId(SiteId, id)).Select(file => file.Id).ToList();

                    var missingFileIds = allExpenseFileIds.ToList();
                    if (model.ExistingFiles != null)
                    {
                        var existingFileIds = model.ExistingFiles.Select(fileJson =>
                        {
                            var file = JsonConvert.DeserializeObject<Picture>(fileJson);
                            return file.Id.Trim().ToLower();
                        }).ToList();

                        // Compare and find missing file IDs
                        missingFileIds = allExpenseFileIds.Except(existingFileIds).ToList();
                    }

                    if (missingFileIds.Any())
                    {
                        foreach (var expenseFilesId in missingFileIds)
                        {
                            var expenseFileDate = await _expenseFileService.GetExpenseFileById(expenseFilesId);
                            if (expenseFileDate != null)
                                _expenseFileService.DeleteExpenseFiles(expenseFileDate);
                        }
                    }

                    if (model.ExpenseFiles != null && model.ExpenseFiles.Any())
                    {
                        int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(id, "Expense");
                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "finance-expenses", model.ExpenseFiles, entity.ExpenseNumber.ToString(), existingImagesCount);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.ExpenseFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = id,
                                Module = entity.ExpenseNumber,
                                SubModuleId = id,
                                Sub_Module = entity.ExpenseNumber,
                                Type = "Expense",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var expenseFiles = new Expense_Files
                            {
                                ExpenseId = id,
                                FileId = picture.Id,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _expenseFileService.InsertExpenseFile(expenseFiles);

                            index++;
                        }
                    }

                    var statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Expense Status", model.StatusId);

                    if (model.StatusId != "Draft" && !model.IsSubmitted)
                    {
                        model.Approver = model.StatusId;
                        await UpdateStatusDetails(model, entity, SiteId, GetDateTime);
                    }

                    entity.BackAccountId = model.BackAccountId;
                    entity.PayeeId = model.PayeeId;
                    entity.Ref_no = model.IsReImbursement ? null : model.Ref_no;
                    entity.LocationId = model.LocationId;
                    entity.CustomerId = model.CustomerId;
                    //entity.RecurringIntervalId = model.RecurringIntervalId;
                    entity.RecurringStartDate = model.RecurringStartDate;
                    entity.RecurringEndDate = model.RecurringEndDate;
                    if (model.ExpenseVendorBankAccountId != "cash")
                        entity.ExpenseVendorBankAccountId = model.ExpenseVendorBankAccountId;
                    else
                        entity.ExpenseVendorBankAccountId = null;

                    if (!string.IsNullOrEmpty(model.Memo))
                    {
                        entity.Memo = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Memo,
                                SiteData.Name,
                                "finance-expenses",
                                entity.ExpenseNumber.ToString(),
                                entity.Memo
                            );
                    }

                    entity.IsReImbursement = model.IsReImbursement;
                    entity.StatusId = statusId;
                    entity.ExpenseDate = model.ExpenseDate;
                    entity.SiteId = SiteId;
                    entity.IsEdited = model.IsEdited;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    _expensesService.UpdateExpenses(entity);

                    if (model.ExpensesListModelProps.Count > 0)
                    {
                        var updateList = new List<Expense_Lines>();
                        var deleteList = new List<Expense_Lines>();
                        var newList = new List<Expense_Lines>();

                        foreach (var item in model.ExpensesListModelProps)
                        {
                            var existingLine = await _expenselinesService.GetByExpenseLinesId(item.Id);
                            if (existingLine == null)
                            {
                                var newLine = new Expense_Lines
                                {
                                    ExpenseCategoryId = item.ExpenseCategoryId,
                                    ExpenseSubcategoryId = item.ExpenseSubcategoryId,
                                    Description = item.Description,
                                    Amount = item.Amount,
                                    Quantity = item.Quantity,
                                    UnitPrice = item.UnitPrice,
                                    CreatedOnUtc = GetDateTime,
                                    CreatedById = LoggedUserId,
                                    UpdatedById = LoggedUserId, //added
                                    UpdatedOnUtc = GetDateTime, //added
                                    ExpenseId = entity.Id
                                };

                                newList.Add(newLine);
                            }
                            else
                            {
                                if (item.Flag == "Delete")
                                {
                                    if (existingLine == null)
                                        continue;

                                    existingLine.Deleted = true;
                                    existingLine.UpdatedOnUtc = GetDateTime;
                                    existingLine.UpdatedById = LoggedUserId;
                                    deleteList.Add(existingLine);

                                }
                                else
                                {
                                    existingLine.ExpenseCategoryId = item.ExpenseCategoryId;
                                    existingLine.ExpenseSubcategoryId = item.ExpenseSubcategoryId;
                                    existingLine.Description = item.Description;
                                    existingLine.Amount = item.Amount;
                                    existingLine.Quantity = item.Quantity;
                                    existingLine.UnitPrice = item.UnitPrice;
                                    existingLine.UpdatedOnUtc = GetDateTime;
                                    existingLine.UpdatedById = LoggedUserId;
                                    updateList.Add(existingLine);
                                }
                            }
                        }

                        if (updateList.Count > 0)
                            _expenselinesService.UpdateExpensExpense_Lines(updateList);

                        if (deleteList.Count > 0)
                            _expenselinesService.DeleteExpensExpense_Lines(deleteList);

                        if (newList.Count > 0)
                            _expenselinesService.InsertExpensExpense_Lines(newList);
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

        #region ForwardExpenseToApprovers
        [HttpPost("forward-expense-to-approvers")]
        public async Task<IActionResult> ForwardExpenseToApprovers(ExpenseModel model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var entity = await _expensesService.GetExpenseById(model.Id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No expense found with the specified ID."));

                var statusId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Expense Status", model.Approver);
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

                _expensesService.UpdateExpenses(entity);
                _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Expense", entity.Id, model.ExpenseNumber, entity.Id, model.ExpenseNumber, "Expense Status", model.Approver, LoggedUserId, GetDateTime);
                return Ok(entity);
            }
            catch (Exception e)
            {
                return BadRequest(new BadRequestError(e.Message));
            }
        }
        #endregion

        #region Update Status & Send Notifications
        private async Task<bool> UpdateStatusDetails(ExpenseModel model, Expense entity, string SiteId, DateTime GetDateTime)
        {
            string approverRole = model.Approver == "Submitted" ? "Finance-Preapprove"
                             : model.Approver == "Pre-Approved" ? "Finance-Approver"
                             : model.Approver == "Request For Cancellation" ? "Finance-Approver"
                             : model.Approver == "Cancelled" ? "Finance-Preapprove"
                             : model.Approver == "Approved" ? "Finance-PaidBy"
                             : null;
            string ApproverNotificationNumber = model.Approver == "Submitted" ? "ExpenseSubmitted1"
                                        : model.Approver == "Pre-Approved" ? "ExpensePreApproved1"
                                        : model.Approver == "Request For Cancellation" ? "ExpenseCancelRequest1"
                                        : model.Approver == "Approved" ? "ExpenseApproved1"
                                        : null;


            string userRole = model.Approver == "Declined"
                              || model.Approver == "Cancelled"
                              || model.Approver == "Submitted"
                              || model.Approver == "Approved"
                              || model.Approver == "Paid"
                              || model.Approver == "Pre-Approved" ? "Finance" 
                              : null;

            string userNotificationNumber = model.Approver == "Submitted" ? "ExpenseSubmitted2"
                                            : model.Approver == "Declined" ? "ExpenseDeclined2"
                                            : model.Approver == "Cancelled" ? "ExpenseCancelled2"
                                            : model.Approver == "Approved" ? "ExpenseApproved2"
                                            : model.Approver == "Pre-Approved" ? "ExpensePreApproved2"
                                            : model.Approver == "Paid" ? "ExpensePaid2"
                                            : null;

            var expenseSheetsLines = await _expenselinesService.GetByExpenseId(SiteId, entity.Id);
            var totalExpenseAmount = expenseSheetsLines.Any() ? expenseSheetsLines.Sum(x => x.Amount) : entity.ExpensesListModelProps.Sum(x => x.Amount);

            if (!string.IsNullOrEmpty(approverRole))
                await SendNotification(SiteId, ApproverNotificationNumber, approverRole, "/approve-expense", null, entity.Id, model.Approver, totalExpenseAmount, entity.ExpenseNumber, entity.PreApproverNote, entity.PreApproverNote,entity.PaidByNote, GetDateTime);

            if (!string.IsNullOrEmpty(userRole))
                await SendNotification(SiteId, userNotificationNumber, userRole, "/finance-expense", entity.CreatedById, entity.Id, model.Approver, totalExpenseAmount, entity.ExpenseNumber, entity.PreApproverNote, entity.PreApproverNote, entity.PaidByNote, GetDateTime);

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
                        var userData = await _userService.GetById(SiteId, userId);

                        string Message = MasterNotificationData.Message.Replace("[Expense Number]", expenseNumber);

                        string expenseRole = Approver == "Submitted" ? "Finance" 
                              : Approver == "Pre-Approved" ? "Finance-Preapprove" 
                              : Approver == "Approved" ? "Finance-Approver"
                              : Approver == "Paid" ? "Finance-PaidBy" : null;

                        // get correct template name
                        string templateName = null;
                        bool canSendEmail = true;

                        if (string.IsNullOrEmpty(createdBy))
                        {
                            // Approver side
                            templateName =
                                expenseRole == "Finance" && Approver == "Submitted"
                                    ? "Expense.ExpensePreApproveRequest"
                                : expenseRole == "Finance-Preapprove" && Approver == "Pre-Approved"
                                    ? "Expense.ExpenseApproveRequest"
                                : expenseRole == "Finance-Approver" && Approver == "Approved"
                                    ? "Expense.ExpensePayRequest"
                                : null;
                        }
                        else
                        {
                            // User side
                            templateName =
                                expenseRole == "Finance-Preapprove" && Approver == "Pre-Approved"
                                    ? "Expense.ExpensePreApproved"
                                : expenseRole == "Finance-Approver" && Approver == "Approved"
                                    ? "Expense.ExpenseApproved"
                                : expenseRole == "Finance-PaidBy" && Approver == "Paid"
                                    ? "Expense.ExpensePaid"
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

                        if (!string.IsNullOrEmpty(expenseRole) && string.IsNullOrEmpty(createdBy))
                        {
                            _notificationService.AddNotification(SiteId, MasterNotificationData.Title, Message, MasterNotificationData.Type, userId, recordId, redirectUrl, userId, userId, GetDateTime);

                            //Send Email ONLY if allowed
                            if (canSendEmail)
                            {
                                await _workflowMessageService.SendExpenseMailToApprovers(userData, amount, redirectionUrl, expenseRole, Approver, PreApproverNote, SiteId);
                            }
                        }

                        if (!string.IsNullOrEmpty(expenseRole) && !string.IsNullOrEmpty(createdBy) && user.Id == createdBy)
                        {
                            _notificationService.AddNotification(SiteId, MasterNotificationData.Title, Message, MasterNotificationData.Type, userId, recordId, redirectUrl, userId, userId, GetDateTime);

                            //Send Email ONLY if allowed
                            if (canSendEmail)
                            {
                                await _workflowMessageService.SendExpenseMailToUsers(userData, amount, redirectionUrl, expenseRole, Approver, PostApproverNote, paidByNote, SiteId);
                            }
                        }
                    }
                }
            }
            return true;
        }

        #endregion
    }
}
