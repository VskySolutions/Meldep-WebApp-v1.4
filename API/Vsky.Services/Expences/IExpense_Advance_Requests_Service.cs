using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vsky.Core;
using Vsky.Models;
using Vsky.Models.Expens;

namespace Vsky.Services.Expences
{
    public interface IExpense_Advance_Requests_Service
    {
        #region GetAllAdvanceExpenseRequests
        Task<IPagedList<Expense_Advance_Requests>> GetAllAdvanceExpenseRequests
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
        Task<Expense_Advance_Requests> GetById(string id);
        #endregion

        #region GetAdvanceExpenseDetailsById
        Task<Expense_Advance_Requests> GetAdvanceExpenseDetailsById(string id);
        #endregion

        #region GetExpenseAdvanceListForDropdown
        Task<List<SelectListItem>> GetExpenseAdvanceListForDropdown(string siteId, string LoggedUserId, string statusId = null);
        #endregion

        #region GetReferenceId
        Task<string> GetReferenceId(string siteId);
        #endregion

        #region InsertExpenseAdvanceRequests
        void InsertExpenseAdvanceRequests(Expense_Advance_Requests entity);
        #endregion

        #region UpdateExpenseAdvanceRequests
        void UpdateExpenseAdvanceRequests(Expense_Advance_Requests entity);
        #endregion

        #region DeleteExpenseAdvanceRequests
        void DeleteExpenseAdvanceRequests(Expense_Advance_Requests entity);
        #endregion
    }
}
