using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using MailKit.Search;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.Notifications
{
    public class NotificationPermissionsService : INotificationPermissionsService
    {
        #region Define Services
        private readonly IRepository<NotificationPermissions> _notificationPermissionsRepository;
        private readonly ISiteService _siteService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        #endregion

        #region Services Initializations
        public NotificationPermissionsService(
            IRepository<NotificationPermissions> notificationPermissionsRepository,
            ISiteService siteService, UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _notificationPermissionsRepository = notificationPermissionsRepository;
            _siteService = siteService;
            _userManager = userManager;
            _db = db;
        }
        #endregion

        #region Private Methods
        // Title: GetOrderBy
        // Description: This method returns the input string as it is, which can be used as the `ORDER BY` clause in a SQL query.
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllNotificationPermissions
        // Title: GetAllNotifications
        // Description: This method retrieves a paginated list of NotificationPermissions based on various search criteria such as LoggedUserId, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<NotificationPermissions> GetAllNotificationPermissions(string SiteId, string LoggedUserId, string SearchText, string title, string type, string message, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _notificationPermissionsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.AspNetUserId == LoggedUserId);
            if (!string.IsNullOrWhiteSpace(title))
            {
                title = title.Trim().ToLower();
                query = query.Where(x => x.Notification.Title.ToLower().Contains(title));
            }
            if (!string.IsNullOrWhiteSpace(type))
            {
                type = type.Trim().ToLower();
                query = query.Where(x => x.Notification.Type.ToLower().Contains(type));
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                message = message.Trim().ToLower();
                query = query.Where(x => x.Notification.Message.ToLower().Contains(message));
            }
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            // static search
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                  m.Notification.Type.ToLower().Contains(SearchText.ToLower()) ||
                  m.Notification.Title.ToLower().Contains(SearchText.ToLower()) ||
                  m.Notification.Message.ToLower().Contains(SearchText.ToLower())
                );
            }

            query = query.Select(x => new NotificationPermissions
            {
                Id = x.Id,
                Active = x.Active,
                Notification = new MasterNotification
                {
                    Id = x.Notification.Id,
                    Title = x.Notification.Title,
                    Message = x.Notification.Message,
                    Type = x.Notification.Type
                }
            });

            var list = new PagedList<NotificationPermissions>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetNotificationPermissionsCountByUserId
        public async Task<int> GetNotificationPermissionsCountByUserId(string SiteId, string userId)
        {
            return await _notificationPermissionsRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId && m.AspNetUserId == userId).CountAsync();
        }
        #endregion

        #region GetPermissionsByUserId
        // Title: GetPermissionsByUserId
        public List<NotificationPermissions> GetPermissionsByUserId(string SiteId, string userId)
        {

            var query = _notificationPermissionsRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId && m.AspNetUserId == userId);
            query = query.Select(x => new NotificationPermissions
            {
                Id = x.Id
            });
            return query.ToList();
        }
        #endregion

        #region GetNotificationPermissionsById
        // Title: GetNotificationPermissionsById
        // Description: This method retrieves a NotificationPermissions from the database by its unique identifier (`id`). 
        public async Task<NotificationPermissions> GetNotificationPermissionsById(string notificationPermissionId)
        {
            var query = _notificationPermissionsRepository.TableNoTracking.Where(x => x.Id == notificationPermissionId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetPermissionByNotificationId
        // Title: GetPermissionByNotificationId
        public async Task<NotificationPermissions> GetPermissionByNotificationId(string SiteId, string notificationId, string userId)
        {
            var query = _notificationPermissionsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.NotificationId == notificationId && x.AspNetUserId == userId);

            //if (!string.IsNullOrEmpty(id))
            //    query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertNotificationPermissionsList
        public void InsertNotificationPermissionsList(IList<NotificationPermissions> entities)
        {
            _notificationPermissionsRepository.Insert(entities);
        }
        #endregion

        #region UpdateNotificationPermission
        // Title: UpdateNotificationPermission
        // Description: This method updates the specified NotificationPermission entity in the repository. It takes a NotificationPermission object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateNotificationPermission(NotificationPermissions entity)
        {
            _notificationPermissionsRepository.Update(entity);
        }
        #endregion
    }
}
