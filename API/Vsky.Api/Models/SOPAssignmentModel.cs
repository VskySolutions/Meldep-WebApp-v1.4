using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record SOPAssignmentModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string TemplateId { get; set; }
        public string AssignedToEmployeeId { get; set; }
        public string ApproverEmployeeId { get; set; }
        public string StatusId { get; set; }
        public string PriorityId { get; set; }
        public string AssignedDateStr { get; set; }
        public string DueDateStr { get; set; }
        public int TotalCount { get; set; }
        public int ApprovedCount { get; set; }

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
        public bool IsSubmitted { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual SOPTemplateModel Template { get; set; }
        public virtual EmployeeModel AssignedToEmployee { get; set; }
        public virtual EmployeeModel ApproverEmployee { get; set; }
        public virtual DropDownModel Status { get; set; }
        public virtual DropDownModel Priority { get; set; }
        public virtual ICollection<SOPAssignmentResponseModel> SOPAssignmentResponses { get; set; } = new List<SOPAssignmentResponseModel>();
    }

    public record SOPAssignmentResponseModel
    {
        public string Id { get; set; }
        public string AssignementId { get; set; }
        public string SectionItemId { get; set; }

        public bool IsChecked { get; set; }
        public string Response { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedComment { get; set; }
        public List<IFormFile> EvidenceFiles { get; set; }
        public List<string> ExistingFiles { get; set; }
        public List<string> DeletedFiles { get; set; }
        public string FileChangeFlag { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public string UpdatedDateStr { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual SOPAssignmentModel Assignement { get; set; }
        public virtual SOPTemplateSectionItemsModel SectionItem { get; set; }
        public virtual ICollection<SOPAssignmentResponseEvidencesModel> SOPAssignmentResponseEvidences { get; set; } = new List<SOPAssignmentResponseEvidencesModel>();
    }

    public class SOPAssignmentResponseEvidencesModel : BaseEntity
    {
        public string ResponseId { get; set; }
        public string FileId { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public virtual SOPAssignmentResponseModel Response { get; set; }
        public virtual PicturesModel File { get; set; }
    }
    public record SOPAssignmentSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public string Name { get; set; }
        public List<string> TemplateIds { get; set; }
        public List<string> AssignedToEmployeeIds { get; set; }
        public List<string> ApproverEmployeeIds { get; set; }
        public List<string> StatusIds { get; set; }
        public List<string> PriorityIds { get; set; }
        public bool IsApproved { get; set; }
    }

    public class DeletedFileModel
    {
        public string id { get; set; }
        public string fileId { get; set; }
    }
    public record SOPAssignmentListModel : BasePagedListModel<SOPAssignmentModel>
    {
    }
}
