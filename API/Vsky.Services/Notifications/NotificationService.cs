using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Sites;

namespace Vsky.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        #region Define Services
        private readonly IRepository<Notification> _notificationsRepository;
        private readonly IRepository<NotificationDetails> _notificationDetailsRepository;
        private readonly IRepository<NotificationPermissions> _notificationPermissionsRepository;
        private readonly ISiteService _siteService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations
        public NotificationService(
            IRepository<Notification> notificationsRepository, 
            IRepository<NotificationDetails> notificationDetailsRepository,
            IRepository<NotificationPermissions> notificationPermissionsRepository,
            ISiteService siteService, UserManager<ApplicationUser> userManager, 
            ApplicationDbContext db,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _notificationsRepository = notificationsRepository;
            _notificationDetailsRepository = notificationDetailsRepository;
            _notificationPermissionsRepository = notificationPermissionsRepository;
            _siteService = siteService;
            _userManager = userManager;
            _db = db;
            _applicationUserRoleService = applicationUserRoleService;
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

        #region GetAllNotifications
        // Title: GetAllNotifications
        // Description: This method retrieves a paginated list of Notifications based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<Notification> GetAllNotifications(string SiteId, string LoggedUserId, string searchText, DateTime? startDate, DateTime? endDate, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _notificationsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
            //Get user by userid
            var userdata = _userManager.FindByIdAsync(LoggedUserId).GetAwaiter().GetResult();
            var user = _userManager.FindByNameAsync(userdata.UserName).GetAwaiter().GetResult();
            if (user != null && !user.Deleted && user.Active)
            {
                //Get user roles
                var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
                // Fetch the NormalizedName of each role
                //var normalizedRoles = _db.Roles.Where(r => roles.Contains(r.Name)).Select(r => r.NormalizedName).ToArray();
                var normalizedRoles = _applicationUserRoleService
                    .GetNormalizedRoleNamesByUserAndSite(user.Id, SiteId)
                    .GetAwaiter()
                    .GetResult()
                    .ToArray();

                if (!normalizedRoles.Contains("admin"))
                {
                    if (LoggedUserId != null)
                        query = query.Where(x => x.NotificationDetails.Any(x => x.ToUserId == LoggedUserId));
                }
                else
                {
                    if (LoggedUserId != null)
                        query = query.OrderByDescending(x => x.NotificationDetails.Any(x => x.ToUserId == LoggedUserId)).ThenByDescending(x => x.CreatedOnUtc);
                }
            }
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
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                DateTime.TryParse(searchText, out var parsedDate);
                searchText = searchText.ToLower();

                query = query.Where(m =>
                    m.Type.ToLower().Contains(searchText) ||
                    m.Title.ToLower().Contains(searchText) ||
                    m.Message.ToLower().Contains(searchText) ||
                    m.CreatedOnUtc.Date == parsedDate.Date);
            }

            query = query.Select(x => new Notification
            {
                Id = x.Id,
                //SiteId = x.SiteId,
                Title = x.Title,
                Message = x.Message,
                Type = x.Type,
                FromUserId = x.FromUserId,
                RecordId = x.RecordId,
                RedirectURL = x.RedirectURL,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                User = new ApplicationUser
                {
                    Id = x.User.Id,
                    Person = new Person
                    {
                        Id = x.User.Person.Id,
                        FullName = x.User.Person.LastName + " " + x.User.Person.LastName
                    }
                },
                NotificationDetails = x.NotificationDetails.Select(p => new NotificationDetails
                {
                    Id = p.Id
                }).ToList(),
            });

            var list = new PagedList<Notification>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetNotificationsForGeneratePermissionsData
        // Title: GetNotificationsForGeneratePermissionsData
        // Description: This method retrieves a list of Notifications for Generate Notification Permissions Data,
        public async Task<List<Notification>> GetNotificationsForGeneratePermissionsData(string SiteId, string LoggedUserId)
        {
            var query = _notificationsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if(!string.IsNullOrWhiteSpace(LoggedUserId))
                query = query.Where(x => x.NotificationDetails.Any(x => x.ToUserId == LoggedUserId));

            query = query.Select(x => new Notification
            {
                Id = x.Id,
                CreatedById = x.CreatedById,
                NotificationDetails = x.NotificationDetails.Select(p => new NotificationDetails
                {
                    Id = p.Id,
                    ToUserId = p.ToUserId
                }).ToList(),
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region Get Notification Count
        public int NotificationCount(string SiteId, string LoggedUserId)
        {
            int notificationCount = 0;
            notificationCount = _notificationsRepository.Table
                .Where(x => !x.Deleted && x.SiteId == SiteId && x.NotificationDetails.Any(x => x.ToUserId == LoggedUserId && x.IsRead == 0))
                .OrderByDescending(m => m.CreatedOnUtc).Count();           
            // Return leave credits as a string
            return notificationCount;
        }
        #endregion

        #region GetNotificationDetailsByNotificationId
        // Title: GetNotificationDetailsByNotificationId
        // Description: This method retrieves a NotificationDetails from the database by its unique identifier (`id`). 
        public async Task<NotificationDetails> GetNotificationDetailsByNotificationId(string notificationId)
        {
            var query = _notificationDetailsRepository.TableNoTracking.Where(x => x.NotificationId == notificationId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion
       

        #region Add Notification
        public int AddNotification(string SiteId, string Title = null, string Message = null, string Type = null, string FromUserId = null, string RecordId = null, string RedirectURL = null, string ToUserId = null, string CreatedById = null, DateTime? GetDateTime = null)
        {
            try
            {
                var Notification = new Notification();
                Notification.SiteId = SiteId;
                Notification.Title = Title;
                Notification.Message = Message;
                Notification.Type = Type;
                Notification.FromUserId = FromUserId;
                Notification.RecordId = RecordId;
                Notification.RedirectURL = RedirectURL;
                Notification.CreatedById = CreatedById;
                Notification.CreatedOnUtc = (DateTime)GetDateTime;
                _notificationsRepository.Insert(Notification);

                var NotificationDetails = new NotificationDetails();
                NotificationDetails.ToUserId = ToUserId;
                NotificationDetails.NotificationId = Notification.Id;
                NotificationDetails.IsRead = 0;
                _notificationDetailsRepository.Insert(NotificationDetails);
            }
            catch (Exception ex)
            {
            }
            return 0;
        }
        #endregion

        //bool HasNotificationPermission(string siteId, string Type, string Title)
        //{
        //    var norification = _notificationPermissionsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.Notification.Type.ToLower() == Type.ToLower() && x.Notification.Title.ToLower() == Title.ToLower()).FirstOrDefault();
        //    return norification?.Active?? false;
        //}
        #region GetNotificationDetails
        // Title: GetNotificationDetails
        // Description: The method selects relevant fields from the Notifications entity.
        public async Task<List<Notification>> GetNotificationDetails(string SiteId, string LoggedUserId, string flag)
        {
            var query = _notificationsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.NotificationDetails.Any(x => x.ToUserId == LoggedUserId && x.IsRead == 0));

            if (flag == "clearAll")
            {
                return await query
                    .Select(x => new Notification
                    {
                        Id = x.Id
                    }).ToListAsync();
            }
            query = query.Select(x => new Notification
            {
                Id = x.Id,
                //SiteId = x.SiteId,
                Title = x.Title,
                Message = x.Message,
                Type = x.Type,
                FromUserId = x.FromUserId,
                RecordId = x.RecordId,
                RedirectURL = x.RedirectURL,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                User = new ApplicationUser
                {
                    Id = x.User.Id,
                    Person = new Person
                    {
                        Id = x.User.Person.Id,
                        FullName = x.User.Person.LastName + " " + x.User.Person.LastName
                    }
                },
                NotificationDetails = x.NotificationDetails.Select(p => new NotificationDetails
                {
                    Id = p.Id                                                           
                }).ToList(),
            }).Take(5);
            query = query.OrderByDescending(m => m.CreatedOnUtc);

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region UpdateNotificationDetails
        // Title: UpdateNotificationDetails
        // Description: This method updates the specified NotificationDetails entity in the repository. It takes a NotificationDetails object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateNotificationDetails(NotificationDetails entity)
        {
            _notificationDetailsRepository.Update(entity);
        }
        #endregion
    }
}
