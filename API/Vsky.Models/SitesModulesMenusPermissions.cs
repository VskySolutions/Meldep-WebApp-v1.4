using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models
{
    public class SitesModulesMenusPermissions : BaseEntity
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

        public virtual Site Site { get; set; }
        public virtual SitesRoles SitesRoles { get; set; }
        public virtual SitesModulesMenus SitesModulesMenus { get; set; }

    }
    public class RoleDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class ModuleMenuRoleDto
    {
        public string SiteModuleMenuId { get; set; }
        public string ModuleName { get; set; }
        public string MenuName { get; set; }
        public string MenuId { get; set; }
        public List<RoleDto> Roles { get; set; }
    }
}
