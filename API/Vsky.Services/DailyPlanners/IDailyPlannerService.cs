using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.DailyPlanners
{
    public interface IDailyPlannerService
    {
        #region GetAllDailyPlanner
        IPagedList<DailyPlanner> GetAllDailyPlanner(string SiteId, string createdBy, string SearchText, string employeeId, string projectId, DateTime? activityDate, DateTime? fromDate, DateTime? toDate, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetDailyPlannerDetailsById
        Task<DailyPlanner> GetDailyPlannerDetailsById(string id);
        #endregion

        #region GetDailyPlannerById
        Task<DailyPlanner> GetDailyPlannerById(string id);
        #endregion

        #region GetDailyPlannerByProject
        Task<DailyPlanner> GetDailyPlannerByDate(string SiteId, string LoggedUserId, DateTime? DailyPlannerDate, string id = null);
        #endregion

        #region InsertDailyPlanner
        void InsertDailyPlanner(DailyPlanner entity);
        #endregion

        #region UpdateDailyPlanner
        void UpdateDailyPlanner(DailyPlanner entity);
        #endregion

        #region DeleteDailyPlanner
        void DeleteDailyPlanner(DailyPlanner entity);
        #endregion

        IPagedList<DailyPlanner> GetAllDailyPlannerDashboard(string SiteId, string createdBy, string employeeId, string projectId, DateTime? activityDate, DateTime? fromDate, DateTime? toDate, string sortBy, bool descending, int page = 1, int pageSize = 5, bool lookup = false);

    }
}
