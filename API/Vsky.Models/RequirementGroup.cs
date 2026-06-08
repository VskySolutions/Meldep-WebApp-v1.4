using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class RequirementGroup : BaseEntity
{
    public string ProjectId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int RequirementGroupNumber { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public string CreatedById { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public bool Deleted { get; set; }

    public virtual Project Project { get; set; }
    //public virtual ICollection<ProjectUserMapping> ProjectUserMappings { get; set; } = new List<ProjectUserMapping>();
}
