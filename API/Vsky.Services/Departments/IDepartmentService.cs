using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Departments
{
    public interface IDepartmentService
    {
        #region GetAllDepartments
        IPagedList<Department> GetAllDepartments(string SiteId, string SearchText, List<string> departmentIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetDepartmentById
        Task<Department> GetById(string id);
        #endregion

        #region GetAllDepartmentListForDropdown
        Task<List<Department>> GetAllDepartmentListForDropdown(string SiteId);
        #endregion

        #region GetDepartmentDetailsById
        Task<Department> GetDepartmentDetailsById(string id);
        #endregion

        #region GetDepartmentByName
        Task<Department> GetDepartmentByName(string SiteId, string name, string id = null);
        #endregion

        #region InsertDepartment
        void InsertDepartment(Department entity);
        #endregion

        #region UpdateDepartment
        void UpdateDepartment(Department entity);
        #endregion

        #region DeleteDepartment
        void DeleteDepartment(Department entity);
        #endregion
    }
}