using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ItemSubCategoryAttributes : BaseEntity
    {
        public string Name { get; set; }
        public string FieldType { get; set; }

        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public int TotalItemSubCategoryAttributesValuesCount { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<ItemSubCategoryAttributesValues> ItemSubCategoryAttributesValues { get; set; } = new List<ItemSubCategoryAttributesValues>();
        public virtual ICollection<SitesItemSubCategoryAttributesMapping> SitesItemSubCategoryAttributesMapping { get; set; } = new List<SitesItemSubCategoryAttributesMapping>();
    }
    public class ItemSubCategoryAttributesValues : BaseEntity
    {
        public string ItemSubCategoryAttributeId { get; set; }
        public string ItemSubCategoryId { get; set; }
        public string CreatedById { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public int SortOrder { get; set; }
       
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ItemSubcategory ItemSubcategory { get; set; }
        public virtual ItemSubCategoryAttributes ItemSubCategoryAttributes { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
    }

    public class SaveItemSubCategoryAttributes
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FieldType { get; set; }
    }

    public class SaveItemSubCategoryAttributesValues
    {
        public string ItemSubCategoryAttributeId { get; set; }
        public string ItemSubCategoryId { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public int SortOrder { get; set; }
    }
}
