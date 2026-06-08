using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models;

public class ProjectModule : BaseEntity
{
    public string SiteId { get; set; }
    public string ProjectId { get; set; }
    public string ProjectModuleTypeId { get; set; }
    public string ProjectModuleStatusId { get; set; }

    public int ProjectModuleNumber { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }
    public string Notes { get; set; }
    public bool IsDuplicate { get; set; }
    public bool IsMoved { get; set; }
    public string IsDuplicateFromId { get; set; }
    public string MovedFromProjectId { get; set; }
    
    public int SortOrder { get; set; }
    public bool Active { get; set; }
    public string CreatedById { get; set; }
    public string UpdatedById { get; set; }
    public bool Deleted { get; set; }
    
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? CloseDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }

    [NotMapped]
    public int ProjectTasksCount { get; set; }
    [NotMapped]
    public int ProjectModuleNotesCount { get; set; }

    public virtual Project Project { get; set; }
    public virtual DropDown ProjectModuleStatus { get; set; }
    public virtual DropDown ProjectModuleType { get; set; }

    public virtual ApplicationUser CreatedBy { get; set; }
    public virtual ApplicationUser UpdatedBy { get; set; }

    public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
    public virtual ICollection<ProjectActivity> ProjectActivities { get; set; } = new List<ProjectActivity>();
    public virtual ICollection<ProjectModuleFiles> ProjectModuleFilesList { get; set; } = new List<ProjectModuleFiles>();
    public virtual ICollection<ProjectModulesUserMapping> ProjectModulesUserMappings { get; set; } = new List<ProjectModulesUserMapping>();
}

public class ProjectModulesUserMapping : BaseEntity
{
    public string ProjectModuleId { get; set; }
    public string AspNetUserId { get; set; }

    public bool FullAccess { get; set; }
    public bool ViewOnly { get; set; }
    public bool Notes { get; set; }

    public string CreatedById { get; set; }
    public DateTime? CreatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    public virtual ProjectModule ProjectModule { get; set; }
    public virtual ApplicationUser User { get; set; }
}

public class SaveProjectModulesUser
{
    public string ProjectModuleId { get; set; }
    public string AspNetUserId { get; set; }

    public bool FullAccess { get; set; }
    public bool ViewOnly { get; set; }
    public bool Notes { get; set; }

    public string CreatedById { get; set; }
    public DateTime? CreatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    public virtual ProjectModule ProjectModule { get; set; }
    public virtual ApplicationUser User { get; set; }
}