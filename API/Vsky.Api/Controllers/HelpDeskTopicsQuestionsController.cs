using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.HelpDesks;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("helpDeskTopicsQuestions")]
    public class HelpDeskTopicsQuestionsController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IHelpDeskTopicService _helpDeskTopicService;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations
        public HelpDeskTopicsQuestionsController(
            GlobalVariable globalVariable,
            IMapper mapper,
            IHelpDeskTopicService helpDeskTopicService,
            ISiteService siteService,
            ICommonService commonService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _helpDeskTopicService = helpDeskTopicService;
            _siteService = siteService;
            _commonService = commonService;
        }
        #endregion

        #region GetAllHelpDeskTopicListForDropdown
        // Title: GetAllHelpDeskTopicListForDropdown
        // Description: This endpoint retrieves the list of Help Desk Topics for the list page.
        [HttpGet("topic/list")]
        public async Task<IActionResult> GetAllHelpDeskTopicList()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _helpDeskTopicService.GetAllHelpDeskTopicList(SiteId);
                var model = _mapper.Map<List<HelpDeskTopicModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllHelpDeskTopicQuestionList
        // Title: GetAllHelpDeskTopicQuestionList
        // Description: This endpoint retrieves the list of Help Desk Topic Questions for the list page.
        [HttpGet("questions/list")]
        public async Task<IActionResult> GetAllHelpDeskTopicQuestionList(string topicId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _helpDeskTopicService.GetAllHelpDeskTopicQuestionList(SiteId, topicId);
                var model = _mapper.Map<List<HelpDeskTopicQuestionsModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllHelpDeskTopicListForDropdown
        // Title: GetAllHelpDeskTopicListForDropdown
        // Description: This method retrieves the list of active Help Desk Topics for the dropdown based on the specified site.
        [HttpGet("topicdropdown/list")]
        public async Task<IActionResult> GetAllHelpDeskTopicListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _helpDeskTopicService.GetAllHelpDeskTopicListForDropdown(SiteId);
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllHelpDeskTopicQuestionsListForDropdown
        // Title: GetAllProjectModuleListForDropdown
        // Description: This method retrieves the list of active Help Desk Topic Questions for the dropdown based on the specified site and topic.
        [HttpGet("questionsdropdown/list")]
        public async Task<IActionResult> GetAllHelpDeskTopicQuestionsListForDropdown(string topicId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _helpDeskTopicService.GetAllHelpDeskTopicQuestionsListForDropdown(SiteId, topicId);
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllHelpDeskTopicAndQuestionList
        [HttpPost("topic-and-questions/list")]
        public IActionResult GetAllHelpDeskTopicAndQuestionList(HelpDeskTopicQuestionsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var list = _helpDeskTopicService.GetAllHelpDeskTopicAndQuestionList(
                    SiteId,
                    searchModel.SearchText,
                    searchModel.Topic,
                    searchModel.Question,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = new HelpDeskTopicQuestionsList
                {
                    HelpDeskTopicQuestionList = list,
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region GetHelpDeskTopicById
        // Title: GetHelpDeskTopicById
        // Description: This endpoint retrieve help desk topic for the specified  help desk topic ID.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHelpDeskTopicById(string id)
        {
            var entity = await _helpDeskTopicService.GetHelpDeskTopicById(id);
            if (entity == null)
                return Ok(new List<HelpDeskTopic>());

            return Ok(new List<HelpDeskTopic> { entity });
        }
        #endregion

        #region CreateHelpDeskTopic
        // Title: CreateHelpDeskTopic
        // Description: This endpoint handles the creation of a new help desk topic. It first checks if a topic with the same name already exists for the specified topic. If not, it maps the helpDesk topic model to the helpDesk topic entity, sets the creation details, and inserts the help desk topic into the database.
        [HttpPost]
        public async Task<IActionResult> CreateHelpDeskTopic(HelpDeskTopicModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the help desk topic already exists
                    var exists = await _helpDeskTopicService.GetHelpDeskTopicByTitle(SiteId, model.Title);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Help desk workspace already exists, Please try with another."));

                    var entity = _mapper.Map<HelpDeskTopic>(model);
                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _helpDeskTopicService.InsertHelpDeskTopic(entity);

                    return Ok(new { entity.Id, entity.Title });
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateHelpDeskQuestion
        // Title: CreateHelpDeskQuestion
        // Description: This endpoint handles the creation of a new question. It first checks if a question with the same name already exists for the specified topic. If not, it maps the help desk topic questions model to the help desk topic questions entity, sets the creation details, and inserts the help desk questions into the database.
        [HttpPost("helpDeskQuestion")]
        public async Task<IActionResult> CreateHelpDeskQuestion(HelpDeskTopicQuestionsModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the help desk question already exists
                    var exists = await _helpDeskTopicService.GetHelpDeskQuestionByTopicAndQuestion(SiteId, model.TopicId, model.Question);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Menu already exists, Please try with another."));

                    var entity = _mapper.Map<HelpDeskTopicQuestions>(model);
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _helpDeskTopicService.InsertHelpDeskQuestion(entity);

                    return Ok(new { entity.Id });
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateHelpDeskTopic
        // Title: UpdateHelpDeskTopic
        // Description: This endpoint updates an existing help desk topic by its ID. It validates the help desk topic model, checks for duplicate  help desk topic, updates the  help desk topic's details.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHelpDeskTopic(string id, HelpDeskTopicModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                //Check if the help desk topic already exists
                var exists = await _helpDeskTopicService.GetHelpDeskTopicByTitle(SiteId, model.Title, id);
                if (exists != null)
                    return BadRequest(new BadRequestError("Help desk workspace already exists, Please try with another."));

                var entity = await _helpDeskTopicService.GetHelpDeskTopicById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No help desk workspace found with the specified id."));

                entity.Title = model.Title;
                entity.Description = model.Description;
                entity.IsActive = model.IsActive;
                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;
                _helpDeskTopicService.UpdateHelpDeskTopic(entity);

                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateHelpDeskQuestion
        // Title: UpdateHelpDeskQuestion
        // Description: This endpoint updates an existing help desk topic by its ID. It validates the help desk topic question model, checks for duplicate question  within the same help desk topic, updates the help desk question's details.
        [HttpPut("helpDeskQuestion/{id}")]
        public async Task<IActionResult> UpdateHelpDeskQuestion(string id, HelpDeskTopicQuestionsModel model)
        {
            if (ModelState.IsValid)
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                //Check if the help desk question already exists
                var exists = await _helpDeskTopicService.GetHelpDeskQuestionByTopicAndQuestion(SiteId, model.TopicId, model.Question, id);
                if (exists != null)
                    return BadRequest(new BadRequestError("Menu already exists, Please try with another."));

                var entity = await _helpDeskTopicService.GetHelpDeskQuestionById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No menu found with the specified id."));

                entity.Question = model.Question;
                entity.Description = model.Description;
                entity.IsActive = model.IsActive;
                entity.UpdatedById = LoggedUserId;
                entity.UpdatedOnUtc = GetDateTime;
                _helpDeskTopicService.UpdateHelpDeskQuestion(entity);

                return NoContent();
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteHelpDeskTopic
        // Title: DeleteHelpDeskTopic
        // Description: This endpoint deletes a help desk topic based on the provided help desk topic ID. It first retrieves the help desk topic entity by ID, checks if it exists, and if so, deletes the help desk topic. If the help desk topic is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHelpDeskTopic(string id)
        {
            var entity = await _helpDeskTopicService.GetHelpDeskTopicById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No help desk workspace found with the specified id."));

            _helpDeskTopicService.DeleteHelpDeskTopic(entity);

            return NoContent();
        }
        #endregion

        #region DeleteHelpDeskQuestion
        // Title: DeleteHelpDeskQuestion
        // Description: This endpoint deletes a help desk question based on the provided help desk question ID. It first retrieves the help desk question entity by ID, checks if it exists, and if so, deletes the help desk question. If the help desk question is not found, it returns a BadRequest response with an error message.
        [HttpDelete("helpDeskQuestion/{id}")]
        public async Task<IActionResult> DeleteHelpDeskQuestion(string id)
        {
            var entity = await _helpDeskTopicService.GetHelpDeskQuestionById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No help desk menu found with the specified id."));

            _helpDeskTopicService.DeleteHelpDeskQuestion(entity);

            return NoContent();
        }
        #endregion

        #region checkTopicCanBeDeleted
        // Title: checkTopicCanBeDeleted
        // Description: A topic cannot be deleted if it has any active (non-deleted) questions.
        [HttpGet("checkTopicCanBeDeleted/{topicId}")]
        public async Task<IActionResult> CheckTopicCanBeDeleted(string topicId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            // Fetch all questions under this topic
            var questions = await _helpDeskTopicService.GetAllHelpDeskTopicQuestionList(SiteId, topicId);

            // Check if any a questions exist
            bool hasActiveQuestions = questions.Any(q =>
                !q.Deleted
            );

            // If any active questions exist, we cannot delete
            bool canDelete = !hasActiveQuestions;

            return Ok(new { canDelete });
        }
        #endregion
    }

}
