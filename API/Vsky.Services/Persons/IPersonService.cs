using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Persons
{
    public interface IPersonService
    {
        #region GetAllPerson
        IPagedList<Person> GetAllPerson(
            string SiteId,
            string SearchText,
            string firstName,
            string lastName,
            string primaryPhoneNumber,
            DateTime? fromDate,
            DateTime? toDate,
            string countryId,
            string stateProvinceId,
            string city,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetAllPersonListForDropdown
        Task<List<Person>> GetAllPersonListForDropdown(string SiteId);
        #endregion

        #region GetAllPersonPrimaryEmailAddressListForDropdown
        Task<List<Person>> GetAllPersonPrimaryEmailAddressListForDropdown(string SiteId);
        #endregion

        #region GetAllIsSharedPersonListForDropdown
        Task<List<Person>> GetAllIsSharedPersonListForDropdown(string SiteId);
        #endregion

        #region GetPersonDetailsById
        Task<Person> GetPersonDetailsById(string id);
        #endregion

        #region GetPersonById
        Task<Person> GetPersonById(string id);
        #endregion
       
        #region GetPersonByEmail
        Task<Person> GetPersonByEmail(string primaryEmailAddress, string id = null, string SiteId = null);
        Task<Person> GetPersonByEmailAddress(string primaryEmailAddress, string id = null, string SiteId = null);
        #endregion

        #region GetPersonByUserEmail
        Task<Person> GetPersonByUserEmail(string email, string SiteId = null);
        #endregion

        #region 
        Task<Person> GetPersonByColor(string id, string bgColor, string color, string SiteId = null);
        #endregion

        #region InsertPerson
        void InsertPerson(Person entity);
        #endregion

        #region GetPersonSiteMappingByPersonId
        Task<PersonSitesMapping> GetPersonSiteMappingByPersonId(string personId, string siteId);
        #endregion

        #region InsertPersonSites
        void InsertPersonSites(PersonSitesMapping entity);
        #endregion

        #region UpdatePerson
        void UpdatePerson(Person entity);
        #endregion

        #region UpdatePersonSites
        void UpdatePersonSites(PersonSitesMapping entity);
        #endregion

        #region DeletePerson
        void DeletePerson(Person entity);
        #endregion
    }
}