using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Services.ExpenseExpenseBankAccounts;
using static Vsky.Api.Models.Expense_BankAccountsModel;
using Vsky.Services.Common;
using Vsky.Services.Sites;
namespace Vsky.Api.Controllers
{
    [Route("expense-bankAccount")]
    public class ExpenseBankAccountController : BaseController
    {
        #region Fields
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;       
        private readonly IExpense_BankAccountsService _expenseBankAccountsService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly ApplicationDbContext _db;
        #endregion

        #region Ctor
        public ExpenseBankAccountController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            IExpense_BankAccountsService Expense_BankAccountsService, 
            ICommonService commonService, 
            ISiteService siteService, 
            ApplicationDbContext db)
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _expenseBankAccountsService = Expense_BankAccountsService;
            _commonService = commonService;
            _siteService = siteService;
            _db = db;
        }
        #endregion

        #region Methods Bank Account
        [HttpPost("list")]
        public IActionResult GetAll(ExpenseBankAccountsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _expenseBankAccountsService.GetAllExpenseBankAccounts(SiteId, searchModel.SearchText, searchModel.AccountNumber, searchModel.BankName, searchModel.BranchName, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                var model = new ExpenseBankAccountsListModel
                {
                    Data = _mapper.Map<IList<Expense_BankAccountsModel>>(list),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);

            }
        }

        [HttpGet("GetByBankAccountId/{id}")]
        public async Task<IActionResult> GetByBankAccountId(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new BadRequestError("The bank account ID is required"));
                }
                var entity = await _expenseBankAccountsService.GetBankAccountDetailById(id);

                if (entity == null)
                {
                    return BadRequest(new BadRequestError("No bank account found with the specified id."));
                }

                var model = _mapper.Map<Expense_BankAccounts>(entity);

                return Ok(model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Expense_BankAccountsModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var AccountNumber = await _expenseBankAccountsService.BankAccountNumberValidation(SiteId, model.AccountNumber);

                    if (AccountNumber)
                    {
                        return BadRequest(new BadRequestError("Bank account number is already available."));
                    }

                    var entity1 = _mapper.Map<Expense_BankAccounts>(model);  //map model
                    entity1.SiteId = SiteId;
                    entity1.CreatedById = LoggedUserId;
                    entity1.CreatedOnUtc = GetDateTime;
                    entity1.UpdatedById = LoggedUserId;
                    entity1.UpdatedOnUtc = GetDateTime;

                    _expenseBankAccountsService.InsertExpenseBankAccounts(entity1);

                    return NoContent();
                }

                return ModelStateError(ModelState);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Expense_BankAccountsModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _expenseBankAccountsService.GetByBankAccountId(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No bank account found with the specified id."));

                    var AccountNumber = await _expenseBankAccountsService.BankAccountNumberValidation(SiteId, model.AccountNumber, id);
                    if (AccountNumber)
                        return BadRequest(new BadRequestError("Bank account number is already available."));

                    entity.BankName = model.BankName;
                    entity.AccountNumber = model.AccountNumber;
                    entity.IFSCCode = model.IFSCCode;
                    entity.AccountTypeId = model.AccountTypeId;
                    entity.BranchName = model.BranchName;
                    entity.IsActive = model.IsActive;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    _expenseBankAccountsService.UpdateExpenseBankAccounts(entity);
                    return NoContent();
                }

                return ModelStateError(ModelState);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("Deactivate/{id}")]
        public async Task<IActionResult> DeactivateAccount(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var entity = await _expenseBankAccountsService.GetByBankAccountId(id);
                if (entity == null)
                    return NotFound(new BadRequestError("No bank account found with the specified ID."));

                entity.IsActive = !entity.IsActive;
                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;

                _expenseBankAccountsService.UpdateExpenseBankAccounts(entity);
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(new BadRequestError(e.Message));
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var entity = await _expenseBankAccountsService.GetByBankAccountId(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No bank account found with specified id"));

                _expenseBankAccountsService.DeleteExpenseBankAccounts(entity);
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion
    }
}
