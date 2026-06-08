using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record CandidateNotesModel : BaseEntityModel
    {
        public string CandidateId { get; set; }
        public string NoteId { get; set; }

        public virtual Candidate Candidates { get; set; }
        public virtual Notes Note { get; set; }
    }
    public record CandidateNotesSearchModel : BaseSearchModel
    {
    }

    public record CandidateNotesListModel : BasePagedListModel<CandidateNotesModel>
    {
        public bool editing { get; set; }
    }
}
