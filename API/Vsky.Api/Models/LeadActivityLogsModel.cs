using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record LeadActivityLogsModel : BaseEntityModel 
    {
        public int ReminderAfterDays { get; set; }
        public string Time { get; set; }
        public string Note { get; set; }

        public string[] SetReminder { get; set; }
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
        public virtual ICollection<SetReminderModels> SetReminderModels { get; set; } = new List<SetReminderModels>();

        //public virtual ICollection<SetReminderModels> SetReminders { get; set; } = new List<SetReminderModels>();
    }
    public record LeadActivityLogsSearchModels : BaseSearchModel
    {
        public string ActivityNote { get; set; }
    }
    public record LeadActivityLogsListModel : BasePagedListModel<LeadActivityLogsModel>
    {
    }
}
