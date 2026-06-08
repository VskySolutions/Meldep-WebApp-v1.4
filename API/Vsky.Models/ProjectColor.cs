using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Vsky.Core;

namespace Vsky.Models
{
    public class ProjectColor : BaseEntity
    {
        public string ProjectId { get; set; }
        public string AspNetUserId { get; set; }
        public string Color { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Project Project { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
