using System.Collections.Generic;

namespace Vsky.Api.Models
{
    #region Timesheet Register
    /// <summary>
    /// Request of the Timesheet Register. Dates are calendar dates as text ("yyyy-MM-dd" or "MM/dd/yyyy") so
    /// they never pass through a time-zone conversion.
    /// </summary>
    public record TimesheetRegisterSearchModel
    {
        public string FromDate { get; set; }

        public string ToDate { get; set; }

        /// <summary>Optional: only these employees. Empty = every active employee of the site.</summary>
        public List<string> EmployeeIds { get; set; }
    }
    #endregion

    #region TimeInTimeOut-MovementRegister (Attendance Register)
    /// <summary>
    /// Request of the Attendance Register (TimeInTimeOut + Movement Register). Dates are calendar dates as text
    /// ("yyyy-MM-dd" or "MM/dd/yyyy") so they never pass through a time-zone conversion.
    /// </summary>
    public record AttendanceRegisterSearchModel
    {
        public string FromDate { get; set; }

        public string ToDate { get; set; }

        /// <summary>Optional: only these employees. Empty = every active employee of the site.</summary>
        public List<string> EmployeeIds { get; set; }
    }
    #endregion
}
