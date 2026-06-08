using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.CustomersFile
{
    public interface ICustomerFilesService
    {
        Task<CustomerFiles> GetById(string id);
        Task<CustomerFiles> GetCustomerFileDetailsById(string id);
        Task<CustomerFiles> GetCustomerFileByIdAndName(string customerId, string name);
        IPagedList<CustomerFiles> GetAllCustomerFiles(string SiteId, string note, string createdBy, string customerId, int year, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        IPagedList<VW_CustomerFiles> GetAllCustomerFilesFromVW(string SiteId, string customerId, int year, int page = 1, int pageSize = int.MaxValue);

        #region GetCustomerFilesDetailsByYearAndId
        Task<List<VW_CustomerFiles>> GetCustomerFilesDetailsByYearAndId(string SiteId, int year, string customerId);
        #endregion

        #region GetCustomerFilesDetailsByYearAndIdAndNote
        Task<CustomerFiles> GetCustomerFilesDetailsByYearAndIdAndNote(string SiteId, int year, string customerId, string note);
        #endregion

        void InsertCustomerFiles(CustomerFiles entity);
        void InsertCustomerFilesList(IList<CustomerFiles> entities);
        void UpdateCustomerFiles(CustomerFiles entity);
        void DeleteCustomerFiles(CustomerFiles entity);
    }
}
