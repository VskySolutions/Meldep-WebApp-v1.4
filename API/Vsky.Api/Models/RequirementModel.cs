using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Core;

namespace Vsky.Api.Models
{
    public record RequirementModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectModuleId { get; set; }
        public string RequirementGroupId { get; set; }
        public string AreaId { get; set; }
        public string WorkspaceId { get; set; }
        public string RequirementTypeId { get; set; }
        public string ApprovalStatus { get; set; }
        public string RequirementEnteredBy { get; set; }
        public string StatusId { get; set; }
        public string IdentifiedUserType { get; set; }
        public string IdentifiedEmployeeId { get; set; }
        public string IdentifiedCustomerId { get; set; }
        public string PriorityId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? IdentifiedDate { get; set; }
        public string IdentifiedDateStr { get; set; }
        public DateTime? CloseDate { get; set; }
        public string CloseDateStr { get; set; }
        public string Notes { get; set; }
        public int EditingStatus { get; set; }
        public int RequirementNumber { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public int RequirementNotesCount { get; set; }
        public string LastNote { get; set; }
        public bool IsPinned { get; set; }
        public string RequirementColor { get; set; }
        public List<string> RequirementIds { get; set; }

        public virtual Site Site { get; set; }
        public virtual Project Project { get; set; }
        public virtual ProjectModule ProjectModule { get; set; }
        public virtual RequirementGroup RequirementGroup { get; set; }
        public virtual DropDown Area { get; set; }
        public virtual DropDown Workspace { get; set; }
        public virtual DropDown RequirementType { get; set; }
        public virtual DropDown ApprovalStatusDropDown { get; set; }
        public virtual Employee RequirementEntered { get; set; }
        public virtual DropDown Status { get; set; }
        public virtual DropDown UserType { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Person Customer { get; set; }
        public virtual DropDown Priority { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<FilePathDetailsModel> FilePathDetailsModel { get; set; } = new List<FilePathDetailsModel>();
        public virtual ICollection<FilePathDetails> FilePathDetails { get; set; } = new List<FilePathDetails>();
        public virtual ICollection<RequirementChangeLogModel> RequirementChangeLogModel { get; set; } = new List<RequirementChangeLogModel>();
        public virtual ICollection<RequirementChangeLog> RequirementChangeLog { get; set; } = new List<RequirementChangeLog>();
        public virtual ICollection<ProjectTaskRelatedMapping> ProjectTaskRelatedMappings { get; set; } = new List<ProjectTaskRelatedMapping>();
        public virtual ICollection<RequirementTags> RequirementTags { get; set; } = new List<RequirementTags>();
    }

    public record RequirementSearchModel : BaseSearchModel
    {
        public int RequirementNumber { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string EditingStatus { get; set; }

        public List<string> ProjectIds { get; set; }
        public List<string> ProjectModuleIds { get; set; }
        public List<string> RequirementGroupIds { get; set; }
        public List<string> StatusIds { get; set; }
        public List<string> RequirementTypeIds { get; set; }
        public string identifiedUserTypeId { get; set; }
        public List<string> identifiedCustomerIds { get; set; }
        public List<string> identifiedEmployeeIds { get; set; }
        public List<string> RequirementTagIds { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchText { get; set; }
    }

    public record RequirementListModel : BasePagedListModel<RequirementModel>
    {
        public bool editing { get; set; }
    }
    public class RequirementListWithSummary
    {
        public List<VWProjectRequirementStatusSummary> RequirementStatusSummary { get; set; }
    }
    public record RequirementUploadModel : BaseEntityModel
    {
    }
}
