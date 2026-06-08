using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class EmployeeOrgLocation : BaseEntity
{
    public string EmployeeId { get; set; }
    public string OrgLocationId { get; set; }   

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? CreatedOnUtc { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public string CreatedById { get; set; }

    public string UpdatedById { get; set; }

    public bool Deleted { get; set; }

    public bool Active { get; set; }
    public string Duration { get; set; }
    public string Note { get; set; }

    public virtual Employee Employee { get; set; }
    public virtual DropDown OrgLocation { get; set; }
}

