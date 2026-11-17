using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.SecurityAdmins
{
    public interface ISecurityAdminService
    {
        #region GetOverview
        Task<SecurityOverviewDto> GetOverview(string SiteId);
        #endregion

        #region Users and site access
        Task<SecurityUserListDto> GetUsers(string SiteId, string search, bool blockedOnly, int page, int pageSize);

        // These return null when the user isn't in the site.
        Task<List<SecurityUserSiteDto>> GetUserSites(string SiteId, string LoggedUserId, string userId);
        Task<List<SecurityUserRoleDto>> GetUserRoles(string SiteId, string userId);
        #endregion

        #region SetUserRoles
        Task<SecurityAdminResult> ValidateUserRoles(string LoggedUserId, string SiteId, string userId, List<string> siteRoleIds);
        Task<List<ApplicationUserRole>> GetUserRolesBySiteRoleIds(string SiteId, string userId, List<string> siteRoleIds);
        void UpdateUserRoles(List<ApplicationUserRole> entities, string LoggedUserId);
        #endregion

        #region GetRoles
        Task<List<SecurityRoleDto>> GetRoles(string SiteId);
        #endregion

        #region Groups
        Task<List<SecurityGroupDto>> GetGroups(string SiteId);

        // Returns null when the group isn't in the site.
        Task<SecurityGroup> GetGroupById(string SiteId, string groupId);

        Task<SecurityAdminResult> ValidateGroup(SecurityGroup entity);
        void InsertGroup(SecurityGroup entity);
        void UpdateGroup(SecurityGroup entity);
        void DeleteGroup(SecurityGroup entity);
        #endregion

        #region Group members
        // Returns null when the group isn't in the site.
        Task<List<SecurityGroupMemberDto>> GetGroupMembers(string SiteId, string groupId);
        Task<SecurityGroupMember> GetGroupMember(string groupId, string userId);

        Task<SecurityAdminResult> ValidateGroupMembers(List<SecurityGroupMember> entities);
        void InsertGroupMembers(List<SecurityGroupMember> entities);
        void DeleteGroupMember(SecurityGroupMember entity);
        #endregion

        #region Permission matrix
        // These return null when the group or user isn't in the site.
        Task<List<SecurityMenuPermissionDto>> GetGroupPermissions(string SiteId, string groupId);
        Task<List<SecurityMenuPermissionDto>> GetUserPermissions(string SiteId, string userId);

        Task<SecurityAdminResult> ValidateGroupPermission(SecurityGroupMenuPermission entity);
        Task SaveGroupPermission(SecurityGroupMenuPermission entity);

        Task<SecurityAdminResult> ValidateUserPermission(UserMenuPermission entity);
        Task SaveUserPermission(UserMenuPermission entity);
        #endregion

        #region Access rules
        // These return the reason the change is rejected, or null when it's allowed.
        Task<SecurityAdminResult> CheckCanChangeUser(string LoggedUserId, string userId, string SiteId);
        Task<SecurityAdminResult> CheckCanChangeGroup(string LoggedUserId, string SiteId, string groupId);
        #endregion

        #region GetEffectivePermissions
        // Returns null when the user isn't in the site.
        Task<EffectivePermissionsDto> GetEffectivePermissions(string SiteId, string LoggedUserId, string userId);
        #endregion

        #region GetAuditLog
        Task<SecurityAuditListDto> GetAuditLog(
            string SiteId,
            string LoggedUserId,
            string category,
            string search,
            string userId,
            DateTime? fromUtc,
            DateTime? toUtc,
            bool allSites,
            int page,
            int pageSize
        );
        #endregion
    }
}
