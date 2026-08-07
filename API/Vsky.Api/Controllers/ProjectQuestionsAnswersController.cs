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
using Vsky.Services.AzureBlobImage;
using Vsky.Services.ProjectQuestionsAnswer;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("project-questions-answers")]
    public class ProjectQuestionsAnswerController : BaseController
    {
        #region Define Services and Initializations
        private readonly GlobalVariable _globalVariable;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly IProjectQuestionsAnswersService _projectQuestionsAnswerService;
        private readonly IProjectQuestionsAnswersResponseLogService _projectQuestionsAnswersResponseLogService;
        public ProjectQuestionsAnswerController(
            GlobalVariable globalVariable,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices,
            IProjectQuestionsAnswersService projectQuestionsAnswerService,
            IProjectQuestionsAnswersResponseLogService projectQuestionsAnswersResponseLogService)
        {
            _globalVariable = globalVariable;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
            _projectQuestionsAnswerService = projectQuestionsAnswerService;
            _projectQuestionsAnswersResponseLogService = projectQuestionsAnswersResponseLogService;
        }
        #endregion

        [HttpPost("list")]
        public IActionResult GetAllProjectQuestionsAnswers(ProjectQuestionsAnswersSearchModel searchModel)
        {
            try
            {
                var SiteId = _globalVariable.SiteId;

                var list = _projectQuestionsAnswerService.GetAllProjectQuestionsAnswers(
                    SiteId,
                    searchModel.SearchText,
                    searchModel.Title,
                    searchModel.ProjectIds,
                    searchModel.RequirementIds,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = new ProjectQuestionsAnswersList
                {
                    ProjectQuestionsAnswerList = list,
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetProjectQuestionsAnswerByIdInDetail(string id)
        {
            try
            {
                var SiteId = _globalVariable.SiteId;

                var entity = await _projectQuestionsAnswerService.GetProjectQuestionsAnswerByIdInDetail(SiteId, id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No Project Questions Answer Found"));

                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        #region CreateProjectQuestionsAnswer
        // Title: CreateProjectQuestionsAnswer
        // Description: This endpoint handles the creation of a new Project Questions Answer. It sets the creation details, and inserts the Project Questions Answer into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateProjectQuestionsAnswer(SaveProjectQuestionsAnswers model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _projectQuestionsAnswerService.GetProjectQuestionsAnswerByTitle(SiteId, model.ProjectId, model.Title);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Project question already exists"));

                    var entity = new ProjectQuestionsAnswers
                    {
                        Id = Guid.NewGuid().ToString(),
                        SiteId = SiteId,
                        Title = model.Title,
                        ProjectId = model.ProjectId,
                        RequirementId = model.RequirementId,
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime,
                    };

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "Project-Questions-Answers",
                                entity.Id
                            );
                    }
                    _projectQuestionsAnswerService.InsertProjectQuestionsAnswer(entity);

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

        #region UpdateProjectQuestionsAnswer
        // Title: UpdateProjectQuestionsAnswer
        // Description: This endpoint updates an existing project questions answer by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProjectQuestionsAnswer(string id, SaveProjectQuestionsAnswers model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var loggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var currentDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _projectQuestionsAnswerService.GetProjectQuestionsAnswerById(id);
                    // If no project questions answer is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project questions answer found with the specified id."));

                    //Check if the project questions answer already exists
                    var exists = await _projectQuestionsAnswerService.GetProjectQuestionsAnswerByTitle(SiteId, model.ProjectId, model.Title, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("project questions answer title already exists, try with another."));

                    entity.Title = model.Title;
                    entity.ProjectId = model.ProjectId;
                    entity.RequirementId = model.RequirementId;
                    entity.UpdatedById = loggedUserId;
                    entity.UpdatedOnUtc = currentDateTime;

                    if (!string.IsNullOrWhiteSpace(model.Description))
                    {
                        entity.Description =
                            await _azureBlobImageServices.ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "Project-Questions-Answers",
                                entity.Id,
                                entity.Description);
                    }

                    _projectQuestionsAnswerService.UpdateProjectQuestionsAnswer(entity);


                    //save Response Change Log
                    if (model.ProjectQuestionsAnswersResponseLogs?.Any() == true)
                    {
                        var addList = new List<ProjectQuestionsAnswersResponseLog>();
                        var deleteList = new List<ProjectQuestionsAnswersResponseLog>();
                        var updateList = new List<ProjectQuestionsAnswersResponseLog>();

                        foreach (var item in model.ProjectQuestionsAnswersResponseLogs)
                        {
                            // Fetch the ProjectQuestionsAnswersResponseLog entity by its ID
                            var type = await _projectQuestionsAnswersResponseLogService.GetProjectQuestionsAnswersResponseLogById(item.Id);
                            if (item.Flag == "Edit")
                            {
                                // If no ProjectQuestionsAnswersResponseLog is found with the given ID, continue
                                if (type == null)
                                    continue;

                                type.ProjectQuestionsAnswersId = entity.Id;

                                if (!string.IsNullOrEmpty(item.Description))
                                {
                                    type.Description = await _azureBlobImageServices
                                        .ProcessHtmlAndManageImagesAsync(
                                            item.Description,
                                            SiteData.Name,
                                            "Project-Questions-Answers",
                                            entity.Id,
                                            type.Description
                                        );
                                }

                                // Set the Updated by and Updated on properties
                                type.UpdatedById = loggedUserId;
                                type.UpdatedOnUtc = currentDateTime;
                                updateList.Add(type);
                            }
                            else if (item.Flag == "New")
                            {
                                // If no ProjectQuestionsAnswersResponseLog is found with the given ID, continue
                                if (type != null)
                                    continue;

                                var data = new ProjectQuestionsAnswersResponseLog
                                {
                                    ProjectQuestionsAnswersId = entity.Id,
                                    CreatedById = loggedUserId,
                                    UpdatedById = loggedUserId,
                                    CreatedOnUtc = currentDateTime,
                                    UpdatedOnUtc = currentDateTime
                                };

                                if (!string.IsNullOrEmpty(item.Description))
                                {
                                    data.Description = await _azureBlobImageServices
                                        .ProcessHtmlAndManageImagesAsync(
                                            item.Description,
                                            SiteData.Name,
                                            "Project-Questions-Answers",
                                            entity.Id
                                        );
                                }

                                addList.Add(data);
                            }
                            else if (item.Flag == "Delete")
                            {
                                // If no RequirementChangeLog is found with the given ID, continue
                                if (type == null)
                                    continue;

                                deleteList.Add(type);
                            }
                        }

                        if (addList.Count > 0)
                            _projectQuestionsAnswersResponseLogService.InsertProjectQuestionsAnswersResponseLogList(addList);

                        if (updateList.Count > 0)
                            _projectQuestionsAnswersResponseLogService.UpdateProjectQuestionsAnswersResponseLogList(updateList);

                        if (deleteList.Count > 0)
                            _projectQuestionsAnswersResponseLogService.DeleteProjectQuestionsAnswersResponseLogList(deleteList);
                    }
                    return Ok();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectQuestionsAnswer(string id)
        {
            try
            {
                // Fetch the  project questions answer entity by its ID
                var entity = await _projectQuestionsAnswerService.GetProjectQuestionsAnswerById(id);
                // If no  project questions answer is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No project questions answer found with the specified id."));

                // Delete the  project questions answer using the  project questions answer service
                _projectQuestionsAnswerService.DeleteProjectQuestionsAnswer(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
