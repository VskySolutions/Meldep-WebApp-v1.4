using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AutoMapper;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.Companies;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.EmailNotifications;
using Vsky.Services.EmailReply;
using Vsky.Services.Employees;
using Vsky.Services.HelpDesks;
using Vsky.Services.Logging;
using Vsky.Services.Messages;
using Vsky.Services.Notifications;
using Vsky.Services.Persons;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.Users;

namespace Vsky.Api.Controllers
{
    [Route("help-desk")]
    public class HelpDeskController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IHelpDeskService _helpDeskService;
        private readonly IHelpDeskTopicService _helpDeskTopicService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IUserService _userService;
        private readonly IEmployeeService _employeeService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly INotificationService _notificationService;
        private readonly IMasterNotificationService _masterNotificationService;
        private readonly IEmailRepliesServices _emailRepliesServices;
        private readonly IHelpDeskFilesService _helpDeskFilesService;
        private readonly IHelpDeskEmailRepliesMappingService _helpDeskEmailRepliesMappingService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly IDropDownService _dropDownService;
        private readonly ILogger _loggerService;
        private readonly IPersonService _personService;
        private readonly ISitesEmailNotificationsPermissionServices _sitesEmailNotificationsPermissionServices;
        private readonly ICompanyService _companyService;
        private readonly ICompanyClientsService _companyClientsService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations
        public HelpDeskController(
            GlobalVariable globalVariable,
            IMapper mapper,
            IHelpDeskService helpDeskService,
            ICommonService commonService,
            ISiteService siteService,
            IHelpDeskTopicService helpDeskTopicService,
            IUserService userService,
            IEmployeeService employeeService,
            IWorkflowMessageService workflowMessageService,
            INotificationService notificationService,
            IMasterNotificationService masterNotificationService,
            IEmailRepliesServices emailRepliesServices,
            IHelpDeskFilesService helpDeskFilesService,
            IHelpDeskEmailRepliesMappingService helpDeskEmailRepliesMappingService,
            ISitesModifiedLogsService sitesModifiedLogsService,
            IDropDownService dropDownService,
            IDropDownTypeService dropDownTypeService,
            ILogger loggerService,
            IPersonService personService,
            ISitesEmailNotificationsPermissionServices sitesEmailNotificationsPermissionServices,
            ICompanyService companyService,
            ICompanyClientsService companyClientsService,
            IAzureBlobImageServices azureBlobImageServices
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _helpDeskService = helpDeskService;
            _commonService = commonService;
            _siteService = siteService;
            _helpDeskTopicService = helpDeskTopicService;
            _userService = userService;
            _employeeService = employeeService;
            _workflowMessageService = workflowMessageService;
            _notificationService = notificationService;
            _masterNotificationService = masterNotificationService;
            _emailRepliesServices = emailRepliesServices;
            _helpDeskFilesService = helpDeskFilesService;
            _helpDeskEmailRepliesMappingService = helpDeskEmailRepliesMappingService;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _loggerService = loggerService;
            _personService = personService;
            _sitesEmailNotificationsPermissionServices = sitesEmailNotificationsPermissionServices;
            _companyService = companyService;
            _companyClientsService = companyClientsService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllHelpDesks
        // Title: Get All HelpDesk
        // Description: This endpoint fetches a list of HelpDesk based on the provided search criteria such as title, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllHelpDesks(HelpDeskSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                //var createdBy = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);

                var createdBy = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                // Fetch a list of HelpDesk based on search criterias
                var list = _helpDeskService.GetAllHelpDesks(
                    SiteId,
                    searchModel.SearchText,
                    LoggedUserId,
                    searchModel.AssignedToId,
                    searchModel.EmployeeEmails,
                    searchModel.StatusIds,
                    searchModel.PriorityIds,
                    searchModel.TopicIds,
                    searchModel.QuestionIds,
                    createdBy,
                    searchModel.Title,
                    searchModel.TicketNo,
                    searchModel.CompanyIds,
                    searchModel.CategoryIds,
                    searchModel.TicketFromDate,
                    searchModel.TicketToDate,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var prefix = GetSiteTicketInfoAsync(SiteId).Result;

                // Map the fetched list to a model suitable for the response
                var model = new HelpDeskListModel
                {
                    Data = _mapper.Map<IList<HelpDeskModel>>(list),
                    Total = list.TotalCount
                };

                foreach (var item in model.Data)
                {
                    item.DisplayTicketNo = $"{prefix.TicketNoPrefix}-{item.TicketNo:D2}";
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                _loggerService.Error("Error fetching helpdesk list", ex);
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetRequesterAndComapnyDropdowns
        // Title: GetRequesterAndComapnyDropdowns
        // Description: This endpoint retrieves the list of Requester. 
        [HttpGet("requesterDropdown/list")]
        public async Task<IActionResult> GetRequesterDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _helpDeskService.GetRequesterDropdown(SiteId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Description: This endpoint retrieves the list of Company. 
        [HttpGet("companyDropdown/list")]
        public async Task<IActionResult> GetCompanyDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _helpDeskService.GetCompanyDropdown(SiteId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllHelpDeskTopicListForDropdown
        // Title: GetAllHelpDeskTopicListForDropdown
        // Description: This endpoint retrieves the list of topic. 
        [HttpGet("topicdropdown/list")]
        public async Task<IActionResult> GetAllHelpDeskTopicListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _helpDeskTopicService.GetAllHelpDeskTopicListForDropdown(SiteId);
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllHelpDeskTopicQuestionsListForDropdown
        // Title: GetAllProjectModuleListForDropdown
        // Description: This endpoint retrieves the list of Help Desk Topic Questions. 
        [HttpGet("questionsdropdown/list")]
        public async Task<IActionResult> GetAllHelpDeskTopicQuestionsListForDropdown(string TopicId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _helpDeskTopicService.GetAllHelpDeskTopicQuestionsListForDropdown(SiteId, TopicId);
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetHelpDeskDetailsById
        // Title: GetHelpDeskDetailsById
        // Description: This endpoint retrieves the details of a specific helpdesk based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHelpDeskDetailsById(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                // Fetch the helpdesk entity by its ID from the service
                var entity = await _helpDeskService.GetHelpDeskDetailsById(id);
                var prefix = await GetSiteTicketInfoAsync(SiteId);

                // If the helpdesk entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No help desk found with the specified id."));

                entity.AverageDurationText = FormatDuration(entity.AverageDurationInMinutes);

                var logs = entity.HelpDeskStatusLog?.OrderByDescending(x => x.CreatedOnUtc).ToList();

                if (logs?.Count > 0)
                {
                    int lastIndex = logs.Count - 1;

                    logs[lastIndex].StatusDurationText = "";

                    for (int i = 0; i < lastIndex; i++)
                    {
                        logs[i].StatusDurationText =
                            logs[i].DurationInMinutes > 0
                            ? FormatDuration(logs[i].DurationInMinutes)
                            : "0 minutes";
                    }

                    entity.HelpDeskStatusLog = logs;
                }

                // Map the helpdesk entity to a HelpDeskModel object
                var model = _mapper.Map<HelpDeskModel>(entity);
                model.DisplayTicketNo = $"{prefix.TicketNoPrefix}-{entity.TicketNo:D2}";

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region AddFiles
        // Title: AddFiles
        [HttpPost("add-help-desk-files")]
        public async Task<IActionResult> AddFiles([FromForm] HelpDeskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //var siteDetails = await _siteService.GetSiteTicketNoPrefixById(SiteId);
                    //var ticketNoPrefix = siteDetails != null ? siteDetails.TicketNoPrefix : "HR";
                    var siteInfo = await GetSiteTicketInfoAsync(SiteId);
                    string ticketNoPrefix = siteInfo != null ? siteInfo.TicketNoPrefix : "HR";

                    var entity = await _helpDeskService.GetHelpDeskDetailsById(model.Id);

                    // If no help desk is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No help desk found with the specified id."));

                    //Add
                    string HelpDeskId = entity.Id;
                    if (model.HelpDeskFiles != null && model.HelpDeskFiles.Any())
                    {
                        string ticketNo = $"{ticketNoPrefix}-{entity.TicketNo:D2}";

                        int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(HelpDeskId, "Help Desk");

                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "helpdesk", model.HelpDeskFiles, entity.TicketNo.ToString(), existingImagesCount);
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.HelpDeskFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = model.Id,
                                Module = entity.Title,
                                SubModuleId = HelpDeskId,
                                Sub_Module = entity.Title,
                                Type = "Help Desk",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var HelpDeskFiles = new HelpDeskFiles
                            {
                                FileId = picture.Id,
                                HelpDeskId = HelpDeskId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _helpDeskFilesService.InsertHelpDeskFile(HelpDeskFiles);

                            index++;
                        }
                        //var uploadDirectory = Path.Combine("wwwroot", "uploads", "HelpDesk", ticketNo);

                        //if (!Directory.Exists(uploadDirectory))
                        //    Directory.CreateDirectory(uploadDirectory);

                        //foreach (var file in model.HelpDeskFiles)
                        //{
                        //    var originalFileName = Path.GetFileName(file.FileName);
                        //    string sanitizedFileName = Regex.Replace(originalFileName.Trim(), "[^A-Za-z0-9_. ]+", "").Replace(" ", "_");

                        //    string uniqueFileName = GetUniqueFileName(uploadDirectory, sanitizedFileName);
                        //    var filePath = Path.Combine(uploadDirectory, uniqueFileName);
                        //    var virtualPath = $"/uploads/HelpDesk/{ticketNo}/{uniqueFileName}";

                        //    // Save file
                        //    using (var stream = System.IO.File.Create(filePath))
                        //    {
                        //        file.CopyTo(stream);
                        //    }

                        //    // Save picture metadata
                        //    var picture = new Picture
                        //    {
                        //        SeoFilename = originalFileName,
                        //        MimeType = file.ContentType,
                        //        VirtualPath = virtualPath,
                        //        ModuleId = model.Id,
                        //        Module = entity.Title,
                        //        SubModuleId = HelpDeskId,
                        //        Sub_Module = entity.Title,
                        //        Type = "Help Desk",
                        //        SiteId = SiteId,
                        //        CreatedById = LoggedUserId,
                        //        CreatedOnUtc = GetDateTime
                        //    };

                        //    _commonService.InsertPicture(picture);

                        //    var HelpDeskFiles = new HelpDeskFiles();

                        //    // Set custom properties
                        //    HelpDeskFiles.FileId = picture.Id;
                        //    HelpDeskFiles.HelpDeskId = HelpDeskId;

                        //    // Set the created by and created on properties
                        //    HelpDeskFiles.CreatedById = LoggedUserId;
                        //    HelpDeskFiles.CreatedOnUtc = GetDateTime;
                        //    _helpDeskFilesService.InsertHelpDeskFile(HelpDeskFiles);
                        //}
                    }
                    return Ok(entity);
                }
                // Return model state errors if the model state is not valid
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region CreateHelpDesk
        // Title: CreateHelpDesk
        // Description: This endpoint handles the creation of a new helpdesk. It first checks if a helpdesk with the same name already exists. If not, it maps the helpdesk model to the helpdesk entity, sets the creation details, and inserts the helpdesk into the database.
        [HttpPost]
        public async Task<IActionResult> CreateHelpDesk([FromForm] HelpDeskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // get site name and TicketNoPrefix
                    var siteInfo = await GetSiteTicketInfoAsync(SiteId);
                    string ticketNoPrefix = siteInfo != null ? siteInfo.TicketNoPrefix : "HR";

                    // Map the HelpDesk model to the HelpDesk entity
                    var entity = _mapper.Map<HelpDesk>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    // generate twilio EmailId
                    string twilioEmailId = Guid.NewGuid().ToString("N");

                    if (string.IsNullOrEmpty(model.CompanyId))
                    {
                        if (SiteData != null)
                        {
                            var company = await _companyService.GetCompanyByNameAndSiteId(SiteData.Name, SiteId);
                            if (company != null)
                            {
                                var customer = await _companyClientsService.GetCustomerByCompanyId(company.Id);
                                if (customer != null)
                                {
                                    entity.CompanyId = customer.Id;
                                }
                            }
                        }
                    }
                    else
                    {
                        entity.CompanyId = model.CompanyId;
                    }

                    entity.TwilioEmailId = twilioEmailId;
                    entity.SiteId = SiteId;
                    entity.RequesterId = model.RequesterId;
                    entity.TicketNo = await _helpDeskService.GetLastTicketId(SiteId);
                    entity.PriorityId = model.PriorityId;
                    //entity.CompanyId = model.CompanyId;
                    entity.CategoryId = model.CategoryId;

                    if (model.TopicId != null && model.TopicId != "")
                        entity.TopicId = model.TopicId;
                    else
                        entity.TopicId = null;

                    if (model.QuestionId != null && model.QuestionId != "")
                        entity.QuestionId = model.QuestionId;
                    else
                        entity.QuestionId = null;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "helpdesk",
                                entity.TicketNo.ToString(),
                                entity.Description
                            );
                    }

                    // Set the created by and created on properties
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _helpDeskService.InsertHelpDesk(entity);

                    //Add
                    string HelpDeskId = entity.Id;
                    if (model.HelpDeskFiles != null && model.HelpDeskFiles.Any())
                    {
                        string ticketNo = $"{ticketNoPrefix}-{entity.TicketNo:D2}";

                        // Upload multiple files to Azure
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "helpdesk", model.HelpDeskFiles, entity.TicketNo.ToString());
                        int index = 0;

                        foreach (var fileUrl in urls)
                        {
                            var file = model.HelpDeskFiles[index];

                            var picture = new Picture
                            {
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                VirtualPath = fileUrl, // Azure URL
                                ModuleId = HelpDeskId,
                                Module = entity.Title,
                                SubModuleId = HelpDeskId,
                                Sub_Module = entity.Title,
                                Type = "Help Desk",
                                SiteId = SiteId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _commonService.InsertPicture(picture);

                            var HelpDeskFiles = new HelpDeskFiles
                            {
                                FileId = picture.Id,
                                HelpDeskId = HelpDeskId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            _helpDeskFilesService.InsertHelpDeskFile(HelpDeskFiles);

                            index++;
                        }
                    }

                    // add help desk status
                    var statusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "HelpDesk Status", "New");
                    if(statusId == null)
                        return BadRequest("Help desk status not found");

                    _helpDeskService.AddHelpDeskStatusLog(entity.Id, statusId, LoggedUserId, GetDateTime);

                    #region Emails and notifications
                    var helpDesk = await _helpDeskService.GetHelpDeskDetailsById(entity.Id);

                    // ---------------------------
                    // REQUESTER EMAIL
                    // ---------------------------
                    Employee requesterEmployee = null;
                    string requesterUserId = null;

                    if (!string.IsNullOrEmpty(helpDesk.RequesterId))
                    {
                        requesterEmployee = await _employeeService.GetEmployeeDetailsById(helpDesk.RequesterId);
                        if (requesterEmployee != null)
                        {
                            var user = await _userService.GetUserByEmployeeId(SiteId, requesterEmployee.Id);
                            requesterUserId = user?.Id;
                        }
                    }

                    string requesterEmailId =
                        requesterEmployee?.Person?.PrimaryEmailAddress ??
                        helpDesk.RequesterEmail;

                    if (!string.IsNullOrEmpty(requesterEmailId) && !string.IsNullOrEmpty(requesterUserId))
                    {
                        var canSendEmail = await _sitesEmailNotificationsPermissionServices
                            .ShouldSendNotification(
                            SiteId,
                            requesterUserId,
                            "HelpDesk.RequestReceived"
                            );

                        if (canSendEmail)
                        {
                            await _workflowMessageService.SendRequestReceivedMailToRequester(
                                requesterEmployee,
                                helpDesk,
                                twilioEmailId,
                                requesterEmailId,
                                true,
                                ticketNoPrefix,
                                null,
                                GetDateTime
                            );
                        }
                    }

                    var badderSiteData = await _siteService.GetBySiteName("Bader Rutter");
                    if (badderSiteData == null || SiteId != badderSiteData.Id)
                    {
                        // send meail to admin/support
                        var supports = await _userService.GetUsersByRole(SiteId, "support team");
                        var supportEmails = new List<string>();
                        Employee primaryEmployee = null;

                        foreach (var user in supports)
                        {
                            // Check permission per support user
                            var canSendAdmin = await _sitesEmailNotificationsPermissionServices
                                    .ShouldSendNotification(
                                        SiteId,
                                        user.Id,
                                        "HelpDesk.HelpDeskMailSendToAdmin"
                                    );

                            if (!canSendAdmin)
                                continue;

                            //var employeeId = _commonService.GetEmployeeIdByUserId(SiteId, user.Id);

                            var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                            var employeeData = await _employeeService.GetEmployeeDetailsById(employeeId);

                            if (!string.IsNullOrEmpty(employeeData?.Person?.PrimaryEmailAddress))
                            {
                                supportEmails.Add(employeeData.Person.PrimaryEmailAddress);

                                // keep first employee as owner reference
                                if (primaryEmployee == null)
                                    primaryEmployee = employeeData;
                            }
                        }

                        if (supportEmails.Any())
                        {
                            await _workflowMessageService.SendHelpDeskMailToAdmin(
                                primaryEmployee,
                                helpDesk,
                                true,
                                twilioEmailId,
                                ticketNoPrefix,
                                supportEmails.Distinct().ToList(),
                                "",
                                "",
                                GetDateTime
                            );
                        }
                    }
                    #endregion

                    return Ok(entity);
                }

                // Return model state errors if the model state is not valid
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                _loggerService.Error("Email Send Failed", ex, null);

                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Twilio Client Email Replies
        [AllowAnonymous]
        [HttpPost("email-replies")]
        public async Task<IActionResult> TwilioClientEmailReplies()
        {
            try
            {
                // Detect content-type
                var form = await Request.ReadFormAsync();

                //Normal Form mode(text / html provided separately)
                string fromNormal = form["from"];
                string toNormal = form["to"];
                string subjectNormal = form["subject"];
                string text = form["text"];
                string html = form["html"];
                // string bodyNormal = !string.IsNullOrWhiteSpace(text) ? text : html;
                string finalHtml = await FilterRawHTMLBody(html, text, form.Files, form["attachment-info"], form["content-ids"]);

                return await SaveEmailReply(toNormal, fromNormal, subjectNormal, finalHtml);
            }
            catch (Exception ex)
            {
                _loggerService.Error("Ticket Replies Failed", ex, null);
                return StatusCode(500, new { error = ex.Message });
            }
        }
        #endregion

        #region GetAllHelpDeskEmailRepliesMappingList
        // Title: GetAllHelpDeskEmailRepliesMappingList
        // Description: This endpoint retrieves the list of email replies
        [HttpGet("emailReplies/list")]
        public async Task<IActionResult> GetAllHelpDeskEmailRepliesMappingList(string HelpDeskId = null, int skipIndex = 0, int takeCount = 10, bool isSystemEmail = false)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                #region Logged-in Employee
                //var loggedEmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);

                var loggedEmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                var loggedEmployee = await _employeeService.GetEmployeeDetailsById(loggedEmployeeId);
                string fromEmail = loggedEmployee?.Person?.PrimaryEmailAddress;
                #endregion

                // HelpDesk
                var helpDesk = await _helpDeskService.GetHelpDeskDetailsById(HelpDeskId);
                if (helpDesk == null)
                    return BadRequest("Help Desk not found");

                #region Admin/ SupportTeam/ Requester/AssignedTo users
                var adminUsers = await _userService.GetUsersByRole(SiteId, "admin");
                var supportUsers = await _userService.GetUsersByRole(SiteId, "support team");

                bool isAdmin = adminUsers.Any(x => x.Id == LoggedUserId);
                bool isSupportTeam = supportUsers.Any(x => x.Id == LoggedUserId);
                bool isRequester = helpDesk.RequesterId == loggedEmployeeId;
                bool isAssignedPerson = helpDesk.AssignedToId == loggedEmployeeId;

                bool canReplyToRequester = isAdmin || isSupportTeam || isAssignedPerson;
                #endregion

                // Replies list
                var replies = await _helpDeskEmailRepliesMappingService.GetAllHelpDeskEmailRepliesMappingList(SiteId, HelpDeskId, skipIndex, takeCount, isSystemEmail);

                var emailRepliesList = new List<EmailRepliesModel>();

                foreach (var reply in replies)
                {
                    var from = reply.EmailReplies.FromEmail;
                    var to = reply.EmailReplies.ToEmail;

                    var mainToEmails = reply.EmailReplies.ToEmail;
                    var externalToEmails = reply.EmailReplies.ExternalToEmail;
                    var ccEmails = reply.EmailReplies.CCEmail;

                    // Merge To + ExternalTo for display
                    //var to = string.Join(",",
                    //    new[]
                    //    {
                    //        mainToEmails,
                    //        externalToEmails
                    //    }
                    //    .Where(x => !string.IsNullOrWhiteSpace(x))
                    //);

                    var email = new EmailRepliesModel
                    {
                        Id = reply.Id,
                        FromEmail = from,
                        ToEmail = to,
                        ExternalToEmail = reply.EmailReplies.ExternalToEmail,
                        CCEmail = reply.EmailReplies.CCEmail,
                        Subject = reply.EmailReplies.Subject,
                        Body = reply.EmailReplies.Body,
                        IsRead = reply.EmailReplies.IsRead,
                        IsSystemEmail = reply.EmailReplies.IsSystemEmail,
                        CreatedOnUtc = reply.EmailReplies.CreatedOnUtc,
                        CreatedOnStr = reply.EmailReplies.CreatedOnUtc.ToString("MM/dd/yyyy hh:mm tt"),
                        CreatedOnUtcStr = reply.EmailReplies.CreatedOnUtc.ToString("ddd, d MMM yyyy 'at' h:mm tt"),
                        OwnerId = reply.EmailReplies.OwnerId,
                        TwilioEmailId = reply.EmailReplies.TwilioEmailId,
                        CreatedBy = reply.HelpDesk.CreatedBy.Person.FullName,
                        StatusText = reply.HelpDesk.StatusText,
                        ToName = await GetNameByEmail(SiteId, to),
                        FromName = await GetNameByEmail(SiteId, from),
                        CCName = await GetNameByEmail(SiteId, ccEmails),
                        ExternalName = await GetNameByEmail(SiteId, externalToEmails)
                    };

                    emailRepliesList.Add(email);
                }

                #region From / To Email Logic

                var toEmails = new List<string>();

                //Requester replying
                if (isRequester)
                {
                    // Assigned exists → assigned person
                    if (!string.IsNullOrEmpty(helpDesk.AssignedToId))
                    {
                        var assignedEmp = await _employeeService.GetEmployeeDetailsById(helpDesk.AssignedToId);
                        if (!string.IsNullOrEmpty(assignedEmp?.Person?.PrimaryEmailAddress))
                        {
                            toEmails.Add(assignedEmp.Person.PrimaryEmailAddress);
                        }
                    }
                    else
                    {
                        // No assigned → ticket team
                        toEmails.Add("ticketteam@vskysolutions.com");
                    }
                }

                // Admin OR Support Team → Requester
                else if (canReplyToRequester)
                {
                    // Existing requester
                    if (!string.IsNullOrEmpty(helpDesk.RequesterId))
                    {
                        var requesterEmp = await _employeeService.GetEmployeeDetailsById(helpDesk.RequesterId);

                        if (!string.IsNullOrEmpty(requesterEmp?.Person?.PrimaryEmailAddress))
                        {
                            toEmails.Add(requesterEmp.Person.PrimaryEmailAddress);
                        }
                    }
                    // New requester (email entered manually)
                    else if (!string.IsNullOrEmpty(helpDesk.RequesterEmail))
                    {
                        toEmails.Add(helpDesk.RequesterEmail);
                    }
                }
                #endregion

                var Data = new EmailRepliesList
                {
                    EmailRepliesLists = emailRepliesList,
                    Total = emailRepliesList.Count,
                    ReplyToEmails = toEmails,
                    FromEmail = fromEmail
                };

                return Ok(Data);
            }
            catch (Exception ex)
            {
                _loggerService.Error("Error while fetching helpdesk email replies", ex);
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region craete email replies
        [HttpPost("save-email-replies")]
        public async Task<IActionResult> CreateEmailReplies(EmailRepliesModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ModelStateError(ModelState);

                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                // get site name and TicketNoPrefix
                var siteInfo = await GetSiteTicketInfoAsync(SiteId);
                string prefixNo = siteInfo != null ? siteInfo.TicketNoPrefix : "HR";

                #region Fetch HelpDesk

                var helpDesk = await _helpDeskService.GetHelpDeskDetailsById(model.HelpDeskId);
                if (helpDesk == null)
                    return BadRequest("Help Desk not found");
                #endregion

                #region Role Detection (MULTI-ROLE SAFE)

                var adminUsers = await _userService.GetUsersByRole(SiteId, "admin");
                var supportUsers = await _userService.GetUsersByRole(SiteId, "support team");

                bool isAdmin = adminUsers.Any(x => x.Id == LoggedUserId);
                bool isSupportTeam = supportUsers.Any(x => x.Id == LoggedUserId);

                #endregion

                #region From Employee (Logged-in User)

                //var loggedEmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var loggedEmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                var fromEmployee = await _employeeService.GetEmployeeDetailsById(loggedEmployeeId);
                if(fromEmployee == null)
                    return BadRequest("Employee not found. Please create employee.");

                #endregion

                #region Find who is replying

                bool isRequesterReply =
                    !string.IsNullOrEmpty(helpDesk.RequesterId) &&
                    helpDesk.RequesterId == loggedEmployeeId;

                bool isAssignedReply =
                    !string.IsNullOrEmpty(helpDesk.AssignedToId) &&
                    helpDesk.AssignedToId == loggedEmployeeId;

                bool hasAssignedTo = !string.IsNullOrEmpty(helpDesk.AssignedToId);

                #endregion

                #region status logic         

                // Admin OR Support Team → Requester
                if (isAdmin || isSupportTeam)
                {
                    // Status updates
                    if (helpDesk.StatusText == "New")
                    {
                        var statusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "HelpDesk Status", "Open");
                        if(statusId == null)
                            return BadRequest("Help desk status not found");

                        _helpDeskService.AddHelpDeskStatusLog(model.HelpDeskId, statusId, LoggedUserId, GetDateTime);

                        await SendHelpDeskStatusUpdateMail(model.HelpDeskId, SiteId, "", GetDateTime);
                    }
                }
                else if (helpDesk.StatusText == "Awaiting Client")
                {
                    var statusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "HelpDesk Status", "In Progress");
                    if (statusId == null)
                        return BadRequest("Help desk status not found");

                    _helpDeskService.AddHelpDeskStatusLog(model.HelpDeskId, statusId, LoggedUserId, GetDateTime);

                    await SendHelpDeskStatusUpdateMail(model.HelpDeskId, SiteId, "", GetDateTime);
                }
                #endregion

                #region Prepare TO Recipients

                // actual system generated TO emails
                var actualToEmails = new List<string>();

                // merged TO emails for sending
                var toEmails = new List<string>(); 
                var ccEmails = new List<string>();

                // REQUESTER REPLY
                if (isRequesterReply)
                {
                    if (hasAssignedTo)
                    {
                        var assignedEmp = await _employeeService.GetEmployeeDetailsById(helpDesk.AssignedToId);

                        //if (!string.IsNullOrEmpty(assignedEmp?.Person?.PrimaryEmailAddress))
                        //    toEmails.Add(assignedEmp.Person.PrimaryEmailAddress);
                        if (!string.IsNullOrEmpty(assignedEmp?.Person?.PrimaryEmailAddress))
                            actualToEmails.Add(assignedEmp.Person.PrimaryEmailAddress);
                    }
                    else
                    {
                        foreach (var user in supportUsers)
                        {
                            //var empId = _commonService.GetEmployeeIdByUserId(SiteId, user.Id);

                            var empId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, user.Id);
                            var emp = await _employeeService.GetEmployeeDetailsById(empId);

                            //if (!string.IsNullOrEmpty(emp?.Person?.PrimaryEmailAddress))
                            //    toEmails.Add(emp.Person.PrimaryEmailAddress);
                            if (!string.IsNullOrEmpty(emp?.Person?.PrimaryEmailAddress))
                                actualToEmails.Add(emp.Person.PrimaryEmailAddress);
                        }
                    }
                }

                // ASSIGNED PERSON REPLY
                else
                {
                    if (!string.IsNullOrEmpty(helpDesk.RequesterId))
                    {
                        var requesterEmp = await _employeeService.GetEmployeeDetailsById(helpDesk.RequesterId);

                        //if (!string.IsNullOrEmpty(requesterEmp?.Person?.PrimaryEmailAddress))
                        //    toEmails.Add(requesterEmp.Person.PrimaryEmailAddress);
                        if (!string.IsNullOrEmpty(requesterEmp?.Person?.PrimaryEmailAddress))
                            actualToEmails.Add(requesterEmp.Person.PrimaryEmailAddress);
                    }
                    else if (!string.IsNullOrEmpty(helpDesk.RequesterEmail))
                    {
                        //toEmails.Add(helpDesk.RequesterEmail);
                        actualToEmails.Add(helpDesk.RequesterEmail);
                    }
                }

                #endregion

                #region External TO Emails
                var externalToEmails = new List<string>();

                if (!string.IsNullOrWhiteSpace(model.ExternalToEmail))
                {
                    externalToEmails = model.ExternalToEmail
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    //toEmails.AddRange(externalToEmails);
                }

                #endregion

                #region CC Emails

                if (!string.IsNullOrWhiteSpace(model.CCEmail))
                {
                    ccEmails = model.CCEmail
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }

                #endregion

                #region Merge TO Emails For Sending

                toEmails.AddRange(actualToEmails);
                toEmails.AddRange(externalToEmails);

                #endregion

                #region Remove Duplicate Emails

                actualToEmails = actualToEmails
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                toEmails = toEmails
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                ccEmails = ccEmails
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                #endregion

                var emailReplyEntity = new EmailReplies();
                emailReplyEntity.Id = Guid.NewGuid().ToString();

                emailReplyEntity.Subject = model.Subject;
                emailReplyEntity.FromEmail = fromEmployee?.Person?.PrimaryEmailAddress;

                if(externalToEmails.Count > 0 && externalToEmails.Any())
                    emailReplyEntity.ExternalToEmail = string.Join(",", externalToEmails);

                if(ccEmails.Count > 0 && ccEmails.Any())
                    emailReplyEntity.CCEmail = string.Join(",", ccEmails);

                emailReplyEntity.OwnerId = fromEmployee.Id;

                if (!string.IsNullOrEmpty(model.Body))
                {
                    emailReplyEntity.Body = await _azureBlobImageServices
                        .ProcessHtmlAndManageImagesAsync(
                            model.Body,
                            SiteData.Name,
                            "helpdesk",
                            emailReplyEntity.Id,
                            emailReplyEntity.Body
                        );
                }

                #region Save Replies + Send Mail (EMPLOYEE GROUP)

                if (toEmails.Any())
                {
                    await _workflowMessageService.SendHelpDeskReplyMail(
                        null,
                        helpDesk,
                        emailReplyEntity,
                        model.TwilioEmailId,
                        toEmails,
                        actualToEmails,
                        ccEmails,
                        prefixNo,
                        GetDateTime
                    );
                }
                #endregion

                return Ok();
            }
            catch (Exception ex)
            {
                _loggerService.Error("Help Desk-> Email Replies-> Failed", ex, null);
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateHelpDeskStatus
        //created for update HelpDesk status from list page
        [HttpPut("{id}/{statusId}")]
        public async Task<IActionResult> UpdateHelpDeskStatus(string id, string statusId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the HelpDesk entity by its ID
                    var entity = await _helpDeskService.GetHelpDeskById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No HelpDesk found with the specified id."));

                    _helpDeskService.AddHelpDeskStatusLog(entity.Id, statusId, LoggedUserId, GetDateTime);

                    // send mail to all users
                    await SendHelpDeskStatusUpdateMail(id, SiteId, "", GetDateTime);
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

        #region UpdateAssignedTo
        //created for update HelpDesk assigned to from list page
        [HttpPut("assignedTo/{id}/{assignedToId}")]
        public async Task<IActionResult> UpdateAssignedTo(string id, string assignedToId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // get site name and TicketNoPrefix
                    var siteInfo = await GetSiteTicketInfoAsync(SiteId);
                    string prefixNo = siteInfo != null ? siteInfo.TicketNoPrefix : "HR";
                    string siteName = siteInfo.Name;

                    // Fetch the issue entity by its ID
                    var entity = await _helpDeskService.GetHelpDeskById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No HelpDesk found with the specified id."));

                    bool IsAssignedToChanged = assignedToId != entity.AssignedToId;

                    entity.AssignedToId = assignedToId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _helpDeskService.UpdateHelpDesk(entity);

                    // get helpDesk details
                    var HelpDeskDetails = await _helpDeskService.GetHelpDeskDetailsById(id);

                    // Assigned To
                    var assignedTo = await _employeeService.GetEmployeeDetailsById(entity.AssignedToId);
                    var assignedToName = assignedTo?.Person?.FullName;

                    if (IsAssignedToChanged)
                    {
                        if (HelpDeskDetails.StatusText == "New" || HelpDeskDetails.StatusText == "Open")
                        {
                            var statusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "HelpDesk Status", "Assigned");
                            if(statusId == null)
                                return BadRequest("Help desk status not found");

                            _helpDeskService.AddHelpDeskStatusLog(entity.Id, statusId, LoggedUserId, GetDateTime);
                        }

                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "HelpDesk", entity.Id, entity.Title, entity.Id, entity.Title, "Assigned To", HelpDeskDetails.AssignedTo.Person.FullName, LoggedUserId, GetDateTime);

                        await SendHelpDeskStatusUpdateMail(id, SiteId, assignedToName, GetDateTime);
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

        #region AddorUpdateHelpDeskStatusComment
        //created for update HelpDesk Status Comment from list page
        [HttpPut("comment/{id}")]
        public async Task<IActionResult> AddorUpdateHelpDeskStatusComment(string id, HelpDeskModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the issue entity by its ID
                    var entity = await _helpDeskService.GetHelpDeskById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No HelpDesk found with the specified id."));

                    entity.ClosingComment = model.ClosingComment;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _helpDeskService.UpdateHelpDesk(entity);

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

        #region UpdateHelpDeskPriority
        //created for update HelpDesk status from list page
        [HttpPut("priority/{id}/{priorityId}")]
        public async Task<IActionResult> UpdateHelpDeskPriority(string id, string priorityId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool result = await UpdateHelpDeskDetails(id, priorityId, "priority");
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

        #region UpdateHelpDeskCompanyClient
        //created for update HelpDesk status from list page
        [HttpPut("company/{id}/{companyId}")]
        public async Task<IActionResult> UpdateHelpDeskCompany(string id, string companyId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool result = await UpdateHelpDeskDetails(id, companyId, "company");
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

        #region Microsoft PowerMate Integration Webhooks
        [AllowAnonymous]
        [HttpPost("createTicketFromEmail")]
        public async Task<IActionResult> createTicketFromEmailAsync([FromBody] EmailTicketDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.SiteId))
                    return BadRequest("No Site Id Found");

                var SiteData = await _siteService.GetById(model.SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                string twilioEmailId = Guid.NewGuid().ToString("N");

                // get site name and TicketNoPrefix
                var siteInfo = await GetSiteTicketInfoAsync(model.SiteId);
                string prefixNo = siteInfo != null ? siteInfo.TicketNoPrefix : "HR";

                var TopicData = await _helpDeskTopicService.GetAllHelpDeskTopicListForDropdown(model.SiteId);
                string TopicId = TopicData.FirstOrDefault().Value;
                var QuestionData = await _helpDeskTopicService.GetAllHelpDeskTopicQuestionsListForDropdown(model.SiteId, TopicId);
                string QuestionId = QuestionData.FirstOrDefault().Value;

                var PriorityId = await _dropDownService.GetDropDownByTypeNameAndName(model.SiteId, "HelpDesk Priority", "Medium");
                var CategoryId = await _dropDownService.GetDropDownByTypeNameAndName(model.SiteId, "HelpDesk Category", "Other");
                var StatusId = await _dropDownService.GetDropDownByTypeNameAndName(model.SiteId, "HelpDesk Status", "New");

                var entity = new HelpDesk();

                if (string.IsNullOrEmpty(model.CompanyId))
                {
                    if (SiteData != null)
                    {
                        var company = await _companyService.GetCompanyByNameAndSiteId(SiteData.Name, model.SiteId);
                        if (company != null)
                        {
                            var customer = await _companyClientsService.GetCustomerByCompanyId(company.Id);
                            if (customer != null)
                            {
                                entity.CompanyId = customer.Id;
                            }
                        }
                    }
                }
                else
                {
                    entity.CompanyId = model.CompanyId;
                }

                entity.Id = Guid.NewGuid().ToString();
                entity.SiteId = model.SiteId;
                entity.TopicId = TopicId;
                entity.QuestionId = QuestionId;
                entity.Title = model.Subject;
                entity.RequesterEmail = model.From;
                entity.TwilioEmailId = twilioEmailId;
                entity.TicketNo = await _helpDeskService.GetLastTicketId(model.SiteId);
                entity.PriorityId = PriorityId;
                entity.CategoryId = CategoryId;

                if (!string.IsNullOrEmpty(model.Body))
                {
                    entity.Description = await _azureBlobImageServices
                        .ProcessHtmlAndManageImagesAsync(
                            model.Body,
                            SiteData.Name,
                            "helpdesk",
                            entity.Id,
                            entity.Description
                        );
                }

                // Set the created by and created on properties
                entity.CreatedById = SiteData.CreatedById;
                entity.UpdatedById = SiteData.UpdatedById;
                entity.CreatedOnUtc = GetDateTime;
                entity.UpdatedOnUtc = GetDateTime;
                _helpDeskService.InsertHelpDesk(entity);

                _helpDeskService.AddHelpDeskStatusLog(entity.Id, StatusId, SiteData.CreatedById, GetDateTime);

                #region Save email repy
                // Save email repy
                var EmailReplies = new EmailReplies
                {
                    Id = Guid.NewGuid().ToString(),
                    ToEmail = model.To,
                    FromEmail = model.From,
                    TwilioEmailId = twilioEmailId,
                    Subject = model.Subject,
                    Body = entity.Description,
                    IsSystemEmail = false,
                    CreatedOnUtc = GetDateTime,
                    UpdatedOnUtc = GetDateTime,
                    Deleted = false,
                };

                // Insert the twilio replie record
                _emailRepliesServices.InsertEmailReplies(EmailReplies);

                var emailRepliesMapping = new HelpDeskEmailRepliesMapping
                {
                    Id = Guid.NewGuid().ToString(),
                    HelpDeskId = entity.Id,
                    EmailRepliesId = EmailReplies.Id
                };

                _helpDeskEmailRepliesMappingService.InsertHelpDeskEmailRepliesMapping(emailRepliesMapping);

                #endregion

                #region Send Emails to admin/Support team and requester

                var helpDesk = await _helpDeskService.GetHelpDeskDetailsById(entity.Id);
                if (helpDesk == null)
                    return BadRequest("Help Desk not found.");

                string requesterName = null;
                // check email send or not 
                bool canSendRequesterMail = true;
                string requesterUserId = null;

                if (!string.IsNullOrEmpty(model.From))
                {
                    var person = await _personService.GetPersonByEmail(model.From, null, model.SiteId);
                    if (person?.PrimaryEmailAddress != null)
                    {
                        requesterName = person?.FirstName + " " + person?.LastName;

                        //var employee = await _employeeService.GetEmployeeByPersonId(person.Id);
                        var employee = await _employeeService.GetEmployeeByPersonIdBySiteId(person.Id, model.SiteId);
                        if (employee != null)
                        {
                            requesterUserId = _commonService.GetLoggeduserIdByEmployeeId(model.SiteId, employee.Id);
                            if (!string.IsNullOrEmpty(requesterUserId))
                            {
                                var canSendEmail = await _sitesEmailNotificationsPermissionServices
                                    .ShouldSendNotification(
                                    model.SiteId,
                                    requesterUserId,
                                    "HelpDesk.RequestReceived"
                                    );
                            }
                        }
                    }
                }

                // REQUESTER EMAIL
                // If allowed OR external user → send mail
                if (canSendRequesterMail)
                {
                    await _workflowMessageService.SendRequestReceivedMailToRequester(
                        null,
                        helpDesk,
                        twilioEmailId,
                        model.From,
                        true,
                        prefixNo,
                        requesterName,
                        GetDateTime
                    );
                }

                var badderSiteData = await _siteService.GetBySiteName("Bader Rutter");
                if (model.SiteId != badderSiteData.Id)
                {
                    // send meail to admin/support
                    var supports = await _userService.GetUsersByRole(model.SiteId, "support team");
                    var supportEmails = new List<string>();
                    Employee primaryEmployee = null;

                    foreach (var user in supports)
                    {
                        // Permission Check
                        var canSend = await _sitesEmailNotificationsPermissionServices
                            .ShouldSendNotification(
                                model.SiteId,
                                user.Id,
                                "HelpDesk.HelpDeskMailSendToAdmin"
                            );

                        if (!canSend)
                            continue;

                        //var employeeId = _commonService.GetEmployeeIdByUserId(model.SiteId, user.Id);

                        var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(model.SiteId, user.Id);
                        var employeeData = await _employeeService.GetEmployeeDetailsById(employeeId);

                        if (!string.IsNullOrEmpty(employeeData?.Person?.PrimaryEmailAddress))
                        {
                            supportEmails.Add(employeeData.Person.PrimaryEmailAddress);

                            // keep first employee as owner reference
                            if (primaryEmployee == null)
                                primaryEmployee = employeeData;
                        }
                    }

                    if (supportEmails.Any())
                    {
                        await _workflowMessageService.SendHelpDeskMailToAdmin(
                            primaryEmployee,
                            helpDesk,
                            true,
                            twilioEmailId,
                            prefixNo,
                            supportEmails.Distinct().ToList(),
                            requesterName,
                            model.From,
                            GetDateTime
                        );
                    }
                }
                #endregion

                return Ok(new { status = "Ticket Created" });
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex.Message + ":- " + Ex.InnerException);
            }
        }

        [AllowAnonymous]
        [HttpGet("getSiteIdFromTicketGenerationEmail")]
        public async Task<IActionResult> createTicketFromEmailAsync([FromQuery] string Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
                return BadRequest("Email is required");

            string SiteId = await _siteService.GetSiteIdFromTicketGenerationEmail(Email);

            if (!string.IsNullOrEmpty(SiteId))
                return Ok(SiteId);

            return BadRequest("SiteId Not Found");
        }
        #endregion

        #region DeleteHelpDesk
        // Title: DeleteHelpDeskById
        // Description: This endpoint deletes a HelpDesk based on the provided HelpDesk ID. It first retrieves the HelpDesk entity by ID, checks if it exists, and if so, deletes the HelpDesk. If the HelpDesk is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHelpDesk(string id)
        {
            try
            {
                // Fetch the HelpDesk entity by its ID
                var entity = await _helpDeskService.GetHelpDeskById(id);

                // If no HelpDesk is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Help Desk found with the specified id."));

                // Delete the HelpDesk using the HelpDesk service
                _helpDeskService.DeleteHelpDesk(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Private methods
        private string GetUniqueFileName(string uploadDir, string fileName)
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string finalName = fileName;
            int counter = 1;

            while (System.IO.File.Exists(Path.Combine(uploadDir, finalName)))
            {
                if (counter == 1)
                    finalName = $"{fileNameWithoutExt} - copy{extension}";
                else
                    finalName = $"{fileNameWithoutExt} - copy({counter}){extension}";
                counter++;
            }

            return finalName;
        }
        private async Task<string> FilterRawHTMLBody(string html, string text, IFormFileCollection files, string attachmentInfoJson, string contentIdsJson)
        {
            // 1. If HTML missing, build it from text
            if (string.IsNullOrWhiteSpace(html))
            {
                html = $"<p>{WebUtility.HtmlEncode(text).Replace("\n", "<br/>")}</p>";
            }

            // 2. Deserialize metadata
            var attachmentInfo = JsonConvert.DeserializeObject<Dictionary<string, TwilioAttachmentInfo>>(attachmentInfoJson ?? "{}");
            var contentIds = JsonConvert.DeserializeObject<Dictionary<string, string>>(contentIdsJson ?? "{}");

            if (attachmentInfo.Count == 0 || contentIds.Count == 0)
                return html;

            // 3. Build lookup: attachmentKey → file
            var fileLookup = files.ToDictionary(f => f.Name, f => f);

            // 4. Parse HTML
            var doc = new HtmlAgilityPack.HtmlDocument { OptionFixNestedTags = true };
            doc.LoadHtml(html);

            // 5. Parse HTML
            var imgNodes = doc.DocumentNode.SelectNodes("//img[@src]");
            if (imgNodes == null)
                return html;

            // 6. Replace CID images
            foreach (var img in imgNodes)
            {
                var src = img.GetAttributeValue("src", string.Empty);
                if (!src.StartsWith("cid:", StringComparison.OrdinalIgnoreCase))
                    continue;

                var cid = src.Substring(4).Trim();

                // CID → attachment key
                if (!contentIds.TryGetValue(cid, out var attachmentKey))
                    continue;

                // attachment key → metadata
                if (!attachmentInfo.TryGetValue(attachmentKey, out var info))
                    continue;

                // attachment key → file
                if (!fileLookup.TryGetValue(attachmentKey, out var file))
                    continue;

                var base64 = await ConvertToBase64Async(file);
                var dataUri = $"data:{info.type};base64,{base64}";

                img.SetAttributeValue("src", dataUri);
            }

            // 7. Remove quoted blocks
            var blockquotes = doc.DocumentNode.SelectNodes("//blockquote");
            if (blockquotes != null)
                foreach (var bq in blockquotes)
                    bq.Remove();

            // 8. Remove signatures
            var signatures = doc.DocumentNode.SelectNodes("//*[contains(@class,'signature')]");
            if (signatures != null)
                foreach (var sig in signatures)
                    sig.Remove();

            // 9. Remove Gmail quoted content
            var gmailQuote = doc.DocumentNode.SelectNodes("//blockquote[contains(@class,'gmail_quote')]");
            if (gmailQuote != null)
                foreach (var gQ in gmailQuote)
                    gQ.ParentNode.Remove();

            // 🔴 REMOVE GMAIL QUOTED CONTENT
            var gmailQuotes = doc.DocumentNode.SelectNodes("//div[contains(@class,'gmail_quote')]");
            if (gmailQuotes != null)
                foreach (var quote in gmailQuotes)
                    quote.Remove();

            // Optional: remove gmail attribution line ("On Tue... wrote:")
            var gmailAttrs = doc.DocumentNode.SelectNodes("//div[contains(@class,'gmail_attr')]");
            if (gmailAttrs != null)
                foreach (var attr in gmailAttrs)
                    attr.Remove();

            // 10. Remove hr-separated content
            var hrs = doc.DocumentNode.SelectNodes("//hr");
            if (hrs != null)
                foreach (var hr in hrs)
                    hr.ParentNode.Remove();

            // 11. Sanitize
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Add("img");
            sanitizer.AllowedAttributes.Add("src");
            sanitizer.AllowedAttributes.Add("style");
            sanitizer.AllowedAttributes.Add("data-outlook-trace");
            sanitizer.AllowedSchemes.Add("data");

            // 12. Return HTML
            return sanitizer.Sanitize(doc.DocumentNode.OuterHtml);
        }
        private static async Task<string> ConvertToBase64Async(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return Convert.ToBase64String(ms.ToArray());
        }
        private async Task<IActionResult> SaveEmailReply(string to, string from, string subject, string body)
        {
            var GetDateTime = _siteService.GetDateTime();
            string Body = ExtractReplyBody(body);

            // multiple TO Emails
            string cleanedTo = null;
            if (!string.IsNullOrWhiteSpace(to))
            {
                var toEmails = to
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Select(email =>
                    {
                        // clean individual email only if needed
                        return email.Contains('+')
                            ? CleanEmailAddress(email)
                            : email;
                    })
                    .Distinct()
                    .ToList();

                cleanedTo = string.Join(", ", toEmails);
            }

            // extract Twilio token
            string token = ExtractIdFromSender(to);
            if (string.IsNullOrWhiteSpace(token))
                return StatusCode(500, new { error = "TwilioEmailId is required." });

            var helpDesk = await _helpDeskService.GetTwilioEmailReplies(token);
            if (helpDesk == null)
                return BadRequest("Help Desk not found.");

            var entity = new EmailReplies();
            entity.Id = Guid.NewGuid().ToString("N");

            entity.TwilioEmailId = token;
            entity.FromEmail = from;
            entity.ToEmail = cleanedTo;
            entity.Subject = subject;
            entity.IsSystemEmail = false;
            entity.CreatedOnUtc = GetDateTime;
            entity.UpdatedOnUtc = GetDateTime;
            entity.Deleted = false;

            if (!string.IsNullOrEmpty(Body))
            {
                var SiteData = await _siteService.GetById(helpDesk.SiteId);
                entity.Body = await _azureBlobImageServices.ProcessHtmlAndManageImagesAsync(Body, SiteData.Name, "helpdesk", entity.Id, entity.Body);
            }

            _emailRepliesServices.InsertEmailReplies(entity);

            var emailRepliesMapping = new HelpDeskEmailRepliesMapping
            {
                Id = Guid.NewGuid().ToString(),
                HelpDeskId = helpDesk.Id,
                EmailRepliesId = entity.Id
            };

            _helpDeskEmailRepliesMappingService.InsertHelpDeskEmailRepliesMapping(emailRepliesMapping);

            #region Role Detection (MULTI-ROLE SAFE)

            var adminUsers = await _userService.GetUsersByRole(helpDesk.SiteId, "admin");
            var supportUsers = await _userService.GetUsersByRole(helpDesk.SiteId, "support team");
            var vskySite = await _siteService.GetBySiteName("Vsky Solutions");
            var systemAdmin = (await _userService
                .GetUsersByRole(vskySite.Id, "system super admin"))
                .FirstOrDefault();

            if (systemAdmin != null)
            {
                bool isAdmin = adminUsers.Any(x => x.Email == from || x.Person.PrimaryEmailAddress == from);
                bool isSupportTeam = supportUsers.Any(x => x.Email == from || x.Person.PrimaryEmailAddress == from);

                #region status logic
                var StatusText = helpDesk?.HelpDeskStatusLog?.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status?.DropDownValue).FirstOrDefault();

                if (!isAdmin && !isSupportTeam && StatusText == "Awaiting Client")
                {
                    var statusId = _commonService.GetDrownValueIdByTypeandValue(helpDesk.SiteId, "HelpDesk Status", "In Progress");
                    if (statusId == null)
                        return BadRequest("Help desk status not found");

                    _helpDeskService.AddHelpDeskStatusLog(helpDesk.Id, statusId, systemAdmin.Id, GetDateTime);

                    await SendHelpDeskStatusUpdateMail(helpDesk.Id, helpDesk.SiteId, "", GetDateTime);
                }
                #endregion
            }

            #endregion

            return StatusCode(200, new { success = "Success" });
        }
        private static string ExtractIdFromSender(string to)
        {
            var m = Regex.Match(to, @"(?<addr>[\w\.+-]+@[\w\.-]+\.\w+)");
            string addr = m.Success ? m.Groups["addr"].Value : to;

            string token = null;
            if (!string.IsNullOrEmpty(addr))
            {
                // extract token
                var atIndex = addr.IndexOf('@');
                if (atIndex > 0)
                {
                    var localPart = addr.Substring(0, atIndex);  // everything before @
                    var plusIndex = localPart.LastIndexOf('+');  // last + in local part
                    if (plusIndex > 0)
                    {
                        token = localPart.Substring(plusIndex + 1);  // everything after the last +
                    }
                }
            }
            return token;
        }
        private static string CleanEmailAddress(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return email;

            var atIndex = email.IndexOf('@');
            if (atIndex < 0)
                return email;

            var localPart = email.Substring(0, atIndex);
            var domainPart = email.Substring(atIndex);

            var plusIndex = localPart.IndexOf('+');
            if (plusIndex > -1)
            {
                localPart = localPart.Substring(0, plusIndex);
            }

            return localPart + domainPart;
        }
        private static string ExtractReplyBody(string fullBody)
        {
            if (string.IsNullOrWhiteSpace(fullBody))
                return fullBody;

            var lines = fullBody.Split('\n').Select(l => l.TrimEnd('\r')).ToList();
            List<string> replyLines = new();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    replyLines.Add(string.Empty);
                    continue;
                }

                // 🔴 Gmail reply boundary (MOST IMPORTANT)
                if (trimmed.StartsWith("On ", StringComparison.OrdinalIgnoreCase)
                    && trimmed.Contains(" wrote:"))
                    break;

                // Stop at start of quoted previous emails
                if (line.StartsWith("From: ", StringComparison.OrdinalIgnoreCase)) break;
                if (line.StartsWith("Sent:", StringComparison.OrdinalIgnoreCase)) break;
                if (line.StartsWith("To: ", StringComparison.OrdinalIgnoreCase)) break;
                if (line.StartsWith("Subject:", StringComparison.OrdinalIgnoreCase)) break;
                if (line.StartsWith(">")) break;  // Quoted text
                if (line.Contains("Click here to Unsubscribe")) break;
                if (line.Contains("The content of this message is confidential")) break;

                // Quoted text
                if (trimmed.StartsWith(">")) break;

                // Legal footer
                if (trimmed.Contains("The content of this message is confidential",
                    StringComparison.OrdinalIgnoreCase))
                    break;

                replyLines.Add(line);
            }

            // Remove trailing signature lines
            var cleaned = string.Join("\n", replyLines).Trim();

            // Optional: remove common signature indicators (use LAST occurrence)
            string[] signatureMarkers = { "Thanks", "Regards", "Best", "Sincerely" };
            int lastSignatureIndex = -1;

            foreach (var mark in signatureMarkers)
            {
                int idx = cleaned.LastIndexOf(mark, StringComparison.OrdinalIgnoreCase);
                if (idx > lastSignatureIndex)
                    lastSignatureIndex = idx;
            }

            if (lastSignatureIndex > 0)
                cleaned = cleaned.Substring(0, lastSignatureIndex).Trim();

            return cleaned;
        }       
        private async Task<bool> UpdateHelpDeskDetails(string helpDeskId, object data, string flag)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            // Fetch the issue entity by its ID
            var entity = await _helpDeskService.GetHelpDeskById(helpDeskId);
            if (entity == null)
                return false;

            switch (flag.ToLower())
            {
                case "assignedTo":
                    if (data is string statusId)
                        entity.StatusId = statusId;
                    break;

                case "priority":
                    if (data is string priorityId)
                        entity.PriorityId = priorityId;
                    break;

                case "company":
                    if (data is string companyId)
                        entity.CompanyId = companyId;
                    break;

                default:
                    return false;
            }

            entity.UpdatedById = LoggedUserId;
            entity.UpdatedOnUtc = GetDateTime;
            _helpDeskService.UpdateHelpDesk(entity);

            return true;
        }

