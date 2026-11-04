using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;
using Vsky.Models.Analytics;

namespace Vsky.Services.Analytics
{
    /// <summary>
    /// Builds the Timesheet Register from four set-based queries (holidays, employees with their designation and
    /// department history, leaves, timesheet headers with the hours and entry time of each line) and classifies
    /// every employee-day in memory with <see cref="TimesheetRegisterRules"/>. Line descriptions are never loaded
    /// for the matrix; they come only through <see cref="GetDayDetailAsync"/>.
    /// </summary>
    public class TimesheetRegisterService : ITimesheetRegisterService
    {
        private const string IsoDate = "yyyy-MM-dd";
        private const string IsoMinute = "yyyy-MM-dd'T'HH:mm";

        #region Fields
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<EmployeeLeave> _employeeLeaveRepository;
        private readonly IRepository<LeaveSchedules> _leaveScheduleRepository;
        private readonly IRepository<Timesheet> _timesheetRepository;
        private readonly IRepository<TimesheetLines> _timesheetLinesRepository;
        #endregion

        #region Ctor
        public TimesheetRegisterService(
            IRepository<Employee> employeeRepository,
            IRepository<EmployeeLeave> employeeLeaveRepository,
            IRepository<LeaveSchedules> leaveScheduleRepository,
            IRepository<Timesheet> timesheetRepository,
            IRepository<TimesheetLines> timesheetLinesRepository)
        {
            _employeeRepository = employeeRepository;
            _employeeLeaveRepository = employeeLeaveRepository;
            _leaveScheduleRepository = leaveScheduleRepository;
            _timesheetRepository = timesheetRepository;
            _timesheetLinesRepository = timesheetLinesRepository;
        }
        #endregion

        #region GetRegisterAsync
        public async Task<TimesheetRegisterResult> GetRegisterAsync(TimesheetRegisterQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (string.IsNullOrWhiteSpace(query.SiteId)) throw new ArgumentException("Site is required.");

            var from = query.FromDate.Date;
            var to = query.ToDate.Date;
            if (to < from) (from, to) = (to, from);
            var dayCount = (to - from).Days + 1;
            if (dayCount > TimesheetRegisterRules.MaxRangeDays)
                throw new ArgumentException($"The register covers at most {TimesheetRegisterRules.MaxRangeDays} days per request; narrow the date range.");
            var toExclusive = to.AddDays(1);

            var tz = ResolveTimeZone(query.TimeZoneId);
            var nowUtc = DateTime.SpecifyKind(query.NowUtc ?? DateTime.UtcNow, DateTimeKind.Utc);
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);
            var today = nowLocal.Date;
            var filterEmployees = query.EmployeeIds != null && query.EmployeeIds.Count > 0;
            var siteId = query.SiteId;

            // 1. Holidays of the site in the range (LeaveSchedule). Sundays are added per day by the rules.
            var holidayRows = await _leaveScheduleRepository.TableNoTracking
                .Where(x => !x.Deleted && x.SiteId == siteId && x.Date != null && x.Date >= from && x.Date < toExclusive)
                .Select(x => new { x.Date, x.Title })
                .ToListAsync();

            var holidays = holidayRows
                .GroupBy(x => x.Date.Value.Date)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(", ", g.Select(h => (h.Title ?? string.Empty).Trim()).Where(t => t.Length > 0).Distinct()));

            // 2. Active employees with their designation (shift) and department history. Client contacts are
            //    stored as employees too; a department record is what marks a real employee, so rows without
            //    one are left out.
            var employeeQuery = _employeeRepository.TableNoTracking
                .Where(e => !e.Deleted && e.Active && e.SiteId == siteId && e.Person != null && !e.Person.Deleted
                    && e.EmployeeDepartment.Any(d => !d.Deleted));
            if (filterEmployees)
                employeeQuery = employeeQuery.Where(e => query.EmployeeIds.Contains(e.Id));

