using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Numerics;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerBI.Api.Models;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DailyPlanners;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.Employees;
using Vsky.Services.Notifications;
using Vsky.Services.Sites;
using Vsky.Services.Timesheets;
using Vsky.Services.Users;

namespace Vsky.Api.Controllers
{
    [Route("daily-planner")]
    public class DailyPlannerController : BaseController
    {
        #region Define Services
        private readonly IMapper _mapper;
        private readonly GlobalVariable _globalVariable;
        private readonly IDailyPlannerService _dailyPlannerService;
        private readonly IDailyPlannerLineService _dailyPlannerLineService;
        private readonly ICommonService _commonService;
        private readonly ITimesheetService _timesheetService;
        private readonly ITimesheetLinesService _timesheetLinesService;
        private readonly ISiteService _siteService;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly IUserService _userService;
        public DailyPlannerController(
            IMapper mapper,
            GlobalVariable globalVariable,
            IDailyPlannerService dailyPlannerService, 
            IDailyPlannerLineService dailyPlannerLineService,
            ICommonService commonService, 
            ITimesheetService timesheetService,
            ITimesheetLinesService timesheetLinesService,
            ISiteService siteService,
            IDropDownService dropDownService,
            IDropDownTypeService dropDownTypeService,
            IMasterNotificationService masterNotificationService,
            IAzureBlobImageServices azureBlobImageServices,
            IUserService userService
        )
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _dailyPlannerService = dailyPlannerService;
            _dailyPlannerLineService = dailyPlannerLineService;
            _commonService = commonService;
            _timesheetService = timesheetService;
            _timesheetLinesService = timesheetLinesService;
            _siteService = siteService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _azureBlobImageServices = azureBlobImageServices;
            _userService = userService;
        }
        #endregion

