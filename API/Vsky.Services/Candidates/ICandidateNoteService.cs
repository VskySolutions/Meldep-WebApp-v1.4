using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.Candidates
{
    public interface ICandidateNoteService
    {
        #region GetById
        Task<CandidateNotes> GetById(string id);

        Task<CandidateNotes> GetByCandidateId(string id);
        #endregion

        #region InsertCandidateNote
        void InsertCandidateNote(CandidateNotes entity);
        #endregion

        #region UpdateCandidateNote
        void UpdateCandidateNote(CandidateNotes entity);
        #endregion

    }
}
