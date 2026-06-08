using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models
{
    public class ProjectWeeklyPlanList
    {
        public virtual ICollection<ProjectWeeklyPlan> WeeklyPlanList { get; set; } = new List<ProjectWeeklyPlan>();
        public int Total { get; set; }
    }

    public class ProjectWeeklyPlan : BaseEntity
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }

        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Project Project { get; set; }
        public virtual ICollection<ProjectWeeklyPlanDates> ProjectWeeklyPlanDates { get; set; } = new List<ProjectWeeklyPlanDates>();
    }

    public class ProjectWeeklyPlanDates : BaseEntity
    {
        public string ProjectWeeklyPlanId { get; set; }
        public string PlanTypeId { get; set; }
        public DateTime WeekDate { get; set; }

        public bool IsApproved { get; set; }
        public string ApprovedById { get; set; }
        public DateTime? ApprovedOnUtc { get; set; }

        public bool IsCompleted { get; set; }
        public int CompletionPercentage { get; set; }
        public string CompletedById { get; set; }
        public DateTime? CompletedOnUtc { get; set; }

        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ApplicationUser ApprovedBy { get; set; }
        public virtual ApplicationUser CompletedBy { get; set; }
        public virtual ProjectWeeklyPlan ProjectWeeklyPlan { get; set; }
        public virtual DropDown PlanType { get; set; }
        public virtual ICollection<ProjectWeeklyPlanDatesLines> ProjectWeeklyPlanDatesLines { get; set; } = new List<ProjectWeeklyPlanDatesLines>();
        public virtual ICollection<ProjectWeeklyPlanDatesReqTaskIssueMapping> ProjectWeeklyPlanDatesReqTaskIssueMapping { get; set; } = new List<ProjectWeeklyPlanDatesReqTaskIssueMapping>();

        [NotMapped]
        public virtual List<EmployeeEstimateHoursForWeekSummary> EmployeeEstimateHoursForWeekSummaryList { get; set; } = new List<EmployeeEstimateHoursForWeekSummary>();
    }

    public class ProjectWeeklyPlanDatesReqTaskIssueMapping : BaseEntity
    {
        public string ProjectWeeklyPlanDatesId { get; set; }
        public string RequirementId { get; set; }
        public string TaskId { get; set; }
        public string IssueId { get; set; }

        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ProjectWeeklyPlanDates ProjectWeeklyPlanDates { get; set; }
        public virtual Requirement Requirement { get; set; }
        public virtual ProjectTask Task { get; set; }
        public virtual Issue Issue { get; set; }
    }

    public class ProjectWeeklyPlanDatesLines : BaseEntity
    {
        public string ProjectWeeklyPlanDatesId { get; set; }
        public string ExpectedDescription { get; set; }
        public string ActualDescription { get; set; } = "";
        public decimal ExpectedHours { get; set; }

        public string ExpectedDescriptionCreatedById { get; set; }
        public DateTime ExpectedDescriptionCreatedOnUtc { get; set; }
        public string ExpectedDescriptionUpdatedById { get; set; }
        public DateTime ExpectedDescriptionUpdatedOnUtc { get; set; }

        public string? ActualDescriptionCreatedById { get; set; }
        public DateTime? ActualDescriptionCreatedOnUtc { get; set; }
        public string? ActualDescriptionUpdatedById { get; set; }
        public DateTime? ActualDescriptionUpdatedOnUtc { get; set; }
        
        [NotMapped]
        public bool IsEditExpectedDescription { get; set; }
        [NotMapped]
        public bool IsEditActualDescription { get; set; }
        
        public string? DeletedById { get; set; }
        public DateTime? DeletedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ApplicationUser ExpectedDescriptionCreatedBy { get; set; }
        public virtual ApplicationUser ExpectedDescriptionUpdatedBy { get; set; }
        public virtual ApplicationUser ActualDescriptionCreatedBy { get; set; }
        public virtual ApplicationUser ActualDescriptionUpdatedBy { get; set; }
        public virtual ProjectWeeklyPlanDates ProjectWeeklyPlanDates { get; set; }

        public virtual ICollection<ProjectWeeklyPlanDatesLinesAssignedTo> ProjectWeeklyPlanDatesLinesAssignedTo { get; set; } = new List<ProjectWeeklyPlanDatesLinesAssignedTo>();
    }

    public class ProjectWeeklyPlanDatesLinesAssignedTo : BaseEntity
    {
        public string ProjectWeeklyPlanDatesLineId { get; set; }
        public string EmployeeId { get; set; }
        public decimal EstimatedHours { get; set; }

        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ProjectWeeklyPlanDatesLines ProjectWeeklyPlanDatesLine { get; set; }
        public virtual Employee Employee { get; set; }
    }

    #region Used For Add Or Custom Actions
    public class SaveProjectWeeklyPlan
    {
        public string ProjectId { get; set; }
        public string PlanTypeId { get; set; }
        public string WeekDate { get; set; }

        public List<SaveWeeklyLines> weekDateLines { get; set; } = new List<SaveWeeklyLines>();
    }
    public class SaveWeeklyLines
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public decimal ExpectedHours { get; set; }

        public List<SaveWeeklyLinesAssignTo> saveWeeklyLinesAssignTos { get; set; } = new List<SaveWeeklyLinesAssignTo>();
    }
    public class SaveWeeklyLinesAssignTo
    {
        public string Id { get; set; }
        public string ProjectWeeklyPlanDatesLineId { get; set; }
        public string EmployeeId { get; set; }
        public decimal EstimatedHours { get; set; }
    }
    public class LinkReqTaskIssueToDate
    {
        public string ProjectId { get; set; }
        public string PlanTypeId { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string[] Ids { get; set; }
    }
    public class EmployeeEstimatedHoursDropdownList
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set;}
    }
    public class EmployeeEstimateHoursForWeekSummary
    {
        public string EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public decimal TotalEstimatedHours { get; set; }
    }
    #endregion
}