            var employees = await employeeQuery
                .Select(e => new EmployeeSnapshot
                {
                    Id = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    FirstName = e.Person.FirstName,
                    LastName = e.Person.LastName,
                    Email = e.OfficialEmail ?? e.Person.PrimaryEmailAddress,
                    JoiningDate = e.JoiningDate,
                    ReleaseDate = e.ReleaseDate,
                    Designations = e.EmployeeDesignation
                        .Where(d => !d.Deleted)
                        .Select(d => new PeriodSnapshot
                        {
                            Value = d.Shift != null ? d.Shift.DropDownValue : null,
                            StartDate = d.StartDate,
                            EndDate = d.EndDate,
                            CreatedOnUtc = d.CreatedOnUtc
                        })
                        .ToList(),
                    Departments = e.EmployeeDepartment
                        .Where(d => !d.Deleted)
                        .Select(d => new PeriodSnapshot
                        {
                            Value = d.Department != null ? d.Department.Name : null,
                            StartDate = d.StartDate,
                            EndDate = d.EndDate,
                            CreatedOnUtc = d.CreatedOnUtc
                        })
                        .ToList()
                })
                .ToListAsync();

            employees = employees
                .OrderBy(e => e.FirstName ?? string.Empty)
                .ThenBy(e => e.LastName ?? string.Empty)
                .ToList();
            var employeeIds = new HashSet<string>(employees.Select(e => e.Id));

            // 3. Leaves overlapping the range. Any status that is not declined/cancelled covers the day.
            var leaveQuery = _employeeLeaveRepository.TableNoTracking
                .Where(l => !l.Deleted && l.FromDate < toExclusive && l.ToDate >= from);
            leaveQuery = filterEmployees
                ? leaveQuery.Where(l => query.EmployeeIds.Contains(l.EmployeeId))
                : leaveQuery.Where(l => l.Employee.SiteId == siteId);

            var leaveRows = await leaveQuery
                .Select(l => new
                {
                    l.EmployeeId,
                    l.FromDate,
                    l.ToDate,
                    l.IsHalfDay,
                    l.HalfDayType,
                    Status = l.LeaveStatuses != null ? l.LeaveStatuses.DropDownValue : null,
                    Category = l.LeaveCategories != null ? l.LeaveCategories.DropDownValue : null
                })
                .ToListAsync();

            var leaves = new Dictionary<(string EmployeeId, DateTime Date), RegisterLeave>();
            foreach (var leave in leaveRows)
            {
                if (!employeeIds.Contains(leave.EmployeeId) || !TimesheetRegisterRules.IsLeaveCounted(leave.Status))
                    continue;

                var kind = leave.IsHalfDay
                    ? (string.IsNullOrWhiteSpace(leave.HalfDayType) ? "Half day" : leave.HalfDayType.Trim())
                    : TimesheetRegisterRules.FullDayLeaveKind;
                var start = leave.FromDate.Date < from ? from : leave.FromDate.Date;
                var end = leave.ToDate.Date > to ? to : leave.ToDate.Date;

                for (var date = start; date <= end; date = date.AddDays(1))
                {
                    var candidate = new RegisterLeave
                    {
                        Kind = kind,
                        Status = leave.Status,
                        Category = leave.Category,
                        IsApproved = TimesheetRegisterRules.IsLeaveApproved(leave.Status)
                    };
                    var key = (leave.EmployeeId, date);
                    if (!leaves.TryGetValue(key, out var existing) || Outranks(candidate, existing))
                        leaves[key] = candidate;
                }
            }

            // 4. Timesheet headers of the range with the hours and entry time of every line.
            var timesheetQuery = _timesheetRepository.TableNoTracking
                .Where(t => !t.Deleted && t.SiteId == siteId && t.TimesheetDate != null && t.TimesheetDate >= from && t.TimesheetDate < toExclusive);
            if (filterEmployees)
                timesheetQuery = timesheetQuery.Where(t => query.EmployeeIds.Contains(t.EmployeeId));

