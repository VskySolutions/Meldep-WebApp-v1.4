using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Domains
{
    public class DomainAttributeService : IDomainAttributeService
    {
        #region Services Intialization

        private readonly IRepository<DomainAttributes> _domainAttributesRepository;

        public DomainAttributeService(IRepository<DomainAttributes> domainAttributesRepository)
        {
            _domainAttributesRepository = domainAttributesRepository;
        }
        #endregion

        #region GetOrderBy
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetById
        public async Task<DomainAttributes> GetById(string id)
        {
            var query = _domainAttributesRepository.TableNoTracking.Where(x => x.Id == id && !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertDomainAttributes
        public void InsertDomainAttributes(DomainAttributes entity)
        {
            _domainAttributesRepository.Insert(entity);
        }
        #endregion

        #region UpdateDomainAttributes
        public void UpdateDomainAttributes(DomainAttributes entity)
        {
            _domainAttributesRepository.Update(entity);
        }
        #endregion

        #region DeleteDomainAttributes
        public void DeleteDomainAttributes(DomainAttributes entity)
        {
            entity.Deleted = true;

            _domainAttributesRepository.Update(entity);
        }
        #endregion
    }
}
