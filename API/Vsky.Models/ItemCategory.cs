using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ItemCategoryList
    {
        public virtual ICollection<ItemCategory> ItemCategoriesList { get; set; } = new List<ItemCategory>();
        public int Total { get; set; }
    }
    public class ItemCategory : BaseEntity
    {
        public string Name { get; set; }
        public string GroupName { get; set; }
        public string Prefix { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public int TotalItemSubcategoryCount { get; set; }
        [NotMapped]
        public int TotalSitesItemSubCategoryAttributesMappingCount { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual ICollection<ItemSubcategory> ItemSubcategory { get; set; } = new List<ItemSubcategory>();
    }
    public class ItemSubcategory : BaseEntity
    {
        public string ItemCategoryId { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public int TotalSitesItemSubCategoryAttributesMappingCount { get; set; }

        public virtual ItemCategory ItemCategory { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<SitesItemSubCategoryAttributesMapping> SitesItemSubCategoryAttributesMapping { get; set; } = new List<SitesItemSubCategoryAttributesMapping>();

    }
    public class SaveItemCategory
    {
        public string Name { get; set; }
        public string GroupName { get; set; }
        public string Prefix { get; set; }
        public virtual ICollection<ItemSubcategory> ItemSubcategoryList { get; set; } = new List<ItemSubcategory>();
    }

    public class SaveItemSubcategory
    {
        public string ItemCategoryId { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public int SortOrder { get; set; }
    }
}
