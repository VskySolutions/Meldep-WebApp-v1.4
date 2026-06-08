using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class Expense_Lines : BaseEntity
    {
        public string ExpenseId { get; set; }
        public string ExpenseCategoryId { get; set; }
        public string ExpenseSubcategoryId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Attachment { get; set; }

        [NotMapped]
        public string Flag { get; internal set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public string VartualPath { get; set; }  //newly added

        [NotMapped]
        public string PictureId { get; set; }  //newly added  
        [NotMapped]
        public string PictureFileName { get; set; }  //newly added

        public virtual Expense Expense { get; set; }
        public virtual DropDownType Category { get; set; }
        public virtual DropDown ExpenseCategorySubcategory { get; set; }
    }
}