            var timesheetRows = await timesheetQuery
                .Select(t => new TimesheetSnapshot
                {
                    EmployeeId = t.EmployeeId,
                    Date = t.TimesheetDate.Value,
                    Status = t.TimesheetStatus != null ? t.TimesheetStatus.DropDownValue : null,
                    Lines = t.TimesheetLines
                        .Where(l => !l.Deleted)
                        .Select(l => new LineSnapshot { Hours = l.Hours, CreatedOnUtc = l.CreatedOnUtc })
                        .ToList()
                })
                .ToListAsync();

            // A date can (rarely) have two headers for the same employee; merge their lines.
            var sheets = new Dictionary<(string EmployeeId, DateTime Date), DaySheet>();
            foreach (var sheet in timesheetRows)
            {
                if (!employeeIds.Contains(sheet.EmployeeId)) continue;

                var key = (sheet.EmployeeId, sheet.Date.Date);
                if (!sheets.TryGetValue(key, out var day))
                {
                    day = new DaySheet { Status = sheet.Status };
                    sheets[key] = day;
                }
                else if (string.IsNullOrEmpty(day.Status) && !string.IsNullOrEmpty(sheet.Status))
                {
                    day.Status = sheet.Status;
                }

                foreach (var line in sheet.Lines)
                    day.Entries.Add(new RegisterEntry { Hours = line.Hours, EnteredAtLocal = ToLocal(line.CreatedOnUtc, tz) });
            }

