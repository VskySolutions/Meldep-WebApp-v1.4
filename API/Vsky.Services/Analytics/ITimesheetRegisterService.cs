using System;
using System.Threading.Tasks;
using Vsky.Models.Analytics;

namespace Vsky.Services.Analytics
{
    /// <summary>
    /// The Timesheet Register: every active employee of a site against every day of a range, each day judged
    /// on time / delayed / partial / pending / leave / holiday by <see cref="TimesheetRegisterRules"/>.
    /// </summary>
    public interface ITimesheetRegisterService
    {
        /// <summary>The full matrix for the range (at most <see cref="TimesheetRegisterRules.MaxRangeDays"/> days).</summary>
        Task<TimesheetRegisterResult> GetRegisterAsync(TimesheetRegisterQuery query);

        /// <summary>One employee on one day, with the timesheet lines behind the cell.</summary>
        Task<TimesheetRegisterDayDetail> GetDayDetailAsync(string siteId, string timeZoneId, string employeeId, DateTime date, DateTime? nowUtc = null);
    }
}
