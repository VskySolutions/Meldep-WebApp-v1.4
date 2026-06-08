using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class LeaveRuleLines : BaseEntity
{
    public string LeaveRuleId { get; set; }

    public string EmploymentTypeId { get; set; }
    public decimal CasualLeaves { get; set; }
    public decimal SickLeaves { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public string CreatedById { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    public virtual LeaveRules LeaveRules { get; set; }
    public virtual DropDown EmploymentType { get; set; }
}
