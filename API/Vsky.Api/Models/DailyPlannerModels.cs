using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record DailyPlannerModel : BaseEntityModel
    {
        public string EmployeeId { get; set; }
        public int[] Projects { get; set; }
        public string SiteId { get; set; }
        public DateTime? DailyPlannerDate { get; set; }
        public bool IsForwordedToTimesheet { get; set; }
        public decimal TotalHours { get; set; }
        public virtual Site Sites { get; set; }
        public virtual EmployeeModel Employee { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<DailyPlannerLineModel> DailyPlannerLineModel { get; set; } = new List<DailyPlannerLineModel>();
        public virtual ICollection<DailyPlannerLineModel> DailyPlannerLines { get; set; } = new List<DailyPlannerLineModel>();

    }

    public record DailyPlannerSearchModel : BaseSearchModel
    {
        public string CreatedBy { get; set; }
        public string EmployeeId { get; set; }
        public string ProjectId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ActivityDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchText { get; set; }
    }

    public record DailyPlannerListModel : BasePagedListModel<DailyPlannerModel>
    {
    }

    public record DailyPlannerUploadModel : BaseEntityModel
    {
    }
}
