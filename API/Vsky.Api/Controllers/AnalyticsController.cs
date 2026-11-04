using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Models.Analytics;
using Vsky.Services.Analytics;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    /// <summary>
    /// Analytics pages are reached by URL and are for administrators only: a logged-in user (BaseController's
    /// [Authorize]) who holds the admin, site-super-admin or system-super-admin role. The JWT carries role
    /// names as typed in AspNetRoles ("Site Super Admin"), so the check normalises them instead of relying on
    /// [Authorize(Roles = ...)]'s exact, case-sensitive match.
    /// </summary>
    [Route("Analytics")]
    public class AnalyticsController : BaseController
    {
        private static readonly HashSet<string> AdminRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "admin",
            "site-super-admin",
            "system-super-admin"
        };

        private static readonly string[] DateFormats = { "yyyy-MM-dd", "MM/dd/yyyy", "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss.FFFK" };

        #region Fields
        private readonly ITimesheetRegisterService _timesheetRegisterService;
        private readonly ISiteService _siteService;
        private readonly GlobalVariable _globalVariable;
        #endregion

        #region Ctor
        public AnalyticsController(
            ITimesheetRegisterService timesheetRegisterService,
            ISiteService siteService,
            GlobalVariable globalVariable)
        {
            _timesheetRegisterService = timesheetRegisterService;
            _siteService = siteService;
            _globalVariable = globalVariable;
        }
        #endregion

        #region GetTimesheetRegister
        /// <summary>Every active employee of the site against every day of the range, each day classified.</summary>
        [HttpPost("timesheet-register")]
        public async Task<IActionResult> GetTimesheetRegister(TimesheetRegisterSearchModel model)
        {
            try
            {
                if (!IsAdmin()) return AdminOnly();

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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetTimesheetRegisterDay
        /// <summary>One employee on one day, with the timesheet lines behind the cell.</summary>
        [HttpGet("timesheet-register/day")]
        public async Task<IActionResult> GetTimesheetRegisterDay([FromQuery] string employeeId, [FromQuery] string date)
        {
            try
            {
                if (!IsAdmin()) return AdminOnly();

                if (string.IsNullOrWhiteSpace(employeeId) || !TryParseDate(date, out var day))
                    return BadRequest("employeeId and date (yyyy-MM-dd) are required.");

                var siteId = _globalVariable.SiteId;
                var result = await _timesheetRegisterService.GetDayDetailAsync(siteId, await ResolveTimeZoneIdAsync(siteId), employeeId, day);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Helpers
        private bool IsAdmin()
        {
            var roles = User?.Claims?
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
                .Select(c => NormalizeRole(c.Value));

            return roles != null && roles.Any(r => AdminRoles.Contains(r));
        }

        /// <summary>"Site Super Admin" → "site-super-admin", the normalised form the web app compares against.</summary>
        private static string NormalizeRole(string role)
        {
            return string.IsNullOrWhiteSpace(role) ? string.Empty : Regex.Replace(role.Trim().ToLowerInvariant(), @"[\s_]+", "-");
        }

        private IActionResult AdminOnly()
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Analytics is available to administrators only.");
        }

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
