using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.Messages;
using Vsky.Services.Module;
using Vsky.Services.Notifications;
using Vsky.Services.Persons;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.SitesModule;
using Vsky.Services.SitesModulesMenu;
using Vsky.Services.SitesModulesMenusPermission;
using Vsky.Services.SitesRole;
using Vsky.Services.TimeZone;
using Vsky.Services.Users;

namespace Vsky.Api.Controllers
{
    [Route("sites")]
    public class SiteController : BaseController
    {
        #region Define services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICommonService _commonService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IUserService _userService;
        private readonly ISiteService _siteService;
        private readonly IPersonService _personService;
        private readonly ISitesRolesService _sitesRolesService;
        private readonly IModulesService _moduleService;
        private readonly ISitesModulesService _sitesModulesService;
        private readonly IModulesMenusService _menuService;
        private readonly ISitesModulesMenusService _sitesModulesMenusService;
        private readonly ISitesModulesMenusPermissionsService _sitesModulesMenusPermissionsService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly IDropDownService _dropDownService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly IMasterNotificationService _masterNotificationService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly ITimeZoneService _timeZoneServices;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services initialization
        public SiteController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ICommonService commonService,
            IWorkflowMessageService workflowMessageService,
            IUserService userService,
            ISiteService siteService,
            IPersonService PersonService,
            ISitesRolesService sitesRolesService,
            IModulesService moduleService,
            ISitesModulesService sitesModulesService,
            IModulesMenusService menuService,
            ISitesModulesMenusService sitesModulesMenusService,
            ISitesModulesMenusPermissionsService sitesModulesMenusPermissionsService,
            IDropDownTypeService dropDownTypeService,
             IDropDownService dropDownService,
             ISitesModifiedLogsService sitesModifiedLogsService,
             IMasterNotificationService masterNotificationService,
             IAzureBlobImageServices azureBlobImageServices,
            ITimeZoneService timeZoneServices,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _commonService = commonService;
            _workflowMessageService = workflowMessageService;
            _userService = userService;
            _siteService = siteService;
            _personService = PersonService;
            _sitesRolesService = sitesRolesService;
            _moduleService = moduleService;
            _sitesModulesService = sitesModulesService;
            _menuService = menuService;
            _sitesModulesMenusService = sitesModulesMenusService;
            _sitesModulesMenusPermissionsService = sitesModulesMenusPermissionsService;
            _dropDownTypeService = dropDownTypeService;
            _dropDownService = dropDownService;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _masterNotificationService = masterNotificationService;
            _azureBlobImageServices = azureBlobImageServices;
            _timeZoneServices = timeZoneServices;
            _applicationUserRoleService = applicationUserRoleService;
        }
        #endregion

