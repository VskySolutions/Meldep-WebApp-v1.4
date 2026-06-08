using System;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models
{
    public class ReportUserMapping : BaseEntity
    {
        public string ReportSettingsDetailId { get; set; }
        public string AspNetUserId { get; set; }
        public bool FullAccess { get; set; }
        public bool ViewOnly { get; set; }
        public string CreatedById { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ReportSettingsDetails ReportSettingsDetail { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
