using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class DomainAttributes : BaseEntity
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

        public virtual Domain Domain { get; set; }
        public virtual DropDown DomainAttribute { get; set; }
    }
}
