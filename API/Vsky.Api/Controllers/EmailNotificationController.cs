using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.EmailNotifications;
using Vsky.Services.Messages;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("email-Notification")]
    public class EmailNotificationController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly ISitesEmailNotificationsServices _sitesEmailNotificationsService;
        private readonly ISitesEmailNotificationsPermissionServices _sitesEmailNotificationsPermissionServices;
        private readonly IWorkflowMessageService _workflowMessageService;
        #endregion

        #region Services Initializations      
        public EmailNotificationController(
            GlobalVariable globalVariable,
            IMessageTemplateService messageTemplateService, 
            ICommonService commonService,
            ISiteService siteService,
            ISitesEmailNotificationsServices sitesEmailNotificationsServices,
            ISitesEmailNotificationsPermissionServices sitesEmailNotificationsPermissionServices,
            IWorkflowMessageService workflowMessageService
        )
        {
            _globalVariable = globalVariable;
            _messageTemplateService = messageTemplateService;
            _commonService = commonService;
            _siteService = siteService;
            _sitesEmailNotificationsService = sitesEmailNotificationsServices;
            _sitesEmailNotificationsPermissionServices = sitesEmailNotificationsPermissionServices;
            _workflowMessageService = workflowMessageService;
        }
        #endregion

        #region Get All Master MessageTemplate List
        [HttpPost("messageTemplateList")]
        public IActionResult GetAllMessageTemplates(MessageTemplateSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var List = _messageTemplateService.GetAllMessageTemplates(
                    SiteId,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var Data = new MessageTemplateList
                {
                    MessageTemplateLists = List,
                    Total = List.TotalCount
                };

                return Ok(Data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region GetAllSitesEmailNotificationsPermissions
        [HttpPost("emailNotificationsPermissionsList")]
        public async Task<IActionResult> GetAllSitesEmailNotificationsPermissions(SiteEmailNotificationsSearchModel searchModel)
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

                // Insert if not exists
                await AddSiteEmailNotificationsAndPermissions(SiteId, UserId, GetDateTime);

                // Get and map permision list
                var list = _sitesEmailNotificationsPermissionServices.GetAllSitesEmailNotificationsPermissions(
                    SiteId,
                    UserId,
                    searchModel.Name,
                    searchModel.Subject,
                    searchModel.SearchText,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                    );

                var model = new SitesEmailNotificationsPermissionList
                {
                    SitesEmailNotificationsPermissionsList = list,
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region UpdateEmailNotificationPermission
        [HttpPut("on-off-status/{id}/{active}/{userId}")]
        public async Task<IActionResult> UpdateEmailNotificationPermission(string id, bool active, string userId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _sitesEmailNotificationsPermissionServices.GetEmailNotificationPermissionsById(id);
                    if (entity == null)
                        BadRequest(new BadRequestError("No email notification found with the specified id."));

                    entity.Active = active;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _sitesEmailNotificationsPermissionServices.UpdateSitesEmailNotificationsPermission(entity);

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

        #region UpdateAllEmailNotificationPermissions
        [HttpPut("all-permissions/{active}/{userId}")]
        public async Task<IActionResult> UpdateAllEmailNotificationPermissions(bool active, string userId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var UserId = (string.IsNullOrWhiteSpace(userId) || userId == "null") ? LoggedUserId : userId;

                    var userPermissionList = _sitesEmailNotificationsPermissionServices.GetPermissionsByUserId(SiteId, UserId);
                    if (userPermissionList.Count == 0)
                        BadRequest(new BadRequestError("No email notifications were found for the specified user."));

                    foreach (var permissions in userPermissionList)
                    {
                        var entity = await _sitesEmailNotificationsPermissionServices.GetEmailNotificationPermissionsById(permissions.Id);
                        if (entity != null)
                        {
                            entity.Active = active;
                            entity.UpdatedById = LoggedUserId;
                            entity.UpdatedOnUtc = GetDateTime;
                            _sitesEmailNotificationsPermissionServices.UpdateSitesEmailNotificationsPermission(entity);
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

        #region emailPreview
        [HttpGet("emailPreview/{id}")]
        public async Task<IActionResult> GetEmailPreview(string id)
        {
            var template = await _sitesEmailNotificationsPermissionServices.GetEmailNotificationPermissionDetailsById(id);

            var html = _workflowMessageService.BuildEmailLayout(
                template.SitesEmailNotifications.Body,
                null,
                false
            );

            return Ok(new { html, subject = template.SitesEmailNotifications.Subject });
        }
        #endregion

        #region private methods
        private async Task AddSiteEmailNotificationsAndPermissions(string siteId, string currentUserId, DateTime GetDateTime)
        {
            // Check if site notifications already exist
            var siteNotifications = _sitesEmailNotificationsService.GetSiteEmailNotificationBySiteId(siteId);

            // Get all active templates
            if (siteNotifications == null || !siteNotifications.Any())
            {
                var templates = await _messageTemplateService.GetAllMasterMessageTemplates();

                foreach (var template in templates)
                {
                    var siteNotification = new SitesEmailNotifications
                    {
                        Id = Guid.NewGuid().ToString(),
                        SiteId = siteId,
                        MessageTemplateId = template.Id,
                        Name = template.Name,
                        Subject = template.Subject,
                        Body = template.Body,
                        EmailAccountId = template.EmailAccountId,
                        CreatedById = currentUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedById = currentUserId,
                        UpdatedOnUtc = GetDateTime,
                        Active = true,
                        Deleted = false
                    };

                    _sitesEmailNotificationsService.InsertSitesEmailNotifications(siteNotification);
                }
            }

            //Insert permission only if not exists
            var ExistsPermission = _sitesEmailNotificationsPermissionServices.GetPermissionsByUserId(siteId, currentUserId);
            if(ExistsPermission == null || !ExistsPermission.Any())
            {
                var siteNotificationsList = _sitesEmailNotificationsService.GetSiteEmailNotificationBySiteId(siteId);

                foreach (var notification in siteNotificationsList)
                {
                    var permission = new SitesEmailNotificationsPermission
                    {
                        Id = Guid.NewGuid().ToString(),
                        SiteId = siteId,
                        SiteEmailNotificationId = notification.Id,
                        UserId = currentUserId,
                        CreatedById = currentUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedById = currentUserId,
                        UpdatedOnUtc = GetDateTime,
                        Active = true,
                        Deleted = false
                    };

                    _sitesEmailNotificationsPermissionServices.InsertSitesEmailNotificationsPermission(permission);
                }
            }
        }

        #endregion
    }
}
