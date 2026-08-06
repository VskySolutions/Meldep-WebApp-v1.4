using System;
using System.Threading.Tasks;
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
        public ProjectQuestionsAnswerController(
            GlobalVariable globalVariable,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices,
            IProjectQuestionsAnswersService projectQuestionsAnswerService)
        {
            _globalVariable = globalVariable;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
            _projectQuestionsAnswerService = projectQuestionsAnswerService;
        }
        #endregion

        [HttpPost("list")]
        public IActionResult GetAllProjectQuestionsAnswers(ProjectQuestionsAnswersSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
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
                var LoggedUserId = User.GetLoggedInUserId<string>();
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

                    var exists = await _projectQuestionsAnswerService.GetProjectQuestionsAnswerByTitle(SiteId, model.Title);
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
                    var exists = await _projectQuestionsAnswerService.GetProjectQuestionsAnswerByTitle(SiteId, model.Title, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("project questions answer title already exists, try with another."));

                    bool isDescriptionChanged = !string.Equals(
                        entity.Description?.Trim(),
                        model.Description?.Trim(),
                        StringComparison.Ordinal);


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
                }
                return NoContent();
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
