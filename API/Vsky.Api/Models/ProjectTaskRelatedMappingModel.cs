using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ProjectTaskRelatedMappingModel : BaseEntityModel
    {
        public string TaskId { get; set; }
        public string RequirementId { get; set; }
        public string IssueId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ProjectTask ProjectTask { get; set; }
        public virtual Requirement Requirement { get; set; }
        public virtual Issue Issue { get; set; }
    }
    public record ProjectTaskRelatedMappingSearchModel : BaseSearchModel
    {
    }
    public record ProjectTaskRelatedMappingListModel : BasePagedListModel<ProjectTaskRelatedMappingModel>
    {
    }
}
