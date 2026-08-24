using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models
{
    public class ProjectQuestionsAnswersList
    {
        public virtual ICollection<ProjectQuestionsAnswers> ProjectQuestionsAnswerList { get; set; } = new List<ProjectQuestionsAnswers>();
        public int Total { get; set; }
    }
    public class ProjectQuestionsAnswersResponseLogList
    {
        public virtual ICollection<ProjectQuestionsAnswersResponseLog> ProjectQuestionsAnswersResponseLogLists { get; set; } = new List<ProjectQuestionsAnswersResponseLog>();
        public int Total { get; set; }
    }

    public class ProjectQuestionsAnswers : BaseEntity
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }
        public string RequirementId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        [NotMapped] 
        public string LastAnswer { get; set; }

        public virtual Site Site { get; set; }
        public virtual Project Project { get; set; }
        public virtual Requirement Requirement { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<ProjectQuestionsAnswersResponseLog> ProjectQuestionsAnswersResponseLog { get; set; } = new List<ProjectQuestionsAnswersResponseLog>();
    }
    public class ProjectQuestionsAnswersResponseLog : BaseEntity
    {
        public string ProjectQuestionsAnswersId { get; set; }
        public string Description { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        [NotMapped]
        public string Flag { get; set; }
        

        public virtual ProjectQuestionsAnswers ProjectQuestionsAnswers { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
    }
    public class SaveProjectQuestionsAnswers
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string RequirementId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
        public virtual ICollection<ProjectQuestionsAnswersResponseLog> ProjectQuestionsAnswersResponseLogs { get; set; } = new List<ProjectQuestionsAnswersResponseLog>();
    }
}

