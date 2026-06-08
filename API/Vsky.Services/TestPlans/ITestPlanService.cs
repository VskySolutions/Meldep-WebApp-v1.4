using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.TestPlans
{
    public interface ITestPlanService
    {
        #region GetAllTestPlans
        Task<IPagedList<TestPlan>> GetAllTestPlans(string SiteId, string LoggedUserId, string SearchText, int testPlanNumber, List<string> projectIds, string name, List<string> planMakerIds, List<string> planReviewerIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        IPagedList<TestPlan> GetAllTestPlanForDashboard(string SiteId, string projectId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetTestPlanById
        Task<TestPlan> GetTestPlanById(string id);
        #endregion

        #region GetLastTestPlanNumber
        Task<int> GetLastTestPlanNumber();
        #endregion

        #region GetTestPlanByName
        Task<TestPlan> GetTestPlanByName(string SiteId, string name, string ProjectId = null, string id = null);
        #endregion

        //#region GetTestPlanByProjectId
        //Task<TestPlan> GetTestPlanByProjectId(string ProjectId, string id = null);
        //#endregion

        #region GetAllTestPlansListForDropdown
        Task<List<CommonDropDown>> GetAllTestPlansListForDropdown(string SiteId, string projectId = null);
        #endregion

        #region GetTestPlanDetailsById
        Task<TestPlan> GetTestPlanDetailsById(string id);
        #endregion

        #region InsertTestPlan
        void InsertTestPlan(TestPlan entity);
        #endregion

        #region UpdateTestPlan
        void UpdateTestPlan(TestPlan entity);
        #endregion

        #region DeleteTestPlan
        void DeleteTestPlan(TestPlan entity);
        #endregion
    }
}
