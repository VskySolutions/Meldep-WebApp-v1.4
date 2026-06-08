using System.Collections.Generic;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Services.Candidates;
using Vsky.Services.Common;
using Vsky.Services.Sites;
using System.Linq;
using Vsky.Models;
using System.Threading.Tasks;
using Vsky.Api.ApiErrors;
using Vsky.Services.Persons;
using System.IO;
using System.Text.RegularExpressions;
using Vsky.Services.Note;
using Vsky.Services.DropDowns;
using Microsoft.AspNetCore.Authorization;
using Vsky.Services.Users;
using Vsky.Services.Messages;
using StackExchange.Profiling.Internal;
using Vsky.Services.AzureBlobImage;
using Microsoft.AspNetCore.Http;

namespace Vsky.Api.Controllers
{
    [Route("candidate")]
    public class CandidateController : BaseController
    {
        #region Define services
        private readonly IMapper _mapper;
        private readonly GlobalVariable _globalVariable;
        private readonly ICandidateService _candidateService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IPersonService _personService;
        private readonly ICandidateDepartmentService _candidateDepartmentService;
        private readonly INoteService _noteService;
        private readonly ICandidateActivityService _candidateActivityService;
        private readonly IDropDownService _dropDownService;
        private readonly IUserService _userService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICandidateNoteService _candidateNoteService;
        private readonly ICandidateFeedbacksService _candidateFeedbacksService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations
        public CandidateController(
            IMapper mapper,
            GlobalVariable globalVariable,
            ICandidateService candidateService,
            ICommonService commonService,
            ISiteService siteService,
            IPersonService personService,
            ICandidateDepartmentService candidateDepartmentService,
            INoteService noteService,
            ICandidateActivityService candidateActivityService,
            IDropDownService dropDownService,
            IUserService userService,
            IWorkflowMessageService workflowMessageService,
            ICandidateNoteService candidateNoteService,
            ICandidateFeedbacksService candidateFeedbacksService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _candidateService = candidateService;
            _siteService = siteService;
            _personService = personService;
            _candidateDepartmentService = candidateDepartmentService;
            _noteService = noteService;
            _candidateActivityService = candidateActivityService;
            _dropDownService = dropDownService;
            _userService = userService;
            _workflowMessageService = workflowMessageService;
            _candidateNoteService = candidateNoteService;
            _commonService = commonService;
            _candidateFeedbacksService = candidateFeedbacksService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllCandidateList
        [HttpPost("list")]
        public IActionResult GetAllCandidateList(CandidateSearchModel candidateSearchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _candidateService.GetAllCandidateList(SiteId, candidateSearchModel.SearchText, candidateSearchModel.FullName, candidateSearchModel.EmailAddress, candidateSearchModel.MobileNumber, candidateSearchModel.AppliedWorkLocationId, candidateSearchModel.JobId, candidateSearchModel.FromDate, candidateSearchModel.ToDate, candidateSearchModel.SortBy, candidateSearchModel.Descending, candidateSearchModel.Page, candidateSearchModel.PageSize);

                var model = new CandidateListModel
                {
                    Data = _mapper.Map<IList<CandidateModels>>(list),
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

        #region GetCandidateDetailsById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCandidateDetailsById(string id)
        {
            try
            {
                // Fetch the candidate entity by its ID from the service
                var entity = await _candidateService.GetCandidateDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No candidate found with the specified id."));

                // Map the candidate entity to a CandidateModels object
                var model = _mapper.Map<CandidateModels>(entity);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region create candidate
        [HttpPost]
        public async Task<IActionResult> CreateCandidate([FromForm] CandidateModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _personService.GetPersonByEmail(model.EmailAddress);
                    if (exists != null)
                        return BadRequest(new BadRequestError("A candidate with this email already exists. Please use a different email."));

                    //Map in person
                    var personEntity = new Person();
                    //Add Address
                    string AddressId = _commonService.AddUpdateAddress(model.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, LoggedUserId);

                    //Add in person
                    personEntity.AddressId = AddressId;
                    personEntity.FirstName = model.FirstName;
                    personEntity.MiddleName = model.MiddleName;
                    personEntity.LastName = model.LastName;
                    personEntity.PrimaryEmailAddress = model.EmailAddress;
                    personEntity.PrimaryPhoneNumber = model.MobileNumber;
                    personEntity.CreatedById = LoggedUserId;
                    personEntity.UpdatedById = LoggedUserId;
                    personEntity.CreatedOnUtc = GetDateTime;
                    personEntity.UpdatedOnUtc = GetDateTime;
                    _personService.InsertPerson(personEntity);

                    // Map the site to person
                    var sites = new PersonSitesMapping();
                    sites.PersonId = personEntity.Id;
                    sites.SiteId = SiteId;
                    sites.CreatedById = LoggedUserId;
                    sites.UpdatedById = LoggedUserId;
                    sites.CreatedOnUtc = GetDateTime;
                    sites.UpdatedOnUtc = GetDateTime;
                    _personService.InsertPersonSites(sites);

                    var Status = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Candidate Status", "New Application");

                    // Map the candidate model to the candidate entity
                    var entity = _mapper.Map<Candidate>(model);

                    entity.Id = Guid.NewGuid().ToString();
                    entity.SiteId = SiteId;
                    entity.AddressId = AddressId;
                    entity.PersonId = personEntity.Id;
                    entity.JobId = model.JobId;
                    entity.StatusId = Status;

                    if (!string.IsNullOrEmpty(model.ExperienceDetails))
                    {
                        entity.ExperienceDetails = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.ExperienceDetails,
                                SiteData.Name,
                                "candidate",
                                entity.Id
                            );
                    }

                    if (model.FilePic != null && model.FilePic.Length > 0)
                    {
                        var allowedFileTypes = new[] {
                            "image/jpeg", "image/png", "application/pdf",
                            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                        };

                        var file = model.FilePic;
                        if (!allowedFileTypes.Contains(file.ContentType))
                        {
                            return BadRequest(new BadRequestError("Invalid file type."));
                        }

                        var originalFileName = Path.GetFileName(file.FileName);
                        var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                        var files = new List<IFormFile> { model.FilePic };
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "candidate", files, entity.Id);

                        if (urls != null && urls.Any())
                        {
                            foreach (var blobUrl in urls)
                            {
                                var picture = new Picture
                                {
                                    SeoFilename = originalFileName,
                                    MimeType = mimeType,
                                    VirtualPath = blobUrl,
                                    ModuleId = entity.Id,
                                    Module = $"{model.FirstName} {model.LastName}",
                                    SubModuleId = entity.Id,
                                    Sub_Module = $"{model.FirstName} {model.LastName}",
                                    Type = "Candidate",
                                    SiteId = SiteId,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };

                                _commonService.InsertPicture(picture);

                                entity.CandidateResumeFileId = picture.Id;
                            }
                        }
                    }
                    else
                    {
                        entity.CandidateResumeFileId = null;
                    }

                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _candidateService.InsertCandidate(entity);

                    // department mapping
                    if (model.DepartmentIdsArray != null && model.DepartmentIdsArray.Count() > 0)
                    {
                        foreach (var item in model.DepartmentIdsArray)
                        {
                            if (item != null)
                            {
                                var candidateDepartments = new CandidateDepartments
                                {
                                    CandidateId = entity.Id,
                                    DepartmentsId = item
                                };
                                _candidateDepartmentService.InsertCandidateDepartment(candidateDepartments);
                            }
                        }
                    }

                    var candidateDetails = await _candidateService.GetCandidateDetailsById(entity.Id);

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

        #region UpdateCandidate
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCandidate(string id, [FromForm] CandidateModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _candidateService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No candidate found with the specified id."));

                    //Update in person
                    var personEntity = await _personService.GetPersonById(entity.PersonId);
                    if (personEntity != null)
                    {
                        var existsEmail = await _personService.GetPersonByEmail(model.EmailAddress);
                        if (existsEmail != null && existsEmail.Id != personEntity.Id)
                            return BadRequest(new BadRequestError("A candidate with this email already exists. Please use a different email."));

                        personEntity.FirstName = model.FirstName;
                        personEntity.MiddleName = model.MiddleName;
                        personEntity.LastName = model.LastName;
                        personEntity.PrimaryEmailAddress = model.EmailAddress;
                        personEntity.PrimaryPhoneNumber = model.MobileNumber;
                        personEntity.UpdatedById = LoggedUserId;
                        personEntity.UpdatedOnUtc = GetDateTime;
                        _personService.UpdatePerson(personEntity);
                    }

                    //Update Address
                    string AddressId = _commonService.AddUpdateAddress(entity.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, LoggedUserId);

                    // update candidate
                    entity.AddressId = AddressId;
                    entity.JobId = model.JobId;
                    entity.Source = model.Source;
                    entity.JobApplyDate = model.JobApplyDate;
                    entity.ReferenceName = model.ReferenceName;
                    entity.EnglishFluencyId = model.EnglishFluencyId;
                    entity.Qualification = model.Qualification;
                    entity.AppliedWorkLocationId = model.AppliedWorkLocationId;
                    entity.AppliedJobPositionId = model.AppliedJobPositionId;
                    entity.ExperienceYears = model.ExperienceYears;
                    entity.ExperienceMonths = model.ExperienceMonths;
                    entity.ExpectedSalaryFrom = model.ExpectedSalaryFrom;
                    entity.ExpectedSalaryTo = model.ExpectedSalaryTo;
                    entity.RecruiterId = model.RecruiterId;
                    entity.AvailabilityWorkId = model.AvailabilityWorkId;
                    entity.IsTransportration = model.IsTransportration;
                    entity.IsReadyToRelocate = model.IsReadyToRelocate;
                    entity.IsCandidateOwnSystem = model.IsCandidateOwnSystem;
                    entity.IsExperienced = model.IsExperienced;

                    if (!string.IsNullOrEmpty(model.ExperienceDetails))
                    {
                        entity.ExperienceDetails = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.ExperienceDetails,
                                SiteData.Name,
                                "candidate",
                                entity.Id,
                                entity.ExperienceDetails
                            );
                    }

                    //upload candidate resume file
                    var candidateResumeFileId = "";
                    if (model.FileChangeFlag == "edit")
                    {
                        // Delete old image first
                        if (!string.IsNullOrEmpty(entity.CandidateResumeFileId))
                        {
                            var oldPicture = await _commonService.GetByPictureId(entity.CandidateResumeFileId);

                            if (oldPicture != null && !string.IsNullOrEmpty(oldPicture.VirtualPath))
                            {
                                await _azureBlobImageServices.DeleteImage(oldPicture.VirtualPath);

                                _commonService.DeletePicture(oldPicture);
                            }
                        }

                        if (model.FilePic != null && model.FilePic.Length > 0)
                        {
                            var allowedFileTypes = new[] {
                            "image/jpeg", "image/png", "application/pdf",
                            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                        };

                            var file = model.FilePic;
                            if (!allowedFileTypes.Contains(file.ContentType))
                            {
                                return BadRequest(new BadRequestError("Invalid file type."));
                            }

                            var originalFileName = Path.GetFileName(file.FileName);
                            var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                            var files = new List<IFormFile> { model.FilePic };
                            var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "candidate", files, entity.Id);

                            if (urls != null && urls.Any())
                            {
                                foreach (var blobUrl in urls)
                                {
                                    var picture = new Picture
                                    {
                                        SeoFilename = originalFileName,
                                        MimeType = mimeType,
                                        VirtualPath = blobUrl,
                                        ModuleId = entity.Id,
                                        Module = $"{model.FirstName} {model.LastName}",
                                        SubModuleId = entity.Id,
                                        Sub_Module = $"{model.FirstName} {model.LastName}",
                                        Type = "Candidate",
                                        SiteId = SiteId,
                                        CreatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime
                                    };

                                    _commonService.InsertPicture(picture);

                                    candidateResumeFileId = picture.Id;
                                }
                            }
                        }
                    }
                    else if (model.FileChangeFlag == "remove")
                    {
                        if (!string.IsNullOrEmpty(entity.CandidateResumeFileId))
                        {
                            var oldPicture = await _commonService.GetByPictureId(entity.CandidateResumeFileId);

                            if (oldPicture != null && !string.IsNullOrEmpty(oldPicture.VirtualPath))
                            {
                                await _azureBlobImageServices.DeleteImage(oldPicture.VirtualPath);

                                _commonService.DeletePicture(oldPicture);
                            }
                        }

                        entity.CandidateResumeFileId = null;
                    }
                    //update entry
                    if (candidateResumeFileId != "")
                        entity.CandidateResumeFileId = candidateResumeFileId;

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _candidateService.UpdateCandidate(entity);

                    // department mapping
                    var existingMappings = _candidateDepartmentService.GetCandidateDepartmentById(entity.Id);
                    if (existingMappings != null)
                    {
                        foreach (var existingMapping in existingMappings)
                        {
                            if (!model.DepartmentIdsArray.Contains(existingMapping.DepartmentsId))
                            {
                                // Remove the mapping if the department was deselected
                                _candidateDepartmentService.DeleteCandidateDepartment(existingMapping);
                            }
                        }
                    }

                    if (model.DepartmentIdsArray != null && model.DepartmentIdsArray.Count() > 0)
                    {
                        foreach (var item in model.DepartmentIdsArray)
                        {
                            if (item != null)
                            {
                                var existingMapping = existingMappings.FirstOrDefault(x => x.DepartmentsId == item);
                                if (existingMapping != null)
                                {
                                    existingMapping.DepartmentsId = item;
                                    _candidateDepartmentService.UpdateCandidateDepartment(existingMapping);
                                }
                                else
                                {
                                    var candidateDepartments = new CandidateDepartments
                                    {
                                        CandidateId = entity.Id,
                                        DepartmentsId = item
                                    };
                                    _candidateDepartmentService.InsertCandidateDepartment(candidateDepartments);
                                }
                            }
                        }
                    }

                    return Ok(id);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        //create for update candidate status from candidate list -by SN
        #region update candidate status
        [HttpPut("{id}/{statusId}")]
        public async Task<IActionResult> UpdateCandidateStatus(string id, string statusId)
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
                    var entity = await _candidateService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No candidate found with the specified id."));

                    entity.StatusId = statusId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _candidateService.UpdateCandidate(entity);

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

        #region DeleteCandidate
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidate(string id)
        {
            try
            {
                var entity = await _candidateService.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No candidate found with the specified id."));

                _candidateService.DeleteCandidate(entity);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllActivities
        [HttpGet("candidateactivities-list")]
        public async Task<IActionResult> GetAllActivities()
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _candidateActivityService.GetAllActivities(SiteId);
            var model = _mapper.Map<IList<CandidateActivitiesModels>>(list);

            return Ok(model);
        }
        #endregion

        #region GetCandidateActivityLogById
        [HttpGet("{id}/candidateActivity")]
        public async Task<IActionResult> GetCandidateActivityLogById(string id)
        {
            var entity = await _candidateActivityService.GetById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No candidate activity found with the specified id."));

            var model = _mapper.Map<List<CandidateActivitiesModels>>(entity);
            return Ok(model);
        }
        #endregion

        #region GetActivityById
        [HttpGet("{id}/Activitylog")]
        public async Task<IActionResult> GetActivityLogById(string id)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var entity = _candidateActivityService.GetCandidateDetailsById(SiteId, id);
            if (entity == null)
                return BadRequest(new BadRequestError("No candidate activity log found with the specified id."));

            var model = _mapper.Map<List<CandidateActivitiesModels>>(entity);
            return Ok(model);
        }
        #endregion

        #region Add candidate activity
        [HttpPost("candidateActivityLogs")]
        public async Task<IActionResult> CreateCandidateActivityLogs(CandidateActivitiesModels model)
        {
            if(ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (model.Id == null)
                {
                    var entity = _mapper.Map<CandidateActivities>(model);
                    entity.Id = Guid.NewGuid().ToString();
                    entity.CandidateId = model.CandidateId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _candidateActivityService.InsertCandidateActivityLogs(entity);
                }
                else
                {
                    var entity = await _candidateActivityService.GetById(model.Id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No candidate activity found with the specified id."));

                    entity.ActivityName = model.ActivityName;
                    entity.PriorityId = model.PriorityId;
                    entity.EmployeeOwnerId = model.EmployeeOwnerId;
                    entity.DueDate = model.DueDate;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _candidateActivityService.UpdateCandidateActivityLogs(entity);
                }
                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteCandidateActivityLogs
        [HttpDelete("{Activityid}/candidateactivity")]
        public async Task<IActionResult> DeleteCandidateActivityLogs(string Activityid)
        {
            var entity = await _candidateActivityService.GetById(Activityid);
            if (entity == null)
                return BadRequest(new BadRequestError("No candidate activity found with the specified id."));

            _candidateActivityService.DeleteCandidateActivityLogs(entity);

            return NoContent();
        }
        #endregion

        #region GetCandidateFeedbackById
        [HttpGet("{id}/candidateFeedback")]
        public async Task<IActionResult> GetCandidateFeedbackById(string id)
        {
            var entity = await _candidateFeedbacksService.GetCandidateFeedbackById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No candidate feedback found with the specified id."));

            var model = _mapper.Map<List<CandidateFeedbackModel>>(entity);
            return Ok(model);
        }
        #endregion

        #region GetCandidateFeedbackDetailsById
        [HttpGet("{id}/candidateFeedbackLog")]
        public async Task<IActionResult> GetCandidateFeedbackDetailsById(string id)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var entity = _candidateFeedbacksService.GetCandidateFeedbackDetailsById(SiteId, id);
            if (entity == null)
                return BadRequest(new BadRequestError("No candidate feedback log found with the specified id."));

            var model = _mapper.Map<List<CandidateFeedbackModel>>(entity);
            return Ok(model);
        }
        #endregion

        #region CreateCandidateFeedback
        [HttpPost("createCandidateFeedback")]
        public async Task<IActionResult> CreateCandidateFeedback(CandidateFeedbackModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (model.Id == null)
                {
                    var entity = _mapper.Map<CandidateFeedback>(model);
                    entity.Id = Guid.NewGuid().ToString();
                    entity.CandidateId = model.CandidateId;

                    if (!string.IsNullOrEmpty(model.Answer))
                    {
                        entity.Answer = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Answer,
                                SiteData.Name,
                                "candidate",
                                entity.Id,
                                entity.Answer
                            );
                    }

                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _candidateFeedbacksService.InsertCandidateFeedbacks(entity);
                }
                else
                {
                    var existCandidate = await _candidateFeedbacksService.GetCandidateFeedbackById(model.Id);
                    if (existCandidate == null)
                        return BadRequest(new BadRequestError("No candidate feedback found with the specified id."));

                    existCandidate.EmployeeOwnerId = model.EmployeeOwnerId;
                    existCandidate.QuestionId = model.QuestionId;
                    existCandidate.DueDate = model.DueDate;

                    if (!string.IsNullOrEmpty(model.Answer))
                    {
                        existCandidate.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Answer,
                                SiteData.Name,
                                "candidate",
                                existCandidate.Id,
                                existCandidate.Answer
                            );
                    }

                    existCandidate.UpdatedById = LoggedUserId;
                    existCandidate.UpdatedOnUtc = GetDateTime;
                    _candidateFeedbacksService.UpdateCandidateFeedbacks(existCandidate);
                }
                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteCandidateFeedback
        [HttpDelete("{feedbackId}/deleteCandidateFeedback")]
        public async Task<IActionResult> DeleteCandidateFeedback(string feedbackId)
        {
            var entity = await _candidateFeedbacksService.GetCandidateFeedbackById(feedbackId);
            if (entity == null)
                return BadRequest(new BadRequestError("No candidate feedback found with the specified id."));

            _candidateFeedbacksService.DeleteCandidateFeedbacks(entity);

            return NoContent();
        }
        #endregion

        #region candidate register from VskyApplication and Hr-Lens

        [AllowAnonymous]
        [HttpGet("check-email/{emailAddress}")]

        public async Task<IActionResult> CheckCandidateEmailAlreadyExist(string emailAddress)
        {
            // Check email in Person table
            var person = await _personService.GetPersonByEmail(emailAddress);
            if (person == null)
            {
                return Ok(new
                {
                    Exists = false,
                    CandidateId = (string?)null
                });
            }

            // Get candidate using PersonId
            var candidate = await _candidateService.GetCandidateByPersonId(person.Id);

            if (candidate == null)
            {
                return Ok(new
                {
                    Exists = true,
                    CandidateId = (string?)null
                });
            }

            return Ok(new
            {
                Exists = true,
                CandidateId = candidate.Id
            });
        }

        [AllowAnonymous]
        [HttpPost("register-candidate")]
        public async Task<IActionResult> RegisterCandidate([FromForm] CandidateModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var SiteId = "04086f10-8f09-4e0c-b5c0-c827a244addd";
                    var migrateUserId = _userService.GetIdByMigrateUser("migrate"); //get migrate user id
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    
                    var exists = await _personService.GetPersonByEmail(model.EmailAddress);
                    if (exists != null)
                        return BadRequest(new BadRequestError("A candidate with this email already exists. Please use a different email."));

                    var personEntity = new Person();

                    string addressId = null;
                    if (!string.IsNullOrWhiteSpace(model.AddressLine1) &&
                        !string.IsNullOrWhiteSpace(model.StateProvinceId) &&
                        !string.IsNullOrWhiteSpace(model.CountryId) &&
                        !string.IsNullOrWhiteSpace(model.City) &&
                        !string.IsNullOrWhiteSpace(model.ZipCode))
                    {
                        addressId = _commonService.AddUpdateAddress(model.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, migrateUserId);
                    }

                    var existingPerson = await _personService.GetPersonByEmailAddress(model.EmailAddress, null, SiteId);
                    if (existingPerson != null)
                    {
                        if (!string.IsNullOrWhiteSpace(addressId))
                            existingPerson.AddressId = addressId;

                        //Update in person
                        existingPerson.FirstName = model.FirstName;
                        existingPerson.MiddleName = model.MiddleName;
                        existingPerson.LastName = model.LastName;
                        existingPerson.PrimaryEmailAddress = model.EmailAddress;
                        existingPerson.PrimaryPhoneNumber = model.MobileNumber;
                        existingPerson.UpdatedById = migrateUserId;
                        existingPerson.UpdatedOnUtc = GetDateTime;
                        _personService.UpdatePerson(existingPerson);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(addressId))
                            personEntity.AddressId = addressId;
                        //Add in person
                        personEntity.FirstName = model.FirstName;
                        personEntity.MiddleName = model.MiddleName;
                        personEntity.LastName = model.LastName;
                        personEntity.PrimaryEmailAddress = model.EmailAddress;
                        personEntity.PrimaryPhoneNumber = model.MobileNumber;
                        personEntity.CreatedById = migrateUserId;
                        personEntity.UpdatedById = migrateUserId;
                        personEntity.CreatedOnUtc = GetDateTime;
                        personEntity.UpdatedOnUtc = GetDateTime;
                        _personService.InsertPerson(personEntity);

                        // Map the site to person
                        var sites = new PersonSitesMapping();
                        sites.PersonId = personEntity.Id;
                        sites.SiteId = SiteId;
                        sites.CreatedById = migrateUserId;
                        sites.UpdatedById = migrateUserId;
                        sites.CreatedOnUtc = GetDateTime;
                        sites.UpdatedOnUtc = GetDateTime;
                        _personService.InsertPersonSites(sites);
                    }

                    // Map the candidate model to the candidate entity
                    var entity = _mapper.Map<Candidate>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    if (existingPerson != null)
                    {
                        entity.PersonId = existingPerson.Id;
                    }
                    else
                    {
                        entity.PersonId = personEntity.Id;
                    }

                    entity.Source = !string.IsNullOrWhiteSpace(model.Source) ? model.Source : "Vsky Application";
                    entity.JobApplyDate = model.JobApplyDate;
                    entity.ReferenceName = !string.IsNullOrWhiteSpace(model.ReferenceName) ? model.ReferenceName : null;
                    entity.LinkedInProfile = !string.IsNullOrWhiteSpace(model.LinkedInProfile) ? model.LinkedInProfile : null;
                    entity.EnglishFluencyId = !string.IsNullOrWhiteSpace(model.EnglishFluencyId) ? model.EnglishFluencyId : null;
                    entity.Qualification = !string.IsNullOrWhiteSpace(model.Qualification) ? model.Qualification : null;
                    entity.JobId = !string.IsNullOrWhiteSpace(model.JobId) ? model.JobId : null;
                    entity.AppliedJobPositionId = !string.IsNullOrWhiteSpace(model.AppliedJobPositionId) ? model.AppliedJobPositionId : null;
                    entity.ExperienceYears = model.ExperienceYears > 0 ? model.ExperienceYears : 0;
                    entity.ExperienceMonths = model.ExperienceMonths > 0 ? model.ExperienceMonths : 0;
                    entity.ExpectedSalaryFrom = model.ExpectedSalaryFrom > 0 ? model.ExpectedSalaryFrom : 0;
                    entity.ExpectedSalaryTo = model.ExpectedSalaryTo > 0 ? model.ExpectedSalaryTo : 0;
                    entity.RecruiterId = !string.IsNullOrWhiteSpace(model.RecruiterId) ? model.RecruiterId : null;
                    entity.AvailabilityWorkId = !string.IsNullOrWhiteSpace(model.AvailabilityWorkId) ? model.AvailabilityWorkId : null;
                    entity.ExperienceDetails = !string.IsNullOrWhiteSpace(model.ExperienceDetails) ? model.ExperienceDetails : null;
                    entity.IsTransportration = !string.IsNullOrWhiteSpace(model.IsTransportration) ? model.IsTransportration : null;
                    entity.IsReadyToRelocate = !string.IsNullOrWhiteSpace(model.IsReadyToRelocate) ? model.IsReadyToRelocate : null;
                    entity.IsCandidateOwnSystem = !string.IsNullOrWhiteSpace(model.IsCandidateOwnSystem) ? model.IsCandidateOwnSystem : null;
                    entity.IsExperienced = !string.IsNullOrWhiteSpace(model.IsExperienced) ? model.IsExperienced : null;
                    entity.AppliedWorkLocationId = !string.IsNullOrWhiteSpace(model.AppliedWorkLocationId) ? model.AppliedWorkLocationId : null;

                    if (model.FilePic != null && model.FilePic.Length > 0)
                    {
                        var allowedFileTypes = new[] {
                            "image/jpeg", "image/png", "application/pdf",
                            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                        };

                        var file = model.FilePic;
                        if (!allowedFileTypes.Contains(file.ContentType))
                        {
                            return BadRequest(new BadRequestError("Invalid file type."));
                        }

                        var originalFileName = Path.GetFileName(model.FilePic.FileName);
                        var sanitizedFileName = Regex.Replace(originalFileName.Trim(), "[^A-Za-z0-9_.]+", "").Replace(" ", "_");
                        var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;
                        var files = new List<IFormFile> { model.FilePic };
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "candidate", files, entity.Id);

                        if (urls != null && urls.Any())
                        {
                            foreach (var blobUrl in urls)
                            {
                                var picture = new Picture
                                {
                                    SeoFilename = originalFileName,
                                    MimeType = mimeType,
                                    VirtualPath = blobUrl,
                                    ModuleId = entity.Id,
                                    Module = $"{model.FirstName} {model.LastName}",
                                    SubModuleId = entity.Id,
                                    Sub_Module = $"{model.FirstName} {model.LastName}",
                                    Type = "Candidate",
                                    SiteId = SiteId,
                                    CreatedById = migrateUserId,
                                    CreatedOnUtc = GetDateTime
                                };

                                _commonService.InsertPicture(picture);

                                entity.CandidateResumeFileId = picture.Id;
                            }
                        }
                    }
                    else
                    {
                        entity.CandidateResumeFileId = null;
                    }

                    if (!string.IsNullOrWhiteSpace(addressId))
                        entity.AddressId = addressId;

                    if (!string.IsNullOrEmpty(model.ExperienceDetails))
                    {
                        entity.ExperienceDetails = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.ExperienceDetails,
                                SiteData.Name,
                                "candidate",
                                entity.Id
                            );
                    }

                    entity.SiteId = SiteId;
                    entity.CreatedById = migrateUserId;
                    entity.UpdatedById = migrateUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _candidateService.InsertCandidate(entity);

                    // department mapping
                    if (model.DepartmentIdsArray != null && model.DepartmentIdsArray.Count() > 0)
                    {
                        foreach (var item in model.DepartmentIdsArray)
                        {
                            if (item != null)
                            {
                                var candidateDepartments = new CandidateDepartments
                                {
                                    CandidateId = entity.Id,
                                    DepartmentsId = item
                                };
                                _candidateDepartmentService.InsertCandidateDepartment(candidateDepartments);
                            }
                        }
                    }

                    #region email notification
                    await _workflowMessageService.SendRegisterUserEmail(entity.Id);

                    await _workflowMessageService.SendRegisterCandidateEmail(entity.Id);
                    #endregion

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
    }
}
