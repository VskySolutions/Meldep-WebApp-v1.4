using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ExpenseVendorBankAccount
{
    public interface IExpenseVendorsService
    {
        #region GetAllExpenseVendors
        IPagedList<ExpenseVendors> GetAllExpenseVendors(string siteId, string SearchText, string vendorName, List<string> VendorIds, string vendorEmail, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetAllExpenseVendorListForDropdown
        Task<List<ExpenseVendors>> GetAllExpenseVendorListForDropdown(string SiteId, bool isOwnerName);
        #endregion

        #region GetExpenseVendorsById
        Task<ExpenseVendors> GetExpenseVendorsById(string id);
        #endregion

        #region GetVendorCode
        Task<string> GetVendorCode(string SiteId);
        #endregion

        #region GetExpenseVendorsDetailsById
        Task<ExpenseVendors> GetExpenseVendorsDetailsById(string id);
        #endregion

        #region GetExpenseVendorsByEmail
        Task<ExpenseVendors> GetExpenseVendorsByEmail(string siteId, string name, string id = null);
        #endregion

        #region InsertExpenseVendor
        void InsertExpenseVendor(ExpenseVendors entity);
        #endregion

        #region UpdateExpenseVendor
        void UpdateExpenseVendor(ExpenseVendors entity);
        #endregion

        #region DeleteExpenseVendor
        void DeleteExpenseVendor(ExpenseVendors entity);
        #endregion
    }
}