        private async Task SendHelpDeskStatusUpdateMail(string helpDeskId, string siteId, string assignedToName, DateTime GetDateTime)
        {
            var helpDesk = await _helpDeskService.GetHelpDeskDetailsById(helpDeskId);

            // get site name and TicketNoPrefix
            var siteDetails = await _siteService.GetSiteTicketNoPrefixById(siteId);
            string prefixNo = siteDetails?.TicketNoPrefix ?? "HR";

            bool hasAssignedTo = !string.IsNullOrEmpty(helpDesk.AssignedToId);

            #region Requester mail
            if (!string.IsNullOrEmpty(helpDesk.RequesterId))
            {
                var requester = await _employeeService.GetEmployeeDetailsById(helpDesk.RequesterId);

                if (requester?.Person?.PrimaryEmailAddress != null)
                {
                    //var requesterUser = await _userService.GetUserByEmployeeId(siteId, requester.Id);
                    var requesterUser = await _userService.GetUserByEmployeeId(siteId, requester.Id);
                    bool canSend = true;

                    if(requesterUser != null)
                    {
                        canSend = await _sitesEmailNotificationsPermissionServices
                            .ShouldSendNotification(
                                siteId,
                                requesterUser.Id,
                                "HelpDesk.HelpDeskStatusMail"
                            );
                    }

                    if(canSend)
                    {
                        await _workflowMessageService.SendHelpDeskStatusMail(
                            requester,
                            helpDesk,
                            null,
                            helpDesk.TwilioEmailId,
                            prefixNo,
                            siteDetails.Name,
                            null,
                            assignedToName,
                            GetDateTime
                        );
                    }
                }
            }
            // New requester (Email only)
            else if (!string.IsNullOrEmpty(helpDesk.RequesterEmail))
            {
                await _workflowMessageService.SendHelpDeskStatusMail(
                    null,    // no employee
                    helpDesk,
                    null,
                    helpDesk.TwilioEmailId,
                    prefixNo,
                    siteDetails.Name,
                    helpDesk.RequesterEmail,
                    assignedToName,
                    GetDateTime
                );
            }
            #endregion

            #region AssignedTo email
            // If AssignedTo exists → mail ONLY assigned person
            if (hasAssignedTo)
            {
                var assignedEmp = await _employeeService.GetEmployeeDetailsById(helpDesk.AssignedToId);

                if (assignedEmp != null)
                {
                    var assignedUser = await _userService.GetUserByEmployeeId(siteId, assignedEmp.Id);

                    bool canSend = true;

                    if (assignedUser != null)
                    {
                        canSend = await _sitesEmailNotificationsPermissionServices
                            .ShouldSendNotification(
                                siteId,
                                assignedUser.Id,
                                "HelpDesk.HelpDeskStatusMail"
                            );
                    }

                    if (canSend)
                    {
                        await _workflowMessageService.SendHelpDeskStatusMail(
                            assignedEmp,
                            helpDesk,
                            null,
                            helpDesk.TwilioEmailId,
                            prefixNo,
                            siteDetails.Name,
                            null,
                            assignedToName,
                            GetDateTime
                        );
                    }
                }

                return; // STOP here (NO support team mail)
            }
            #endregion

            #region Support Team (ONLY if not assigned)

            // Collect email recipients            
            var supportEmails = new List<string>();
            Employee primaryEmployee = null;

            var supportUsers = await _userService.GetUsersByRole(siteId, "support team");

            foreach (var user in supportUsers)
            {
                // Permission check per support user
                var canSend = await _sitesEmailNotificationsPermissionServices
                    .ShouldSendNotification(
                        siteId,
                        user.Id,
                        "HelpDesk.HelpDeskStatusMail"
                    );

                if (!canSend)
                    continue;

                //var empId = _commonService.GetEmployeeIdByUserId(siteId, user.Id);

                var empId = _commonService.GetEmployeeIdByUserIdAndEmail(siteId, user.Id);
                var employee = await _employeeService.GetEmployeeDetailsById(empId);

                if (!string.IsNullOrEmpty(employee?.Person?.PrimaryEmailAddress))
                {
                    supportEmails.Add(employee.Person.PrimaryEmailAddress);
                    primaryEmployee = employee;
                }
            }

            if (supportEmails.Any())
            {
                await _workflowMessageService.SendHelpDeskStatusMail(
                    primaryEmployee,
                    helpDesk,
                    supportEmails.Distinct().ToList(), // comma separated
                    helpDesk.TwilioEmailId,
                    prefixNo,
                    siteDetails.Name,
                    null,
                    assignedToName,
                    GetDateTime
                );
            }
            #endregion
        }

        // Get Name by email
        private async Task<string> GetNameByEmail(string siteId, string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            if (email == "noreply-smtp@vskysolutions.com")
                return "System Generated";

            var employee = await _employeeService.GetEmployeeByEmail(siteId, email);
            return employee?.Person?.FullName;
        }

        // get site TicketNOPrefix
        private async Task<Site> GetSiteTicketInfoAsync(string siteId)
        {
            return await _siteService.GetSiteTicketNoPrefixById(siteId);
        }
        private string FormatDuration(int totalMinutes)
        {
            var time = TimeSpan.FromMinutes(totalMinutes);
            var parts = new List<string>();

            if (time.Days > 0)
                parts.Add($"{time.Days} day{(time.Days > 1 ? "s" : "")}");

            if (time.Hours > 0)
                parts.Add($"{time.Hours} hour{(time.Hours > 1 ? "s" : "")}");

            if (time.Minutes > 0)
                parts.Add($"{time.Minutes} minute{(time.Minutes > 1 ? "s" : "")}");

            return string.Join(" ", parts);
        }
        #endregion
    }
}