using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ReportSettingsDetailsModel : BaseEntityModel
    {
        public string ReportSettingId { get; set; }
        public string SiteId { get; set; }
        public string ReportId { get; set; }
        public string ReportGroupId { get; set; }
        public string ReportName { get; set; }
        public string ReportDescription { get; set; }
        public string Url { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual DropDown ReportGroup { get; set; }
        public virtual Site Sites { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<ReportUserMapping> ReportUserMapping { get; set; } = new List<ReportUserMapping>();
    }

    public record ReportSettingsDetailsSearchModel : BaseSearchModel
    {
        public string ReportGroupId { get; set; }
        public List<string> ReportIds { get; set; }
        public string SearchText { get; set; }
    }

    public record ReportSettingsDetailsListModel : BasePagedListModel<ReportSettingsDetailsModel>
    {
        public bool ReportId { get; set; }
    }
}
