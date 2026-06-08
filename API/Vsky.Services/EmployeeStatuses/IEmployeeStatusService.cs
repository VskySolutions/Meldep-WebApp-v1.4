using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.EmployeeStatuses
{
    public interface IEmployeeStatusService
    {
        #region GetEmployeeStatusById
        Task<EmployeeStatus> GetEmployeeStatusById(string id);
        #endregion

        #region InsertEmployeeStatusList
        void InsertEmployeeStatusList(IList<EmployeeStatus> entities);
        #endregion

        #region UpdateEmployeeStatusList
        void UpdateEmployeeStatusList(IList<EmployeeStatus> entities);
        #endregion

        #region DeleteEmployeeStatusList
        void DeleteEmployeeStatusList(List<EmployeeStatus> entities);
        #endregion
    }
}
