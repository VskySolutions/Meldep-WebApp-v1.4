using System;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ExpensesModel : BaseEntityModel
    {
        [Required(ErrorMessage = "Payment Type is Required")]
        public string PaymentTypeId { get; set; }


        [Required(ErrorMessage = "Bank Account is Required")]
        public string BackAccountId { get; set; }


        [Required(ErrorMessage = "Payee is Required")]
        public string PayeeId { get; set; }


        [Required(ErrorMessage = "Status is Required")]
        public string StatusId { get; set; }


        public int SearchNo { get; set; } //auto incr


        [Required(ErrorMessage = "Expense Date is Required")]
        public DateTime ExpenseDate { get; set; }


        public string Ref_no { get; set; }

        public string Memo { get; set; }

        public string Attachment { get; set; }

        [Required(ErrorMessage = "Invalid is Required")]

        public string RecurringIntervalId { get; set; }

        public bool SetRecurring { get; set; }

        public DateTime RecurringStartDate { get; set; }

        public DateTime RecurringEndDate { get; set; }


        public IFormFile FilePic { get; set; }

        public string ChangeFlag { get; set; }  //picture update

        public string LocationId { get; set; }  //newly added

        public record ExpensesSearchModel : BaseSearchModel
        {
            public string BackAccountId { get; set; }
        }


        public record ExpensesListModel : BasePagedListModel<ExpensesModel> { }
    }
}
