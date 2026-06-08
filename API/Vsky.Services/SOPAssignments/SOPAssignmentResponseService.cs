using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.SOPAssignments
{
    public class SOPAssignmentResponseService : ISOPAssignmentResponseService
    {
        #region Define services

        private readonly IRepository<SOPAssignmentResponse> _sOPAssignmentResponseRepository;
        public SOPAssignmentResponseService(IRepository<SOPAssignmentResponse> sOPAssignmentResponseRepository)
        {
            _sOPAssignmentResponseRepository = sOPAssignmentResponseRepository;
        }

        #endregion

        #region Get By Id

        public async Task<SOPAssignmentResponse> GetSOPAssignmentResponseById(string Id)
        {
            var query = _sOPAssignmentResponseRepository.TableNoTracking.FirstOrDefault(x => !x.Deleted && x.Id == Id);
            return query ?? null;
        }

        #endregion

        #region Insert Update Delete
        public void InsertSOPAssignmentResponse(SOPAssignmentResponse entity)
        {
            _sOPAssignmentResponseRepository.Insert(entity);
        }

        public void UpdateSOPAssignmentResponse(SOPAssignmentResponse entity)
        {
            _sOPAssignmentResponseRepository.Update(entity);
        }

        public void DeleteSOPAssignmentResponse(SOPAssignmentResponse entity)
        {
            _sOPAssignmentResponseRepository.Update(entity);
        }
        #endregion
    }
}
