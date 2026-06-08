using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class NotificationPermissions : BaseEntity
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
