using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class LeaveCredit : BaseEntity
{
    public string EmployeeId { get; set; }
    public string LeaveTypeId { get; set; }
    public decimal CasualLeaves { get; set; }
    public decimal SickLeaves { get; set; }
    public string CreditReason { get; set; }
    public int LeaveCreditsforYear { get; set; }
    public string Note { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public string CreatedById { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }
    public bool IsDefault { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual DropDown LeaveTypes { get; set; }
    public virtual ApplicationUser CreatedBy { get; set; }
}