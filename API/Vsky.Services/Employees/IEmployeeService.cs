using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Employees
{
    public interface IEmployeeService
    {
        #region GetAllEmployees
        IPagedList<Employee> GetAllEmployees(
            string SiteId,
            string SearchText,
            string employeeCode,
            List<string> EmployeeIds,
            string primaryEmailAddress,
            List<string> employeeTypeIds,
            List<string> employeeDepartmentIds,
            List<string> employeeDesignationIds,
            List<string> orgLocationIds,
            string employeeStatus,
            string AllStatusId,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion
        
        //#region GetAllActiveEmployee
        //IPagedList<Employee> GetAllActiveEmployee(string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        //#endregion

        #region GetAllEmployeeByStatusId
        List<Employee> GetAllEmployeeByStatusId(string SiteId, string statusId);
        #endregion

        #region GetAllEmployeeByStatusId
        Task<Employee> GetActiveEmployeeByStatusId(string SiteId, string statusId, string assignedToId);
        #endregion

        #region GetAllEmployeesByEmployementType
        List<Employee> GetAllEmployeesByEmployementType(string SiteId, string employementTypeId, string activeEmployeeStatus, string exEmployeeStatus);
        #endregion

        #region GetEmployeeById
        Task<Employee> GetById(string id);
        #endregion

        #region GetEmployeeByPersonId
        Task<Employee> GetEmployeeByPersonId(string PersonId, string id = null);

        Task<Employee> GetEmployeeByPersonIdBySiteId(string PersonId, string siteId, string id = null);

        Task<Employee> GetEmployeeByEmailAndBySiteId(string Email, string siteId, string id = null);
        Task<Employee> GetEmployeeDetailsByPersonId(string PersonId);

        #endregion

        //#region GetLeaveApproverByEmployeeId
        //Task<EmployeeDesignation> GetLeaveApproverByEmployeeId(string EmployeeId);
        //#endregion

        #region GetAllEmployeeListForDropdown
        Task<List<Employee>> GetAllEmployeeListForDropdown(string SiteId);
        #endregion

        #region GetAllActivityOwnerListForDropdown
        Task<List<Employee>> GetAllActivityOwnerListForDropdown(string SiteId, string activeEmployeeStatus, string exEmployeeStatus);
        #endregion

        #region GetAllActiveEmployeeListForDropdown
        //Task<List<Employee>> GetAllActiveEmployeeListForDropdown(string SiteId);
        Task<List<Employee>> GetAllActiveEmployeeListForDropdown(string SiteId, DateTime? TargetMonth = null);
        #endregion

        #region GetEmployeesByStatus
        Task<List<Employee>> GetEmployeesByStatus(string SiteId, string statusId);
        #endregion

        #region GetEmployeeDetailsById
        Task<Employee> GetEmployeeDetailsById(string id);
        Task<Employee> GetEmployeeByEmail(string SiteId, string Email);
        #endregion

        #region GetEmployeeCode
        Task<Employee> GetEmployeeCode();

        #endregion

        #region GetEmployeeByEmployeeCode
        Task<Employee> GetEmployeeByEmployeeCode(string SiteId, string employeeCode, string id = null);
        #endregion


        #region GetEmployeesForAnniversary
        Task<List<Employee>> GetEmployeesForAnniversary(string SiteId, int daysBefore);
        #endregion

        #region InsertEmployee
        void InsertEmployee(Employee entity);
        #endregion

        #region UpdateEmployee
        void UpdateEmployee(Employee entity);
        #endregion

        #region DeleteEmployee
        void DeleteEmployee(Employee entity);
        #endregion
    }
}