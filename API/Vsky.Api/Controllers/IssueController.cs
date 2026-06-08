using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.IssueActivitys;
using Vsky.Services.Issues;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("issue")]
    public class IssueController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IIssueService _issueService;
        private readonly ICommonService _commonService;
        private readonly IIssueStatusChangedLogService _issueStatusChangedLogService;
        private readonly IIssueActivityService _issueActivityService;
        private readonly ISiteService _siteService;
        private readonly ApplicationDbContext _db;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations      
        public IssueController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            IIssueService issueService, 
            ICommonService commonService, 
            IIssueStatusChangedLogService issueStatusChangedLogService, 
            IIssueActivityService issueActivityService,
            ApplicationDbContext db, 
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _issueService = issueService;
            _commonService = commonService;
            _issueStatusChangedLogService = issueStatusChangedLogService;
            _issueActivityService = issueActivityService;
            _siteService = siteService;
            _db = db;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllIssues
        // Title: Get All Issues
        // Description: This endpoint fetches a list of issue based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllIssues(IssueSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of issues on search criteria (name, sorting, pagination)
                var list = await _issueService.GetAllIssues(
                    SiteId,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.IssueNumber,
                    searchModel.ProjectIds,
                    searchModel.ProjectModuleIds,
                    searchModel.Name,
                    searchModel.PriorityIds,
                    searchModel.StatusIds,
                    searchModel.IssueTypeIds,
                    searchModel.EmployeeIds,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                    );
                
                // Get projectIds from issues
                var projectIds = list.Select(x => x.ProjectId).Distinct().ToList();

                // Get summary
                var summaryList = _issueService.GetIssueStatusSummaryByProjectIds(projectIds);

                // Prepare final response
                var model = new
                {
                    Data = _mapper.Map<IList<IssueModel>>(list),
                    Total = list.TotalCount,
                    StatusSummary = summaryList
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetIssueById
        //Title: GetIssueById
        //Description: This endpoint retrieves the details of a specific issue based on its unique identifier(ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIssueById(string id)
        {
            try
            {
                // Fetch the issue entity by its ID from the service
                var entity = await _issueService.GetIssueById(id);
                // If the issue entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No issue found with the specified id."));

                // Map the issue entity to a IssueModel object
                var model = _mapper.Map<IssueModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetIssueDetailsById
        // Title: GetIssueDetailsById
        // Description: This endpoint retrieves the details of a specific issue based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetIssueDetailsById(string id)
        {
            try
            {
                // Fetch the issue entity by its ID from the service
                var entity = await _issueService.GetIssueDetailsById(id);
                // If the issue entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No issue found with the specified id."));

                // Map the issue entity to a IssueModel object
                var model = _mapper.Map<IssueModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateIssue
        // Title: CreateIssue
        // Description: This endpoint handles the creation of a new issue. It maps the issue model to the issue entity, sets the creation details, and inserts the issue into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateIssue(IssueModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _issueService.GetIssueByName(SiteId, model.Name, model.ProjectId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("The issue already exists"));
                    // Map the Issue model to the Issue entity
                    var entity = _mapper.Map<Issue>(model);
                    entity.IssueNumber = await _issueService.GetLastIssueNumber() + 1;

                    if (model.DueDateStr != "" && model.DueDateStr != null)
                        entity.DueDate = DateTime.ParseExact(model.DueDateStr, "MM/dd/yyyy", null);

                    if (model.ReportedById != null && model.ReportedById != "")
                        entity.ReportedById = model.ReportedById;
                    else
                        entity.ReportedById = null;

                    if (model.EmployeeId != null && model.EmployeeId != "")
                        entity.EmployeeId = model.EmployeeId;
                    else
                        entity.EmployeeId = null;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "issue",
                                entity.IssueNumber.ToString()
                            );
                    }

                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _issueService.InsertIssue(entity);

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

        #region UpdateIssue
        // Title: UpdateIssue
        // Description: This endpoint updates an existing issue by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIssue(string id, IssueModel model)
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
                    var entity = await _issueService.GetIssueById(id);
                    // If no issue is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No issue found with the specified id."));

                    //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                    var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                    var leavestatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Issue Status", "Closed");
                    if (model.StatusId == leavestatus)
                    {
                        entity.ClosedBy = EmployeeId;
                        entity.CloseDate = GetDateTime;
                    }

                    entity.ProjectId = model.ProjectId;
                    entity.ProjectModuleId = model.ProjectModuleId;
                    entity.AreaId = model.AreaId;
                    entity.WorkspaceId = model.WorkspaceId;
                    entity.Name = model.Name;
                    entity.PriorityId = model.PriorityId;
                    entity.StatusId = model.StatusId;
                    entity.TypeId = model.TypeId;

                    if (model.ReportedById != null && model.ReportedById != "")
                        entity.ReportedById = model.ReportedById;
                    else
                        entity.ReportedById = null;

                    if (model.EmployeeId != null && model.EmployeeId != "")
                        entity.EmployeeId = model.EmployeeId;
                    else
                        entity.EmployeeId = null;

                    entity.LastModifiedBy = EmployeeId;
                    entity.LastUpdatedDate = GetDateTime;

                    if (model.DueDateStr != "" && model.DueDateStr != null)
                        entity.DueDate = DateTime.ParseExact(model.DueDateStr, "MM/dd/yyyy", null);

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "issue",
                                entity.IssueNumber.ToString(),
                                entity.Description
                            );
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _issueService.UpdateIssue(entity);

                    var statusLog = new IssueStatusChangedLog();
                    statusLog.IssueId = entity.Id;
                    statusLog.StatusId = model.StatusId;
                    statusLog.StatusChangedBy = EmployeeId;
                    statusLog.StatusChangedDate = GetDateTime;
                    _issueStatusChangedLogService.InsertIssueStatusChangedLog(statusLog);

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

        //create for update issue status from issue list -by SN
        #region
        [HttpPost("updateIssueStatus")]
        public async Task<IActionResult> UpdateIssueStatus(IssueModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.IssueIds != null && model.IssueIds.Count > 0)
                    {
                        foreach (var issueId in model.IssueIds)
                        {
                            var entity = await _issueService.GetIssueById(issueId);
                            if (entity != null)
                            {
                                bool result = await UpdateIssueDetails(issueId, model.StatusId, "status");
                            }
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
        #endregion

        #region
        [HttpPost("updateIssuePriority")]
        public async Task<IActionResult> UpdateIssuePriority(IssueModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.IssueIds != null && model.IssueIds.Count > 0)
                    {
                        foreach (var issueId in model.IssueIds)
                        {
                            var entity = await _issueService.GetIssueById(issueId);
                            if (entity != null)
                            {
                                bool result = await UpdateIssueDetails(issueId, model.PriorityId, "priority");
                            }
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
        #endregion

        #region DeleteIssue
        // Title: DeleteIssueById
        // Description: This endpoint deletes a issue based on the provided issue ID. It first retrieves the issue entity by ID, checks if it exists, and if so, deletes the issue. If the issue is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssue(string id)
        {
            try
            {
                // Fetch the issue entity by its ID
                var entity = await _issueService.GetIssueById(id);
                // If no issue is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No issue found with the specified id."));

                // Delete the issue using the issue service
                _issueService.DeleteIssue(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetActivityById
        [HttpGet("{id}/issueActivity")]
        public async Task<IActionResult> GetActivityLogById(string id)
        {
            var entity = _issueActivityService.GetByIssueId(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No issue activity log found with the specified id."));

            var model = _mapper.Map<List<IssueActivityModel>>(entity);
            return Ok(model);
        }
        #endregion

        #region CreateIssueActivity
        [HttpPost("issueActivity")]
        public async Task<IActionResult> CreateIssueActivity(IssueActivityModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (model.Id == null)
                {
                    var entity = _mapper.Map<IssueActivity>(model);
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _issueActivityService.InsertIssueActivity(entity);
                }
                else
                {
                    var entity = await _issueActivityService.GetById(model.Id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No issue activity found with the specified id."));

                    // entity = _mapper.Map(model, entity);
                    entity.IssueId = model.IssueId;
                    entity.ActivityName = model.ActivityName;
                    entity.DueDate = Convert.ToDateTime(model.DueDate);
                    entity.PriorityId = model.PriorityId;
                    entity.AssignedTo = model.AssignedTo;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _issueActivityService.UpdateIssueActivity(entity);
                }
                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteLeadActivityLogs
        [HttpDelete("{Activityid}/issueActivity")]
        public async Task<IActionResult> DeleteIssueActivity(string Activityid)
        {
            var entity = await _issueActivityService.GetById(Activityid);
            if (entity == null)
                return BadRequest(new BadRequestError("No lead activity found with the specified id."));

            _issueActivityService.DeleteIssueActivity(entity);

            return NoContent();
        }
        #endregion

        #region private functions for issues
        private async Task<bool> UpdateIssueDetails(string issueId, object data, string flag)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
            var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
            var statusLog = new IssueStatusChangedLog();

            var entity = await _issueService.GetIssueById(issueId);
            if (entity == null)
                return false;

            switch (flag.ToLower())
            {
                case "status":
                    if (data is string statusId)
                        entity.StatusId = statusId;

                    statusLog.StatusId = data.ToString();
                    statusLog.IssueId = entity.Id;
                    statusLog.StatusChangedBy = EmployeeId;
                    statusLog.StatusChangedDate = GetDateTime;
                    _issueStatusChangedLogService.InsertIssueStatusChangedLog(statusLog);

                    break;

                case "priority":
                    if (data is string priorityId)
                        entity.PriorityId = priorityId;
                    break;

                default:
                    return false;
            }

            entity.UpdatedById = LoggedUserId;
            entity.UpdatedOnUtc = GetDateTime;
            _issueService.UpdateIssue(entity);

            return true;
        }
        #endregion
    }
}
