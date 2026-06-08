using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models.Expens;

namespace Vsky.Models
{ 
    public class Expense : BaseEntity
    {
        public string SiteId { get; set; }
        public string ExpenseNumber { get; set; }
        public string BackAccountId { get; set; }
        public string PayeeId { get; set; }
        public string StatusId { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Ref_no { get; set; }
        public string Memo { get; set; }
        public string Attachment { get; set; }
        public string LocationId { get; set; }
        public string CustomerId { get; set; }
        public bool SetRecurring { get; set; }
        public string RecurringIntervalId { get; set; }
        public DateTime? RecurringStartDate { get; set; }
        public DateTime? RecurringEndDate { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }
        public bool IsEdited { get; set; }
        public string ExpenseVendorBankAccountId { get; set; }
        public bool IsReImbursement { get; set; }
        public string PostApproverNote { get; set; }
        public string PreApproverNote { get; set; }
        public string PaidByNote { get; set; }

        [NotMapped]
        public int SearchNo { get; set; }
        [NotMapped]
        public string Category { get; set; }
        [NotMapped]
        public string Subcategory { get; set; }
        [NotMapped]
        public decimal Amount { get; set; }

        public virtual Site Site { get; set; }
        public virtual ExpenseVendorBankAccounts ExpenseVendorBankAccounts { get; set; }
        public virtual ExpenseVendors ExpenseVendors { get; set; }
        public virtual DropDown ExpenseStatus { get; set; }
        public virtual DropDown Location { get; set; }
        public virtual DropDown RecurringInterval { get; set; }
        public virtual Picture Picture { get; set; }
        public virtual Expense_BankAccounts ExpenseBankAccounts { get; set; }
        public virtual Expense_Advance_Requests Expense_Advance_Requests { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public List<Expense_Lines> ExpenseLines { get; set; }
        [NotMapped]
        public virtual ICollection<Expense_Lines> ExpensesListModelProps { get; set; } = new List<Expense_Lines>();
        public virtual ICollection<Expense_Files> ExpenseFilesList { get; set; } = new List<Expense_Files>();

    }
}


