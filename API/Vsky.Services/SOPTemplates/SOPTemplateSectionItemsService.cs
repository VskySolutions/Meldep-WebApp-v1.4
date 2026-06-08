using System.Linq;
using System.Linq.Dynamic.Core;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.SOPTemplates
{
    public class SOPTemplateSectionItemsService : ISOPTemplateSectionItemsService
    {
        #region Define services

        private readonly IRepository<SOPTemplateSectionItems> _sOPTemplateSectionItemsRepository;
        public SOPTemplateSectionItemsService(IRepository<SOPTemplateSectionItems> sOPTemplateSectionItemsRepository)
        {
            _sOPTemplateSectionItemsRepository = sOPTemplateSectionItemsRepository;
        }

        #endregion

        #region Get By Id

        public SOPTemplateSectionItems GetSOPTemplateSectionItemById(string Id)
        {
            var query = _sOPTemplateSectionItemsRepository.TableNoTracking.FirstOrDefault(x => !x.Deleted && x.Id == Id);
            return query ?? null;
        }

        #endregion

        #region Insert Update Delete
        public void InsertSOPTemplateSectionItem(SOPTemplateSectionItems entity)
        {
            _sOPTemplateSectionItemsRepository.Insert(entity);
        }

        public void UpdateSOPTemplateSectionItem(SOPTemplateSectionItems entity)
        {
            _sOPTemplateSectionItemsRepository.Update(entity);
        }

        public void DeleteSOPTemplateSectionItem(SOPTemplateSectionItems entity)
        {
            _sOPTemplateSectionItemsRepository.Update(entity);
        }
        #endregion
    }
}
