using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class SetReminder : BaseEntity
    {
        public string LeadActivityLogId { get; set; }
        public int ReminderAfterDays { get; set; }
        public string Time { get; set; }
        public string Note { get; set; }
        public DateTime? ReminderDateTime { get; set; }
        public bool IsMailStatus { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual LeadActivityLogs LeadActivityLogs { get; set; }
    }
}
