using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;
using Vsky.Models.Analytics;

namespace Vsky.Services.Analytics
{
    /// <summary>
    /// Builds the Attendance Register: the shared <see cref="RegisterContext"/> (period, holidays, employees, team
    /// leads, leaves) plus two set-based queries, the time-ins of TimeInTimeOut and the entries of the Movement
    /// Register, merged by employee and day and classified in memory with <see cref="AttendanceRegisterRules"/>.
    /// The matrix carries counts and minutes only; messages and approvers come through the detail calls.
    /// </summary>
    public class AttendanceRegisterService : IAttendanceRegisterService
    {
        private const string IsoDate = RegisterContext.IsoDate;
        private const string IsoMinute = RegisterContext.IsoMinute;

        /// <summary>
        /// TimeInDate is the UTC date of the time-in. Its site-local date can be the day after, and a time-in before
        /// the day start counts for the day before, so the time-ins are read with a margin around the period and
        /// placed on their day in memory.
        /// </summary>
        private const int QueryMarginDays = 2;

        #region Fields
        private readonly RegisterContextLoader _contextLoader;
        private readonly IRepository<TimeInTimeOut> _timeInTimeOutRepository;
        private readonly IRepository<MovementRegisterDetails> _movementRegisterDetailsRepository;
        #endregion

        #region Ctor
        public AttendanceRegisterService(
            IRepository<Employee> employeeRepository,
            IRepository<EmployeeLeave> employeeLeaveRepository,
            IRepository<LeaveSchedules> leaveScheduleRepository,
            IRepository<EmployeeOrgStructure> employeeOrgStructureRepository,
            IRepository<TimeInTimeOut> timeInTimeOutRepository,
            IRepository<MovementRegisterDetails> movementRegisterDetailsRepository)
        {
            _contextLoader = new RegisterContextLoader(employeeRepository, employeeLeaveRepository, leaveScheduleRepository, employeeOrgStructureRepository);
            _timeInTimeOutRepository = timeInTimeOutRepository;
            _movementRegisterDetailsRepository = movementRegisterDetailsRepository;
        }
        #endregion

        #region GetRegisterAsync
        public async Task<AttendanceRegisterResult> GetRegisterAsync(AttendanceRegisterQuery query)
        {
            return (await BuildAsync(query)).Result;
        }

        /// <summary>The register, with the context and the time-ins it was built from (the detail calls list them).</summary>
        private async Task<RegisterBuild> BuildAsync(AttendanceRegisterQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            // The period, the holidays, the employees with their team leads and the leaves: shared by every register.
            var context = await _contextLoader.LoadAsync(query.SiteId, query.TimeZoneId, query.FromDate, query.ToDate, query.EmployeeIds, query.NowUtc);
            var days = context.Days;

            // TimeInTimeOut and the Movement Register, each keyed by employee and day.
            var sessions = await LoadSessionsAsync(context);
            var movements = await LoadMovementsAsync(context);

            // One row per employee, classified cell by cell.
            var result = new AttendanceRegisterResult
            {
                FromDate = context.From.ToString(IsoDate, CultureInfo.InvariantCulture),
                ToDate = context.To.ToString(IsoDate, CultureInfo.InvariantCulture),
                Today = context.Today.ToString(IsoDate, CultureInfo.InvariantCulture),
                GeneratedAt = context.NowLocal.ToString(IsoMinute, CultureInfo.InvariantCulture),
                Days = days,
                Rules = AttendanceRegisterRules.Describe(context.TimeZone.Id)
            };
            var summary = result.Summary;
            summary.Employees = context.Employees.Count;
            summary.PerDay = days.Select(d => new AttendanceDayTotal { Date = d.Date }).ToList();

            foreach (var employee in context.Employees)
            {
                var row = context.NewRow<AttendanceRow>(employee);
                if (!row.ShiftAssigned) summary.EmployeesWithoutShift++;

                for (var i = 0; i < days.Count; i++)
                {
                    var date = context.From.AddDays(i);
                    sessions.TryGetValue((employee.Id, date), out var daySessions);
                    movements.TryGetValue((employee.Id, date), out var dayMovement);

                    var (cell, state) = BuildCell(context, employee, days[i], date, daySessions, dayMovement);
                    // The shift travels on the row; a cell repeats it only when a designation change made it differ.
                    if (cell.Shift == row.Shift && cell.ShiftAssigned == row.ShiftAssigned)
                    {
                        cell.Shift = null;
                        cell.ShiftAssigned = null;
                    }
                    row.Cells.Add(cell);
                    Accumulate(row.Totals, cell, state);
                    Accumulate(summary, cell, state);
                    Accumulate(summary.PerDay[i], cell, state);
                }

                // The hours balance is struck per employee over the period (a long day offsets a short one),
                // so the register's shortfall is the sum of the employees' shortfalls, not one net figure.
                row.Totals.ShortMinutes = AttendanceRegisterRules.ShortMinutes(row.Totals.RequiredMinutes, row.Totals.WorkedMinutes);
                row.Totals.ExtraMinutes = AttendanceRegisterRules.ExtraMinutes(row.Totals.RequiredMinutes, row.Totals.WorkedMinutes);
                summary.ShortMinutes += row.Totals.ShortMinutes;
                summary.ExtraMinutes += row.Totals.ExtraMinutes;
                if (row.Totals.ShortMinutes > 0) summary.EmployeesShort++;

                result.Rows.Add(row);
            }

            return new RegisterBuild { Context = context, Result = result, Sessions = sessions };
        }
        #endregion

