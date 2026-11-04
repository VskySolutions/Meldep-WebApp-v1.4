using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Vsky.Models.Analytics
{
    /// <summary>
    /// What the Timesheet Register is asked for. FromDate/ToDate are site-local calendar dates (time ignored).
    /// </summary>
    public class TimesheetRegisterQuery
    {
        public string SiteId { get; set; }

        /// <summary>Windows time-zone id of the site (e.g. "India Standard Time"). Falls back to IST when empty.</summary>
        public string TimeZoneId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        /// <summary>Optional: restrict to these employees. Empty = every active employee of the site.</summary>
        public List<string> EmployeeIds { get; set; }

        /// <summary>Overrides "now" (UTC) so a run can be reproduced; null = DateTime.UtcNow.</summary>
        public DateTime? NowUtc { get; set; }
    }

    /// <summary>The state of one employee on one calendar day. Serialised by name.</summary>
    public enum RegisterCellState
    {
        /// <summary>Sunday or a LeaveSchedule date of the site. Nothing expected.</summary>
        Holiday,
        /// <summary>Before the joining date or after the release date. Nothing expected.</summary>
        NotEmployed,
        /// <summary>A full-day leave (applied, forwarded or approved) covers the date. Nothing expected.</summary>
        Leave,
        /// <summary>After today.</summary>
        Future,
        /// <summary>Expected hours not reached yet and the cutoff has not passed.</summary>
        Due,
        /// <summary>Nothing logged and the cutoff has passed.</summary>
        Pending,
        /// <summary>Some hours logged, below the expected hours, and the cutoff has passed.</summary>
        Partial,
        /// <summary>Expected hours reached, but only after the cutoff.</summary>
        Delayed,
        /// <summary>Expected hours reached by the cutoff.</summary>
        OnTime
    }

    /// <summary>A working shift: start/end clock times; End may fall on the next calendar day.</summary>
    public class ShiftWindow
    {
        public ShiftWindow(string name, TimeSpan start, TimeSpan end, bool endsNextDay)
        {
            Name = name;
            Start = start;
            End = end;
            EndsNextDay = endsNextDay;
        }

        public string Name { get; }
        public TimeSpan Start { get; }
        public TimeSpan End { get; }
        public bool EndsNextDay { get; }
    }

    /// <summary>One timesheet line reduced to what the rules need: hours and when they were entered (site-local).</summary>
    public class RegisterEntry
    {
        public decimal Hours { get; set; }
        public DateTime EnteredAtLocal { get; set; }
    }

    public class RegisterDay
    {
        /// <summary>yyyy-MM-dd</summary>
        public string Date { get; set; }
        /// <summary>Mon, Tue, ...</summary>
        public string Day { get; set; }
        public int DayNumber { get; set; }
        /// <summary>Jan, Feb, ...</summary>
        public string Month { get; set; }
        public bool IsWeeklyOff { get; set; }
        public bool IsHoliday { get; set; }
        public string HolidayTitle { get; set; }
        public bool IsToday { get; set; }
        public bool IsFuture { get; set; }
    }

    public class RegisterLeave
    {
        /// <summary>"Full", "1st Half" or "2nd Half".</summary>
        public string Kind { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public bool IsApproved { get; set; }
    }

    /// <summary>
    /// One employee-day. Kept lean on purpose: the matrix is the landing payload, so zero/false values are not
    /// serialised, the date comes from the day column, and the shift is sent only when it differs from the row.
    /// </summary>
    public class RegisterCell
    {
        /// <summary>RegisterCellState by name.</summary>
        public string State { get; set; }
        /// <summary>Hours the employee was expected to log that day (8, 4 on a half-day leave, 0 otherwise).</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public decimal Expected { get; set; }
        /// <summary>Hours actually logged (all non-deleted lines of that date).</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public decimal Logged { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Lines { get; set; }
        /// <summary>Shift the day was judged with; null = the shift on the row.</summary>
        public string Shift { get; set; }
        /// <summary>Null = same as the row.</summary>
        public bool? ShiftAssigned { get; set; }
        /// <summary>Site-local cutoff (yyyy-MM-ddTHH:mm) = shift end + grace. Null when nothing was expected.</summary>
        public string Cutoff { get; set; }
        /// <summary>Site-local time of the first line entered for the day.</summary>
        public string FirstEntryAt { get; set; }
        /// <summary>Site-local time at which the cumulative hours reached the expected hours.</summary>
        public string CompletedAt { get; set; }
        /// <summary>Minutes between the cutoff and CompletedAt when Delayed.</summary>
        public int? LateMinutes { get; set; }
        /// <summary>The hours were completed before the day had started.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool PreFilled { get; set; }
        public RegisterLeave Leave { get; set; }
        /// <summary>Weekly approval status of the day's timesheet header (Submitted, Approved, ...), when any.</summary>
        public string TimesheetStatus { get; set; }
    }

    public class RegisterRowTotals
    {
        public decimal LoggedHours { get; set; }
        public decimal ExpectedHours { get; set; }
        public int ExpectedDays { get; set; }
        public int OnTime { get; set; }
        public int Delayed { get; set; }
        public int Partial { get; set; }
        public int Pending { get; set; }
        public int Due { get; set; }
        public int Leave { get; set; }
        public int HalfDay { get; set; }
        public int Holiday { get; set; }
        public int LateMinutes { get; set; }
    }

    public class RegisterRow
    {
        public string EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        /// <summary>Shift used for the last day of the range (resolved; the default when none is assigned).</summary>
        public string Shift { get; set; }
        public bool ShiftAssigned { get; set; }
        /// <summary>The raw shift value on the designation record, when any.</summary>
        public string ShiftSource { get; set; }
        public string JoiningDate { get; set; }
        public string ReleaseDate { get; set; }
        public List<RegisterCell> Cells { get; set; } = new List<RegisterCell>();
        public RegisterRowTotals Totals { get; set; } = new RegisterRowTotals();
    }

    public class RegisterDayTotal
    {
        public string Date { get; set; }
        public int Expected { get; set; }
        /// <summary>OnTime + Delayed.</summary>
        public int Complete { get; set; }
        public int OnTime { get; set; }
        public int Delayed { get; set; }
        public int Partial { get; set; }
        public int Pending { get; set; }
        public int Due { get; set; }
        public int Leave { get; set; }
    }

    public class RegisterSummary
    {
        public int Employees { get; set; }
        public int EmployeesWithoutShift { get; set; }
        public int ExpectedDays { get; set; }
        public int OnTime { get; set; }
        public int Delayed { get; set; }
        public int Partial { get; set; }
        public int Pending { get; set; }
        public int Due { get; set; }
        public int LeaveDays { get; set; }
        public int HalfDays { get; set; }
        public decimal LoggedHours { get; set; }
        public decimal ExpectedHours { get; set; }
        public List<RegisterDayTotal> PerDay { get; set; } = new List<RegisterDayTotal>();
    }

    public class RegisterShiftRule
    {
        public string Name { get; set; }
        /// <summary>HH:mm</summary>
        public string Start { get; set; }
        /// <summary>HH:mm</summary>
        public string End { get; set; }
        public bool EndsNextDay { get; set; }
        /// <summary>Human text, e.g. "00:00 next day".</summary>
        public string Cutoff { get; set; }
    }

    /// <summary>The rules the register was built with, echoed so the page and its guide show live values.</summary>
    public class RegisterRules
    {
        public decimal FullDayHours { get; set; }
        public decimal HalfDayHours { get; set; }
        public int GraceMinutes { get; set; }
        public string DefaultShift { get; set; }
        public string WeeklyOff { get; set; }
        public string TimeZone { get; set; }
        public int MaxRangeDays { get; set; }
        public List<RegisterShiftRule> Shifts { get; set; } = new List<RegisterShiftRule>();
        public List<string> LeaveStatusesIgnored { get; set; } = new List<string>();
    }

    public class TimesheetRegisterResult
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        /// <summary>Today in the site time zone (yyyy-MM-dd).</summary>
        public string Today { get; set; }
        /// <summary>Site-local timestamp the register was computed at (yyyy-MM-ddTHH:mm).</summary>
        public string GeneratedAt { get; set; }
        public List<RegisterDay> Days { get; set; } = new List<RegisterDay>();
        public List<RegisterRow> Rows { get; set; } = new List<RegisterRow>();
        public RegisterSummary Summary { get; set; } = new RegisterSummary();
        public RegisterRules Rules { get; set; } = new RegisterRules();
    }

    public class RegisterDayLine
    {
        public string Project { get; set; }
        public string ProjectModule { get; set; }
        public string Task { get; set; }
        public string Activity { get; set; }
        /// <summary>Plain-text excerpt of the line description.</summary>
        public string Description { get; set; }
        public decimal Hours { get; set; }
        public decimal BillableHours { get; set; }
        /// <summary>Site-local yyyy-MM-ddTHH:mm</summary>
        public string EnteredAt { get; set; }
        public bool IsApproved { get; set; }
    }

    /// <summary>One employee on one day, with the lines behind the cell.</summary>
    public class TimesheetRegisterDayDetail
    {
        public RegisterRow Employee { get; set; }
        public RegisterDay Day { get; set; }
        public RegisterCell Cell { get; set; }
        public RegisterRules Rules { get; set; }
        public List<RegisterDayLine> Lines { get; set; } = new List<RegisterDayLine>();
    }
}
