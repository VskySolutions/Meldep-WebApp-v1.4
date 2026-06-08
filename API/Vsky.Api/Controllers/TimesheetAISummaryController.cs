using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Models;
using Vsky.Services.Sites;
using Vsky.Api.ApiErrors;
using Vsky.Models;
using Vsky.Services.TimesheetAISummarys;

namespace Vsky.Api.Controllers
{
    [Route("timesheet-AI-summary")]
    public class TimesheetAISummaryController : BaseController
    {
        #region Define Services
        private readonly IMapper _mapper;
        private readonly ITimesheetAISummaryService _timesheetAISummaryService;
        private readonly ISiteService _siteService;
        #endregion

        #region Services Initializations
        public TimesheetAISummaryController
        (
            IMapper mapper,
            ITimesheetAISummaryService timesheetAISummaryService,
            ISiteService siteService
        )
        {
            _mapper = mapper;
            _timesheetAISummaryService = timesheetAISummaryService;
            _siteService = siteService;
        }
        #endregion

        #region GetAllTimesheetAISummary
        // Title: Get All TimesheetAISummary
        // Description: This endpoint fetches a list of Timesheet AI Summary based on the provided search criteria such as sorting, and pagination. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllTimesheetAISummary(TimesheetAISummarySearchModel searchModel)
        {
            try
            {
                // Fetch a list of Timesheet AI Summary on search criteria (sorting, pagination)
                var list = await _timesheetAISummaryService.GetAllTimesheetAISummary(
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                // Map the fetched list to a model suitable for the response
                var model = new TimesheetAISummaryListModel
                {
                    Data = _mapper.Map<IList<TimesheetAISummaryModel>>(list),
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

        #region GetTimesheetAISummaryDetailsById
        // Title: GetTimesheetAISummaryDetailsById
        // Description: This endpoint retrieves the details of a specific Timesheet AI Summary based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetTimesheetAISummaryDetailsById(string id)
        {
            try
            {
                // Fetch the Timesheet AI Summary entity by its ID from the service
                var entity = await _timesheetAISummaryService.GetTimesheetAISummaryDetailsById(id);

                // If the Timesheet AI Summary entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Timesheet AI Summary found with the specified id."));

                // Map the Timesheet AI Summary entity to a TimesheetAISummaryModel object
                var model = _mapper.Map<TimesheetAISummaryModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateTimesheetAISummary
        // Title: CreateTimesheetAISummary
        // Description: This endpoint handles the creation of a new Timesheet AI Summary. It maps the Timesheet AI Summary model to the Timesheet AI Summary entity, sets the creation details, and inserts the Timesheet AI Summary into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateTimesheetAISummary(TimesheetAISummaryModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var GetDateTime = _siteService.GetDateTime();

                    // Map the TimesheetAISummary model to the TimesheetAISummary entity
                    var entity = _mapper.Map<TimesheetAISummary>(model);
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _timesheetAISummaryService.InsertTimesheetAISummary(entity);

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

        #region UpdateTimesheetAISummary
        // Title: UpdateTimesheetAISummary
        // Description: This endpoint updates an existing Timesheet AI Summary by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimesheetAISummary(string id, TimesheetAISummaryModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var GetDateTime = _siteService.GetDateTime();

                    // Fetch the Timesheet AI Summary entity by its ID
                    var entity = await _timesheetAISummaryService.GetTimesheetAISummaryById(id);

                    // If no Timesheet AI Summary is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Timesheet AI Summary found with the specified id."));

                    entity.TaskId = model.TaskId;
                    entity.TimesheetLineId = model.TimesheetLineId;
                    entity.SummaryId = model.SummaryId;
                    entity.Summary = model.Summary;

                    entity.UpdatedOnUtc = GetDateTime;
                    _timesheetAISummaryService.UpdateTimesheetAISummary(entity);

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

        #region DeleteTimesheetAISummary
        // Title: DeleteTimesheetAISummaryById
        // Description: This endpoint deletes a Timesheet AI Summary based on the provided TimesheetAISummary ID. It first retrieves the TimesheetAISummary entity by ID, checks if it exists, and if so, deletes the TimesheetAISummary. If the TimesheetAISummary is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimesheetAISummary(string id)
        {
            try
            {
                // Fetch the TimesheetAISummary entity by its ID
                var entity = await _timesheetAISummaryService.GetTimesheetAISummaryById(id);
                // If no TimesheetAISummary is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No TimesheetAISummary found with the specified id."));

                // Delete the TimesheetAISummary using the TimesheetAISummary service
                _timesheetAISummaryService.DeleteTimesheetAISummary(entity);

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
