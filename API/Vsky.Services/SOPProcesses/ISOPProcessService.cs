using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.SOPProcesses
{
    public interface ISOPProcessService
    {
        IPagedList<Vsky.Models.SOPProcess> GetAllSOPProcesses(
            string searchText, 
            string siteId, 
            string logginuser, 
            string title, 
            List<string> categoryIds, 
            List<string> subCategoryIds, 
            List<string> statusIds, 
            bool isActive, 
            string sortBy,
            Dictionary<string, string> sorts, 
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue
        );
        SOPProcess GetSOPProcessById(string siteId, string Id);
        Task<Vsky.Models.SOPProcess> GetSOPProcessByIdInDetail(string siteId, string Id);
        Task<SOPProcess> GetSOPProcessByTitle(string SiteId, string title, int number = 0, string id = null);
        Task<int> GetLastSOPProcessNumber();
        Task<string> GetNextSOPProcessVersion(string currentVersion = null, bool createMajorVersion = false);
        void InsertSOPProcess(SOPProcess entity);
        void UpdateSOPProcess(SOPProcess entity);
        void DeleteSOPProcess(SOPProcess entity);
    }
}
