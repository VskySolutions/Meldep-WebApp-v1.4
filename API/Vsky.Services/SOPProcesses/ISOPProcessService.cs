using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.SOPProcesses
{
    public interface ISOPProcessService
    {
        IPagedList<Vsky.Models.SOPProcess> GetAllSOPProcesses(string searchText, string siteId, string logginuser, string title, bool isActive, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue);
        SOPProcess GetSOPProcessById(string siteId, string Id);
        Task<Vsky.Models.SOPProcess> GetSOPProcessByIdInDetail(string siteId, string Id);
        Task<SOPProcess> GetSOPProcessByTitle(string SiteId, string title, string id = null);
        void InsertSOPProcess(SOPProcess entity);
        void UpdateSOPProcess(SOPProcess entity);
        void DeleteSOPProcess(SOPProcess entity);
    }
}
