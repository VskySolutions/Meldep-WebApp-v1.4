using System;
using Vsky.Core;

namespace Vsky.Models;

public class ProjectFiles : BaseEntity
{
    public string ProjectId { get; set; }
    public string FileId { get; set; }
    public string CreatedById { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public bool Deleted { get; set; }
    public virtual Project Project { get; set; }
    public virtual Picture File { get; set; }
    public virtual ApplicationUser CreatedBy { get; set; }
}