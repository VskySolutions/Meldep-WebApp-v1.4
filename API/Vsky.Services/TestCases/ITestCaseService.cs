using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.TestCases
{
    public interface ITestCaseService
    {
        #region GetAllTestCases
        Task<IPagedList<TestCase>> GetAllTestCases(string SiteId, string LoggedUserId, string SearchText, int testCaseNumber, List<string> projectIds, List<string> planIds, List<string> testedBys, List<string> statusIds, DateTime? fromDate, DateTime? toDate, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        IPagedList<TestCase> GetAllTestCasesForDashboard(string SiteId, string projectId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetTestCaseById
        Task<TestCase> GetTestCaseById(string id);
        #endregion

        #region GetLastTestCaseNumber
        Task<int> GetLastTestCaseNumber();
        #endregion

        //#region GetTestCaseByPlanId
        //Task<TestCase> GetTestCaseByPlanId(string PlanId, string id = null);
        //#endregion

        #region GetAllTestCasesListForDropdown
        Task<List<TestCase>> GetAllTestCasesListForDropdown(string SiteId);
        #endregion

        #region GetTestCaseDetailsById
        Task<TestCase> GetTestCaseDetailsById(string id);
        #endregion

        #region InsertTestCase
        void InsertTestCase(TestCase entity);
        #endregion

        #region UpdateTestCase
        void UpdateTestCase(TestCase entity);
        #endregion

        #region DeleteTestCase
        void DeleteTestCase(TestCase entity);
        #endregion
    }
}
