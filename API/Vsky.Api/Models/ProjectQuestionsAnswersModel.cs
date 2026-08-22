using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record ProjectQuestionsAnswersModel : BaseEntityModel
    {
    }

    public record ProjectQuestionsAnswersSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public string Title { get; set; }
        public string ProjectId { get; set; }
        public string RequirementId { get; set; }
        public List<string> ProjectIds { get; set; }
        public List<string> RequirementIds { get; set; }
    }
    public record ProjectQuestionsAnswersListModel : BasePagedListModel<ProjectQuestionsAnswersModel>
    {
    }
}


