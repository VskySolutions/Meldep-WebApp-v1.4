using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Vsky.Models.Analytics
{
    // The Attendance Register (Analytics): TimeInTimeOut and the Movement Register (MovementRegister +
    // MovementRegisterDetails) merged into one employee-by-day view. It shares RegisterDay, RegisterLeave,
    // RegisterEmployee and ShiftWindow with the Timesheet Register (TimesheetRegisterResult.cs).
    //
    // Every date and time here is site-local text and every duration is whole minutes, so nothing passes through
    // the API's DateTime/TimeSpan JSON converters (they shift values to the site zone a second time).

    /// <summary>
    /// What the Attendance Register is asked for. FromDate/ToDate are site-local calendar dates (time ignored).
    /// </summary>
    public class AttendanceRegisterQuery
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

    /// <summary>The attendance verdict of one employee on one day. Serialised by name.</summary>
    public enum AttendanceCellState
    {
        /// <summary>Sunday or a LeaveSchedule date of the site. Nobody is expected.</summary>
        Holiday,
        /// <summary>Before the joining date or after the release date. Nobody is expected.</summary>
        NotEmployed,
        /// <summary>A full-day leave (applied, forwarded or approved) covers the date.</summary>
        Leave,
        /// <summary>After today.</summary>
        Future,
        /// <summary>Today, no time-in yet, and the shift start plus grace has not passed.</summary>
        Awaited,
        /// <summary>A working day with no time-in and no Work From Home entry.</summary>
        Absent,
        /// <summary>A Work From Home entry covers the day and there is no time-in.</summary>
        Wfh,
        /// <summary>Timed in and not out yet, within the hours a working day can plausibly last.</summary>
        InNow,
        /// <summary>A time-in was never closed, or was closed implausibly late. The hours are not counted.</summary>
        NoTimeOut,
        /// <summary>Timed in and out, but worked less than the expected hours.</summary>
        Short,
        /// <summary>The expected hours were worked, but the first time-in came after the shift start plus grace.</summary>
        Late,
        /// <summary>Timed in by the shift start plus grace and worked the expected hours.</summary>
        OnTime
    }

    /// <summary>How one TimeInTimeOut row ended.</summary>
    public enum AttendanceSessionStatus
    {
        /// <summary>Timed in and out.</summary>
        Closed,
        /// <summary>Timed in, not out yet, and still within the hours a working day can last.</summary>
        Live,
        /// <summary>Timed in and never timed out.</summary>
        NoTimeOut,
        /// <summary>Timed out so long after the time-in that the time-out was plainly forgotten.</summary>
        TooLong
    }

    /// <summary>What a Movement Register entry is, from its "Type" dropdown value.</summary>
    public enum AttendanceMovementKind
    {
        Other,
        Break,
        TimeAdjustment,
        WorkFromHome
    }

    /// <summary>One TimeInTimeOut row reduced to what the rules need: in and out in site-local time, to the minute.</summary>
    public class AttendanceSession
    {
        public DateTime In { get; set; }
        /// <summary>Null when the employee never timed out.</summary>
        public DateTime? Out { get; set; }
    }

    /// <summary>The Movement Register entries of one employee on one day, added up.</summary>
    public class AttendanceMovement
    {
        public int Breaks { get; set; }
        public int BreakMinutes { get; set; }
        public int Adjustments { get; set; }
        public int AdjustMinutes { get; set; }
        /// <summary>Length of the Work From Home entry ("Full Day", "1st Half", ...); null when there is none.</summary>
        public string Wfh { get; set; }
    }

    /// <summary>
    /// One employee-day. Kept lean on purpose: the matrix is the landing payload, so zero values are not
    /// serialised, the date comes from the day column, and the shift is sent only when it differs from the row.
    /// </summary>
    public class AttendanceCell
    {
        /// <summary>AttendanceCellState by name.</summary>
        public string State { get; set; }
        /// <summary>Minutes the employee was expected to work that day (480, 240 under a half-day leave, 0 otherwise).</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Expected { get; set; }
        /// <summary>Time-ins recorded for the day.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Sessions { get; set; }
        /// <summary>Site-local time of the first time-in (yyyy-MM-ddTHH:mm).</summary>
        public string FirstIn { get; set; }
        /// <summary>Site-local time of the last time-out; null while timed in or when a time-out is missing.</summary>
        public string LastOut { get; set; }
        /// <summary>Minutes between time-in and time-out over the day's sessions (up to now for a live one). 0 when a time-out is missing.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Clocked { get; set; }
        /// <summary>The productive minutes: clocked minus the Break and Time Adjustment minutes of the Movement Register, never below 0.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Worked { get; set; }
        /// <summary>Minutes between the shift start and the first time-in, when that is beyond the grace.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int LateMinutes { get; set; }
        /// <summary>Break entries of the Movement Register for the day, and their minutes.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Breaks { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int BreakMinutes { get; set; }
        /// <summary>Time Adjustment entries of the Movement Register for the day, and their minutes.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Adjustments { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int AdjustMinutes { get; set; }
        /// <summary>Length of the day's Work From Home entry ("Full Day", "1st Half", "2nd Half", ...); null = none.</summary>
        public string Wfh { get; set; }
        /// <summary>Shift the day was judged with; null = the shift on the row.</summary>
        public string Shift { get; set; }
        /// <summary>Null = same as the row.</summary>
        public bool? ShiftAssigned { get; set; }
        public RegisterLeave Leave { get; set; }
    }

    /// <summary>Counts over a set of employee-days: one employee's period, or the whole register.</summary>
    public class AttendanceRowTotals
    {
        /// <summary>Working days on which the employee was expected (past days and today; holidays and full-day leaves out).</summary>
        public int ExpectedDays { get; set; }
        public int ExpectedMinutes { get; set; }
        public int ClockedMinutes { get; set; }
        /// <summary>Expected minutes of the days that can be judged: ExpectedMinutes without the days still open (in now, not in yet).</summary>
        public int RequiredMinutes { get; set; }
        /// <summary>Productive minutes (clocked minus breaks and time adjustments) over every day, hours on a holiday or leave day included.</summary>
        public int WorkedMinutes { get; set; }
        /// <summary>Required minus productive when productive is lower. Struck per employee over the period; on the summary, the sum of the employees' shortfalls.</summary>
        public int ShortMinutes { get; set; }
        /// <summary>Productive minus required when productive is higher; summed like ShortMinutes.</summary>
        public int ExtraMinutes { get; set; }
        /// <summary>Expected days with a time-in, whatever the verdict.</summary>
        public int Present { get; set; }
        public int OnTime { get; set; }
        /// <summary>Days whose first time-in came after the shift start plus grace, whatever else happened that day.</summary>
        public int Late { get; set; }
        /// <summary>Late days with no Time Adjustment entry in the Movement Register.</summary>
        public int LateUndeclared { get; set; }
        public int LateMinutes { get; set; }
        public int Short { get; set; }
        public int NoTimeOut { get; set; }
        public int Absent { get; set; }
        public int InNow { get; set; }
        public int Awaited { get; set; }
        /// <summary>Days with a Work From Home entry, with or without a time-in.</summary>
        public int Wfh { get; set; }
        /// <summary>Days with a Work From Home entry and no time-in.</summary>
        public int WfhNoTimeIn { get; set; }
        public int Leave { get; set; }
        public int HalfDay { get; set; }
        public int Holiday { get; set; }
        public int Breaks { get; set; }
        public int BreakMinutes { get; set; }
        public int Adjustments { get; set; }
        public int AdjustMinutes { get; set; }
    }

    public class AttendanceRow : RegisterEmployee
    {
        [JsonPropertyOrder(1)]
        public List<AttendanceCell> Cells { get; set; } = new List<AttendanceCell>();
        [JsonPropertyOrder(2)]
        public AttendanceRowTotals Totals { get; set; } = new AttendanceRowTotals();
    }

    public class AttendanceDayTotal
    {
        public string Date { get; set; }
        public int Expected { get; set; }
        /// <summary>Expected employees with a time-in.</summary>
        public int Present { get; set; }
        /// <summary>Expected employees on a Work From Home entry without a time-in.</summary>
        public int Wfh { get; set; }
        public int Absent { get; set; }
        public int Awaited { get; set; }
        public int OnTime { get; set; }
        public int Late { get; set; }
        public int Short { get; set; }
        public int NoTimeOut { get; set; }
        public int InNow { get; set; }
        public int Leave { get; set; }
    }

    public class AttendanceSummary : AttendanceRowTotals
    {
        public int Employees { get; set; }
        public int EmployeesWithoutShift { get; set; }
        /// <summary>Employees whose productive hours are below the required hours of the period.</summary>
        public int EmployeesShort { get; set; }
        public List<AttendanceDayTotal> PerDay { get; set; } = new List<AttendanceDayTotal>();
    }

    public class AttendanceShiftRule
    {
        public string Name { get; set; }
        /// <summary>HH:mm</summary>
        public string Start { get; set; }
        /// <summary>HH:mm</summary>
        public string End { get; set; }
        public bool EndsNextDay { get; set; }
        /// <summary>HH:mm = shift start + grace; a first time-in after it is late.</summary>
        public string LateAfter { get; set; }
    }

    /// <summary>The rules the register was built with, echoed so the page and its guide show live values.</summary>
    public class AttendanceRules
    {
        public int FullDayMinutes { get; set; }
        public int HalfDayMinutes { get; set; }
        public int LateGraceMinutes { get; set; }
        /// <summary>A time-in open (or a session lasting) longer than this is a forgotten time-out.</summary>
        public int ForgottenAfterMinutes { get; set; }
        /// <summary>HH:mm; a time-in before it counts for the previous day (the tail of a night shift).</summary>
        public string DayStartsAt { get; set; }
        public string DefaultShift { get; set; }
        public string WeeklyOff { get; set; }
        public string TimeZone { get; set; }
        public int MaxRangeDays { get; set; }
        public List<AttendanceShiftRule> Shifts { get; set; } = new List<AttendanceShiftRule>();
        public List<string> LeaveStatusesIgnored { get; set; } = new List<string>();
        /// <summary>The Movement Register "Type" values the register understands.</summary>
        public List<string> MovementTypes { get; set; } = new List<string>();
    }

    public class AttendanceRegisterResult
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        /// <summary>Today in the site time zone (yyyy-MM-dd).</summary>
        public string Today { get; set; }
        /// <summary>Site-local timestamp the register was computed at (yyyy-MM-ddTHH:mm).</summary>
        public string GeneratedAt { get; set; }
        public List<RegisterDay> Days { get; set; } = new List<RegisterDay>();
        public List<AttendanceRow> Rows { get; set; } = new List<AttendanceRow>();
        public AttendanceSummary Summary { get; set; } = new AttendanceSummary();
        public AttendanceRules Rules { get; set; } = new AttendanceRules();
    }

    /// <summary>One TimeInTimeOut row behind a cell.</summary>
    public class AttendanceSessionLine
    {
        /// <summary>The attendance day the session counts for (yyyy-MM-dd).</summary>
        public string Date { get; set; }
        /// <summary>Site-local yyyy-MM-ddTHH:mm</summary>
        public string In { get; set; }
        /// <summary>Site-local yyyy-MM-ddTHH:mm; null when the employee never timed out.</summary>
        public string Out { get; set; }
        /// <summary>Minutes counted for the session: in to out, in to now while live, 0 when the time-out is missing.</summary>
        public int Minutes { get; set; }
        /// <summary>AttendanceSessionStatus by name.</summary>
        public string Status { get; set; }
    }

    /// <summary>One MovementRegisterDetails row behind a cell.</summary>
    public class AttendanceMovementLine
    {
        /// <summary>The register date the entry was filed under (yyyy-MM-dd).</summary>
        public string Date { get; set; }
        /// <summary>The "Type" dropdown value as stored ("Break", "Time Adjustment", "Work From Home").</summary>
        public string Type { get; set; }
        /// <summary>AttendanceMovementKind by name.</summary>
        public string Kind { get; set; }
        public int Minutes { get; set; }
        /// <summary>The length as picked: "15 mins" for a break or adjustment, "Full Day" / "1st Half" / "2nd Half" for Work From Home.</summary>
        public string Duration { get; set; }
        public string Message { get; set; }
        /// <summary>"Permission by": the approver named on the entry. There is no approval status on these records.</summary>
        public string Approver { get; set; }
        public bool NotifyToStakeholders { get; set; }
        /// <summary>Site-local yyyy-MM-ddTHH:mm</summary>
        public string EnteredAt { get; set; }
        public string EnteredBy { get; set; }
        /// <summary>Days between the register date and the day the entry was made: above 0 = entered afterwards, below 0 = entered ahead.</summary>
        public int EnteredDaysLater { get; set; }
    }

    /// <summary>One employee on one day, with the time-ins and the Movement Register entries behind the cell.</summary>
    public class AttendanceRegisterDayDetail
    {
        public AttendanceRow Employee { get; set; }
        public RegisterDay Day { get; set; }
        public AttendanceCell Cell { get; set; }
        public AttendanceRules Rules { get; set; }
        public List<AttendanceSessionLine> Sessions { get; set; } = new List<AttendanceSessionLine>();
        public List<AttendanceMovementLine> Movements { get; set; } = new List<AttendanceMovementLine>();
    }

    /// <summary>One employee over a range: the register row with its cells, the days, and every record behind them.</summary>
    public class AttendanceRegisterEmployeeDetail
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public AttendanceRow Employee { get; set; }
        public List<RegisterDay> Days { get; set; } = new List<RegisterDay>();
        public AttendanceRules Rules { get; set; }
        public List<AttendanceSessionLine> Sessions { get; set; } = new List<AttendanceSessionLine>();
        public List<AttendanceMovementLine> Movements { get; set; } = new List<AttendanceMovementLine>();
    }
}
