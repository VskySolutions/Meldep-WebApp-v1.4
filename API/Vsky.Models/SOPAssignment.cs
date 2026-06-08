using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class SOPAssignment : BaseEntity
    {
        public string SiteId { get; set; }
        public string TemplateId { get; set; }
        public string AssignedToEmployeeId { get; set; }
        public string ApproverEmployeeId { get; set; }
        public string StatusId { get; set; }
        public string PriorityId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        [NotMapped]
        public int TotalCount { get; set; }
        [NotMapped]
        public int ApprovedCount { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual SOPTemplate Template { get; set; }
        public virtual Employee AssignedToEmployee { get; set; }
        public virtual Employee ApproverEmployee { get; set; }
        public virtual DropDown Status { get; set; }
        public virtual DropDown Priority { get; set; }
        public virtual ICollection<SOPAssignmentResponse> SOPAssignmentResponses { get; set; } = new List<SOPAssignmentResponse>();
    }

    public class SOPAssignmentResponse : BaseEntity
    {
        public string AssignementId { get; set; }
        public string SectionItemId { get; set; }

        public bool IsChecked { get; set; }
        public string Response { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedComment { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        [NotMapped]
        public string UpdatedDateStr { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual SOPAssignment Assignement { get; set; }
        public virtual SOPTemplateSectionItems SectionItem { get; set; }
        public virtual ICollection<SOPAssignmentResponseEvidences> SOPAssignmentResponseEvidences { get; set; } = new List<SOPAssignmentResponseEvidences>();
    }

    public class SOPAssignmentResponseEvidences : BaseEntity
    {
        public string ResponseId { get; set; }
        public string FileId { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public virtual SOPAssignmentResponse Response { get; set; }
        public virtual Picture File { get; set; }
    }
}
