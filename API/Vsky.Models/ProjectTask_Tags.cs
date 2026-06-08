using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ProjectTask_Tags : BaseEntity
    {
        public string TaskId { get; set; }
        public string AspNetUserId { get; set; }
        public string TagId { get; set; }
        public bool Deleted { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public virtual ProjectTask Task { get; set; }
        public virtual Tags Tags { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
