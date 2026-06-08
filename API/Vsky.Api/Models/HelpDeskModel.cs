using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record HelpDeskModel : BaseEntityModel
    {
        [NotMapped]
        public string StatusId { get; set; }
        [NotMapped]
        public string StatusText { get; set; }
        public string PreviousStatusText { get; set; }
        [NotMapped]
        public DateTime CreatedDate { get; set; }
        public int AssignedToCount { get; set; }
        public string CreatedDateStr { get; set; }
        public string SiteId { get; set; }
        public string RequesterId { get; set; }
        public string TopicId { get; set; }
        public string QuestionId { get; set; }
        //public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int AverageDurationInMinutes { get; set; }
        public string AverageDurationText { get; set; }

        public List<IFormFile> HelpDeskFiles { get; set; }
        public List<string> ExistingFiles { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public string DateStr { get; set; }
        public string TwilioEmailId { get; set; }
        public int TicketNo { get; set; }
        public string PriorityId { get; set; }
        public string CompanyId { get; set; }
        public string CategoryId { get; set; }
        public string AssignedToId { get; set; }
        public string RequesterEmail { get; set; }
        public string UpdatedDateStr { get; set; }

        public int EmailRepliesCount { get; set; }
        public int HelpDeskNotesCount { get; set; }
        public string OwnerId { get; set; }
        public string ClosingComment { get; set; }
        public string DisplayTicketNo { get; set; }
        public string SitePrefix { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Employee AssignedTo { get; set; }
        //public virtual DropDown Status { get; set; }
        public virtual DropDown Priority { get; set; }
        public virtual DropDown Category { get; set; }
        public virtual CompanyClients Company { get; set; }
        public virtual HelpDeskTopic HelpDeskTopic { get; set; }
        public virtual HelpDeskTopicQuestions HelpDeskTopicQuestions { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual Site Site { get; set; }
        public virtual ICollection<HelpDeskStatusLog> HelpDeskStatusLog { get; set; } = new List<HelpDeskStatusLog>();

    }
    public record HelpDeskSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public string Title { get; set; }
        public int TicketNo { get; set; }
        public string AssignedToId { get; set; }
        public DateTime? TicketFromDate { get; set; }
        public DateTime? TicketToDate { get; set; }
        public List<string> EmployeeIds { get; set; }
        public List<string> EmployeeEmails { get; set; }
        public List<string> StatusIds { get; set; }
        public List<string> PriorityIds { get; set; }
        public List<string> TopicIds { get; set; }
        public List<string> QuestionIds { get; set; }
        public List<string> CompanyIds { get; set; }
        public List<string> CategoryIds { get; set; }
    }

    public record HelpDeskListModel : BasePagedListModel<HelpDeskModel>
    {
        public bool editing { get; set; }
    }
    public record HelpDeskUploadModel : BaseEntityModel
    {
    }
}
