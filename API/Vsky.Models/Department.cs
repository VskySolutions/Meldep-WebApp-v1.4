using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class Department : BaseEntity
{
    public string SiteId { get; set; }
    public string Name { get; set; }

    public string Description { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public string CreatedById { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }
    public virtual Site Site { get; set; }

}