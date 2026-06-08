using System;
using Vsky.Api.Framework.Models;
using Vsky.Models.Expens;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ExpensePurchaseRequestFilesModel : BaseEntityModel
    {
        public string ExpensePurchaseRequestId { get; set; }
        public string FileId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public virtual Expense_Purchase_Requests Expense_Purchase_Request { get; set; }
        public virtual Picture File { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
    public record ExpensePurchaseRequestFilesSearchModel : BaseSearchModel
    {
    }

    public record ExpensePurchaseRequestFilesListModel : BasePagedListModel<ExpensePurchaseRequestFilesModel>
    {
        public bool editing { get; set; }
    }
}
