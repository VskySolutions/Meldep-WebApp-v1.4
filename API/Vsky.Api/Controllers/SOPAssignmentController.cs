using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.Sites;
using Vsky.Services.SOPAssignments;

namespace Vsky.Api.Controllers
{
    [Route("sop-assignment")]
    public class SOPAssignmentController : BaseController
    {
        #region Define Services and Initializations
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly ISOPAssignmentService _sOPAssignmentService;
        private readonly ISOPAssignmentResponseService _sOPAssignmentResponseService;
        private readonly ISOPAssignmentResponseEvidencesService _sOPAssignmentResponseEvidencesService;
        public SOPAssignmentController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ICommonService commonService,
            IDropDownService dropDownService,
            IDropDownTypeService dropDownTypeService,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices,
            ISOPAssignmentService sOPAssignmentService,
            ISOPAssignmentResponseService sOPAssignmentResponseService,
            ISOPAssignmentResponseEvidencesService sOPAssignmentResponseEvidencesService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _commonService = commonService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
            _sOPAssignmentService = sOPAssignmentService;
            _sOPAssignmentResponseService = sOPAssignmentResponseService;
            _sOPAssignmentResponseEvidencesService = sOPAssignmentResponseEvidencesService;
        }
        #endregion

        [HttpPost("list")]
        public IActionResult GetAllSOPAssignments(SOPAssignmentSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                //var LoggeduserEmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var LoggeduserEmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                var list = _sOPAssignmentService.GetAllSOPAssignments(
                    searchModel.SearchText,
                    SiteId,
                    LoggedUserId,
                    LoggeduserEmployeeId,
                    searchModel.TemplateIds,
                    searchModel.AssignedToEmployeeIds,
                    searchModel.ApproverEmployeeIds,
                    searchModel.StatusIds,
                    searchModel.PriorityIds,
                    searchModel.Name,
                    searchModel.IsApproved,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = new SOPAssignmentListModel
                {
                    Data = _mapper.Map<IList<SOPAssignmentModel>>(list),
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
        public IActionResult GetSOPAssignmentByIdInDetail(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var entity = _sOPAssignmentService.GetSOPAssignmentByIdInDetail(SiteId, id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No SOP assignment Found"));

                var model = _mapper.Map<SOPAssignmentModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost()]
        public async Task<IActionResult> CreateUpdateSOPAssignment(SOPAssignmentModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the sop assignment already exists
                    var exists = await _sOPAssignmentService.GetSOPAssignmentByName(SiteId, model.Name);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Name already exists, try with another."));

                    var statusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "SOP Assignment Status", "In Progress");

                    var assignment = new SOPAssignment();
                    assignment.SiteId = SiteId;
                    assignment.TemplateId = model.TemplateId;
                    assignment.AssignedToEmployeeId = model.AssignedToEmployeeId;
                    assignment.ApproverEmployeeId = model.ApproverEmployeeId;
                    assignment.StatusId = statusId;
                    assignment.PriorityId = model.PriorityId;
                    assignment.Name = model.Name;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        assignment.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "SOP-assignment",
                                assignment.Id,
                                assignment.Description
                            );
                    }

                    // Assigned Date
                    if (model.AssignedDateStr != "" && model.AssignedDateStr != null)
                        assignment.AssignedDate = DateTime.ParseExact(model.AssignedDateStr, "MM/dd/yyyy", null);

                    if (model.DueDateStr != "" && model.DueDateStr != null)
                        assignment.DueDate = DateTime.ParseExact(model.DueDateStr, "MM/dd/yyyy", null);

                    assignment.CreatedById = LoggedUserId;
                    assignment.UpdatedById = LoggedUserId;
                    assignment.CreatedOnUtc = GetDateTime;
                    assignment.UpdatedOnUtc = GetDateTime;
                    _sOPAssignmentService.InsertSOPAssignment(assignment);

                    return Ok();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost("save-assignment-responce")]
        public async Task<IActionResult> SaveSOPAssignmentResponses([FromForm] SOPAssignmentModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (model.IsSubmitted) 
                {
                    var submittedStatusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "SOP Assignment Status", "Submitted");
                    var assignment = _sOPAssignmentService.GetSOPAssignmentById(SiteId, model.Id);

                    if (assignment == null)
                        return BadRequest(new BadRequestError("No SOP assignment Found"));

                    assignment.StatusId = submittedStatusId;
                    assignment.UpdatedById = LoggedUserId;
                    assignment.UpdatedOnUtc = GetDateTime;
                    _sOPAssignmentService.UpdateSOPAssignment(assignment);
                }

