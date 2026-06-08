using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models;

public class TimeInTimeOut : BaseEntity
{
    public string SiteId { get; set; }
    public string EmployeeId { get; set; }
    public string WorkHoursApprovalStatusId { get; set; }

    public DateTime? TimeInDate { get; set; }
    public TimeSpan TimeIn { get; set; }
    public DateTime? TimeOutDate { get; set; }
    public TimeSpan TimeOut { get; set; }

    public TimeSpan TotalHours { get; set; }
    public TimeSpan TotalBreak { get; set; }
    public TimeSpan ActualHours { get; set; }

    public string CreatedById { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    [NotMapped]
    public string TimeInStr { get; set; }

    [NotMapped]
    public string TimeOutStr { get; set; }

    [NotMapped]
    public string ActualHoursStr { get; set; }

    public virtual Site Site { get; set; }
    public virtual ApplicationUser CreatedBy { get; set; }
    public virtual ApplicationUser UpdatedBy { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual DropDown WorkHoursApprovalStatus { get; set; }
    public virtual ICollection<TimeInTimeOutBreakDetail> TimeInTimeOutBreakDetailList { get; set; } = new List<TimeInTimeOutBreakDetail>();
}

