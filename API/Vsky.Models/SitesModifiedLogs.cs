using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class SitesModifiedLogs : BaseEntity
    {
        public string SiteId { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ModuleId { get; set; }
        public string SubModule { get; set; }
        public string SubModuleId { get; set; }
        public string ColumnName { get; set; }
        public string ColumnValue { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedOnUtc { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public string LastModifiedonUtcStr { get; set; }

        public virtual Site Sites { get; set; }
        public virtual ApplicationUser user { get; set; }
    }
}
