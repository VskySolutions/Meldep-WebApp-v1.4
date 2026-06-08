using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.SOPAssignments
{
    public class SOPAssignmentResponseEvidencesService : ISOPAssignmentResponseEvidencesService
    {
        #region Define services

        private readonly IRepository<SOPAssignmentResponseEvidences> _sOPAssignmentResponseEvidenceRepository;
        public SOPAssignmentResponseEvidencesService(IRepository<SOPAssignmentResponseEvidences> sOPAssignmentResponseEvidenceRepository)
        {
            _sOPAssignmentResponseEvidenceRepository = sOPAssignmentResponseEvidenceRepository;
        }

        #endregion

        #region Get By Id
        public async Task<SOPAssignmentResponseEvidences> GetSOPAssignmentResponseEvidenceById(string Id)
        {
            var query = _sOPAssignmentResponseEvidenceRepository.TableNoTracking.FirstOrDefault(x => !x.Deleted && x.Id == Id);
            return query ?? null;
        }

        public async Task<List<SOPAssignmentResponseEvidences>> GetSOPAssignmentResponseEvidenceByResponseId(string responseId)
        {
            var query = _sOPAssignmentResponseEvidenceRepository.TableNoTracking
                .Where(x => !x.Deleted && x.ResponseId == responseId)
                .ToList();

            return query;
        }
        #endregion

        #region Insert Update Delete
        public void InsertSOPAssignmentResponseEvidence(SOPAssignmentResponseEvidences entity)
        {
            _sOPAssignmentResponseEvidenceRepository.Insert(entity);
        }

        public void UpdateSOPAssignmentResponseEvidence(SOPAssignmentResponseEvidences entity)
        {
            _sOPAssignmentResponseEvidenceRepository.Update(entity);
        }

        public void DeleteSOPAssignmentResponseEvidence(SOPAssignmentResponseEvidences entity)
        {
            entity.Deleted = true;
            _sOPAssignmentResponseEvidenceRepository.Update(entity);
        }
        #endregion
    }
}
