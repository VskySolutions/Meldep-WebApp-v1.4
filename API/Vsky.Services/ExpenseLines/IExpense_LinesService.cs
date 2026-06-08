
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ExpenseExpensExpense_Lines
{
    public interface IExpense_LinesService
    {
        IPagedList<Expense_Lines> GetAllExpensExpense_Lines(string SiteId, string ExpenseCategoryId,string sortBy,
            bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);

        Task<Expense_Lines> GetByExpenseLinesId(string id);
        Task<List<Expense_Lines>> GetByExpenseId(string SiteId, string ExpenseId); //added for delete
        void InsertExpensExpense_Lines(List<Expense_Lines> addList);

        void UpdateExpensExpense_Lines(List<Expense_Lines> updateList);
        void DeleteExpensExpense_Lines(List<Expense_Lines> deleteList);

    }
}

