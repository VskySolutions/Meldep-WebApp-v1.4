using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class JobCreate : BaseEntity
{
    public string SiteId { get; set; }
    public string JobDescription { get; set; }
    public string Criteria { get; set; }
    public string JobTitle { get; set; }
    public DateTime? JobCreatedDate { get; set; }
    public DateTime PublishedJobDate { get; set; }
    public string JobReference { get; set; }
    public string CreatedById { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }
    public bool IsActive { get; set; }

    public virtual Site Site { get; set; }
}
