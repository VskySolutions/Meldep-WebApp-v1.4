using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.TimeInTimeOuts
{
    public interface ITimeInTimeOutService
    {
        #region GetAllTimeInTimeOuts
        IPagedList<TimeInTimeOut> GetAllTimeInTimeOuts(
            string siteId,
            string createdBy,
            string employeeId,
            string searchText,
            DateTime? fromDate,
            DateTime? toDate,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
            );
        #endregion

        #region GetTimeInTimeOutById
        Task<TimeInTimeOut> GetTimeInTimeOutById(string id);
        #endregion

        #region GetTimeInTimeOutByEmployeeId
        Task<TimeInTimeOut> GetTimeInTimeOutByEmployeeId(string siteId, string employeeId);
        #endregion


        #region GetTimeInTimeOutDetailsById
        Task<TimeInTimeOut> GetTimeInTimeOutDetailsById(string id);
        #endregion

        #region InsertTimeInTimeOut
        void InsertTimeInTimeOut(TimeInTimeOut entity);
        #endregion

        #region UpdateTimeInTimeOut
        void UpdateTimeInTimeOut(TimeInTimeOut entity);
        #endregion

        #region DeleteTimeInTimeOut
        void DeleteTimeInTimeOut(TimeInTimeOut entity);
        #endregion
    }
}
