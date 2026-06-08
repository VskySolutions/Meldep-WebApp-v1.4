using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api.Models;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.ExpenseVendorBankAccount;
using Vsky.Services.Messages;
using Vsky.Services.Notifications;
using Vsky.Services.Persons;
using Vsky.Services.Sites;
using Vsky.Services.Users;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Vsky.Api.Controllers
{
    [Route("expense-vendor")]
    public class ExpenseVendorBankAccountsController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IExpenseVendorBankAccountsService _expenseVendorBankAccountsService;
        private readonly IExpenseVendorsService _expenseVendorsService;
        private readonly IPersonService _personService;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly ApplicationDbContext _db;
        #endregion

        #region Services Initializations
        public ExpenseVendorBankAccountsController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            ICommonService commonService,
            ISiteService siteService, 
            IExpenseVendorBankAccountsService expenseVendorBankAccountsService, 
            IExpenseVendorsService expenseVendorsService, 
            IPersonService personService, 
            IDropDownService dropDownService, 
            IDropDownTypeService dropDownTypeService, 
            ApplicationDbContext db)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _commonService = commonService;
            _siteService = siteService;
            _expenseVendorBankAccountsService = expenseVendorBankAccountsService;
            _expenseVendorsService = expenseVendorsService;
            _personService = personService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _db = db;
        }
        #endregion

        #region GetAllExpenseVendors
        // Title: Get All ExpenseVendors
        // Description: This endpoint fetches a list of ExpenseVendor based on the provided search criteria such as title, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllExpenseVendors(ExpenseVendorsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of ExpenseVendor based on search criterias
                var list = _expenseVendorsService.GetAllExpenseVendors(SiteId, searchModel.SearchText, searchModel.VendorName, searchModel.VendorIds, searchModel.VendorEmail, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                // Map the fetched list to a model suitable for the response
                var model = new ExpenseVendorsListModel
                {
                    Data = _mapper.Map<IList<ExpenseVendorsModel>>(list),
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

        #region GetAllExpenseVendorListForDropdown
        // Title: GetAllExpenseVendorListForDropdown
        // Description: This endpoint retrieves the list of vendors. 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllExpenseVendorListForDropdown(bool isOwnerName)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _expenseVendorsService.GetAllExpenseVendorListForDropdown(SiteId, isOwnerName);
                var model = _mapper.Map<List<ExpenseVendorsModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllVendorsAccountListForDropdown
        // Title: GetAllVendorsAccountListForDropdown
        // Description: This endpoint retrieves the list of Accounts. 
        [HttpGet("accountsdropdown/list/{VendorId}")]
        public async Task<IActionResult> GetAllVendorsAccountListForDropdown(string VendorId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _expenseVendorBankAccountsService.GetAllVendorsAccountListForDropdown(SiteId, VendorId);
                var model = _mapper.Map<List<ExpenseVendorBankAccountsModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetExpenseVendorsDetailsById
        // Title: GetExpenseVendorsDetailsById
        // Description: This endpoint retrieves the details of a specific ExpenseVendor based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpenseVendorsDetailsById(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch the ExpenseVendor entity by its ID from the service
                var entity = await _expenseVendorsService.GetExpenseVendorsDetailsById(id);

                // If the ExpenseVendor entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No vendor found with the specified id."));

                // Map the ExpenseVendor entity to a ExpenseVendorsModel object
                var model = _mapper.Map<ExpenseVendorsModel>(entity);
                var AccountType = await _dropDownTypeService.GetDropDownTypeByType(SiteId, "Vendor Account Type");
                var bankAccount = await _dropDownService.GetDropDownByTypeAndValue(SiteId, AccountType.Id, "Bank Account");
                var upiAccount = await _dropDownService.GetDropDownByTypeAndValue(SiteId, AccountType.Id, "UPI");
                var byCash = await _dropDownService.GetDropDownByTypeAndValue(SiteId, AccountType.Id, "By Cash");
                model.BankAccountType = bankAccount.Id;
                model.UpiAccountType = upiAccount.Id;
                model.ByCashType = byCash.Id;

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateExpenseVendorAccount
        // Title: CreateExpenseVendorAccount
        // Description: This endpoint handles the creation of a new ExpenseVendorAccount. It first checks if a ExpenseVendor with the same name already exists. If not, it maps the ExpenseVendor model to the ExpenseVendor entity, sets the creation details, and inserts the ExpenseVendor into the database.
        [HttpPost]
        public async Task<IActionResult> CreateExpenseVendorAccount(ExpenseVendorsModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the ExpenseVendor already exists
                    var exists = await _expenseVendorsService.GetExpenseVendorsByEmail(SiteId,model.VendorName);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Vendor name already exists, try with another."));

                    //Add/Update Address
                    string AddressId = _commonService.AddUpdateAddress(model.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, LoggedUserId);

                    // Map the ExpenseVendor model to the ExpenseVendor entity
                    var entity = _mapper.Map<ExpenseVendors>(model);

                    // Fetch the GetVendorCode entity by its ID from the service
                    var Code = await _expenseVendorsService.GetVendorCode(SiteId);

                    // If the GetVendorCode entity is not found, return a BadRequest response with an error message
                    if (Code != null)
                        entity.VendorCode = Code;
                    // Set custom properties
                    entity.SiteId = SiteId;
                    entity.AddressId = AddressId;
                    entity.IsActive = true;

                    // Set the created by and created on properties
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _expenseVendorsService.InsertExpenseVendor(entity);

                    foreach (var item in model.BankAccountList)
                    {
                        if (item.Flag != "Delete")
                        {
                            var account = _mapper.Map<ExpenseVendorBankAccounts>(item);

                            // Set custom properties
                            account.VendorId = entity.Id;

                            // Set the created by and created on properties
                            var BankAccount = await _dropDownService.GetDropDownById(item.BankAccount);
                            account.IsBankAccount = BankAccount.DropDownValue == "Bank Account";
                            account.IsActive = true;
                            account.AccountTypeId = item.AccountTypeId;
                            account.PaymentTypeId = item.PaymentTypeId;

                            account.CreatedById = LoggedUserId;
                            account.UpdatedById = LoggedUserId;
                            account.CreatedOnUtc = GetDateTime;
                            account.UpdatedOnUtc = GetDateTime;
                            _expenseVendorBankAccountsService.InsertExpenseVendorBankAccounts(account);
                        }
                    }

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

        #region UpdateExpenseVendor
        // Title: UpdateExpenseVendor
        // Description: This endpoint updates an existing ExpenseVendor by its ID. It validates the ExpenseVendor model, checks for duplicate ExpenseVendor title, updates the ExpenseVendor's details.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpenseVendor(string id, ExpenseVendorsModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Check if there is any ExpenseVendor with the same title that is not marked as deleted and has a different ID
                    var exists = await _expenseVendorsService.GetExpenseVendorsByEmail(SiteId,model.VendorName, id);

                    // If ExpenseVendor exists, return a bad request with an error message
                    if (exists != null)
                        return BadRequest(new BadRequestError("Vendor name already exists, try with another."));

                    // Fetch the ExpenseVendor entity by its ID
                    var entity = await _expenseVendorsService.GetExpenseVendorsById(id);

                    // If no ExpenseVendor is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Expense Vendor found with the specified id."));

                    //Add/Update Address
                    string AddressId = _commonService.AddUpdateAddress(entity.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, LoggedUserId);

                    entity.PersonId = model.PersonId;
                    entity.VendorName = model.VendorName;
                    entity.Vendor_Phone = model.Vendor_Phone;
                    entity.Vendor_Email = model.Vendor_Email;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _expenseVendorsService.UpdateExpenseVendor(entity);

                    if (model.BankAccountList != null && model.BankAccountList.Count() > 0)
                    {
                        // Loop through each Daily Planner Line
                        foreach (var account in model.BankAccountList)
                        {
                            var bankAccountEntity = await _expenseVendorBankAccountsService.GetExpenseVendorsBankAccountById(account.Id);
                            var BankAccount = await _dropDownService.GetDropDownById(account.BankAccount);

                            if (bankAccountEntity == null)
                            {
                                var accountEntity = _mapper.Map<ExpenseVendorBankAccounts>(account);

                                accountEntity.VendorId = entity.Id;
                                accountEntity.AccountTypeId = account.AccountTypeId;
                                accountEntity.IsBankAccount = BankAccount.DropDownValue == "Bank Account";
                                accountEntity.IsActive = true;
                                accountEntity.CreatedById = LoggedUserId;
                                accountEntity.UpdatedById = LoggedUserId;
                                accountEntity.CreatedOnUtc = GetDateTime;
                                accountEntity.UpdatedOnUtc = GetDateTime;
                                _expenseVendorBankAccountsService.InsertExpenseVendorBankAccounts(accountEntity);

                            }
                            else
                            {
                                if (account.Flag != "Delete")
                                {
                                    bankAccountEntity.BankName = account.BankName;
                                    bankAccountEntity.AccountNumber = account.AccountNumber;
                                    bankAccountEntity.IFSCCode = account.IFSCCode;
                                    bankAccountEntity.AccountTypeId = account.AccountTypeId;
                                    bankAccountEntity.PaymentTypeId = account.PaymentTypeId;
                                    bankAccountEntity.BranchName = account.BranchName;
                                    bankAccountEntity.UPI_ID = account.UPI_ID;
                                    bankAccountEntity.VendorId = entity.Id;
                                    account.IsBankAccount = BankAccount.DropDownValue == "Bank Account";
                                    bankAccountEntity.IsActive = true;
                                    bankAccountEntity.UpdatedById = LoggedUserId;
                                    bankAccountEntity.UpdatedOnUtc = GetDateTime;
                                    _expenseVendorBankAccountsService.UpdateExpenseVendorBankAccounts(bankAccountEntity);
                                }
                                else
                                {
                                    _expenseVendorBankAccountsService.DeleteExpenseVendorBankAccounts(bankAccountEntity);
                                }
                            }
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

        #region DeleteExpenseVendorBankAccounts
        // Title: DeleteExpenseVendorBankAccountsById
        // Description: This endpoint deletes a ExpenseVendor based on the provided ExpenseVendor ID. It first retrieves the ExpenseVendor entity by ID, checks if it exists, and if so, deletes the ExpenseVendor. If the ExpenseVendor is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpenseVendorBankAccounts(string id)
        {
            try
            {
                // Fetch the ExpenseVendor entity by its ID
                var entity = await _expenseVendorsService.GetExpenseVendorsById(id);

                // If no ExpenseVendor is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No ExpenseVendor found with the specified id."));

                // Delete the ExpenseVendor using the ExpenseVendor service
                _expenseVendorsService.DeleteExpenseVendor(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}