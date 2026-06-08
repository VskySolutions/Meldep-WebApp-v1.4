using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class LeaveRules : BaseEntity
{
    public string SiteId { get; set; }
    public int Year { get; set; }
    public bool IsGenerated { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public string CreatedById { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }
    public virtual Site Site { get; set; }
    public virtual ICollection<LeaveRuleLines> LeaveRuleLinesList { get; set; } = new List<LeaveRuleLines>();
}
