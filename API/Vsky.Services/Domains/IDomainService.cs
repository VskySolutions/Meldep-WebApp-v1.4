using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Domains
{
    public interface IDomainService
    {

        #region GetAllDomains
        IPagedList<Domain> GetAllDomains(string SiteId, string SearchText, List<string> projectIds, string url, List<string> domainTypeIds, List<string> domainServerIds, List<string> hostingServerIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetById
        Task<Domain> GetById(string id);
        #endregion

        #region GetDomainDetailsById
        Task<Domain> GetDomainDetailsById(string id);
        #endregion

        #region InsertDomain
        void InsertDomain(Domain entity);
        #endregion

        #region UpdateDomain
        void UpdateDomain(Domain entity);
        #endregion

        #region DeleteDomain
        void DeleteDomain(Domain entity);
        #endregion
    }
}
