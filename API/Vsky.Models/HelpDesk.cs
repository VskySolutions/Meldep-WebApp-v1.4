using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class HelpDesk : BaseEntity
{
    [NotMapped]
    public string StatusId { get; set; }
    [NotMapped]
    public string StatusText { get; set; }
    [NotMapped]
    public string PreviousStatusText { get; set; }
    [NotMapped]
    public DateTime CreatedDate { get; set; }
    [NotMapped]
    public string CreatedDateStr { get; set; }
    [NotMapped]
    public string UpdatedDateStr { get; set; }
    [NotMapped]
    public string DateStr { get; set; }

    [NotMapped]
    public int EmailRepliesCount { get; set; }

    [NotMapped]
    public int HelpDeskNotesCount { get; set; }

    [NotMapped]
    public string OwnerId { get; set; }

    [NotMapped]
    public string SitePrefix { get; set; }

    [NotMapped]
    public int AssignedToCount { get; set; }

    [NotMapped]
    public string AverageDurationText { get; set; }
    public string TwilioEmailId { get; set; }
    public string SiteId { get; set; }
    public string RequesterId { get; set; }
    public string TopicId { get; set; }
    public string QuestionId { get; set; }
    //public DateTime Date { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int AverageDurationInMinutes { get; set; }
    public DateTime? CreatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public bool Deleted { get; set; }
    public int TicketNo { get; set; }
    public string PriorityId { get; set; }
    public string CompanyId { get; set; }
    public string CategoryId { get; set; }
    public string AssignedToId { get; set; }
    public string RequesterEmail { get; set; }
    public string ClosingComment { get; set; }

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
    //public virtual ICollection<HelpDeskEmailRepliesMapping> HelpDeskEmailRepliesMapping { get; set; } = new List<HelpDeskEmailRepliesMapping>();
}
public class RequesterDropdownDto
{
    public string HelpDeskId { get; set; }
    public string Email { get; set; }
}
public class CompanyDropdownDto
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class HelpDeskReminderLog : BaseEntity
{
    public string HelpDeskId { get; set; }
    public string SiteEmailNotificationId { get; set; }
    public string ToEmail { get; set; }
    public DateTime Date { get; set; }
    public string IsRequesterOrSupport { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    public virtual HelpDesk HelpDesk { get; set; }
    public virtual SitesEmailNotifications SitesEmailNotifications { get; set; }
}

