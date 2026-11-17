using System;
using System.Collections.Generic;

namespace Vsky.Api.Models
{
    public record SetUserRolesModel
    {
        public List<string> SiteRoleIds { get; set; }
    }

    public record SecurityGroupModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
    }

    public record GroupMembersModel
    {
        public List<string> UserIds { get; set; }
    }

    public record MenuPermissionModel
    {
        public string SiteModuleMenuId { get; set; }
        public bool ShowInMenu { get; set; }
        public byte ViewAccess { get; set; }
        public byte ManageAccess { get; set; }
        public string Reason { get; set; }
        public DateTime? ExpiresOnUtc { get; set; }
    }

    public record SwitchSiteModel
    {
        public string SiteId { get; set; }
    }
}
