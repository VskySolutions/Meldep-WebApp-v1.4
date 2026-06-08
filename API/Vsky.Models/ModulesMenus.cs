using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ModulesMenus : BaseEntity
    {
        public string ModuleId { get; set; }

        public string MenuName { get; set; }

        public string DisplayName { get; set; }

        public int Sortorder { get; set; }

        public string ParentMenuId { get; set; }

        public bool Active { get; set; }

        public string Icon { get; set; }

        public string Link { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public string UpdatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public bool Deleted { get; set; }

        public bool OpenInNewTab { get; set; }

        public virtual Modules Modules { get; set; }

        [NotMapped]
        public bool IsManageMenuPermission { get; set; }

        [NotMapped]
        public bool IsViewMenuPermission { get; set; }

        [NotMapped]
        public bool IsModuleMenuActivatedForSite { get; set; }

        [NotMapped]
        public bool IsShowMenuPermission { get; set; }

        public virtual ICollection<SitesModulesMenus> SitesModulesMenus { get; set; } = new List<SitesModulesMenus>();    
    }
}
