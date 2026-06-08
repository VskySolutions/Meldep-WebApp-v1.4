using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class SitesEmailNotificationList
    {
        public virtual ICollection<SitesEmailNotifications> SitesEmailNotificationsList { get; set; } = new List<SitesEmailNotifications>();
        public int Total { get; set; }
    }

    public class SitesEmailNotificationsPermissionList
    {
        public virtual ICollection<SitesEmailNotificationsPermission> SitesEmailNotificationsPermissionsList { get; set; } = new List<SitesEmailNotificationsPermission>();
        public int Total { get; set; }
    }

    public class SitesEmailNotifications : BaseEntity
    {
        public string SiteId { get; set; }
        public string MessageTemplateId { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string EmailAccountId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Sites { get; set; }
        public virtual MessageTemplate MessageTemplate { get; set; }
    }

    public class SitesEmailNotificationsPermission : BaseEntity
    {
        public string SiteId { get; set; }
        public string SiteEmailNotificationId { get; set; }
        public string UserId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Sites { get; set; }
        public virtual SitesEmailNotifications SitesEmailNotifications { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

    public class SaveSitesEmailNotifications
    {
        public string Id { get; set; }
        public string SiteId { get; set; }
        public string MessageTemplateId { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string EmailAccountId { get; set; }
        public bool Active { get; set; }
    }

    public class SaveSitesEmailNotificationsPermission
    {
        public string Id { get; set; }
        public string SiteId { get; set; }
        public string SiteEmailNotificationId { get; set; }
        public string UserId { get; set; }
        public bool Active { get; set; }
    }
}
