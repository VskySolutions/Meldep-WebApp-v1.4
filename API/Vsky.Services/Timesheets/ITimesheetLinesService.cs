using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Timesheets
{
    public interface ITimesheetLinesService
    {
        #region GetAllTimesheetLines
        IPagedList<TimesheetLines> GetAllTimesheetLines(string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetTimesheetByTaskId
        Task<List<TimesheetLines>> GetTimesheetByTaskId(string id);
        #endregion

        #region GetTimesheetLinesDetailsById
        Task<TimesheetLines> GetTimesheetLinesDetailsById(string id);
        #endregion

        #region GetTimesheetLinesById
        Task<TimesheetLines> GetTimesheetLinesById(string id);
        #endregion

        #region GetTimesheetLinesByTimesheet
        List<TimesheetLines> GetTimesheetLinesByTimesheet(string TimesheetId);
        #endregion

        #region GetTimesheetLineByTimesheetId
        Task<TimesheetLines> GetTimesheetLineByTimesheetId(string timesheetId);
        #endregion

        #region GetTimesheetLinesByProjectModuleId
        Task<List<TimesheetLines>> GetTimesheetLinesByProjectModuleId(string ProjectModuleId);
        #endregion

        #region GetTimesheetLinesByProjectModuleIdForMoveModuleAsProject
        Task<List<TimesheetLines>> GetTimesheetLinesByProjectModuleIdForMoveModuleAsProject(string ProjectModuleId);
        #endregion

        #region GetTimesheetLinesDetailsByIds
        Task<List<TimesheetLines>> GetTimesheetLinesDetailsByIds(string[] ids);
        #endregion

        #region InsertTimesheetLines
        void InsertTimesheetLines(TimesheetLines entity);
        #endregion

        #region UpdateTimesheetLines
        void UpdateTimesheetLines(TimesheetLines entity);
        #endregion

        #region DeleteTimesheetLines
        void DeleteTimesheetLines(TimesheetLines entity);
        #endregion

        #region InsertTimesheetLinesList
        void InsertTimesheetLinesList(IList<TimesheetLines> entities);
        #endregion

        #region UpdateTimesheetLinesList
        void UpdateTimesheetLinesList(IList<TimesheetLines> entities);
        #endregion

        #region DeleteTimesheetLinesList
        void DeleteTimesheetLinesList(List<TimesheetLines> entities);
        #endregion

        #region GetTimesheetLinesDetailsByMeetingUId
        Task<TimesheetLines> GetTimesheetLinesDetailsByMeetingUId(string meetingUId);
        #endregion
    }
}

