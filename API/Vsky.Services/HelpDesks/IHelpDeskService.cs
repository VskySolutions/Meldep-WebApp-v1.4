using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.HelpDesks
{
    public interface IHelpDeskService
    {
        #region GetAllHelpDesks
        IPagedList<HelpDesk> GetAllHelpDesks(
            string siteId, 
            string searchText, 
            string LoggedUserId, 
            string assignedToId, 
            List<string> employeeEmails,
            List<string> statusIds, 
            List<string> priorityIds,
            List<string> topicIds,
            List<string> questionIds,
            string createdBy, 
            string title, 
            int TicketNo, 
            List<string> CompanyIds,
            List<string> CategoryIds, 
            DateTime? ticketFromDate, 
            DateTime? ticketToDate,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue, 
            bool lookup = false
      );
        #endregion

        #region GetDropdowns
        Task<List<RequesterDropdownDto>> GetRequesterDropdown(string siteId);
        Task<List<CompanyDropdownDto>> GetCompanyDropdown(string siteId);
        #endregion

        #region GetHelpDeskById
        Task<HelpDesk> GetHelpDeskById(string id);
        #endregion

        #region GetAllHelpDesksByStatusId
        Task<IList<HelpDesk>> GetAllHelpDesksByStatusId(string statusId);
        #endregion

        #region GetHelpDeskDetailsById
        Task<HelpDesk> GetHelpDeskDetailsById(string id);
        #endregion

        #region GetHelpDeskByTitle
        Task<HelpDesk> GetHelpDeskByTitle(string siteId, string title, string id = null);
        #endregion

        #region GetTwilioEmailReplies
        Task<HelpDesk> GetTwilioEmailReplies(string TwilioEmailId);
        #endregion

        #region GetLastTicketId
        Task<int> GetLastTicketId(string SiteId);
        #endregion

        #region GetLatestStatusLogByTicketId
        //Task<HelpDeskStatusLog> GetLatestStatusLogByTicketId(string siteId, string ticketId);

        Task<(HelpDeskStatusLog latestLog, int logCount)> GetLatestStatusLogByTicketId(string siteId, string ticketId);
        #endregion

        void AddHelpDeskStatusLog(string helpDeskId, string statusId, string userId, DateTime GetDateTime);

        #region InsertHelpDesk
        void InsertHelpDesk(HelpDesk entity);
        #endregion

        #region InsertHelpDeskStatusLog
        void InsertHelpDeskStatusLog(HelpDeskStatusLog entity);
        #endregion

        #region UpdateHelpDesk
        void UpdateHelpDesk(HelpDesk entity);
        #endregion

        #region DeleteHelpDesk
        void DeleteHelpDesk(HelpDesk entity);
        #endregion
    }
}
