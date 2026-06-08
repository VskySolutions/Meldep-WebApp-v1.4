using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class SitesModules : BaseEntity
    {
        public string SiteId { get; set; }
        public string ModuleId { get; set; }
        public bool Active { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public virtual Site Site { get; set; }
        public virtual Modules Modules { get; set; }
        public virtual ICollection<SitesModulesMenus> SitesModulesMenus { get; set; } = new List<SitesModulesMenus>();
        
    }

    public class CustomSiteModule
    {
        public string Name { get; set; }
        public List<CustomSiteModuleMenu> CustomSiteModuleMenuList { get; set; }

        public class CustomSiteModuleMenu
        {
            public string DisplayName { get; set; }
            public string Link { get; set; }
            public string Icon { get; set; }
            public bool SetAsLanding { get; set; }
            public bool OpenInNewTab { get; set; }
        }
    }
}
