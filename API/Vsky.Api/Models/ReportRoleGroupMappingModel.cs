using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ReportRoleGroupMappingModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string SiteRoleId { get; set; }
        public string ReportGroupId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual SitesRoles SitesRoles { get; set; }
        public virtual DropDown ReportGroup { get; set; }

        public string[] ReportGroupIds { get; set; }
    }
    public record ReportRoleGroupMappingSearchModel : BaseSearchModel
    {
        public List<string> SiteRoleIds { get; set; }
        public List<string> ReportGroupIds { get; set; }
        public string SearchText { get; set; }
    }

    public record ReportRoleGroupMappingListModel : BasePagedListModel<ReportRoleGroupMappingModel>
    {
        public bool editing { get; set; }
    }

}
