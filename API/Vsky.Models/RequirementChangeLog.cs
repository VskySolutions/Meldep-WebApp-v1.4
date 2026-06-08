using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class RequirementChangeLog : BaseEntity
{
    public string RequirementId { get; set; }
    public string EmployeeId { get; set; }
    public DateTime RequirementLogDate { get; set; }
    public string Description { get; set; }

    public string RequirementName { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public string CreatedById { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public bool Deleted { get; set; }

    [NotMapped]
    public string CreatedTimeStr { get; set; }

    public virtual Requirement Requirement { get; set; }
    public virtual Employee Employee { get; set; }
}

