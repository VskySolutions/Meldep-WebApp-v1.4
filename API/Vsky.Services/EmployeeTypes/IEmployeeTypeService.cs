using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.EmployeeTypes
{
    public interface IEmployeeTypeService
    {
        #region GetEmployeeTypeById
        Task<EmployeeType> GetEmployeeTypeById(string id);
        #endregion

        #region InsertEmployeeTypeList
        void InsertEmployeeTypeList(IList<EmployeeType> entities);
        #endregion

        #region UpdateEmployeeTypeList
        void UpdateEmployeeTypeList(IList<EmployeeType> entities);
        #endregion

        #region DeleteEmployeeTypeList
        void DeleteEmployeeTypeList(List<EmployeeType> entities);
        #endregion
    }
}
