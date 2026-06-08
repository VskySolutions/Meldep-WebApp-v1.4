using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.SalesPersons
{
    public interface ISalesPersonService
    {
        IPagedList<SalesPerson> GetAllSalesPersons(string SiteId, string name, string sortBy,
            bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        Task<SalesPerson> GetById(string id);
        Task<SalesPerson> GetSalesPersonDetailsById(string id);
        void InsertSalesPerson(SalesPerson entity);
    }
}
