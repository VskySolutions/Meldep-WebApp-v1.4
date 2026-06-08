using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Note
{
    public interface INoteService
    {
        #region GetById
        Task<Notes> GetById(string id);
        #endregion

        #region GetAllNoteByTypeAndRecord
        List<Notes> GetAllNoteByTypeAndRecord(string SiteId, string subModuleId, string Type, bool latestOnTop);
        List<Notes> GetAllNoteByTypeAndRecordIdForCandidate(string SiteId, string subModuleId, string Type);
        IPagedList<Notes> GetAllNotesByProjectId(string SiteId, string projectId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);

        #endregion

        #region InsertNote
        void InsertNote(Notes entity);
        #endregion

        #region UpdateNote
        void UpdateNote(Notes entity);
        #endregion

        #region DeleteNotes
        void DeleteNotes(Notes entity);
        #endregion

    }
}
