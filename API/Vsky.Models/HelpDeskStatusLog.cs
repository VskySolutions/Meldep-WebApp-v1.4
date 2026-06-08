using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class HelpDeskStatusLog : BaseEntity
{
    public string HelpDeskId { get; set; }
    public string StatusId { get; set; }
    public int DurationInMinutes { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    [NotMapped]
    public string CreatedDate { get; set; }
    [NotMapped]
    public string StatusDurationText { get; set; }
    public bool Deleted { get; set; }

    public virtual HelpDesk HelpDesk { get; set; }
    public virtual DropDown Status { get; set; }
    public virtual ApplicationUser CreatedBy { get; set; }
}
