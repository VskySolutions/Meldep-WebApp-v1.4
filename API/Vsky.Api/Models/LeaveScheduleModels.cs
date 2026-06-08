using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record LeaveScheduleModels : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string LeaveRuleId { get; set; }
        public DateTime? Date { get; set; }
        public List<string> SelectedDates { get; set; } // Add this property
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual LeaveRules LeaveRules { get; set; }
        public virtual Site Site { get; set; }
    }

    public record LeaveScheduleSearchModel : BaseSearchModel
    {
      
    }
    public record LeaveScheduleListModel : BasePagedListModel<LeaveScheduleModels>
    {
        public bool editing { get; set; }
    }
}
