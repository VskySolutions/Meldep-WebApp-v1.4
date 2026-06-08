using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class Modules : BaseEntity
    {
        public string Name { get; set; }

        public int Sortorder { get; set; }

        public bool Active { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public string UpdatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public bool Deleted { get; set; }

        //public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();
        [NotMapped]
        public bool IsModuleActivatedForSite { get; set; }        


        [NotMapped]
        public virtual SitesModules ActivatedModule { get; set; }
        //[NotMapped]
        //public virtual SitesModulesMenus ActivatedMenu { get; set; }
        public virtual ICollection<ModulesMenus> ModulesMenus { get; set; } = new List<ModulesMenus>();
        public virtual ICollection<SitesModules> SiteModules { get; set; } = new List<SitesModules>();
       
    }
}
