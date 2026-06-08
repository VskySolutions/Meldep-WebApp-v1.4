using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class Issue : BaseEntity
{
    public string SiteId { get; set; }
    public string ProjectId { get; set; }
    public string ProjectModuleId { get; set; }
    public string AreaId { get; set; }
    public string WorkspaceId { get; set; }
    public string PriorityId { get; set; }
    public string StatusId { get; set; }
    public string TypeId { get; set; }
    public string EmployeeId { get; set; }
    public string TestCaseId { get; set; }
    public string ClosedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public string ReportedById { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public DateTime? CloseDate { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsTaskCreated { get; set; }
    public int IssueNumber { get; set; }
    public string CreatedById { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    [NotMapped]
    public int IssueNotesCount { get; set; }

    public virtual Site Site { get; set; }
    public virtual Project Project { get; set; }
    public virtual DropDown Area { get; set; }
    public virtual DropDown Workspace { get; set; }
    public virtual DropDown Priority { get; set; }
    public virtual DropDown Status { get; set; }
    public virtual DropDown Type { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual Employee ClosedByEmployee { get; set; }
    public virtual Employee LastModifiedByEmployee { get; set; }
    public virtual Employee ReportedBy { get; set; }
    public virtual TestCase TestCase { get; set; }
    public virtual ProjectModule ProjectModule { get; set; }
    public virtual ICollection<IssueStatusChangedLog> IssueStatusChangedLog { get; set; } = new List<IssueStatusChangedLog>();
    public virtual ICollection<IssueActivity> IssueActivity { get; set; } = new List<IssueActivity>();
    public virtual ICollection<ProjectTaskRelatedMapping> ProjectTaskRelatedMappings { get; set; } = new List<ProjectTaskRelatedMapping>();
    //public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
    //public virtual ICollection<ProjectUserMapping> ProjectUserMappings { get; set; } = new List<ProjectUserMapping>();
    //public virtual VWProjectIssueStatusSummary VWProjectIssueStatusSummary { get; set; }
}
