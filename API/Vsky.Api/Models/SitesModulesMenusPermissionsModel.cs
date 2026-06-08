using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record SitesModulesMenusPermissionsModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string SiteRoleId { get; set; }
        public string SiteModuleMenuId { get; set; }
        public bool IsShowMenu { get; set; }
        public bool IsManage { get; set; }
        public bool IsView { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public string PermissionType { get; set; }
        [NotMapped]
        public string SiteRoleIds { get; set; }

        public virtual Site Site { get; set; }
        public virtual SitesRoles SitesRoles { get; set; }
        public virtual SitesModulesMenus SitesModulesMenus { get; set; }

    }
    public record SitesModulesMenusPermissionsSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public List<string> ModuleIds { get; set; }
        public List<string> MenuIds { get; set; }
        public List<string> RoleIds { get; set; }
        public string SiteId { get; set; }
    }
    public record SitesModulesMenusPermissionsListModel : BasePagedListModel<SitesModulesMenusPermissionsModel>
    {
    }
}
