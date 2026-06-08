using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record SetReminderModels : BaseEntityModel
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

    public record SetReminderSearchModels : BaseSearchModel
    {
        public string Note { get; set; }
    }
    public record SetReminderListModel : BasePagedListModel<SetReminderModels>
    {
    }
}
