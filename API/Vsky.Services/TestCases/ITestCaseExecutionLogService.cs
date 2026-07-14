using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.TestCases
{
    public interface ITestCaseExecutionLogExecutionLogService
    {
        #region GetAllTestCaseExecutionLogs
        Task<IPagedList<TestCaseExecutionLog>> GetAllTestCaseExecutionLogs(string SiteId, string LoggedUserId, string SearchText, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetTestCaseExecutionLogById
        Task<TestCaseExecutionLog> GetTestCaseExecutionLogById(string id);
        #endregion
        Task<List<TestCaseExecutionLog>> GetTestCaseExecutionLogsByMappingId(string mappingId);

        #region GetTestCaseExecutionLogDetailsById
        Task<TestCaseExecutionLog> GetTestCaseExecutionLogDetailsById(string id);
        #endregion

        #region InsertTestCaseExecutionLog
        void InsertTestCaseExecutionLog(TestCaseExecutionLog entity);
        #endregion

        #region UpdateTestCaseExecutionLog
        void UpdateTestCaseExecutionLog(TestCaseExecutionLog entity);
        #endregion

        #region DeleteTestCaseExecutionLog
        void DeleteTestCaseExecutionLog(TestCaseExecutionLog entity);
        #endregion
    }
}
