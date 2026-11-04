using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Vsky.Models.Analytics;

namespace Vsky.Services.Analytics
{
    /// <summary>
    /// Every rule of the Timesheet Register in one place, as pure functions: the shift windows and the grace
    /// that give a day its cutoff, which leave statuses count, the expected hours, and how a day is classified.
    /// Nothing here touches the database, so each rule can be read (and unit-tested) on its own.
    /// </summary>
    public static class TimesheetRegisterRules
    {
        /// <summary>Hours an employee is expected to log on a working day.</summary>
        public const decimal FullDayHours = 8m;

        /// <summary>Hours expected on a day covered by a half-day leave.</summary>
        public const decimal HalfDayHours = 4m;

        /// <summary>Allowed after the shift end before a day's timesheet counts as late.</summary>
        public static readonly TimeSpan GraceAfterShiftEnd = TimeSpan.FromHours(4);

        /// <summary>Shift used for an employee whose designation carries no (recognised) shift.</summary>
        public const string DefaultShiftName = "Shift 1";

        /// <summary>Fixed weekly off; LeaveSchedule supplies every other holiday (including Saturdays off).</summary>
        public const DayOfWeek WeeklyOff = DayOfWeek.Sunday;

        /// <summary>Longest range one request may cover (a quarter).</summary>
        public const int MaxRangeDays = 93;

        public const string DefaultTimeZoneId = "India Standard Time";

        /// <summary>
        /// The shifts, keyed by the "Employee Shift" dropdown value on the designation record. The dropdown
        /// carries no times, so the windows live here.
        /// </summary>
        public static readonly IReadOnlyList<ShiftWindow> Shifts = new List<ShiftWindow>
        {
            new ShiftWindow("Shift 1", new TimeSpan(11, 0, 0), new TimeSpan(20, 0, 0), endsNextDay: false),
            new ShiftWindow("Shift 2", new TimeSpan(14, 0, 0), new TimeSpan(23, 0, 0), endsNextDay: false),
            new ShiftWindow("Shift 3", new TimeSpan(18, 0, 0), new TimeSpan(3, 0, 0), endsNextDay: true)
        };

        /// <summary>Leave statuses that do NOT cover a day. Everything else (Applied, Sent to Approver, Approved) does.</summary>
        public static readonly IReadOnlyList<string> LeaveStatusesIgnored = new[] { "Decline", "Declined", "Cancelled", "Canceled", "Rejected" };

        public const string FullDayLeaveKind = "Full";

        public static ShiftWindow DefaultShift => Shifts.First(s => s.Name == DefaultShiftName);

        /// <summary>"  shift   1 " → "shift 1"; null/blank → null.</summary>
        public static string NormalizeShiftName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? null : Regex.Replace(name.Trim(), @"\s+", " ").ToLowerInvariant();
        }

        /// <summary>The shift for a dropdown value; the default shift (recognised = false) when it is blank or unknown.</summary>
        public static ShiftWindow ResolveShift(string shiftName, out bool recognised)
        {
            var key = NormalizeShiftName(shiftName);
            var match = key == null ? null : Shifts.FirstOrDefault(s => NormalizeShiftName(s.Name) == key);
            recognised = match != null;
            return match ?? DefaultShift;
        }

        public static bool IsWeeklyOff(DateTime date) => date.DayOfWeek == WeeklyOff;

        /// <summary>The clock time the shift ends for work done on <paramref name="date"/> (next calendar day for a night shift).</summary>
        public static DateTime ShiftEndOn(DateTime date, ShiftWindow shift)
        {
            return date.Date.AddDays(shift.EndsNextDay ? 1 : 0).Add(shift.End);
        }

        /// <summary>The moment after which the day's timesheet counts as late: shift end + grace, site-local.</summary>
        public static DateTime CutoffFor(DateTime date, ShiftWindow shift)
        {
            return ShiftEndOn(date, shift).Add(GraceAfterShiftEnd);
        }

        /// <summary>A leave counts from the moment it is applied; only declined/cancelled ones are ignored.</summary>
        public static bool IsLeaveCounted(string status)
        {
            return string.IsNullOrWhiteSpace(status) || !LeaveStatusesIgnored.Contains(status.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsLeaveApproved(string status)
        {
            return string.Equals(status?.Trim(), "Approved", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>8 h on a plain working day, 4 h under a half-day leave, 0 under a full-day leave.</summary>
        public static decimal ExpectedHours(RegisterLeave leave)
        {
            if (leave == null) return FullDayHours;
            return leave.Kind == FullDayLeaveKind ? 0m : HalfDayHours;
        }

        /// <summary>
        /// When the running total of hours, taking the lines in the order they were entered, first reached the
        /// expected hours. Null when it never did.
        /// </summary>
        public static DateTime? CompletedAt(IEnumerable<RegisterEntry> entries, decimal expected)
        {
            if (expected <= 0 || entries == null) return null;

            decimal total = 0;
            foreach (var entry in entries.OrderBy(e => e.EnteredAtLocal))
            {
                total += entry.Hours;
                if (total >= expected) return entry.EnteredAtLocal;
            }

            return null;
        }

        /// <summary>
        /// Classifies a working day (holidays, leave and employment bounds are decided before this is called):
        /// expected reached → OnTime/Delayed by the cutoff; otherwise Due until the cutoff, then Partial or Pending.
        /// </summary>
        public static RegisterCellState Classify(decimal expected, decimal logged, DateTime? completedAt, DateTime cutoff, DateTime nowLocal)
        {
            if (expected > 0 && logged >= expected && completedAt != null)
                return completedAt.Value <= cutoff ? RegisterCellState.OnTime : RegisterCellState.Delayed;

            if (nowLocal < cutoff)
                return RegisterCellState.Due;

            return logged > 0 ? RegisterCellState.Partial : RegisterCellState.Pending;
        }

        /// <summary>"00:00 next day", "07:00 next day", "20:00 same day"...</summary>
        public static string DescribeCutoff(ShiftWindow shift)
        {
            var anchor = new DateTime(2000, 1, 3);
            var cutoff = CutoffFor(anchor, shift);
            var days = (cutoff.Date - anchor).Days;
            var time = cutoff.ToString("HH:mm", CultureInfo.InvariantCulture);

            switch (days)
            {
                case 0: return $"{time} same day";
                case 1: return $"{time} next day";
                default: return $"{time} +{days} days";
            }
        }

        /// <summary>The rules as data, echoed with every register so the page and its guide show live values.</summary>
        public static RegisterRules Describe(string timeZoneId)
        {
            return new RegisterRules
            {
                FullDayHours = FullDayHours,
                HalfDayHours = HalfDayHours,
                GraceMinutes = (int)GraceAfterShiftEnd.TotalMinutes,
                DefaultShift = DefaultShiftName,
                WeeklyOff = WeeklyOff.ToString(),
                TimeZone = timeZoneId,
                MaxRangeDays = MaxRangeDays,
                Shifts = Shifts.Select(s => new RegisterShiftRule
                {
                    Name = s.Name,
                    Start = s.Start.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                    End = s.End.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                    EndsNextDay = s.EndsNextDay,
                    Cutoff = DescribeCutoff(s)
                }).ToList(),
                LeaveStatusesIgnored = LeaveStatusesIgnored.ToList()
            };
        }
    }
}
