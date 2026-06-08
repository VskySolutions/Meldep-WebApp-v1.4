using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{

    public class Requirement : BaseEntity
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
        public DateTime? CloseDate { get; set; }
        public string Notes { get; set; }
        public int EditingStatus { get; set; }
        public int RequirementNumber { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public int RequirementNotesCount { get; set; }

        [NotMapped]
        public string LastNote { get; set; }

        [NotMapped]
        public bool IsPinned { get; set; }

        [NotMapped]
        public string RequirementColor { get; set; }

        public virtual Site Site { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
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
        public virtual ICollection<FilePathDetails> FilePathDetails { get; set; } = new List<FilePathDetails>();
        public virtual ICollection<RequirementChangeLog> RequirementChangeLog { get; set; } = new List<RequirementChangeLog>();
        public virtual ICollection<ProjectTaskRelatedMapping> ProjectTaskRelatedMappings { get; set; } = new List<ProjectTaskRelatedMapping>();
        public virtual ICollection<RequirementTags> RequirementTags { get; set; } = new List<RequirementTags>();
        public virtual ICollection<RequirementPinned> RequirementPinned { get; set; } = new List<RequirementPinned>();
        public virtual ICollection<RequirementColor> RequirementColors { get; set; } = new List<RequirementColor>();
    }

    public class RequirementTags : BaseEntity
    {
        public string RequirementId { get; set; }
        public string AspNetUserId { get; set; }
        public string TagId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Requirement Requirement { get; set; }
        public virtual Tags Tags { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
