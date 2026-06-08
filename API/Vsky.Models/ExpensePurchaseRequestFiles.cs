using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models.Expens;

namespace Vsky.Models
{
    public class ExpensePurchaseRequestFiles : BaseEntity
    {
        public string ExpensePurchaseRequestId { get; set; }
        public string FileId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public virtual Expense_Purchase_Requests Expense_Purchase_Requests { get; set; }
        public virtual Picture File { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
}
