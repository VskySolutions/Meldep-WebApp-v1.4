using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class Notes : BaseEntity
    {
        public string SiteId { get; set; }
        public string Note { get; set; }
        public string Type { get; set; }
        //public string RecordId { get; set; }
        public string SubModuleId { get; set; }
        public string Module { get; set; }
        public string ModuleId { get; set; }
        public string Sub_Module { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        [NotMapped]
        public string CreatedDateStr { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Site Site { get; set; }
    }
}