        #region GetDayDetailAsync / GetEmployeeDetailAsync
        public async Task<AttendanceRegisterDayDetail> GetDayDetailAsync(string siteId, string timeZoneId, string employeeId, DateTime date, DateTime? nowUtc = null)
        {
            var detail = await GetEmployeeDetailAsync(siteId, timeZoneId, employeeId, date.Date, date.Date, nowUtc);

            return new AttendanceRegisterDayDetail
            {
                Employee = detail.Employee,
                Day = detail.Days.First(),
                Cell = detail.Employee.Cells.First(),
                Rules = detail.Rules,
                Sessions = detail.Sessions,
                Movements = detail.Movements
            };
        }

        public async Task<AttendanceRegisterEmployeeDetail> GetEmployeeDetailAsync(string siteId, string timeZoneId, string employeeId, DateTime fromDate, DateTime toDate, DateTime? nowUtc = null)
        {
            if (string.IsNullOrWhiteSpace(employeeId)) throw new ArgumentException("Employee is required.");

            var build = await BuildAsync(new AttendanceRegisterQuery
            {
                SiteId = siteId,
                TimeZoneId = timeZoneId,
                FromDate = fromDate,
                ToDate = toDate,
                EmployeeIds = new List<string> { employeeId },
                NowUtc = nowUtc
            });

            var register = build.Result;
            var context = build.Context;
            var row = register.Rows.FirstOrDefault()
                ?? throw new ArgumentException("The employee was not found among the active employees of this site.");

            // The time-ins behind the cells: already loaded and placed on their day for the register.
            var sessionLines = build.Sessions
                .Where(pair => pair.Key.EmployeeId == employeeId)
                .OrderBy(pair => pair.Key.Date)
                .SelectMany(pair => pair.Value.Select(session => new AttendanceSessionLine
                {
                    Date = pair.Key.Date.ToString(IsoDate, CultureInfo.InvariantCulture),
                    In = session.In.ToString(IsoMinute, CultureInfo.InvariantCulture),
                    Out = RegisterContext.IsoMinuteText(session.Out),
                    Minutes = AttendanceRegisterRules.MinutesOf(session, context.NowLocal),
                    Status = AttendanceRegisterRules.StatusOf(session, context.NowLocal).ToString()
                }))
                .ToList();

            return new AttendanceRegisterEmployeeDetail
            {
                FromDate = register.FromDate,
                ToDate = register.ToDate,
                Employee = row,
                Days = register.Days,
                Rules = register.Rules,
                Sessions = sessionLines,
                Movements = await LoadMovementLinesAsync(context, employeeId)
            };
        }
        #endregion

