using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record Expense_BankAccountsModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string AccountNumber { get; set; }

        public string BankName { get; set; }

        public string IFSCCode { get; set; }

        public string BranchName { get; set; }

        public string AccountTypeId { get; set; }

        public bool IsActive { get; set; }

        public string AccountType { get; set; }

        public string AccountId { get; set; }
        public virtual Site Site { get; set; }
        public virtual DropDown AccountTypeDropDown { get; set; }
        public virtual ICollection<ExpenseModel> ExpenseList { get; set; } = new List<ExpenseModel>();

        public record ExpenseBankAccountsSearchModel : BaseSearchModel
        {
            public string AccountNumber { get; set; }
            public string BankName { get; set; }
            public string BranchName { get; set; }
            public string SearchText { get; set; }
        }
        public record ExpenseBankAccountsListModel : BasePagedListModel<Expense_BankAccountsModel> { }
    }
}
