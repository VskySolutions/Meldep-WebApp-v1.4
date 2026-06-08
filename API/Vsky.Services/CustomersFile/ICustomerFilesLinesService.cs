using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.CustomersFile
{
    public interface ICustomerFilesLinesService
    {
        #region GetById
        Task<CustomerFilesLines> GetById(string id);
        #endregion

        #region GetCustomerFileLinesByCustomerFileId
        Task<List<CustomerFilesLines>> GetCustomerFileLinesByCustomerFileId(string customerFileId);
        #endregion

        #region InsertCustomerFilesLines
        void InsertCustomerFilesLines(CustomerFilesLines entity);

        void InsertCustomerFilesLinesList(IList<CustomerFilesLines> entities);
        #endregion

        #region UpdateCustomerFilesLines
        void UpdateCustomerFilesLines(CustomerFilesLines entity);
        #endregion

        #region DeleteCustomerFilesLines
        void DeleteCustomerFilesLines(CustomerFilesLines entity);
        #endregion
    }
}