        #region GetAllDailyPlannerForDashboard
        // Title: Get latest Three DailyPlanner data
        // Description: This endpoint fetches a list of daily planners 
        [HttpPost("Dashboardlist")]
        public IActionResult GetAllDailyPlannerForDashboard(DailyPlannerSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";
                // Fetch a list of daily planner
                var list = _dailyPlannerService.GetAllDailyPlannerDashboard(SiteId, createdBy, searchModel.EmployeeId, searchModel.ProjectId, searchModel.ActivityDate, searchModel.FromDate, searchModel.ToDate, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new DailyPlannerListModel
                {
                    Data = _mapper.Map<IList<DailyPlannerModel>>(list),
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

        #region GetAllDailyPlanner
        // Title: Get All DailyPlanner
        // Description: This endpoint fetches a list of daily planners based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllDailyPlanner(DailyPlannerSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";
                // Fetch a list of daily planner based on search criteria
                var list = _dailyPlannerService.GetAllDailyPlanner(SiteId, createdBy, searchModel.SearchText, searchModel.EmployeeId, searchModel.ProjectId, searchModel.ActivityDate, searchModel.FromDate, searchModel.ToDate, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                List<DailyPlanner> dailyPlannerList = new List<DailyPlanner>();
                foreach (var row in list)
                {
                    var obj = new DailyPlanner();
                    obj.Id = row.Id;
                    obj.DailyPlannerDate = row.DailyPlannerDate;
                    obj.User = row.User;
                    obj.IsForwordedToTimesheet = row.IsForwordedToTimesheet;

                    var lines = row.DailyPlannerLines.AsQueryable();

                    // Apply dynamic sorting if `searchModel.SortBy` is specified, otherwise apply default sorting.
                    if (!string.IsNullOrWhiteSpace(searchModel.SortBy))
                    {
                        lines = lines.OrderBy($"{searchModel.SortBy} {(searchModel.Descending ? "desc" : "asc")}");
                    }
                    else
                    {
                        // Apply default sorting
                        lines = lines.OrderBy(x => x.Project.Name)
                                     .ThenBy(x => x.ProjectModule.Name)
                                     .ThenBy(x => x.ProjectTask.Name)
                                     .ThenBy(x => x.ProjectActivity.Name);
                    }

                    // Materialize the query into a list.
                    var result = lines.ToList();
                    if (lines.Count() > 0)
                    {
                        foreach (var line in lines)
                        {
                            var obj2 = new DailyPlannerLine();
                            obj2.Id = line.Id;
                            obj2.Project = line.Project;
                            obj2.ProjectModule = line.ProjectModule;
                            obj2.ProjectTask = line.ProjectTask;
                            obj2.ProjectActivity = line.ProjectActivity;
                            obj2.Hours = line.Hours;
                            obj2.Description = line.Description;
                            obj.DailyPlannerLines.Add(obj2);
                        }
                    }
                    dailyPlannerList.Add(obj);
                }

                var dailyPlannerModelList = _mapper.Map<IList<DailyPlannerModel>>(dailyPlannerList);

                // Fetch dropdown type and values
                var activityDropDownType = await _dropDownTypeService.GetDropDownType(SiteId, "Project Activities");
                var activityDropdowns = await _dropDownService.GetDropDowns(activityDropDownType.Id);

                // Loop through mapped DailyPlannerModel and set ActivityNameDescription
                foreach (var planner in dailyPlannerModelList)
                {
                    foreach (var line in planner.DailyPlannerLines)
                    {
                        var activityName = line.ProjectActivity?.Name;
                        if (!string.IsNullOrEmpty(activityName))
                        {
                            var activityDropdownItem = activityDropdowns.FirstOrDefault(d => d.DropDownValue == activityName);
                            if (activityDropdownItem != null)
                            {
                                line.ActivityNameDescription = activityDropdownItem.Description;
                            }
                        }
                    }
                }

                // Map the fetched list to a model suitable for the response
                var model = new DailyPlannerListModel
                {
                    Data = dailyPlannerModelList,
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

        #region GetDailyPlannerById
        // Title: GetDailyPlannerById
        // Description: This endpoint retrieves the details of a specific planner based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDailyPlannerById(string id)
        {
            // Fetch the daily planner entity by its ID from the service
            var entity = await _dailyPlannerService.GetDailyPlannerById(id);
            // If the daily planner entity is not found, return a BadRequest response with an error message
            if (entity == null)
                return BadRequest(new BadRequestError("No daily planner found with the specified id."));

            // Map the daily planner entity to a DailyPlannerModel object
            var model = _mapper.Map<DailyPlannerModel>(entity);
            return Ok(model);
        }
        #endregion

        #region GetPlannerdetailsById
        [HttpGet("{id}/dailyplannerdetails")]
        public async Task<IActionResult> GetPlannerdetailsById(string id)
        {
            var entity = await _dailyPlannerService.GetDailyPlannerDetailsById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No company found with the specified id."));

            var model = _mapper.Map<DailyPlannerModel>(entity);
            return Ok(model);
        }
        #endregion

        #region CreateDailyPlanner
        // Title: CreateDailyPlanner
        // Description: This endpoint handles the creation of a new daily plan. it maps the daily planner model to the daily planner entity, sets the creation details, and inserts the daily planner into the database.
        [HttpPost]
        public async Task<IActionResult> CreateDailyPlanner(DailyPlannerModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    var UserData = await _userService.GetById(SiteId, LoggedUserId);
                    var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, UserData.Id);

                    //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);

                    string DailyPlannerId = null;
                    //Check if the planner date already exists
                    var data = await _dailyPlannerService.GetDailyPlannerByDate(SiteId, EmployeeId, model.DailyPlannerDate);
                    if (data == null)
                    {
                        // Map the Daily Planner model to the Daily Planner entity
                        var entity = _mapper.Map<DailyPlanner>(model);
                        entity.EmployeeId = EmployeeId;
                        entity.SiteId = SiteId;
                        entity.CreatedById = LoggedUserId;
                        entity.CreatedOnUtc = GetDateTime;
                        entity.UpdatedById = LoggedUserId;
                        entity.UpdatedOnUtc = GetDateTime;
                        _dailyPlannerService.InsertDailyPlanner(entity);

                        DailyPlannerId = entity.Id;
                    }
                    else
                        DailyPlannerId = data.Id;

                    if (model.DailyPlannerLineModel != null && model.DailyPlannerLineModel.Count() > 0)
                    {
                        var dailyPlannerLines = _dailyPlannerLineService.GetDailyPlannerLineByDailyPlanner(DailyPlannerId);
                        // Loop through each plan
                        foreach (var plan in model.DailyPlannerLineModel)
                        {
                            // Check if a similar line already exists
                            var existingLine = dailyPlannerLines.FirstOrDefault(x => x.Id == plan.Id && !x.Deleted);
                            if (existingLine == null && !plan.Deleted)
                            {
                                DailyPlannerLine plannerEntity = new DailyPlannerLine();
                                plannerEntity.Id = plan.Id;
                                plannerEntity.DailyPlannerId = DailyPlannerId;
                                plannerEntity.ProjectId = plan.ProjectId;
                                plannerEntity.ProjectModuleId = plan.ProjectModuleId;
                                plannerEntity.ProjectTaskId = plan.ProjectTaskId;
                                plannerEntity.ProjectActivityId = plan.ProjectActivityId;
                                plannerEntity.Hours = plan.Hours;

                                plannerEntity.Description = string.IsNullOrEmpty(plan.Description) ? ""
                                            : await _azureBlobImageServices
                                        .ProcessHtmlAndManageImagesAsync(
                                            plan.Description,
                                            SiteData.Name,
                                            "dailyplanner",
                                            plan.Id
                                        );

                                plannerEntity.CreatedById = LoggedUserId;
                                plannerEntity.CreatedOnUtc = GetDateTime;
                                plannerEntity.UpdatedById = LoggedUserId;
                                plannerEntity.UpdatedOnUtc = GetDateTime;
                                _dailyPlannerLineService.InsertDailyPlannerLine(plannerEntity);
                            }
                            else if (existingLine != null && !plan.Deleted)
                            {
                                // Update the existing DailyPlannerLine if found and not marked for deletion
                                existingLine.DailyPlannerId = DailyPlannerId;
                                existingLine.ProjectId = plan.ProjectId;
                                existingLine.ProjectModuleId = plan.ProjectModuleId;
                                existingLine.ProjectTaskId = plan.ProjectTaskId;
                                existingLine.ProjectActivityId = plan.ProjectActivityId;


                                existingLine.Description = string.IsNullOrEmpty(plan.Description) ? ""
                                           : await _azureBlobImageServices
                                        .ProcessHtmlAndManageImagesAsync(
                                            plan.Description,
                                            SiteData.Name,
                                            "dailyplanner",
                                            existingLine.Id,
                                            existingLine.Description
                                        );

                                existingLine.Hours = plan.Hours;
                                existingLine.Deleted = plan.Deleted;
                                existingLine.UpdatedById = LoggedUserId;
                                existingLine.UpdatedOnUtc = GetDateTime;
                                _dailyPlannerLineService.UpdateDailyPlannerLine(existingLine);
                            }
                        }
                    }
                    return Ok(new { DailyPlannerId, model.DailyPlannerLineModel });
                }
            }
            catch (Exception ex)
            {
                return ModelStateError(ModelState);
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateDailyPlanner
        // Title: UpdateDailyPlanner
        // Description: This endpoint updates an existing daily plan by its ID. It validates the daily planner model, updates the dailyplanner's details.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDailyPlanner(string id, DailyPlannerModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    var UserData = await _userService.GetById(SiteId, LoggedUserId);

                    // Fetch the daily planner entity by its ID
                    var entity = await _dailyPlannerService.GetDailyPlannerById(id);
                    // If no daily planner is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No daily planner found with the specified id."));


                    // Check if there are Daily Planner Lines to be added to the DailyPlanner
                    string DailyPlannerId = null;

                    //Check if the Timesheet date already exists
                    var dailyPlanData = await _dailyPlannerService.GetDailyPlannerByDate(SiteId, LoggedUserId, model.DailyPlannerDate, id);
                    if (dailyPlanData == null)
                    {
                        entity.DailyPlannerDate = model.DailyPlannerDate;
                        entity.UpdatedById = LoggedUserId;
                        entity.UpdatedOnUtc = GetDateTime;
                        _dailyPlannerService.UpdateDailyPlanner(entity);

                        DailyPlannerId = id;
                    }
                    else
                    {
                        // Delete the plan using the dailyPlannerService
                        _dailyPlannerService.DeleteDailyPlanner(entity);
                        DailyPlannerId = dailyPlanData.Id;
                    }

                    var addList = new List<DailyPlannerLine>();
                    var updateList = new List<DailyPlannerLine>();
                    var deleteList = new List<DailyPlannerLine>();

                    if (!model.IsForwordedToTimesheet)
                    {
                        if (model.DailyPlannerLineModel != null && model.DailyPlannerLineModel.Count() > 0)
                        {
                            // Loop through each Daily Planner Line
                            foreach (var planner in model.DailyPlannerLineModel)
                            {
                                // planner.DailyPlannerId = id;
                                planner.DailyPlannerId = DailyPlannerId;
                                if (planner.Deleted)
                                {
                                    var toDelete = await _dailyPlannerLineService.GetDailyPlannerLineById(planner.Id);
                                    if (toDelete == null)
                                    {
                                        continue;
                                    }
                                    toDelete.DailyPlannerId = planner.DailyPlannerId;
                                    deleteList.Add(toDelete);
                                }
                                var existingPlanner = await _dailyPlannerLineService.GetDailyPlannerLineById(planner.Id);
                                if (existingPlanner == null && !planner.Deleted)
                                {
                                    var plannerEntity = _mapper.Map<DailyPlannerLine>(planner);
                                    plannerEntity.DailyPlannerId = planner.DailyPlannerId;

                                    plannerEntity.Description = string.IsNullOrEmpty(planner.Description) ? ""
                                           : await _azureBlobImageServices
                                            .ProcessHtmlAndManageImagesAsync(
                                                planner.Description,
                                                SiteData.Name,
                                                "dailyplanner",
                                                planner.Id
                                            );

                                    plannerEntity.CreatedById = LoggedUserId;
                                    plannerEntity.UpdatedById = LoggedUserId;
                                    plannerEntity.CreatedOnUtc = GetDateTime;
                                    plannerEntity.UpdatedOnUtc = GetDateTime;
                                    addList.Add(plannerEntity);
                                }
                                else if (existingPlanner != null && !planner.Deleted)
                                {
                                    existingPlanner.DailyPlannerId = planner.DailyPlannerId;
                                    existingPlanner.ProjectId = planner.ProjectId;
                                    existingPlanner.ProjectModuleId = planner.ProjectModuleId;
                                    existingPlanner.ProjectTaskId = planner.ProjectTaskId;
                                    existingPlanner.ProjectActivityId = planner.ProjectActivityId;
                                    //existingPlanner.Description = planner.Description;
                                    existingPlanner.Hours = planner.Hours;

                                    existingPlanner.Description = string.IsNullOrEmpty(planner.Description) ? ""
                                          : await _azureBlobImageServices
                                            .ProcessHtmlAndManageImagesAsync(
                                                planner.Description,
                                                SiteData.Name,
                                                "dailyplanner",
                                                existingPlanner.Id,
                                                existingPlanner.Description
                                            );

                                    existingPlanner.Deleted = planner.Deleted;
                                    existingPlanner.UpdatedOnUtc = GetDateTime;
                                    existingPlanner.UpdatedById = LoggedUserId;
                                    updateList.Add(existingPlanner);
                                }
                            }
                            if (addList.Count > 0)
                                _dailyPlannerLineService.InsertDailyPlannerLineList(addList);

                            if (updateList.Count > 0)
                                _dailyPlannerLineService.UpdateDailyPlannerLineList(updateList);

                            if (deleteList.Count > 0)
                                _dailyPlannerLineService.DeleteDailyPlannerLineList(deleteList);
                        }
                    }

                    if (model.IsForwordedToTimesheet)
                    {
                        //var employeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                        var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, UserData.Id);
                        string TimesheetId = null;
                        entity.IsForwordedToTimesheet = model.IsForwordedToTimesheet;
                        //Check if the Timesheet date already exists
                        var data = await _timesheetService.GetTimesheetByDate(SiteId, LoggedUserId, model.DailyPlannerDate);
                        if (data == null)
                        {
                            // Map the Timesheet model to the Timesheet entity
                            Timesheet timesheetentity = new Timesheet();
                            timesheetentity.EmployeeId = null;
                            timesheetentity.TimesheetDate = model.DailyPlannerDate;
                            timesheetentity.SiteId = SiteId;
                            timesheetentity.EmployeeId = employeeId;
                            timesheetentity.CreatedById = LoggedUserId;
                            timesheetentity.UpdatedById = LoggedUserId;
                            timesheetentity.CreatedOnUtc = GetDateTime;
                            timesheetentity.UpdatedOnUtc = GetDateTime;
                            _timesheetService.InsertTimesheet(timesheetentity);

                            TimesheetId = timesheetentity.Id;
                        }
                        else
                            TimesheetId = data.Id;

                        if (model.DailyPlannerLineModel != null && model.DailyPlannerLineModel.Count() > 0)
                        {
                            var timesheetLines = _timesheetLinesService.GetTimesheetLinesByTimesheet(TimesheetId);
                            // Loop through each Project
                            foreach (var planner in model.DailyPlannerLineModel)
                            {
                                var existingLine = timesheetLines.FirstOrDefault(x =>
                                    x.Id == planner.Id && !x.Deleted);

                                if (existingLine == null && !planner.Deleted)
                                {
                                    TimesheetLines timesheetEntity = new TimesheetLines();
                                    timesheetEntity.Id = planner.Id;
                                    timesheetEntity.TimesheetId = TimesheetId;
                                    timesheetEntity.Description = planner.Description;
                                    timesheetEntity.Hours = planner.Hours;
                                    timesheetEntity.ProjectId = planner.ProjectId;
                                    timesheetEntity.ProjectModuleId = planner.ProjectModuleId;
                                    timesheetEntity.ProjectTaskId = planner.ProjectTaskId;
                                    timesheetEntity.ProjectActivityId = planner.ProjectActivityId;
                                    timesheetEntity.CreatedById = LoggedUserId;
                                    timesheetEntity.CreatedOnUtc = GetDateTime;
                                    timesheetEntity.UpdatedById = LoggedUserId;
                                    timesheetEntity.UpdatedOnUtc = GetDateTime;
                                    _timesheetLinesService.InsertTimesheetLines(timesheetEntity);
                                }
                                else if (existingLine != null)
                                {
                                    existingLine.ProjectId = planner.ProjectId;
                                    existingLine.ProjectModuleId = planner.ProjectModuleId;
                                    existingLine.ProjectTaskId = planner.ProjectTaskId;
                                    existingLine.ProjectActivityId = planner.ProjectActivityId;
                                    existingLine.Description = planner.Description;
                                    existingLine.Hours = planner.Hours;
                                    existingLine.Deleted = planner.Deleted;
                                    existingLine.UpdatedById = LoggedUserId;
                                    existingLine.UpdatedOnUtc = GetDateTime;
                                    _timesheetLinesService.UpdateTimesheetLines(existingLine);

                                }
                            }
                        }
                    }
                    _dailyPlannerService.UpdateDailyPlanner(entity);

                    return Ok(new { DailyPlannerId = id, model.DailyPlannerLineModel });
                }
            }
            catch (Exception ex)
            {
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteDailyPlanner
        // Title: DeleteDailyPlanner
        // Description: This endpoint deletes a daily planner based on the provided daily planner ID. It first retrieves the daily planner entity by ID, checks if it exists, and if so, deletes the daily planner. If the daily planner is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDailyPlanner(string id)
        {
            try
            {
                // Fetch the daily planner entity by its ID
                var entity = await _dailyPlannerService.GetDailyPlannerById(id);
                // If no daily planner is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No daily planner found with the specified id."));

                // Delete the daily planner using the daily planner service
                _dailyPlannerService.DeleteDailyPlanner(entity);

                var deleteList = new List<DailyPlannerLine>();

                var existingPlanner = _dailyPlannerLineService.GetDailyPlannerLineByDailyPlanner(id);
                if (existingPlanner.Count() > 0)
                {
                    foreach (var item in existingPlanner)
                    {
                        var existingLine = await _dailyPlannerLineService.GetDailyPlannerLineById(item.Id);
                        //existingLine.DailyPlannerId = item.DailyPlannerId;
                        deleteList.Add(existingLine);
                    }
                }
                if (deleteList.Count > 0)
                {
                    _dailyPlannerLineService.DeleteDailyPlannerLineList(deleteList);
                }
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
