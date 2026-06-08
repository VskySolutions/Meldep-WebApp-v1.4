using System;
using Vsky.Models.Expens;
using Vsky.Models;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record ExpenseAdvanceRequestFilesModel : BaseEntityModel
    {
        public string ExpenseAdvanceRequestId { get; set; }
        public string FileId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public virtual Expense_Advance_Requests Expense_Advance_Requests { get; set; }
        public virtual Picture File { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
    public record ExpenseAdvanceRequestFilesSearchModel : BaseSearchModel
    {
    }

    public record ExpenseAdvanceRequestFilesListModel : BasePagedListModel<ExpenseAdvanceRequestFilesModel>
    {
        public bool editing { get; set; }
    }
}
