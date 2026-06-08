using System;
using Vsky.Core;

namespace Vsky.Models;

public class HelpDeskFiles : BaseEntity
{
    public string HelpDeskId { get; set; }
    public string FileId { get; set; }
    public string CreatedById { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public bool Deleted { get; set; }
    public virtual HelpDesk HelpDesk { get; set; }
    public virtual Picture File { get; set; }
}

