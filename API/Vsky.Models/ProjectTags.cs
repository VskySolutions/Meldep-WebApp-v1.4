using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ProjectTags : BaseEntity
    {
        public string ProjectId { get; set; }
        public string AspNetUserId { get; set; }
        public string TagId { get; set; }
        public bool Deleted { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public virtual Project Projects { get; set; }
        public virtual Tags Tags { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
