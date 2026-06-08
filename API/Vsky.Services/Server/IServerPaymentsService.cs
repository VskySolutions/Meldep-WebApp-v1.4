using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Servers
{
    public interface IServerPaymentsService
    {

        #region GetAllServerPayments
        IPagedList<ServerPayments> GetAllServerPayments(string SiteId, string name, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetById
        Task<ServerPayments> GetById(string id);
        #endregion

        #region InsertServerPayments
        void InsertServerPayments(ServerPayments entity);
        #endregion

        #region UpdateServerPayments
        void UpdateServerPayments(ServerPayments entity);
        #endregion

        #region DeleteServerPayments
        void DeleteServerPayments(ServerPayments entity);
        #endregion
    }
}
