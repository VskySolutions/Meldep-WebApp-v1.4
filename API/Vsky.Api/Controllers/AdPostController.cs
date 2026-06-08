using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vsky.Models;
using Vsky.Api.Models;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Services.AdPosts;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("ad-post")]
    public class AdPostController : BaseController
    {

        #region Define Services      
        private readonly IMapper _mapper;
        private readonly GlobalVariable _globalVariable;
        private readonly ICommonService _commonService;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly IAdPostService _adPostService;
        private readonly IAdPostingStatusService _adPostingStatusService;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations      
        public AdPostController(
            IMapper mapper,
            GlobalVariable globalVariable,
            ICommonService commonService,
            IDropDownService dropDownService,
            IDropDownTypeService dropDownTypeService,
            IAdPostService adPostService,
            ISiteService siteService,
            IAdPostingStatusService adPostingStatusService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _commonService = commonService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _adPostService = adPostService;
            _adPostingStatusService = adPostingStatusService;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllAdPosts
        // Title: Get All AdPosts
        // Description: This endpoint fetches a list of AdPosts based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllAdPosts(AdPostSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of AdPosts on search criteria (name, sorting, pagination)
                var list = _adPostService.GetAllAdPosts(SiteId, searchModel.SearchText, searchModel.ProjectIds, searchModel.Name, searchModel.CustomerIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                // Map the fetched list to a model suitable for the response
                var model = new AdPostListModel
                {
                    Data = _mapper.Map<IList<AdPostModel>>(list),
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

        #region GetAdPostDetailsById
        // Title: GetAdPostDetailsById
        // Description: This endpoint retrieves the details of a specific AdPost based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetAdPostDetailsById(string id)
        {
            try
            {
                // Fetch the AdPost entity by its ID from the service
                var entity = await _adPostService.GetAdPostDetailsById(id);
                // If the AdPost entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No AdPost found with the specified id."));

                // Map the AdPost entity to a AdPostModel object
                var model = _mapper.Map<AdPostModel>(entity);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAdPostingStatusesByAdId
        // Title: GetAdPostingStatusesByAdId
        // Description: This endpoint retrieves the details of a specific AdPostingStatus based on its unique identifier (ID). 
        [HttpGet("{adId}")]
        public async Task<IActionResult> GetAdPostingStatusesByAdId(string adId)
        {
            try
            {
                // Fetch the AdPostingStatus entity by its ID from the service
                var entity = await _adPostingStatusService.GetAdPostingStatusesByAdId(adId);
                // If the AdPostingStatus entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No ad posting status found with the specified id."));

                // Map the AdPostingStatus entity to a AdPostingStatusModel object
                var model = _mapper.Map<List<AdPostingStatusModel>>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAdPostNumber
        // Title: GetAdPostNumber
        // Description: This endpoint retrieves the details of last records. 
        [HttpGet("number")]
        public async Task<IActionResult> GetAdPostNumber()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch the AdPostNumber entity by its ID from the service
                var postNumber = await _adPostService.GetAdPostNumber(SiteId);
                var AdNumber = 1;

                // If the AdPostNumber entity is not found, return a BadRequest response with an error message
                if (postNumber != null)
                    AdNumber = postNumber.AdNumber + 1;

                return Ok(AdNumber);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateAdPost
        // Title: CreateAdPost
        // Description: This endpoint handles the creation of a new AdPost. It maps the AdPost model to the AdPost entity, sets the creation details, and inserts the AdPost into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateAdPost([FromForm] AdPostModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _adPostService.GetAdPostByName(model.Name, model.ProjectId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("This Project and Ad combination already exists."));

                    // Map the AdPost model to the AdPost entity
                    var entity = _mapper.Map<AdPost>(model);
                    entity.Id = Guid.NewGuid().ToString();
                    entity.SiteId = SiteId;

                    if (model.PostDesignPic != null && model.PostDesignPic.Length > 0)
                    {
                        var file = model.PostDesignPic;
                        var originalFileName = Path.GetFileName(file.FileName);
                        var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                        var files = new List<IFormFile> { model.PostDesignPic };
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "marketing-ad-post", files, entity.AdNumber.ToString());

                        if (urls != null && urls.Any())
                        {
                            foreach (var blobUrl in urls)
                            {
                                var picture = new Picture
                                {
                                    SiteId = SiteId,
                                    Type = "AdPost",
                                    SeoFilename = originalFileName,
                                    MimeType = mimeType,
                                    VirtualPath = blobUrl,
                                    ModuleId = entity.Id,
                                    Module = entity.Name,
                                    SubModuleId = entity.Id,
                                    Sub_Module = entity.Name,
                                    CreatedOnUtc = GetDateTime,
                                    CreatedById = LoggedUserId
                                };

                                _commonService.InsertPicture(picture);

                                entity.PictureId = picture.Id;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "marketing-ad-post",
                                entity.AdNumber.ToString()
                            );
                    }

                    // Set the created by and created on properties
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _adPostService.InsertAdPost(entity);

                    return Ok(entity);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            // Return model state errors if the model state is not valid
            return ModelStateError(ModelState);
        }
        #endregion

        #region CreateAdPostingStatus
        // Title: CreateAdPostingStatus
        // Description: This endpoint handles the creation of a new AdPostingStatus. It maps the AdPostingStatus model to the AdPostingStatus entity, sets the creation details, and inserts the AdPostingStatus into the database. 
        [HttpPost("ad-posting-status")]
        public async Task<IActionResult> CreateAdPostingStatus(AdPostingStatusModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //save ad posting status
                    if (model.AdPostingStatuses.Count() > 0)
                    {
                        foreach (var item in model.AdPostingStatuses)
                        {
                            var existing = await _adPostingStatusService.GetAdPostingStatusById(item.Id);

                            // delete
                            if (existing != null && item.Deleted)
                            {
                                _adPostingStatusService.DeleteAdPostingStatus(existing);
                                continue;
                            }

                            // skip deleted new records
                            if (item.Deleted)
                                continue;

                            // map common fields
                            Action<AdPostingStatus> mapFields = entity =>
                            {
                                entity.AdId = model.AdId;
                                entity.AdPostChannelId = item.AdPostChannelId;
                                entity.Likes = item.Likes;
                                entity.Comments = item.Comments;
                                entity.Shares = item.Shares;

                                if (!string.IsNullOrWhiteSpace(item.DateStr))
                                    entity.Date = DateTime.ParseExact(item.DateStr, "MM/dd/yyyy", null);

                                entity.UpdatedById = LoggedUserId;
                                entity.UpdatedOnUtc = GetDateTime;
                            };

                            // update or insert
                            if (existing != null)
                            {
                                mapFields(existing);
                                _adPostingStatusService.UpdateAdPostingStatus(existing);
                            }
                            else
                            {
                                var newEntity = new AdPostingStatus();
                                mapFields(newEntity);

                                newEntity.CreatedById = LoggedUserId;
                                newEntity.CreatedOnUtc = GetDateTime;

                                _adPostingStatusService.InsertAdPostingStatus(newEntity);
                            }
                        }
                    }
                    return Ok(model);
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

        #region UpdateAdPost
        // Title: UpdateAdPost
        // Description: This endpoint updates an existing AdPost by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdPost(string id, [FromForm] AdPostModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the AdPost entity by its ID
                    var entity = await _adPostService.GetAdPostById(id);
                    // If no AdPost is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No AdPost found with the specified id."));

                    entity.ProjectId = model.ProjectId;
                    entity.Name = model.Name;
                    entity.CustomerId = model.CustomerId;
                    entity.URL = model.URL;
                    entity.Caption = model.Caption;
                    entity.Tags = model.Tags;
                    entity.ImageType = model.ImageType;
                    entity.ImageProviderClientId = model.ImageProviderClientId;
                    entity.ImageProviderEmpId = model.ImageProviderEmpId;
                    entity.ContentType = model.ContentType;
                    entity.ContentProviderClientId = model.ContentProviderClientId;
                    entity.ContentProviderEmpId = model.ContentProviderEmpId;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "marketing-ad-post",
                                entity.AdNumber.ToString(),
                                entity.Description
                            );
                    }

                    //Upload post design
                    var PostFileId = "";
                    if (model.PostChangeFlag == "edit")
                    {
                        // Delete old image first
                        if (!string.IsNullOrEmpty(entity.PictureId))
                        {
                            var oldPicture = await _commonService.GetByPictureId(entity.PictureId);

                            if (oldPicture != null && !string.IsNullOrEmpty(oldPicture.VirtualPath))
                            {
                                await _azureBlobImageServices.DeleteImage(oldPicture.VirtualPath);

                                _commonService.DeletePicture(oldPicture);
                            }
                        }

                        if (model.PostDesignPic != null && model.PostDesignPic.Length > 0)
                        {
                            var file = model.PostDesignPic;
                            var originalFileName = Path.GetFileName(file.FileName);
                            var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                            var files = new List<IFormFile> { model.PostDesignPic };
                            var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "marketing-ad-post", files, entity.AdNumber.ToString());

                            if (urls != null && urls.Any())
                            {
                                foreach (var blobUrl in urls)
                                {
                                    var picture = new Picture
                                    {
                                        SiteId = SiteId,
                                        Type = "AdPost",
                                        SeoFilename = originalFileName,
                                        MimeType = mimeType,
                                        VirtualPath = blobUrl,
                                        ModuleId = entity.Id,
                                        Module = entity.Name,
                                        SubModuleId = entity.Id,
                                        Sub_Module = entity.Name,
                                        CreatedOnUtc = GetDateTime,
                                        CreatedById = LoggedUserId
                                    };

                                    _commonService.InsertPicture(picture);

                                    PostFileId = picture.Id;
                                }
                            }
                        }
                    }
                    else if (model.PostChangeFlag == "remove")
                    {
                        // Remove the logo file
                        if (!string.IsNullOrEmpty(entity.PictureId))
                        {
                            var oldPicture = await _commonService.GetByPictureId(entity.PictureId);

                            if (oldPicture != null && !string.IsNullOrEmpty(oldPicture.VirtualPath))
                            {
                                await _azureBlobImageServices.DeleteImage(oldPicture.VirtualPath);

                                _commonService.DeletePicture(oldPicture);
                            }
                        }

                        entity.PictureId = null;
                    }

                    //Update Entry
                    if (PostFileId != "")
                        entity.PictureId = PostFileId;

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _adPostService.UpdateAdPost(entity);

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

        #region DeleteAdPost
        // Title: DeleteAdPostById
        // Description: This endpoint deletes a test case based on the provided AdPost ID. It first retrieves the AdPost entity by ID, checks if it exists, and if so, deletes the AdPost. If the AdPost is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdPost(string id)
        {
            try
            {
                // Fetch the AdPost entity by its ID
                var entity = await _adPostService.GetAdPostById(id);
                // If no AdPost is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Ad found with the specified id."));

                // Delete the AdPost using the AdPost service
                _adPostService.DeleteAdPost(entity);

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
