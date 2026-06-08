using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record CandidateDepartmentModel : BaseEntityModel
    {
        public string CandidateId { get; set; }
        public string DepartmentsId { get; set; }

        public virtual Candidate Candidates { get; set; }
        public virtual Department Departments { get; set; }
    }

    public record CandidateDepartmentSearchModel : BaseSearchModel
    {
    }

    public record CandidateDepartmentListModel : BasePagedListModel<CandidateDepartmentModel>
    {
        public bool editing { get; set; }
    }
}
