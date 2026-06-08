using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ReportSettingsDetails : BaseEntity
    {
        public string ReportSettingId { get; set; }
        public string ReportId { get; set; }
        public string SiteId { get; set; }
        public string ReportGroupId { get; set; }
        public string ReportName { get; set; }
        public string ReportDescription { get; set; }
        public string Url { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual ReportSettings ReportSetting { get; set; }
        public virtual DropDown ReportGroup { get; set; }
        public virtual Site Sites { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<ReportUserMapping> ReportUserMapping { get; set; } = new List<ReportUserMapping>();

    }
}
