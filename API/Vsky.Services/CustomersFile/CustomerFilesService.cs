using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

namespace Vsky.Services.CustomersFile
{
    public class CustomerFilesService : ICustomerFilesService
    {
        #region Define Services
        private readonly IRepository<CustomerFiles> _customerFileRepository;
        private readonly IRepository<VW_CustomerFiles> _VWCustomerFiles;

        public CustomerFilesService(
            IRepository<CustomerFiles> customerFileRepository, 
            IRepository<VW_CustomerFiles> vWCustomerFiles)
        {
            _customerFileRepository = customerFileRepository;
            _VWCustomerFiles = vWCustomerFiles;
        }
        #endregion

        #region Private Methods
        // Title: GetOrderBy
        // Description: This method returns the input string as it is, which can be used as the `ORDER BY` clause in a SQL query.
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAll
        public IPagedList<CustomerFiles> GetAllCustomerFiles(string SiteId, string note, string createdBy, string customerId, int year, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _customerFileRepository.Table
                .Where(x => !x.Deleted && x.SiteId == SiteId && (x.CompanyClients.CompanyId != null || x.CompanyClients.PersonId != null))
                .Include(x => x.CustomerFilesLines.OrderBy(m=>m.SortOrder).Where(m=> !m.Deleted))
                .Include(x => x.CreatedBy)
                .Include(x => x.UpdatedBy)
                .Include(x => x.CompanyClients)
                .Include(x => x.CompanyClients).ThenInclude(x => x.Company)
                .Include(x => x.CompanyClients).ThenInclude(x => x.Person)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(note))
              query = query.Where(x => x.Note == note);
            
            if (!string.IsNullOrWhiteSpace(createdBy))
              query = query.Where(x => x.CreatedById == createdBy);

            if(!string.IsNullOrWhiteSpace(customerId))
              query = query.Where(x => x.CustomerId == customerId);

            if(year != 0)
              query = query.Where(x => x.Year == year);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            var list = new PagedList<CustomerFiles>(query, page, pageSize);
            return list;
        }
        
        public IPagedList<VW_CustomerFiles> GetAllCustomerFilesFromVW(string SiteId, string customerId, int year, int page = 1, int pageSize = int.MaxValue)
        {
            var query = _VWCustomerFiles.TableNoTracking.Where(m => m.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(customerId))
                query = query.Where(x => x.CustomerId == customerId);

            if (year != 0)
                query = query.Where(x => x.Year == year);

            query = query.OrderByDescending(m => m.Year).ThenBy(m => m.CustomerName).ThenBy(m => m.SortOrder);

            var list = new PagedList<VW_CustomerFiles>(query.Count() > 0 ? query : null, page, pageSize);
            return list;
        }
        #endregion

        #region Get By Id
        public async Task<CustomerFiles> GetById(string id)
        {
            var query = _customerFileRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<CustomerFiles> GetCustomerFileDetailsById(string id)
        {
            var item = await _customerFileRepository.Table
                .Where(x => !x.Deleted && x.Id == id)
                .Include(x => x.CustomerFilesLines)
                .Include(x => x.CompanyClients)
                .Include(x => x.CompanyClients).ThenInclude(c => c.Company)
                .Include(x => x.CompanyClients).ThenInclude(c => c.Person)                
                .FirstOrDefaultAsync();

            return item;
        }

        public async Task<CustomerFiles> GetCustomerFileByIdAndName(string customerId, string name)
        {
            var query = _customerFileRepository.TableNoTracking.Where(x => !x.Deleted && x.CustomerId == customerId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetCustomerFilesDetailsByYearAndId
        public async Task<List<VW_CustomerFiles>> GetCustomerFilesDetailsByYearAndId(string SiteId, int year, string customerId)
        {
            var query = _VWCustomerFiles.TableNoTracking.Where(m => m.SiteId == SiteId && m.Year == year && m.CustomerId == customerId);

            return await query.ToListAsync();
        }
        #endregion

        #region GetCustomerFilesDetailsByYearAndIdAndNote
        public async Task<CustomerFiles> GetCustomerFilesDetailsByYearAndIdAndNote(string SiteId, int year, string customerId, string note)
        {
            var query = _customerFileRepository.TableNoTracking.Where(m => m.SiteId == SiteId && m.Year == year && m.CustomerId == customerId && m.Note == note);

            return await query.FirstOrDefaultAsync();
        }
        #endregion

        #region InsertCustomerFiles
        public void InsertCustomerFiles(CustomerFiles entity)
        {
            _customerFileRepository.Insert(entity);
        }

        public void InsertCustomerFilesList(IList<CustomerFiles> entities)
        {
            _customerFileRepository.Insert(entities);
        }
        #endregion

        #region UpdateCustomerFiles
        public void UpdateCustomerFiles(CustomerFiles entity)
        {
            _customerFileRepository.Update(entity);
        }
        #endregion

        #region DeleteCustomerFiles
        public void DeleteCustomerFiles(CustomerFiles entity)
        {
            entity.Deleted = true;
            _customerFileRepository.Update(entity);
        }
        #endregion
    }
}
