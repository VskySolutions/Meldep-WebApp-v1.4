using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class TimesheetLines : BaseEntity
{
    public string TimesheetId { get; set; }

    public string ProjectId { get; set; }

    public string ProjectModuleId { get; set; }

    public string ProjectTaskId { get; set; }
    public string ProjectActivityId { get; set; }

    public string Description { get; set; }

    public decimal Hours { get; set; }
    public decimal BillableHours { get; set; }
    public string MeetingUId { get; set; }

    public string CreatedById { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public string? BillableCreatedById { get; set; }

    public DateTime? BillableCreatedOnUtc { get; set; }

    public bool Deleted { get; set; }

    //public virtual Employee AssignedTo { get; set; }

    public virtual Timesheet Timesheet { get; set; }

    public virtual Project Project { get; set; }

    public virtual ProjectTask Task { get; set; }

    public virtual ProjectModule ProjectModule { get; set; }

    public virtual ProjectActivity ProjectActivity { get; set; }

    public virtual ApplicationUser BillableCreatedBy { get; set; }

}
