using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;
using Vsky.Models.Expens;

namespace Vsky.Api.Models.ExpenseModels
{
    public record Expense_Purchase_Requests_Models : BaseEntityModel
    {
        public DateTime RequestDate { get; set; }
        public string ReferenceId { get; set; }
        public string SiteId { get; set; }
        public string RequestedById { get; set; }
        public string PurchaserId { get; set; }
        public string ItemName { get; set; }
        public string StatusId { get; set; }
        public string ItemCategoryId { get; set; }
        public string ItemSubCategoryId { get; set; }
        public string VendorId { get; set; }
        public decimal Quantity { get; set; }
        public decimal EstimatedRate { get; set; }
        public decimal Discount { get; set; }
        public decimal EstimatedAmount { get; set; }
        public string Description { get; set; }
        public string Approver { get; set; }
        public string StatusType { get; set; }
        public string PostApproverNote { get; set; }
        public string PreApproverNote { get; set; }
        public string PaidByNote { get; set; }
        public bool IsEdited { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public List<IFormFile> ExpensePurchaseRequestFiles { get; set; }
        public List<string> ExistingFiles { get; set; }

        public virtual DropDown PurchaseRequestStatus { get; set; }
        public virtual DropDownType ItemCategory { get; set; }
        public virtual DropDown ItemSubCategory { get; set; }
        public virtual Employee RequestedEmployee { get; set; }
        public virtual Employee PurchaserEmployee { get; set; }
        public virtual ExpenseVendors ExpenseVendors { get; set; }

        public virtual ICollection<ExpensePurchaseRequestFilesModel> ExpensePurchaseRequestFileList { get; set; } = new List<ExpensePurchaseRequestFilesModel>();
        public record Expense_Purchase_RequestsSearchModel : BaseSearchModel
        {
            public List<string> ReferenceId { get; set; }
            public DateTime RequestDate { get; set; }
            public List<string> StatusId { get; set; }
            public string StatusName { get; set; }
            public string SearchText { get; set; }
            public string EmployeeId { get; set; }
        }
        public record Expense_Purchase_Requests_ListModel : BasePagedListModel<Expense_Purchase_Requests_Models> { }
    }
}
