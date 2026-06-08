using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ModulesMenusModel : BaseEntityModel
    {
        public string ModuleId { get; set; }

        public string MenuName { get; set; }

        public string DisplayName { get; set; }

        public int Sortorder { get; set; }

        public string ParentMenuId { get; set; }

        public bool Active { get; set; }
        public bool IsQuickLink { get; set; }

        public string Icon { get; set; }

        public string Link { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public string UpdatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public bool Deleted { get; set; }

        public bool OpenInNewTab { get; set; }
        public virtual ICollection<SitesModulesMenusModel> SitesModulesMenus { get; set; } = new List<SitesModulesMenusModel>();
    }
}
