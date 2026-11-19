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
    /// What every Analytics register starts from, whatever it measures: the period and "now" in the site zone,
    /// the site's holidays, the listed employees (shift and department history, team lead) and their leaves.
    /// Built once per request by <see cref="RegisterContextLoader"/>; the Timesheet Register and the Attendance
    /// Register each add their own records on top and classify the days.
    /// </summary>
    internal sealed class RegisterContext
    {
        public const string IsoDate = "yyyy-MM-dd";
        public const string IsoMinute = "yyyy-MM-dd'T'HH:mm";

        #region The request, resolved
        public string SiteId { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        /// <summary>First and last day of the period: site-local calendar dates, already in order.</summary>
        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public DateTime ToExclusive => To.AddDays(1);

        /// <summary>"Now" in the site time zone; <see cref="Today"/> is its date.</summary>
        public DateTime NowLocal { get; set; }

        public DateTime Today => NowLocal.Date;

        /// <summary>The employees the caller restricted the register to; null = every active employee of the site.</summary>
        public List<string> RequestedEmployeeIds { get; set; }

        public bool FilterEmployees => RequestedEmployeeIds != null;
        #endregion

        #region What was loaded
        /// <summary>LeaveSchedule dates of the site in the period, with their titles. Sundays are added by the rules.</summary>
        public Dictionary<DateTime, string> Holidays { get; set; } = new Dictionary<DateTime, string>();

        /// <summary>The listed employees, by first then last name.</summary>
        public List<EmployeeSnapshot> Employees { get; set; } = new List<EmployeeSnapshot>();

        public HashSet<string> EmployeeIds { get; set; } = new HashSet<string>();

        /// <summary>The manager on each employee's latest org-structure row.</summary>
        public Dictionary<string, TeamLeadSnapshot> TeamLeads { get; set; } = new Dictionary<string, TeamLeadSnapshot>();

        /// <summary>The leave that covers an employee-day (declined and cancelled ones left out).</summary>
        public Dictionary<(string EmployeeId, DateTime Date), RegisterLeave> Leaves { get; set; } = new Dictionary<(string EmployeeId, DateTime Date), RegisterLeave>();

        /// <summary>One entry per day of the period, in order: the columns of the register.</summary>
        public List<RegisterDay> Days { get; set; } = new List<RegisterDay>();
        #endregion

        #region Employees
        /// <summary>
        /// The date the row-level shift and department are read as of: the last day of the period, or today when
        /// the period runs into the future.
        /// </summary>
        public DateTime ShiftAsOf => To < Today ? To : Today;

        /// <summary>The shift in force for an employee on a date; the default shift (assigned = false) when none is set.</summary>
        public ShiftWindow ShiftOn(EmployeeSnapshot employee, DateTime date, out bool assigned)
        {
            return TimesheetRegisterRules.ResolveShift(PeriodValue(employee.Designations, date), out assigned);
        }

        /// <summary>Nothing is expected before the joining date or after the release date.</summary>
        public static bool IsEmployedOn(EmployeeSnapshot employee, DateTime date)
        {
            return !((employee.JoiningDate != null && date < employee.JoiningDate.Value.Date)
                || (employee.ReleaseDate != null && date > employee.ReleaseDate.Value.Date));
        }

        /// <summary>
        /// The identity half of a register row: who the employee is, with the department, team lead and shift as
        /// of <see cref="ShiftAsOf"/>. Each register adds its own cells and totals.
        /// </summary>
        public TRow NewRow<TRow>(EmployeeSnapshot employee) where TRow : RegisterEmployee, new()
        {
            var shiftSource = PeriodValue(employee.Designations, ShiftAsOf);
            var shift = TimesheetRegisterRules.ResolveShift(shiftSource, out var shiftAssigned);
            TeamLeads.TryGetValue(employee.Id, out var lead);

            return new TRow
            {
                EmployeeId = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                FullName = FullName(employee),
                Email = employee.Email,
                Department = PeriodValue(employee.Departments, ShiftAsOf),
                EmploymentType = LatestValue(employee.EmploymentTypes),
                TeamLeadId = lead?.ManagerId,
                TeamLeadName = lead?.DisplayName,
                Shift = shift.Name,
                ShiftAssigned = shiftAssigned,
                ShiftSource = string.IsNullOrWhiteSpace(shiftSource) ? null : shiftSource.Trim(),
                JoiningDate = Iso(employee.JoiningDate),
                ReleaseDate = Iso(employee.ReleaseDate)
            };
        }

        /// <summary>
        /// The value (shift, department) in force on a date: the row whose start/end period covers it, latest
        /// start first; otherwise the most recent row that has a value at all.
        /// </summary>
        public static string PeriodValue(List<PeriodSnapshot> rows, DateTime asOf)
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

        /// <summary>
        /// The value of the latest record, whatever the period: an employee can have several employment type
        /// records (Trainee, Probation, Permanent) and the latest one is what they are now. Latest = the latest
        /// start date, then the most recently created; null when no record has a value.
        /// </summary>
        public static string LatestValue(List<PeriodSnapshot> rows)
        {
            var latest = rows?
                .Where(r => !string.IsNullOrWhiteSpace(r.Value))
                .OrderByDescending(r => r.StartDate ?? DateTime.MinValue)
                .ThenByDescending(r => r.CreatedOnUtc ?? DateTime.MinValue)
                .FirstOrDefault();

            return latest?.Value.Trim();
        }

        public static string FullName(EmployeeSnapshot employee)
        {
            var name = $"{employee.FirstName} {employee.LastName}".Trim();
            return name.Length > 0 ? name : (employee.EmployeeCode ?? employee.Id);
        }
        #endregion

        #region Time zone and text helpers
        public static TimeZoneInfo ResolveTimeZone(string id)
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
        public static DateTime ToLocal(DateTime utc, TimeZoneInfo tz)
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

        public static string Iso(DateTime? date)
        {
            return date?.ToString(IsoDate, CultureInfo.InvariantCulture);
        }

        public static string IsoMinuteText(DateTime? moment)
        {
            return moment?.ToString(IsoMinute, CultureInfo.InvariantCulture);
        }
        #endregion
    }

    /// <summary>
    /// Loads a <see cref="RegisterContext"/> with four set-based queries: holidays, employees with their
    /// designation and department history, team leads, leaves. Kept internal and created by the register
    /// services themselves, so it adds nothing to the DI container.
    /// </summary>
    internal sealed class RegisterContextLoader
    {
        #region Fields
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<EmployeeLeave> _employeeLeaveRepository;
        private readonly IRepository<LeaveSchedules> _leaveScheduleRepository;
        private readonly IRepository<EmployeeOrgStructure> _employeeOrgStructureRepository;
        #endregion

        #region Ctor
        public RegisterContextLoader(
            IRepository<Employee> employeeRepository,
            IRepository<EmployeeLeave> employeeLeaveRepository,
            IRepository<LeaveSchedules> leaveScheduleRepository,
            IRepository<EmployeeOrgStructure> employeeOrgStructureRepository)
        {
            _employeeRepository = employeeRepository;
            _employeeLeaveRepository = employeeLeaveRepository;
            _leaveScheduleRepository = leaveScheduleRepository;
            _employeeOrgStructureRepository = employeeOrgStructureRepository;
        }
        #endregion

        #region LoadAsync
        /// <param name="fromDate">Site-local calendar date (time ignored); may come after <paramref name="toDate"/>.</param>
        /// <param name="employeeIds">Optional: only these employees. Null or empty = every active employee of the site.</param>
        /// <param name="nowUtc">Overrides "now" (UTC) so a run can be reproduced; null = DateTime.UtcNow.</param>
        public async Task<RegisterContext> LoadAsync(string siteId, string timeZoneId, DateTime fromDate, DateTime toDate, List<string> employeeIds, DateTime? nowUtc)
        {
            if (string.IsNullOrWhiteSpace(siteId)) throw new ArgumentException("Site is required.");

            var from = fromDate.Date;
            var to = toDate.Date;
            if (to < from) (from, to) = (to, from);
            var dayCount = (to - from).Days + 1;
            if (dayCount > TimesheetRegisterRules.MaxRangeDays)
                throw new ArgumentException($"The register covers at most {TimesheetRegisterRules.MaxRangeDays} days per request; narrow the date range.");
            var toExclusive = to.AddDays(1);

            var tz = RegisterContext.ResolveTimeZone(timeZoneId);
            var utcNow = DateTime.SpecifyKind(nowUtc ?? DateTime.UtcNow, DateTimeKind.Utc);
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
            var today = nowLocal.Date;
            var filterEmployees = employeeIds != null && employeeIds.Count > 0;

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
                employeeQuery = employeeQuery.Where(e => employeeIds.Contains(e.Id));

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
                        .ToList(),
                    EmploymentTypes = e.EmployeeType
                        .Where(t => !t.Deleted)
                        .Select(t => new PeriodSnapshot
                        {
                            Value = t.EmployeeTypeDropdown != null ? t.EmployeeTypeDropdown.DropDownValue : null,
                            StartDate = t.StartDate,
                            EndDate = t.EndDate,
                            CreatedOnUtc = t.CreatedOnUtc
                        })
                        .ToList()
                })
                .ToListAsync();

            employees = employees
                .OrderBy(e => e.FirstName ?? string.Empty)
                .ThenBy(e => e.LastName ?? string.Empty)
                .ToList();
            var employeeIdSet = new HashSet<string>(employees.Select(e => e.Id));
            var employeeIdList = employeeIdSet.ToList();

            // 2b. Team lead of each employee: the manager on the latest org-structure row (by year, then creation).
            var orgRows = await _employeeOrgStructureRepository.TableNoTracking
                .Where(o => !o.Deleted && employeeIdList.Contains(o.EmployeeId))
                .Select(o => new TeamLeadSnapshot
                {
                    EmployeeId = o.EmployeeId,
                    ManagerId = o.ManagerId,
                    Year = o.Year,
                    CreatedOnUtc = o.CreatedOnUtc,
                    ManagerFirstName = o.Manager != null && o.Manager.Person != null ? o.Manager.Person.FirstName : null,
                    ManagerLastName = o.Manager != null && o.Manager.Person != null ? o.Manager.Person.LastName : null
                })
                .ToListAsync();

            var teamLeads = orgRows
                .GroupBy(o => o.EmployeeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(o => o.Year).ThenByDescending(o => o.CreatedOnUtc).First());

            // 3. Leaves overlapping the range. Any status that is not declined/cancelled covers the day.
            var leaveQuery = _employeeLeaveRepository.TableNoTracking
                .Where(l => !l.Deleted && l.FromDate < toExclusive && l.ToDate >= from);
            leaveQuery = filterEmployees
                ? leaveQuery.Where(l => employeeIds.Contains(l.EmployeeId))
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
                if (!employeeIdSet.Contains(leave.EmployeeId) || !TimesheetRegisterRules.IsLeaveCounted(leave.Status))
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

            // 4. The days of the period: the columns every register shares.
            var days = new List<RegisterDay>(dayCount);
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                var weeklyOff = TimesheetRegisterRules.IsWeeklyOff(date);
                var isHoliday = holidays.TryGetValue(date, out var title);
                days.Add(new RegisterDay
                {
                    Date = date.ToString(RegisterContext.IsoDate, CultureInfo.InvariantCulture),
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

            return new RegisterContext
            {
                SiteId = siteId,
                TimeZone = tz,
                From = from,
                To = to,
                NowLocal = nowLocal,
                RequestedEmployeeIds = filterEmployees ? employeeIds : null,
                Holidays = holidays,
                Employees = employees,
                EmployeeIds = employeeIdSet,
                TeamLeads = teamLeads,
                Leaves = leaves,
                Days = days
            };
        }

        /// <summary>A full-day leave outranks a half day; an approved leave outranks one still in the workflow.</summary>
        private static bool Outranks(RegisterLeave candidate, RegisterLeave existing)
        {
            var candidateFull = candidate.Kind == TimesheetRegisterRules.FullDayLeaveKind;
            var existingFull = existing.Kind == TimesheetRegisterRules.FullDayLeaveKind;
            if (candidateFull != existingFull) return candidateFull;
            return candidate.IsApproved && !existing.IsApproved;
        }
        #endregion
    }

    #region Query snapshots (what the context queries project into)
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
        public List<PeriodSnapshot> EmploymentTypes { get; set; } = new List<PeriodSnapshot>();
    }

    internal sealed class PeriodSnapshot
    {
        public string Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
    }

    internal sealed class TeamLeadSnapshot
    {
        public string EmployeeId { get; set; }
        public string ManagerId { get; set; }
        public int Year { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string ManagerFirstName { get; set; }
        public string ManagerLastName { get; set; }

        /// <summary>The manager's name; the manager id when the person has no name; null when there is no manager.</summary>
        public string DisplayName
        {
            get
            {
                if (ManagerId == null) return null;
                var name = $"{ManagerFirstName} {ManagerLastName}".Trim();
                return name.Length > 0 ? name : ManagerId;
            }
        }
    }
    #endregion
}
