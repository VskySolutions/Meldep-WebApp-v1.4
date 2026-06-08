using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models;

public class ProjectTask : BaseEntity
{
    public string SiteId { get; set; }
    public string ProjectId { get; set; }
    public string ProjectModuleId { get; set; }
    public string AreaId { get; set; }
    public string WorkspaceId { get; set; }
    public string StatusId { get; set; }
    public string PriorityId { get; set; }
    public string AssignedToId { get; set; }
    public string TypeId { get; set; }
    public string ActionId { get; set; }


    public int ProjectTaskNumber { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal EstimateTime { get; set; }
    public string Color { get; set; }
    public string Instructions { get; set; }

    public bool IsDuplicate { get; set; }
    public string IsDuplicateFromId { get; set; }

    public bool IsMoved { get; set; }
    public bool Active { get; set; }
    public decimal SortOrder { get; set; }
    public string CreatedById { get; set; }
    public string UpdatedById { get; set; }
    public bool Deleted { get; set; }

    [NotMapped]
    public int ActivitiesCount { get; set; }
    [NotMapped]
    public int ProjectTaskNotesCount { get; set; }
    [NotMapped]
    public decimal TotalTimesheetEstHours { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? TaskMonth { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public virtual Project Project { get; set; }
    public virtual ProjectModule ProjectModule { get; set; }
    public virtual DropDown Area { get; set; }
    public virtual DropDown Workspace { get; set; }
    public virtual Employee AssignedTo { get; set; }
    public virtual DropDown Priority { get; set; }
    public virtual DropDown Status { get; set; }
    public virtual DropDown Type { get; set; }
    public virtual DropDown Action { get; set; }

    public virtual ApplicationUser CreatedBy { get; set; }
    public virtual ApplicationUser UpdatedBy { get; set; }

    public virtual ICollection<ProjectActivity> ProjectActivities { get; set; } = new List<ProjectActivity>();
    public virtual ICollection<ProjectTaskStatusLog> ProjectTaskStatusLog { get; set; } = new List<ProjectTaskStatusLog>();
    public virtual ICollection<ProjectTaskFiles> ProjectTaskFilesList { get; set; } = new List<ProjectTaskFiles>();
    public virtual ICollection<ProjectTask_Tags> ProjectTask_Tags { get; set; } = new List<ProjectTask_Tags>();
    public virtual ICollection<ProjectTaskRelatedMapping> ProjectTaskRelatedMappings { get; set; } = new List<ProjectTaskRelatedMapping>();
    public virtual ICollection<ProjectWeeklyPlanDatesReqTaskIssueMapping> ProjectWeeklyPlanDatesReqTaskIssueMappingList { get; set; } = new List<ProjectWeeklyPlanDatesReqTaskIssueMapping>();
}