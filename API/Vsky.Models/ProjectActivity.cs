using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models;

public class ProjectActivity : BaseEntity
{
    public string SiteId { get; set; }
    public string ProjectId { get; set; }
    public string ProjectModuleId { get; set; }
    public string TaskId { get; set; }
    public string ActivityStatusId { get; set; }
    public string AssignedToId { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public decimal EstimateHours { get; set; }
   
    public bool Active { get; set; }
    public int SortOrder { get; set; }
    public string CreatedById { get; set; }
    public string UpdatedById { get; set; }
    public bool Deleted { get; set; }

    public DateTime? DueDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? TargetMonth { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    [NotMapped]
    public int ActivitiesCount { get; set; }
    [NotMapped]
    public bool IsDescription { get; set; }

    [NotMapped]
    public string DisplayText { get; set; }

    public virtual Project Project { get; set; }
    public virtual ProjectModule ProjectModule { get; set; }
    public virtual ProjectTask Task { get; set; }
    public virtual Employee AssignedTo { get; set; }
    public virtual DropDown ActivityStatus { get; set; }

    public virtual ApplicationUser CreatedByUser { get; set; }
    public virtual ApplicationUser UpdatedByUser { get; set; }
    public virtual ICollection<ProjectActivityFiles> ProjectTaskActivityFilesList { get; set; } = new List<ProjectActivityFiles>();
}