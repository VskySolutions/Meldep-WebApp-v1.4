using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models;

public class Project : BaseEntity
{
    public string SiteId { get; set; }
    public string ProjectCoordinatorId { get; set; }   
    public string ProjectPriorityId { get; set; }
    public string ProjectStatusId { get; set; }
    public string ProjectTypeId { get; set; }
    public string CustomerId { get; set; }
    public string CompanyContactId { get; set; }
    public string ProjectCategoryId { get; set; }
    public string ProjectSubcategoryId { get; set; }
    public string PlanApproverId { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public string Website { get; set; }
    public string Code { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? GoLiveDate { get; set; }
    
    public int Year { get; set; }
    public bool IsPinned { get; set; }
    public bool IsTemplate { get; set; }
    public string IsFrom { get; set; }
    public bool Active { get; set; }
    public int SortOrder { get; set; }

    public DateTime CreatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public string UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    [NotMapped]
    public int ProjectNotesCount { get; set; }

    [NotMapped]
    public int ProjectMessageCount { get; set; }

    [NotMapped]
    public int CompletedTaskCount { get; set; }

    [NotMapped]
    public int TotalTaskCount { get; set; }

    [NotMapped]
    public int TotalIssueCount { get; set; }

    [NotMapped]
    public int CompletedIssueCount { get; set; }

    [NotMapped]
    public string CustomerName { get; set; }

    [NotMapped]
    public DateTime? NewStartDate { get; set; }

    [NotMapped]
    public int CompletedRequirementCount { get; set; }

    [NotMapped]
    public int TotalRequirementCount { get; set; }

    [NotMapped]
    public decimal TotalTaskEstimateHours { get; set; }

    [NotMapped]
    public decimal TotalActivityHours { get; set; }

    [NotMapped]
    public int TotalModuleCount { get; set; }
    [NotMapped]
    public int TotalTasksCount { get; set; }

    [NotMapped]
    public string ProjectColor{ get; set; }

    public virtual CompanyClients Customer { get; set; }
    public virtual CompanyContacts CompanyContact { get; set; }
    public virtual DropDown ProjectPriority { get; set; }
    public virtual DropDown ProjectStatus { get; set; }
    public virtual DropDown ProjectType { get; set; }
    public virtual Employee ProjectCoordinator { get; set; }
    public virtual Employee PlanApprover { get; set; }
    public virtual DropDownType ProjectCategories { get; set; }
    public virtual DropDown ProjectCategoriesSubCategories { get; set; }
    public virtual ApplicationUser CreatedBy { get; set; }
    public virtual ApplicationUser UpdatedBy { get; set; }

    // Project Connections
    public virtual ICollection<ProjectEmployeeMapping> ProjectEmployeeMappings { get; set; } = new List<ProjectEmployeeMapping>();
    public virtual ICollection<ProjectUserMapping> ProjectUserMappings { get; set; } = new List<ProjectUserMapping>();
    public virtual ICollection<ProjectTags> ProjectTags { get; set; } = new List<ProjectTags>();
    public virtual ICollection<ProjectPinned> ProjectPinned { get; set; } = new List<ProjectPinned>();
    public virtual ICollection<ProjectColor> ProjectColors { get; set; } = new List<ProjectColor>();
    public virtual ICollection<ProjectFiles> ProjectFileList { get; set; } = new List<ProjectFiles>();
    public virtual ICollection<ProjectsMessages> ProjectsMessages { get; set; } = new List<ProjectsMessages>();

    // Primary DataFlow - Project
    public virtual ICollection<ProjectModule> ProjectModules { get; set; } = new List<ProjectModule>();
    public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
    public virtual ICollection<ProjectActivity> ProjectActivities { get; set; } = new List<ProjectActivity>();
    public virtual ICollection<Issue> Issue { get; set; } = new List<Issue>();
    public virtual ICollection<Requirement> Requirement { get; set; } = new List<Requirement>();
    public virtual ICollection<TestPlan> TestPlans { get; set; } = new List<TestPlan>();
    public virtual ICollection<TimesheetLines> TimesheetLine { get; set; } = new List<TimesheetLines>();
    public virtual ICollection<ProjectWeeklyPlan> ProjectWeeklyPlans { get; set; } = new List<ProjectWeeklyPlan>();
    public ICollection<InfraProjectServices> InfraProjectServices { get; set; }

    [NotMapped]
    public IEnumerable<ProjectWeeklyPlan> WeeklyPlan { get; set; }

    [NotMapped]
    public IEnumerable<ProjectWeeklyPlan> MonthlyPlan { get; set; }
    //public virtual ICollection<VWProjectRequirementStatusSummary> RequirementStatusSummary { get; set; } = new List<VWProjectRequirementStatusSummary>();
    //public virtual ICollection<VWProjectIssueStatusSummary> IssueStatusSummary { get; set; } = new List<VWProjectIssueStatusSummary>();
    //public virtual ICollection<VWProjectTaskStatusSummary> TaskStatusSummary { get; set; } = new List<VWProjectTaskStatusSummary>();

    // Secondary DataFlow - WorkBoard
    public virtual ICollection<ProjectSwimLanes> ProjectSwimLanes { get; set; } = new List<ProjectSwimLanes>();

    // Weekly/Monthly Planner
    [NotMapped]
    public virtual List<ProjectCharterGroupBy> ProjectCharterGroupByList { get; set; } = new List<ProjectCharterGroupBy>();
}
public class ProjectCharterGroupBy
{
    public string GroupId { get; set; }
    public string GroupValue { get; set; }
    public List<ProjectEmployeeMapping> EmployeeMappingList { get; set; } = new List<ProjectEmployeeMapping>();
}

public class MoveProjectModuleAsProject
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ProjectId { get; set; }
}

public class ProjectListRequest
{
    public string SiteId { get; set; }  

    public bool Deleted { get; set; }
    public bool IsTemplate { get; set; }
    public bool IsPinned { get; set; }
    public int Active { get; set; }

    public string AspNetUserId { get; set; }
    public List<string> CustomerIds { get; set; }
    public List<string> CompanyContactIds { get; set; }
    public List<string> Ids { get; set; }
    public List<string> CategoryIds { get; set; }
    public List<string> SubCategoryIds { get; set; }
    public List<string> StatusIds { get; set; }
    public List<string> PriorityIds { get; set; }
    public List<string> TypeIds { get; set; }

    public List<string> CoordinatorIds { get; set; }
    public List<string> ManagerIds { get; set; }
    public List<string> LeadIds { get; set; }

    public List<string> ProjectTagIds { get; set; }

    public string Search { get; set; }

    public string DefaultSortBy { get; set; }
    public string DefaultSortDirection { get; set; }

    public Dictionary<string, string> MultiColumnSorting { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}