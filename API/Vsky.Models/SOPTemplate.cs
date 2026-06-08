using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models
{
    public class SOPTemplate : BaseEntity
    {
        public string SiteId { get; set; }

        public string Name { get; set; }
        public int? SortOrder { get; set; }
        public string Description { get; set; }
        public decimal Version { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual ICollection<SOPTemplateSection> SOPTemplateSections { get; set; } = new List<SOPTemplateSection>();
    }

    public class SOPTemplateSection : BaseEntity
    {
        public string TemplateId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual SOPTemplate Template { get; set; }
        public virtual ICollection<SOPTemplateSectionItems> SOPTemplateSectionItems { get; set; } = new List<SOPTemplateSectionItems>();
    }

    public class SOPTemplateSectionItems : BaseEntity
    {
        public string TemplateId { get; set; }
        public string SectionId { get; set; }
        public string InputTypeId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsRequiredEvidence { get; set; }
        public string ValidationJson { get; set; }
        [NotMapped]
        public string UpdatedDateStr { get; set; }

        public int SortOrder { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual SOPTemplate Template { get; set; }
        public virtual SOPTemplateSection Section { get; set; }
        public virtual DropDown InputType { get; set; }
    }
}
