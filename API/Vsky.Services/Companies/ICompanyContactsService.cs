using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Companies
{
    public interface ICompanyContactsService
    {
        #region GetAllContactList
        IPagedList<CompanyContacts> GetAllContactList(string SiteId, string SearchText, string companyId,string sortBy,
            bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetById
        Task<CompanyContacts> GetById(string id);
        #endregion

        #region GetCompanyContactdetailsById
        Task<CompanyContacts> GetCompanyContactdetailsById(string id);
        #endregion

        #region GetAllContacts
        Task<IList<CompanyContacts>> GetAllContacts(string companyId);
        #endregion

        #region GetAllCustomerContactListForDropdown
        Task<List<CompanyContacts>> GetAllCustomerContactListForDropdown(string SiteId, string statusId);
        #endregion

        //new bs 
        #region GetAllContactListForDropdown
        Task<List<CompanyContacts>> GetAllContactListForDropdown(string SiteId, string customerId);
        #endregion

        #region GetAllContactListByCompanyIdForDropdown
        Task<List<CompanyContacts>> GetAllContactListByCompanyIdForDropdown(string SiteId, string companyId = null);
        #endregion


        #region GetByPersonAndCompanyId
        Task<CompanyContacts> GetByPersonAndCompanyId(string companyId, string personId, string id = null);
        Task<CompanyContacts> GetCompanyContactByPersonId(string personId, string id = null);
        #endregion

        #region InsertCompanyContact
        void InsertCompanyContact(CompanyContacts entity);
        #endregion

        #region UpdateCompanyContact
        void UpdateCompanyContact(CompanyContacts entity);
        #endregion

        #region DeleteCompanyContact
        void DeleteCompanyContact(CompanyContacts entity);
        #endregion

        #region InsertCompanyContactList
        void InsertCompanyContactList(IList<CompanyContacts> entities);
        #endregion

        #region UpdateCompanyContactList
        void UpdateCompanyContactList(IList<CompanyContacts> entities);
        #endregion

        #region DeleteCompanyContactList
        void DeleteCompanyContactList(List<CompanyContacts> entities);
        #endregion
    }
}