        #region Loading
        /// <summary>
        /// The time-ins of the period by employee and attendance day, earliest first. TimeInDate + TimeIn (and the
        /// time-out pair) are stored in UTC; they are joined, converted to the site zone and cut to the minute.
        /// </summary>
        private async Task<Dictionary<(string EmployeeId, DateTime Date), List<AttendanceSession>>> LoadSessionsAsync(RegisterContext context)
        {
            var siteId = context.SiteId;
            var first = context.From.AddDays(-QueryMarginDays);
            var lastExclusive = context.ToExclusive.AddDays(QueryMarginDays);

            var query = _timeInTimeOutRepository.TableNoTracking
                .Where(t => !t.Deleted && t.SiteId == siteId && t.TimeInDate != null && t.TimeInDate >= first && t.TimeInDate < lastExclusive);
            if (context.FilterEmployees)
                query = query.Where(t => context.RequestedEmployeeIds.Contains(t.EmployeeId));

            var rows = await query
                .Select(t => new { t.EmployeeId, t.TimeInDate, t.TimeIn, t.TimeOutDate, t.TimeOut })
                .ToListAsync();

            var sessions = new Dictionary<(string EmployeeId, DateTime Date), List<AttendanceSession>>();
            foreach (var row in rows)
            {
                if (row.EmployeeId == null || !context.EmployeeIds.Contains(row.EmployeeId)) continue;

                var timeIn = ToLocalMinute(row.TimeInDate.Value.Date.Add(row.TimeIn), context.TimeZone);
                var date = AttendanceRegisterRules.AttendanceDateOf(timeIn);
                if (date < context.From || date > context.To) continue;

                var key = (row.EmployeeId, date);
                if (!sessions.TryGetValue(key, out var list))
                {
                    list = new List<AttendanceSession>();
                    sessions[key] = list;
                }

                list.Add(new AttendanceSession
                {
                    In = timeIn,
                    Out = row.TimeOutDate == null
                        ? (DateTime?)null
                        : ToLocalMinute(row.TimeOutDate.Value.Date.Add(row.TimeOut), context.TimeZone)
                });
            }

            foreach (var list in sessions.Values)
                list.Sort((a, b) => a.In.CompareTo(b.In));

            return sessions;
        }

        /// <summary>
        /// The Movement Register entries of the period added up by employee and day. The day is the register date
        /// the entry was filed under (MovementRegister.Date), which is what the Movement Register list shows.
        /// </summary>
        private async Task<Dictionary<(string EmployeeId, DateTime Date), AttendanceMovement>> LoadMovementsAsync(RegisterContext context)
        {
            var siteId = context.SiteId;
            var from = context.From;
            var toExclusive = context.ToExclusive;

            var query = _movementRegisterDetailsRepository.TableNoTracking
                .Where(d => !d.Deleted
                    && d.MovementRegister != null && !d.MovementRegister.Deleted
                    && d.MovementRegister.SiteId == siteId
                    && d.MovementRegister.Date != null
                    && d.MovementRegister.Date >= from && d.MovementRegister.Date < toExclusive);
            if (context.FilterEmployees)
                query = query.Where(d => context.RequestedEmployeeIds.Contains(d.EmployeeId));

            var rows = await query
                .Select(d => new
                {
                    d.EmployeeId,
                    Date = d.MovementRegister.Date.Value,
                    Type = d.Type != null ? d.Type.DropDownValue : null,
                    d.TimeInMinutes,
                    WfhDuration = d.WFHDuration != null ? d.WFHDuration.DropDownText : null
                })
                .ToListAsync();

            var movements = new Dictionary<(string EmployeeId, DateTime Date), AttendanceMovement>();
            foreach (var row in rows)
            {
                if (!context.EmployeeIds.Contains(row.EmployeeId)) continue;

                var key = (row.EmployeeId, row.Date.Date);
                if (!movements.TryGetValue(key, out var day))
                {
                    day = new AttendanceMovement();
                    movements[key] = day;
                }

                AttendanceRegisterRules.Add(day, row.Type, row.TimeInMinutes, row.WfhDuration);
            }

            return movements;
        }

