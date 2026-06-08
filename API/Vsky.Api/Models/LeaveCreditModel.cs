using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record LeaveCreditModel : BaseEntityModel
    {
        public string EmployeeId { get; set; }
        public string LeaveTypeId { get; set; }
        public decimal CasualLeaves { get; set; }
        public decimal SickLeaves { get; set; }
        public string CreditReason { get; set; }
        public int LeaveCreditsforYear { get; set; }
        public string Note { get; set; }
        //leave data
        public string EmployeeName { get; set; }

        public decimal RemainingLeaves { get; set; }
        public decimal UsedLeaves { get; set; }
        public decimal CreditLeaves { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool EmployeeActiveStatus { get; set; }
        public bool IsDefault { get; set; }

        public string LeaveCreditId { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual DropDown LeaveTypes { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }

    public record LeaveCreditSearchModel : BaseSearchModel
    {
        //public List<int> Years { get; set; }
        public int Years { get; set; }
        public List<string> EmployeeIds { get; set; }
        public string SearchText { get; set; }
        //public string PrimaryEmailAddress { get; set; }

    }
    public class LeaveBalanceDetailsResponse
    {
        public string TotalLeaves { get; set; }
        public string CasualLeaves { get; set; }
        public string SickLeaves { get; set; }
        public decimal LeaveBalance { get; set; }
        public List<string> OfficeLeaves { get; set; }
        public List<string> EmployeeLeaves { get; set; }
    }

    public record LeaveCreditListModel : BasePagedListModel<LeaveCreditModel>
    {
        // public bool editing { get; set; }
    }
}