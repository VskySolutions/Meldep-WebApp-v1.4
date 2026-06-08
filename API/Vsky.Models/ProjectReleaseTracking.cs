using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models
{
    public class ProjectReleaseTrackingList
    {
        public virtual ICollection<ProjectReleaseTracking> ProjectReleaseTrackingsList { get; set; } = new List<ProjectReleaseTracking>();
        public int Total { get; set; }
    }
    public class ProjectReleaseTrackingReqPlanTaskIssueMappingsList
    {
        public virtual ICollection<ProjectReleaseTrackingReqPlanTaskIssueMapping> ProjectReleaseTrackingReqPlanTaskIssueMappingList { get; set; } = new List<ProjectReleaseTrackingReqPlanTaskIssueMapping>();
        public int Total { get; set; }
    }
    public class ProjectReleaseTracking : BaseEntity
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }
        public string InfraInstanceId { get; set; }
        public string DeploymentOwnerId { get; set; }
        public string ApproverId { get; set; }
        public string TesterId { get; set; }
        public string ReleaseTypeId { get; set; }
        public string VersionNumber { get; set; }
        public string Name { get; set; }
        [NotMapped]
        public string StatusId { get; set; }
        [NotMapped]
        public string StatusText { get; set; }
        [NotMapped]
        public string PreviousStatusText { get; set; }
        public DateTime PlannedReleaseDate { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual Project Project { get; set; }
        public virtual InfraProjectInstance InfraInstance { get; set; }
        public virtual Employee DeploymentOwner { get; set; }
        public virtual Employee Approver { get; set; }
        public virtual Employee Tester { get; set; }
        public virtual DropDown ReleaseType { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<ProjectReleaseTrackingStatusLog> ProjectReleaseTrackingStatusLog { get; set; } = new List<ProjectReleaseTrackingStatusLog>();
    }
    public class ProjectReleaseTrackingReqPlanTaskIssueMapping : BaseEntity
    {
        public string ReleaseTrackingId { get; set; }
        public string RequirementId { get; set; }
        public string WeeklyPlanId { get; set; }
        public string MonthlyPlanId { get; set; }
        public string TaskId { get; set; }
        public string IssueId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual ProjectReleaseTracking ReleaseTracking { get; set; }
        public virtual Requirement Requirement { get; set; }
        public virtual ProjectWeeklyPlan WeeklyPlan { get; set; }
        public virtual ProjectWeeklyPlan MonthlyPlan { get; set; }
        public virtual ProjectTask Task { get; set; }
        public virtual Issue Issue { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
    public class ProjectReqPlanTaskIssueItemDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public DateTime? Date { get; set; }
    }
    public class ProjectReleaseTrackingStatusLog : BaseEntity
    {
        public string ReleaseTrackingId { get; set; }
        public string StatusId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual ProjectReleaseTracking ReleaseTracking { get; set; }
        public virtual DropDown Status { get; set; }
    }
    public class SaveProjectReleaseTracking
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }
        public string InfraInstanceId { get; set; }
        public string DeploymentOwnerId { get; set; }
        public string ApproverId { get; set; }
        public string TesterId { get; set; }
        public string ReleaseTypeId { get; set; }
        public string VersionNumber { get; set; }
        public string Name { get; set; }
        public string PlannedReleaseDateStr { get; set; }
        public string Description { get; set; }
        public string Tab { get; set; }
        public bool? IsDraft { get; set; }
        public string ProjectName { get; set; }
        public virtual ICollection<SaveProjectReleaseTrackingReqPlanTaskIssueMapping> ProjectReleaseTrackingReqPlanTaskIssueList { get; set; } = new List<SaveProjectReleaseTrackingReqPlanTaskIssueMapping>();
    }
    public class SaveProjectReleaseTrackingReqPlanTaskIssueMapping
    {
        public string Id { get; set; }
        public string RefId { get; set; }
        public string Type { get; set; }
    }
    public class SaveProjectReleaseTrackingStatusLog
    {
        public string ReleaseTrackingId { get; set; }
        public string StatusId { get; set; }
    }
}

