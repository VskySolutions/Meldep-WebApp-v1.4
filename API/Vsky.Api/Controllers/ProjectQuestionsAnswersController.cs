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
using Vsky.Services.Common;
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
        private readonly IProjectQuestionsAnswersContributorsService _projectQuestionAnswerContributorsService;
        private readonly ICommonService _commonService;
        public ProjectQuestionsAnswerController(
            GlobalVariable globalVariable,
            ISiteService siteService,
            ICommonService commonService,
            IAzureBlobImageServices azureBlobImageServices,
            IProjectQuestionsAnswersService projectQuestionsAnswerService,
            IProjectQuestionsAnswersResponseLogService projectQuestionsAnswersResponseLogService,
            IProjectQuestionsAnswersContributorsService projectQuestionAnswerContributorsService)
        {
            _globalVariable = globalVariable;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
            _projectQuestionsAnswerService = projectQuestionsAnswerService;
            _projectQuestionsAnswersResponseLogService = projectQuestionsAnswersResponseLogService;
            _commonService = commonService;
            _projectQuestionAnswerContributorsService = projectQuestionAnswerContributorsService;
        }
        #endregion

        [HttpPost("list")]
        public async Task<IActionResult> GetAllProjectQuestionsAnswers(ProjectQuestionsAnswersSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // var employeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                var list = await _projectQuestionsAnswerService.GetAllProjectQuestionsAnswers(
                    SiteId,
                    LoggedUserId,
                    employeeId,
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

        #region GetAllQuestionAnswersByQuestionId
        [HttpGet("questions-answers-log")]
        public async Task<IActionResult> GetAllQuestionAnswersByQuestionId(string questionId, bool latestOnTop = false)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _projectQuestionsAnswerService.GetAllQuestionAnswersByQuestionId(SiteId, questionId, latestOnTop);

                var model = new ProjectQuestionsAnswersList
                {
                    ProjectQuestionsAnswerList = list
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

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

                    var questionAnswerId = Guid.NewGuid().ToString();
                    var entity = new ProjectQuestionsAnswers
                    {
                        Id = questionAnswerId,
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

                    var contributors = new List<ProjectQuestionsAnswersContributors>();

                    // Employees
                    foreach (var employeeId in model.ContributorEmployeeIds ?? new List<string>())
                    {
                        if (string.IsNullOrEmpty(employeeId))
                            continue;

                        contributors.Add(new ProjectQuestionsAnswersContributors
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProjectQuestionAnswerId = entity.Id,
                            ContributorTypeId = model.ContributorTypeId,
                            ContributorEmployeeId = employeeId,
                            ContributorCustomerId = null,
                            CreatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime,
                            UpdatedById = LoggedUserId,
                            UpdatedOnUtc = GetDateTime,
                            Deleted = false
                        });
                    }

                    // Customers
                    foreach (var customerId in model.ContributorCustomerIds ?? new List<string>())
                    {
                        if (string.IsNullOrEmpty(customerId))
                            continue;

                        contributors.Add(new ProjectQuestionsAnswersContributors
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProjectQuestionAnswerId = entity.Id,
                            ContributorTypeId = model.ContributorTypeId,
                            ContributorEmployeeId = null,
                            ContributorCustomerId = customerId,
                            CreatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime,
                            UpdatedById = LoggedUserId,
                            UpdatedOnUtc = GetDateTime,
                            Deleted = false
                        });
                    }

                    if (contributors.Any())
                    {
                        _projectQuestionAnswerContributorsService
                            .InsertProjectQuestionAnswerContributorList(contributors);
                    }

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
                        return BadRequest(new BadRequestError("Question already exists, try with another."));

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

                    // Update Q&A Contributors
                    var existingContributors =
                        await _projectQuestionAnswerContributorsService
                            .GetProjectQuestionAnswerContributorsByQuestionAnswerId(
                                SiteId,
                                entity.Id);

                    var selectedEmployeeIds =
                        model.ContributorEmployeeIds ?? new List<string>();

                    var selectedCustomerIds =
                        model.ContributorCustomerIds ?? new List<string>();

                    var existingActiveContributors = existingContributors
                        .Where(x => !x.Deleted)
                        .ToList();

                    var newContributors =
                        new List<ProjectQuestionsAnswersContributors>();

                    // Employee Contributors
                    foreach (var employeeId in selectedEmployeeIds)
                    {
                        if (string.IsNullOrEmpty(employeeId))
                            continue;

                        var existing = existingActiveContributors
                            .FirstOrDefault(x =>
                                x.ContributorEmployeeId == employeeId);

                        if (existing != null)
                        {
                            existing.ContributorTypeId = model.ContributorTypeId;
                            existing.UpdatedById = loggedUserId;
                            existing.UpdatedOnUtc = currentDateTime;

                            _projectQuestionAnswerContributorsService
                                .UpdateProjectQuestionAnswerContributor(existing);

                            continue;
                        }

                        var deletedContributor = existingContributors
                            .FirstOrDefault(x =>
                                x.Deleted &&
                                x.ContributorEmployeeId == employeeId);

                        if (deletedContributor != null)
                        {
                            deletedContributor.Deleted = false;
                            deletedContributor.ContributorTypeId = model.ContributorTypeId;
                            deletedContributor.UpdatedById = loggedUserId;
                            deletedContributor.UpdatedOnUtc = currentDateTime;

                            _projectQuestionAnswerContributorsService
                                .UpdateProjectQuestionAnswerContributor(
                                    deletedContributor);

                            continue;
                        }

                        newContributors.Add(
                            new ProjectQuestionsAnswersContributors
                            {
                                Id = Guid.NewGuid().ToString(),
                                ProjectQuestionAnswerId = entity.Id,
                                ContributorTypeId = model.ContributorTypeId,
                                ContributorEmployeeId = employeeId,
                                ContributorCustomerId = null,
                                CreatedById = loggedUserId,
                                UpdatedById = loggedUserId,
                                CreatedOnUtc = currentDateTime,
                                UpdatedOnUtc = currentDateTime,
                                Deleted = false
                            });
                    }

                    // Customer Contributors
                    foreach (var customerId in selectedCustomerIds)
                    {
                        if (string.IsNullOrEmpty(customerId))
                            continue;

                        var existing = existingActiveContributors
                            .FirstOrDefault(x =>
                                x.ContributorCustomerId == customerId);

                        if (existing != null)
                        {
                            existing.ContributorTypeId = model.ContributorTypeId;
                            existing.UpdatedById = loggedUserId;
                            existing.UpdatedOnUtc = currentDateTime;

                            _projectQuestionAnswerContributorsService
                                .UpdateProjectQuestionAnswerContributor(existing);

                            continue;
                        }

                        var deletedContributor = existingContributors
                            .FirstOrDefault(x =>
                                x.Deleted &&
                                x.ContributorCustomerId == customerId);

                        if (deletedContributor != null)
                        {
                            deletedContributor.Deleted = false;
                            deletedContributor.ContributorTypeId = model.ContributorTypeId;
                            deletedContributor.UpdatedById = loggedUserId;
                            deletedContributor.UpdatedOnUtc = currentDateTime;

                            _projectQuestionAnswerContributorsService
                                .UpdateProjectQuestionAnswerContributor(
                                    deletedContributor);

                            continue;
                        }

                        newContributors.Add(
                            new ProjectQuestionsAnswersContributors
                            {
                                Id = Guid.NewGuid().ToString(),
                                ProjectQuestionAnswerId = entity.Id,
                                ContributorTypeId = model.ContributorTypeId,
                                ContributorEmployeeId = null,
                                ContributorCustomerId = customerId,
                                CreatedById = loggedUserId,
                                UpdatedById = loggedUserId,
                                CreatedOnUtc = currentDateTime,
                                UpdatedOnUtc = currentDateTime,
                                Deleted = false
                            });
                    }

                    // Delete removed contributors
                    var contributorsToDelete = existingActiveContributors
                        .Where(x =>
                            (x.ContributorEmployeeId != null &&
                             !selectedEmployeeIds.Contains(x.ContributorEmployeeId)) ||
                            (x.ContributorCustomerId != null &&
                             !selectedCustomerIds.Contains(x.ContributorCustomerId)))
                        .ToList();

                    foreach (var contributor in contributorsToDelete)
                    {
                        contributor.Deleted = true;
                        contributor.UpdatedById = loggedUserId;
                        contributor.UpdatedOnUtc = currentDateTime;
                    }

                    if (contributorsToDelete.Any())
                    {
                        _projectQuestionAnswerContributorsService
                            .UpdateProjectQuestionAnswerContributorList(
                                contributorsToDelete);
                    }

                    if (newContributors.Any())
                    {
                        _projectQuestionAnswerContributorsService
                            .InsertProjectQuestionAnswerContributorList(
                                newContributors);
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
