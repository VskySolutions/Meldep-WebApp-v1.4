using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.CustomersFile
{
    public class CustomerFilesLinesService : ICustomerFilesLinesService
    {
        #region Define Services
        private readonly IRepository<CustomerFilesLines> _customerFilesLinesRepository;

        public CustomerFilesLinesService(
            IRepository<CustomerFilesLines> customerFilesLinesRepository)
        {
            _customerFilesLinesRepository = customerFilesLinesRepository;
        }
        #endregion

        #region GetById
        public async Task<CustomerFilesLines> GetById(string id)
        {
            return await _customerFilesLinesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).FirstOrDefaultAsync();
        }
        #endregion

        #region GetCustomerFileLinesByCustomerFileId
        public async Task<List<CustomerFilesLines>> GetCustomerFileLinesByCustomerFileId(string customerFileId)
        {
            return await _customerFilesLinesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == customerFileId).ToListAsync();
        }
        #endregion

        #region InsertCustomerFilesLines
        public void InsertCustomerFilesLines(CustomerFilesLines entity)
        {
            _customerFilesLinesRepository.Insert(entity);
        }

        public void InsertCustomerFilesLinesList(IList<CustomerFilesLines> entities)
        {
            _customerFilesLinesRepository.Insert(entities);
        }
        #endregion

        #region UpdateCustomerFilesLines
        public void UpdateCustomerFilesLines(CustomerFilesLines entity)
        {
            _customerFilesLinesRepository.Update(entity);
        }
        #endregion

        #region DeleteCustomerFilesLines
        public void DeleteCustomerFilesLines(CustomerFilesLines entity)
        {
            entity.Deleted = true;
            _customerFilesLinesRepository.Update(entity);
        }
        #endregion
    }
}
