using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ReportRoleGroupMapping : BaseEntity
    {
        public string SiteId { get; set; }
        public string SiteRoleId { get; set; }
        public string ReportGroupId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual SitesRoles SitesRoles { get; set; }
        public virtual DropDown ReportGroup { get; set; }

    }
}
