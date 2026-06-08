using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.EmployeeDesignations
{
    public interface IEmployeeDesignationService
    {
        #region GetEmployeeDesignationById
        Task<EmployeeDesignation> GetEmployeeDesignationById(string id);
        #endregion

        #region GetEmployeeDesignationByEmployeeId
        Task<EmployeeDesignation> GetEmployeeDesignationByEmployeeId(string employeeId);
        #endregion

        #region InsertEmployeeDesignationList
        void InsertEmployeeDesignationList(IList<EmployeeDesignation> entities);
        #endregion

        #region UpdateEmployeeDesignationList
        void UpdateEmployeeDesignationList(IList<EmployeeDesignation> entities);
        #endregion

        #region DeleteEmployeeDesignationList
        void DeleteEmployeeDesignationList(List<EmployeeDesignation> entities);
        #endregion
    }
}


