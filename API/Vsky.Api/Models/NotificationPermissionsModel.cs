using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record NotificationPermissionsModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string NotificationId { get; set; }
        public string AspNetUserId { get; set; }
        public bool Active { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual MasterNotification Notification { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
    public record NotificationPermissionsSearchModel : BaseSearchModel
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public string SearchText { get; set; }
    }

    public record NotificationPermissionsListModel : BasePagedListModel<NotificationPermissionsModel>
    {
        public bool editing { get; set; }
    }
}
