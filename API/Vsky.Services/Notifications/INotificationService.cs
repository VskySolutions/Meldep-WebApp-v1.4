using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Notifications
{
    public interface INotificationService
    {
        #region GetAllNotifications
        IPagedList<Notification> GetAllNotifications(string SiteId, string LoggedUserId, string searchText, DateTime? startDate, DateTime? endDate, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion
        Task<List<Notification>> GetNotificationsForGeneratePermissionsData(string SiteId, string LoggedUserId);

        #region Get Notification Count
        int NotificationCount(string SiteId, string LoggedUserId);
        #endregion

        #region GetNotificationDetailsByNotificationId
        Task<NotificationDetails> GetNotificationDetailsByNotificationId(string notificationId);
        #endregion

        #region AddNotification
        int AddNotification(string SiteId, string Title = null, string Message = null, string Type = null, string FromUserId = null, string RecordId = null, string RedirectURL = null, string ToUserId = null, string CreatedById = null, DateTime? GetDateTime = null);
        #endregion

        #region GetNotificationDetails
        Task<List<Notification>> GetNotificationDetails(string SiteId, string LoggedUserId, string flag);
        #endregion

        #region UpdateNotificationDetails
        void UpdateNotificationDetails(NotificationDetails entity);
        #endregion
    }
}
