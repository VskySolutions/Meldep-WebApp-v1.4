using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.Sites;
using Vsky.Services.TrainingPortalMappings;
using Vsky.Services.TrainingPortals;

namespace Vsky.Api.Controllers
{
    [Route("training-portals")]
    public class TrainingPortalController : BaseController
    {
        #region Define services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ITrainingPortalService _trainingPortalService;
        private readonly ICommonService _commonService;
        private readonly ApplicationDbContext _db;
        private readonly ISiteService _siteService;
        private readonly ITrainingPortalMappingService _trainingPortalMappingService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations
        public TrainingPortalController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ITrainingPortalService trainingPortalService,
            ICommonService commonService,
            ApplicationDbContext db,
            ISiteService siteService,
            ITrainingPortalMappingService trainingPortalMappingService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _trainingPortalService = trainingPortalService;
            _commonService = commonService;
            _db = db;
            _siteService = siteService;
            _trainingPortalMappingService = trainingPortalMappingService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllTrainingList
        [HttpPost("list")]
        public IActionResult GetAllTrainingList(TrainingPortalSearchModels trainingModel)
        {
            try
            {
                //Get LoggedUser Info
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _trainingPortalService.GetAllTrainingList(SiteId, trainingModel.SearchText, trainingModel.Name, trainingModel.EmployeeDesignationIds, trainingModel.SortBy, trainingModel.Descending, trainingModel.Page, trainingModel.PageSize);
                var model = new TrainingPortalListModel
                {
                    Data = _mapper.Map<IList<TrainingPortalModel>>(list),
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

        #region GetTrainingDetailsById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrainingDetailsById(string id)
        {
            try
            {
                var entity = await _trainingPortalService.GetTrainingDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No training found with the specified id."));

                var model = _mapper.Map<TrainingPortalModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region create training
        [HttpPost]
        public async Task<IActionResult> CreateTraining([FromForm] TrainingPortalModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _trainingPortalService.GetTrainingByName(SiteId, model.Name);
                    if (exists != null)
                        return BadRequest(new BadRequestError("The training already exists"));

                    // Map the training model to the training entity
                    var entity = _mapper.Map<TrainingPortal>(model);

                    var record = _db.TrainingPortal.OrderByDescending(m => m.TrainingPortalNumber).FirstOrDefault();
                    entity.TrainingPortalNumber = record != null ? record.TrainingPortalNumber + 1 : 1;
                    entity.Id = Guid.NewGuid().ToString();
                    entity.Name = model.Name;
                    entity.SiteId = SiteId;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "training-portal",
                                entity.TrainingPortalNumber.ToString()
                            );
                    }

                    if (model.FilePic != null && model.FilePic.Length > 0)
                    {
                        var allowedFileTypes = new[] {
                            "image/jpeg", "image/png", "application/pdf",
                            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "application/vnd.openxmlformats-officedocument.presentationml.presentation"
                        };

                        var file = model.FilePic;
                        if (!allowedFileTypes.Contains(file.ContentType))
                        {
                            return BadRequest(new BadRequestError("Invalid file type."));
                        }

                        var originalFileName = Path.GetFileName(file.FileName);
                        var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                        var files = new List<IFormFile> { model.FilePic };
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "training-portal", files, entity.TrainingPortalNumber.ToString());

                        if (urls != null && urls.Any())
                        {
                            foreach (var blobUrl in urls)
                            {
                                var picture = new Picture
                                {
                                    SiteId = SiteId,
                                    Type = "TrainingPortal",
                                    SeoFilename = originalFileName,
                                    MimeType = mimeType,
                                    VirtualPath = blobUrl,
                                    ModuleId = entity.Id,
                                    Module = entity.Name,
                                    SubModuleId = entity.Id,
                                    Sub_Module = entity.Name,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };

                                _commonService.InsertPicture(picture);

                                entity.TrainingFileId = picture.Id;
                            }
                        }
                    }
                    else
                    {
                        entity.TrainingFileId = null;
                    }
                    entity.Url = model.Url;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _trainingPortalService.InsertTraining(entity);

                    // Add Assign to 
                    if (model.EmployeeDesignationIdsArray != null && model.EmployeeDesignationIdsArray.Count() > 0)
                    {
                        foreach (var item in model.EmployeeDesignationIdsArray)
                        {
                            if (item != null)
                            {
                                var entityTrainingMapping = new Training_Portal_Mapping
                                {
                                    TrainingId = entity.Id,
                                    EmployeeDesignationId = item
                                };
                                _trainingPortalMappingService.InsertTrainingEmployees(entityTrainingMapping);
                            }
                        }
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

        #region UpdateTraining
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTraining(string id, [FromForm] TrainingPortalModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //check if any training with same name
                    var exists = await _trainingPortalService.GetTrainingByName(SiteId, model.Name, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("The training already exists"));

                    //get training entity by its ID
                    var entity = await _trainingPortalService.GetById(id, SiteId);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No training found with the specified id."));

                    //upload training file
                    var trainingFileId = "";
                    if (model.FileChangeFlag == "edit")
                    {
                        // Delete old image first
                        if (!string.IsNullOrEmpty(entity.TrainingFileId))
                        {
                            var oldPicture = await _commonService.GetByPictureId(entity.TrainingFileId);

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
                                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "application/vnd.openxmlformats-officedocument.presentationml.presentation"
                            };
                            var file = model.FilePic;

                            if (!allowedFileTypes.Contains(file.ContentType))
                            {
                                return BadRequest(new BadRequestError("Invalid file type."));
                            }

                            var originalFileName = Path.GetFileName(file.FileName);
                            var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                            int existingImagesCount = await _commonService.GetPicturesCountBySubModuleId(entity.Id, "Training Portal");
                            var files = new List<IFormFile> { model.FilePic };
                            var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "training-portal", files, entity.TrainingPortalNumber.ToString(), existingImagesCount);

                            if (urls != null && urls.Any())
                            {
                                foreach (var blobUrl in urls)
                                {
                                    var picture = new Picture
                                    {
                                        SiteId = SiteId,
                                        Type = "Training Portal",
                                        SeoFilename = originalFileName,
                                        MimeType = mimeType,
                                        VirtualPath = blobUrl,
                                        ModuleId = entity.Id,
                                        Module = entity.Name,
                                        SubModuleId = entity.Id,
                                        Sub_Module = entity.Name,
                                        CreatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime
                                    };

                                    _commonService.InsertPicture(picture);

                                    trainingFileId = picture.Id;
                                }
                            }
                        }
                    }
                    else if (model.FileChangeFlag == "remove")
                    {
                        // Remove the logo file
                        if (!string.IsNullOrEmpty(entity.TrainingFileId))
                        {
                            var oldPicture = await _commonService.GetByPictureId(entity.TrainingFileId);

                            if (oldPicture != null && !string.IsNullOrEmpty(oldPicture.VirtualPath))
                            {
                                await _azureBlobImageServices.DeleteImage(oldPicture.VirtualPath);

                                _commonService.DeletePicture(oldPicture);
                            }
                        }

                        entity.TrainingFileId = null;
                    }

                    //update entry
                    if (trainingFileId != "")
                        entity.TrainingFileId = trainingFileId;

                    entity.Name = model.Name;
                    entity.Url = model.Url;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "training-portal",
                                entity.TrainingPortalNumber.ToString(),
                                entity.Description
                            );
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _trainingPortalService.UpdateTraining(entity);

                    // Add Assign to 
                    var existingMappings = _trainingPortalMappingService.GetTrainingByTrainingId(SiteId, entity.Id);

                    var existingIds = existingMappings
                        .Select(x => x.EmployeeDesignationId)
                        .ToList();

                    // Normalize incoming IDs
                    var newIds = model.EmployeeDesignationIdsArray?
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()
                        .ToList()
                        ?? new List<string>();

                    //Delete all if newIds are empty
                    if (!newIds.Any())
                    {
                        foreach (var mapping in existingMappings)
                        {
                            _trainingPortalMappingService.DeleteTrainingEmployees(mapping);
                        }

                    }

                    // add
                    foreach (var existingId in newIds.Except(existingIds))
                    {
                        var entityTrainingMapping = new Training_Portal_Mapping
                        {
                            TrainingId = entity.Id,
                            EmployeeDesignationId = existingId
                        };

                        _trainingPortalMappingService.InsertTrainingEmployees(entityTrainingMapping);
                    }

                    // remove deselected mappings
                    if (newIds.Any())
                    {
                        foreach (var mapping in existingMappings.Where(x => !newIds.Contains(x.EmployeeDesignationId)))
                        {
                            _trainingPortalMappingService.DeleteTrainingEmployees(mapping);
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

        #region DeleteTraining
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTraining(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var entity = await _trainingPortalService.GetById(id, SiteId);
                if (entity == null)
                    return BadRequest(new BadRequestError("No training found with the specified id."));

                _trainingPortalService.DeleteTraining(entity);
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
