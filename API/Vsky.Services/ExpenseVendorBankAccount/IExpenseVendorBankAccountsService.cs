using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ExpenseVendorBankAccount
{
    public interface IExpenseVendorBankAccountsService
    {
        #region GetAllExpenseVendorBankAccounts
        IPagedList<ExpenseVendorBankAccounts> GetAllExpenseVendorBankAccounts(string siteId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetAllVendorsAccountListForDropdown
        Task<List<ExpenseVendorBankAccounts>> GetAllVendorsAccountListForDropdown(string SiteId, string vendorId = null);

        #endregion 

        #region GetExpenseVendorsBankAccountById
        Task<ExpenseVendorBankAccounts> GetExpenseVendorsBankAccountById(string id);
        #endregion

        #region GetExpenseVendorsBankAccountDetailsById
        Task<ExpenseVendorBankAccounts> GetExpenseVendorsBankAccountDetailsById(string id);
        #endregion

        //#region GetExpenseVendorsByEmail
        //Task<ExpenseVendors> GetExpenseVendorsByEmail(/*string siteId,*/ string email, string id = null);
        //#endregion

        #region InsertExpenseVendorBankAccounts
        void InsertExpenseVendorBankAccounts(ExpenseVendorBankAccounts entity);
        #endregion

        #region UpdateExpenseVendorBankAccounts
        void UpdateExpenseVendorBankAccounts(ExpenseVendorBankAccounts entity);
        #endregion

        #region DeleteExpenseVendorBankAccounts
        void DeleteExpenseVendorBankAccounts(ExpenseVendorBankAccounts entity);
        #endregion
    }
}
