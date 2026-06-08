using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models.Expens
{
    public class Expense_Advance_Requests : BaseEntity
    {
        public string SiteId { get; set; }
        public string RequestedBy { get; set; }
        public string PaymentTypeId { get; set; }
        public string LocationId { get; set; }
        public string ReferenceId { get; set; }
        public string StatusId { get; set; }
        public DateTime RequestDate { get; set; }
        public decimal Amount { get; set; }
        public bool ApplyToTrip { get; set; }
        public string AdvanceDetails { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public decimal Balance { get; set; }
        public string ItemCategoryId { get; set; }
        public string ItemSubCategoryId { get; set; }
        public string PostApproverNote { get; set; }
        public string PreApproverNote { get; set; }
        public string PaidByNote { get; set; }
        public bool IsEdited { get; set; }
        public virtual DropDown AdvanceExpenseStatus { get; set; }
        public virtual DropDown Location { get; set; }
        public virtual DropDown PaymentType { get; set; }
        public virtual Employee RequestedEmployee { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual DropDownType ItemCategory { get; set; }
        public virtual DropDown ItemSubCategory { get; set; }

        public virtual ICollection<ExpenseAdvanceRequestFiles> ExpenseAdvanceRequestFileList { get; set; } = new List<ExpenseAdvanceRequestFiles>();

    }
}
