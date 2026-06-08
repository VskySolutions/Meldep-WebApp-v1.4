using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class Notification : BaseEntity
{
    public string SiteId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }
    public string FromUserId { get; set; }
    public string RecordId { get; set; }
    public string RedirectURL { get; set; }
    public string CreatedById { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    public virtual Site Site { get; set; }
    public virtual ApplicationUser User { get; set; }
    public virtual ICollection<NotificationDetails> NotificationDetails { get; set; } = new List<NotificationDetails>();
}
