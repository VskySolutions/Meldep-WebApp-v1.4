using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Notifications
{
    public interface IMasterNotificationService
    {
        #region GetAllMasterNotifications
        IPagedList<MasterNotification> GetAllMasterNotifications(string SiteId, DateTime? startDate, DateTime? endDate, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion
        Task<List<MasterNotification>> GetNotificationsForGeneratePermissionsData(string SiteId);
        Task<string> GetTypeNumber(string SiteId, string type);

        #region GetById
        Task<MasterNotification> GetById(string notificationId);
        Task<MasterNotification> GetMasterNotificationByNumber(string siteId, string number, string userId);
        Task<MasterNotification> GetMasterNotificationBySiteIdAndNumber(string siteId, string number, string id = null);
        #endregion

        #region AddMasterNotification
        Task AddMasterNotification(
           string siteId,
           string number,
           string title,
           string message,
           string type,
           string createdById,
           DateTime GetDateTime);
        #endregion

        //void InsertMasterNotification(MasterNotification entity);
    }
}
