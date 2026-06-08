using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.LeaveCredits
{
    public interface ILeaveCreditService
    {
        #region GetAllLeaveCredits
        IPagedList<LeaveCredit> GetAllLeaveCredits(string SiteId, string SearchText, List<string> employeeIds, /*List<int> years,*/ int years, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region Get LeaveCredits By EmployeeId
        decimal GetLeaveCreditsByEmployeeId(string employeeId, int year);
        #endregion

        #region Get LeaveCredits By EmployeeId and type
        (decimal PaidLeaveCredits, decimal UnpaidLeaveCredits) GetLeaveCreditsByEmployeeIdandType(string SiteId, string employeeId, int year);
        #endregion

        #region GetLeaveCreditById
        Task<LeaveCredit> GetLeaveCreditById(string id);
        #endregion

        #region GetLeaveCreditByEmployeeIdandCreditReason
        Task<LeaveCredit> GetLeaveCreditByEmployeeIdandCreditReason(string employeeId, string CreditReason);
        #endregion

        #region GetLeaveCreditByEmployeeId
        Task<List<LeaveCredit>> GetLeaveCreditByEmployeeId(string employeeId, int year);
        #endregion

        #region GetLeaveCreditByEmployeeId
        Task<(decimal TotalLeaves, decimal CasualLeaves, decimal SickLeaves)> GetAllLeaveCreditsByEmployeeId(string employeeId, int year);

        Task<LeaveCredit> GetLeaveCreditsOfYearByEmployeeId(string employeeId, int year);

        #endregion

        #region GetLeaveCreditByEmployeeIdByType
        Task<List<LeaveCredit>> GetLeaveCreditByEmployeeIdByType(string employeeId, int year, string leaveTypeId);
        #endregion

        #region GetLeaveCreditsByEmployeeData
        //Task<LeaveCredit> GetLeaveCreditsByEmployeeData(string EmployeeId, string creditReason, int year);
        Task<LeaveCredit> GetLeaveCreditsByEmployeeData(string EmployeeId, int year);

        #endregion

        #region GetLeaveCreditDetailsById
        Task<LeaveCredit> GetLeaveCreditDetailsById(string id);
        #endregion

        #region InsertLeaveCredit
        void InsertLeaveCredit(LeaveCredit entity);
        #endregion

        #region UpdateLeaveCredit
        void UpdateLeaveCredit(LeaveCredit entity);
        #endregion

        #region DeleteLeaveCredit
        void DeleteLeaveCredit(LeaveCredit entity);
        #endregion
    }
}