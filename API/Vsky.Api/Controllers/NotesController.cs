using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.Companies;
using Vsky.Services.HelpDesks;
using Vsky.Services.Note;
using Vsky.Services.Notifications;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("notes")]
    public class NotesController : BaseController
    {
        #region Services Initialization
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly INoteService _noteService;
        private readonly ISiteService _siteService;
        private readonly IHelpDeskService _helpDeskService;
        private readonly INotificationService _notificationService;
        private readonly ICommonService _commonService;
        private readonly IMasterNotificationService _masterNotificationService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;

        public NotesController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            ISiteService siteService,
            INoteService noteService, 
            IHelpDeskService helpDeskService, 
            INotificationService notificationService, 
            ICommonService commonService,
            IMasterNotificationService masterNotificationService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _noteService = noteService;
            _mapper = mapper;
            _siteService = siteService;
            _helpDeskService = helpDeskService;
            _notificationService = notificationService;
            _commonService = commonService;
            _masterNotificationService = masterNotificationService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllNoteByTypeAndRecord
        [HttpGet]
        public async Task<IActionResult> GetAllNoteByTypeAndRecord(string subModuleId, string type, bool latestOnTop)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _noteService.GetAllNoteByTypeAndRecord(SiteId, subModuleId, type, latestOnTop);
                var model = _mapper.Map<List<NoteModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region AddNote
        [HttpPost]
        public async Task<IActionResult> AddEditNote(NoteModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var Editentity = await _noteService.GetById(model.Id);
                    if (Editentity != null)
                    {
                        //Editentity.Note = model.Note;
                        Editentity.Type = model.Type;
                        Editentity.SubModuleId = model.SubModuleId;

                        Editentity.Module = model.Module;
                        Editentity.ModuleId = model.ModuleId;
                        Editentity.Sub_Module = model.Sub_Module;

                        if (!string.IsNullOrEmpty(model.Note))
                        {
                            Editentity.Note = await _azureBlobImageServices
                                .ProcessHtmlAndManageImagesAsync(
                                    model.Note,
                                    SiteData.Name,
                                    "notes",
                                    Editentity.Id,
                                    Editentity.Note
                                );
                        }

                        Editentity.UpdatedOnUtc = GetDateTime;
                        Editentity.UpdatedById = LoggedUserId;
                        _noteService.UpdateNote(Editentity);

                        await SendTaggedUserNotifications(
                            model,
                            Editentity.Type,
                            Editentity.Id,
                            SiteId,
                            LoggedUserId,
                            GetDateTime
                        );
                    }
                    else
                    {
                        var Addentity = _mapper.Map<Notes>(model);
                        Addentity.Id = Guid.NewGuid().ToString();
                        Addentity.SiteId = SiteId;

                        Addentity.Module = model.Module;
                        Addentity.ModuleId = model.ModuleId;
                        Addentity.Sub_Module = model.Sub_Module;

                        if (!string.IsNullOrEmpty(model.Note))
                        {
                            Addentity.Note = await _azureBlobImageServices
                                .ProcessHtmlAndManageImagesAsync(
                                    model.Note,
                                    SiteData.Name,
                                    "notes",
                                    Addentity.Id
                                );
                        }

                        Addentity.CreatedById = LoggedUserId;
                        Addentity.CreatedOnUtc = GetDateTime;
                        Addentity.UpdatedById = LoggedUserId;
                        Addentity.UpdatedOnUtc = GetDateTime;
                        _noteService.InsertNote(Addentity);

                        await SendTaggedUserNotifications(
                            model,
                            Addentity.Type,
                            Addentity.Id,
                            SiteId,
                            LoggedUserId,
                            GetDateTime
                        );
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

        #region DeleteNote
        [HttpDelete("deletenote")]
        public async Task<IActionResult> DeleteNote(string id)
        {
            var entity = await _noteService.GetById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No comment found with the specified id."));

            _noteService.DeleteNotes(entity);

            return NoContent();
        }
        #endregion

        #region private method
        private async Task SendTaggedUserNotifications(
            NoteModel model,
            string noteType,
            string noteId,
            string siteId,
            string LoggedUserId,
            DateTime GetDateTime)
        {
            if (string.IsNullOrWhiteSpace(model.TaggedPersonId))
                return;

            bool isHelpDesk = noteType == "Help Desk Notes";
            string notificationKey = isHelpDesk ? "HelpDeskNotes1" : "ProjectNotes1";
            string redirectUrl = isHelpDesk ? "/help-desk" : "/project";

            foreach (var employeeId in model.TaggedPersonId.Split(','))
            {
                var CLoggedUserId = _commonService.GetLoggeduserIdByEmployeeId(siteId, employeeId);
                if (CLoggedUserId == null)
                    continue;

                var masterNotification = await _masterNotificationService.GetMasterNotificationByNumber(siteId, notificationKey, CLoggedUserId);

                if (masterNotification == null)
                    continue;

                var message = masterNotification.Message
                    .Replace("[Module Type]", model.Type)
                    .Replace("[Module Name]", model.Sub_Module);

                _notificationService.AddNotification(
                    siteId,
                    masterNotification.Title,
                    message,
                    masterNotification.Type,
                    CLoggedUserId,
                    noteId,
                    redirectUrl,
                    CLoggedUserId,
                    CLoggedUserId,
                    GetDateTime
                );
            }
        }
        #endregion
    }
}
