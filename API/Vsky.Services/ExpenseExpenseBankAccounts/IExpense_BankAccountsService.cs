using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ExpenseExpenseBankAccounts
{
 
    public interface IExpense_BankAccountsService
    {
        IPagedList<Expense_BankAccounts> GetAllExpenseBankAccounts(string siteId, string SearchText, string accountNumber, string bankName, string branchName, string sortBy,
            bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);

        #region GetByBankAccountId
        Task<Expense_BankAccounts> GetByBankAccountId(string id);
        #endregion
        Task<Expense_BankAccounts> GetBankAccountDetailById(string id);

        Task<bool> BankAccountNumberValidation(string siteId, string AccountNumber, string id = null);

        void InsertExpenseBankAccounts(Expense_BankAccounts entity);

        void UpdateExpenseBankAccounts(Expense_BankAccounts entity);

        void DeleteExpenseBankAccounts(Expense_BankAccounts entity);              

    }
}
