using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record LeadUserGroupMappingModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string UserId { get; set; }
        public string LeadGroupId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public string[] LeadGroupIds { get; set; }
        public string[] UserIds { get; set; }

        public virtual Site Site { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual DropDown LeadGroup { get; set; }
    }
    public record LeadUserGroupMappingSearchModel : BaseSearchModel
    {
        public List<string> UserIds { get; set; }
        public List<string> LeadGroupIds { get; set; }
        public string SearchText { get; set; }
    }

    public record LeadUserGroupMappingListModel : BasePagedListModel<LeadUserGroupMappingModel>
    {
        public bool editing { get; set; }
    }

}