            // 5. Days, then one row per employee, classified cell by cell.
            var days = new List<RegisterDay>(dayCount);
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                var weeklyOff = TimesheetRegisterRules.IsWeeklyOff(date);
                var isHoliday = holidays.TryGetValue(date, out var title);
                days.Add(new RegisterDay
                {
                    Date = date.ToString(IsoDate, CultureInfo.InvariantCulture),
                    Day = date.ToString("ddd", CultureInfo.InvariantCulture),
                    DayNumber = date.Day,
                    Month = date.ToString("MMM", CultureInfo.InvariantCulture),
                    IsWeeklyOff = weeklyOff,
                    IsHoliday = weeklyOff || isHoliday,
                    HolidayTitle = !string.IsNullOrEmpty(title) ? title : (weeklyOff ? "Weekly off" : (isHoliday ? "Holiday" : null)),
                    IsToday = date == today,
                    IsFuture = date > today
                });
            }

            var result = new TimesheetRegisterResult
            {
                FromDate = from.ToString(IsoDate, CultureInfo.InvariantCulture),
                ToDate = to.ToString(IsoDate, CultureInfo.InvariantCulture),
                Today = today.ToString(IsoDate, CultureInfo.InvariantCulture),
                GeneratedAt = nowLocal.ToString(IsoMinute, CultureInfo.InvariantCulture),
                Days = days,
                Rules = TimesheetRegisterRules.Describe(tz.Id)
            };
            var summary = result.Summary;
            summary.Employees = employees.Count;
            summary.PerDay = days.Select(d => new RegisterDayTotal { Date = d.Date }).ToList();

            var shiftAsOf = to < today ? to : today;
            foreach (var employee in employees)
            {
                var shiftSource = PeriodValue(employee.Designations, shiftAsOf);
                var shift = TimesheetRegisterRules.ResolveShift(shiftSource, out var shiftAssigned);
                if (!shiftAssigned) summary.EmployeesWithoutShift++;

                var row = new RegisterRow
                {
                    EmployeeId = employee.Id,
                    EmployeeCode = employee.EmployeeCode,
                    FullName = FullName(employee),
                    Email = employee.Email,
                    Department = PeriodValue(employee.Departments, shiftAsOf),
                    Shift = shift.Name,
                    ShiftAssigned = shiftAssigned,
                    ShiftSource = string.IsNullOrWhiteSpace(shiftSource) ? null : shiftSource.Trim(),
                    JoiningDate = Iso(employee.JoiningDate),
                    ReleaseDate = Iso(employee.ReleaseDate)
                };

                for (var i = 0; i < days.Count; i++)
                {
                    var date = from.AddDays(i);
                    var (cell, state) = BuildCell(employee, days[i], date, today, nowLocal, leaves, sheets);
                    // The shift travels on the row; a cell repeats it only when a designation change made it differ.
                    if (cell.Shift == row.Shift && cell.ShiftAssigned == row.ShiftAssigned)
                    {
                        cell.Shift = null;
                        cell.ShiftAssigned = null;
                    }
                    row.Cells.Add(cell);
                    Accumulate(row.Totals, summary.PerDay[i], summary, cell, state);
                }

                result.Rows.Add(row);
            }

            return result;
        }
        #endregion

        #region GetDayDetailAsync
        public async Task<TimesheetRegisterDayDetail> GetDayDetailAsync(string siteId, string timeZoneId, string employeeId, DateTime date, DateTime? nowUtc = null)
        {
            if (string.IsNullOrWhiteSpace(employeeId)) throw new ArgumentException("Employee is required.");

            var register = await GetRegisterAsync(new TimesheetRegisterQuery
            {
                SiteId = siteId,
                TimeZoneId = timeZoneId,
                FromDate = date.Date,
                ToDate = date.Date,
                EmployeeIds = new List<string> { employeeId },
                NowUtc = nowUtc
            });

            var row = register.Rows.FirstOrDefault()
                ?? throw new ArgumentException("The employee was not found among the active employees of this site.");

            var tz = ResolveTimeZone(timeZoneId);
            var from = date.Date;
            var toExclusive = from.AddDays(1);

            var lines = await _timesheetLinesRepository.TableNoTracking
                .Where(l => !l.Deleted
                    && l.Timesheet != null && !l.Timesheet.Deleted
                    && l.Timesheet.SiteId == siteId
                    && l.Timesheet.EmployeeId == employeeId
                    && l.Timesheet.TimesheetDate != null
                    && l.Timesheet.TimesheetDate >= from && l.Timesheet.TimesheetDate < toExclusive)
                .OrderBy(l => l.CreatedOnUtc)
                .Select(l => new
                {
                    Project = l.Project != null ? l.Project.Name : null,
                    ProjectModule = l.ProjectModule != null ? l.ProjectModule.Name : null,
                    Task = l.Task != null ? l.Task.Name : null,
                    Activity = l.ProjectActivity != null ? l.ProjectActivity.Name : null,
                    l.Description,
                    l.Hours,
                    l.BillableHours,
                    l.CreatedOnUtc,
                    l.IsApproved
                })
                .ToListAsync();

            return new TimesheetRegisterDayDetail
            {
                Employee = row,
                Day = register.Days.First(),
                Cell = row.Cells.First(),
                Rules = register.Rules,
                Lines = lines.Select(l => new RegisterDayLine
                {
                    Project = l.Project,
                    ProjectModule = l.ProjectModule,
                    Task = l.Task,
                    Activity = l.Activity,
                    Description = Excerpt(l.Description),
                    Hours = l.Hours,
                    BillableHours = l.BillableHours,
                    EnteredAt = ToLocal(l.CreatedOnUtc, tz).ToString(IsoMinute, CultureInfo.InvariantCulture),
                    IsApproved = l.IsApproved
                }).ToList()
            };
        }
        #endregion

        #region Classification
        private static (RegisterCell Cell, RegisterCellState State) BuildCell(
            EmployeeSnapshot employee,
            RegisterDay day,
            DateTime date,
            DateTime today,
            DateTime nowLocal,
            Dictionary<(string EmployeeId, DateTime Date), RegisterLeave> leaves,
            Dictionary<(string EmployeeId, DateTime Date), DaySheet> sheets)
        {
            var cell = new RegisterCell();

            sheets.TryGetValue((employee.Id, date), out var sheet);
            if (sheet != null)
            {
                cell.Logged = sheet.Entries.Sum(e => e.Hours);
                cell.Lines = sheet.Entries.Count;
                cell.TimesheetStatus = sheet.Status;
                var first = sheet.Entries.OrderBy(e => e.EnteredAtLocal).FirstOrDefault();
                if (first != null)
                    cell.FirstEntryAt = first.EnteredAtLocal.ToString(IsoMinute, CultureInfo.InvariantCulture);
            }

            // Employment bounds first: nothing is expected before joining or after release.
            if ((employee.JoiningDate != null && date < employee.JoiningDate.Value.Date)
                || (employee.ReleaseDate != null && date > employee.ReleaseDate.Value.Date))
                return Finish(cell, RegisterCellState.NotEmployed);

            // Sunday or a LeaveSchedule date (the title travels on the day column).
            if (day.IsHoliday)
                return Finish(cell, RegisterCellState.Holiday);

            // Leave: a full day expects nothing; a half day expects 4 h.
            leaves.TryGetValue((employee.Id, date), out var leave);
            cell.Leave = leave;
            cell.Expected = TimesheetRegisterRules.ExpectedHours(leave);
            if (leave != null && cell.Expected == 0)
                return Finish(cell, RegisterCellState.Leave);

            // Not due yet: nothing to judge, so no shift or cutoff is sent for it.
            if (date > today)
            {
                cell.Expected = 0;
                cell.PreFilled = cell.Logged > 0;
                return Finish(cell, RegisterCellState.Future);
            }

            // The shift (as of that date) gives the cutoff.
            var shift = TimesheetRegisterRules.ResolveShift(PeriodValue(employee.Designations, date), out var shiftAssigned);
            cell.Shift = shift.Name;
            cell.ShiftAssigned = shiftAssigned;
            var cutoff = TimesheetRegisterRules.CutoffFor(date, shift);
            cell.Cutoff = cutoff.ToString(IsoMinute, CultureInfo.InvariantCulture);

            var entries = sheet?.Entries ?? new List<RegisterEntry>();
            var completedAt = TimesheetRegisterRules.CompletedAt(entries, cell.Expected);
            var state = TimesheetRegisterRules.Classify(cell.Expected, cell.Logged, completedAt, cutoff, nowLocal);

            if (completedAt != null)
            {
                cell.CompletedAt = completedAt.Value.ToString(IsoMinute, CultureInfo.InvariantCulture);
                cell.PreFilled = completedAt.Value < date;
                if (state == RegisterCellState.Delayed)
                    cell.LateMinutes = (int)Math.Ceiling((completedAt.Value - cutoff).TotalMinutes);
            }

            return Finish(cell, state);
        }

        private static (RegisterCell, RegisterCellState) Finish(RegisterCell cell, RegisterCellState state)
        {
            cell.State = state.ToString();
            return (cell, state);
        }

        private static void Accumulate(RegisterRowTotals row, RegisterDayTotal day, RegisterSummary summary, RegisterCell cell, RegisterCellState state)
        {
            row.LoggedHours += cell.Logged;
            summary.LoggedHours += cell.Logged;

            var expected = false;
            switch (state)
            {
                case RegisterCellState.OnTime:
                    row.OnTime++; day.OnTime++; day.Complete++; summary.OnTime++; expected = true;
                    break;
                case RegisterCellState.Delayed:
                    row.Delayed++; day.Delayed++; day.Complete++; summary.Delayed++; expected = true;
                    row.LateMinutes += cell.LateMinutes ?? 0;
                    break;
                case RegisterCellState.Partial:
                    row.Partial++; day.Partial++; summary.Partial++; expected = true;
                    break;
                case RegisterCellState.Pending:
                    row.Pending++; day.Pending++; summary.Pending++; expected = true;
                    break;
                case RegisterCellState.Due:
                    row.Due++; day.Due++; summary.Due++; expected = true;
                    break;
                case RegisterCellState.Leave:
                    row.Leave++; day.Leave++; summary.LeaveDays++;
                    break;
                case RegisterCellState.Holiday:
                    row.Holiday++;
                    break;
            }

            if (expected)
            {
                row.ExpectedDays++;
                row.ExpectedHours += cell.Expected;
                day.Expected++;
                summary.ExpectedDays++;
                summary.ExpectedHours += cell.Expected;
            }

            if (cell.Leave != null && cell.Leave.Kind != TimesheetRegisterRules.FullDayLeaveKind)
            {
                row.HalfDay++;
                summary.HalfDays++;
            }
        }

        /// <summary>A full-day leave outranks a half day; an approved leave outranks one still in the workflow.</summary>
        private static bool Outranks(RegisterLeave candidate, RegisterLeave existing)
        {
            var candidateFull = candidate.Kind == TimesheetRegisterRules.FullDayLeaveKind;
            var existingFull = existing.Kind == TimesheetRegisterRules.FullDayLeaveKind;
            if (candidateFull != existingFull) return candidateFull;
            return candidate.IsApproved && !existing.IsApproved;
        }

        /// <summary>
        /// The value (shift, department) in force on a date: the row whose start/end period covers it, latest
        /// start first; otherwise the most recent row that has a value at all.
        /// </summary>
        private static string PeriodValue(List<PeriodSnapshot> rows, DateTime asOf)
        {
            if (rows == null || rows.Count == 0) return null;

            var withValue = rows.Where(r => !string.IsNullOrWhiteSpace(r.Value)).ToList();
            if (withValue.Count == 0) return null;

            var covering = withValue
                .Where(r => (r.StartDate == null || r.StartDate.Value.Date <= asOf) && (r.EndDate == null || r.EndDate.Value.Date >= asOf))
                .OrderByDescending(r => r.StartDate ?? DateTime.MinValue)
                .ThenByDescending(r => r.CreatedOnUtc ?? DateTime.MinValue)
                .FirstOrDefault();

            var chosen = covering ?? withValue
                .OrderByDescending(r => r.StartDate ?? DateTime.MinValue)
                .ThenByDescending(r => r.CreatedOnUtc ?? DateTime.MinValue)
                .First();

            return chosen.Value;
        }
        #endregion

        #region Helpers
        private static TimeZoneInfo ResolveTimeZone(string id)
        {
            foreach (var candidate in new[] { id, TimesheetRegisterRules.DefaultTimeZoneId })
            {
                if (string.IsNullOrWhiteSpace(candidate)) continue;
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(candidate.Trim());
                }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }

            return TimeZoneInfo.Utc;
        }

        /// <summary>Entry times are stored as UTC (SiteService.GetDateTime returns UtcNow) with an unspecified kind.</summary>
        private static DateTime ToLocal(DateTime utc, TimeZoneInfo tz)
        {
            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), tz);
            }
            catch (ArgumentOutOfRangeException)
            {
                return utc;
            }
        }

        private static string Iso(DateTime? date)
        {
            return date?.ToString(IsoDate, CultureInfo.InvariantCulture);
        }

        private static string FullName(EmployeeSnapshot employee)
        {
            var name = $"{employee.FirstName} {employee.LastName}".Trim();
            return name.Length > 0 ? name : (employee.EmployeeCode ?? employee.Id);
        }

        private static readonly Regex HtmlTag = new Regex("<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex WhiteSpace = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>Plain text of a (HTML) description, trimmed to a readable length.</summary>
        private static string Excerpt(string html, int max = 300)
        {
            if (string.IsNullOrWhiteSpace(html)) return null;
            var text = WebUtility.HtmlDecode(HtmlTag.Replace(html, " "));
            text = WhiteSpace.Replace(text, " ").Trim();
            if (text.Length == 0) return null;
            return text.Length <= max ? text : text.Substring(0, max).TrimEnd() + "…";
        }
        #endregion
    }

    #region Query snapshots (what the four queries project into)
    internal sealed class EmployeeSnapshot
    {
        public string Id { get; set; }
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<PeriodSnapshot> Designations { get; set; } = new List<PeriodSnapshot>();
        public List<PeriodSnapshot> Departments { get; set; } = new List<PeriodSnapshot>();
    }

    internal sealed class PeriodSnapshot
    {
        public string Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
    }

    internal sealed class TimesheetSnapshot
    {
        public string EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public List<LineSnapshot> Lines { get; set; } = new List<LineSnapshot>();
    }

    internal sealed class LineSnapshot
    {
        public decimal Hours { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    internal sealed class DaySheet
    {
        public string Status { get; set; }
        public List<RegisterEntry> Entries { get; } = new List<RegisterEntry>();
    }
    #endregion
}
