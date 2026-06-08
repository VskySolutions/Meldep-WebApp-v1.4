using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectWeeklyPlan
{
    public interface IProjectWeeklyService
    {
        #region GetById
        Task<Models.ProjectWeeklyPlan> GetById(string SiteId, string Id);
        Task<Models.ProjectWeeklyPlan> GetByProjectId(string SiteId, string ProjectId);
        #endregion

        #region GetAllProjectWeeklyPlanList
        Task<IPagedList<Models.ProjectWeeklyPlan>> GetAllProjectWeeklyPlanListAsync(
            string SiteId,
            string LoggedUserId,
            DateTime GetDateTime,
            string PlanTypeId,
            string SearchText,
            List<string> ProjectIds,
            List<string> ProjectCoordinatorIds,
            List<string> ProjectLeadsIds,
            List<string> ProjectStatusIds,
            int Status,
            List<string> ProjectPriorityIds,
            List<string> ProjectTypeIds,
            List<string> CustomerIds,
            List<string> CompanyContactIds,
            string SortBy,
            bool Descending,
            int PageIndex = 1,
            int PageSize = int.MaxValue);
        #endregion

        #region InsertProjectWeeklyPlan
        void InsertProjectWeeklyPlan(Models.ProjectWeeklyPlan entity);
        #endregion

        #region UpdateProjectWeeklyPlan
        void UpdateProjectWeeklyPlan(Models.ProjectWeeklyPlan entity);
        #endregion

        #region DeleteProjectWeeklyPlan
        void DeleteProjectWeeklyPlan(Models.ProjectWeeklyPlan entity);
        #endregion
    }
}
