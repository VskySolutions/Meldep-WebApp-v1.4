using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Timesheets
{
    public interface ITimesheetService
    {
        #region GetAllTimesheets
        IPagedList<Timesheet> GetAllTimesheets(
            string SiteId, 
            string createdBy, 
            string SearchText, 
            string employeeId, 
            string projectId, 
            string projectModuleId, 
            string projectTaskId, 
            string projectActivityId, 
            DateTime? activityDate, 
            DateTime? fromDate, 
            DateTime? toDate,
            bool thisWeek,
            int lastNumberOfWeeks,
            string sortBy, 
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue, 
            bool lookup = false
        );

        IPagedList<TimesheetLines> GetAllBillingTimesheets(
            string SiteId, 
            string SearchText, 
            DateTime? fromDate, 
            DateTime? toDate, 
            string projectId, 
            List<string> projectModuleIds, 
            List<string> projectTaskIds, 
            List<string> customerIds, 
            List<string> companyContactIds, 
            string sortBy, 
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue, 
            bool lookup = false
        );

        IPagedList<TimesheetLines> GetGroupedBillingTimesheetsByEmployee(
            string SiteId, 
            string SearchText, 
            DateTime? fromDate, 
            DateTime? toDate, 
            string projectId, 
            List<string> projectModuleIds, 
            List<string> projectTaskIds, 
            List<string> customerIds, 
            List<string> companyContactIds, 
            string sortBy, bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue, 
            bool lookup = false
        );

        Task<List<Timesheet>> GetAllTimesheetByWeek(string siteId, string employeeId, DateTime? fromDate, DateTime? toDate);
        #endregion

        #region GetTimesheetById
        Task<Timesheet> GetTimesheetById(string id);
        #endregion

        #region GetAllTimesheetListForDropdown
        Task<List<Timesheet>> GetAllTimesheetListForDropdown(string SiteId);
        #endregion

        #region GetTimesheetDetailsById
        Task<Timesheet> GetTimesheetDetailsById(string id);
        #endregion

        #region GetTimesheetByDate
        Task<Timesheet> GetTimesheetByDate(string SiteId, string LoggedUserId, DateTime? TimesheetDate, string id = null);
        #endregion

        #region InsertTimesheet
        void InsertTimesheet(Timesheet entity);
        #endregion

        #region UpdateTimesheet
        void UpdateTimesheet(Timesheet entity);
        #endregion

        #region DeleteTimesheet
        void DeleteTimesheet(Timesheet entity);
        #endregion

        IPagedList<Timesheet> GetAllTimesheetsForDashboard(string SiteId, string createdBy, string employeeId, string projectId, string projectModuleId, string projectTaskId, string projectActivityId, DateTime? activityDate, DateTime? fromDate, DateTime? toDate, string sortBy, bool descending, int page = 1, int pageSize = 5, bool lookup = false);

    }
}
