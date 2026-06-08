using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record TagModels : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string Name { get; set; }
        public List<string> TagsNameList { get; set; }
        public string Color { get; set; }
        public string BgColor { get; set; }
        public int? SortOrder { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public bool IsBgColor { get; set; }
        public string Flag { get; set; }

        public string TaskId { get; set; }
        public string ProjectId { get; set; }
        public List<string> TaskIds { get; set; }
        public List<string> RequirementIds { get; set; }

        public virtual Site Sites { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }

    public record TagSearchModel : BaseSearchModel
    {
        public string Name { get; set; }
        public string SearchText { get; set; }
    }
    public record TagListModel : BasePagedListModel<TagModels>
    {
    }
}
