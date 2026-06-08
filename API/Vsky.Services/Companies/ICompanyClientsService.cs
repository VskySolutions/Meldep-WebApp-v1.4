using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Companies
{
    public interface ICompanyClientsService
    {
        Task<CompanyClients> GetById(string id);
        Task<CompanyClients> GetByPersonId(string personId);
        Task<CompanyClients> GetByCompanyId(string companyId);
        string GetCompanyContactIdById(string customerId);
        Task<CompanyClients> GetCustomerDetailsById(string id);
        //IPagedList<CompanyClients> GetAllCustomers(string SiteId, string SearchText, string customerTypeId, string companyId, string employeeId, string sortBy,
        //    bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        IPagedList<CompanyClients> GetAllCustomers(string SiteId, string SearchText, List<string> customerTypeIds, List<string> customerIds, List<string> employeeIds, string emailAddress, string phoneNumber, List<string> parentCustomerIds, string sortBy,
           bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);

        Task<IList<CompanyClients>> GetAllCompanyClients(string companyId);
        Task<List<CompanyClients>> GetAllClientListForDropdown(string SiteId);
        Task<List<CompanyClients>> GetAllParentCustomerList(string SiteId);
        Task<List<CompanyClients>> GetAllParentCustomerListForDropdown(string siteId, string customerId = null);

        #region GetCustomerByCompanyId
        Task<CompanyClients> GetCustomerByCompanyId(string companyId);
        #endregion

        void InsertCompanyClient(CompanyClients entity);
        void UpdateCompanyClient(CompanyClients entity);
        void DeleteCustomer(CompanyClients entity);
        Task<List<CompanyClients>> GetAllCompanyListForCustomersDropdown(string SiteId);

    }
}
