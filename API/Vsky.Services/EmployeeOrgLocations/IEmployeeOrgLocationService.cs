using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.EmployeeOrgLocations
{
    public interface IEmployeeOrgLocationService
    {
        #region GetEmployeeOrgLocationById
        Task<EmployeeOrgLocation> GetEmployeeOrgLocationById(string id);
        #endregion

        #region InsertEmployeeOrgLocationList
        void InsertEmployeeOrgLocationList(IList<EmployeeOrgLocation> entities);
        #endregion

        #region UpdateEmployeeOrgLocationList
        void UpdateEmployeeOrgLocationList(IList<EmployeeOrgLocation> entities);
        #endregion

        #region DeleteEmployeeOrgLocationList
        void DeleteEmployeeOrgLocationList(List<EmployeeOrgLocation> entities);
        #endregion
    }
}



