using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;


namespace Vsky.Services.ExpenseExpenseBankAccounts
{

    public class Expense_BankAccountsService : IExpense_BankAccountsService
    {
        #region Fields

        private readonly IRepository<Expense_BankAccounts> _expenseBankAccountsRepository;
        private readonly ApplicationDbContext _dbContext;

        #endregion

        #region Ctor

        public Expense_BankAccountsService(IRepository<Expense_BankAccounts> expenseBankAccountsRepository, ApplicationDbContext dbContext)
        {
            _expenseBankAccountsRepository = expenseBankAccountsRepository;   // dependancy injection
            _dbContext = dbContext;
        }

        #endregion

        #region Private Methods

        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region Public Methods

        public IPagedList<Expense_BankAccounts> GetAllExpenseBankAccounts(string siteId, string SearchText, string accountNumber, string bankName, string branchName, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _expenseBankAccountsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);

            // Custom filter
            if (!string.IsNullOrWhiteSpace(bankName))
            {
                query = query.Where(x => x.BankName.Contains(bankName));
            }
            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                query = query.Where(x => x.AccountNumber.Contains(accountNumber));
            }
            if (!string.IsNullOrWhiteSpace(branchName))
            {
                query = query.Where(x => x.BranchName.Contains(branchName));
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.UpdatedOnUtc);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                    m.AccountNumber.Contains(SearchText.ToLower()) ||
                    m.BankName.ToLower().Contains(SearchText.ToLower()) ||
                    m.BankName.ToLower().Contains(SearchText.ToLower()) ||
                    m.IFSCCode.ToLower().Contains(SearchText.ToLower()) ||
                    m.AccountTypeDropDown.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.BranchName.ToLower().Contains(SearchText.ToLower())
                );
            }

            if (lookup)
            {
                query = query.Select(x => new Expense_BankAccounts
                {
                    Id = x.Id,
                    BankName = x.BankName,
                    AccountNumber = x.AccountNumber,
                    IFSCCode = x.IFSCCode
                });
            }
            else
            {
                query = query.Select(x => new Expense_BankAccounts
                {
                    Id = x.Id,
                    BankName = x.BankName,
                    AccountNumber = x.AccountNumber,
                    IFSCCode = x.IFSCCode,
                    IsActive = x.IsActive,
                    BranchName = x.BranchName,
                    AccountTypeDropDown = new DropDown
                    {
                        Id = x.AccountTypeDropDown.Id,
                        DropDownValue = x.AccountTypeDropDown.DropDownValue
                    }
                });
            }

            var list = new PagedList<Expense_BankAccounts>(query, page, pageSize);

            return list;
        }

        public async Task<bool> BankAccountNumberValidation(string siteId, string AccountNumber, string id = null)
        {
            var item = await _expenseBankAccountsRepository.TableNoTracking
                        .FirstOrDefaultAsync(x => x.AccountNumber == AccountNumber && !x.Deleted && x.SiteId == siteId && (x.Id != id));

            return item != null;
        }


        //List

        //public async Task<Expense_BankAccounts> GetByBankAccountId(string id)
        //{
        //    var query = _expenseBankAccountsRepository.TableNoTracking.Where(x => x.Id == id);

        //    query = query.Where(x => !x.Deleted && x.Id == id);

        //    var item = await query.FirstOrDefaultAsync();

        //    return item;
        //}

        #region GetByBankAccountId
        // Title: GetByBankAccountId
        // Description: This method retrieves a Expense_BankAccounts from the database by its unique identifier (`id`). 
        public async Task<Expense_BankAccounts> GetByBankAccountId(string id)
        {
            var query = _expenseBankAccountsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        public async Task<Expense_BankAccounts> GetBankAccountDetailById(string id)
        {
            var query = _expenseBankAccountsRepository.Table
                .Where(x => !x.Deleted && x.Id == id)
                .Select(x => new Expense_BankAccounts
                {
                    Id = x.Id,
                    BankName = x.BankName,
                    AccountNumber = x.AccountNumber,
                    AccountTypeId = x.AccountTypeId,
                    IFSCCode = x.IFSCCode,
                    IsActive = x.IsActive,
                    //SiteId = x.SiteId,
                    BranchName = x.BranchName,
                    CreatedById = x.CreatedById,
                    AccountTypeDropDown = new DropDown
                    {
                        Id = x.AccountTypeDropDown.Id,
                        DropDownValue = x.AccountTypeDropDown.DropDownValue
                    },
                    ExpenseList = x.ExpenseList.Where(p => !p.Deleted).Select(p => new Expense
                    {
                        Id = p.Id,
                        CreatedOnUtc = p.CreatedOnUtc,
                        ExpenseNumber = p.ExpenseNumber,
                        ExpenseStatus = new DropDown
                        {
                            Id = p.ExpenseStatus.Id,
                            DropDownValue = p.ExpenseStatus.DropDownValue
                        },
                        Amount = _dbContext.ExpenceLines
                                 .Where(m => !m.Deleted && m.ExpenseId == p.Id)
                                 .Sum(d => d.Amount)
                    }).ToList(),
                });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        public void InsertExpenseBankAccounts(Expense_BankAccounts entity)
        {
            _expenseBankAccountsRepository.Insert(entity);
        }

        public void UpdateExpenseBankAccounts(Expense_BankAccounts entity)
        {
            _expenseBankAccountsRepository.Update(entity);
        }

        public void DeleteExpenseBankAccounts(Expense_BankAccounts entity)
        {
            entity.Deleted = true;
            _expenseBankAccountsRepository.Update(entity);
        }
        #endregion
    }
}
