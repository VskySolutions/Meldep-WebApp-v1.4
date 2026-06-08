using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Sites
{
    public interface ISiteService
    {
        #region
        Task<IList<Site>> GetAllSitesList();
        #endregion

        #region GetAllSites
        IPagedList<Site> GetAllSites(string SearchText, string name, string fullName, string emailAddress, string siteStatus, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region Find site by id
        Task<Site> GetById(string id);
        Task<Site> GetSiteDetailsById(string id);
        Task<string> GetSiteIdFromTicketGenerationEmail(string Email);

        Task<Site> GetSiteTicketNoPrefixById(string siteId);
        #endregion

        #region Find site by name
        Task<Site> GetBySiteName(string name, string id = null);
        #endregion

        #region Find Site Name By Id
        Task<Site> GetSiteNameById(string id);
        #endregion

        #region Insert site
        void InsertSite(Site entity);
        #endregion

        #region Update site
        void UpdateSite(Site entity);
        #endregion

        #region Delete site
        void DeleteSite(Site entity);
        #endregion

        #region Get site details
        public Site GetSiteDetails();
        public DateTime GetDateTime(string siteId = null);
        string GetTimeZoneFromSiteId(string Id);
        #endregion

        #region GetVskySiteId
        string GetVskySiteId();
        #endregion

    }
}