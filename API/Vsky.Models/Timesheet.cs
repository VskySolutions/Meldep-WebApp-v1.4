using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class Timesheet : BaseEntity
{        
    public string SiteId { get; set; }

    public string EmployeeId { get; set; }

    public DateTime? TimesheetDate { get; set; }

    public string CreatedById { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public bool Deleted { get; set; }

    [NotMapped]
    public bool IsActionVisible { get; set; }

    public virtual Site Sites{ get; set; }

    public virtual Employee Employee { get; set; }
    public virtual ApplicationUser User { get; set; }
    public virtual ICollection<TimesheetLines> TimesheetLines { get; set; } = new List<TimesheetLines>();
}
