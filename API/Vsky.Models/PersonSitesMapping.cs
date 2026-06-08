using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class PersonSitesMapping : BaseEntity
{
    public string PersonId { get; set; }
    public string SiteId { get; set; }
    public DateTime? CreatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public string UpdatedById { get; set; }    
    public bool IsSharedUser { get; set; }
    public bool Deleted { get; set; }
    public bool LastUsed { get; set; }

    public virtual Site Sites { get; set; }
    public virtual Person Person { get; set; }
    public virtual ApplicationUser CreatedBy { get; set; }
    public virtual ApplicationUser UpdatedBy { get; set; }

}

