using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.SOPAssignments
{
    public interface ISOPAssignmentResponseEvidencesService
    {
        Task<SOPAssignmentResponseEvidences> GetSOPAssignmentResponseEvidenceById(string Id);
        Task<List<SOPAssignmentResponseEvidences>> GetSOPAssignmentResponseEvidenceByResponseId(string responseId);
        void InsertSOPAssignmentResponseEvidence(SOPAssignmentResponseEvidences entity);
        void UpdateSOPAssignmentResponseEvidence(SOPAssignmentResponseEvidences entity);
        void DeleteSOPAssignmentResponseEvidence(SOPAssignmentResponseEvidences entity);
    }
}
