using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.EmployeeDepartments
{
    public interface IEmployeeDepartmentService
    {
        #region GetDepartmentById
        Task<EmployeeDepartment> GetDepartmentById(string id, string employeeDepartmentId);
        #endregion

        #region GetEmployeeDepartmentById
        Task<EmployeeDepartment> GetEmployeeDepartmentById(string id);
        #endregion

        #region InsertEmployeeDepartmentList
        void InsertEmployeeDepartmentList(IList<EmployeeDepartment> entities);
        #endregion

        #region UpdateEmployeeDepartmentList
        void UpdateEmployeeDepartmentList(IList<EmployeeDepartment> entities);
        #endregion

        #region DeleteEmployeeDepartmentList
        void DeleteEmployeeDepartmentList(List<EmployeeDepartment> entities);
        #endregion
    }
}

