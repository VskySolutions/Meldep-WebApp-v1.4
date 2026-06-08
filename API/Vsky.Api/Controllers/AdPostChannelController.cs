using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.AdPosts;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("ad-post-channel")]
    public class AdPostChannelController : BaseController
    {
        #region Define Services      
        private readonly IMapper _mapper;
        private readonly GlobalVariable _globalVariable;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly IAdPostChannelService _adPostChannelService;
        #endregion

        #region Services Initializations      
        public AdPostChannelController(
            IMapper mapper, 
            GlobalVariable globalVariable,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices,
            IAdPostChannelService adPostChannelService
        )
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
            _adPostChannelService = adPostChannelService;
        }
        #endregion

        #region GetAllAdPostChannel
        // Title: Get All AdPostChannel
        // Description: This endpoint fetches a list of Ad Post Channel based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllAdPostChannel(AdPostChannelSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                // Fetch a list of Ad Post Channels on search criteria (name, sorting, pagination)
                var list = _adPostChannelService.GetAllAdPostChannel(SiteId, searchModel.SearchText, searchModel.ProjectIds, searchModel.Name, searchModel.CustomerIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new AdPostChannelListModel
                {
                    Data = _mapper.Map<IList<AdPostChannelModel>>(list),
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

        #region GetAllAdPostChannelsListForDropdown
        // Title: GetAllAdPostChannelsListForDropdown
        // Description: This endpoint retrieves the details of a specific AdPostChannel based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllAdPostChannelsListForDropdown()
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _adPostChannelService.GetAllAdPostChannelsListForDropdown(SiteId);
            var model = _mapper.Map<List<CommonDropDown>>(list);

            return Ok(model);
        }
        #endregion

        #region GetChannelNumber
        // Title: GetChannelNumber
        // Description: This endpoint retrieves the details of last records. 
        [HttpGet("number")]
        public async Task<IActionResult> GetChannelNumber()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                // Fetch the ChannelNumber entity by its ID from the service
                var Number = await _adPostChannelService.GetChannelNumber(SiteId);
                var ChannelNumber = 1;

                // If the ChannelNumber entity is not found, return a BadRequest response with an error message
                if (Number != null)
                    ChannelNumber = Number.ChannelNumber + 1;

                return Ok(ChannelNumber);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAdPostChannelDetailsById
        // Title: GetAdPostChannelDetailsById
        // Description: This endpoint retrieves the details of a specific ad post channel based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetAdPostChannelDetailsById(string id)
        {
            try
            {
                // Fetch the ad post channel entity by its ID from the service
                var entity = await _adPostChannelService.GetAdPostChannelDetailsById(id);
                // If the ad post channel entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No ad post channel found with the specified id."));

                // Map the ad post channel entity to a TestPlanModel object
                var model = _mapper.Map<AdPostChannelModel>(entity);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateAdPostChannel
        // Title: CreateAdPostChannel
        // Description: This endpoint handles the creation of a new ad post channel. It maps the ad post channel model to the ad post channel entity, sets the creation details, and inserts the ad post channel into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateAdPostChannel(AdPostChannelModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _adPostChannelService.GetAdPostChannelByName(model.Name, model.ProjectId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("This Project and Channel combination already exists."));

                    // Map the ad post channel model to the ad post channel entity
                    var entity = _mapper.Map<AdPostChannel>(model);

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "marketing-ad-post-channel",
                                entity.ChannelNumber.ToString()
                            );
                    }
                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _adPostChannelService.InsertAdPostChannel(entity);

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

        #region UpdateAdPostChannel
        // Title: UpdateAdPostChannel
        // Description: This endpoint updates an existing AdPostChannel by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdPostChannel(string id, AdPostChannelModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the AdPostChannel entity by its ID
                    var entity = await _adPostChannelService.GetAdPostChannelById(id);
                    // If no AdPostChannel is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No ad post channel found with the specified id."));

                    var exists = await _adPostChannelService.GetAdPostChannelByName(model.Name, model.ProjectId, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("The ad post channel already exists"));

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "marketing-ad-post-channel",
                                entity.ChannelNumber.ToString(),
                                entity.Description
                            );
                    }

                    entity.ChannelNumber = model.ChannelNumber;
                    entity.ProjectId = model.ProjectId;
                    entity.Name = model.Name;
                    entity.CustomerId = model.CustomerId;
                    entity.GroupMemberCount = model.GroupMemberCount;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _adPostChannelService.UpdateAdPostChannel(entity);

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

        #region DeleteAdPostChannel
        // Title: DeleteAdPostChannelById
        // Description: This endpoint deletes a ad post channel based on the provided ad post channel ID. It first retrieves the ad post channel entity by ID, checks if it exists, and if so, deletes the ad post channel. If the ad post channel is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdPostChannel(string id)
        {
            try
            {
                // Fetch the ad post channel entity by its ID
                var entity = await _adPostChannelService.GetAdPostChannelById(id);
                // If no ad post channel is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No ad post channel found with the specified id."));

                // Delete the ad post channel using the ad post channel service
                _adPostChannelService.DeleteAdPostChannel(entity);

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