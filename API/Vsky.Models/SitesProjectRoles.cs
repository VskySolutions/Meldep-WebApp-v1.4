using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models
{
    public class SitesProjectRoles : BaseEntity
    {
        public string SiteId { get; set; }
        public string MasterProjectRoleId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual MasterProjectRoles MasterProjectRoles { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        //public virtual ICollection<SitesProjectRolesPermissions> SitesProjectRolesPermissions { get; set; }
        public virtual ICollection<SitesProjectRolesPermissions> SitesProjectRolesPermissions { get; set; } = new List<SitesProjectRolesPermissions>();
    }
    public class SitesProjectRolesPermissions : BaseEntity
    {
        public string SiteProjectRoleId { get; set; }
        public bool FullAccess { get; set; }
        public bool ViewOnly { get; set; }
        public bool Notes { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual SitesProjectRoles SitesProjectRoles { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
    }
}


