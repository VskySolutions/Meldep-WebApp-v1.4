using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Services.ProjectTasks;
using AutoMapper;
using Vsky.Services.Common;
using Vsky.Api.ApiErrors;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("tag-master")]
    public class TagsController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly ITagService _tagService;
        private readonly IProjectTaskTagService _projectTaskTagService;
        private readonly ISiteService _siteService;

        public TagsController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ICommonService commonService,
            ITagService tagService,
            IProjectTaskTagService projectTaskTagService, ISiteService siteService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _commonService = commonService;
            _tagService = tagService;
            _projectTaskTagService = projectTaskTagService;
            _siteService = siteService;
        }
        #endregion

        #region Get All lists
        [HttpPost("list")]
        public IActionResult GetAllTags(TagSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _tagService.GetAllTags(SiteId, searchModel.SearchText, searchModel.Name, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                var model = new TagListModel
                {
                    Data = _mapper.Map<IList<TagModels>>(list),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("dropdown/tagslist")]
        public async Task<IActionResult> GetAllTagsListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _tagService.GetAllTagList(SiteId);
                var model = _mapper.Map<List<TagModels>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTagById(string id)
        {
            try
            {
                var entity = await _tagService.GetTagDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No tag found with the specified id."));

                // Map the project entity to a ProjectModel object
                var model = _mapper.Map<TagModels>(entity);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region CreateTags
        [HttpPost]
        public async Task<IActionResult> CreateTags(TagModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _tagService.GetTagByName(SiteId, model.Name);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Tag name already exists, try with another."));

                    var entity = _mapper.Map<Tags>(model);

                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _tagService.InsertTags(entity);

                    return Ok(entity);
                }
            }
            catch (Exception ex)
            {
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region update tags
        [HttpPut("tagid/{id}")]
        public async Task<IActionResult> EditTag(string id, TagModels model)
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
                    var entity = await _tagService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No tag found with the specified id."));

                    entity.Color = model.Color;
                    entity.BgColor = model.BgColor;
                    entity.Name = model.Name;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _tagService.UpdateTags(entity);

                    return NoContent();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTags(string id, TagModels model)
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
                    var entity = await _tagService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No tag found with the specified id."));

                    if(model.IsBgColor)
                    {
                        entity.BgColor = model.BgColor;
                    }
                    else
                    {
                        entity.Color = model.Color;
                    }
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _tagService.UpdateTags(entity);

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

        #region DeleteTags
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTags(string id)
        {
            try
            {
                var entity = await _tagService.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No tag found with the specified id."));

                _tagService.DeleteTags(entity);

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
