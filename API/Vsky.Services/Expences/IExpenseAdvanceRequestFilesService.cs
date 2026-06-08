using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.Expences
{
    public interface IExpenseAdvanceRequestFilesService
    {
        #region GetExpenseAdvanceRequestFileById
        Task<ExpenseAdvanceRequestFiles> GetExpenseAdvanceRequestFileById(string id);
        #endregion

        #region GetExpenseAdvanceRequestFileByFileId
        Task<ExpenseAdvanceRequestFiles> GetExpenseAdvanceRequestFileByFileId(string fileId);
        #endregion

        #region GetAllExpenseAdvanceRequestFileByExpenseAdvanceRequestId
        Task<List<ExpenseAdvanceRequestFiles>> GetAllExpenseAdvanceRequestFileByExpenseAdvanceRequestId(string siteId, string expensePurchaseRequestId);
        #endregion

        #region InsertExpenseAdvanceRequestFile
        void InsertExpenseAdvanceRequestFile(ExpenseAdvanceRequestFiles entity);
        #endregion

        #region UpdateExpenseAdvanceRequestFile
        void UpdateExpenseAdvanceRequestFile(ExpenseAdvanceRequestFiles entity);
        #endregion

        #region DeleteExpenseAdvanceRequestFile
        void DeleteExpenseAdvanceRequestFile(ExpenseAdvanceRequestFiles entity);
        #endregion
    }
}
