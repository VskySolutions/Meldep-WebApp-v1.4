using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Servers
{
    public interface IServerService
    {
        #region GetAllServers
        IPagedList<Server> GetAllServers(string SiteId, string SearchText, string ProviderId, string CustomerId, string ContractId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetFTPList
        IPagedList<Server> GetFTPList(string SiteId, string SearchText, string ContractId, string CustomerId, string FTPUsername, string FTPHostname, string Port, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetById
        Task<Server> GetById(string id);
        Task<Server> GetByCustomerId(string customerId);
        #endregion

        #region GetServerDetailsById
        Task<Server> GetServerDetailsById(string id);
        #endregion

        #region InsertServer
        void InsertServer(Server entity);
        #endregion

        #region GetAllServerListForDropdown
        Task<List<Server>> GetAllServerListForDropdown(string SiteId);
        #endregion

        #region GetAllFTPListForDropdown
        Task<List<Server>> GetAllFTPListForDropdown(string SiteId, string type);
        #endregion

        #region UpdateServer
        void UpdateServer(Server entity);
        #endregion

        #region DeleteServer
        void DeleteServer(Server entity);
        #endregion
    }
}
