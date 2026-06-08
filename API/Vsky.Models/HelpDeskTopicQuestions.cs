using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class HelpDeskTopicQuestionsList
    {
        public virtual ICollection<HelpDeskTopicQuestions> HelpDeskTopicQuestionList { get; set; } = new List<HelpDeskTopicQuestions>();
        public int Total { get; set; }
    }
    public class HelpDeskTopicQuestions : BaseEntity
    {
        public string TopicId { get; set; }
        public string Question { get; set; }
        public string Description { get; set; }

        [NotMapped]
        public string Topic { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool IsActive { get; set; }
        public bool Deleted { get; set; }
        public virtual HelpDeskTopic HelpDeskTopic { get; set; }
    }
    public class HelpDeskTopicQuestionList
    {
        public string TopicId { get; set; }
        public string Topic { get; set; }
        public string Question { get; set; }
    }
}

