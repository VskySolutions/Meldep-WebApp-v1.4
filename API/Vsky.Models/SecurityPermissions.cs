using System;
using Vsky.Core;

namespace Vsky.Models
{
    /// <summary>A named set of users in one site. It can't span sites, so a shared user's groups stay per site.</summary>
    public class SecurityGroup : BaseEntity
    {
        public string SiteId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }

    public class SecurityGroupMember : BaseEntity
    {
        public string SecurityGroupId { get; set; }
        public string UserId { get; set; }
        public string SiteId { get; set; }
        public bool Deleted { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }

    /// <summary>ViewAccess and ManageAccess hold 0 = Inherit, 1 = Allow, 2 = Deny.</summary>
    public class SecurityGroupMenuPermission : BaseEntity
    {
        public string SecurityGroupId { get; set; }
        public string SiteId { get; set; }
        public string SiteModuleMenuId { get; set; }
        public bool ShowInMenu { get; set; }
        public byte ViewAccess { get; set; }
        public byte ManageAccess { get; set; }
        public bool Deleted { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }

    /// <summary>A direct grant or deny for one user, on one menu, in one site. Can end by itself.</summary>
    public class UserMenuPermission : BaseEntity
    {
        public string UserId { get; set; }
        public string SiteId { get; set; }
        public string SiteModuleMenuId { get; set; }
        public bool ShowInMenu { get; set; }
        public byte ViewAccess { get; set; }
        public byte ManageAccess { get; set; }
        public string Reason { get; set; }
        public DateTime? ExpiresOnUtc { get; set; }
        public bool Deleted { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }
}
