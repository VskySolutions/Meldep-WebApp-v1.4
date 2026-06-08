using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.LeaveRule
{
    public interface ILeaveRulesService
    {
        #region GetAllLeaveRules
        IPagedList<LeaveRules> GetAllLeaveRules(string SiteId, string SearchText, List<int> years, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetLeaveRulesById
        Task<LeaveRules> GetLeaveRulesById(string id);
        #endregion

        #region GetLeaveRulesByYear
        Task<LeaveRules> GetLeaveRulesByYear(string SiteId, int year);
        #endregion

        //#region GetLeaveCreditByEmployeeId
        //Task<LeaveRules> GetLeaveCreditByEmployeeId(string EmployeeId, string id = null);
        //#endregion

        //#region GetAllEmployeeListForDropdown
        //Task<List<LeaveCredit>> GetAllEmployeeListForDropdown();
        //#endregion

        #region GetLeaveRulesDetailsById
        Task<LeaveRules> GetLeaveRulesDetailsById(string id);
        #endregion

        #region InsertLeaveRules
        void InsertLeaveRules(LeaveRules entity);
        #endregion

        #region UpdateLeaveRules
        void UpdateLeaveRules(LeaveRules entity);
        #endregion

        #region DeleteLeaveRules
        void DeleteLeaveRules(LeaveRules entity);
        #endregion
    }
}