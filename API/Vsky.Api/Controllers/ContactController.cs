using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.Contacts;
using Vsky.Services.Messages;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("contact")]
    public class ContactController : BaseController
    {
        #region Services Initializations
        private readonly IMapper _mapper;
        private readonly GlobalVariable _globalVariable;
        private readonly IContactService _contactService;
        private readonly ISiteService _siteService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICommonService _commonService;

        public ContactController(
            IMapper mapper,
            GlobalVariable globalVariable,
            IContactService contactService,
            ISiteService siteService,
            IWorkflowMessageService workflowMessageService,
            ICommonService commonService)
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _contactService = contactService;
            _siteService = siteService;
            _workflowMessageService = workflowMessageService;
            _commonService = commonService;
        }
        #endregion

        #region Contact Us --Meldep

        #region Get All
        [HttpPost("contact-us-list")]
        public IActionResult GetAllContactUs(ContactSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var List = _contactService.GetAllContactUs(
                   SiteId,
                   searchModel.SearchText,
                   searchModel.FullName,
                   searchModel.Email,
                   searchModel.PhoneNo,
                   searchModel.Title,
                   searchModel.Source,
                   searchModel.SortBy,
                   searchModel.Descending,
                   searchModel.Page,
                   searchModel.PageSize
               );

                var model = new ContactUsList
                {
                    ContactUsLists = List,
                    Total = List.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region CreateContactUs
        [AllowAnonymous]
        [HttpPost("save-contact-us")]
        public async Task<IActionResult> CreateContactUs(SaveContactUs model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!await VerifyRecaptchaAsync(model.RecaptchaToken))
                        return BadRequest(new { message = "reCAPTCHA validation failed." });

                    var GetDateTime = _siteService.GetDateTime();

                    var entity = new Contact
                    {
                        Id = Guid.NewGuid().ToString(),
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNo = model.PhoneNo,
                        Title = model.Title,
                        Message = model.Message,
                        Source = "Meldep",
                        ContactedDate = GetDateTime,
                        Deleted = false
                    };
                    _contactService.InsertContact(entity);

                    await _workflowMessageService.SendContactUserEmail(entity);

                    return Ok(entity);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #endregion

        #region Vksy Website

        #region create contact
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateContact(ContactModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var GetDateTime = _siteService.GetDateTime();

                    var entity = _mapper.Map<Contact>(model);

                    entity.Source = "Vsky";
                    entity.ContactedDate = GetDateTime;
                    _contactService.InsertContact(entity);

                    await _workflowMessageService.SendContactUserEmail(entity);
                    await _workflowMessageService.SendContactCandidateEmail(entity);

                    return Ok(entity);
                }
                // Return model state errors if the model state is not valid
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #endregion

        #region private methods
        private async Task<bool> VerifyRecaptchaAsync(string token)
        {
            var secret = "6Lc4MnIsAAAAAPuolPzOkwEGxWKkYxhAAqGiPwxo"; // Get from Google reCAPTCHA admin
            using var client = new HttpClient();
            var response = await client.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}",
                null
            );

            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RecaptchaResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Success ?? false;
        }

        private class RecaptchaResponse
        {
            public bool Success { get; set; }
            public List<string> ErrorCodes { get; set; }
        }
        #endregion
    }
}
