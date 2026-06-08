using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record MasterNotificationModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string Number { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual ICollection<NotificationPermissions> NotificationPermissions { get; set; } = new List<NotificationPermissions>();
    }
    public record MasterNotificationSearchModel : BaseSearchModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public record MasterNotificationListModel : BasePagedListModel<MasterNotificationModel>
    {
        public bool editing { get; set; }
    }
    public record MasterNotificationUploadModel : BaseEntityModel
    {
    }
}
