using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.EmployeeLeaves
{
    public interface IEmployeeLeaveService
    {
        #region GetAllEmployeeLeave
        IPagedList<EmployeeLeave> GetAllEmployeeLeave(string SiteId, string logginuser, string createdBy, string SearchText, string Flag, List<string> employeeIds, List<string> statusIds, List<string> leaveCategoryId, DateTime? createdOnUtc, string leaveMonthStr, int years, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetFiveEmployeeLeaveForApprove
        Task<List<EmployeeLeave>> GetFiveEmployeeLeaveForApprove(string SiteId, string employeeId, string leavestatus, string activeEmployeeStatus, string exEmployeeStatus);
        #endregion

        #region GetEmployeeLeaveListForDashboard
        Task<List<EmployeeLeave>> GetEmployeeLeaveListForDashboard(string SiteId, DateTime GetDateTime);
        Task<List<EmployeeLeave>> GetEmployeeLeaveListForMovReg(string siteId, string dateStr, DateTime GetDateTime);
        #endregion

        #region GetAllEmployeeLeaveForApprove
        IPagedList<EmployeeLeave> GetAllEmployeeLeaveForApprove
        (
            string SiteId, 
            string employeeId, 
            string SearchText, 
            List<string> personIds,
            List<string> statusIds,
            DateTime? createdOnUtc, 
            string leaveMonthStr, 
            int years,
            List<string> leaveCategoryId,
            string sortBy,
            bool descending, 
            int page = 1, int
            pageSize = int.MaxValue,
            bool lookup = false
       );
        #endregion

        #region Get usedLeave By EmployeeId
        decimal GetUsedLeaveByEmployeeId(string employeeId, int year);
        #endregion

        #region GetEmployeeLeaveById
        Task<EmployeeLeave> GetEmployeeLeaveById(string id);
        #endregion

        #region GetEmployeeLeaveByEmployeeId
        Task<List<EmployeeLeave>> GetEmployeeLeaveByEmployeeId(string employeeId);
        #endregion

        #region GetUsedLeaveByEmployeeIdAndLeaveCategoryId
        decimal GetUsedLeaveByEmployeeIdAndLeaveCategoryId(string employeeId, int year, string leaveCategoryId, string id = null);
        #endregion

        #region GetEmployeeLeaveDetailsById
        Task<EmployeeLeave> GetEmployeeLeaveDetailsById(string id);
        #endregion

        #region GetAllEmployeeLeaves
        //Task<HashSet<DateTime>> GetAllOfficeLeaves(string siteId, DateTime startDate, DateTime endDate);
        Task<List<EmployeeLeave>> GetAllEmployeeLeaves(string employeeId);
        Task<List<EmployeeLeave>> GetEmployeeLeavesThatIncludeDates(string employeeId, List<DateTime> targetDates, string id = null);
        #endregion

        #region IsSandwichLeave
        Task<bool> CheckPreviousHoliday(string SiteId, string employeeId, DateTime startDate);
        //Task<bool> CheckNextHoliday(string SiteId, string employeeId, DateTime endDate);
        Task<SandwichLeaveResult> IsSandwichLeave(string SiteId, string EmployeeId, DateTime startDate, DateTime endDate);
        #endregion

        #region InsertEmployeeLeave
        void InsertEmployeeLeave(EmployeeLeave entity);
        #endregion

        #region UpdateEmployeeLeave
        void UpdateEmployeeLeave(EmployeeLeave entity);
        #endregion

        #region DeleteEmployeeLeave
        void DeleteEmployeeLeave(EmployeeLeave entity);
        #endregion
    }
}