using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.TimeInTimeOuts
{
    public interface ITimeInTimeOutBreakDetailService
    {
        //#region GetAllTimeInTimeOutBreakDetails
        //IPagedList<TimeInTimeOutBreakDetail> GetAllTimeInTimeOutBreakDetails(string createdBy, string employeeId, string projectId, string projectModuleId, string projectTaskId, string projectActivityId, DateTime? activityDate, DateTime? fromDate, DateTime? toDate, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        //#endregion

        #region GetTimeInTimeOutBreakById
        Task<TimeInTimeOutBreakDetail> GetTimeInTimeOutBreakById(string id);
        #endregion

        //#region GetAllTimesheetListForDropdown
        //Task<List<Timesheet>> GetAllTimesheetListForDropdown();
        //#endregion

        #region GetBreakInOutByTimeInOutId
        Task<List<TimeInTimeOutBreakDetail>> GetBreakInOutByTimeInOutId(string timeInTimeOutId);
        #endregion

        #region GetTimeInTimeOutBreakDetailByTimeInTimeOutId
        Task<TimeInTimeOutBreakDetail> GetTimeInTimeOutBreakDetailByTimeInTimeOutId(string timeInTimeOutId, string id = null);
        #endregion

        #region InsertTimeInTimeOutBreakDetail
        void InsertTimeInTimeOutBreakDetail(TimeInTimeOutBreakDetail entity);
        #endregion

        #region UpdateTimeInTimeOutBreakDetail
        void UpdateTimeInTimeOutBreakDetail(TimeInTimeOutBreakDetail entity);
        #endregion

        #region DeleteTimeInTimeOutBreakDetail
        void DeleteTimeInTimeOutBreakDetail(TimeInTimeOutBreakDetail entity);
        #endregion

        #region InsertTimeInTimeOutBreakDetailList
        void InsertTimeInTimeOutBreakDetailList(IList<TimeInTimeOutBreakDetail> entities);
        #endregion

        #region UpdateTimeInTimeOutBreakDetailList
        void UpdateTimeInTimeOutBreakDetailList(IList<TimeInTimeOutBreakDetail> entities);
        #endregion

        #region DeleteTimeInTimeOutBreakDetailList
        void DeleteTimeInTimeOutBreakDetailList(List<TimeInTimeOutBreakDetail> entities);
        #endregion
    }
}
