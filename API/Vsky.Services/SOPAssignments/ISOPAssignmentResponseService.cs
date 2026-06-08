using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.SOPAssignments
{
    public interface ISOPAssignmentResponseService
    {
        Task<SOPAssignmentResponse> GetSOPAssignmentResponseById(string Id);
        void InsertSOPAssignmentResponse(SOPAssignmentResponse entity);
        void UpdateSOPAssignmentResponse(SOPAssignmentResponse entity);
        void DeleteSOPAssignmentResponse(SOPAssignmentResponse entity);
    }
}