        /// <summary>Every Movement Register entry of one employee in the period, with its message, approver and entry time.</summary>
        private async Task<List<AttendanceMovementLine>> LoadMovementLinesAsync(RegisterContext context, string employeeId)
        {
            var siteId = context.SiteId;
            var from = context.From;
            var toExclusive = context.ToExclusive;

            var rows = await _movementRegisterDetailsRepository.TableNoTracking
                .Where(d => !d.Deleted
                    && d.EmployeeId == employeeId
                    && d.MovementRegister != null && !d.MovementRegister.Deleted
                    && d.MovementRegister.SiteId == siteId
                    && d.MovementRegister.Date != null
                    && d.MovementRegister.Date >= from && d.MovementRegister.Date < toExclusive)
                .OrderBy(d => d.MovementRegister.Date)
                .ThenBy(d => d.CreatedOnUtc)
                .Select(d => new
                {
                    Date = d.MovementRegister.Date.Value,
                    Type = d.Type != null ? d.Type.DropDownValue : null,
                    d.TimeInMinutes,
                    WfhDuration = d.WFHDuration != null ? d.WFHDuration.DropDownText : null,
                    BreakTime = d.BreakTime != null ? d.BreakTime.DropDownText : null,
                    d.Message,
                    ApproverFirstName = d.Approvers != null && d.Approvers.Person != null ? d.Approvers.Person.FirstName : null,
                    ApproverLastName = d.Approvers != null && d.Approvers.Person != null ? d.Approvers.Person.LastName : null,
                    d.NotifyToStakeholders,
                    d.CreatedOnUtc,
                    EnteredByFirstName = d.CreatedBy != null && d.CreatedBy.Person != null ? d.CreatedBy.Person.FirstName : null,
                    EnteredByLastName = d.CreatedBy != null && d.CreatedBy.Person != null ? d.CreatedBy.Person.LastName : null
                })
                .ToListAsync();

            return rows.Select(m =>
            {
                var kind = AttendanceRegisterRules.KindOf(m.Type);
                var enteredAt = RegisterContext.ToLocal(m.CreatedOnUtc, context.TimeZone);

                return new AttendanceMovementLine
                {
                    Date = m.Date.ToString(IsoDate, CultureInfo.InvariantCulture),
                    Type = m.Type,
                    Kind = kind.ToString(),
                    Minutes = m.TimeInMinutes,
                    Duration = kind == AttendanceMovementKind.WorkFromHome
                        ? AttendanceRegisterRules.WfhLength(m.WfhDuration, m.TimeInMinutes)
                        : Text(m.BreakTime),
                    Message = Text(m.Message),
                    Approver = Text($"{m.ApproverFirstName} {m.ApproverLastName}"),
                    NotifyToStakeholders = m.NotifyToStakeholders,
                    EnteredAt = enteredAt.ToString(IsoMinute, CultureInfo.InvariantCulture),
                    EnteredBy = Text($"{m.EnteredByFirstName} {m.EnteredByLastName}"),
                    EnteredDaysLater = (enteredAt.Date - m.Date.Date).Days
                };
            }).ToList();
        }
        #endregion

