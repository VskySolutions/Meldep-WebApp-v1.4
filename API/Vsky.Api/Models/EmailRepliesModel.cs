using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public class EmailRepliesList
    {
        public List<EmailRepliesModel> EmailRepliesLists { get; set; }
        public List<string> ReplyToEmails { get; set; }
        public string FromEmail { get; set; }
        public string ToName { get; set; }
        public int Total { get; set; }
    }
    public record EmailRepliesModel : BaseEntityModel
    {
        public string OwnerId { get; set; }
        public string TwilioEmailId { get; set; }
        public string HelpDeskId { get; set; }
        public string FromEmail { get; set; }
        public string ToName { get; set; }
        public string FromName { get; set; }
        public string CCName { get; set; }
        public string ExternalName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ToEmail { get; set; }
        public string ExternalToEmail { get; set; }
        public string CCEmail { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public bool IsRead { get; set; }
        public bool IsSystemEmail { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedOnUtcStr { get; set; }
        public string CreatedOnStr { get; set; }
        public string CreatedBy { get; set; }
        public string StatusText { get; set; }

        public virtual HelpDesk HelpDesk { get; set; }
        public virtual Employee Employee { get; set; }
    }

    public record EmailTicketDto : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string CompanyId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        //public DateTime ReceivedAt { get; set; }
        //public List<AttachmentDto> Attachments { get; set; }
    }

    public class TwilioAttachmentInfo
    {
        public string filename { get; set; }
        public string type { get; set; }        // image/png, image/jpeg
        public string disposition { get; set; } // inline / attachment
        public string content_id { get; set; }  // CID reference
    }

    public record EmailRepliesListModel : BasePagedListModel<EmailRepliesModel>
    {
        public bool editing { get; set; }
    }
}
