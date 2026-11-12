using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Vsky.Models.Analytics;

namespace Vsky.Services.Analytics
{
    /// <summary>
    /// Every rule of the Attendance Register (TimeInTimeOut + Movement Register) in one place, as pure functions:
    /// which day a time-in counts for, when a time-out is taken as forgotten, how the worked minutes are made,
    /// when an arrival is late, how a Movement Register entry is read, and how a day is classified. Nothing here
    /// touches the database. The working-day basics (shift windows, expected hours, weekly off, which leaves
    /// count, the longest period) are the Timesheet Register's and are read from <see cref="TimesheetRegisterRules"/>.
    /// </summary>
    public static class AttendanceRegisterRules
    {
        /// <summary>Allowed after the shift start before the first time-in counts as late.</summary>
        public static readonly TimeSpan LateGrace = TimeSpan.FromMinutes(15);

        /// <summary>
        /// A time-in still open after this long, or a session that lasted this long, is a forgotten time-out: the
        /// day counts as attended but its hours are not counted.
        /// </summary>
        public static readonly TimeSpan ForgottenAfter = TimeSpan.FromHours(16);

        /// <summary>
        /// The attendance day runs from this clock time to the same time the next morning, so the time-ins a night
        /// shift makes after midnight stay on the day the shift began.
        /// </summary>
        public static readonly TimeSpan DayStartsAt = TimeSpan.FromHours(5);

        /// <summary>The "Type" dropdown values of MovementRegisterDetails.</summary>
        public const string BreakType = "Break";
        public const string TimeAdjustmentType = "Time Adjustment";
        public const string WorkFromHomeType = "Work From Home";

        /// <summary>How long a Work From Home entry covers.</summary>
        public const string WfhFullDay = "Full Day";
        public const string WfhFirstHalf = "1st Half";
        public const string WfhSecondHalf = "2nd Half";
        /// <summary>Older entries carry typed minutes instead of a duration: fewer than a full day.</summary>
        public const string WfhPartDay = "Part Day";
        /// <summary>Older entries with neither a duration nor minutes.</summary>
        public const string WfhUnspecified = "Unspecified";

        /// <summary>EmployeeLeave.HalfDayType of a half-day leave that leaves the morning untouched.</summary>
        public const string SecondHalfLeaveKind = "2nd Half";

        public static int FullDayMinutes => (int)(TimesheetRegisterRules.FullDayHours * 60m);

        public static int HalfDayMinutes => (int)(TimesheetRegisterRules.HalfDayHours * 60m);

        #region Time-in / time-out
        /// <summary>Clock times are compared and shown to the minute, so seconds never decide a verdict.</summary>
        public static DateTime TruncateToMinute(DateTime value)
        {
            return value.AddTicks(-(value.Ticks % TimeSpan.TicksPerMinute));
        }

        /// <summary>The day a time-in counts for: its own date, or the day before when it falls before <see cref="DayStartsAt"/>.</summary>
        public static DateTime AttendanceDateOf(DateTime timeInLocal)
        {
            return timeInLocal.Subtract(DayStartsAt).Date;
        }

        /// <summary>
        /// Closed when timed out within <see cref="ForgottenAfter"/>; live while still open within it; otherwise
        /// the time-out is missing (never made) or too long after the time-in (or before it) to be believed.
        /// </summary>
        public static AttendanceSessionStatus StatusOf(AttendanceSession session, DateTime nowLocal)
        {
            if (session.Out == null)
                return nowLocal - session.In < ForgottenAfter ? AttendanceSessionStatus.Live : AttendanceSessionStatus.NoTimeOut;

            var length = session.Out.Value - session.In;
            return length < TimeSpan.Zero || length >= ForgottenAfter ? AttendanceSessionStatus.TooLong : AttendanceSessionStatus.Closed;
        }

