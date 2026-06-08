using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class Expense_BankAccounts : BaseEntity
    {
        public string SiteId { get; set; }
        public string AccountNumber { get; set; }

        public string BankName { get; set; }

        public string IFSCCode { get; set; }

        public string BranchName { get; set; }

        public string AccountTypeId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public string UpdatedById { get; set; }

        public bool Deleted { get; set; }
        public virtual Site Site { get; set; }
        public virtual DropDown AccountTypeDropDown { get; set; }
        public virtual ICollection<Expense> ExpenseList { get; set; } = new List<Expense>();
    }
}
