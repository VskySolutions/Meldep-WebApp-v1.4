using System;
using System.Threading.Tasks;
using Vsky.Models.Analytics;

namespace Vsky.Services.Analytics
{
    /// <summary>
    /// The Attendance Register: TimeInTimeOut and the Movement Register (MovementRegister +
    /// MovementRegisterDetails) merged into one view. Every active employee of a site against every day of a
    /// range, each day judged on time / late / short hours / no time-out / absent / work from home / leave /
    /// holiday by <see cref="AttendanceRegisterRules"/>, with the day's breaks, time adjustments and work from
    /// home carried on the cell.
    /// </summary>
    public interface IAttendanceRegisterService
    {
        /// <summary>The full matrix for the range (at most <see cref="TimesheetRegisterRules.MaxRangeDays"/> days).</summary>
        Task<AttendanceRegisterResult> GetRegisterAsync(AttendanceRegisterQuery query);

        /// <summary>One employee on one day, with the time-ins and the Movement Register entries behind the cell.</summary>
        Task<AttendanceRegisterDayDetail> GetDayDetailAsync(string siteId, string timeZoneId, string employeeId, DateTime date, DateTime? nowUtc = null);

        /// <summary>One employee over a range (at most <see cref="TimesheetRegisterRules.MaxRangeDays"/> days): the register row, the days and every record behind them.</summary>
        Task<AttendanceRegisterEmployeeDetail> GetEmployeeDetailAsync(string siteId, string timeZoneId, string employeeId, DateTime fromDate, DateTime toDate, DateTime? nowUtc = null);
    }
}