        /// <summary>Minutes a session counts for: in to out when closed, in to now while live, nothing otherwise.</summary>
        public static int MinutesOf(AttendanceSession session, DateTime nowLocal)
        {
            switch (StatusOf(session, nowLocal))
            {
                case AttendanceSessionStatus.Closed:
                    return (int)(session.Out.Value - session.In).TotalMinutes;
                case AttendanceSessionStatus.Live:
                    return Math.Max(0, (int)(TruncateToMinute(nowLocal) - session.In).TotalMinutes);
                default:
                    return 0;
            }
        }
        #endregion

        #region Expected hours and arrival
        /// <summary>480 on a plain working day, 240 under a half-day leave, 0 under a full-day leave.</summary>
        public static int ExpectedMinutes(RegisterLeave leave)
        {
            return (int)(TimesheetRegisterRules.ExpectedHours(leave) * 60m);
        }

        public static DateTime ShiftStartOn(DateTime date, ShiftWindow shift)
        {
            return date.Date.Add(shift.Start);
        }

        /// <summary>The moment after which a first time-in is late: shift start + grace, site-local.</summary>
        public static DateTime LateAfter(DateTime date, ShiftWindow shift)
        {
            return ShiftStartOn(date, shift).Add(LateGrace);
        }

        /// <summary>
        /// The arrival is judged against the shift start unless a half-day leave may cover the start of the day:
        /// only a leave on the second half leaves the morning arrival as it was.
        /// </summary>
        public static bool IsArrivalJudged(RegisterLeave leave)
        {
            return leave == null || Normalize(leave.Kind) == Normalize(SecondHalfLeaveKind);
        }

        /// <summary>Minutes from the shift start to the first time-in when that is beyond the grace; otherwise 0.</summary>
        public static int LateMinutes(DateTime firstTimeIn, DateTime date, ShiftWindow shift)
        {
            return firstTimeIn > LateAfter(date, shift)
                ? (int)(firstTimeIn - ShiftStartOn(date, shift)).TotalMinutes
                : 0;
        }
        #endregion

        #region Movement Register
        public static AttendanceMovementKind KindOf(string typeName)
        {
            var key = Normalize(typeName);
            if (key == Normalize(BreakType)) return AttendanceMovementKind.Break;
            if (key == Normalize(TimeAdjustmentType)) return AttendanceMovementKind.TimeAdjustment;
            if (key == Normalize(WorkFromHomeType)) return AttendanceMovementKind.WorkFromHome;
            return AttendanceMovementKind.Other;
        }

        /// <summary>
        /// How long a Work From Home entry covers: from its "WFH Duration" (Full Day, 1st Half, 2nd Half) when it
        /// has one; older entries only carry typed minutes, read as a full day from 8 hours up.
        /// </summary>
        public static string WfhLength(string durationText, int minutes)
        {
            var text = Normalize(durationText);
            if (text != null)
            {
                if (text.Contains("full")) return WfhFullDay;
                if (text.StartsWith("1") || text.Contains("first")) return WfhFirstHalf;
                if (text.StartsWith("2") || text.Contains("second")) return WfhSecondHalf;
            }

            if (minutes >= FullDayMinutes) return WfhFullDay;
            return minutes > 0 ? WfhPartDay : WfhUnspecified;
        }

        /// <summary>Two Work From Home entries on one day: a full day wins, two different halves make a full day, else the longer one.</summary>
        public static string CombineWfh(string first, string second)
        {
            if (first == null) return second;
            if (second == null) return first;
            if (first == WfhFullDay || second == WfhFullDay) return WfhFullDay;
            if ((first == WfhFirstHalf && second == WfhSecondHalf) || (first == WfhSecondHalf && second == WfhFirstHalf)) return WfhFullDay;
            return WfhRank(first) >= WfhRank(second) ? first : second;
        }

