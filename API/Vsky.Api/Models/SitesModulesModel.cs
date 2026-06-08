using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record SitesModulesModel : BaseEntityModel
    {
        public string SiteId { get; set; }

        public string ModuleId { get; set; }
        public string MenuId { get; set; }
        public bool Active { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public bool ModuleStatus { get; set; }
        public bool ModuleMenuStatus { get; set; }

        public virtual SiteModel Site { get; set; }
        public virtual ModulesModel Modules { get; set; }
        //public virtual ICollection<ModulesMenus> ModulesMenus { get; set; } = new List<ModulesMenus>();
        public virtual ICollection<SitesModulesMenus> SitesModulesMenus { get; set; } = new List<SitesModulesMenus>();
        // public virtual ICollection<ModulesMenus> ModulesMenus { get; set; } = new List<ModulesMenus>();
    }
}
