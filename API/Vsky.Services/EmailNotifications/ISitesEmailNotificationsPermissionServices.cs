using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.EmailNotifications
{
    public interface ISitesEmailNotificationsPermissionServices
    {
        #region GetAllSitesEmailNotificationsPermissions
        IPagedList<SitesEmailNotificationsPermission> GetAllSitesEmailNotificationsPermissions(
            string siteId,
            string currentUserId,
            string name,
            string subject,
            string SearchText,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetEmailNotificationPermissionsById
        Task<SitesEmailNotificationsPermission> GetEmailNotificationPermissionsById(string notificationPermissionId);
        #endregion

        #region GetPermissionsByUserId
        List<SitesEmailNotificationsPermission> GetPermissionsByUserId(string SiteId, string userId);
        #endregion

        #region GetEmailNotificationPermissionDetailsById
        Task<SitesEmailNotificationsPermission> GetEmailNotificationPermissionDetailsById(string id);
        #endregion

        #region Should Send Notification
        Task<bool> ShouldSendNotification(
            string siteId,
            string userId,
            string templateName
        );
        #endregion

        #region InsertSitesEmailNotificationsPermission
        void InsertSitesEmailNotificationsPermission(SitesEmailNotificationsPermission entity);
        #endregion

        #region UpdateSitesEmailNotificationsPermission
        void UpdateSitesEmailNotificationsPermission(SitesEmailNotificationsPermission entity);
        #endregion
    }
}
