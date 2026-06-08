using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ServerPaymentsModel : BaseEntityModel
    {
        public string ServerId { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public DateTime? RenewDate { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public string Flag { get; set; }
        public string RenewDateStr { get; set; }

        public virtual ServerModel Server { get; set; }
    }
}
