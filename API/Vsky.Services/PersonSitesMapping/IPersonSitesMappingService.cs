using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Persons
{
    public interface IPersonSitesMappingService
    {
        #region GetAllSiteShare
        IPagedList<PersonSitesMapping> GetAllSiteShare(
            string SiteId,
            string SearchText,
            List<string> personIds,
            string primaryEmailAddress,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetAllSharedSitesByLoggedUserId
        Task<IList<PersonSitesMapping>> GetAllSharedSitesByLoggedUserId(string LoggedUserId, string SiteId);
        #endregion

        #region GetAllSitesByPersonId
        Task<IList<PersonSitesMapping>> GetAllSitesByPersonId(string personId);
        #endregion

        #region Find by id
        Task<PersonSitesMapping> GetById(string id);
        #endregion

        #region GetPersonInSite
        Task<bool> GetPersonInSite(string personId, string siteId);
        #endregion

        #region GetPersonInOtherSite
        Task<bool> GetPersonInOtherSite(string personId, string siteId);
        #endregion

        #region GetPersonSiteMappingByPersonId
        Task<PersonSitesMapping> GetPersonSiteMappingByPersonId(string personId, string siteId);
        #endregion

        #region Find InvitedBy from createdById
        Task<PersonSitesMapping> GetInvitedByCreatedById(string createdById);
        #endregion

        #region GetLastUsedSiteByPersonId
        Task<PersonSitesMapping> GetLastUsedSiteByPersonId(string personId);
        #endregion


        #region InsertPersonSites
        void InsertPersonSites(PersonSitesMapping entity);
        #endregion

        #region UpdatePersonSites
        void UpdatePersonSites(PersonSitesMapping entity);
        #endregion

        #region DeletePersonSites
        void DeletePersonSites(PersonSitesMapping entity);
        #endregion
    }
}