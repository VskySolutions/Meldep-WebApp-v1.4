using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Requirements
{
    public interface IRequirementService
    {
        #region GetAllRequirements
        Task<IPagedList<Requirement>> GetAllRequirements(
            string SiteId,
            string LoggedUserId,
            string SearchText,
            int requirementNumber,
            List<string> projectIds,
            List<string> projectModuleIds,
            List<string> requirementGroupIds,
            string name,
            string editingStatus,
            List<string> statusIds,
            List<string> requirementTypeIds,
            string identifiedUserTypeId,
            List<string> identifiedCustomerIds,
            List<string> identifiedEmployeeIds,
            List<string> requirementTagIds,
            DateTime? fromDate,
            DateTime? toDate,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        IPagedList<Requirement> GetAllRequirementsForDashboard(string SiteId, string projectId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        List<VWProjectRequirementStatusSummary> GetRequirementStatusSummaryByProjectIds(List<string> projectIds);

        #region GetRequirementById
        Task<Requirement> GetRequirementById(string id);
        #endregion

        #region GetLastRequirementNumber
        Task<int> GetLastRequirementNumber();
        #endregion

        //#region GetAllRequirementsListForDropdown
        //Task<List<Requirement>> GetAllRequirementsListForDropdown();
        //#endregion

        #region GetRequirementDetailsById
        Task<Requirement> GetRequirementDetailsById(string id);
        #endregion

        #region GetRequirementDescriptionById
        Task<string> GetRequirementDescriptionById(string id);
        #endregion


        #region GetRequirementByName
        Task<Requirement> GetRequirementByName(string title, string ProjectId, string ProjectModuleId = null, string id = null);
        #endregion

        #region InsertRequirement
        void InsertRequirement(Requirement entity);
        #endregion

        #region UpdateRequirement
        void UpdateRequirement(Requirement entity);
        #endregion

        #region DeleteRequirement
        void DeleteRequirement(Requirement entity);
        #endregion
    }
}

