using System;
using Vsky.Core;

namespace Vsky.Models
{
    public class LeadUserGroupMapping : BaseEntity
    {
        public string SiteId { get; set; }
        public string UserId { get; set; }
        public string LeadGroupId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual DropDown LeadGroup { get; set; }
    }
}

