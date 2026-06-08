using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.EmailNotifications
{
    public interface ISitesEmailNotificationsServices
    {
        #region SiteEmailNotificationExists
        Task<bool> SiteEmailNotificationExists(string siteId);
        #endregion

        #region GetSiteEmailNotificationBySiteId
        //Task<SitesEmailNotifications> GetSiteEmailNotificationBySiteId(string siteId);
        List<SitesEmailNotifications> GetSiteEmailNotificationBySiteId(string siteId);
        #endregion

        #region GetSiteEmailNotificationByMessageTemplateId
        Task<SitesEmailNotifications> GetSiteEmailNotificationByMessageTemplateId(string messageTemplateId);
        #endregion

        #region InsertSitesEmailNotifications
        void InsertSitesEmailNotifications(SitesEmailNotifications entity);
        #endregion
    }
}
