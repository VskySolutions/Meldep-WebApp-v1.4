using System;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record SitesModifiedLogsModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ModuleId { get; set; }
        public string SubModule { get; set; }
        public string SubModuleId { get; set; }
        public string ColumnName { get; set; }
        public string ColumnValue { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedOnUtc { get; set; }
        public string LastModifiedonUtcStr { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Sites { get; set; }
        public virtual ApplicationUser user { get; set; }
    }

    public record SitesModifiedLogsSearchModel : BaseSearchModel
    {
    }

    public record SitesModifiedLogsListModel : BasePagedListModel<SitesModifiedLogsModel>
    {
    }
}
