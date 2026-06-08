using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.LeadActivityLogss
{
    public interface ILeadActivityLogsService
    {
        IPagedList<LeadActivityLogs> GetAllLeadActivityLogs(string SiteId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        Task<LeadActivityLogs> GetById(string id);
        List<LeadActivityLogs> GetByLeadId(string id);

        void InsertLeadActivityLogs(LeadActivityLogs entity);

        void UpdateLeadActivityLogs(LeadActivityLogs entity);

        void DeleteLeadActivityLogs(LeadActivityLogs entity);
    }
}
