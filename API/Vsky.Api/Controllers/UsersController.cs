using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Common;
using Vsky.Services.Employees;
using Vsky.Services.Messages;
using Vsky.Services.Persons;
using Vsky.Services.Sites;
using Vsky.Services.SitesRole;
using Vsky.Services.Users;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace Vsky.Api.Controllers
{
    [Route("users")]
    //[Authorize(Roles = Roles.Administrator + "," + Roles.SuperAdmin)]
    public class UsersController : BaseController
    {
        #region Fields
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IUserService _userService;
        private readonly ISiteService _siteService;
        private readonly IEmployeeService _employeeService;
        private readonly IPersonService _personService;
        private readonly ICommonService _commonService;
        private readonly ISitesRolesService _sitesRolesService;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Ctor

        public UsersController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            ApplicationDbContext db, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager,
            IWorkflowMessageService workflowMessageService, 
            IUserService userService, 
            ISiteService SiteService, 
            IEmployeeService employeeService,
            IPersonService personService,
            ICommonService commonService,
            ISitesRolesService sitesRolesService,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _workflowMessageService = workflowMessageService;
            _userService = userService;
            _siteService = SiteService;
            _employeeService = employeeService;
            _personService = personService;
            _commonService = commonService;
            _sitesRolesService = sitesRolesService;
            _applicationUserRoleService = applicationUserRoleService;
        }

        #endregion

        #region Methods

        #region GetAllCompanies
        [HttpPost("list")]
        public IActionResult GetAllUsersList(UserSearchModel searchModel)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            searchModel.SiteId = searchModel.SiteId != null ? searchModel.SiteId : SiteId;
            var list = _userService.GetAllUsersList(searchModel.SiteId,
                searchModel.SearchText,
                searchModel.UserStatus,
                searchModel.UserName,
                searchModel.FullName,
                searchModel.Email,
                searchModel.SiteRoleIds,
                searchModel.UserId,
                searchModel.SortBy,
                searchModel.Descending,
                searchModel.Page,
                searchModel.PageSize);
            var model = new UserListModel
            {
                Data = _mapper.Map<IList<UserModel>>(list),
                Total = list.TotalCount
            };
            return Ok(model);
        }
        #endregion


        [HttpPut("updateUserStatus/{id}")]
        public async Task<IActionResult> UpdateUserStatus(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByIdAsync(id);

                    if (user == null)
                        return NotFound(new BadRequestError("No user was found with the specified id"));

                    var isActivating = !user.Active;

                    user.Active = isActivating;
                    user.UpdatedById = LoggedUserId;
                    user.UpdatedOnUtc = GetDateTime;

                    await _userManager.SetLockoutEndDateAsync(user, isActivating ? null : DateTimeOffset.UtcNow.AddYears(100));

                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                        return BadRequest(result.Errors);

                    // Update Employee if exists
                    //var employeeId = _commonService.GetEmployeeIdByUserId(SiteId, id);
                    var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, id);
                    if (employeeId != null)
                    {
                        var employee = await _employeeService.GetById(employeeId);
                        if (employee != null)
                        {
                            employee.Active = isActivating;
                            employee.UpdatedById = LoggedUserId;
                            employee.UpdatedOnUtc = GetDateTime;
                            _employeeService.UpdateEmployee(employee);
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

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var loggedUser = _db.Users.Where(x => x.Id == LoggedUserId).FirstOrDefault();
            var query = _userManager.Users.Where(x => !x.Deleted && x.UserRoles.Any(m => m.Role.Name != "SuperAdmin"));
            query = query.Select(x => new ApplicationUser
            {
                Id = x.Id,
                UserName = x.UserName,
                Active = x.Active,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                UserRoles = x.UserRoles.Select(mapping => new ApplicationUserRole
                {
                    Role = mapping.Role
                }).ToList()
            });

            var list = await query.ToListAsync();
            var model = _mapper.Map<IList<UserModel>>(list);
            return Ok(model);
        }

        [HttpGet("{id}/{siteId}")]
        public async Task<IActionResult> GetUserById(string id, string siteId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;

            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.Deleted)
                return BadRequest(new BadRequestError("No user was found with the specified id."));

            //var roleNames = await _userManager.GetRolesAsync(user);
            //var roleIds = _roleManager.Roles.Where(r => roleNames.Contains(r.Name)).Select(r => r.Id).ToArray();

            var roleIds = await _applicationUserRoleService.GetRoleIdsByUserAndSite(user.Id, SiteId);

            // Get siteRoleIds based on the roleIds
            //var siteRoleIds = await _sitesRolesService.GetSiteRoleIdsByRoleIds(SiteId, roleIds);
            var siteRoleIds = await _sitesRolesService.GetSiteRoleIdsByRoleIds(SiteId, roleIds.ToArray());
            var model = _mapper.Map<UserModel>(user);
            model.SiteRoleIds = siteRoleIds.ToArray();

            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = string.IsNullOrWhiteSpace(model.SiteId) ?  _globalVariable.SiteId : model.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var user = _mapper.Map<ApplicationUser>(model);

                var exists = await _userManager.Users.AnyAsync(x => x.UserName == model.Username && !x.Deleted);
                if (exists)
                    return BadRequest(new BadRequestError("Username already exists. Please try a different one."));

                if (model.personId != null || model.employeeId != null)
                {
                    var contactexists =_userManager.Users.Where(x => x.PersonId == model.personId && !x.Deleted && x.PersonId != null);
                    if(contactexists.Count() > 0)
                        return BadRequest(new BadRequestError("User already exists. Please try a different one."));
                    
                    var PersonByEmployeeId =await _employeeService.GetEmployeeDetailsById(model.employeeId);
                    if(PersonByEmployeeId !=null)
                    {
                        model.employeeId = PersonByEmployeeId.Person.Id;
                        var personExist = _userManager.Users.Where(x => x.PersonId == model.employeeId && !x.Deleted && x.PersonId != null);
                        if (personExist.Count() > 0)
                            return BadRequest(new BadRequestError("User already exists. Please try a different one."));
                    }
                }

                if (_userManager.Options.User.RequireUniqueEmail)
                {
                    exists = await _userManager.Users.AnyAsync(x => x.Email == model.Email && !x.Deleted);
                    if (exists)
                        return BadRequest(new BadRequestError("Email already exists. Please try a different one."));
                }

                user.Id = Guid.NewGuid().ToString();
                if (model.personId != null)
                {
                    user.PersonId = model.personId;
                    user.Email = model.Email;
                    user.Type = "Customer";
                }
                else
                {
                    user.PersonId = model.employeeId;
                    user.Email = model.employeeEmail;
                    user.Type = "Employee";
                }

                user.EmailConfirmed = true;
                user.PhoneNumberConfirmed = true;
                user.TwoFactorEnabled = false;
                user.LockoutEnabled = true;
                user.CreatedOnUtc = GetDateTime;
                user.CreatedById = LoggedUserId;
                user.UpdatedById = LoggedUserId;
                user.UpdatedOnUtc = GetDateTime;
                user.Active = true;
                var password = model.Password;

                var result = await _userManager.CreateAsync(user, model.Password);

                var existPersonSiteMapping = await _personService.GetPersonSiteMappingByPersonId(user.PersonId, SiteId);
                if (existPersonSiteMapping == null)
                {
                    var personSiteMapping = new PersonSitesMapping();
                    personSiteMapping.PersonId = user.PersonId;
                    personSiteMapping.SiteId = SiteId;
                    personSiteMapping.CreatedById = LoggedUserId;
                    personSiteMapping.UpdatedById = LoggedUserId;
                    personSiteMapping.CreatedOnUtc = GetDateTime;
                    personSiteMapping.UpdatedOnUtc = GetDateTime;
                    _personService.InsertPersonSites(personSiteMapping);
                }

                if (result.Succeeded)
                {
                    //var siteRolesList = await _sitesRolesService.GetRolesBySiteRoleIds(model.SiteRoleIds);
                    //foreach (var siteRole in siteRolesList)
                    //{
                    //    var roleName = siteRole.ApplicationRole?.NormalizedName;

                    //    if (!string.IsNullOrEmpty(roleName))
                    //    {
                    //        await _userManager.AddToRoleAsync(user, roleName);
                    //    }

                    //}

                    var siteRolesList = await _sitesRolesService.GetRolesBySiteRoleIds(model.SiteRoleIds);
                    foreach (var siteRole in siteRolesList)
                    {
                        var roleName = siteRole.ApplicationRole?.NormalizedName;
                        var roleId = siteRole.ApplicationRole?.Id;

                        if (!string.IsNullOrEmpty(roleId))
                        {
                            await _applicationUserRoleService.AddUserRoleAsync(
                                user.Id,
                                roleId,
                                SiteId);

                            //await _userManager.AddToRoleAsync(user, roleName);
                        }
                    }

                    await _db.SaveChangesAsync();

                    //var userrole = await _roleManager.FindByNameAsync(Roles.Employee);

                    //await _userManager.AddToRoleAsync(user, userrole.Name);

                    if (model.SendEmail)
                        await _workflowMessageService.SendWelcomeEmail(user, password);

                    return Ok(new { password });
                }
                else
                {
                    return InternalServerError(result.Errors);
                }
            }
            return ModelStateError(ModelState);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UserModel model)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var user = await _userManager.FindByIdAsync(id);

                    if (user == null || user.Deleted)
                        return BadRequest(new BadRequestError("No user was found with the specified id."));

                    // user = _mapper.Map(model, user);
                    user.UpdatedById = LoggedUserId;
                    user.UpdatedOnUtc = GetDateTime;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        if (model.Password != null && model.Password != "")
                        {
                            var password = model.Password;
                            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                            await _userManager.ResetPasswordAsync(user, code, password);

                            if (model.SendEmail)
                                await _workflowMessageService.SendWelcomeEmail(user, password);
                        }

                        //var roleNames = await _userManager.GetRolesAsync(user);
                        //foreach (var roleName in roleNames)
                        //{
                        //    var roleData = _db.Roles.AsNoTracking().Where(x => x.Name == roleName && x.SiteId == SiteId).FirstOrDefault();
                        //    var userRole = await _db.UserRoles
                        //        .FirstOrDefaultAsync(x =>
                        //            x.UserId == user.Id &&
                        //            x.RoleId == roleData.Id &&
                        //            x.SiteId == SiteId);

                        //    if (userRole != null)
                        //    {
                        //        _db.UserRoles.Remove(userRole);
                        //        await _db.SaveChangesAsync();
                        //    }

                        //    //var result1 = await _userManager.RemoveFromRoleAsync(user, roleData.NormalizedName);
                        //}

                        //var siteRoles = await _sitesRolesService.GetRolesBySiteRoleIds(model.SiteRoleIds);
                        //foreach (var siteRole in siteRoles)
                        //{
                        //    var roleName = siteRole.ApplicationRole?.NormalizedName;

                        //    if (!string.IsNullOrEmpty(roleName))
                        //    {
                        //        await _userManager.AddToRoleAsync(user, roleName);
                        //    }

                        //}

                        // Remove existing roles for current site
                        await _applicationUserRoleService.RemoveAllUserRolesAsync(
                            user.Id,
                            SiteId);

                        await _db.SaveChangesAsync();

                        // Add selected roles
                        var siteRoles = await _sitesRolesService.GetRolesBySiteRoleIds(model.SiteRoleIds);

                        foreach (var siteRole in siteRoles)
                        {
                            var roleId = siteRole.ApplicationRole?.Id;

                            await _applicationUserRoleService.AddUserRoleAsync(
                                user.Id,
                                roleId,
                                SiteId);
                        }

                        await _db.SaveChangesAsync();

                        return NoContent();
                    }
                    else
                    {
                        return InternalServerError(result.Errors);
                    }
                }

                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.Deleted)
            {
                return BadRequest(new BadRequestError("No user was found with the specified id."));
            }

            user.Deleted = true;
            user.Active = false;
            user.UpdatedById = LoggedUserId;
            user.UpdatedOnUtc = GetDateTime;

            var userCount = _userManager.Users.Count(x => x.UserName.StartsWith(user.UserName + "_deleted"));
            var emailCount = _userManager.Users.Count(x => x.Email.StartsWith(user.Email + "_deleted"));

            // update username, email
            user.UserName += "_deleted_" + userCount;
            user.Email += "_deleted_" + emailCount;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return InternalServerError(result.Errors);
            }
        }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.Deleted)
                return BadRequest(new BadRequestError("No user was found with the specified id."));

            var password = _userService.GeneratePassword();

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, code, password);

            if (result.Succeeded)
            {
                await _workflowMessageService.SendResetPasswordEmail(user, password);

                return Ok(new { password });
            }
            else
            {
                return InternalServerError(result.Errors);
            }
        }

        #endregion

        #region GetAllUserFirstNameListForDropdown
        [HttpGet("dropdown/firstnamelist")]
        public async Task<IActionResult> GetAllUserFirstNameListForDropdown()
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _userService.GetAllUserFirstNameListForDropdown(SiteId);
            var model = _mapper.Map<List<ApplicationUser>>(list);
            return Ok(model);
        }
        #endregion

        #region GetAllUserLastNameListForDropdown
        [HttpGet("dropdown/lastnamelist")]
        public async Task<IActionResult> GetAllUserLastNameListForDropdown()
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _userService.GetAllUserLastNameListForDropdown(SiteId);
            var model = _mapper.Map<List<ApplicationUser>>(list);
            return Ok(model);
        }
        #endregion

        #region GetAllUserListForDropdown
        [HttpGet("dropdown/userlist/{siteId}/{flag}")]
        public async Task<IActionResult> GetAllUserListForDropdown(string siteId, string flag = null)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var isSystemSuperAdmin = _db.UserRoles.Any(ur => ur.UserId == LoggedUserId && _roleManager.Roles.Any(r => r.Id == ur.RoleId && r.NormalizedName == "system-super-admin"));
            var SiteId = (isSystemSuperAdmin && !string.IsNullOrWhiteSpace(siteId) && siteId != "undefined") ? siteId : _globalVariable.SiteId;
            var list = await _userService.GetAllUserListForDropdown(SiteId, flag);
            var model = _mapper.Map<List<ApplicationUser>>(list);
            return Ok(model);
        }
        #endregion

        #region GetAllUserListByRoleForDropdown
        // Title: GetAllUserListByRoleForDropdown
        // Description: This endpoint retrieves the list of user. 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllUserListByRoleForDropdown(string roleName)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _userService.GetUsersByRole(SiteId, roleName);
                var model = _mapper.Map<List<ApplicationUser>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Title: GetSupportTeamUsersData
        // Description: This endpoint retrieves the list of user. 
        [HttpGet("team-workload-dropdown/list")]
        public async Task<IActionResult> GetSupportTeamUsersData(string roleName)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _userService.GetSupportTeamUsersData(SiteId, roleName);
                var model = _mapper.Map<List<ApplicationUser>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region SendUserLoginDetails
        [HttpPost("{id}/send-user-login")]
        public async Task<IActionResult> SendUserLoginDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.Deleted)
                return BadRequest(new BadRequestError("No user was found with the specified id."));

            var password = _userService.GeneratePassword();
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, code, password);
            if (result.Succeeded)
            {
                await _workflowMessageService.SendWelcomeEmail(user, password);

                return Ok(new { password });
            }
            else
            {
                return InternalServerError(result.Errors);
            }
        }
        #endregion
    }
}