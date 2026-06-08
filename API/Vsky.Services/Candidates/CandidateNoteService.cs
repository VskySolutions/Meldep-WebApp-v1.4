using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Services.Candidates
{
    public class CandidateNoteService : ICandidateNoteService
    {
        #region Service Initialization
        private readonly IRepository<CandidateNotes> _noteRepository;
        public CandidateNoteService(IRepository<CandidateNotes> noteRepository)
        {
            _noteRepository = noteRepository;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetById
        public async Task<CandidateNotes> GetById(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var query = await _noteRepository.TableNoTracking.Where(x => x.Id == id).FirstOrDefaultAsync();

                return query;
            }
            return null;
        }

        public async Task<CandidateNotes> GetByCandidateId(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var query = await _noteRepository.TableNoTracking.Where(x => x.CandidateId == id).FirstOrDefaultAsync();
                return query;
            }
            return null;
        }

        #endregion

        #region InsertCandidateNote
        public void InsertCandidateNote(CandidateNotes entity)
        {
            _noteRepository.Insert(entity);
        }
        #endregion

        #region UpdateCandidateNote
        public void UpdateCandidateNote(CandidateNotes entity)
        {
            _noteRepository.Update(entity);
        }
        #endregion
    }
}
