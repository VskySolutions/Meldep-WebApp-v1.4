using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Authorization;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Models.Analytics;
using Vsky.Services.Analytics;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    /// <summary>
    /// Each register is its own menu, so access is granted per register through the permission screens
    /// (role, security group or individual user, per site): the Timesheet Register actions need
    /// MenuLinks.TimesheetRegister and the Attendance Register actions need MenuLinks.AttendanceRegister.
    /// Every action only reads, so each needs View; the two POST searches are marked [ReadAccess].
    /// One region per register: "Timesheet Register" and "TimeInTimeOut-MovementRegister" (the Attendance
    /// Register); the helpers at the end serve both.
    /// </summary>
    [Route("Analytics")]
    public class AnalyticsController : BaseController
    {
        private static readonly string[] DateFormats = { "yyyy-MM-dd", "MM/dd/yyyy", "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss.FFFK" };

        #region Fields
        private readonly ITimesheetRegisterService _timesheetRegisterService;
        private readonly IAttendanceRegisterService _attendanceRegisterService;
        private readonly ISiteService _siteService;
        private readonly GlobalVariable _globalVariable;
        #endregion

        #region Ctor
        public AnalyticsController(
            ITimesheetRegisterService timesheetRegisterService,
            IAttendanceRegisterService attendanceRegisterService,
            ISiteService siteService,
            GlobalVariable globalVariable)
        {
            _timesheetRegisterService = timesheetRegisterService;
            _attendanceRegisterService = attendanceRegisterService;
            _siteService = siteService;
            _globalVariable = globalVariable;
        }
        #endregion

        #region Timesheet Register

        #region GetTimesheetRegister
        /// <summary>Every active employee of the site against every day of the range, each day classified.</summary>
        [HttpPost("timesheet-register")]
        [RequireMenu(MenuLinks.TimesheetRegister)]
        [ReadAccess] // A search: a POST only for its body, so View is enough and it isn't logged as a change.
        public async Task<IActionResult> GetTimesheetRegister(TimesheetRegisterSearchModel model)
        {
            try
            {
                if (model == null || !TryParseDate(model.FromDate, out var fromDate) || !TryParseDate(model.ToDate, out var toDate))
                    return BadRequest("From date and To date are required (yyyy-MM-dd).");

                var siteId = _globalVariable.SiteId;
                var result = await _timesheetRegisterService.GetRegisterAsync(new TimesheetRegisterQuery
                {
                    SiteId = siteId,
                    TimeZoneId = await ResolveTimeZoneIdAsync(siteId),
                    FromDate = fromDate,
                    ToDate = toDate,
                    EmployeeIds = model.EmployeeIds
                });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // The register services raise these on purpose, with messages written for the user.
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetTimesheetRegisterDay
        /// <summary>One employee on one day, with the timesheet lines behind the cell.</summary>
        [HttpGet("timesheet-register/day")]
        [RequireMenu(MenuLinks.TimesheetRegister)]
        public async Task<IActionResult> GetTimesheetRegisterDay([FromQuery] string employeeId, [FromQuery] string date)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId) || !TryParseDate(date, out var day))
                    return BadRequest("employeeId and date (yyyy-MM-dd) are required.");

                var siteId = _globalVariable.SiteId;
                var result = await _timesheetRegisterService.GetDayDetailAsync(siteId, await ResolveTimeZoneIdAsync(siteId), employeeId, day);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // The register services raise these on purpose, with messages written for the user.
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetTimesheetRegisterEmployee
        /// <summary>One employee over a period (at most a quarter): the register row, the days and every line.</summary>
        [HttpGet("timesheet-register/employee")]
        [RequireMenu(MenuLinks.TimesheetRegister)]
        public async Task<IActionResult> GetTimesheetRegisterEmployee([FromQuery] string employeeId, [FromQuery] string fromDate, [FromQuery] string toDate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId) || !TryParseDate(fromDate, out var from) || !TryParseDate(toDate, out var to))
                    return BadRequest("employeeId, fromDate and toDate (yyyy-MM-dd) are required.");

                var siteId = _globalVariable.SiteId;
                var result = await _timesheetRegisterService.GetEmployeeDetailAsync(siteId, await ResolveTimeZoneIdAsync(siteId), employeeId, from, to);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // The register services raise these on purpose, with messages written for the user.
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #endregion

        #region TimeInTimeOut-MovementRegister (Attendance Register)

        #region GetAttendanceRegister
        /// <summary>
        /// TimeInTimeOut and the Movement Register merged: every active employee of the site against every day of
        /// the range, each day judged (on time, late, short hours, no time-out, absent, work from home, leave,
        /// holiday) and carrying its breaks, time adjustments and work from home.
        /// </summary>
        [HttpPost("attendance-register")]
        [RequireMenu(MenuLinks.AttendanceRegister)]
        [ReadAccess] // A search: a POST only for its body, so View is enough and it isn't logged as a change.
        public async Task<IActionResult> GetAttendanceRegister(AttendanceRegisterSearchModel model)
        {
            try
            {
                if (model == null || !TryParseDate(model.FromDate, out var fromDate) || !TryParseDate(model.ToDate, out var toDate))
                    return BadRequest("From date and To date are required (yyyy-MM-dd).");

                var siteId = _globalVariable.SiteId;
                var result = await _attendanceRegisterService.GetRegisterAsync(new AttendanceRegisterQuery
                {
                    SiteId = siteId,
                    TimeZoneId = await ResolveTimeZoneIdAsync(siteId),
                    FromDate = fromDate,
                    ToDate = toDate,
                    EmployeeIds = model.EmployeeIds
                });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // The register services raise these on purpose, with messages written for the user.
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetAttendanceRegisterDay
        /// <summary>One employee on one day, with the time-ins and the Movement Register entries behind the cell.</summary>
        [HttpGet("attendance-register/day")]
        [RequireMenu(MenuLinks.AttendanceRegister)]
        public async Task<IActionResult> GetAttendanceRegisterDay([FromQuery] string employeeId, [FromQuery] string date)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId) || !TryParseDate(date, out var day))
                    return BadRequest("employeeId and date (yyyy-MM-dd) are required.");

                var siteId = _globalVariable.SiteId;
                var result = await _attendanceRegisterService.GetDayDetailAsync(siteId, await ResolveTimeZoneIdAsync(siteId), employeeId, day);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // The register services raise these on purpose, with messages written for the user.
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #region GetAttendanceRegisterEmployee
        /// <summary>One employee over a period (at most a quarter): the register row, the days, every time-in and every Movement Register entry.</summary>
        [HttpGet("attendance-register/employee")]
        [RequireMenu(MenuLinks.AttendanceRegister)]
        public async Task<IActionResult> GetAttendanceRegisterEmployee([FromQuery] string employeeId, [FromQuery] string fromDate, [FromQuery] string toDate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId) || !TryParseDate(fromDate, out var from) || !TryParseDate(toDate, out var to))
                    return BadRequest("employeeId, fromDate and toDate (yyyy-MM-dd) are required.");

                var siteId = _globalVariable.SiteId;
                var result = await _attendanceRegisterService.GetEmployeeDetailAsync(siteId, await ResolveTimeZoneIdAsync(siteId), employeeId, from, to);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // The register services raise these on purpose, with messages written for the user.
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
            }
        }
        #endregion

        #endregion

        #region Helpers
        private static bool TryParseDate(string text, out DateTime date)
        {
            var value = (text ?? string.Empty).Trim();
            if (DateTime.TryParseExact(value, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out date))
                return true;

            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        /// <summary>The site's time zone: the tenant header first, then the site record; the service falls back to IST.</summary>
        private async Task<string> ResolveTimeZoneIdAsync(string siteId)
        {
            if (!string.IsNullOrWhiteSpace(_globalVariable.TimeZone))
                return _globalVariable.TimeZone;

            if (string.IsNullOrWhiteSpace(siteId))
                return null;

            var site = await _siteService.GetById(siteId);
            return site?.TimeZone;
        }
        #endregion
    }
}
