using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models.Expens
{
    public class Expense_Purchase_Requests : BaseEntity
    {
        public DateTime RequestDate { get; set; }
        public string ReferenceId { get; set; }
        public string SiteId { get; set; }
        public string RequestedById { get; set; }
        public string PurchaserId { get; set; }
        public string ItemName { get; set; }
        public string ItemCategoryId { get; set; }
        public string ItemSubCategoryId { get; set; }
        public string StatusId { get; set; }
        public string VendorId { get; set; }
        public decimal Quantity { get; set; }
        public decimal EstimatedRate { get; set; }
        public decimal Discount { get; set; }
        public decimal EstimatedAmount { get; set; }
        public string Description { get; set; }
        public string PostApproverNote { get; set; }
        public string PreApproverNote { get; set; }
        public string PaidByNote { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual DropDown PurchaseRequestStatus { get; set; }
        public virtual DropDownType ItemCategory { get; set; }
        public virtual DropDown ItemSubCategory { get; set; }
        public virtual Employee RequestedEmployee { get; set; }
        public virtual Employee PurchaserEmployee { get; set; }
        public virtual ExpenseVendors ExpenseVendors { get; set; }
        public virtual ICollection<ExpensePurchaseRequestFiles> ExpensePurchaseRequestFileList { get; set; } = new List<ExpensePurchaseRequestFiles>();
    }
}
