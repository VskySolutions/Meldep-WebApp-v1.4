using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.Sites;
using Vsky.Services.SOPProcesses;
using static Dapper.SqlMapper;

namespace Vsky.Api.Controllers
{
    [Route("sop-process")]
    public class SOPProcessController : BaseController
    {
        #region Define Services and Initializations
        private readonly GlobalVariable _globalVariable;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly ISOPProcessService _sOPProcessService;
        private readonly ISOPProcessStatusLogService _sOPProcessStatusLogService;
        public SOPProcessController(
            GlobalVariable globalVariable,
            ICommonService commonService,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices,
            ISOPProcessService sOPProcessService,
            ISOPProcessStatusLogService sOPProcessStatusLogService)
        {
            _globalVariable = globalVariable;
            _commonService = commonService;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
            _sOPProcessService = sOPProcessService;
            _sOPProcessStatusLogService = sOPProcessStatusLogService;
        }
        #endregion

        [HttpPost("list")]
        public IActionResult GetAllSOPProcesses(SOPProcessSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var list = _sOPProcessService.GetAllSOPProcesses(searchModel.SearchText, SiteId, LoggedUserId, searchModel.Title, searchModel.IsActive, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                var model = new SOPProcessList
                {
                    SOPProcessesList = list,
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
        public async Task<IActionResult> GetSOPProcessByIdInDetail(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var entity = await _sOPProcessService.GetSOPProcessByIdInDetail(SiteId, id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No SOP Process Found"));

                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        #region CreateSOPProcess
        // Title: CreateSOPProcess
        // Description: This endpoint handles the creation of a new test plan. It maps the test plan model to the test plan entity, sets the creation details, and inserts the test plan into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateSOPProcess(SaveSOPProcess model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _sOPProcessService.GetSOPProcessByTitle(SiteId, model.Title);
                    if (exists != null)
                        return BadRequest(new BadRequestError("The sop process title already exists"));

                    var SOPProcessEntity = new SOPProcess
                    {
                        Id = Guid.NewGuid().ToString(),
                        SiteId = SiteId,
                        Title = model.Title,
                        CategoryId = model.CategoryId,
                        SubCategoryId = model.SubCategoryId,
                        ShortDescription = model.ShortDescription,
                        Purpose = model.Purpose,
                        Version = model.Version,
                        IsActive = model.IsActive,
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime,
                    };

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        SOPProcessEntity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "SOP-Process",
                                SOPProcessEntity.Id
                            );
                    }
                    _sOPProcessService.InsertSOPProcess(SOPProcessEntity);

                    if (!string.IsNullOrWhiteSpace(model.StatusId))
                    {
                        // Add Status Log
                        AddSOPProcessStatusLog(SOPProcessEntity.Id, model.StatusId, LoggedUserId, GetDateTime);
                    }

                    return Ok(SOPProcessEntity);
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

        #region UpdateSOPProcess
        // Title: UpdateSOPProcess
        // Description: This endpoint updates an existing SOP Process by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSOPProcess(string id, SaveSOPProcess model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = _sOPProcessService.GetSOPProcessById(SiteId, id);
                    // If no sop process is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No sop process found with the specified id."));

                    //Check if the sop process already exists
                    var exists = await _sOPProcessService.GetSOPProcessByTitle(SiteId, model.Title, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("sop process title already exists, try with another."));

                    entity.Title = model.Title;
                    entity.CategoryId = model.CategoryId;
                    entity.SubCategoryId = model.SubCategoryId;
                    entity.ShortDescription = model.ShortDescription;
                    entity.Purpose = model.Purpose;
                    entity.Version = model.Version;
                    entity.IsActive = model.IsActive;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "SOP-Process",
                                entity.Id,
                                entity.Description
                            );
                    }
                    _sOPProcessService.UpdateSOPProcess(entity);

                    var lastStatusId = entity.SOPProcessStatusLog?.Select(x => x.StatusId).FirstOrDefault();
                    bool IsSOPProcessStatusChanged = model.StatusId != lastStatusId;

                    if (IsSOPProcessStatusChanged)
                    {
                        // Add Status Log
                        AddSOPProcessStatusLog(entity.Id, model.StatusId, LoggedUserId, GetDateTime);
                    }
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region Update SOPProcess Status
        [HttpPut("{id}/{statusId}")]
        public async Task<IActionResult> UpdateSOPProcessStatus(string id, string statusId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the SOPProcess entity by its ID
                    var entity = _sOPProcessService.GetSOPProcessById(SiteId, id);

                    if (entity == null)
                        return BadRequest(new BadRequestError("No SOP process found with the specified id."));

                    // Update Status
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _sOPProcessService.UpdateSOPProcess(entity);

                    // Add Status Log
                    AddSOPProcessStatusLog(entity.Id, statusId, LoggedUserId, GetDateTime);

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

        #region Update SOPProcess Active/Inactive Status
        [HttpPut("update-active-status/{id}/{isActive}")]
        public async Task<IActionResult> UpdateSOPProcessActiveStatus(string id, bool isActive)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the SOPProcess entity by its ID
                    var entity = _sOPProcessService.GetSOPProcessById(SiteId, id);

                    if (entity == null)
                        return BadRequest(new BadRequestError("No SOP process found with the specified id."));

                    // Update Status
                    entity.IsActive = isActive;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _sOPProcessService.UpdateSOPProcess(entity);

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

        [HttpDelete("{id}")]
        public IActionResult DeleteSOPProcess(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                // Fetch the project entity by its ID
                var entity = _sOPProcessService.GetSOPProcessById(SiteId, id);
                // If no project is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No SOP process found with the specified id."));

                // Delete the project using the project service
                _sOPProcessService.DeleteSOPProcess(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region private methods
        private void AddSOPProcessStatusLog(
            string sOPProcessId,
            string statusId,
            string LoggedUserId,
            DateTime cDateTime
        )
        {
            var statusLog = new SOPProcessStatusLog
            {
                SOPProcessId = sOPProcessId,
                StatusId = statusId,
                CreatedById = LoggedUserId,
                CreatedOnUtc = cDateTime
            };
            _sOPProcessStatusLogService.InsertSOPProcessStatusLog(statusLog);
        }
        #endregion
    }
}
