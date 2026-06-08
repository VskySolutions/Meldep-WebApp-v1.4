using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.LeaveRuleLine
{
    public interface ILeaveRuleLinesService
    {
        //#region GetAllLeaveRuleLines
        //IPagedList<LeaveRuleLines> GetAllLeaveRuleLines(string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        //#endregion
        #region GetLeaveRuleLinesByEmployeeTypeId
        Task<LeaveRuleLines> GetLeaveRuleLinesByEmployeeTypeId(string EmployeeTypeId, string id = null);
        #endregion

        #region GetLeaveRuleLinesByLeaveRuleId
        Task<List<LeaveRuleLines>> GetLeaveRuleLinesByLeaveRuleId(string LeaveRuleId, string employeeType = null);
        #endregion

        #region GetLeaveRuleLineById
        Task<LeaveRuleLines> GetLeaveRuleLineById(string id);
        #endregion

        #region InsertLeaveRuleLine
        void InsertLeaveRuleLine(LeaveRuleLines entity);
        #endregion

        #region InsertLeaveRuleLinesList
        void InsertLeaveRuleLinesList(IList<LeaveRuleLines> entities);
        #endregion

        #region UpdateLeaveRuleLinesList
        void UpdateLeaveRuleLinesList(IList<LeaveRuleLines> entities);
        #endregion

        #region DeleteLeaveRuleLinesList
        void DeleteLeaveRuleLinesList(List<LeaveRuleLines> entities);
        #endregion
    }
}
