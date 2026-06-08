using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record DomainAttributesModel : BaseEntityModel
    {
        public string DomainId { get; set; }
        public string DomainAttributeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Amount { get; set; }
        public string Duration { get; set; }
        public string Notes { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public string Flag { get; set; }

        public virtual DomainModel Domain { get; set; }
        public virtual DropDownModel DomainAttribute { get; set; }
    }
}
