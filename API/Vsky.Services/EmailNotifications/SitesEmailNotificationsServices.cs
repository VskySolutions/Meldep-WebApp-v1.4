using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.EmailNotifications
{
    public class SitesEmailNotificationsServices : ISitesEmailNotificationsServices
    {
        #region Define Services
        private readonly IRepository<SitesEmailNotifications> _sitesEmailNotificationsRepository;
        #endregion

        #region Services Initializations
        public SitesEmailNotificationsServices(
            IRepository<SitesEmailNotifications> sitesEmailNotificationsRepository
            )
        {
            _sitesEmailNotificationsRepository = sitesEmailNotificationsRepository;
        }
        #endregion

        #region SiteEmailNotificationExists
        public async Task<bool> SiteEmailNotificationExists(string siteId)
        {
            return await _sitesEmailNotificationsRepository.TableNoTracking.AnyAsync(x => x.SiteId == siteId && !x.Deleted);
        }
        #endregion

        #region GetSiteEmailNotificationBySiteId
        //public async Task<SitesEmailNotifications> GetSiteEmailNotificationBySiteId(string siteId)
        //{
        //    var query = _sitesEmailNotificationsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);
        //    var item = await query.FirstOrDefaultAsync();
        //    return item;
        //}
        public List<SitesEmailNotifications> GetSiteEmailNotificationBySiteId(string siteId)
        {
            var query = _sitesEmailNotificationsRepository.TableNoTracking.Where(m => !m.Deleted && m.Active && m.SiteId == siteId);
            query = query.Select(x => new SitesEmailNotifications
            {
                Id = x.Id,
                Active = x.Active
            });
            return query.ToList();
        }

        #endregion

        #region GetSiteEmailNotificationByMessageTemplateId
        public async Task<SitesEmailNotifications> GetSiteEmailNotificationByMessageTemplateId(string messageTemplateId)
        {
            var query = _sitesEmailNotificationsRepository.TableNoTracking.Where(m => !m.Deleted && m.Active && m.MessageTemplateId == messageTemplateId);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertSitesEmailNotifications
        public void InsertSitesEmailNotifications(SitesEmailNotifications entity)
        {
            _sitesEmailNotificationsRepository.Insert(entity);
        }
        #endregion

    }
}
