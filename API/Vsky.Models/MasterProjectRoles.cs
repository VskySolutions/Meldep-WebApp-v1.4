using System;
using Vsky.Core;

namespace Vsky.Models
{
    public class MasterProjectRoles : BaseEntity
    {
        public string Name { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
    }
}

