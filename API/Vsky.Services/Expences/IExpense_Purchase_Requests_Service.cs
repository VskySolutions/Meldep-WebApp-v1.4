using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vsky.Core;
using Vsky.Models.Expens;
using Vsky.Models;

namespace Vsky.Services.Expences
{
    public interface IExpense_Purchase_Requests_Service
    {
        #region GetAllPurchaseExpenseRequests
        Task<IPagedList<Expense_Purchase_Requests>> GetAllPurchaseExpenseRequests
        (
            string SiteId, 
            bool isApprove, 
            string LoggedUserId, 
            string SearchText, 
            List<string> referenceId, 
            DateTime RequestDate,
            List<string> StatusId,
            string employeeId,
            string sortBy, 
            bool descending,
            string statusName = "", 
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetById
        Task<Expense_Purchase_Requests> GetById(string id);
        #endregion

        #region GetPurchaseExpenseDetailsById
        Task<Expense_Purchase_Requests> GetPurchaseExpenseDetailsById(string id);
        #endregion

        #region GetExpensePurchaseListForDropdown
        Task<List<SelectListItem>> GetExpensePurchaseListForDropdown(string siteId, string LoggedUserId, string statusId = null);
        #endregion

        #region GetReferenceId
        Task<string> GetReferenceId(string siteId);
        #endregion

        #region InsertExpensePurchaseRequests
        void InsertExpensePurchaseRequests(Expense_Purchase_Requests entity);
        #endregion

        #region UpdateExpensePurchaseRequests
        void UpdateExpensePurchaseRequests(Expense_Purchase_Requests entity);
        #endregion

        #region DeleteExpensePurchaseRequests
        void DeleteExpensePurchaseRequests(Expense_Purchase_Requests entity);
        #endregion
        
    }
}
