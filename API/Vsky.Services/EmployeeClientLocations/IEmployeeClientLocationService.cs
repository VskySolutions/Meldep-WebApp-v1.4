using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.EmployeeClientLocations
{
    public interface IEmployeeClientLocationService
    {
        #region GetEmployeeClientLocationById
        Task<EmployeeClientLocation> GetEmployeeClientLocationById(string id);
        #endregion

        #region InsertEmployeeClientLocationList
        void InsertEmployeeClientLocationList(IList<EmployeeClientLocation> entities);
        #endregion

        #region UpdateEmployeeClientLocationList
        void UpdateEmployeeClientLocationList(IList<EmployeeClientLocation> entities);
        #endregion

        #region DeleteEmployeeClientLocationList
        void DeleteEmployeeClientLocationList(List<EmployeeClientLocation> entities);
        #endregion
    }
}



