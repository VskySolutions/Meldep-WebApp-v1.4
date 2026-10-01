using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.RequirementDataMappings
{
    public interface IRequirementDataMappingNotesService
    {
        #region GetAllRequirementDataMappingNotesByRequirementMappingId
        Task<List<RequirementDataMappingNotes>> GetAllRequirementDataMappingNotesByRequirementMappingId(string SiteId, string requirementMappingId, bool latestOnTop);
        #endregion

        #region GetRequirementDataMappingNotesById
        Task<RequirementDataMappingNotes> GetRequirementDataMappingNotesById(string id);
        #endregion

        #region InsertRequirementDataMappingNotes
        void InsertRequirementDataMappingNotes(RequirementDataMappingNotes entity);
        #endregion

        #region UpdateRequirementDataMappingNotes
        void UpdateRequirementDataMappingNotes(RequirementDataMappingNotes entity);
        #endregion

        #region DeleteRequirementDataMappingNotes
        void DeleteRequirementDataMappingNotes(RequirementDataMappingNotes entity);
        #endregion
    }
}