        #region GetAllSites
        [HttpPost("list")]
        public IActionResult GetAllSites(SiteSearchModel searchModel)
        {
            try
            {
                //Get all site list
                var siteList = _siteService.GetAllSites(searchModel.SearchText, searchModel.Name, searchModel.FullName, searchModel.EmailAddress, searchModel.SiteStatus, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                //Append site list to model using mapper
                var model = new SiteListModel
                {
                    Data = _mapper.Map<IList<SiteModel>>(siteList),
                    Total = siteList.TotalCount
                };
                //return site list 
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllTimeZoneForDropdown
        // Title: GetAllTimeZoneForDropdown
        // Description: This endpoint retrieves all timezone list for dropdown.
        [HttpGet("timezone/dropdown/list")]
        public async Task<IActionResult> GetAllTimeZoneListForDropdown()
        {
            try
            {
                var list = await _timeZoneServices.GetAllTimeZoneListForDropdown();
                var model = _mapper.Map<List<TimeZones>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllSitesRoleListForDropdown
        // Title: GetAllSitesRoleListForDropdown
        // Description: This endpoint retrieves the details of a specific roles based on its unique identifier (ID). 
        [HttpGet("dropdown/list/{siteId}")]
        public async Task<IActionResult> GetAllSitesRoleListForDropdown(string siteId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
                var list = await _sitesRolesService.GetRolesBySiteId(SiteId);
                var model = _mapper.Map<List<SitesRolesModels>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllSiteModuleListForDropdown
        // Title: GetAllSiteModuleListForDropdown
        // Description: This endpoint retrieves the list of site modules. 
        [HttpGet("site-module-dropdown/list/{siteId}")]
        public async Task<IActionResult> GetAllSiteModuleListForDropdown(string siteId)
        {
            try
            {
                var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
                var list = await _sitesModulesService.GetAllSiteModuleListForDropdown(SiteId);
                var model = _mapper.Map<List<SitesModulesModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("site-module-menus-dropdown/list")]
        public async Task<IActionResult> GetAllSiteModuleMenuListForDropdown(
            string siteId,
            string moduleIds = null
        )
        {
            try
            {
                var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
                var list = await _sitesModulesMenusService.GetSitesModulesMenusBySiteModuleIdForDropdown(siteId, moduleIds);
                var model = _mapper.Map<List<SitesModulesMenusModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Find site by id
        ///<summary> Find record by id </summary>
        ///<param name="id"> id of the record </param>
        ///<Author> Sachin Bhanse </Author>
        ///<CreatedOn> 07/19/2024 </CreatedOn>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSiteById(string id)
        {
            try
            {
                //Find site by id
                var entity = await _siteService.GetSiteDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No site found with the specified id."));

                //Map to model
                var model = _mapper.Map<SiteModel>(entity);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region SetGlobalSite
        // Title: SetGlobalSite
        // Description: This endpoint update global siteId.
        [HttpGet("GetGlobalSiteData")]
        public async Task<IActionResult> GetGlobalSiteData(string siteId)
        {
            try
            {
                var userId = User.GetLoggedInUserId<string>();
                var SiteData = await _siteService.GetById(siteId);
                var LandingPageLink = await _sitesModulesMenusService.GetLandingPageBySiteId(SiteData.Id);
                var roles = await _applicationUserRoleService.GetNormalizedRoleNamesByUserAndSite(userId, siteId);

                if (SiteData != null)
                    return Ok(new { siteId = SiteData.Id, timeZone = SiteData.TimeZone, Name = SiteData.Name, LandingPage = LandingPageLink, Roles = roles });
                else
                    return BadRequest("No Site Fount");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Create site
        ///<summary> Save record </summary>
        ///<param name="model"> Collection of values </param>
        ///<Author> Sachin Bhanse </Author>
        ///<CreatedOn> 07/19/2024 </CreatedOn>
        [HttpPost]
        public async Task<IActionResult> CreateSite([FromForm] SiteModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    // var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime();

                    //Check duplicate site name
                    var siteExists = await _siteService.GetBySiteName(model.Name);
                    if (siteExists != null)
                        return BadRequest(new BadRequestError("Site name already exists."));

                    var exists = await _userManager.Users.AnyAsync(x => x.UserName == model.UserName && !x.Deleted);
                    if (exists)
                        return BadRequest(new BadRequestError("Username already exists. Please try a different one."));

                    var personExists = _userManager.Users.Where(x => x.PersonId == model.PersonId && !x.Deleted && x.PersonId != null);
                    if (personExists.Count() > 0)
                        return BadRequest(new BadRequestError("Person already exists. Please try a different one."));

                    var entity = _mapper.Map<Site>(model);
                    entity.Id = string.IsNullOrWhiteSpace(model.Id) ? Guid.NewGuid().ToString() : model.Id;

                    // Generate ticket prefix from site name
                    entity.TicketNoPrefix = GenerateTicketPrefix(model.Name);

                    if (model.File != null && model.File.Length > 0)
                    {
                        var allowedFileTypes = new[] {
                                "image/jpeg", "image/png", "application/pdf",
                                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                            };

                        var file = model.File;
                        if (!allowedFileTypes.Contains(file.ContentType))
                        {
                            return BadRequest(new BadRequestError("Invalid file type."));
                        }

                        var fileId = Guid.NewGuid().ToString();
                        var originalFileName = Path.GetFileName(file.FileName);
                        var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                        var files = new List<IFormFile> { model.File };
                        var urls = await _azureBlobImageServices.UploadFilesAsync(model.Name, "sites", files, entity.Id);

                        if (urls != null && urls.Any())
                        {
                            foreach (var blobUrl in urls)
                            {
                                var picture = new Picture
                                {
                                    SiteId = entity.Id,
                                    Type = "Sites",
                                    SeoFilename = originalFileName,
                                    MimeType = mimeType,
                                    VirtualPath = blobUrl,
                                    ModuleId = entity.Id,
                                    Module = model.Name,
                                    SubModuleId = entity.Id,
                                    Sub_Module = model.Name,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };

                                _commonService.InsertPicture(picture);

                                entity.SiteLogoId = picture.Id;
                            }
                        }
                    }
                    else
                    {
                        entity.SiteLogoId = null;
                    }

                    if (model.FileIcon != null && model.FileIcon.Length > 0)
                    {
                        var allowedFileTypes = new[] {
                                "image/jpeg", "image/png", "application/pdf",
                                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                            };

                        var file = model.FileIcon;
                        if (!allowedFileTypes.Contains(file.ContentType))
                        {
                            return BadRequest(new BadRequestError("Invalid file type."));
                        }
                        var fileIconId = Guid.NewGuid().ToString();
                        var originalFileName = Path.GetFileName(file.FileName);
                        var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                        var files = new List<IFormFile> { model.File };
                        var urls = await _azureBlobImageServices.UploadFilesAsync(model.Name, "sites", files, entity.Id);

                        if (urls != null && urls.Any())
                        {
                            foreach (var blobUrl in urls)
                            {
                                var picture = new Picture
                                {
                                    SiteId = entity.Id,
                                    Type = "Sites",
                                    SeoFilename = originalFileName,
                                    MimeType = mimeType,
                                    VirtualPath = blobUrl,
                                    ModuleId = entity.Id,
                                    Module = model.Name,
                                    SubModuleId = entity.Id,
                                    Sub_Module = model.Name,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };

                                _commonService.InsertPicture(picture);

                                entity.SiteFaviconId = picture.Id;
                            }
                        }
                    }
                    else
                    {
                        entity.SiteFaviconId = null;
                    }

                    //Add Address
                    string AddressId = _commonService.AddUpdateAddress(entity.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, LoggedUserId);
                    entity.AddressId = AddressId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _siteService.InsertSite(entity);

                    // Create application user
                    var user = new ApplicationUser();
                    user.Id = Guid.NewGuid().ToString();
                    user.PersonId = model.PersonId;
                    user.Email = model.PrimaryEmailAddress;
                    user.PhoneNumber = model.PrimaryPhoneNumber;
                    user.UserName = model.UserName;
                    user.EmailConfirmed = true;
                    user.PhoneNumberConfirmed = true;
                    user.TwoFactorEnabled = false;
                    user.LockoutEnabled = false;
                    user.CreatedOnUtc = GetDateTime;
                    user.UpdatedById = LoggedUserId;
                    user.UpdatedOnUtc = GetDateTime;
                    user.Active = true;

                    string password;
                    if (string.IsNullOrWhiteSpace(model.Password))
                    {
                        // Generate a password
                        password = _userService.GeneratePassword();
                    }
                    else
                    {
                        // Use the provided password
                        password = model.Password;
                    }

                    var result = await _userManager.CreateAsync(user, password);

                    if (result.Succeeded)
                    {
                        //string roleName = "site-super-admin";
                        ////Assign role to user
                        //await _userManager.AddToRoleAsync(user, roleName);

                        var role = await _roleManager.FindByNameAsync("site-super-admin");
                        if (role != null)
                        {
                            await _applicationUserRoleService.AddUserRoleAsync(
                                user.Id,
                                role.Id,
                                entity.Id);
                        }

                        await _db.SaveChangesAsync();

                        if (model.SendEmail)
                            await _workflowMessageService.SendWelcomeEmail(user, password);
                    }

                    var personSiteMapping = new PersonSitesMapping();
                    personSiteMapping.PersonId = model.PersonId;
                    personSiteMapping.SiteId = entity.Id;
                    personSiteMapping.CreatedById = LoggedUserId;
                    personSiteMapping.UpdatedById = LoggedUserId;
                    personSiteMapping.CreatedOnUtc = GetDateTime;
                    personSiteMapping.UpdatedOnUtc = GetDateTime;
                    _personService.InsertPersonSites(personSiteMapping);

                    if (model.RoleIds != null && model.RoleIds.Count() > 0)
                    {
                        foreach (var item in model.RoleIds)
                        {
                            if (item != null)
                            {
                                var sitesRoles = new SitesRoles();
                                {
                                    sitesRoles.SiteId = entity.Id;
                                    sitesRoles.RoleId = item;
                                    sitesRoles.CreatedOnUtc = GetDateTime;
                                    sitesRoles.CreatedById = LoggedUserId;

                                };
                                _sitesRolesService.InsertSitesRoles(sitesRoles);
                            }
                        }
                    }
                    var modules = await _moduleService.GetAllModulesList();
                    var menus = await _menuService.GetAllMenusList();
                    var siteRoles = await _sitesRolesService.GetRolesBySiteId(entity.Id);
                    foreach (var module in modules)
                    {
                        var sitesModule = new SitesModules
                        {
                            SiteId = entity.Id,
                            ModuleId = module.Id,
                            SortOrder = module.Sortorder,
                            CreatedById = LoggedUserId,
                            UpdatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime,
                            UpdatedOnUtc = GetDateTime
                        };
                        _sitesModulesService.InsertSiteModule(sitesModule);

                        var menuList = menus.Where(x => x.ModuleId == module.Id).ToList();
                        foreach (var menu in menuList)
                        {
                            var sitesModulesMenus = new SitesModulesMenus
                            {

                                SiteId = entity.Id,
                                SiteModuleId = sitesModule.Id,
                                MenuId = menu.Id,
                                SortOrder = menu.Sortorder,
                                CreatedById = LoggedUserId,
                                UpdatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime,
                                UpdatedOnUtc = GetDateTime
                            };
                            _sitesModulesMenusService.InsertSitesModulesMenu(sitesModulesMenus);

                            foreach (var role in siteRoles)
                            {
                                var isSiteSuperAdmin = role.ApplicationRole?.Name == "Site Super Admin";
                                var permission = new SitesModulesMenusPermissions
                                {
                                    SiteId = entity.Id,
                                    SiteRoleId = role.Id,
                                    SiteModuleMenuId = sitesModulesMenus.Id,
                                    IsShowMenu = isSiteSuperAdmin,
                                    IsManage = false,
                                    IsView = false,
                                    CreatedById = LoggedUserId,
                                    UpdatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime,
                                    UpdatedOnUtc = GetDateTime
                                };
                                _sitesModulesMenusPermissionsService.InsertSiteModuleMenusPermission(permission);
                            }
                        }

                    }
                    return Ok(password);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Update site
        ///<summary> Update record </summary>
        ///<param name="id"> id of the record </param>
        ///<param name="model"> Collection of values </param>
        ///<Author> Sachin Bhanse </Author>
        ///<CreatedOn> 07/19/2024 </CreatedOn>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSite(string id, [FromForm] SiteModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    // var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(model.Id);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _siteService.GetBySiteName(model.Name, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Site name already exists."));
                    // Fetch the site entity by its ID
                    var entity = await _siteService.GetById(id);

                    // If no site is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No site with the specified id."));

                    if (model.FileChangeFlag == "edit")
                    {
                        // Delete old image first
                        if (!string.IsNullOrEmpty(entity.SiteLogoId))
                        {
                            var oldPicture = await _commonService.GetByPictureId(entity.SiteLogoId);

                            if (oldPicture != null && !string.IsNullOrEmpty(oldPicture.VirtualPath))
                            {
                                await _azureBlobImageServices.DeleteImage(oldPicture.VirtualPath);

                                _commonService.DeletePicture(oldPicture);
                            }
                        }

                        if (model.File != null && model.File.Length > 0)
                        {
                            var allowedFileTypes = new[] {
                            "image/jpeg", "image/png", "application/pdf",
                            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                        };

                            var file = model.File;
                            if (!allowedFileTypes.Contains(file.ContentType))
                            {
                                return BadRequest(new BadRequestError("Invalid file type."));
                            }

                            var fileId = Guid.NewGuid().ToString();
                            var originalFileName = Path.GetFileName(file.FileName);
                            var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                            int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(entity.Id, "Sites");
                            var files = new List<IFormFile> { model.File };
                            var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "sites", files, entity.Id, existingImagesCount);

                            if (urls != null && urls.Any())
                            {
                                foreach (var blobUrl in urls)
                                {
                                    var picture = new Picture
                                    {
                                        SiteId = entity.Id,
                                        Type = "Sites",
                                        SeoFilename = originalFileName,
                                        MimeType = mimeType,
                                        VirtualPath = blobUrl,
                                        ModuleId = entity.Id,
                                        Module = model.Name,
                                        SubModuleId = entity.Id,
                                        Sub_Module = model.Name,
                                        CreatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime
                                    };

                                    _commonService.InsertPicture(picture);

                                    entity.SiteLogoId = picture.Id;
                                }
                            }
                        }
                    }
                    else if (model.FileChangeFlag == "remove")
                    {
                        // Delete old image first
                        if (!string.IsNullOrEmpty(entity.SiteLogoId))
                        {
                            var oldPicture = await _commonService.GetByPictureId(entity.SiteLogoId);

                            if (oldPicture != null && !string.IsNullOrEmpty(oldPicture.VirtualPath))
                            {
                                await _azureBlobImageServices.DeleteImage(oldPicture.VirtualPath);

                                _commonService.DeletePicture(oldPicture);
                            }
                        }
                        entity.SiteLogoId = null;
                    }

                    if (model.FileIconChangeFlag == "edit")
                    {
                        // Delete old image first
                        if (!string.IsNullOrEmpty(entity.SiteFaviconId))
                        {
                            var oldPicture = await _commonService.GetByPictureId(entity.SiteFaviconId);

                            if (oldPicture != null && !string.IsNullOrEmpty(oldPicture.VirtualPath))
                            {
                                await _azureBlobImageServices.DeleteImage(oldPicture.VirtualPath);

                                _commonService.DeletePicture(oldPicture);
                            }
                        }

                        if (model.FileIcon != null && model.FileIcon.Length > 0)
                        {
                            var allowedFileTypes = new[] {
                            "image/jpeg", "image/png", "application/pdf",
                            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                        };

                            var file = model.FileIcon;
                            if (!allowedFileTypes.Contains(file.ContentType))
                            {
                                return BadRequest(new BadRequestError("Invalid file type."));
                            }

                            var fileIconId = Guid.NewGuid().ToString();
                            var originalFileName = Path.GetFileName(file.FileName);
                            var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                            int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(entity.Id, "Sites");
                            var files = new List<IFormFile> { model.File };
                            var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "sites", files, entity.Id, existingImagesCount);

                            if (urls != null && urls.Any())
                            {
                                foreach (var blobUrl in urls)
                                {
                                    var picture = new Picture
                                    {
                                        SiteId = entity.Id,
                                        Type = "Sites",
                                        SeoFilename = originalFileName,
                                        MimeType = mimeType,
                                        VirtualPath = blobUrl,
                                        ModuleId = entity.Id,
                                        Module = model.Name,
                                        SubModuleId = entity.Id,
                                        Sub_Module = model.Name,
                                        CreatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime
                                    };

                                    _commonService.InsertPicture(picture);

                                    entity.SiteFaviconId = picture.Id;
                                }
                            }
                        }

                    }
                    else if (model.FileIconChangeFlag == "remove")
                    {
                        // Delete old image first
                        if (!string.IsNullOrEmpty(entity.SiteFaviconId))
                        {
                            var oldPicture = await _commonService.GetByPictureId(entity.SiteFaviconId);

                            if (oldPicture != null && !string.IsNullOrEmpty(oldPicture.VirtualPath))
                            {
                                await _azureBlobImageServices.DeleteImage(oldPicture.VirtualPath);

                                _commonService.DeletePicture(oldPicture);
                            }
                        }

                        entity.SiteFaviconId = null;
                    }

                    string AddressId = _commonService.AddUpdateAddress(entity.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, LoggedUserId);

                    entity.Name = model.Name;
                    entity.UserName = model.UserName;
                    entity.PersonId = model.PersonId;
                    entity.AddressId = AddressId;
                    entity.Active = model.Active;
                    entity.TimeZone = model.TimeZone;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _siteService.UpdateSite(entity);

                    var existingMappings = await _sitesRolesService.GetRolesBySiteId(entity.Id);
                    if (model.RoleIds != null && model.RoleIds.Count() > 0)
                    {
                        foreach (var item in model.RoleIds)
                        {
                            if (item != null)
                            {
                                var existingMapping = existingMappings.FirstOrDefault(x => x.RoleId == item);
                                if (existingMapping != null)
                                {
                                    existingMapping.RoleId = item;
                                    _sitesRolesService.UpdateSitesRoles(existingMapping);
                                }
                                else
                                {
                                    var sitesRoles = new SitesRoles
                                    {
                                        SiteId = entity.Id,
                                        RoleId = item,
                                        CreatedOnUtc = GetDateTime,
                                        CreatedById = LoggedUserId,

                                    };
                                    _sitesRolesService.InsertSitesRoles(sitesRoles);
                                    var siteModuleMenus = await _sitesModulesMenusService.GetSitesModulesMenusBySiteId(entity.Id);
                                    foreach (var siteModuleMenu in siteModuleMenus)
                                    {
                                        var permission = new SitesModulesMenusPermissions
                                        {
                                            SiteId = entity.Id,
                                            SiteRoleId = sitesRoles.Id,
                                            SiteModuleMenuId = siteModuleMenu.Id,
                                            IsShowMenu = false,
                                            IsManage = false,
                                            IsView = false,
                                            CreatedById = LoggedUserId,
                                            UpdatedById = LoggedUserId,
                                            CreatedOnUtc = GetDateTime,
                                            UpdatedOnUtc = GetDateTime
                                        };

                                        _sitesModulesMenusPermissionsService.InsertSiteModuleMenusPermission(permission);
                                    }
                                }
                            }
                            foreach (var existingMapping in existingMappings)
                            {
                                var isSiteSuperAdmin = existingMapping.ApplicationRole.Name == "Site Super Admin";
                                if (isSiteSuperAdmin)
                                    continue;
                                if (!model.RoleIds.Contains(existingMapping.RoleId))
                                {
                                    var permissions = await _sitesModulesMenusPermissionsService.GetMenusBySiteIdAndRoleId(entity.Id, existingMapping.Id);
                                    foreach (var permission in permissions)
                                    {
                                        var sitesModulesMenusPermission = await _sitesModulesMenusPermissionsService.GetSiteModuleMenuPermissionById(permission.Id);
                                        _sitesModulesMenusPermissionsService.DeleteSiteModuleMenusPermission(sitesModulesMenusPermission);

                                    }
                                    _sitesRolesService.DeleteSitesRoles(existingMapping);
                                }

                            }
                        }
                    }

                    return Ok(id);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Delete site
        ///<summary> Delete record </summary>
        ///<param name="id"> id of the record </param>
        ///<Author> Sachin Bhanse </Author>
        ///<CreatedOn> 07/19/2024 </CreatedOn>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSite(string id)
        {
            try
            {
                //Find record
                var entity = await _siteService.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No site found with the specified id."));

                //Soft delete record
                _siteService.DeleteSite(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        [HttpPut("generateDropdown/{id}")]
        public async Task<IActionResult> GenerateDropdown(string id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Fetch the site entity by its ID
                    var entity = await _siteService.GetById(id);
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var GetDateTime = _siteService.GetDateTime(entity.TimeZone);

                    // If no site is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No site with the specified id."));

                    var vskySite = await _siteService.GetBySiteName("Vsky Solutions");
                    if (vskySite == null)
                        return BadRequest(new BadRequestError("Vsky Solutions site not found."));

                    var masterDropDownTypeList = await _dropDownTypeService.GetAllDropDownTypeListBySiteId(vskySite.Id);
                    var masterDropDownList = await _dropDownService.GetAllDropDowns();

                    var existingDropDownTypeList = await _dropDownTypeService.GetAllDropDownTypeListBySiteId(entity.Id);
                    foreach (var masterDropDownType in masterDropDownTypeList)
                    {
                        // Check dropdown type already exists
                        var isDropDownTypeExists = existingDropDownTypeList.Any(x => x.Type == masterDropDownType.Type);

                        // Skip existing dropdown type and its values
                        if (isDropDownTypeExists)
                            continue;

                        var dropDownType = new DropDownType
                        {
                            Type = masterDropDownType.Type,
                            SiteId = entity.Id,
                            DisplayName = masterDropDownType.DisplayName,
                            GroupName = masterDropDownType.GroupName,
                            SortOrder = masterDropDownType.SortOrder,
                            ModuleName = masterDropDownType.ModuleName,
                            CreatedById = LoggedUserId,
                            UpdatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime,
                            UpdatedOnUtc = GetDateTime
                        };
                        _dropDownTypeService.InsertDropDownType(dropDownType);

                        var masterDropDownValues = masterDropDownList.Where(x => x.DropDownTypeId == masterDropDownType.Id).ToList();
                        foreach (var masterDropDownValue in masterDropDownValues)
                        {
                            var dropDown = new DropDown
                            {
                                DropDownTypeId = dropDownType.Id,
                                DropDownValue = masterDropDownValue.DropDownValue,
                                Active = masterDropDownValue.Active,
                                Description = masterDropDownValue.Description,
                                SortOrder = masterDropDownValue.SortOrder,
                                DropDownText = masterDropDownValue.DropDownText,
                                CreatedById = LoggedUserId,
                                UpdatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime,
                                UpdatedOnUtc = GetDateTime
                            };
                            _dropDownService.InsertDropDown(dropDown);
                        }
                    }

                    entity.IsDropdownGenerated = !entity.IsDropdownGenerated;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _siteService.UpdateSite(entity);

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("generateMasterNotifications/{siteId}")]
        public async Task<IActionResult> GenerateMasterNotifications(string siteId)
        {
            try
            {
                // Fetch the target site entity by its ID
                var targetSite = await _siteService.GetById(siteId);
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var GetDateTime = _siteService.GetDateTime(targetSite.TimeZone);

                // If no site is found with the given ID, return a bad request with an error message
                if (targetSite == null)
                    return BadRequest(new BadRequestError("No site with the specified id."));

                var vskySite = await _siteService.GetBySiteName("Vsky Solutions");
                if (vskySite == null)
                    return BadRequest(new BadRequestError("Vsky Solutions site not found."));

                var masterNotifications = await _masterNotificationService.GetNotificationsForGeneratePermissionsData(vskySite.Id);
                foreach (var notification in masterNotifications)
                {
                    var exists = await _masterNotificationService.GetMasterNotificationBySiteIdAndNumber(targetSite.Id, notification.Number);

                    if (exists != null)
                        continue;

                    await _masterNotificationService.AddMasterNotification(targetSite.Id, notification.Number, notification.Title, notification.Message, notification.Type, LoggedUserId, GetDateTime);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSiteModifiedLogsById(string subModuleId, string columnNames)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var sitesModifiedLogs = new List<SitesModifiedLogsModel>();
                if (!string.IsNullOrEmpty(columnNames))
                {

                    foreach (var column in columnNames.Split(','))
                    {
                        var model = await _sitesModifiedLogsService.GetAllSitesModifiedLogDetailsById(SiteId, subModuleId, column);
                        if (model != null)
                        {
                            var mappedLogs = _mapper.Map<List<SitesModifiedLogsModel>>(model);
                            sitesModifiedLogs.AddRange(mappedLogs);
                        }
                    }
                }
                return Ok(sitesModifiedLogs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region Private methhods 
        private string GenerateTicketPrefix(string siteName)
        {
            if (string.IsNullOrWhiteSpace(siteName))
                return string.Empty;

            var words = siteName
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Take first letter of each word
            var prefix = string.Concat(words.Select(w => char.ToUpper(w[0])));

            // Optional: limit to 2 characters
            return prefix.Length > 2 ? prefix.Substring(0, 2) : prefix;
        }

        #endregion
    }
}