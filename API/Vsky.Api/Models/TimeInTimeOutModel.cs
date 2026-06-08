using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vsky.Api.Converter;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record TimeInTimeOutModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string EmployeeId { get; set; }
        public string WorkHoursApprovalStatusId { get; set; }
        public DateTime? TimeInDate { get; set; }

        public TimeSpan TimeIn { get; set; }

        public DateTime? TimeOutDate { get; set; }

        public TimeSpan TimeOut { get; set; }

        public TimeSpan TotalHours { get; set; }
        public TimeSpan TotalBreak { get; set; }
        public TimeSpan ActualHours { get; set; }       
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public string ShiftId { get; set; }
        public string TimeInDateStr { get; set; }
        public string TimeOutDateStr { get; set; }
        public string ActualHoursStr { get; set; }
        public string TimeInStr { get; set; }
        public TimeSpan? TimeInSpan { get; set; }
        public string TimeOutStr { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApproverById { get; set; }

        public virtual Site Site { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual DropDown WorkHoursApprovalStatus { get; set; }
        public virtual ICollection<TimeInTimeOutBreakDetailModel> TimeInTimeOutBreakDetailModel { get; set; } = new List<TimeInTimeOutBreakDetailModel>();
        public virtual ICollection<TimeInTimeOutBreakDetail> TimeInTimeOutBreakDetailList { get; set; } = new List<TimeInTimeOutBreakDetail>();
    }

    public record TimeInTimeOutSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public string CreatedBy { get; set; }
        public string EmployeeId { get; set; }     
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public record TimeInTimeOutListModel : BasePagedListModel<TimeInTimeOutModel>
    {
        public bool editing { get; set; }
    }
    public record TimeInTimeOutUploadModel : BaseEntityModel
    {
    }
}
