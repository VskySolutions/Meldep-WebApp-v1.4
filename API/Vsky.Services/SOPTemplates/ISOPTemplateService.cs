using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.SOPTemplates
{
    public interface ISOPTemplateService
    {
        IPagedList<SOPTemplate> GetAllSOPTemplates(string searchText, string siteId, string name, bool isActive, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue);
        Task<List<SOPTemplate>> GetSOPTemplatesListForDropdown(string SiteId);
        SOPTemplate GetSOPTemplateById(string siteId, string Id);
        SOPTemplate GetSOPTemplateByIdInDetail(string siteId, string Id);
        void InsertSOPTemplate(SOPTemplate entity);
        void UpdateSOPTemplate(SOPTemplate entity);
        void DeleteSOPTemplate(SOPTemplate entity);
    }
}
