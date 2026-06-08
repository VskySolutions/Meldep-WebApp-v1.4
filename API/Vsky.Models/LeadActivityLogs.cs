using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class LeadActivityLogs : BaseEntity
    {
        public string LeadsId { get; set; }
        public string LeadActivityId { get; set; }
        public string LeadStageId { get; set; }
        public DateTime? ActivityDate { get; set; }
        public string ActivityNote { get; set; }
        public bool IsFutureActivity { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual Lead Leads { get; set; }
        public virtual LeadActivities LeadActivity { get; set; }
        public virtual LeadStages LeadStage { get; set; }
        public virtual ApplicationUser User { get; set; }

        //public virtual ICollection<SetReminder> SetReminders { get; set; } = new List<SetReminder>();
    }
}
