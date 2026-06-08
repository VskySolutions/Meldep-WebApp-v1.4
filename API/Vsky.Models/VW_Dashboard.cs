using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models
{
    public class VW_CustomerList
    {
        public virtual ICollection<VW_Customer> CustomerList { get; set; } = new List<VW_Customer>();
        public int Total {  get; set; }
    }
    public class VW_Customer : BaseEntity
    {
        public string SiteId { get; set; }
        public string CompanyId { get; set; }
        public string PersonId { get; set; }
        public string CustomerName { get; set; }
        public string AssignToId { get; set; }
        public string AssignTo { get; set; }
        public string CustomerTypeId { get; set; }
        public string CustomerType { get; set; }
        public string ParentCustomerId { get; set; }
        public string ParentCustomerName { get; set; }
        public int CustomerNotesCount { get; set; }
        public int ProjectCount { get; set; }
        public int TotalTaskCount { get; set; }
        public int TotalDoneTaskCount { get; set; }
        public DateTime? CreatedOnUtc { get; set; }

        public virtual VWCustomerTaskStatusSummary CustomerTaskStatusSummary { get; set; }
        public virtual ICollection<VW_Project> Projects { get; set; } = new List<VW_Project>();
    }

    public class VW_Project : BaseEntity
    {
        public string SiteId { get; set; }
        public int Year { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        //public string ProjectCategoryId { get; set; }
        //public string ProjectCategoryName { get; set; }
        //public string ProjectSubCategoryId { get; set; }
        //public string ProjectSubCategoryName { get; set; }
        public string ProjectTypeId { get; set; }
        //public string ProjectType { get; set; }
        public string ProjectTypeName { get; set; }
        public string ProjectStatusId { get; set; }
        public string Status { get; set; }
        public string ProjectPriorityId { get; set; }
        public string Priority { get; set; }
        public bool IsTemplate { get; set; }
        public bool IsPinned { get; set; }
        public bool Active { get; set; }
        public string ProjectColor { get; set; }
        public int ProjectNoteCount { get; set; }
        public int ProjectMessageCount { get; set; }
        public int TotalTaskCount { get; set; }
        public int TotalDoneTaskCount { get; set; }
        public int TotalProjectSwimlaneCount { get; set; }
        public int TotalProjectModuleCount { get; set; }
        public int ProjectModuleCloseCount { get; set; }
        public int TotalProjectIssueCount { get; set; }
        public int TotalProjectRequirementCount { get; set; }
        public int CompletedIssueCount { get; set; }
        public int CompletedRequirementCount { get; set; }
        public string CompanyContactId { get; set; }
        public DateTime? CreatedOnUtc { get; set; }

        public virtual VW_Customer Customer { get; set; }
        public virtual VWProjectTaskStatusSummary ProjectTaskStatusSummary { get; set; }
        public virtual VWProjectIssueStatusSummary ProjectIssueStatusSummary { get; set; }
        public virtual VWProjectRequirementStatusSummary ProjectRequirementStatusSummary { get; set; }

        public virtual ICollection<ProjectEmployeeMapping> ProjectEmployeeMappings { get; set; } = new List<ProjectEmployeeMapping>();
        public virtual ICollection<ProjectUserMapping> ProjectUserMappings { get; set; } = new List<ProjectUserMapping>();

    }

    public class VW_ProjectSwimLane : BaseEntity
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }
        public int ProjectYear { get; set; }
        public string ProjectName { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public string Color { get; set; }
        public int TotalTaskCount { get; set; }
        public int TotalDoneTaskCount { get; set; }
    }

    public class VW_ProjectModules : BaseEntity
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }
        public int ProjectYear { get; set; }
        public string ProjectName { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string ProjectModuleStatusId { get; set; }
        public int SortOrder { get; set; }
        public string Color { get; set; }
        public string ProjectModuleStatus { get; set; }
        public int TotalTaskCount { get; set; }
        public int TaskCloseCount { get; set; }
        public int TotalDoneTaskCount { get; set; }
        public virtual VW_Project Project { get; set; }

    }

    public class VW_ProjectTask : BaseEntity
    {
        public string SiteId { get; set;}
        public string ProjectId { get; set; }
        public int ProjectYear { get; set; }
        public string ProjectName { get; set; }
        //public string ProjectSwimlaneId { get; set; }
        //public string ProjectSwimLaneName { get; set; }
        public string ProjectModuleId { get; set; }
        public string ProjectListName { get; set; }
        public string StatusId { get; set; }
        public string Status { get; set; }
        public string PriorityId { get; set; }
        public string Priority { get; set; }
        public string AssignedToId { get; set; }
        public string AssignTo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrimaryEmailAddress { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public string ProjectTask_Tags { get; set; }
        public decimal EstimateTime { get; set; }
        public DateTime? TaskMonth { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public bool Active { get; set; }
        public decimal SortOrder { get; set; }
        public int TotalActivitiesCount { get; set; }
        public decimal TotalActivityHours { get; set; }
        public decimal TotalCloseActivityHours { get; set; }
        public int ActivityCloseCount { get; set; }
        public int ActivityCompletedCount { get; set; }
        public string Color { get; set; }
        public virtual VW_Project Project { get; set; }

        public virtual ICollection<VW_ProjectTaskActivities> ProjectActivities { get; set; } = new List<VW_ProjectTaskActivities>();

    }

    public class VW_ProjectTaskActivities : BaseEntity
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }
        //public string ProjectSwimlaneId { get; set; }
        public string ProjectModuleId { get; set; }
        public string ProjectTaskId { get; set; }
        public string ActivityStatusId { get; set; }
        public string Status { get; set; }
        public string taskStatus { get; set; }
        public string AssignedToId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrimaryEmailAddress { get; set; }
        public string ActivityName { get; set; }
        public string ActivityOwner { get; set; }
        public string ActivityDescription { get; set; }
        public decimal EstimateHours { get; set; }
        public bool Active { get; set; }
        public bool EmployeeStatus { get; set; }
        public DateTime? TargetMonth { get; set; }
        public DateTime? CreatedOnUtc { get; set; }

        [NotMapped]
        public string ActivityNameDescription { get; set; }

        public virtual VW_Project Project { get; set; }

        public virtual VW_ProjectTask ProjectTask { get; set; }

    }

    public class VWCustomerTaskStatusSummary : BaseEntity
    {
        public string CustomerId { get; set; }
        public int NoStatus { get; set; }
        public int Close { get; set; }
        public int Completed { get; set; }
        public int InDevelopment { get; set; }
        public int InQA { get; set; }
        public int InUAT { get; set; }
        public int New { get; set; }
        public int OnHold { get; set; }
        public int Open { get; set; }
        public int TestSite { get; set; }
        public int UATPassed { get; set; }
        public int Total { get; set; }

        public virtual VW_Customer Customer { get; set; }
    }

    public class VWProjectTaskStatusSummary : BaseEntity
    {
        public string ProjectId { get; set; }
        public int NoStatus { get; set; }
        public int Close { get; set; }
        public int Completed { get; set; }
        public int InDevelopment { get; set; }
        public int InQA { get; set; }
        public int InUAT { get; set; }
        public int New { get; set; }
        public int OnHold { get; set; }
        public int Open { get; set; }
        public int TestSite { get; set; }
        public int DevelopmentCompleted { get; set; }
        public int UnderDeployment { get; set; }
        public int UATPassed { get; set; }
        public int Total { get; set; }

        public virtual VW_Project Project { get; set; }
    }

    public class VW_EmployeeTaskActivitySummary : BaseEntity
    {
        public string AssignToId { get; set;}
        public string AssignTo { get; set;}

        public decimal NoStatusHours { get; set;}
        public int NoStatusCount { get; set;}

        public decimal CloseHours { get; set; }
        public int CloseCount { get; set; }

        public decimal CompletedHours { get; set; }
        public int CompletedCount { get; set; }

        public decimal InDevelopmentHours { get; set; }
        public int InDevelopmentCount { get; set; }

        public decimal DeadlineApproachingCounts { get; set; }
        public int DoneHours { get; set; }

        public decimal InQAHours { get; set; }
        public int InUATCount { get; set; }

        public decimal NewHours { get; set; }
        public int NewCount { get; set; }

        public decimal OnHoldHours { get; set; }
        public int OnHoldCount { get; set; }

        public decimal OpenHours { get; set; }
        public int OpenCount { get; set; }

        public decimal TestSiteHours { get; set; }
        public int TestSiteCount { get; set; }

        public decimal UATPassedHours { get; set; }
        public int UATPassedCount { get; set; }
        
        public decimal TotalHours { get; set; }
        public int TotalCounts { get; set; }
    }

    public class VWProjectIssueStatusSummary : BaseEntity
    {
        public string ProjectId { get; set; }

        public int NoStatus { get; set; }
        public int New { get; set; }
        public int InTesting { get; set; }
        public int ToDo { get; set; }
        public int Reopen { get; set; }
        public int InUAT { get; set; }
        public int UATPassed { get; set; }
        public int NewFromTestPlan { get; set; }
        public int ConvertedToTask { get; set; }
        public int Done { get; set; }
        public int InReview { get; set; }
        public int Closed { get; set; }
        public int OnHold { get; set; }
        public int InDevelopment { get; set; }
        public int Total { get; set; }

        public virtual VW_Project Project { get; set; }
    }

    public class VWProjectRequirementStatusSummary : BaseEntity
    {
        public string ProjectId { get; set; }

        public int NoStatus { get; set; }
        public int New { get; set; }
        public int Open { get; set; }
        public int InProgress { get; set; }
        public int OnHold { get; set; }
        public int Close { get; set; }
        public int Total { get; set; }

        public virtual VW_Project Project { get; set; }
    }
    public class VWEmployeeAssignedHours : BaseEntity
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime WeekendDate { get; set; }
        public decimal TotalHours { get; set; }
        public virtual Employee Employee { get; set; }
    }

    public class VW_UserTaskTags : BaseEntity
    {
        public string TaskId { get; set; }
        public string AspNetUserId { get; set; }
        public string TagId { get; set; }
        public string TagName { get; set; }
        public string Color { get; set; }
        public string BgColor { get; set; }

    }

    public class VW_UserProjectPinned : BaseEntity
    {
        public string ProjectId { get; set; }
        public string AspNetUserId { get; set; }
        public bool IsPinned { get; set; }

    }

    public class VW_UserProjectColor : BaseEntity
    {
        public string ProjectId { get; set; }
        public string AspNetUserId { get; set; }
        public string Color { get; set; }

    }

}
