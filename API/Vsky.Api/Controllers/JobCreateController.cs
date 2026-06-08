using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.JobsCreate;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("jobpost")]
    public class JobCreateController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IJobCreateService _jobCreateService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations
        public JobCreateController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            IJobCreateService jobCreateService, 
            ICommonService commonService,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _jobCreateService = jobCreateService;
            _commonService = commonService;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllJobPosts
        // Title: Get All JobPosts
        // Description: This endpoint fetches a list of jobposts based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllJobPosts(JobCreateSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of JobCreate based on search criterias
                var list = _jobCreateService.GetAllJobPosts(SiteId, searchModel.SearchText, searchModel.JobTitle, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new JobCreateListModel
                {
                    Data = _mapper.Map<IList<JobCreateModel>>(list),
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

        #region GetAllJobPostListForDropdown
        // Title: GetAllJobPostListForDropdown
        // Description: This endpoint retrieves the details of a specific jobcreate based on its unique identifier (ID). 
        [AllowAnonymous]
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllJobPostListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _jobCreateService.GetAllJobPostListForDropdown(SiteId);
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("vskyWebsiteDropdown/list")]
        public async Task<IActionResult> GetAllJobPostListForVskyWebsite(string siteId = null)
        {
            try
            {
                var SiteId = string.IsNullOrEmpty(siteId) ? "04086f10-8f09-4e0c-b5c0-c827a244addd" : siteId;

                var list = await _jobCreateService.GetAllJobPostListForDropdown(SiteId);
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetJobPostDetailsById
        // Title: GetJobPostDetailsById
        // Description: This endpoint retrieves the details of a specific jobcreate based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobPostDetailsById(string id)
        {
            try
            {
                // Fetch the jobcreate entity by its ID from the service
                var entity = await _jobCreateService.GetJobPostDetailsById(id);
                // If the jobcreate entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No job post found with the specified id."));

                // Map the jobcreate entity to a JobCreateModel object
                var model = _mapper.Map<JobCreateModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateJobPost
        // Title: CreateJobPost
        // Description: This endpoint handles the creation of a new JobCreate. It first checks if a JobCreate with the same name already exists. If not, it maps the JobCreate model to the JobCreate entity, sets the creation details, and inserts the JobCreate into the database.
        [HttpPost]
        public async Task<IActionResult> CreateJobPost(JobCreateModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    
                    //Check if the JobCreate already exists
                    var exists = await _jobCreateService.GetJobPostByTitle(SiteId, model.JobTitle);
                    if (exists != null)
                        return BadRequest(new BadRequestError("JobPost title already exists, try with another."));

                    // Map the JobCreate model to the JobCreate entity
                    var entity = _mapper.Map<JobCreate>(model);
                    entity.SiteId = SiteId;
                    entity.IsActive = model.IsActive;
                    entity.JobCreatedDate = GetDateTime;

                    if (model.PublishedJobDateStr != "" && model.PublishedJobDateStr != null)
                        entity.PublishedJobDate = DateTime.ParseExact(model.PublishedJobDateStr, "MM/dd/yyyy", null);

                    if (!string.IsNullOrEmpty(model.JobDescription))
                    {
                        entity.JobDescription = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.JobDescription,
                                SiteData.Name,
                                "job-post",
                                entity.Id
                            );
                    }
                    // Set the created by and created on properties
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _jobCreateService.InsertJobPost(entity);

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

        #region UpdateJobPost
        // Title: UpdateJobPost
        // Description: This endpoint updates an existing JobCreate by its ID. It validates the JobCreate model, checks for duplicate JobCreate title, updates the JobCreate's details.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJobPost(string id, JobCreateModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Check if there is any JobCreate with the same title that is not marked as deleted and has a different ID
                    var exists = await _jobCreateService.GetJobPostByTitle(SiteId, model.JobTitle, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("JobPost title already exists, try with another."));

                    // Fetch the JobCreate entity by its ID
                    var entity = await _jobCreateService.GetJobPostById(id);
                    // If no JobCreate is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No JobPost found with the specified id."));

                    if (!string.IsNullOrEmpty(model.JobDescription))
                    {
                        entity.JobDescription = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.JobDescription,
                                SiteData.Name,
                                "job-post",
                                entity.Id
                            );
                    }
                    // Set the user who updated the JobCreate and the current UTC time for tracking purposes
                    entity.Criteria = model.Criteria;
                    entity.JobTitle = model.JobTitle;
                    entity.JobReference = model.JobReference;
                    entity.IsActive = model.IsActive;
                    // Set custom properties
                    if (model.PublishedJobDateStr != "" && model.PublishedJobDateStr != null)
                        entity.PublishedJobDate = DateTime.ParseExact(model.PublishedJobDateStr, "MM/dd/yyyy", null);

                    // Set the updated by and updated on properties
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _jobCreateService.UpdateJobPost(entity);

                    return Ok(entity);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region
        //created for update job status from list page
        [HttpPut("{id}/{statusId}")]
        public async Task<IActionResult> UpdateJobStatus(string id, bool statusId)
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
                    var entity = await _jobCreateService.GetJobPostById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No job found with the specified id."));

                    entity.IsActive = statusId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _jobCreateService.UpdateJobPost(entity);

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

        #region DeleteJobCreate
        // Title: DeleteJobCreateById
        // Description: This endpoint deletes a jobCreate based on the provided jobCreate ID. It first retrieves the jobCreate entity by ID, checks if it exists, and if so, deletes the jobCreate. If the jobCreate is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobCreate(string id)
        {
            try
            {
                // Fetch the jobCreate entity by its ID
                var entity = await _jobCreateService.GetJobPostById(id);
                // If no jobCreate is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No job post found with the specified id."));

                // Delete the jobCreate using the jobCreate service
                _jobCreateService.DeleteJobPost(entity);

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