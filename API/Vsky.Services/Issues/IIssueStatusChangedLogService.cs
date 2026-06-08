using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Issues
{
    public interface IIssueStatusChangedLogService
    {
        //#region GetAllIssueStatus
        //IPagedList<IssueStatusChangedLog> GetAllIssueStatus(string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        //#endregion

        #region GetIssueStatusLogById
        Task<IssueStatusChangedLog> GetIssueStatusLogById(string id);
        #endregion

        #region GetIssueStatusChangedLogDetailsById
        Task<IssueStatusChangedLog> GetIssueStatusLogDetailsById(string id);
        #endregion

        #region InsertIssueStatusChangedLog
        void InsertIssueStatusChangedLog(IssueStatusChangedLog entity);
        #endregion

        #region UpdateIssueStatusChangedLog
        void UpdateIssueStatusChangedLog(IssueStatusChangedLog entity);
        #endregion
    }
}
