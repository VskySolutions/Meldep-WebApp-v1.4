using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class SitesItemList
    {
        public virtual ICollection<SitesItems> SitesItems { get; set; } = new List<SitesItems>();
        public int Total { get; set; }
    }
    public class SitesItems : BaseEntity
    {
        public string SiteId { get; set; }
        public string ItemSubCategoryId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual ItemSubcategory ItemSubcategory { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual ICollection<SitesItemsAttributes> SitesItemsAttributeList { get; set; } = new List<SitesItemsAttributes>();
    }
    public class SitesItemsAttributes : BaseEntity
    {
        public string SiteItemId { get; set; }
        public string ItemSubCategoryAttributeId { get; set; }
        public string Value { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public string Flag { get; set; }

        public virtual SitesItems SitesItems { get; set; }
        public virtual ItemSubCategoryAttributes ItemSubCategoryAttributes { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
    }

    public class SaveSitesItem
    {
        public string ItemSubCategoryId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
       
        public virtual ICollection<SitesItemsAttributes> SitesItemsAttributesList { get; set; } = new List<SitesItemsAttributes>();
    }
}
