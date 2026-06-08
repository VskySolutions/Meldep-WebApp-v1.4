using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit.Search;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Companies
{
    public interface ICompanyService
    {
        #region GetAllCompanies
        IPagedList<Company> GetAllCompanies(string SiteId, string SearchText, string companyId, string businessTypeId, string employeeId, string sortBy,
            bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetById
        Task<Company> GetById(string id);
        #endregion

        #region GetcompanydetailsById
        Task<Company> GetcompanydetailsById(string id);
        //Task<Company> GetCompanyDetailsForCustomer(string id);
        #endregion

        #region GetAllCompanyListForDropdown
        Task<List<CommonDropDown>> GetAllCompanyListForDropdown(string SiteId);
        #endregion

        #region GetAllPrimaryEmployeeListForDropdown
        Task<List<Company>> GetAllPrimaryEmployeeListForDropdown(string SiteId);
        #endregion

        #region GetCompanyByNameAndSiteId
        Task<Company> GetCompanyByNameAndSiteId(string companyName, string siteId);
        #endregion

        #region InsertCompany
        void InsertCompany(Company entity);
        #endregion

        #region UpdateCompany
        void UpdateCompany(Company entity);
        #endregion

        #region DeleteCompany
        void DeleteCompany(Company entity);
        #endregion
    }
}