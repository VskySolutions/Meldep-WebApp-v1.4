using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class AdPostChannel : BaseEntity
{
    public string ProjectId { get; set; }
    public string SiteId { get; set; }
    public string CustomerId { get; set; }
    public int ChannelNumber { get; set; }
    public string Name { get; set; }
    public int GroupMemberCount { get; set; }
    public string Description { get; set; }
    public string CreatedById { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public bool Deleted { get; set; }

    public virtual CompanyClients Customer { get; set; }
    public virtual Project Project { get; set; }
    public virtual ApplicationUser CreatedBy { get; set; }
    public virtual ApplicationUser UpdatedBy { get; set; }
}

