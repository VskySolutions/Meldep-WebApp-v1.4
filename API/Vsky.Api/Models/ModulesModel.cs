using System.Collections.Generic;
using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ModulesModel : BaseEntityModel
    {
        public string Name { get; set; }

        public int Sortorder { get; set; }

        public bool Active { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public string UpdatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public bool Deleted { get; set; }

        public bool IsModuleActivatedForSite { get; set; }

        // public virtual CompanyModule ActivatedModule { get; set; }
        // public virtual SitesModulesMenus ActivatedMenu { get; set; }
        public virtual ICollection<ModulesMenus> ModulesMenus { get; set; } = new List<ModulesMenus>();
        public virtual ICollection<SitesModules> SiteModules { get; set; } = new List<SitesModules>();
        public virtual ICollection<SitesModulesMenusPermissions> SitesModulesMenusPermissions { get; set; } = new List<SitesModulesMenusPermissions>();

    }

    public record ModulesSearchModel : BaseSearchModel
    {
        public string Name { get; set; }
    }

    public record ModulesListModel : BasePagedListModel<ModulesModel> { }
}
