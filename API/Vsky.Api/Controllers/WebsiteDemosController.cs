using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Api.ApiErrors;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Website_Demo;
using Vsky.Services.Website_Demo_Module;
using Vsky.Services.Sites;
using Vsky.Services.Messages;
using Microsoft.AspNetCore.Authorization;
using Vsky.Services.Users;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

namespace Vsky.Api.Controllers
{
    [Route("websitedemos")]
    public class WebsiteDemosController : BaseController
    {
        #region Define Services
        private readonly IMapper _mapper;
        private readonly IWebsite_DemosService _website_DemosService;
        private readonly IWebsite_Demo_ModulesService _website_Demo_ModulesService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ISiteService _siteService;
        private readonly IUserService _userService;
        #endregion

        #region Services Initializations
        public WebsiteDemosController(
            IMapper mapper,
            IWebsite_DemosService website_DemosService,
            IWebsite_Demo_ModulesService website_Demo_ModulesService,
            IWorkflowMessageService workflowMessageService,
            ISiteService siteService,
            IUserService userService
            )
        {
            _mapper = mapper;
            _website_DemosService = website_DemosService;
            _website_Demo_ModulesService = website_Demo_ModulesService;
            _workflowMessageService = workflowMessageService;
            _siteService = siteService;
            _userService = userService;
        }
        #endregion

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateWebsiteDemo(Website_DemosModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!await VerifyRecaptchaAsync(model.RecaptchaToken))
                        return BadRequest(new { message = "reCAPTCHA validation failed." });

                    var adminUserId = _userService.GetAdminUserId();
                    var GetDateTime = _siteService.GetDateTime();

                    var exists = await _website_DemosService.GetWebsiteDemoByEmail(model.EmailAddress);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Email address already exists, Please try with another."));

                    var entity = _mapper.Map<Website_Demos>(model);
                    entity.CreatedById = adminUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    _website_DemosService.InsertWebsite_Demo(entity);


                    if (model.ModulesIds != null && model.ModulesIds.Count() > 0)
                    {
                        foreach (var item in  model.ModulesIds)
                        {
                            if (item != null)
                            {
                                var website_Demo_Modules = new Website_Demo_Modules();
                                {
                                    website_Demo_Modules.WebsiteDemoId = entity.Id;
                                    website_Demo_Modules.ModuleId = item;

                                }        ;
                                _website_Demo_ModulesService.InsertWebsite_Demo_Modules(website_Demo_Modules);
                            }
                        }
                    }

                    var websiteDemo = await _website_DemosService.GetWebsiteDemoById(entity.Id);

                    await _workflowMessageService.SendWebsiteDemoMailToUser(websiteDemo);
                    await _workflowMessageService.SendDemoRequestSubmittedMailToVsky(websiteDemo);

                    return Ok(entity);
                }

                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
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
