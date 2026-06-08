using System;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models;

public class ProjectUserMapping : BaseEntity
{
    public string ProjectId { get; set; }
    public string AspNetUserId { get; set; }

    public bool FullAccess { get; set; }
    public bool ViewOnly { get; set; }
    public bool Notes { get; set; }

    public DateTime? CreatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public bool Deleted { get; set; }

    public virtual Project Project { get; set; }
    public virtual VW_Project VW_Project { get; set; }
    public virtual ApplicationUser User { get; set; }
}