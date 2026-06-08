using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.Expences
{
    public interface IExpenseFileService
    {
        Task<Expense_Files> GetExpenseFileById(string id);

        Task<List<Expense_Files>> GetAllExpenseFilesByExpenseId(string siteId, string expenseId);

        #region InsertExpenseFile
        void InsertExpenseFile(Expense_Files entity);
        #endregion

        #region UpdateExpenseFile
        void UpdateExpenseFile(Expense_Files entity);
        #endregion

        #region DeleteExpenseFiles
        void DeleteExpenseFiles(Expense_Files entity);
        #endregion
    }
}