                foreach (var item in model.SOPAssignmentResponses)
                {
                    SOPAssignmentResponse responseEntity;

                    // Update
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        responseEntity = await _sOPAssignmentResponseService
                            .GetSOPAssignmentResponseById(item.Id);

                        if (responseEntity == null)
                            continue;

                        responseEntity.SectionItemId = item.SectionItemId;
                        responseEntity.Response = item.Response = item.Response ?? null;
                        responseEntity.IsChecked = item.IsChecked;
                        responseEntity.UpdatedOnUtc = GetDateTime;
                        responseEntity.UpdatedById = LoggedUserId;

                        _sOPAssignmentResponseService.UpdateSOPAssignmentResponse(responseEntity);
                    }
                    else
                    {
                        // Insert
                        responseEntity = new SOPAssignmentResponse
                        {
                            Id = Guid.NewGuid().ToString(),
                            AssignementId = model.Id,
                            SectionItemId = item.SectionItemId,
                            Response = item.Response = item.Response ?? null,
                            IsChecked = item.IsChecked,
                            CreatedOnUtc = GetDateTime,
                            CreatedById = LoggedUserId,
                            UpdatedOnUtc = GetDateTime,
                            UpdatedById = LoggedUserId
                        };

                        _sOPAssignmentResponseService.InsertSOPAssignmentResponse(responseEntity);
                    }

                    var dbEvidences = await _sOPAssignmentResponseEvidencesService
                        .GetSOPAssignmentResponseEvidenceByResponseId(responseEntity.Id);

                    if (item.DeletedFiles != null && item.DeletedFiles.Any())
                    {
                        foreach (var deletedJson in item.DeletedFiles)
                        {
                            var deleted = JsonConvert.DeserializeObject<DeletedFileModel>(deletedJson);

                            var dbFile = dbEvidences.FirstOrDefault(x => x.Id == deleted.id);
                            if (dbFile != null)
                            {
                                var picture = await _commonService.GetByPictureId(dbFile.FileId);

                                if (picture != null && !string.IsNullOrEmpty(picture.VirtualPath))
                                {
                                    await _azureBlobImageServices.DeleteImage(picture.VirtualPath);
                                    _commonService.DeletePicture(picture);
                                }

                                _sOPAssignmentResponseEvidencesService.DeleteSOPAssignmentResponseEvidence(dbFile);
                            }
                        }
                    }

                    if (item.FileChangeFlag == "remove")
                    {
                        foreach (var dbFile in dbEvidences)
                        {
                            var picture = await _commonService.GetByPictureId(dbFile.FileId);

                            if (picture != null && !string.IsNullOrEmpty(picture.VirtualPath))
                            {
                                await _azureBlobImageServices.DeleteImage(picture.VirtualPath);
                                _commonService.DeletePicture(picture);
                            }

                            _sOPAssignmentResponseEvidencesService.DeleteSOPAssignmentResponseEvidence(dbFile);
                        }
                    }

