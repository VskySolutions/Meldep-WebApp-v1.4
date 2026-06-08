using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;


namespace Vsky.Services.ProjectEmployeeMappings
{
    public interface IProjectEmployeeMappingService
    {
        #region GetProjectEmployeeById
        Task<ProjectEmployeeMapping> GetProjectEmployeeById(string id);
        #endregion

        #region GetProjectEmployeeRoleById
        //Task<ProjectEmployeeMapping> GetById(string id);
        Task<ProjectEmployeeMapping> GetProjectEmployeeByRoleIdAndProjectId(string SiteId, string projectId, string roleId, string employeeId = null, string id = null);
        #endregion

        #region GetProjectEmployeeByProjectId
        List<ProjectEmployeeMapping> GetProjectEmployeeByProjectId(string ProjectId);
        #endregion

        #region InsertProjectEmployees
        void InsertProjectEmployees(ProjectEmployeeMapping entity);
        #endregion

        #region UpdateProjectEmployees
        void UpdateProjectEmployees(ProjectEmployeeMapping entity);
        #endregion

        #region DeleteProjectEmployees
        void DeleteProjectEmployees(ProjectEmployeeMapping entity);
        #endregion

        #region InsertProjectEmployeeMappingList
        void InsertProjectEmployeeMappingList(IList<ProjectEmployeeMapping> entities);
        #endregion

        #region UpdateProjectEmployeeMappingList
        void UpdateProjectEmployeeMappingList(List<ProjectEmployeeMapping> entities);
        #endregion

        #region DeleteProjectEmployeeMappingList
        void DeleteProjectEmployeeMappingList(List<ProjectEmployeeMapping> entities);
        #endregion

        #region GetProjectCharterEmployeesWithWeeklyPlanHoursByProjectId
        // Title: GetProjectCharterEmployeesWithWeeklyPlanHoursByProjectId
        Task<List<ProjectEmployeeMapping>> GetProjectCharterEmployeesWithWeeklyPlanHoursByProjectId(string projectId, DateTime? currentDate = null);
        #endregion

        #region GetProjectCharterEmployeeByProjectId
        // Title: GetProjectCharterEmployeeByProjectId
        Task<List<CommonDropDown>> GetProjectCharterEmployeeByProjectId(string projectId);
        #endregion

        #region GetProjectEmployeesByRoleId
        // Title: GetProjectEmployeesByRoleId
        List<ProjectEmployeeMapping> GetProjectEmployeesByRoleId(string projectId, string roleId);
        #endregion
    }
}