        #region Classification
        private static (AttendanceCell Cell, AttendanceCellState State) BuildCell(
            RegisterContext context,
            EmployeeSnapshot employee,
            RegisterDay day,
            DateTime date,
            List<AttendanceSession> sessions,
            AttendanceMovement movement)
        {
            var cell = new AttendanceCell();
            var nowLocal = context.NowLocal;

            // What the Movement Register holds for the day travels on the cell whatever the verdict.
            if (movement != null)
            {
                cell.Breaks = movement.Breaks;
                cell.BreakMinutes = movement.BreakMinutes;
                cell.Adjustments = movement.Adjustments;
                cell.AdjustMinutes = movement.AdjustMinutes;
                cell.Wfh = movement.Wfh;
            }

            // The time-ins of the day: first in, last out, and the minutes that can be trusted.
            var timedIn = sessions != null && sessions.Count > 0;
            var live = false;
            var missingTimeOut = false;
            if (timedIn)
            {
                var clocked = 0;
                foreach (var session in sessions)
                {
                    var status = AttendanceRegisterRules.StatusOf(session, nowLocal);
                    if (status == AttendanceSessionStatus.Live) live = true;
                    else if (status != AttendanceSessionStatus.Closed) missingTimeOut = true;
                    clocked += AttendanceRegisterRules.MinutesOf(session, nowLocal);
                }

                // A forgotten time-out leaves the day's hours unknown, so none are counted; while the employee
                // is in again, the day shows what is known so far.
                if (missingTimeOut && !live) clocked = 0;

                cell.Sessions = sessions.Count;
                cell.FirstIn = sessions[0].In.ToString(IsoMinute, CultureInfo.InvariantCulture);
                if (!live && !missingTimeOut)
                    cell.LastOut = RegisterContext.IsoMinuteText(sessions[sessions.Count - 1].Out);
                cell.Clocked = clocked;
                cell.Worked = AttendanceRegisterRules.ProductiveMinutes(clocked, cell.BreakMinutes, cell.AdjustMinutes);
            }

            // Employment bounds first: nobody is expected before joining or after release.
            if (!RegisterContext.IsEmployedOn(employee, date))
                return Finish(cell, AttendanceCellState.NotEmployed);

            // Sunday or a LeaveSchedule date (the title travels on the day column).
            if (day.IsHoliday)
                return Finish(cell, AttendanceCellState.Holiday);

            // Leave: a full day expects nobody; a half day expects half the hours.
            context.Leaves.TryGetValue((employee.Id, date), out var leave);
            cell.Leave = leave;
            cell.Expected = AttendanceRegisterRules.ExpectedMinutes(leave);
            if (leave != null && cell.Expected == 0)
                return Finish(cell, AttendanceCellState.Leave);

            // Not due yet: nothing to judge, so no shift is sent for it.
            if (date > context.Today)
            {
                cell.Expected = 0;
                return Finish(cell, AttendanceCellState.Future);
            }

            // The shift (as of that date) gives the moment after which the first time-in is late.
            var shift = context.ShiftOn(employee, date, out var shiftAssigned);
            cell.Shift = shift.Name;
            cell.ShiftAssigned = shiftAssigned;
            if (timedIn && AttendanceRegisterRules.IsArrivalJudged(leave))
                cell.LateMinutes = AttendanceRegisterRules.LateMinutes(sessions[0].In, date, shift);

            var state = AttendanceRegisterRules.Classify(
                timedIn,
                live,
                missingTimeOut,
                cell.Worked,
                cell.Expected,
                cell.LateMinutes,
                cell.Wfh != null,
                AttendanceRegisterRules.LateAfter(date, shift),
                nowLocal);

            return Finish(cell, state);
        }

        private static (AttendanceCell, AttendanceCellState) Finish(AttendanceCell cell, AttendanceCellState state)
        {
            cell.State = state.ToString();
            return (cell, state);
        }

