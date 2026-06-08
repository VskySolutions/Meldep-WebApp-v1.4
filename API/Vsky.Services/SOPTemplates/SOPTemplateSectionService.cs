using System.Linq;
using System.Linq.Dynamic.Core;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.SOPTemplates
{
    public class SOPTemplateSectionService : ISOPTemplateSectionService
    {
        #region Define services

        private readonly IRepository<SOPTemplateSection> _sOPTemplateSectionRepository;
        public SOPTemplateSectionService(IRepository<SOPTemplateSection> sOPTemplateSectionRepository)
        {
            _sOPTemplateSectionRepository = sOPTemplateSectionRepository;
        }

        #endregion

        #region Get By Id

        public SOPTemplateSection GetSOPTemplateSectionById(string Id)
        {
            var query = _sOPTemplateSectionRepository.TableNoTracking.FirstOrDefault(x => !x.Deleted && x.Id == Id);
            return query ?? null;
        }

        #endregion

        #region Insert Update Delete
        public void InsertSOPTemplateSection(SOPTemplateSection entity)
        {
            _sOPTemplateSectionRepository.Insert(entity);
        }

        public void UpdateSOPTemplateSection(SOPTemplateSection entity)
        {
            _sOPTemplateSectionRepository.Update(entity);
        }

        public void DeleteSOPTemplateSection(SOPTemplateSection entity)
        {
            _sOPTemplateSectionRepository.Update(entity);
        }
        #endregion
    }
}
