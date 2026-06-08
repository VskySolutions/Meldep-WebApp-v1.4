using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ProjectTaskRelatedMapping : BaseEntity
    {
        public string TaskId { get; set; }
        public string RequirementId { get; set; }
        public string IssueId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ProjectTask ProjectTask { get; set; }
        public virtual Requirement Requirement { get; set; }
        public virtual Issue Issue { get; set; }
    }
}
