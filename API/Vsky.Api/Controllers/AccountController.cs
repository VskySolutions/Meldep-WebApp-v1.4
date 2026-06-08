using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.Persons;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("account")]
    public class AccountController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IPersonService _personService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations
        public AccountController(
            GlobalVariable globalVariable,
            UserManager<ApplicationUser> userManager,
            IMapper mapper, 
            ISiteService siteService, 
            ICommonService commonService,
            IPersonService personService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _userManager = userManager;
            _mapper = mapper;
            _siteService = siteService;
            _commonService = commonService;
            _personService = personService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region ChangePassword
        // Title: Change Password
        // Description: This endpoint allows users to securely update their existing password to a new one.
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();

            //Check the user already exists or not
            var loggedUser = await _userManager.FindByIdAsync(LoggedUserId);

            if (loggedUser == null || loggedUser.Deleted)
            {
                return BadRequest(new BadRequestError("No user was found with the specified id."));
            }

            //check NewPassword and ConfirmPassword are match
            if (model.NewPassword != model.ConfirmPassword)
            {
                return BadRequest(new BadRequestError("New password should match confirm password."));
            }

            // ChangePasswordAsync changes the user password
            var result = await _userManager.ChangePasswordAsync(loggedUser, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
                return Ok();
            else
                return InternalServerError(result.Errors);
        }
        #endregion

        #region Get Person Profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetPerson()
        {
            try
            {
                //set login user Id
                var LoggedUserId = User.GetLoggedInUserId<string>(); ;

                //Check the user already exists or not
                var loggedUser = await _userManager.FindByIdAsync(LoggedUserId);

                if (loggedUser == null || loggedUser.Deleted)
                {
                    return BadRequest(new BadRequestError("No user was found with the specified id."));
                }
                var entity = await _personService.GetPersonDetailsById(loggedUser.PersonId);

                // Map the person entity to a PersonModel object
                var model = _mapper.Map<PersonModel>(entity);

                return Ok(model);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdatePerson
        // Title: UpdatePerson
        // Description: This endpoint updates an existing person by its ID. It validates the person model, checks for duplicate person names within the same customer, updates the person's details.
        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdatePerson([FromForm] PersonModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Check if there is any person with the same email.
                    var exists = await _personService.GetPersonByEmail(model.PrimaryEmailAddress, model.Id, SiteId);

                    // If person not exists, return a bad request with an error message
                    if (exists != null)
                        return BadRequest(new BadRequestError("Email already exists."));

                    if (!string.IsNullOrWhiteSpace(model.BgColor) && !string.IsNullOrWhiteSpace(model.Color))
                    {
                        var existingColor = await _personService.GetPersonByColor(model.Id, model.BgColor, model.Color, SiteId);
                        if (existingColor != null)
                            return BadRequest(new BadRequestError("This color combination already exists."));
                    }

                    // Fetch the person entity by its ID
                    var entity = await _personService.GetPersonById(model.Id);
                    if (entity == null) // If no person is not found with the given ID, return a bad request with an error message
                        return BadRequest(new BadRequestError("No person found with the specified id."));

                    entity.FirstName = model.FirstName;
                    entity.MiddleName = model.MiddleName;
                    entity.LastName = model.LastName;
                    entity.PrimaryEmailAddress = model.PrimaryEmailAddress;
                    entity.Color = model.Color;
                    entity.BgColor = model.BgColor;

                    //Upload profile picture
                    var PersonFileId = "";
                    if (model.PersonChangeFlag == "edit")
                    {
                        var oldPicture = await _commonService.GetByPictureId(entity.PictureId);
                        if (oldPicture != null && !string.IsNullOrEmpty(oldPicture.VirtualPath))
                        {
                            await _azureBlobImageServices.DeleteImage(oldPicture.VirtualPath);
                            _commonService.DeletePicture(oldPicture);
                        }

                        if (model.PersonPic != null && model.PersonPic.Length > 0)
                        {
                            var originalFileName = Path.GetFileName(model.PersonPic.FileName);
                            var mimeType = model.PersonPic.ContentType;
                            var files = new List<IFormFile> { model.PersonPic };

                            var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "profile", files, entity.Id);
                            if (urls != null && urls.Any())
                            {
                                foreach (var blobUrl in urls)
                                {
                                    var picture = new Picture
                                    {
                                        SiteId = SiteId,
                                        Type = "Person",
                                        SeoFilename = originalFileName,
                                        MimeType = mimeType,
                                        VirtualPath = blobUrl,
                                        ModuleId = entity.Id,
                                        Module = entity.FirstName + " " + entity.LastName,
                                        SubModuleId = entity.Id,
                                        Sub_Module = entity.FirstName + " " + entity.LastName,
                                        CreatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime
                                    };

                                    _commonService.InsertPicture(picture);

                                    PersonFileId = picture.Id;
                                }
                            }
                        }
                    }
                    else if (model.PersonChangeFlag == "remove")
                    {
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
                    if (PersonFileId != "")
                        entity.PictureId = PersonFileId;

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    _personService.UpdatePerson(entity);

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

        #region Blob Image Processing
        [HttpPost("upload-image-to-blob-storage")]
        public async Task<IActionResult> UploadImageToBlobStorage(IFormFile file)
        {
            if(file?.Length == 0)
                return BadRequest("Image Missing. Please Upload Image....");

            var url = await _azureBlobImageServices.UploadImageAsync("site-vsky-solutions", "persons", file);
            return Ok(url);
        }

        [HttpPost("delete-upload-image-to-blob-storage")]
        public async Task<IActionResult> DeleteUploadImageToBlobStorage(string fileURL)
        {
            if (string.IsNullOrEmpty(fileURL))
                return BadRequest("Image URL Missing....");

            var result = await _azureBlobImageServices.DeleteImage(fileURL);
            return result ? Ok("Image Deleted Successfully....") : NotFound();
        }
        #endregion
    }
}
