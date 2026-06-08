using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Leads
{
    public interface ILeadService
    {
        #region GetAllLeads
        Task<IPagedList<Lead>> GetAllLeads(
            string SiteId,
            string userId, 
            List<string> leadGroupIdsForUser,
            string SearchText, 
            string personId, 
            string companyId, 
            List<string> leadGroupIds,
            string leadSourceId, 
            string sortBy,
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue, 
            bool lookup = false
        );
        #endregion

        #region GetById
        Task<Lead> GetById(string id);
        #endregion

        #region GetLeadDetailsById
        Task<Lead> GetLeadDetailsById(string id);
        #endregion

        #region GetAllLeadsCount
        int GetAllLeadsCount(string SiteId);
        #endregion

        #region GetAllLeadStages
        Task<IList<LeadStages>> GetAllLeadStages();
        #endregion

        #region GetAllActivities
        Task<IList<LeadActivities>> GetAllActivities();
        #endregion

        #region GetAllLeadActivityListForDropdown
        Task<List<CommonDropDown>> GetAllLeadActivityListForDropdown();
        #endregion

        #region GetAllLeadListForDropdown
        Task<List<Lead>> GetAllLeadListForDropdown(string SiteId);
        #endregion

        #region InsertLead
        void InsertLead(Lead entity);
        #endregion

        #region UpdateLead
        void UpdateLead(Lead entity);
        #endregion

        #region DeleteLead
        void DeleteLead(Lead entity);
        #endregion
    }
}