                    if (item.EvidenceFiles != null && item.EvidenceFiles.Any())
                    {
                        var urls = await _azureBlobImageServices.UploadFilesAsync(
                            SiteData.Name,
                            "sop-assignment",
                            item.EvidenceFiles.ToList(),
                            responseEntity.Id
                        );

                        for (int i = 0; i < urls.Count; i++)
                        {
                            var file = item.EvidenceFiles[i];

                            var picture = new Picture
                            {
                                SiteId = SiteId,
                                Type = "SOPAssignmentResponseEvidences",
                                SeoFilename = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType?.Length > 50
                                    ? file.ContentType.Substring(0, 50)
                                    : file.ContentType,
                                VirtualPath = urls[i],
                                ModuleId = responseEntity.Id,
                                Module = model.Name,
                                SubModuleId = responseEntity.Id,
                                Sub_Module = model.Name,
                                CreatedOnUtc = GetDateTime,
                                CreatedById = LoggedUserId
                            };

                            _commonService.InsertPicture(picture);

                            var evidence = new SOPAssignmentResponseEvidences
                            {
                                Id = Guid.NewGuid().ToString(),
                                ResponseId = responseEntity.Id,
                                FileId = picture.Id,
                                CreatedOnUtc = GetDateTime,
                                CreatedById = LoggedUserId
                            };

                            _sOPAssignmentResponseEvidencesService.InsertSOPAssignmentResponseEvidence(evidence);
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("save-review")]
        public async Task<IActionResult> SubmitReview(SOPAssignmentModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var assignment = _sOPAssignmentService.GetSOPAssignmentById(SiteId, model.Id);

                if (assignment == null)
                    return BadRequest(new BadRequestError("No SOP assignment Found"));

                if(model.IsApproved)
                {
                    var approvedStatusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "SOP Assignment Status", "Approved");
                    assignment.StatusId = approvedStatusId;
                }
                else
                {
                    var approvedStatusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "SOP Assignment Status", "Rejected");
                    assignment.StatusId = approvedStatusId;
                }

                assignment.IsApproved = model.IsApproved;
                assignment.ApprovedDate = GetDateTime;
                assignment.UpdatedById = LoggedUserId;
                assignment.UpdatedOnUtc = GetDateTime;
                _sOPAssignmentService.UpdateSOPAssignment(assignment);

                foreach (var item in model.SOPAssignmentResponses)
                {
                    SOPAssignmentResponse responseEntity;

                    // Update
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        responseEntity = await _sOPAssignmentResponseService.GetSOPAssignmentResponseById(item.Id);

                        if (responseEntity == null)
                            continue;

                        responseEntity.ApprovedComment = (!string.IsNullOrWhiteSpace(item.ApprovedComment)) ? item.ApprovedComment : null;
                        responseEntity.IsApproved = item.IsApproved;
                        responseEntity.UpdatedOnUtc = GetDateTime;
                        responseEntity.UpdatedById = LoggedUserId;

                        _sOPAssignmentResponseService.UpdateSOPAssignmentResponse(responseEntity);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region UpdateSOPAssignment
        // Title: UpdateSOPAssignment
        // Description: This endpoint updates an existing SOP Assignment by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSOPAssignment(string id, SOPAssignmentModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the assignment entity by its ID
                    var entity = _sOPAssignmentService.GetSOPAssignmentById(SiteId, id);
                    // If no test plan is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No SOP assignment found with the specified id."));

                    var exists = await _sOPAssignmentService.GetSOPAssignmentByName(SiteId, model.Name, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("The SOP assignment already exists"));

                    entity.TemplateId = model.TemplateId;
                    entity.AssignedToEmployeeId = model.AssignedToEmployeeId;
                    entity.ApproverEmployeeId = model.ApproverEmployeeId;
                    entity.PriorityId = model.PriorityId;
                    entity.Name = model.Name;

                    if (model.AssignedDateStr != "" && model.AssignedDateStr != null)
                        entity.AssignedDate = DateTime.ParseExact(model.AssignedDateStr, "MM/dd/yyyy", null);

                    if (model.DueDateStr != "" && model.DueDateStr != null)
                        entity.DueDate = DateTime.ParseExact(model.DueDateStr, "MM/dd/yyyy", null);

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "SOP-assignment",
                                entity.Id,
                                entity.Description
                            );
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _sOPAssignmentService.UpdateSOPAssignment(entity);

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

        #region DeleteSOPAssignment
        // Title: DeleteSOPAssignmentById
        // Description: This endpoint deletes a SOPAssignment based on the provided SOPAssignment ID. It first retrieves the SOPAssignment entity by ID, checks if it exists, and if so, deletes the SOPAssignment. If the SOPAssignment is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public IActionResult DeleteSOPAssignment(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                // Fetch the SOPAssignment entity by its ID
                var entity = _sOPAssignmentService.GetSOPAssignmentById(SiteId, id);
                // If no SOPAssignment is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No SOP assignment found with the specified id."));

                // Delete the SOPAssignment using the SOPAssignment service
                _sOPAssignmentService.DeleteSOPAssignment(entity);

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
