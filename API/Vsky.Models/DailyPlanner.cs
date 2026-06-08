using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualBasic;
using Vsky.Core;

namespace Vsky.Models;

public class DailyPlanner : BaseEntity
{
    public string SiteId { get; set; }
    public string EmployeeId { get; set; }
    public DateTime? DailyPlannerDate { get; set; }
    public bool IsForwordedToTimesheet { get; set; }
    public DateTime? CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public string UpdatedById { get; set; }
    public bool Deleted { get; set; }
    // public decimal TotalHours { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual Site Sites { get; set; }
    public virtual ApplicationUser User { get; set; }
    public virtual ICollection<DailyPlannerLine> DailyPlannerLines { get; set; } = new List<DailyPlannerLine>();
}
