using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class NotificationDetails : BaseEntity
{
    public string NotificationId { get; set; }
    public string ToUserId { get; set; }
    public int IsRead { get; set; }

    public virtual ApplicationUser User { get; set; }
    public virtual Notification Notification { get; set; }
}
