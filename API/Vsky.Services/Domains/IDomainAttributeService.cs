using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.Domains
{
    public interface IDomainAttributeService
    {
        #region GetById
        Task<DomainAttributes> GetById(string id);
        #endregion

        #region InsertDomainAttributes
        void InsertDomainAttributes(DomainAttributes entity);
        #endregion

        #region UpdateDomainAttributes
        void UpdateDomainAttributes(DomainAttributes entity);
        #endregion

        #region DeleteDomainAttributes
        void DeleteDomainAttributes(DomainAttributes entity);
        #endregion
    }
}
