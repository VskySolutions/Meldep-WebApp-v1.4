using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Requirements
{
    public interface IRequirementGroupService
    {
        #region GetAllRequirementGroups
        Task<IPagedList<RequirementGroup>> GetAllRequirementGroups(string SiteId, string LoggedUserId, string SearchText, int requirementGroupNumber, List<string> projectIds, string name, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        IPagedList<RequirementGroup> GetAllRequirementGroupsForDashboard(string SiteId, string projectId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);

        #endregion

        #region GetRequirementGroupById
        Task<RequirementGroup> GetRequirementGroupById(string id);
        #endregion

        #region GetRequirementGroupByName
        Task<RequirementGroup> GetRequirementGroupByName(string name, string ProjectId = null, string id = null);
        #endregion

        #region GetAllRequirementGroupsListForDropdown
        Task<List<CommonDropDown>> GetAllRequirementGroupsListForDropdown(string SiteId, string projectId = null);
        #endregion

        #region GetRequirementGroupDetailsById
        Task<RequirementGroup> GetRequirementGroupDetailsById(string id);
        #endregion

        #region InsertRequirementGroup
        void InsertRequirementGroup(RequirementGroup entity);
        #endregion

        #region UpdateRequirementGroup
        void UpdateRequirementGroup(RequirementGroup entity);
        #endregion

        #region DeleteRequirementGroup
        void DeleteRequirementGroup(RequirementGroup entity);
        #endregion
    }
}
