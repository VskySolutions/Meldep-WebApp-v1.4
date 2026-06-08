using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ExpenseFilesModel : BaseEntityModel
    {
        public string ExpenseId { get; set; }
        public string FileId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Expense Expenses { get; set; }
        public virtual Picture File { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }

    public record ExpenseFilesSearchModel : BaseSearchModel
    {
    }

    public record ExpenseFilesListModel : BasePagedListModel<ExpenseFilesModel>
    {
        public bool editing { get; set; }
    }
}
