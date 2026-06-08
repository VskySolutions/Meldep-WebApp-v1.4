using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Common;
using Vsky.Services.Messages;
using Vsky.Services.Persons;
using Vsky.Services.Sites;
using Vsky.Services.SitesRole;
using Vsky.Services.Users;

namespace Vsky.Api.Controllers
{
    [Route("person-site-mapping")]
    public class PersonSitesMappingController : BaseController
    {
        #region Define services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IUserService _userService;
        private readonly ISiteService _siteService;
        private readonly IPersonService _personService;
        private readonly IPersonSitesMappingService _personSitesMappingService;
        private readonly ApplicationDbContext _db;
        private readonly ISitesRolesService _sitesRolesService;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services initialization
        public PersonSitesMappingController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ICommonService commonService,
            IWorkflowMessageService workflowMessageService,
            IUserService userService,
            ISiteService siteService,
            IPersonService PersonService,
            IPersonSitesMappingService personSitesMappingService,
            ApplicationDbContext db,
            ISitesRolesService sitesRolesService,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _commonService = commonService;
            _workflowMessageService = workflowMessageService;
            _userService = userService;
            _siteService = siteService;
            _personService = PersonService;
            _personSitesMappingService = personSitesMappingService;
            _db = db;
            _sitesRolesService = sitesRolesService;
            _applicationUserRoleService = applicationUserRoleService;
        }
        #endregion

        #region GetAllSiteShare
        [HttpPost("list")]
        public IActionResult GetAllSiteShare(PersonSitesSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var siteList = _personSitesMappingService.GetAllSiteShare(
                    SiteId, 
                    searchModel.SearchText, 
                    searchModel.PersonIds, 
                    searchModel.PrimaryEmailAddress, 
                    searchModel.SortBy, 
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = new PersonSitesListModel
                {
                    Data = _mapper.Map<IList<PersonSitesMappingModel>>(siteList),
                    Total = siteList.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllSharedSitesByLoggedUserId
        [HttpGet("my-share-sites-list")]
        public async Task<IActionResult> GetAllSharedSitesByLoggedUserId()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var list = await _personSitesMappingService.GetAllSharedSitesByLoggedUserId(LoggedUserId, SiteId);

                var model = new PersonSitesListModel
                {
                    Data = _mapper.Map<IList<PersonSitesMappingModel>>(list),
                    Total = list.Count
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Create Site Share
        [HttpPost("save-site-share-user")]
        public async Task<IActionResult> CreateSiteShareUser(SaveSiteSharing model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Get person by email
                    //var person = await _personService.GetPersonByEmail(email);
                    var person = await _personService.GetPersonByUserEmail(model.Email);
                    if (person == null)
                        return BadRequest(new { message = "User not found" });

                    // Check if already exists in current site
                    var existsInCurrentSite = await _personSitesMappingService.GetPersonInSite(person.Id, SiteId);
                    if (existsInCurrentSite)
                        return BadRequest(new { message = "User already exist" });

                    // Check if exists in other site
                    var existsInOtherSite = await _personSitesMappingService.GetPersonInOtherSite(person.Id, SiteId);
                    if (!existsInOtherSite)
                        return BadRequest(new { message = "User not found" });

                    var personSiteMapping = new PersonSitesMapping();
                    personSiteMapping.PersonId = person.Id;
                    personSiteMapping.SiteId = SiteId;
                    personSiteMapping.IsSharedUser = true;
                    personSiteMapping.CreatedById = LoggedUserId;
                    personSiteMapping.UpdatedById = LoggedUserId;
                    personSiteMapping.CreatedOnUtc = GetDateTime;
                    personSiteMapping.UpdatedOnUtc = GetDateTime;
                    _personSitesMappingService.InsertPersonSites(personSiteMapping);

                    var userId = await _userService.GetUserIdByPersonId(SiteId, person.Id);

                    //var existingRoleIds = await _applicationUserRoleService.GetRoleIdsByUserAndSite(userId, SiteId);

                    var roleIds = await _db.SitesRoles
                        .Where(x => model.RoleIds.Contains(x.Id))
                        .Select(x => x.RoleId)
                        .ToListAsync();

                    foreach (var roleId in roleIds)
                    {
                        //if (!existingRoleIds.Contains(roleId))
                        //{
                        //    _applicationUserRoleService.InsertApplicationUserRole(new ApplicationUserRole
                        //    {
                        //        UserId = userId,
                        //        RoleId = roleId,
                        //        SiteId = SiteId
                        //    });
                        //}

                        await _applicationUserRoleService.AddUserRoleAsync(
                            userId,
                            roleId,
                            SiteId);
                    }

                    await _db.SaveChangesAsync();

                    //await _db.SaveChangesAsync();

                    // Send Invitation Email
                    var siteName = await GetSiteNameById(SiteId);
                    var invitedByName = await _personSitesMappingService.GetInvitedByCreatedById(personSiteMapping.CreatedById);

                    await _workflowMessageService.SendSiteShareInvitationMail(
                        person,
                        model.Email,
                        siteName,
                        invitedByName.CreatedBy.Person.FullName,
                        SiteId
                    );

                    return Ok(new { message = "User saved successfully" });
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Delete site share
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSiteShare(string id)
        {
            try
            {
                //Find record
                var entity = await _personSitesMappingService.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No user found with the specified id."));

                _personSitesMappingService.DeletePersonSites(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        # region Update Last Used Site
        [HttpPost("update-last-used-site")]
        public async Task<IActionResult> UpdateLastUsedSite(string userId, string personId, string siteId)
        {
            var personSites = await _personSitesMappingService.GetAllSitesByPersonId(personId);
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            if (personSites != null)
            {
                foreach (var personSite in personSites)
                {
                    personSite.LastUsed = personSite.SiteId == siteId ? true : false;
                    personSite.UpdatedById = userId;
                    personSite.UpdatedOnUtc = GetDateTime;

                    _personSitesMappingService.UpdatePersonSites(personSite);
                }
            }
            return Ok();
        }
        #endregion

        #region Private Methods
        private async Task<string> GetSiteNameById(string siteId)
        {
            var site = await _siteService.GetSiteNameById(siteId);
            return site?.Name;
        }
        #endregion
    }
}