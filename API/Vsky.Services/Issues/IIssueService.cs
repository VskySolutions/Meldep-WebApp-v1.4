using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Issues
{
    public interface IIssueService
    {
        #region GetAllIssues
        Task<IPagedList<Issue>> GetAllIssues(string SiteId, string LoggedUserId, string SearchText, int issueNumber,List<string> projectIds, List<string> projectModuleIds , string name, List<string> priorityIds, List<string> statusIds, List<string> issueTypeIds, List<string> employeeIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        IPagedList<Issue> GetAllIssuesForDashboard(string SiteId, string projectId, string targetMonthStr, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        List<VWProjectIssueStatusSummary> GetIssueStatusSummaryByProjectIds(List<string> projectIds);
        #endregion

        #region GetIssueById
        Task<Issue> GetIssueById(string id);
        #endregion

        #region GetLastIssueNumber
        Task<int> GetLastIssueNumber();
        #endregion

        #region GetIssueByName
        Task<Issue> GetIssueByName(string siteId, string name, string ProjectId = null, string id = null);
        #endregion

        //#region GetTestPlanByProjectId
        //Task<TestPlan> GetTestPlanByProjectId(string ProjectId, string id = null);
        //#endregion

        //#region GetAllIssuesListForDropdown
        //Task<List<Issue>> GetAllTestPlansListForDropdown(string projectId = null);
        //#endregion

        #region GetIssueDetailsById
        Task<Issue> GetIssueDetailsById(string id);
        #endregion

        #region InsertIssue
        void InsertIssue(Issue entity);
        #endregion

        #region UpdateIssue
        void UpdateIssue(Issue entity);
        #endregion

        #region DeleteIssue
        void DeleteIssue(Issue entity);
        #endregion

    }
}

