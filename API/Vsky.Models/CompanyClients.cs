using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class CompanyClients : BaseEntity
    {
        public string CompanyId { get; set; }
        public string PersonId { get; set; }
        public string SiteId { get; set; }
        public string CustomerTypeId { get; set; }
        public string AssignedToId { get; set; }
        public string ClientId { get; set; }
        public DateTime? AssignedDate { get; set; }

        public DateTime? CreatedOnUtc { get; set; }
        public string? CreatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string? UpdatedById { get; set; }
        public string ParentCustomerId { get; set; }

        [NotMapped]
        public string Name { get; set; }

        [NotMapped]
        public string? ParentCustomerName { get; set; }

        [NotMapped]
        public int CustomerNoteCount { get; set; }
        public bool Deleted { get; set; }
        public virtual Company? Company { get; set; }
        public virtual Person? Person { get; set; }
        public virtual Site Sites { get; set; }
        public virtual Employee? AssignedTo { get; set; }
        public virtual DropDown? CustomerType { get; set; }
        public virtual ApplicationUser? CreatedBy { get; set; }
        public virtual ApplicationUser? UpdatedBy { get; set; }
    }
}
