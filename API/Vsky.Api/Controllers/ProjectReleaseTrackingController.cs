using System.Threading.Tasks;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Models;
using Vsky.Services.Sites;
using Vsky.Services.Common;
using System.Linq;
using Vsky.Api.ApiErrors;
using System.Collections.Generic;
using Vsky.Api.Models;
using Vsky.Services.DropDowns;
using Vsky.Services.AzureBlobImage;
using AngleSharp.Dom;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.ProjectReleaseTrackings;

namespace Vsky.Api.Controllers
{
    [Route("project-release-tracking")]
    public class ProjectReleaseTrackingController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IDropDownService _dropDownService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly IProjectReleaseTrackingService _projectReleaseTrackingService;
        private readonly IProjectReleaseTrackingReqPlanTaskIssueMappingService _projectReleaseTrackingReqPlanTaskIssueMappingService;
        private readonly IProjectReleaseTrackingStatusLogService _projectReleaseTrackingStatusLogService;
        #endregion

        #region Services Initializations
        public ProjectReleaseTrackingController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISiteService siteService,
            ICommonService commonService,
            IDropDownService dropDownService,
            ISitesModifiedLogsService sitesModifiedLogsService,
            IAzureBlobImageServices azureBlobImageServices,
            IProjectReleaseTrackingService projectReleaseTrackingService,
            IProjectReleaseTrackingReqPlanTaskIssueMappingService projectReleaseTrackingReqPlanTaskIssueMappingService,
            IProjectReleaseTrackingStatusLogService projectReleaseTrackingStatusLogService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _siteService = siteService;
            _commonService = commonService;
            _dropDownService = dropDownService;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _azureBlobImageServices = azureBlobImageServices;
            _projectReleaseTrackingService = projectReleaseTrackingService;
            _projectReleaseTrackingReqPlanTaskIssueMappingService = projectReleaseTrackingReqPlanTaskIssueMappingService;
            _projectReleaseTrackingStatusLogService = projectReleaseTrackingStatusLogService;
        }
        #endregion

        #region GetAllProjectReleaseTrackingList
        // Title: GetAllProjectReleaseTrackingList
        // Description: This endpoint retrieves the list of release tracking.
        [HttpPost("list")]
        public async Task<IActionResult> GetAllProjectReleaseTrackingList(ProjectReleaseTrackingSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of release tracking based on search criteria (name, sorting, pagination)
                var list = await _projectReleaseTrackingService.GetAllProjectReleaseTrackingList(
                    SiteId,
                    searchModel.SearchText,
                    LoggedUserId,
                    searchModel.ProjectIds,
                    searchModel.InfraInstanceIds,
                    searchModel.DeploymentOwnerIds,
                    searchModel.ApproverIds,
                    searchModel.TesterIds,
                    searchModel.ReleaseTypeIds,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new ProjectReleaseTrackingList
                {
                    ProjectReleaseTrackingsList = list,
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

        #region GetMappingByReleaseTrackingId
        // Title: GetMappingByReleaseTrackingId
        // Description: This endpoint retrieves the list of ReleaseTrackingReqPlanTaskIssueMapping By ReleaseTrackingId. 
        [HttpGet("get-mapping-by-releaseTrackingId")]
        public async Task<IActionResult> GetMappingByReleaseTrackingId(string releaseTrackingId)
        {
            try
            {
                var existingList = await _projectReleaseTrackingReqPlanTaskIssueMappingService.GetAllProjectReleaseTrackingReqPlanTaskIssueMappingByProjectReleaseTrackingId(releaseTrackingId);

                var result = existingList
                .Select(x =>
                {
                    var isReq = x.RequirementId != null;
                    var isIssue = x.IssueId != null;
                    var isTask = x.TaskId != null;
                    var isWeekly = x.WeeklyPlanId != null;
                    var isMonthly = x.MonthlyPlanId != null;

                    return new
                    {
                        RefId = x.RequirementId ?? x.IssueId ?? x.TaskId ?? x.WeeklyPlanId ?? x.MonthlyPlanId,

                        Type = isReq ? "requirement" :
                               isIssue ? "issue" :
                               isTask ? "task" : null,

                        Name = isReq ? x.Requirement?.Title :
                               isIssue ? x.Issue?.Name :
                               isTask ? x.Task?.Name : null,

                        Number = isReq ? x.Requirement?.RequirementNumber.ToString() :
                                 isIssue ? x.Issue?.IssueNumber.ToString() :
                                 isTask ? x.Task?.ProjectTaskNumber.ToString() : null,

                        Date = isReq ? x.Requirement?.CreatedOnUtc.ToString("MM/dd/yyyy") :
                               isIssue ? x.Issue?.CreatedOnUtc.ToString("MM/dd/yyyy") :
                               isTask ? x.Task?.CreatedOnUtc.ToString("MM/dd/yyyy") : null
                    };
                })
                .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        [HttpGet("get-all-req-plan-task-issues-by-project/{projectId}")]
        public async Task<IActionResult> GetAllReqPlanTaskIssuesByProjectId(string projectId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            if (string.IsNullOrEmpty(projectId))
                return BadRequest(new BadRequestError("projectId is required."));

            var result = await _projectReleaseTrackingReqPlanTaskIssueMappingService.GetAllReqPlanTaskIssuesByProjectId(projectId, SiteId);

            return Ok(result);
        }

        #region GetProjectReleaseTrackingInDetailsById
        // Title: GetProjectReleaseTrackingInDetailsById
        // Description: This endpoint retrieves the details of a specific release tracking based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetProjectReleaseTrackingInDetailsById(string id)
        {
            try
            {
                var entity = await _projectReleaseTrackingService.GetProjectReleaseTrackingInDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No release tracking found with the specified id."));

                var model = _mapper.Map<ProjectReleaseTracking>(entity);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GenerateVersionNumber
        [HttpGet("generate-version")]
        public async Task<IActionResult> GenerateVersionNumber(string projectId, string releaseType)
        {
            var version = await _projectReleaseTrackingService.GenerateVersionNumber(projectId, releaseType);
            return Ok(version);
        }
        #endregion

        #region CreateProjectReleaseTracking
        // Title: CreateProjectReleaseTracking
        // Description: This endpoint handles the creation of a new release tracking. It maps the release tracking model to the release tracking entity, sets the creation details, and inserts the release tracking into the database.
        [HttpPost]
        public async Task<IActionResult> CreateProjectReleaseTracking(SaveProjectReleaseTracking model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    string releaseTrackingId = Guid.NewGuid().ToString();

                    DateTime? plannedReleaseDate = null;
                    if (!string.IsNullOrEmpty(model.PlannedReleaseDateStr))
                    {
                        plannedReleaseDate = DateTime.ParseExact(model.PlannedReleaseDateStr, "MM/dd/yyyy", null);
                    }

                    var ReleaseTracking = new ProjectReleaseTracking
                    {
                        Id = releaseTrackingId,
                        SiteId = SiteId,
                        Name = model.Name,
                        ProjectId = model.ProjectId,
                        InfraInstanceId = model.InfraInstanceId,
                        DeploymentOwnerId = model.DeploymentOwnerId,
                        ApproverId = model.ApproverId,
                        TesterId = model.TesterId,
                        ReleaseTypeId = model.ReleaseTypeId,
                        VersionNumber = model.VersionNumber,
                        PlannedReleaseDate = plannedReleaseDate.Value,
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime,
                    };

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        ReleaseTracking.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "project release tracking",
                                ReleaseTracking.Id
                            );
                    }

                    _projectReleaseTrackingService.InsertProjectReleaseTracking(ReleaseTracking);

                    // Get Status
                    string statusName = null;
                    if (model.IsDraft.HasValue)
                        statusName = model.IsDraft.HasValue ? "Draft" : "In-Process";
                    else
                        statusName = "Draft";

                    var statusId = await _dropDownService
                        .GetDropDownByTypeNameAndName(SiteId, "Release Tracking Status", statusName);

                    var statusLog = new ProjectReleaseTrackingStatusLog
                    {
                        ReleaseTrackingId = ReleaseTracking.Id,
                        StatusId = statusId,
                        CreatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime
                    };

                    _projectReleaseTrackingStatusLogService.InsertProjectReleaseTrackingStatusLog(statusLog);
                    _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Release Tracking", ReleaseTracking.ProjectId, model.ProjectName, ReleaseTracking.Id, ReleaseTracking.Name, "Release Tracking Status", statusName, LoggedUserId, GetDateTime);

                    return Ok(ReleaseTracking);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateProjectReleaseTracking
        // Title: UpdateProjectReleaseTracking
        // Description: This endpoint updates an existing release tracking by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProjectReleaseTracking(string id, SaveProjectReleaseTracking model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    
                    var entity = await _projectReleaseTrackingService.GetProjectReleaseTrackingById(id);
                    // If no release tracking is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No project release tracking found with the specified id."));

                    DateTime? plannedReleaseDate = null;
                    if (!string.IsNullOrEmpty(model.PlannedReleaseDateStr))
                    {
                        plannedReleaseDate = DateTime.ParseExact(model.PlannedReleaseDateStr, "MM/dd/yyyy", null);
                    }
                    entity.Name = model.Name;
                    entity.ProjectId = model.ProjectId;
                    entity.InfraInstanceId = model.InfraInstanceId;
                    entity.DeploymentOwnerId = model.DeploymentOwnerId;
                    entity.ApproverId = model.ApproverId;
                    entity.TesterId = model.TesterId;
                    entity.ReleaseTypeId = model.ReleaseTypeId;
                    entity.VersionNumber = model.VersionNumber;
                    entity.PlannedReleaseDate = plannedReleaseDate.Value;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "project release tracking",
                                entity.Id,
                                entity.Description
                            );
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _projectReleaseTrackingService.UpdateProjectReleaseTracking(entity);

                    #region status logic
                    var latestStatus = await _projectReleaseTrackingStatusLogService.GetLatestStatusByProjectReleaseTrackingId(id);

                    var latestStatusName = latestStatus?.Status?.DropDownValue;
                    string newStatusName;

                    if (model.IsDraft.HasValue)
                    {
                        newStatusName = model.IsDraft.Value ? "Draft" : "In-Process";
                    }
                    else
                    {
                        // click Save & Next button 
                        if (latestStatus == null)
                        {
                            newStatusName = "Draft";
                        }
                        else
                        {
                            newStatusName = null;
                        }
                    }

                    bool shouldInsert = false;

                    if (newStatusName != null)
                    {
                        shouldInsert =
                            latestStatusName == null ||
                            (latestStatusName == "Draft" && newStatusName == "In-Process");
                    }

                    if (shouldInsert)
                    {
                        var statusId = await _dropDownService
                            .GetDropDownByTypeNameAndName(SiteId, "Release Tracking Status", newStatusName);

                        _projectReleaseTrackingStatusLogService.InsertProjectReleaseTrackingStatusLog(
                            new ProjectReleaseTrackingStatusLog
                            {
                                ReleaseTrackingId = id,
                                StatusId = statusId,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            });

                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Release Tracking", entity.ProjectId, model.ProjectName, entity.Id, entity.Name, "Release Tracking Status", newStatusName, LoggedUserId, GetDateTime);
                    }
                    #endregion

                    if (model.Tab == "2_tab")
                    {
                        var incomingList = model.ProjectReleaseTrackingReqPlanTaskIssueList ?? new List<SaveProjectReleaseTrackingReqPlanTaskIssueMapping>();

                        // Normalize incoming data (avoid ToLower multiple times)
                        var incomingSet = incomingList
                            .Select(x => new
                            {
                                RefId = x.RefId,
                                Type = x.Type.ToLower()
                            })
                            .ToHashSet();

                        // Get existing records
                        var existingList = await _projectReleaseTrackingReqPlanTaskIssueMappingService.GetAllProjectReleaseTrackingReqPlanTaskIssueMappingByProjectReleaseTrackingId(id);

                        // Convert existing to comparable set
                        var existingSet = existingList
                            .Select(x => new
                            {
                                RefId = x.RequirementId ?? x.IssueId ?? x.TaskId ?? x.WeeklyPlanId ?? x.MonthlyPlanId,
                                Type =
                                    x.RequirementId != null ? "requirement" :
                                    x.IssueId != null ? "issue" :
                                    x.TaskId != null ? "task" : ""
                            })
                            .ToHashSet();

                        // Add new items
                        var itemsToAdd = incomingSet
                            .Where(x => !existingSet.Contains(x))
                            .ToList();

                        foreach (var item in itemsToAdd)
                        {
                            var mappingEntity = new ProjectReleaseTrackingReqPlanTaskIssueMapping
                            {
                                Id = Guid.NewGuid().ToString(),
                                ReleaseTrackingId = id,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime
                            };

                            AssignType(mappingEntity, item);

                            _projectReleaseTrackingReqPlanTaskIssueMappingService.InsertProjectReleaseTrackingReqPlanTaskIssueMapping(mappingEntity);
                        }

                        var itemsToDelete = existingList
                        .Where(x =>
                        {
                            var refId =
                                x.RequirementId ??
                                x.IssueId ??
                                x.TaskId;

                            var type =
                                x.RequirementId != null ? "requirement" :
                                x.IssueId != null ? "issue" :
                                x.TaskId != null ? "task" : "";

                            // If NOT present in incoming delete
                            return !incomingSet.Contains(new
                            {
                                RefId = refId,
                                Type = type
                            });
                        })
                        .Select(x => x.Id)
                        .ToList();

                        foreach (var mappingId in itemsToDelete)
                        {
                            var mappingEntity = await _projectReleaseTrackingReqPlanTaskIssueMappingService.GetProjectReleaseTrackingReqPlanTaskIssueMappingById(mappingId);
                            _projectReleaseTrackingReqPlanTaskIssueMappingService.DeleteProjectReleaseTrackingReqPlanTaskIssueMapping(mappingEntity);
                        }
                    }

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

        #region Update Release Tracking Status
        [HttpPut("{id}/{statusId}")]
        public async Task<IActionResult> UpdateReleaseTrackingStatus(string id, string statusId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the Release Tracking entity by its ID
                    var entity = await _projectReleaseTrackingService.GetProjectReleaseTrackingInDetailsById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No release tracking found with the specified id."));

                    bool IsTaskStatusChanged = statusId != entity.StatusId;

                    if (IsTaskStatusChanged)
                    {
                        var statusLog = new ProjectReleaseTrackingStatusLog
                        {
                            ReleaseTrackingId = id,
                            StatusId = statusId,
                            CreatedById = LoggedUserId,
                            CreatedOnUtc = GetDateTime
                        };
                        _projectReleaseTrackingStatusLogService.InsertProjectReleaseTrackingStatusLog(statusLog);

                        var releaseTrackingStatus = await _dropDownService.GetDropDownById(statusId);
                        var status = releaseTrackingStatus.DropDownValue;

                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Release Tracking", entity.Project.Id, entity.Project.Name, entity.Id, entity.Name, "Release Tracking Status", status, LoggedUserId, GetDateTime);
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

        #region DeleteProjectReleaseTracking
        // Title: DeleteProjectReleaseTracking
        // Description: This endpoint deletes a release tracking based on the provided release tracking ID. It first retrieves the release tracking entity by ID, checks if it exists, and if so, deletes the release tracking. If the release tracking is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectReleaseTracking(string id)
        {
            try
            {
                // Fetch the release tracking entity by its ID
                var entity = await _projectReleaseTrackingService.GetProjectReleaseTrackingById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No release tracking found with the specified id."));

                _projectReleaseTrackingService.DeleteProjectReleaseTracking(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region private methods
        private void AssignType(ProjectReleaseTrackingReqPlanTaskIssueMapping entity, dynamic item)
        {
            entity.RequirementId = null;
            entity.IssueId = null;
            entity.TaskId = null;
            entity.WeeklyPlanId = null;
            entity.MonthlyPlanId = null;

            switch (item.Type)
            {
                case "requirement":
                    entity.RequirementId = item.RefId;
                    break;

                case "issue":
                    entity.IssueId = item.RefId;
                    break;

                case "task":
                    entity.TaskId = item.RefId;
                    break;

                case "weeklyplan":
                    entity.WeeklyPlanId = item.RefId;
                    break;

                case "monthlyplan":
                    entity.MonthlyPlanId = item.RefId;
                    break;

                default:
                    throw new Exception($"Invalid type: {item.Type}");
            }
        }    

        #endregion
    }
}
