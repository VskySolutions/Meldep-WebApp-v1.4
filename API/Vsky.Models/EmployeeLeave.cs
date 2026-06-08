using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class EmployeeLeave : BaseEntity
{
    public string EmployeeId { get; set; }
    public string LeaveApproverId { get; set; }
    public string FileId { get; set; }
    //public string LeaveStatus { get; set; }
    public string LeaveStatusId { get; set; }
    public string LeaveCategoryId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal NoofLeaves { get; set; }
    public string Reason { get; set; }
    public string ApproverNote { get; set; }
    public string HRNote { get; set; }
    public string HalfDayType { get; set; }
    public bool IsHalfDay { get; set; }
    public bool IsPaidLeave { get; set; }
    public bool IsSandwich { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public string CreatedById { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual Employee LeaveApprover { get; set; }
    public virtual Picture File { get; set; }
    public virtual DropDown LeaveStatuses { get; set; }
    public virtual DropDown LeaveCategories { get; set; }

    // public virtual EmployeeDesignation EmpDesignation { get; set; }

    //public virtual ApplicationUser CreatedBy { get; set; }
}
public class SandwichLeaveResult
{
    public bool IsSandwich { get; set; }
    public DateTime? SeriesStart { get; set; }
    public DateTime? SeriesEnd { get; set; }
    public decimal TotalDays { get; set; }
}
