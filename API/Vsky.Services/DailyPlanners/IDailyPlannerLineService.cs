using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.DailyPlanners
{
    public interface IDailyPlannerLineService
    {
        #region GetAllDailyPlannerLine
        IPagedList<DailyPlannerLine> GetAllDailyPlannerLine(string SiteId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetDailyPlannerLineDetailsById
        Task<DailyPlannerLine> GetDailyPlannerLineDetailsById(string id);
        #endregion

        #region GetDailyPlannerLineById
        Task<DailyPlannerLine> GetDailyPlannerLineById(string id);
        #endregion

        #region GetDailyPlannerLineByProject
        List<DailyPlannerLine> GetDailyPlannerLineByDailyPlanner(string DailyPlannerId);
        #endregion

        #region GetDailyPlannerLineByProjectModuleId
        Task<List<DailyPlannerLine>> GetDailyPlannerLineByProjectModuleId(string ProjectModuleId);
        #endregion

        #region GetAllDailyPlannerLineByProjectModuleIdForMoveModuleAsProject
        Task<List<DailyPlannerLine>> GetAllDailyPlannerLineByProjectModuleIdForMoveModuleAsProject(string ProjectModuleId);
        #endregion

        #region InsertDailyPlannerLine
        void InsertDailyPlannerLine(DailyPlannerLine entity);
        #endregion

        #region UpdateDailyPlannerLine
        void UpdateDailyPlannerLine(DailyPlannerLine entity);
        #endregion

        #region DeleteDailyPlannerLine
        void DeleteDailyPlannerLine(DailyPlannerLine entity);
        #endregion

        #region InsertDailyPlannerLineList
        void InsertDailyPlannerLineList(IList<DailyPlannerLine> entities);
        #endregion

        #region UpdateDailyPlannerLineList
        void UpdateDailyPlannerLineList(IList<DailyPlannerLine> entities);
        #endregion

        #region DeleteDailyPlannerLineList
        void DeleteDailyPlannerLineList(List<DailyPlannerLine> entities);
        #endregion
    }
}
