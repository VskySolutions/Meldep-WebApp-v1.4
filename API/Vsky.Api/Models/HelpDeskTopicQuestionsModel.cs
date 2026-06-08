using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record HelpDeskTopicQuestionsModel : BaseEntityModel
    {
        public string TopicId { get; set; }
        public string Question { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool IsActive { get; set; }
        public bool Deleted { get; set; }
        public virtual HelpDeskTopic HelpDeskTopic { get; set; }
    }

    public record HelpDeskTopicQuestionsSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public string Topic { get; set; }
        public string Question { get; set; }
    }
}
