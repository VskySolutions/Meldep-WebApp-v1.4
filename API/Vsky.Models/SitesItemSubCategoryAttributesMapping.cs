using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class SitesItemSubCategoryAttributesMapping : BaseEntity
    {
        public string SiteId { get; set; }
        public string ItemSubCategoryId { get; set; }
        public string ItemSubCategoryAttributeId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }
       
        public virtual Site Site { get; set; }
        public virtual ItemSubcategory ItemSubcategory { get; set; }
        public virtual ItemSubCategoryAttributes ItemSubCategoryAttributes { get; set; }
    }

    public class SaveItemSubCategoryAttributesMapping
    {
        public string SiteId { get; set; }
        public string ItemSubCategoryId { get; set; }
        public string ItemSubCategoryAttributeId { get; set; }
        public bool Deleted { get; set; }
        public List<string> ItemSubCategoryAttributeIds { get; set; }
    }

}
