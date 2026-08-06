using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.Issues;
using Vsky.Services.ProjectActionItem;
using Vsky.Services.ProjectReleaseTrackings;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.TestCases;

namespace Vsky.Api.Controllers
{
    [Route("project-action-items")]
    public class ProjectActionItemsController : BaseController
    {
        #region Define Services      
        private readonly GlobalVariable _globalVariable;
        private readonly IProjectActionItemsService _projectActionItemsService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly IDropDownService _dropDownService;

        #endregion

        #region Services Initializations      
        public ProjectActionItemsController(
            GlobalVariable globalVariable,
            IProjectActionItemsService projectActionItemsService,
            ICommonService commonService,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices,
            ISitesModifiedLogsService sitesModifiedLogsService,
            IDropDownService dropDownService
        )
        {
            _globalVariable = globalVariable;
            _projectActionItemsService = projectActionItemsService;
            _commonService = commonService;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _dropDownService = dropDownService;
        }
        #endregion

        #region GetAllProjectActionItems
        [HttpPost("list")]
        public async Task<IActionResult> GetAllProjectActionItems(ProjectActionItemsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var list = await _projectActionItemsService.GetAllProjectActionItems(
                    SiteId,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.ProjectIds,
                    searchModel.RequirementIds,
                    searchModel.PriorityIds,
                    searchModel.Title,
                    searchModel.AssignedTo,
                    searchModel.DueDate,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = new ProjectActionItemsList
                {
                    ProjectActionItemList = list,
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

        #region GetProjectActionItemById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectActionItemById(string id)
        {
            try
            {
                var entity = await _projectActionItemsService.GetProjectActionItemById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No project action item found with the specified id."));

                //var model = new ProjectActionItems
                //{
                //    ProjectId = entity.ProjectId,
                //    RequirementId = entity.RequirementId,
                //    PriorityId = entity.PriorityId,
                //    Title = entity.Title,
                //    Description = entity.Description,
                //    AssignedTo = entity.AssignedTo,
                //    DueDate = entity.DueDate
                //};

                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetProjectActionItemDetailsById
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetProjectActionItemDetailsById(string id)
        {
            try
            {
                var entity = await _projectActionItemsService.GetProjectActionItemDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No project action item found with the specified id."));

                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region
        [HttpPost("save-project-action-items")]
        public async Task<IActionResult> AddUpdateProjectActionItems(SaveProjectActionItems model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // update existing record
                    var exists = await _projectActionItemsService.GetProjectActionItemById(model.Id);
                    if (exists != null)
                    {
                        exists.ProjectId = model.ProjectId;
                        exists.RequirementId = model.RequirementId;
                        exists.PriorityId = model.PriorityId;
                        exists.Title = model.Title;
                        exists.Description = model.Description;
                        exists.AssignedTo = model.AssignedTo;
                        exists.DueDate = model.DueDate;

                        exists.UpdatedOnUtc = GetDateTime;
                        exists.UpdatedById = LoggedUserId;

                        _projectActionItemsService.UpdateProjectActionItems(exists);

                    }
                    else if (exists == null)
                    {
                        // Check duplicate

                        //Add new record
                        var projectActionItems = new ProjectActionItems
                        {
                            Id = Guid.NewGuid().ToString(),
                            SiteId = SiteId,
                            ProjectId = model.ProjectId,
                            RequirementId = !string.IsNullOrEmpty(model.RequirementId) ? model.RequirementId : null,
                            PriorityId = !string.IsNullOrEmpty(model.PriorityId) ? model.PriorityId : null,
                            Title = !string.IsNullOrEmpty(model.Title) ? model.Title : null,
                            Description = !string.IsNullOrEmpty(model.Description) ? model.Description : null,
                            AssignedTo = !string.IsNullOrEmpty(model.AssignedTo) ? model.AssignedTo : null,
                            DueDate = model.DueDate,

                            CreatedOnUtc = GetDateTime,
                            CreatedById = LoggedUserId,
                            UpdatedOnUtc = GetDateTime,
                            UpdatedById = LoggedUserId
                        };
                        _projectActionItemsService.InsertProjectActionItems(projectActionItems);

                    }

                    return Ok(model);
                }
            }
            catch (Exception ex)
            {
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region Delete ProjectActionItems
        [HttpDelete("{id}/delete-project-action-items")]
        public async Task<IActionResult> DeleteProjectActionItems(string id)
        {
            var entity = await _projectActionItemsService.GetProjectActionItemById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No project action item found with the specified id."));

            _projectActionItemsService.DeleteProjectActionItems(entity);

            return NoContent();
        }
        #endregion
    }
}
