using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record  SitesModulesMenusModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string SiteModuleId { get; set; }
        public string MenuId { get; set; }
        public bool Active { get; set; }
        public int SortOrder { get; set; }
        public bool SetAsLanding { get; set; }
        public bool IsQuickLink { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual SitesModules SitesModules { get; set; }
        public virtual ModulesMenus ModulesMenus { get; set; }
        public virtual ICollection<SitesModulesMenusPermissions> SitesModulesMenusPermissions { get; set; } = new List<SitesModulesMenusPermissions>();
    }
    public record SitesModulesMenusListModel : BasePagedListModel<SitesModulesMenusModel>
    {
        public bool editing { get; set; }
    }
}
