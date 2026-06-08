using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.LeadUserGroupMappings
{
    public interface ILeadUserGroupMappingService
    {
        #region GetAllLeadUserGroups
        Task<IPagedList<LeadUserGroupMapping>> GetAllLeadUserGroups(
            string SiteId, 
            string SearchText, 
            List<string> userIds, 
            List<string> leadGroupIds, 
            string sortBy,
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue, 
            bool lookup = false
            );
        #endregion

        #region GetLeadUserGroupUserById
        Task<LeadUserGroupMapping> GetLeadUserGroupUserById(string id);
        #endregion

        #region GetAssignedLeadGroupNames
        Task<List<string>> GetAssignedLeadGroupNames(string siteId, string userId, List<string> leadGroupIds);
        #endregion

        #region GetLeadGroupsByUsers
        Task<List<LeadUserGroupMapping>> GetLeadGroupsByUsers(string siteId, string userId);
        #endregion

        #region InsertLeadUserGroup
        void InsertLeadUserGroup(LeadUserGroupMapping entity);
        #endregion

        #region UpdateLeadUserGroup
        void UpdateLeadUserGroup(LeadUserGroupMapping entity);
        #endregion

        #region DeleteLeadUserGroup
        void DeleteLeadUserGroup(LeadUserGroupMapping entities);
        #endregion
    }
}
