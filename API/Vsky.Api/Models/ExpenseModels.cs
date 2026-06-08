using System;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Vsky.Models.Expens;

namespace Vsky.Api.Models
{
    public record ExpenseModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string ExpenseNumber { get; set; }
        public string BackAccountId { get; set; }
        public string PayeeId { get; set; }
        public string StatusId { get; set; }
        public int SearchNo { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Ref_no { get; set; }
        public string Memo { get; set; }
        public string Attachment { get; set; }
        public string RecurringIntervalId { get; set; }
        public bool SetRecurring { get; set; }
        public DateTime? RecurringStartDate { get; set; }
        public DateTime? RecurringEndDate { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ChangeFlag { get; set; }
        public string LocationId { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; } 
        public decimal Amount { get; set; } 
        public string ExpenseVendorBankAccountId { get; set; }
        public string Approver { get; set; }
        public string CustomerId { get; set; }
        public bool IsReImbursement { get; set; }
        public IFormFile FilePic { get; set; }
        public string PostApproverNote { get; set; }
        public string PreApproverNote { get; set; }
        public string PaidByNote { get; set; }
        public bool IsEdited { get; set; }
        public bool IsSubmitted { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual Site Site { get; set; }
        public virtual ExpenseVendorBankAccounts ExpenseVendorBankAccounts { get; set; }
        public virtual ExpenseVendors ExpenseVendors { get; set; }
        public virtual DropDown ExpenseStatus { get; set; }
        public virtual DropDown Location { get; set; }
        public virtual DropDown RecurringInterval { get; set; }
        public virtual Picture Picture { get; set; }
        public virtual Expense_BankAccounts ExpenseBankAccounts { get; set; }
        public virtual Expense_Advance_Requests Expense_Advance_Requests { get; set; }

        public List<string> ExistingFiles { get; set; }
        public List<IFormFile> ExpenseFiles { get; set; }
        public List<IFormFile> ExpensesListModelImages { get; set; }
        public List<Expense_Lines> ExpenseLines { get; set; }
        public List<Expense_LinesModel> ExpenseItems { get; set; }

        [FromForm]
        public virtual ICollection<Expense_LinesModel> ExpensesListModelProps { get; set; } = new List<Expense_LinesModel>();
        public virtual ICollection<ExpenseFilesModel> ExpenseFilesList { get; set; } = new List<ExpenseFilesModel>();

        public record ExpensesSearchModel : BaseSearchModel
        {
            public List<string> BankAccountIds { get; set; }
            public List<string> PayeeIds { get; set; }
            public DateTime ExpenseDate { get; set; }
            public string ExpenseNumber { get; set; }
            public string StatusId { get; set; }
            public string StatusName { get; set; }
            public string SearchText { get; set; }
            public string CreatedBy { get; set; }
        }

        public record ExpensesListModel : BasePagedListModel<ExpenseModel> { }
    }
}
