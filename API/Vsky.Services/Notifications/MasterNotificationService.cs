using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;
using System.Linq.Dynamic.Core;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Vsky.Services.Notifications
{
    public class MasterNotificationService : IMasterNotificationService
    {
        #region Define Services
        private readonly IRepository<MasterNotification> _masterNotificationRepository;
        private readonly ISiteService _siteService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        #endregion

        #region Services Initializations
        public MasterNotificationService(
            IRepository<MasterNotification> masterNotificationRepository,
            ISiteService siteService, UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _masterNotificationRepository = masterNotificationRepository;
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

        #region GetAllMasterNotifications
        // Title: GetAllMasterNotifications
        // Description: This method retrieves a paginated list of Notifications based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<MasterNotification> GetAllMasterNotifications(string SiteId, DateTime? startDate, DateTime? endDate, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _masterNotificationRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            //Search by FromDate and Todate
            if (startDate != null)
                query = query.Where(x => x.CreatedOnUtc >= startDate);
            if (endDate != null)
                query = query.Where(a => a.CreatedOnUtc.Date <= endDate);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            query = query.Select(x => new MasterNotification
            {
                Id = x.Id,
                //SiteId = x.SiteId,
                Title = x.Title,
                Message = x.Message,
                Type = x.Type,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc
            });

            var list = new PagedList<MasterNotification>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetNotificationsForGeneratePermissionsData
        // Title: GetNotificationsForGeneratePermissionsData
        // Description: This method retrieves a list of Notifications for Generate Notification Permissions Data,
        public async Task<List<MasterNotification>> GetNotificationsForGeneratePermissionsData(string SiteId)
        {
            var query = _masterNotificationRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            query = query.Select(x => new MasterNotification
            {
                Id = x.Id,
                Number = x.Number,
                Title = x.Title,
                Message = x.Message,
                Type = x.Type,
                CreatedById = x.CreatedById
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetById
        // Title: GetById
        // Description: This method retrieves a NotificationDetails from the database by its unique identifier (`id`). 
        public async Task<MasterNotification> GetById(string notificationId)
        {
            var query = _masterNotificationRepository.TableNoTracking.Where(x => x.Id == notificationId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetMasterNotificationByNumber
        // Title: GetMasterNotificationByNumber
        // Description: This method retrieves a MasterNotification data from the database by siteid and number. 
        public async Task<MasterNotification> GetMasterNotificationByNumber(string siteId, string number, string userId)
        {
            var notification = await _masterNotificationRepository.TableNoTracking
                .Include(m => m.NotificationPermissions.Where(np => !np.Deleted))
                .Where(m => !m.Deleted && m.SiteId == siteId && m.Number == number)
                .FirstOrDefaultAsync();

            if (notification == null)
                return null;

            var hasPermission = notification.NotificationPermissions
                .Any(np => np.AspNetUserId == userId);

            if (!hasPermission)
            {
                return new MasterNotification
                {
                    Title = notification.Title ?? "",
                    Message = notification.Message ?? "",
                    Type = notification.Type ?? "",
                    IsSend = true
                };
            }

            var activePermission = notification.NotificationPermissions
                .Any(np => np.AspNetUserId == userId && np.Active);

            if (activePermission)
            {
                return new MasterNotification
                {
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type
                };
            }

            return null;
        }
        #endregion

        #region GetMasterNotificationBySiteIdAndNumber
        // Title: GetMasterNotificationBySiteIdAndNumber
        // Description: This method retrieves a Master Notification based on its number and site ID. It allows an optional exclusion of a Master Notification by its ID.
        public async Task<MasterNotification> GetMasterNotificationBySiteIdAndNumber(string siteId, string number, string id = null)
        {
            var query = _masterNotificationRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.Number.ToLower() == number.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetTypeNumber
        // Title: GetTypeNumber
        // Description: This method retrieves a MasterNotification from the database. 
        public async Task<string> GetTypeNumber(string SiteId, string type)
        {
            var query = _masterNotificationRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Type == type).OrderByDescending(x => x.Number);
            var item = await query.FirstOrDefaultAsync();

            int nextNum = 1;
            if (item != null && !string.IsNullOrEmpty(item.Number))
            {
                var currentNumber = item.Number;
                var numPart = currentNumber.Substring(type.Length);
                if (int.TryParse(numPart, out int parsedNum))
                {
                    nextNum = parsedNum + 1;
                }
            }

            return $"{type}{nextNum}";
        }
        #endregion

        #region Add Master Notification
        public async Task AddMasterNotification(
            string siteId,
            string number,
            string title,
            string message,
            string type,
            string createdById,
            DateTime GetDateTime)
        {
            var masterNotification = new MasterNotification
            {
                SiteId = siteId,
                Number = number,
                Title = title,
                Message = message,
                Type = type,
                CreatedById = createdById,
                CreatedOnUtc = GetDateTime,
                UpdatedById = createdById,
                UpdatedOnUtc = GetDateTime
            };

            _masterNotificationRepository.Insert(masterNotification);
        }

        #endregion
    }
}
