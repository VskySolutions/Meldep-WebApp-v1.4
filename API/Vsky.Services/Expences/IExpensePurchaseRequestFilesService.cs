using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;
using Vsky.Models.Expens;

namespace Vsky.Services.Expences
{
    public interface IExpensePurchaseRequestFilesService
    {
        #region GetExpensePurchaseRequestFileById
        Task<ExpensePurchaseRequestFiles> GetExpensePurchaseRequestFileById(string id);
        #endregion

        #region GetExpensePurchaseRequestFileByFileId
        Task<ExpensePurchaseRequestFiles> GetExpensePurchaseRequestFileByFileId(string fileId);
        #endregion

        #region GetAllExpensePurchaseRequestFileByExpensePurchaseRequestId
        Task<List<ExpensePurchaseRequestFiles>> GetAllExpensePurchaseRequestFileByExpensePurchaseRequestId(string siteId, string expensePurchaseRequestId);
        #endregion

        #region InsertExpensePurchaseRequestFile
        void InsertExpensePurchaseRequestFile(ExpensePurchaseRequestFiles entity);
        #endregion

        #region UpdateExpensePurchaseRequestFile
        void UpdateExpensePurchaseRequestFile(ExpensePurchaseRequestFiles entity);
        #endregion

        #region DeleteExpensePurchaseRequestFile
        void DeleteExpensePurchaseRequestFile(ExpensePurchaseRequestFiles entity);
        #endregion
    }
}
