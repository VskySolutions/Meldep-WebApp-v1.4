using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record NotificationsModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string FromUserId { get; set; }
        public string RecordId { get; set; }
        public string RedirectURL { get; set; }
        public string UserName { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedOnUtcStr { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<NotificationDetails> NotificationDetails { get; set; } = new List<NotificationDetails>();
    }
    public record NotificationsSearchModel : BaseSearchModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SearchText { get; set; }
    }

    public record NotificationsListModel : BasePagedListModel<NotificationsModel>
    {
        public bool editing { get; set; }
    }
    public record NotificationsUploadModel : BaseEntityModel
    {
    }
}
