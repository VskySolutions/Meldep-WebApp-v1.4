using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record SOPTemplateModel : BaseEntityModel
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

        public virtual ICollection<SOPTemplateSectionModel> SOPTemplateSections { get; set; } = new List<SOPTemplateSectionModel>();
    }

    public record SOPTemplateSectionModel : BaseEntityModel 
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

        public virtual SOPTemplateModel Template { get; set; }
        public virtual ICollection<SOPTemplateSectionItemsModel> SOPTemplateSectionItems { get; set; } = new List<SOPTemplateSectionItemsModel>();
    }

    public record SOPTemplateSectionItemsModel : BaseEntityModel
    {
        public string TemplateId { get; set; }
        public string SectionId { get; set; }
        public string InputTypeId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsRequiredEvidence { get; set; }
        public string ValidationJson { get; set; }
        public string UpdatedDateStr { get; set; }

        public int SortOrder { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        public virtual SOPTemplateModel Template { get; set; }
        public virtual SOPTemplateSectionModel Section { get; set; }
        public virtual DropDownModel InputType { get; set; }
    }

    public record SOPTemplateSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    public record SOPTemplateListModel : BasePagedListModel<SOPTemplateModel>
    {

    }
}
