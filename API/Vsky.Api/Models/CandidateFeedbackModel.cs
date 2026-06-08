using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record CandidateFeedbackModel : BaseEntityModel
    {
        public string CandidateId { get; set; }
        public string EmployeeOwnerId { get; set; }
        public string QuestionId { get; set; }
        public string QuestionTypeId { get; set; }
        public string Answer { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string CreatedById { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Candidate Candidates { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual DropDown CandidateQuestions { get; set; }
        public virtual DropDown CandidateQuestionType { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

    public record CandidateFeedbackSearchModel : BaseSearchModel
    {
    }
    public record CandidateFeedbackListModel : BasePagedListModel<CandidateFeedbackModel>
    {
        public bool editing { get; set; }
    }
}
