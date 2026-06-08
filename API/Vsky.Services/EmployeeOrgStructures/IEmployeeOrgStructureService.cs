using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.EmployeeOrgStructures
{
    public interface IEmployeeOrgStructureService
    {
        #region GetAllEmployeeOrgStructureList
        IPagedList<EmployeeOrgStructure> GetAllEmployeeOrgStructureList(
            string SiteId,
            string SearchText,
            int years,
            string level,
            List<string> departmentIds,
            List<string> employeeDesignationIds,
            List<string> managerIds,
            List<string> employeeIds,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetById
        Task<EmployeeOrgStructure> GetById(string id);
        #endregion

        #region GetEmployeeOrgStructureDetailsById
        Task<EmployeeOrgStructure> GetEmployeeOrgStructureDetailsById(string id);
        #endregion


        #region GetEmployeeOrgStructureByManagerAndEmployee
        Task<EmployeeOrgStructure> GetEmployeeOrgStructureByManagerAndEmployee(string SiteId, string managerId, string employeeId, string id = null);
        List<OrgChartPreview> GetOrgStructurePreview(string siteId, int year);

        Task<EmployeeOrgStructure> GetEmployeeOrgStructureByYearAndLevel(string SiteId, int year, int? level, string id = null);
        #endregion

        #region InsertEmployeeOrgStructure
        void InsertEmployeeOrgStructure(EmployeeOrgStructure entity);
        #endregion

        #region UpdateEmployeeOrgStructure
        void UpdateEmployeeOrgStructure(EmployeeOrgStructure entity);
        #endregion

        #region DeleteEmployeeOrgStructure
        void DeleteEmployeeOrgStructure(EmployeeOrgStructure entity);
        #endregion
    }
}