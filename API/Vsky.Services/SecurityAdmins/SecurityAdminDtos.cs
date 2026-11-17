using System;
using System.Collections.Generic;

namespace Vsky.Services.SecurityAdmins
{
    public enum SecurityAdminStatus
    {
        NotFound,
        BadRequest,
        Forbidden
    }

    /// <summary>Why a security change was rejected. The service decides it; the controller only turns it into a response.</summary>
    public class SecurityAdminResult
    {
        public SecurityAdminStatus Status { get; init; }
        public string Message { get; init; }

        public static SecurityAdminResult NotFound() => new() { Status = SecurityAdminStatus.NotFound };
        public static SecurityAdminResult BadRequest(string message) => new() { Status = SecurityAdminStatus.BadRequest, Message = message };
        public static SecurityAdminResult Forbidden(string message) => new() { Status = SecurityAdminStatus.Forbidden, Message = message };
    }

    public class SecurityOverviewDto
    {
        public string Mode { get; set; }
        public string SiteGateMode { get; set; }
        public string RecordScopeMode { get; set; }
        public bool EnforceAllLinks { get; set; }
        public bool LegacyShowOnlyAccess { get; set; }
        public int UserCount { get; set; }
        public int BlockedUserCount { get; set; }
        public int RoleCount { get; set; }
        public int GroupCount { get; set; }
        public int DirectGrantCount { get; set; }
        public int NotConfiguredRowCount { get; set; }
    }

    public class SecurityUserListDto
    {
        public int Total { get; set; }
        public List<SecurityUserDto> Items { get; set; }
    }

    public class SecurityUserDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool IsSharedUser { get; set; }
        public bool LastUsed { get; set; }
        public List<SecurityUserRoleDto> Roles { get; set; }
        public int GroupCount { get; set; }
        public int DirectGrantCount { get; set; }
        public bool CanAccessSite { get; set; }
        public string AccessIssue { get; set; }
    }

    public class SecurityUserRoleDto
    {
        public string SiteRoleId { get; set; }
        public string Name { get; set; }
    }

    public class SecurityUserSiteDto
    {
        public string SiteId { get; set; }
        public string Name { get; set; }
        public bool IsSharedUser { get; set; }
        public bool LastUsed { get; set; }
        public bool CanAccess { get; set; }
    }

    public class SecurityRoleDto
    {
        public string SiteRoleId { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public int UserCount { get; set; }
        public int MenuCount { get; set; }
        public int NotConfiguredCount { get; set; }
    }

    public class SecurityGroupDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public int MemberCount { get; set; }
        public int PermissionCount { get; set; }
    }

    public class SecurityGroupMemberDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public bool CanAccessSite { get; set; }
    }

    public class SecurityMenuPermissionDto
    {
        public string SiteModuleMenuId { get; set; }
        public string Module { get; set; }
        public string Menu { get; set; }
        public string Link { get; set; }
        public bool ShowInMenu { get; set; }
        public byte ViewAccess { get; set; }
        public byte ManageAccess { get; set; }
        public string Reason { get; set; }
        public DateTime? ExpiresOnUtc { get; set; }
    }

    public class EffectivePermissionsDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string SiteId { get; set; }
        public bool IsSiteMember { get; set; }
        public string SiteIssue { get; set; }
        public string Mode { get; set; }
        public bool ViewerIsSystemAdmin { get; set; }
        public List<EffectiveMenuDto> Menus { get; set; }
    }

    public class EffectiveMenuDto
    {
        public string SiteModuleMenuId { get; set; }
        public string Module { get; set; }
        public string Menu { get; set; }
        public string Link { get; set; }
        public bool Show { get; set; }
        public bool View { get; set; }
        public bool Manage { get; set; }
        public string ViewReason { get; set; }
        public string ManageReason { get; set; }
        public List<EffectiveSourceDto> Sources { get; set; }
    }

    public class EffectiveSourceDto
    {
        public string Kind { get; set; }
        public string Name { get; set; }
        public bool Show { get; set; }
        public string View { get; set; }
        public string Manage { get; set; }
    }

    public class SecurityAuditListDto
    {
        public int Total { get; set; }
        public List<SecurityAuditLogDto> Items { get; set; }
        public bool CanSeeAllSites { get; set; }
    }

    public class SecurityAuditLogDto
    {
        public string Id { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string PageUrl { get; set; }
    }
}
