using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.IssueActivitys;
using Vsky.Services.Issues;
using Vsky.Services.Notifications;
using Vsky.Services.ProjectTasks;
using Vsky.Services.Sites;
using Vsky.Services.TestPlans;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Vsky.Api.Models.EmployeeModel;

namespace Vsky.Api.Controllers
{
    [Route("notification")]
    public class NotificationController : BaseController
    {
        #region Define Services  
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ICommonService _commonService;
        private readonly INotificationPermissionsService _notificationPermissionsService;
        private readonly ISiteService _siteService;
        private readonly IMasterNotificationService _masterNotificationService;
        #endregion

        #region Services Initializations      
        public NotificationController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            INotificationService notificationService, 
            ICommonService commonService, 
            INotificationPermissionsService notificationPermissionsService, 
            ISiteService siteService, 
            IMasterNotificationService masterNotificationService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _notificationService = notificationService;
            _commonService = commonService;
            _notificationPermissionsService = notificationPermissionsService;
            _siteService = siteService;
            _masterNotificationService = masterNotificationService;
        }
        #endregion

        #region GetAllNotifications
        // Title: Get All Notifications
        // Description: This endpoint fetches a list of Notifications based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllNotifications(NotificationsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of Notifications on search criteria (name, sorting, pagination)
                var list = _notificationService.GetAllNotifications(SiteId, LoggedUserId, searchModel.SearchText, searchModel.StartDate, searchModel.EndDate, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                var notificationModelList = new List<NotificationsModel>();
                // Iterate through each employee in the list
                foreach (var notification in list)
                {
                    var NotificationsModel = new NotificationsModel
                    {
                        Id = notification.Id,
                        Title = notification.Title,
                        Message = notification.Message,
                        RedirectURL = notification.RedirectURL,
                        CreatedOnUtc = notification.CreatedOnUtc,
                    };

                    notificationModelList.Add(NotificationsModel);
                }
                notificationModelList.OrderByDescending(x => x.CreatedOnUtc);
                // Map the fetched list to a model suitable for the response
                var model = new NotificationsListModel
                {
                    Data = _mapper.Map<IList<NotificationsModel>>(notificationModelList),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllNotificationPermissions
        // Title: Get All Notifications
        // Description: This endpoint fetches a list of NotificationPermissions based on the provided search criteria such as date, sorting, and pagination. 
        [HttpPost("notificationPermissionList")]
        public async Task<IActionResult> GetAllNotificationPermissions(NotificationPermissionsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var UserId = !string.IsNullOrEmpty(searchModel.UserId)
                        ? searchModel.UserId
                        : LoggedUserId;

                var notificationCount = await _notificationPermissionsService.GetNotificationPermissionsCountByUserId(SiteId, UserId);
                if (notificationCount == 0)
                {
                    //await GenerateNotificationsPermissionsData(SiteId, UserId, GetDateTime);
                }

                await GenerateNotificationsPermissionsData(SiteId, UserId, GetDateTime);

                // Fetch a list of NotificationPermissions on search criteria (date, sorting, pagination)
                var list = _notificationPermissionsService.GetAllNotificationPermissions(SiteId, UserId, searchModel.SearchText, searchModel.Title, searchModel.Type, searchModel.Message, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                // Map the fetched list to a model suitable for the response
                var model = new NotificationPermissionsListModel
                {
                    Data = _mapper.Map<IList<NotificationPermissionsModel>>(list),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GenerateNotificationsPermissionsData
        // Title: Generate Notifications Permissions Data
        private async Task GenerateNotificationsPermissionsData(string SiteId, string LoggedUserId, DateTime GetDateTime)
        {
            var notifications = await _masterNotificationService.GetNotificationsForGeneratePermissionsData(SiteId);
            var permissionList = new List<NotificationPermissions>();

            foreach (var notification in notifications)
            {
                //Check if the notification already exists
                var exists = await _notificationPermissionsService.GetPermissionByNotificationId(SiteId, notification.Id, LoggedUserId);
                if (exists != null)
                    continue;

                var permission = new NotificationPermissions
                {
                    SiteId = SiteId,
                    NotificationId = notification.Id,
                    AspNetUserId = LoggedUserId,
                    Active = true,
                    CreatedById = notification.CreatedById,
                    CreatedOnUtc = GetDateTime,
                    UpdatedById = notification.CreatedById,
                    UpdatedOnUtc = GetDateTime
                };
                permissionList.Add(permission);
            }
            if (permissionList.Count > 0)
                _notificationPermissionsService.InsertNotificationPermissionsList(permissionList);
        }
        #endregion

        #region NotificationCount
        [HttpGet("notificationcount")]
        public IActionResult NotificationCount()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var notificationCount = _notificationService.NotificationCount(SiteId, LoggedUserId);
                return Ok(notificationCount);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateNotificationPermission
        [HttpPut("on-off-status/{id}/{active}/{userId}")]
        public async Task<IActionResult> UpdateNotificationPermission(string id, bool active, string userId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _notificationPermissionsService.GetNotificationPermissionsById(id);
                    if (entity == null)
                        BadRequest(new BadRequestError("No notification found with the specified id."));

                    entity.Active = active;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _notificationPermissionsService.UpdateNotificationPermission(entity);

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateAllPermissions
        [HttpPut("all-permissions/{active}/{userId}")]
        public async Task<IActionResult> UpdateAllPermissions(bool active, string userId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var UserId = string.IsNullOrWhiteSpace(userId) || userId == "null" ? User.GetLoggedInUserId<string>() : userId;

                    var userPermissionList = _notificationPermissionsService.GetPermissionsByUserId(SiteId, UserId);
                    if (userPermissionList.Count == 0)
                        BadRequest(new BadRequestError("No notifications were found for the specified user."));

                    foreach (var permissions in userPermissionList)
                    {
                        var entity = await _notificationPermissionsService.GetNotificationPermissionsById(permissions.Id);
                        if (entity != null)
                        {
                            entity.Active = active;
                            entity.UpdatedById = LoggedUserId;
                            entity.UpdatedOnUtc = GetDateTime;
                            _notificationPermissionsService.UpdateNotificationPermission(entity);
                        }
                    }

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region PopupList
        // Title: Get All Notifications
        // Description: This endpoint fetches a list of Notifications based on the LoggedUserId.
        [HttpPost]
        public async Task<IActionResult> PopupList(string Id = null, string Flag = null)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var items = await _notificationService.GetNotificationDetails(SiteId, LoggedUserId, Flag);
            var NotificationDetailIds = new List<string>();
            var Myslist = new List<NotificationsModel>();
            if (Flag != "clearAll")
            {
                foreach (var item in items)
                {
                    var obj = new NotificationsModel();
                    obj.Id = item.Id;
                    obj.Title = item.Title;
                    obj.Message = item.Message;
                    obj.RedirectURL = item.RedirectURL;
                    obj.UserName = item.User.Person.FirstName + ' ' + item.User.Person.LastName;
                    obj.CreatedOnUtc = item.CreatedOnUtc;

                    NotificationDetailIds.AddRange(item.NotificationDetails.Select(nd => nd.Id.ToString()));
                    Myslist.Add(obj);
                }
            }
            if (Flag == "RN" || Flag == "clearAll")
            {
               
                if (Id != null && Id != "undefined" && Id != "null")
                {
                    var NotificationDetails = await _notificationService.GetNotificationDetailsByNotificationId(Id);
                    NotificationDetails.IsRead = 1;
                    _notificationService.UpdateNotificationDetails(NotificationDetails);
                }
                else
                {

                    foreach (var item in items)
                    {
                        var notificationDetails = await _notificationService.GetNotificationDetailsByNotificationId(item.Id);
                        notificationDetails.IsRead = 1;
                        _notificationService.UpdateNotificationDetails(notificationDetails);
                    }
                }

            }
            return Ok(Myslist);
        }
        #endregion
    }
}
