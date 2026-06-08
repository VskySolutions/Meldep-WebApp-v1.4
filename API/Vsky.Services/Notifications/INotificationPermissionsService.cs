using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Notifications
{
    public interface INotificationPermissionsService
    {
        #region GetAllNotifications
        IPagedList<NotificationPermissions> GetAllNotificationPermissions(string SiteId, string LoggedUserId, string SearchText, string title, string type, string message, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion
        Task<int> GetNotificationPermissionsCountByUserId(string SiteId, string userId);

        #region GetPermissionsByUserId
        // Title: GetPermissionsByUserId
        List<NotificationPermissions> GetPermissionsByUserId(string SiteId, string userId);
        #endregion

        #region GetNotificationPermissionsById
        Task<NotificationPermissions> GetNotificationPermissionsById(string notificationPermissionId);
        #endregion

        #region GetPermissionByNotificationId
        Task<NotificationPermissions> GetPermissionByNotificationId(string SiteId, string notificationId, string userId);
        #endregion

        #region InsertNotificationPermissionsList
        void InsertNotificationPermissionsList(IList<NotificationPermissions> entities);
        #endregion

        #region UpdateNotificationPermission
        void UpdateNotificationPermission(NotificationPermissions entity);
        #endregion
    }
}
