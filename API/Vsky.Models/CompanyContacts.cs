using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class CompanyContacts : BaseEntity
    {
        public string AlternateEmail { get; set; }
        //public string SiteId { get; set; }
        public string AlternatePhoneNumber { get; set; }
        public string CompanyId { get; set; }
        public string PersonId { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public int CompanyContactNotesCount { get; set; }
        public virtual Company Company { get; set; }
        //public virtual Site Site { get; set; }
        public virtual Person Person { get; set; }

    }
}
