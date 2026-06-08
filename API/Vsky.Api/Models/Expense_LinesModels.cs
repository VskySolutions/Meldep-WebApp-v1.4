using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record Expense_LinesModel : BaseEntityModel
    {
     
        public string ExpenseId { get; set; }
        public string ExpenseCategoryId { get; set; }
        public string ExpenseSubcategoryId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Attachment { get; set; }
        public string Subcategory { get; set; }  //new
        public string Flag { get; set; }  //new
        public string FileName { get; set; }  //new
        public IFormFile FilePic { get; set; }
        public List<string> AttachmentPaths { get; set; } = new List<string>(); // Store file paths

        public virtual Expense Expense { get; set; }
        public virtual DropDownType Category { get; set; }
        public virtual DropDown ExpenseCategorySubcategory { get; set; }

        public record ExpenseExpense_LinesSearchModel : BaseSearchModel
        {
            public string  ExpenseCategoryId { get; set; }
        }
        public record ExpenseEmployeeModelListModel : BasePagedListModel<Expense_LinesModel> { }
    }
}
