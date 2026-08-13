using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using System.Text.Json.Serialization;
using Vsky.Core;

namespace Vsky.Api.Models
{
    public record TimesheetLinesModel : BaseEntityModel
    {
        public string TimesheetId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectModuleId { get; set; }
        public string ProjectTaskId { get; set; }
        public string ProjectActivityId { get; set; }

        public string Description { get; set; }

        [JsonConverter(typeof(HoursConverter))]
        public decimal Hours { get; set; }

        [JsonConverter(typeof(HoursConverter))]
        public decimal BillableHours { get; set; }
        public string MeetingUId { get; set; }

        public string Flag { get; set; }
        public bool IsApproved { get; set; }
        public bool Deleted { get; set; }
        
        public string TaskId { get; set; }
        public string ActivityNameDescription { get; set; }
        public string? BillableCreatedById { get; set; }
        public DateTime? BillableCreatedOnUtc { get; set; }
        public string HoursStr { get; set; }

        public virtual Timesheet Timesheet { get; set; }
        public virtual Project Project { get; set; }
        public virtual ProjectModule ProjectModule { get; set; }
        public virtual ProjectTask Task { get; set; }
        public virtual ProjectActivity ProjectActivity { get; set; }
        
        public virtual ICollection<TimesheetLinesModel> TimesheetDataModel { get; set; } = new List<TimesheetLinesModel>();
        public List<ColumnModel> Columns { get; set; } = new List<ColumnModel>();
        public virtual ApplicationUser BillableCreatedBy { get; set; }
    }
    public class TimesheetGroupResponse
    {
        public DateTime Date { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }

        [JsonConverter(typeof(HoursConverter))]
        public decimal Hours { get; set; }
    }
    public record TimesheetLinesSearchModel : BaseSearchModel
    {
        public string ProjectId { get; set; }
    }

    public record TimesheetLinesListModel : BasePagedListModel<TimesheetLinesModel>
    {
        public bool editing { get; set; }
    }
    public record TimesheetLinesUploadModel : BaseEntityModel
    {
    }
    public class SendTaskTimmerToTimesheetModel
    {
        public string EmployeeId { get; set; }
        public string TaskId { get; set; }
        public string TaskName { get; set; }

        public string ActivityId { get; set; }
        public string ActivityName { get; set; }

        public decimal Hours { get; set; }
        public string Description { get; set; }
    }
}
