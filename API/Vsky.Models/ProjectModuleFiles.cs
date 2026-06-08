using System;
using Vsky.Core;

namespace Vsky.Models;

public class ProjectModuleFiles : BaseEntity
{
    public string ProjectModuleId { get; set; }
    public string FileId { get; set; }
    public string CreatedById { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public bool Deleted { get; set; }
    public virtual ProjectModule ProjectModule { get; set; }
    public virtual Picture File { get; set; }
    public virtual ApplicationUser CreatedBy { get; set; }
}