using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record EmployeeLeaveModel : BaseEntityModel
    {
        public string EmployeeId { get; set; }
        public string LeaveApproverId { get; set; }
        public string FileId { get; set; }
        public string LeaveStatusId { get; set; }
        public string LeaveCategoryId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal NoofLeaves { get; set; }
        public string Reason { get; set; }
        public string ApproverNote { get; set; }
        public string ApproverName { get; set; }
        public string HRNote { get; set; }
        public string HalfDayType { get; set; }
        public bool IsHalfDay { get; set; }
        public bool FirstHalf { get; set; }
        public bool SecondHalf { get; set; }
        public bool HalfDay { get; set; }
        public string Flag { get; set; }
        public string LeaveStatusFlag { get; set; }
        public bool IsPaidLeave { get; set; }
        public bool IsSandwich { get; set; }
        public decimal TotalDeduction { get; set; }
        public int Year { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public string UpdatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public string FromDateStr { get; set; }
        public string ToDateStr { get; set; }
        public IFormFile FilePic { get; set; }
        public string FileChangeFlag { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Employee LeaveApprover { get; set; }
        public virtual Picture File { get; set; }
        public virtual DropDown LeaveStatuses { get; set; }
        public virtual DropDown LeaveCategories { get; set; }

        // public virtual EmployeeDesignation EmpDesignation { get; set; }
        //public virtual ApplicationUser CreatedBy { get; set; }
    }

    public record EmployeeLeaveSearchModel : BaseSearchModel
    {
        public List<string> EmployeeIds { get; set; }
        //public bool Flag { get; set; }
        public List<string> StatusIds { get; set; }
        public List<string> LeaveCategoryId { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string Flag { get; set; }
        public string LeaveMonthStr { get; set; }
        public int Years { get; set; }
        public string SearchText { get; set; }
        //public string PrimaryEmailAddress { get; set; }

    }
    public record EmployeeLeaveListModel : BasePagedListModel<EmployeeLeaveModel>
    {
        // public bool editing { get; set; }
    }
}