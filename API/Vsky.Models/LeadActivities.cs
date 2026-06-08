using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class LeadActivities : BaseEntity
{
    public string ActivityName { get; set; }
    public string ActivityDescription { get; set; }
    public DateTime? CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public string UpdatedById { get; set; }
    public bool Deleted { get; set; }
    public virtual ICollection<LeadActivityLogs> LeadActivityLogss { get; set; } = new List<LeadActivityLogs>();
}

