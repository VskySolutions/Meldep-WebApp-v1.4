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
    /// Builds the Timesheet Register: the shared <see cref="RegisterContext"/> (period, holidays, employees, team
    /// leads, leaves) plus one query for the timesheet headers with the hours and entry time of each line, then
    /// classifies every employee-day in memory with <see cref="TimesheetRegisterRules"/>. Line descriptions are
    /// never loaded for the matrix; they come only through <see cref="GetDayDetailAsync"/>.
    /// </summary>
    public class TimesheetRegisterService : ITimesheetRegisterService
    {
        private const string IsoDate = RegisterContext.IsoDate;
        private const string IsoMinute = RegisterContext.IsoMinute;

        #region Fields
        private readonly RegisterContextLoader _contextLoader;
        private readonly IRepository<Timesheet> _timesheetRepository;
        private readonly IRepository<TimesheetLines> _timesheetLinesRepository;
        #endregion

        #region Ctor
        public TimesheetRegisterService(
            IRepository<Employee> employeeRepository,
            IRepository<EmployeeLeave> employeeLeaveRepository,
            IRepository<LeaveSchedules> leaveScheduleRepository,
            IRepository<Timesheet> timesheetRepository,
            IRepository<TimesheetLines> timesheetLinesRepository,
            IRepository<EmployeeOrgStructure> employeeOrgStructureRepository)
        {
            _contextLoader = new RegisterContextLoader(employeeRepository, employeeLeaveRepository, leaveScheduleRepository, employeeOrgStructureRepository);
            _timesheetRepository = timesheetRepository;
            _timesheetLinesRepository = timesheetLinesRepository;
        }
        #endregion

        #region GetRegisterAsync
        public async Task<TimesheetRegisterResult> GetRegisterAsync(TimesheetRegisterQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            // The period, the holidays, the employees with their team leads and the leaves: shared by every register.
            var context = await _contextLoader.LoadAsync(query.SiteId, query.TimeZoneId, query.FromDate, query.ToDate, query.EmployeeIds, query.NowUtc);
            var siteId = context.SiteId;
            var from = context.From;
            var to = context.To;
            var toExclusive = context.ToExclusive;
            var tz = context.TimeZone;
            var nowLocal = context.NowLocal;
            var today = context.Today;
            var employees = context.Employees;
            var employeeIds = context.EmployeeIds;
            var leaves = context.Leaves;
            var days = context.Days;

            // Timesheet headers of the range with the hours and entry time of every line.
            var timesheetQuery = _timesheetRepository.TableNoTracking
                .Where(t => !t.Deleted && t.SiteId == siteId && t.TimesheetDate != null && t.TimesheetDate >= from && t.TimesheetDate < toExclusive);
            if (context.FilterEmployees)
                timesheetQuery = timesheetQuery.Where(t => context.RequestedEmployeeIds.Contains(t.EmployeeId));

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

            // One row per employee, classified cell by cell.
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

            foreach (var employee in employees)
            {
                var row = context.NewRow<RegisterRow>(employee);
                if (!row.ShiftAssigned) summary.EmployeesWithoutShift++;

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

        #region GetDayDetailAsync / GetEmployeeDetailAsync
        public async Task<TimesheetRegisterDayDetail> GetDayDetailAsync(string siteId, string timeZoneId, string employeeId, DateTime date, DateTime? nowUtc = null)
        {
            var detail = await GetEmployeeDetailAsync(siteId, timeZoneId, employeeId, date.Date, date.Date, nowUtc);

            return new TimesheetRegisterDayDetail
            {
                Employee = detail.Employee,
                Day = detail.Days.First(),
                Cell = detail.Employee.Cells.First(),
                Rules = detail.Rules,
                Lines = detail.Lines
            };
        }

        public async Task<TimesheetRegisterEmployeeDetail> GetEmployeeDetailAsync(string siteId, string timeZoneId, string employeeId, DateTime fromDate, DateTime toDate, DateTime? nowUtc = null)
        {
            if (string.IsNullOrWhiteSpace(employeeId)) throw new ArgumentException("Employee is required.");

            var from = fromDate.Date;
            var to = toDate.Date;
            if (to < from) (from, to) = (to, from);
            var toExclusive = to.AddDays(1);

            var register = await GetRegisterAsync(new TimesheetRegisterQuery
            {
                SiteId = siteId,
                TimeZoneId = timeZoneId,
                FromDate = from,
                ToDate = to,
                EmployeeIds = new List<string> { employeeId },
                NowUtc = nowUtc
            });

            var row = register.Rows.FirstOrDefault()
                ?? throw new ArgumentException("The employee was not found among the active employees of this site.");

            var tz = RegisterContext.ResolveTimeZone(timeZoneId);

            var lines = await _timesheetLinesRepository.TableNoTracking
                .Where(l => !l.Deleted
                    && l.Timesheet != null && !l.Timesheet.Deleted
                    && l.Timesheet.SiteId == siteId
                    && l.Timesheet.EmployeeId == employeeId
                    && l.Timesheet.TimesheetDate != null
                    && l.Timesheet.TimesheetDate >= from && l.Timesheet.TimesheetDate < toExclusive)
                .OrderBy(l => l.Timesheet.TimesheetDate)
                .ThenBy(l => l.CreatedOnUtc)
                .Select(l => new
                {
                    Date = l.Timesheet.TimesheetDate.Value,
                    Status = l.Timesheet.TimesheetStatus != null ? l.Timesheet.TimesheetStatus.DropDownValue : null,
                    Project = l.Project != null ? l.Project.Name : null,
                    ProjectModule = l.ProjectModule != null ? l.ProjectModule.Name : null,
                    Task = l.Task != null ? l.Task.Name : null,
                    Activity = l.ProjectActivity != null ? l.ProjectActivity.Name : null,
                    // The column is nullable in the database although the model calls it required; a bare read throws on NULL.
                    Description = l.Description ?? string.Empty,
                    l.Hours,
                    l.BillableHours,
                    l.CreatedOnUtc,
                    l.IsApproved
                })
                .ToListAsync();

            return new TimesheetRegisterEmployeeDetail
            {
                FromDate = register.FromDate,
                ToDate = register.ToDate,
                Employee = row,
                Days = register.Days,
                Rules = register.Rules,
                Lines = lines.Select(l => new RegisterDayLine
                {
                    Date = l.Date.ToString(IsoDate, CultureInfo.InvariantCulture),
                    TimesheetStatus = l.Status,
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
            if (!RegisterContext.IsEmployedOn(employee, date))
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
            var shift = TimesheetRegisterRules.ResolveShift(RegisterContext.PeriodValue(employee.Designations, date), out var shiftAssigned);
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

        #endregion

        #region Helpers
        private static DateTime ToLocal(DateTime utc, TimeZoneInfo tz) => RegisterContext.ToLocal(utc, tz);

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

    #region Query snapshots (what the timesheet query projects into)
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
