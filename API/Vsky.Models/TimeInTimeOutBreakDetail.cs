using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class TimeInTimeOutBreakDetail : BaseEntity
{
    public string TimeInTimeOutId { get; set; }
    public string MovementRegisterId { get; set; }
    //public string SiteId { get; set; }
    public TimeSpan BreakIn { get; set; }
    public TimeSpan BreakOut { get; set; }
    public TimeSpan TotalBreak { get; set; }
    public string BreakReason { get; set; }
    public string CreatedById { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    [NotMapped]
    public string BreakInStr { get; set; }
    [NotMapped]
    public string BreakOutStr { get; set; }

    //public virtual Site Sites { get; set; }
    public virtual TimeInTimeOut TimeInTimeOut { get; set; }
}