        /// <summary>Adds one Movement Register entry to the day's totals.</summary>
        public static void Add(AttendanceMovement day, string typeName, int minutes, string wfhDurationText)
        {
            switch (KindOf(typeName))
            {
                case AttendanceMovementKind.Break:
                    day.Breaks++;
                    day.BreakMinutes += Math.Max(0, minutes);
                    break;
                case AttendanceMovementKind.TimeAdjustment:
                    day.Adjustments++;
                    day.AdjustMinutes += Math.Max(0, minutes);
                    break;
                case AttendanceMovementKind.WorkFromHome:
                    day.Wfh = CombineWfh(day.Wfh, WfhLength(wfhDurationText, minutes));
                    break;
            }
        }

        private static int WfhRank(string length)
        {
            switch (length)
            {
                case WfhFullDay: return 4;
                case WfhFirstHalf:
                case WfhSecondHalf: return 3;
                case WfhPartDay: return 2;
                default: return 1;
            }
        }
        #endregion

        #region Classification
        /// <summary>
        /// Classifies a working day (holidays, full-day leave, employment bounds and future days are decided
        /// before this is called). With a time-in: still in → InNow; a time-out missing → NoTimeOut; fewer worked
        /// minutes than expected → Short; a late first time-in → Late; otherwise OnTime. Without one: a Work From
        /// Home entry → Wfh; the shift start plus grace still ahead → Awaited; otherwise Absent.
        /// </summary>
        public static AttendanceCellState Classify(
            bool timedIn,
            bool live,
            bool missingTimeOut,
            int workedMinutes,
            int expectedMinutes,
            int lateMinutes,
            bool workFromHome,
            DateTime lateAfter,
            DateTime nowLocal)
        {
            if (timedIn)
            {
                if (live) return AttendanceCellState.InNow;
                if (missingTimeOut) return AttendanceCellState.NoTimeOut;
                if (workedMinutes < expectedMinutes) return AttendanceCellState.Short;
                return lateMinutes > 0 ? AttendanceCellState.Late : AttendanceCellState.OnTime;
            }

            if (workFromHome) return AttendanceCellState.Wfh;

            return nowLocal <= lateAfter ? AttendanceCellState.Awaited : AttendanceCellState.Absent;
        }
        #endregion

        #region Describe
        /// <summary>The rules as data, echoed with every register so the page and its guide show live values.</summary>
        public static AttendanceRules Describe(string timeZoneId)
        {
            return new AttendanceRules
            {
                FullDayMinutes = FullDayMinutes,
                HalfDayMinutes = HalfDayMinutes,
                LateGraceMinutes = (int)LateGrace.TotalMinutes,
                ForgottenAfterMinutes = (int)ForgottenAfter.TotalMinutes,
                DayStartsAt = ClockText(DayStartsAt),
                DefaultShift = TimesheetRegisterRules.DefaultShiftName,
                WeeklyOff = TimesheetRegisterRules.WeeklyOff.ToString(),
                TimeZone = timeZoneId,
                MaxRangeDays = TimesheetRegisterRules.MaxRangeDays,
                Shifts = TimesheetRegisterRules.Shifts.Select(s => new AttendanceShiftRule
                {
                    Name = s.Name,
                    Start = ClockText(s.Start),
                    End = ClockText(s.End),
                    EndsNextDay = s.EndsNextDay,
                    LateAfter = ClockText(s.Start.Add(LateGrace))
                }).ToList(),
                LeaveStatusesIgnored = TimesheetRegisterRules.LeaveStatusesIgnored.ToList(),
                MovementTypes = new List<string> { BreakType, TimeAdjustmentType, WorkFromHomeType }
            };
        }
        #endregion

        #region Helpers
        private static string ClockText(TimeSpan time)
        {
            return time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        }

        /// <summary>"  Work   From home " → "work from home"; null/blank → null.</summary>
        private static string Normalize(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? null : Regex.Replace(text.Trim(), @"\s+", " ").ToLowerInvariant();
        }
        #endregion
    }
}
