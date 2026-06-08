using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class DailyPlannerLine : BaseEntity
{
    public string DailyPlannerId { get; set; }
    public string Description { get; set; }
    public decimal Hours { get; set; }
    
    public string ProjectId { get; set; }
    public string ProjectModuleId { get; set; }
    public string ProjectTaskId { get; set; }
    public string ProjectActivityId { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public DateTime? CreatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public string UpdatedById { get; set; }
    public bool Deleted { get; set; }

    public virtual Project Project { get; set; }
    public virtual DailyPlanner DailyPlanner { get; set; }
    public virtual ProjectModule ProjectModule { get; set; }
    public virtual ProjectTask ProjectTask { get; set; }
    public virtual ProjectActivity ProjectActivity { get; set; }
}
