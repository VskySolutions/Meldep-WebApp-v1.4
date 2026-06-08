using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Expences
{
    public interface IExpenseService
    {
        Task<IPagedList<Expense>> GetAllExpenses
        (
        string SiteId,
        bool isApprove, 
        string LoggedUserId, 
        string SearchText, 
        string expenseNumber,
        List<string> bankAccountIds, 
        List<string> payeeIds,
        DateTime ExpenseDate, 
        string StatusId,
        string createdBy,
        string sortBy,
        bool descending, 
        int pageIndex = 1, 
        int pageSize = int.MaxValue, 
        bool lookup = false
        );

        Task<Expense> GetExpenseById(string id);
        Task<Expense> GetById(string id);
        Task<string> GetStatusIdByName(string SiteId, string StatusId);
        Task<Picture> GetByPictureId(string Attachment);
        Task<string> GetExpenseNumber(string SiteId);
        Task UpdatePicture(Picture entity);

        void InsertExpenses(Expense entity);

        void UpdateExpenses(Expense entity);

        void DeleteExpenses(Expense entity);
        Task<List<Expense_BankAccounts>> GetBankAccountNoList(string SiteId);       
    }
}

