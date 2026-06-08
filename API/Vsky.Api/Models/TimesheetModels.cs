using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vsky.Api.Models
{
    public record TimesheetModel : BaseEntityModel
    {
        public string SiteId { get; set; }

        public string EmployeeId { get; set; }
        public int[] Projects { get; set; }

        public DateTime TimesheetDate { get; set; }

        public string CreatedById { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string UpdatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public bool Deleted { get; set; }

        public virtual Site Sites { get; set; }

        public bool IsActionVisible { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<TimesheetLinesModel> TimesheetLineModel { get; set; } = new List<TimesheetLinesModel>();
        public virtual ICollection<TimesheetLinesModel> TimesheetLines { get; set; } = new List<TimesheetLinesModel>();

        public virtual ICollection<TimesheetModel> TimesheetDataModel { get; set; } = new List<TimesheetModel>();
        public List<ColumnModel> Columns { get; set; } = new List<ColumnModel>();
    }

    public record TimesheetSearchModel : BaseSearchModel
    {
        public string CreatedBy { get; set; }
        public string EmployeeId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectModuleId { get; set; }
        public List<string> ProjectModuleIds { get; set; }
        public string ProjectTaskId { get; set; }
        public List<string> ProjectTaskIds { get; set; }
        public string ProjectActivityId { get; set; }
        public List<string> CustomerIds { get; set; }
        public List<string> CompanyContactIds { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ActivityDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchText { get; set; }
        public string ViewType { get; set; }
        public bool ThisWeek { get; set; }
        public int LastNumberOfWeeks { get; set; }
        public bool IsActionDisabled { get; set; }
    }

    public record TimesheetListModel : BasePagedListModel<TimesheetModel>
    {
        public bool editing { get; set; }
    }

    public record TimesheetUploadModel : BaseEntityModel
    {
    }
    public class ColumnModel
    {
        public string Type { get; set; } // Maps to `name`
        public string Name { get; set; } // Maps to `name`
        public string Label { get; set; } // Maps to `label`
        public string Field { get; set; } // Maps to `field`
        public string Align { get; set; } // Maps to `align`
        public bool Sortable { get; set; } // Maps to `sortable`
        public bool CheckedStatus { get; set; } // Maps to `checkedStatus`
    }
    public class TimesheetWeeklyMonthlyHoursModel
    {
        public List<TimesheetWeeklyMonthlyColumnModel> Columns { get; set; }
        public List<Dictionary<string, object>> Rows { get; set; }
        public string ViewType { get; set; }
    }
    public class TimesheetWeeklyMonthlyColumnModel
    {
        public int Index { get; set; }
        public DateTime Date { get; set; }
        public string Day { get; set; }
        public string DateTooltip { get; set; }
        public string DisplayDateRange { get; set; }
    }
}
