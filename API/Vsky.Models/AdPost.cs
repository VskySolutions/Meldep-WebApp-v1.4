using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class AdPost : BaseEntity
{
    public string SiteId { get; set; }
    public string PictureId { get; set; }
    public string ProjectId { get; set; }
    public string CustomerId { get; set; }
    public string ImageType { get; set; }
    public string ImageProviderClientId { get; set; }
    public string ImageProviderEmpId { get; set; }
    public string ContentType { get; set; }
    public string ContentProviderClientId { get; set; }
    public string ContentProviderEmpId { get; set; }
    public int AdNumber { get; set; }
    public string Name { get; set; }
    public string URL { get; set; }
    public string Description { get; set; }
    public string Caption { get; set; }
    public string Tags { get; set; }
    public string CreatedById { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public bool Deleted { get; set; }

    public virtual Picture Picture { get; set; }
    public virtual CompanyClients Customer { get; set; }
    public virtual Project Project { get; set; }
    public virtual DropDown ImageTypeDropDown { get; set; }
    public virtual DropDown ContentTypeDropDown { get; set; }
    public virtual Person ImageProviderClient { get; set; }
    public virtual Person ContentProviderClient { get; set; }
    public virtual Employee ImageProviderEmp { get; set; }
    public virtual Employee ContentProviderEmp { get; set; }
    public virtual ApplicationUser CreatedBy { get; set; }
    public virtual ApplicationUser UpdatedBy { get; set; }
    public virtual ICollection<AdPostingStatus> AdPostingStatusList { get; set; } = new List<AdPostingStatus>();
}

