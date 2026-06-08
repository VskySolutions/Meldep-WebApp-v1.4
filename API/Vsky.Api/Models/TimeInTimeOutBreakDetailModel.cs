using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record TimeInTimeOutBreakDetailModel : BaseEntityModel
    {
        public string TimeInTimeOutId { get; set; }
        public string MovementRegisterId { get; set; }
        //public string SiteId { get; set; }
        public TimeSpan BreakIn { get; set; }
        public TimeSpan BreakOut { get; set; }
        public TimeSpan TotalBreak { get; set; }
        public string BreakReason { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public string BreakInStr { get; set; }
        public string BreakOutStr { get; set; }
        public string Flag { get; set; }
        public string ApproverById { get; set; }
        public string TypeId { get; set; }
        public bool NotifyToStakeholders { get; set; }
        public string Type { get; set; }
        //public virtual Site Sites { get; set; }
        public virtual TimeInTimeOut TimeInTimeOut { get; set; }
    }

    public record TimeInTimeOutBreakDetailSearchModel : BaseSearchModel
    {
    }

    public record TimeInTimeOutBreakDetailListModel : BasePagedListModel<TimeInTimeOutBreakDetailModel>
    {
        public bool editing { get; set; }
    }
    public record TimeInTimeOutBreakDetailUploadModel : BaseEntityModel
    {
    }
}
