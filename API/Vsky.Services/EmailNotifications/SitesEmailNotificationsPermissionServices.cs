using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using System.Linq.Dynamic.Core;
using Vsky.Data;
using Vsky.Models;
using Microsoft.EntityFrameworkCore;

namespace Vsky.Services.EmailNotifications
{
    public class SitesEmailNotificationsPermissionServices : ISitesEmailNotificationsPermissionServices
    {
        #region Define Services
        private readonly IRepository<SitesEmailNotificationsPermission> _sitesEmailNotificationsPermissionRepository;
        #endregion

        #region Services Initializations
        public SitesEmailNotificationsPermissionServices(
            IRepository<SitesEmailNotificationsPermission> sitesEmailNotificationsPermissionRepository
            )
        {
            _sitesEmailNotificationsPermissionRepository = sitesEmailNotificationsPermissionRepository;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllSitesEmailNotificationsPermissions
        public IPagedList<SitesEmailNotificationsPermission> GetAllSitesEmailNotificationsPermissions(
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
        )
        {
            var query = _sitesEmailNotificationsPermissionRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.UserId == currentUserId);


            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();
                query = query.Where(x => x.SitesEmailNotifications.Name.ToLower().Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(subject))
            {
                subject = subject.Trim().ToLower();
                query = query.Where(x => x.SitesEmailNotifications.Subject.ToLower().Contains(subject));
            }

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.CreatedOnUtc);
            }

            // static search
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                  m.SitesEmailNotifications.Name.ToLower().Contains(SearchText.ToLower()) ||
                  m.SitesEmailNotifications.Subject.ToLower().Contains(SearchText.ToLower()) ||
                  m.SitesEmailNotifications.Body.ToLower().Contains(SearchText.ToLower())
                );
            }

            query = query.Select(x => new SitesEmailNotificationsPermission
            {
                Id = x.Id,
                SiteEmailNotificationId = x.SiteEmailNotificationId,
                UserId = x.UserId,
                Active = x.Active,
                SiteId = x.SiteId,
                SitesEmailNotifications = new SitesEmailNotifications
                {
                    Id = x.SitesEmailNotifications.Id,
                    Name = x.SitesEmailNotifications.Name,
                    Subject = x.SitesEmailNotifications.Subject,
                    SiteId = x.SitesEmailNotifications.SiteId
                }
            });

            var list = new PagedList<SitesEmailNotificationsPermission>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetEmailNotificationPermissionsById
        public async Task<SitesEmailNotificationsPermission> GetEmailNotificationPermissionsById(string notificationPermissionId)
        {
            var query = _sitesEmailNotificationsPermissionRepository.TableNoTracking.Where(x => x.Id == notificationPermissionId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetPermissionsByUserId
        public List<SitesEmailNotificationsPermission> GetPermissionsByUserId(string SiteId, string userId)
        {

            var query = _sitesEmailNotificationsPermissionRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId && m.UserId == userId);
            query = query.Select(x => new SitesEmailNotificationsPermission
            {
                Id = x.Id
            });
            return query.ToList();
        }
        #endregion

        #region GetEmailNotificationPermissionDetailsById
        public async Task<SitesEmailNotificationsPermission> GetEmailNotificationPermissionDetailsById(string id)
        {
            var query = _sitesEmailNotificationsPermissionRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new SitesEmailNotificationsPermission
            {
                Id = x.Id,
                SiteId = x.SiteId,
                SiteEmailNotificationId = x.SiteEmailNotificationId,
                UserId = x.UserId,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                SitesEmailNotifications = new SitesEmailNotifications
                {
                    Id = x.SitesEmailNotifications.Id,
                    Name = x.SitesEmailNotifications.Name,
                    Subject = x.SitesEmailNotifications.Subject,
                    Body = x.SitesEmailNotifications.Body
                },
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region Should Send Notification
        public async Task<bool> ShouldSendNotification(
            string siteId,
            string userId,
            string templateName
        )
        {
            var permission = await _sitesEmailNotificationsPermissionRepository.TableNoTracking
                .Include(x => x.SitesEmailNotifications)
                .Where(x =>
                    x.SiteId == siteId &&
                    x.UserId == userId &&
                    x.SitesEmailNotifications.Name == templateName
                )
                .FirstOrDefaultAsync();

            // No record -> send email (default behaviour)
            if (permission == null)
                return true;

            // Record exists -> send only if active
            return permission.Active;
        }
        #endregion

        #region InsertSitesEmailNotificationsPermission
        public void InsertSitesEmailNotificationsPermission(SitesEmailNotificationsPermission entity)
        {
            _sitesEmailNotificationsPermissionRepository.Insert(entity);
        }
        #endregion

        #region UpdateSitesEmailNotificationsPermission
        public void UpdateSitesEmailNotificationsPermission(SitesEmailNotificationsPermission entity)
        {
            _sitesEmailNotificationsPermissionRepository.Update(entity);
        }
        #endregion
    }
}