        /// <summary>Adds one employee-day to a set of totals (an employee's period, or the whole register).</summary>
        private static void Accumulate(AttendanceRowTotals totals, AttendanceCell cell, AttendanceCellState state)
        {
            totals.ClockedMinutes += cell.Clocked;
            totals.WorkedMinutes += cell.Worked;
            totals.Breaks += cell.Breaks;
            totals.BreakMinutes += cell.BreakMinutes;
            totals.Adjustments += cell.Adjustments;
            totals.AdjustMinutes += cell.AdjustMinutes;
            if (cell.Wfh != null) totals.Wfh++;

            var expected = false;
            // Required hours count once the day can be judged: not while the employee is still in or still awaited.
            var closed = state != AttendanceCellState.InNow && state != AttendanceCellState.Awaited;
            switch (state)
            {
                case AttendanceCellState.OnTime:
                    totals.OnTime++; totals.Present++; expected = true;
                    break;
                case AttendanceCellState.Late:
                    totals.Present++; expected = true;
                    break;
                case AttendanceCellState.Short:
                    totals.Short++; totals.Present++; expected = true;
                    break;
                case AttendanceCellState.NoTimeOut:
                    totals.NoTimeOut++; totals.Present++; expected = true;
                    break;
                case AttendanceCellState.InNow:
                    totals.InNow++; totals.Present++; expected = true;
                    break;
                case AttendanceCellState.Wfh:
                    totals.WfhNoTimeIn++; expected = true;
                    break;
                case AttendanceCellState.Absent:
                    totals.Absent++; expected = true;
                    break;
                case AttendanceCellState.Awaited:
                    totals.Awaited++; expected = true;
                    break;
                case AttendanceCellState.Leave:
                    totals.Leave++;
                    break;
                case AttendanceCellState.Holiday:
                    totals.Holiday++;
                    break;
            }

            // A late arrival is counted whatever else the day turned into (short hours, no time-out, still in).
            if (cell.LateMinutes > 0)
            {
                totals.Late++;
                totals.LateMinutes += cell.LateMinutes;
                if (cell.Adjustments == 0) totals.LateUndeclared++;
            }

            if (expected)
            {
                totals.ExpectedDays++;
                totals.ExpectedMinutes += cell.Expected;
                if (closed) totals.RequiredMinutes += cell.Expected;
            }

            if (cell.Leave != null && cell.Leave.Kind != TimesheetRegisterRules.FullDayLeaveKind)
                totals.HalfDay++;
        }

        /// <summary>Adds one employee to the totals of a day column.</summary>
        private static void Accumulate(AttendanceDayTotal day, AttendanceCell cell, AttendanceCellState state)
        {
            switch (state)
            {
                case AttendanceCellState.OnTime:
                    day.OnTime++; day.Present++; day.Expected++;
                    break;
                case AttendanceCellState.Late:
                    day.Present++; day.Expected++;
                    break;
                case AttendanceCellState.Short:
                    day.Short++; day.Present++; day.Expected++;
                    break;
                case AttendanceCellState.NoTimeOut:
                    day.NoTimeOut++; day.Present++; day.Expected++;
                    break;
                case AttendanceCellState.InNow:
                    day.InNow++; day.Present++; day.Expected++;
                    break;
                case AttendanceCellState.Wfh:
                    day.Wfh++; day.Expected++;
                    break;
                case AttendanceCellState.Absent:
                    day.Absent++; day.Expected++;
                    break;
                case AttendanceCellState.Awaited:
                    day.Awaited++; day.Expected++;
                    break;
                case AttendanceCellState.Leave:
                    day.Leave++;
                    break;
            }

            if (cell.LateMinutes > 0) day.Late++;
        }
        #endregion

        #region Helpers
        /// <summary>A stored UTC clock time in the site zone, to the minute.</summary>
        private static DateTime ToLocalMinute(DateTime utc, TimeZoneInfo tz)
        {
            return AttendanceRegisterRules.TruncateToMinute(RegisterContext.ToLocal(utc, tz));
        }

        /// <summary>Trimmed text; null when there is nothing to show.</summary>
        private static string Text(string value)
        {
            var text = (value ?? string.Empty).Trim();
            return text.Length > 0 ? text : null;
        }
        #endregion

        /// <summary>What one build produces: the register, and what it was built from.</summary>
        private sealed class RegisterBuild
        {
            public RegisterContext Context { get; set; }
            public AttendanceRegisterResult Result { get; set; }
            public Dictionary<(string EmployeeId, DateTime Date), List<AttendanceSession>> Sessions { get; set; }
        }
    }
}
