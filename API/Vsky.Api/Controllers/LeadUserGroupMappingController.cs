using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.LeadUserGroupMappings;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("lead-groups")]
    public class LeadUserGroupMappingController : BaseController
    {
        private readonly GlobalVariable _globalVariable;
        private readonly ILeadUserGroupMappingService _leadUserGroupMappingService;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;

        public LeadUserGroupMappingController(
            IMapper mapper,
            GlobalVariable globalVariable,
            ILeadUserGroupMappingService leadUserGroupMappingService,
            ISiteService siteService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _leadUserGroupMappingService = leadUserGroupMappingService;
            _siteService = siteService;
        }

        #region GetAllLeadGroupUsers
        [HttpPost("lead-group-users/list")]
        public async Task<IActionResult> GetAllLeadGroupUsers(LeadUserGroupMappingSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _leadUserGroupMappingService.GetAllLeadUserGroups(
                    SiteId,
                    searchModel.SearchText,
                    searchModel.UserIds,
                    searchModel.LeadGroupIds,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize);

                // Map the fetched list to a model suitable for the response
                var model = new LeadUserGroupMappingListModel
                {
                    Data = _mapper.Map<IList<LeadUserGroupMappingModel>>(list),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(new BadRequestError("No data found."));
            }

        }
        #endregion

        #region CreateLeadGroupsUser
        [HttpPost("assign-user-to-lead-group")]
        public async Task<IActionResult> CreateLeadGroupsUser(LeadUserGroupMappingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ModelStateError(ModelState);

                var loggedUserId = User.GetLoggedInUserId<string>();
                var siteId = _globalVariable.SiteId;
                var siteData = await _siteService.GetById(siteId);
                var currentDateTime = _siteService.GetDateTime(siteData.TimeZone);

                foreach (var userId in model.UserIds)
                {
                    var existingGroupNames =
                        await _leadUserGroupMappingService.GetAssignedLeadGroupNames(
                            siteId,
                            userId,
                            model.LeadGroupIds.ToList());

                    if (existingGroupNames.Any())
                        continue;

                    foreach (var groupId in model.LeadGroupIds)
                    {
                        var entity = new LeadUserGroupMapping
                        {
                            SiteId = siteId,
                            UserId = userId,
                            LeadGroupId = groupId,
                            Active = true,
                            CreatedOnUtc = currentDateTime,
                            UpdatedOnUtc = currentDateTime,
                            CreatedById = loggedUserId,
                            UpdatedById = loggedUserId
                        };

                        _leadUserGroupMappingService.InsertLeadUserGroup(entity);
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateLeadGroupsUserStatus
        [HttpPut("active-inactive-status/{id}")]
        public async Task<IActionResult> UpdateLeadGroupsUserStatus(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (ModelState.IsValid)
                {
                    var entity = await _leadUserGroupMappingService.GetLeadUserGroupUserById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Lead group User found with the specified id."));

                    entity.Active = !entity.Active;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _leadUserGroupMappingService.UpdateLeadUserGroup(entity);

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

        #region DeleteLeadGroupsUser
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeadGroupsUser(string id)
        {
            try
            {
                var entity = await _leadUserGroupMappingService.GetLeadUserGroupUserById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No Lead group User found with the specified id."));

                _leadUserGroupMappingService.DeleteLeadUserGroup(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
