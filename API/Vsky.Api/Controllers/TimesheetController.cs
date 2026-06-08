using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using AngleSharp.Dom;
using AutoMapper;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.PowerBI.Api.Models;
using OfficeOpenXml;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.Employees;
using Vsky.Services.ProjectActivities;
using Vsky.Services.Projects;
using Vsky.Services.ProjectTasks;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.Timesheets;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Vsky.Api.Models.TimesheetModel;

namespace Vsky.Api.Controllers
{
    [Route("Timesheet")]
    public class TimesheetController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ITimesheetService _timesheetService;
        private readonly ITimesheetLinesService _timesheetLinesService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IProjectActivityService _projectActivityService;
        private readonly IProjectService _projectService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly IProjectTaskService _projectTaskService;
        #endregion

        #region Services Initializations
        public TimesheetController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ITimesheetService timesheetService,
            ITimesheetLinesService timesheetLinesService,
            ICommonService commonService,
            ISiteService siteService,
            IProjectActivityService projectActivityService,
            IProjectService projectService,
            ISitesModifiedLogsService sitesModifiedLogsService,
            IDropDownService dropDownService,
            IDropDownTypeService dropDownTypeService,
            IAzureBlobImageServices azureBlobImageServices,
            IProjectTaskService projectTaskService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _timesheetService = timesheetService;
            _timesheetLinesService = timesheetLinesService;
            _commonService = commonService;
            _siteService = siteService;
            _projectActivityService = projectActivityService;
            _projectService = projectService;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _azureBlobImageServices = azureBlobImageServices;
            _projectTaskService = projectTaskService;
        }
        #endregion

        #region GetAllTimesheet
        // Title:Get All Timesheet
        // Description: This endpoint fetches a list of timesheets based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllTimesheets(TimesheetSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";
                // Fetch a list of timesheets based on search criteria
                var list = _timesheetService.GetAllTimesheets(
                    SiteId, 
                    createdBy, 
                    searchModel.SearchText, 
                    searchModel.EmployeeId, 
                    searchModel.ProjectId, 
                    searchModel.ProjectModuleId, 
                    searchModel.ProjectTaskId, 
                    searchModel.ProjectActivityId, 
                    searchModel.ActivityDate, 
                    searchModel.FromDate, 
                    searchModel.ToDate,
                    searchModel.ThisWeek,
                    searchModel.LastNumberOfWeeks,
                    searchModel.SortBy, 
                    searchModel.Descending,
                    searchModel.Page, 
                    searchModel.PageSize
                );

                List<Timesheet> timesheetList = new List<Timesheet>();
                foreach (var row in list)
                {
                    var obj = new Timesheet();
                    obj.Id = row.Id;
                    obj.TimesheetDate = row.TimesheetDate;
                    obj.User = row.User;
                    obj.IsActionVisible = row.IsActionVisible;

                    var lines = row.TimesheetLines.AsQueryable();
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
                                     .ThenBy(x => x.Task.Name)
                                     .ThenByDescending(x => x.Hours);
                    }

                    // Materialize the query into a list.
                    var result = lines.ToList();

                    if (lines.Count() > 0)
                    {
                        foreach (var line in lines)
                        {
                            var obj2 = new TimesheetLines();
                            obj2.Id = line.Id;
                            obj2.Project = line.Project;
                            obj2.ProjectModule = line.ProjectModule;
                            obj2.Task = line.Task;
                            obj2.Task = line.Task;
                            obj2.ProjectActivity = line.ProjectActivity;
                            obj2.Hours = line.Hours;
                            obj2.Description = line.Description;
                            obj.TimesheetLines.Add(obj2);
                        }
                    }

                    timesheetList.Add(obj);
                }
                var timesheetModelList = _mapper.Map<IList<TimesheetModel>>(timesheetList);

                // Fetch dropdown type and values
                var activityDropDownType = await _dropDownTypeService.GetDropDownType(SiteId, "Project Activities");
                var activityDropdowns = await _dropDownService.GetDropDowns(activityDropDownType.Id);

                // Loop through mapped TimesheetModel and set ActivityNameDescription
                foreach (var timesheet in timesheetModelList)
                {
                    foreach (var line in timesheet.TimesheetLines)
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
                var model = new TimesheetListModel
                {
                    Data = timesheetModelList,
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("timesheetBillingList")]
        public IActionResult GetAllBillingTimesheet(TimesheetSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of timesheets based on search criteria
                var list = _timesheetService.GetAllBillingTimesheets(
                    SiteId, 
                    searchModel.SearchText, 
                    searchModel.FromDate, 
                    searchModel.ToDate, 
                    searchModel.ProjectId, 
                    searchModel.ProjectModuleIds, 
                    searchModel.ProjectTaskIds, 
                    searchModel.CustomerIds, 
                    searchModel.CompanyContactIds, 
                    searchModel.SortBy, 
                    searchModel.Descending, 
                    searchModel.Page, 
                    searchModel.PageSize
                );

                List<TimesheetLines> timesheetLinesList = new List<TimesheetLines>();
                foreach (var row in list)
                {
                    var obj = new TimesheetLines()
                    {
                        Id = row.Id,
                        Hours = row.Hours,
                        BillableHours = row.BillableHours,
                        Description = row.Description,
                        BillableCreatedOnUtc = row.BillableCreatedOnUtc,
                        Project = new Project
                        {
                            Id = row.Project.Id,
                            Name = row.Project.Name
                        },
                        ProjectModule = new ProjectModule
                        {
                            Id = row.ProjectModule.Id,
                            Name = row.ProjectModule.Name
                        },
                        Task = new ProjectTask
                        {
                            Id = row.Task.Id,
                            Name = row.Task.Name
                        },
                        ProjectActivity = new ProjectActivity
                        {
                            Id = row.ProjectActivity.Id,
                            Name = row.ProjectActivity.Name
                        },
                        BillableCreatedBy = new ApplicationUser
                        {
                            Id = row.BillableCreatedBy.Id,
                            Person = new Person
                            {
                                Id = row.BillableCreatedBy.PersonId,
                                FirstName = row.BillableCreatedBy.Person.FirstName,
                                LastName = row.BillableCreatedBy.Person.LastName,
                                FullName = row.BillableCreatedBy.Person.FirstName + " " + row.BillableCreatedBy.Person.LastName
                            }
                        },
                        Timesheet = new Timesheet
                        {
                            Id = row.Timesheet.Id,
                            TimesheetDate = row.Timesheet.TimesheetDate,
                            User = new ApplicationUser
                            {
                                Id = row.Timesheet.User.Id,
                                UserName = row.Timesheet.User.UserName,
                                Person = new Person
                                {
                                    Id = row.Timesheet.User.PersonId,
                                    FirstName = row.Timesheet.User.Person.FirstName,
                                    LastName = row.Timesheet.User.Person.LastName,
                                    FullName = row.Timesheet.User.Person.FirstName + " " + row.Timesheet.User.Person.LastName
                                }
                            }
                        }
                    };
                    timesheetLinesList.Add(obj);
                }
                // Map the fetched list to a model suitable for the response
                var model = new TimesheetLinesListModel
                {
                    Data = _mapper.Map<IList<TimesheetLinesModel>>(timesheetLinesList),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("groupedBillingTimesheetList")]
        public IActionResult GetGroupedBillingTimesheet(TimesheetSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                // Fetch grouped timesheet data
                var list = _timesheetService.GetGroupedBillingTimesheetsByEmployee(
                    SiteId, 
                    searchModel.SearchText, 
                    searchModel.FromDate, 
                    searchModel.ToDate, 
                    searchModel.ProjectId, 
                    searchModel.ProjectModuleIds, 
                    searchModel.ProjectTaskIds, 
                    searchModel.CustomerIds, 
                    searchModel.CompanyContactIds, 
                    searchModel.SortBy, 
                    searchModel.Descending, 
                    searchModel.Page, 
                    searchModel.PageSize
                );

                // Map directly using AutoMapper
                var model = new TimesheetLinesListModel
                {
                    Data = _mapper.Map<IList<TimesheetLinesModel>>(list),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("timesheetWeeklyList")]
        public async Task<IActionResult> GetAllTimesheetByWeek(TimesheetSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                var data = await _timesheetService.GetAllTimesheetByWeek(SiteId, EmployeeId, searchModel.FromDate, searchModel.ToDate);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetTimesheetTotalHoursByWeekAndMonth 
        // Title:Get All Timesheet
        // Description: This endpoint fetches a list of timesheets based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("total-hours-week-and-month")]
        public async Task<IActionResult> GetTimesheetTotalHoursByWeekAndMonth(TimesheetSearchModel searchModel)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var SiteData = await _siteService.GetById(SiteId);
            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

            var list = _timesheetService.GetAllTimesheets(
                SiteId, 
                LoggedUserId, 
                searchModel.SearchText, 
                searchModel.EmployeeId,
                searchModel.ProjectId, 
                searchModel.ProjectModuleId, 
                searchModel.ProjectTaskId,
                searchModel.ProjectActivityId, 
                searchModel.ActivityDate, 
                searchModel.FromDate,
                searchModel.ToDate,
                searchModel.ThisWeek,
                searchModel.LastNumberOfWeeks, 
                searchModel.SortBy, 
                searchModel.Descending,
                searchModel.Page, 
                searchModel.PageSize
            );

            if (searchModel.ViewType == "weekly")
            {
                int diff = (7 + (GetDateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime startDate = GetDateTime.AddDays(-diff).Date;
                DateTime endDate = startDate.AddDays(6).Date;

                // Build columns for each day
                var columns = new List<TimesheetWeeklyMonthlyColumnModel>();
                int index = 1;
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    columns.Add(new TimesheetWeeklyMonthlyColumnModel
                    {
                        Index = index++,
                        Date = date,
                        Day = date.ToString("dddd")
                    });
                }

                // Create one row with total hours per day
                var rowData = new Dictionary<string, object>();
                index = 1;
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var totalHours = list
                        .Where(t => t.TimesheetDate.HasValue && t.TimesheetDate.Value.Date == date)
                        .SelectMany(t => t.TimesheetLines)
                        .Sum(l => l.Hours);

                    rowData[$"col{index++}"] = totalHours;
                }

                var model = new TimesheetWeeklyMonthlyHoursModel
                {
                    Columns = columns,
                    Rows = new List<Dictionary<string, object>> { rowData },
                    ViewType = "weekly"
                };

                return Ok(model);
            }
            else if (searchModel.ViewType == "monthly")
            {
                DateTime monthStart = new DateTime(GetDateTime.Year, GetDateTime.Month, 1);
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                // Start from Sunday of the first week containing the first of the month
                DateTime current = monthStart.AddDays(-(int)monthStart.DayOfWeek);

                var columns = new List<TimesheetWeeklyMonthlyColumnModel>();
                var rowData = new Dictionary<string, object>();
                int colIndex = 1;

                while (current <= monthEnd)
                {
                    DateTime start = current;
                    DateTime end = current.AddDays(6);

                    columns.Add(new TimesheetWeeklyMonthlyColumnModel
                    {
                        Index = colIndex,
                        Date = start,
                        DisplayDateRange = $"{start:MM/dd} - {end:MM/dd}",
                        DateTooltip = $"{start:MM/dd/yyyy} - {end:MM/dd/yyyy}"
                    });

                    // Total hours for this week
                    var totalHours = list
                        .Where(t =>
                            t.TimesheetDate.HasValue &&
                            t.TimesheetDate.Value.Date >= start &&
                            t.TimesheetDate.Value.Date <= end)
                        .SelectMany(t => t.TimesheetLines)
                        .Sum(l => l.Hours);

                    rowData[$"col{colIndex}"] = totalHours;

                    colIndex++;
                    current = current.AddDays(7);
                }

                var model = new TimesheetWeeklyMonthlyHoursModel
                {
                    Columns = columns,
                    Rows = new List<Dictionary<string, object>> { rowData },
                    ViewType = "monthly"
                };

                return Ok(model);
            }

            return Ok(new TimesheetWeeklyMonthlyHoursModel()); // fallback
        }
        #endregion

        #region GetAllTimesheetForDashboard
        // Title:Get All Timesheet
        // Description: This endpoint fetches a list of timesheets based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("Dashboardlist")]
        public IActionResult GetAllTimesheetForDashboard(TimesheetSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";
                // Fetch a list of timesheets based on search criteria
                var list = _timesheetService.GetAllTimesheetsForDashboard(SiteId, createdBy, searchModel.EmployeeId, searchModel.ProjectId, searchModel.ProjectModuleId, searchModel.ProjectTaskId, searchModel.ProjectActivityId, searchModel.ActivityDate, searchModel.FromDate, searchModel.ToDate, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                // Map the fetched list to a model suitable for the response
                var model = new TimesheetListModel
                {
                    Data = _mapper.Map<IList<TimesheetModel>>(list),
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

        #region GetAllTimesheetListForDropdown
        // Title: GetAllTimesheetListForDropdown
        // Description: This endpoint retrieves the details of a specific timesheet based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllTimesheetListForDropdown()
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _timesheetService.GetAllTimesheetListForDropdown(SiteId);
            var model = _mapper.Map<List<TimesheetModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetTimesheetById
        // Title: GetTimesheetById
        // Description: This endpoint retrieves the details of a specific timesheet based on its unique identifier (ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTimesheetById(string id)
        {
            try
            {
                // Fetch the timesheet entity by its ID from the service
                var entity = await _timesheetService.GetTimesheetDetailsById(id);
                // If the timesheet entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No timesheet found with the specified id."));

                // Map the timesheet entity to a TimesheetModel object
                var model = _mapper.Map<TimesheetModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        [HttpGet("timesheet-lines-detailsbyids")]
        public async Task<IActionResult> GetTimesheetLinesDetailsByIds([FromQuery] string ids)
        {
            var loggedUserId = User.GetLoggedInUserId<string>();
            var siteId = _globalVariable.SiteId;

            if (string.IsNullOrEmpty(ids))
                return BadRequest(new BadRequestError("Timesheet line IDs are missing."));

            var timesheetLineIds = ids.Split(',');
            var entity = await _timesheetLinesService.GetTimesheetLinesDetailsByIds(timesheetLineIds);

            if (entity == null || !entity.Any())
                return BadRequest(new BadRequestError("No timesheet lines found with the specified IDs."));

            var model = _mapper.Map<List<TimesheetLinesModel>>(entity);

            var activityDropDownType = await _dropDownTypeService.GetDropDownType(siteId, "Project Activities");
            var activityDropdowns = await _dropDownService.GetDropDowns(activityDropDownType.Id);
            foreach (var item in model)
            {
                if (!string.IsNullOrEmpty(item.ProjectActivity.Name))
                {
                    var activityDropdownItem = activityDropdowns
                        .FirstOrDefault(d => d.DropDownValue == item.ProjectActivity.Name);

                    if (activityDropdownItem != null)
                    {
                        item.ActivityNameDescription = activityDropdownItem.Description;
                    }
                }
            }

            return Ok(model);
        }

        #region CreateTimesheet
        // Title: CreateTimesheet
        // Description: This endpoint handles the creation of a new timesheet. It maps the timesheet model to the timesheet entity, sets the creation details, and inserts the timesheet into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateTimesheet(TimesheetModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    var projectOpenStatusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Project Status", "Open");
                    var projectInProgressStatusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Project Status", "In progress");

                    //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                    var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
                    string TimesheetId = null;
                    //Check if the Timesheet date already exists
                    var data = await _timesheetService.GetTimesheetByDate(SiteId, LoggedUserId, model.TimesheetDate);
                    if (data == null)
                    {
                        // Map the Timesheet model to the Timesheet entity
                        var entity = _mapper.Map<Timesheet>(model);
                        entity.Id = Guid.NewGuid().ToString();
                        //Get Current User data
                        entity.EmployeeId = EmployeeId;
                        entity.SiteId = SiteId;
                        entity.CreatedById = LoggedUserId;
                        entity.CreatedOnUtc = GetDateTime;
                        entity.UpdatedOnUtc = GetDateTime;
                        entity.UpdatedById = LoggedUserId;
                        _timesheetService.InsertTimesheet(entity);

                        TimesheetId = entity.Id;
                    }
                    else
                        TimesheetId = data.Id;

                    if (model.TimesheetLineModel != null && model.TimesheetLineModel.Count() > 0)
                    {
                        var timesheetLines = _timesheetLinesService.GetTimesheetLinesByTimesheet(TimesheetId);
                        // Loop through each Project
                        foreach (var timesheet in model.TimesheetLineModel)
                        {
                            var existingLine = timesheetLines.FirstOrDefault(x => x.Id == timesheet.Id && !x.Deleted);
                            (string ProjectId, string ProjectModuleId) taskData = (null, null);

                            if (string.IsNullOrEmpty(timesheet.ProjectId) || string.IsNullOrEmpty(timesheet.ProjectModuleId))
                            {
                                taskData = await _projectTaskService.GetProjectIdAndProjectModuleIdByTaskId(timesheet.ProjectTaskId);
                            }

                            string ProjectId = !string.IsNullOrEmpty(timesheet.ProjectId)
                               ? timesheet.ProjectId
                               : taskData.ProjectId;

                            string ProjectModuleId = !string.IsNullOrEmpty(timesheet.ProjectModuleId)
                                ? timesheet.ProjectModuleId
                                : taskData.ProjectModuleId;

                            if (existingLine == null && !timesheet.Deleted)
                            {
                                TimesheetLines timesheetEntity = new TimesheetLines();
                                timesheetEntity.Id = Guid.NewGuid().ToString();
                                //timesheetEntity.Id = timesheet.Id;
                                timesheetEntity.MeetingUId = timesheet.MeetingUId;
                                timesheetEntity.TimesheetId = TimesheetId;
                                timesheetEntity.Hours = timesheet.Hours;
                                timesheetEntity.ProjectId = ProjectId;
                                timesheetEntity.ProjectModuleId = ProjectModuleId;
                                timesheetEntity.ProjectTaskId = timesheet.ProjectTaskId;
                                timesheetEntity.ProjectActivityId = timesheet.ProjectActivityId;

                                timesheetEntity.Description = string.IsNullOrEmpty(timesheet.Description) ? ""
                                            : await _azureBlobImageServices.ProcessHtmlAndManageImagesAsync(
                                            timesheet.Description,
                                            SiteData.Name,
                                            "timesheet",
                                            timesheetEntity.Id);

                                timesheetEntity.CreatedById = LoggedUserId;
                                timesheetEntity.CreatedOnUtc = GetDateTime;
                                timesheetEntity.UpdatedById = LoggedUserId;
                                timesheetEntity.UpdatedOnUtc = GetDateTime;
                                _timesheetLinesService.InsertTimesheetLines(timesheetEntity);

                                // set timesheetLineId
                                timesheet.Id = timesheetEntity.Id;
                            }
                            else if (existingLine != null && !timesheet.Deleted)
                            {
                                existingLine.ProjectId = ProjectId;
                                existingLine.ProjectModuleId = ProjectModuleId;
                                existingLine.ProjectTaskId = timesheet.ProjectTaskId;
                                existingLine.ProjectActivityId = timesheet.ProjectActivityId;

                                existingLine.Description = string.IsNullOrEmpty(timesheet.Description) ? "" : await _azureBlobImageServices
                                                        .ProcessHtmlAndManageImagesAsync(
                                                            timesheet.Description,
                                                            SiteData.Name,
                                                            "timesheet",
                                                            existingLine.Id,
                                                            existingLine.Description);

                                existingLine.Hours = timesheet.Hours;
                                existingLine.UpdatedById = LoggedUserId;
                                existingLine.UpdatedOnUtc = GetDateTime;
                                _timesheetLinesService.UpdateTimesheetLines(existingLine);
                            }

                            var projectData = await _projectService.GetById(timesheet.ProjectId);
                            if (projectData != null)
                            {
                                if (projectData.ProjectStatusId == projectOpenStatusId)
                                {
                                    if (!string.IsNullOrEmpty(projectInProgressStatusId))
                                    {
                                        projectData.ProjectStatusId = projectInProgressStatusId;
                                        projectData.UpdatedById = LoggedUserId;
                                        projectData.UpdatedOnUtc = GetDateTime;
                                        _projectService.UpdateProject(projectData);

                                        //AddSiteModifiedLogs(SiteId, "Project Status", projectData.Name, timesheet.ProjectId, "In progress", "In progress", LoggedUserId, GetDateTime);
                                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Projects", projectData.Id, projectData.Name, projectData.Id, projectData.Name, "Project Status", "In progress", LoggedUserId, GetDateTime);
                                    }
                                }
                            }
                        }
                    }
                    return Ok(new { TimesheetId, model.TimesheetLineModel });
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

        #region UpdateTimesheet     
        // Title: UpdateTimesheet
        // Description: This endpoint updates an existing timesheet by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimesheet(string id, TimesheetModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    
                    var projectOpenStatusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Project Status", "Open");
                    var projectInProgressStatusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Project Status", "In progress");

                    // Fetch the timesheet entity by its ID
                    var entity = await _timesheetService.GetTimesheetById(id);
                    // If no timesheet is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No timesheet found with the specified id."));

                    string TimesheetId = null;
                    //Check if the Timesheet date already exists
                    var timesheetData = await _timesheetService.GetTimesheetByDate(SiteId, LoggedUserId, model.TimesheetDate, id);
                    if (timesheetData == null)
                    {
                        entity.TimesheetDate = model.TimesheetDate;
                        entity.UpdatedById = LoggedUserId;
                        entity.UpdatedOnUtc = GetDateTime;
                        _timesheetService.UpdateTimesheet(entity);

                        TimesheetId = id;
                    }
                    else
                    {
                        // Delete the plan using the dailyPlannerService
                        _timesheetService.DeleteTimesheet(entity);
                        TimesheetId = timesheetData.Id;
                    }

                    var addList = new List<TimesheetLines>();
                    var updateList = new List<TimesheetLines>();
                    var deleteList = new List<TimesheetLines>();

                    if (model.TimesheetLineModel != null && model.TimesheetLineModel.Count() > 0)
                    {
                        // Loop through each Daily Planner Line
                        foreach (var timesheet in model.TimesheetLineModel)
                        {
                            var projectData = await _projectService.GetById(timesheet.ProjectId);
                            timesheet.TimesheetId = id;
                            if (timesheet.Deleted)
                            {
                                var toDelete = await _timesheetLinesService.GetTimesheetLinesById(timesheet.Id);
                                if (toDelete == null)
                                {
                                    continue;
                                }
                                toDelete.TimesheetId = timesheet.TimesheetId;
                                deleteList.Add(toDelete);
                            }
                            var existingTimesheet = await _timesheetLinesService.GetTimesheetLinesById(timesheet.Id);
                            (string ProjectId, string ProjectModuleId) taskData = (null, null);

                            if (string.IsNullOrEmpty(timesheet.ProjectId) || string.IsNullOrEmpty(timesheet.ProjectModuleId))
                            {
                                taskData = await _projectTaskService.GetProjectIdAndProjectModuleIdByTaskId(timesheet.ProjectTaskId);
                            }

                            string ProjectId = !string.IsNullOrEmpty(timesheet.ProjectId)
                                   ? timesheet.ProjectId
                                   : taskData.ProjectId;

                            string ProjectModuleId = !string.IsNullOrEmpty(timesheet.ProjectModuleId)
                                ? timesheet.ProjectModuleId
                                : taskData.ProjectModuleId;

                            if (existingTimesheet == null)
                            {
                                var timesheetEntity = _mapper.Map<TimesheetLines>(timesheet);
                                timesheetEntity.Id = Guid.NewGuid().ToString();
                                timesheetEntity.TimesheetId = timesheet.TimesheetId;
                                timesheetEntity.ProjectId = ProjectId;
                                timesheetEntity.ProjectModuleId = ProjectModuleId;

                                timesheetEntity.Description = string.IsNullOrEmpty(timesheet.Description) ? "" : await _azureBlobImageServices
                                         .ProcessHtmlAndManageImagesAsync(
                                             timesheet.Description,
                                             SiteData.Name,
                                             "timesheet",
                                             timesheet.Id);

                                timesheetEntity.CreatedById = LoggedUserId;
                                timesheetEntity.CreatedOnUtc = GetDateTime;
                                timesheetEntity.UpdatedById = LoggedUserId;
                                timesheetEntity.UpdatedOnUtc = GetDateTime;
                                addList.Add(timesheetEntity);

                                timesheet.Id = timesheetEntity.Id;
                            }
                            else if (existingTimesheet != null && !timesheet.Deleted)
                            {
                                //existingTimesheet.TimesheetId = timesheet.TimesheetId;
                                existingTimesheet.TimesheetId = TimesheetId;
                                existingTimesheet.ProjectId = ProjectId;
                                existingTimesheet.ProjectModuleId = ProjectModuleId;
                                existingTimesheet.ProjectTaskId = timesheet.ProjectTaskId;
                                existingTimesheet.ProjectActivityId = timesheet.ProjectActivityId;
                                //existingTimesheet.Description = timesheet.Description;
                                existingTimesheet.Hours = timesheet.Hours;
                                existingTimesheet.ProjectActivityId = timesheet.ProjectActivityId;

                                existingTimesheet.Description = string.IsNullOrEmpty(timesheet.Description) ? ""
                                                               : await _azureBlobImageServices.ProcessHtmlAndManageImagesAsync(
                                                                timesheet.Description,
                                                                SiteData.Name,
                                                                "timesheet",
                                                                timesheet.Id,
                                                                timesheet.Description);

                                existingTimesheet.Deleted = timesheet.Deleted;
                                existingTimesheet.UpdatedOnUtc = GetDateTime;
                                existingTimesheet.UpdatedById = LoggedUserId;
                                updateList.Add(existingTimesheet);

                                timesheet.Id = existingTimesheet.Id;
                            }

                            if (projectData != null)
                            {
                                if (projectData.ProjectStatusId == projectOpenStatusId)
                                {
                                    if (!string.IsNullOrEmpty(projectInProgressStatusId))
                                    {
                                        projectData.ProjectStatusId = projectInProgressStatusId;
                                        projectData.UpdatedById = LoggedUserId;
                                        projectData.UpdatedOnUtc = GetDateTime;
                                        _projectService.UpdateProject(projectData);

                                        //AddSiteModifiedLogs(SiteId, "Project Status", projectData.Name, timesheet.ProjectId, "In progress", "In progress", LoggedUserId, GetDateTime);
                                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "Projects", projectData.Id, projectData.Name, projectData.Id, projectData.Name, "Project Status", "In progress", LoggedUserId, GetDateTime);
                                    }
                                }
                            }
                        }
                        if (addList.Count > 0)
                            _timesheetLinesService.InsertTimesheetLinesList(addList);

                        if (updateList.Count > 0)
                            _timesheetLinesService.UpdateTimesheetLinesList(updateList);
                        if (deleteList.Count > 0)
                            _timesheetLinesService.DeleteTimesheetLinesList(deleteList);

                    }
                    _timesheetService.UpdateTimesheet(entity);

                    return Ok(new { TimesheetId = TimesheetId, model.TimesheetLineModel });
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

        #region DeleteTimesheet
        // Title: DeleteTimesheet
        // Description: This endpoint deletes a timesheet based on the provided timesheet ID. It first retrieves the timesheet entity by ID, checks if it exists, and if so, deletes the timesheet. If the timesheet is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimesheet(string id)
        {
            try
            {
                // Fetch the timesheet entity by its ID
                var entity = await _timesheetService.GetTimesheetById(id);
                // If no timesheet is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No timesheet found with the specified id."));

                // Delete the timesheet using the timesheet service
                _timesheetService.DeleteTimesheet(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Weekly Timesheet

        #region DeleteWeeklyTimesheet
        [HttpDelete("delete-weekly-timesheets")]
        public async Task<IActionResult> DeleteWeeklyTimesheets([FromBody] List<string> lineIds)
        {
            try
            {
                if (lineIds != null && lineIds.Any())
                {
                    foreach (var id in lineIds)
                    {
                        var entity = await _timesheetLinesService.GetTimesheetLinesById(id);
                        if (entity != null)
                        {
                            // Delete the timesheet using the timesheetLine service
                            _timesheetLinesService.DeleteTimesheetLines(entity);
                        }
                    }
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete-weekly-timesheet-by-id/{timesheetId}")]
        public async Task<IActionResult> DeleteWeeklyTimesheetById(string timesheetId)
        {
            try
            {
                // Fetch the timesheet entity by its ID
                var entity = await _timesheetService.GetTimesheetById(timesheetId);
                // If no timesheet is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No timesheet found with the specified id."));

                // Delete all lines of that timesheet
                var timesheetLine = await _timesheetLinesService.GetTimesheetLineByTimesheetId(timesheetId);
                if (timesheetLine != null)
                {
                    _timesheetLinesService.DeleteTimesheetLines(timesheetLine);
                }

                // Delete the timesheet
                _timesheetService.DeleteTimesheet(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #endregion

        #region ExportTimesheet
        static string ParseHtmlDescription(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Decode HTML entities and combine all text nodes into a single string
            return string.Join("\n", doc.DocumentNode.SelectNodes("//li")?.Select(li => HttpUtility.HtmlDecode(li.InnerText.Trim()))
                                 ?? doc.DocumentNode.SelectNodes("//p")?.Select(p => HttpUtility.HtmlDecode(p.InnerText.Trim()))
                                 ?? new[] { HttpUtility.HtmlDecode(doc.DocumentNode.InnerText.Trim()) });
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportTimesheet(TimesheetModel model)
        {
            try
            {
                if (model.TimesheetDataModel != null && model.TimesheetDataModel.Count() > 0)
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var package = new ExcelPackage();
                    var worksheet = package.Workbook.Worksheets.Add("Timesheet Report");

                    worksheet.Cells[2, 2].Value = "Timesheet Report";
                    worksheet.Cells[2, 2].Style.Font.Bold = true;
                    worksheet.Cells[2, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid; // Solid fill
                    worksheet.Cells[2, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black); // Change the background color to Light Blue
                    worksheet.Cells[2, 2].Style.Font.Color.SetColor(System.Drawing.Color.White); // Change font color to Dark Blue
                    worksheet.Cells[2, 2, 2, 4].Merge = true;

                    // Define Variables
                    int startColIndex = 2;
                    int startRowIndex = 4;
                    // Adding Headers
                    int colIndex = startColIndex;
                    worksheet.Cells[startRowIndex, colIndex++].Value = "Date";
                    foreach (var column in model.Columns)
                    {
                        if (column.CheckedStatus)
                        {
                            worksheet.Cells[startRowIndex, colIndex++].Value = column.Label;
                        }
                    }

                    worksheet.Cells[startRowIndex, colIndex++].Value = "Billable Hours";
                    worksheet.Cells[startRowIndex, colIndex++].Value = "Estimate Amount";
                    // Format Headers
                    using (var headerCells = worksheet.Cells[startRowIndex, startColIndex, startRowIndex, colIndex - 1])
                    {
                        headerCells.Style.Font.Bold = true;
                        headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        headerCells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    // Adding Data
                    int dataRowIndex = startRowIndex + 1; // Start data rows after headers
                    foreach (var timesheet in model.TimesheetDataModel)
                    {
                        foreach (var line in timesheet.TimesheetLines)
                        {
                            int dataColIndex = startColIndex; // Reset column index for each row

                            // Date Column
                            if (timesheet.TimesheetDate != null)
                            {
                                worksheet.Cells[dataRowIndex, dataColIndex].Value = timesheet.TimesheetDate.Date.ToString("MM/dd/yyyy");
                                worksheet.Cells[dataRowIndex, dataColIndex].Style.Numberformat.Format = "mm/dd/yyyy"; // Date formatting
                                worksheet.Cells[dataRowIndex, dataColIndex].Style.WrapText = true;
                            }
                            else
                            {
                                worksheet.Cells[dataRowIndex, dataColIndex].Value = "N/A"; // Handle null date
                            }

                            dataColIndex++;

                            // Dynamic Columns
                            foreach (var column in model.Columns)
                            {
                                if (column.CheckedStatus)
                                {
                                    switch (column.Type)
                                    {
                                        case "P":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = line.Project?.Name ?? "N/A";
                                            break;
                                        case "PM":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = line.ProjectModule?.Name ?? "N/A";
                                            break;
                                        case "PT":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = line.Task?.Name ?? "N/A";
                                            break;
                                        case "PA":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = line.ProjectActivity?.Name ?? "N/A";
                                            break;
                                        case "D":
                                            var Cell = worksheet.Cells[dataRowIndex, dataColIndex++];
                                            //Cell.Value = ParseHtmlDescription(line.Description);
                                            if (!string.IsNullOrWhiteSpace(line.Description))
                                            {
                                                Cell.Value = ParseHtmlDescription(line.Description);
                                            }
                                            else
                                            {
                                                Cell.Value = "N/A";
                                            }

                                            Cell.Style.WrapText = true;
                                            break;
                                        case "U":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = timesheet.User?.Person?.FullName ?? "N/A";
                                            break;
                                        case "H":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = line.Hours;
                                            break;
                                        default:
                                            dataColIndex++; // Skip unknown types
                                            break;
                                    }
                                }
                            }
                            // Static Columns (Billable Hours, Estimate Amount)
                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = ""; // Placeholder for Billable Hours
                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = ""; // Placeholder for Estimate Amount

                            dataRowIndex++; // Move to the next row
                        }
                    }
                    // Define the Range for the Table
                    var totalColumns = startColIndex + model.Columns.Count(c => c.CheckedStatus) + 2; // Dynamic total
                    var dataRange = worksheet.Cells[startRowIndex, startColIndex, dataRowIndex, totalColumns];

                    // Add the Table
                    var table = worksheet.Tables.Add(dataRange, "TimesheetTable");
                    table.TableStyle = OfficeOpenXml.Table.TableStyles.None;

                    // Apply Borders to All Cells
                    using (var allCells = worksheet.Cells[startRowIndex, startColIndex, dataRowIndex, totalColumns])
                    {
                        allCells.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        allCells.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        allCells.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        allCells.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        allCells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    }

                    // Auto-Fit Columns
                    if (worksheet.Dimension != null)
                    {
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    }

                    // Return the File
                    var stream = new MemoryStream(package.GetAsByteArray());
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DataExport.xlsx");

                    //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    //using var package = new ExcelPackage();
                    //var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                    ////Define Variables
                    //int startColIndex = 1;
                    //int startRowIndex = 1;

                    //// Adding headers
                    //worksheet.Cells[startColIndex, startRowIndex].Value = "Date";


                    //worksheet.Cells[startColIndex, startRowIndex++].Value = "Project Name";
                    //worksheet.Cells[startColIndex, startRowIndex++].Value = "Module Order";
                    //worksheet.Cells[startColIndex, startRowIndex++].Value = "Task";
                    //worksheet.Cells[startColIndex, startRowIndex++].Value = "Activity Details";
                    //worksheet.Cells[startColIndex, startRowIndex++].Value = "Created By";
                    //worksheet.Cells[startColIndex, startRowIndex++].Value = "Actual Hours";
                    //worksheet.Cells[startColIndex, startRowIndex++].Value = "Billable Hours";
                    //worksheet.Cells[startColIndex, startRowIndex++].Value = "Estimate Amount";

                    //// Adding data
                    //int index = -1; // Initialize index
                    //foreach (var timesheet in model.TimesheetDataModel)
                    //{
                    //    foreach (var line in timesheet.TimesheetLines)
                    //    {
                    //        index++;
                    //        int startDataColIndex = 1;
                    //        int startDataRowIndex = 2;
                    //        worksheet.Cells[index + 2, startDataColIndex].Value = timesheet.TimesheetDate.Date.ToString("MM/dd/yyyy");
                    //        worksheet.Cells[index + 2, startDataColIndex++].Value = line.Project.Name;
                    //        worksheet.Cells[index + 2, startDataColIndex++].Value = line.ProjectModule.Name;
                    //        worksheet.Cells[index + 2, startDataColIndex++].Value = line.Task.Name;
                    //        worksheet.Cells[index + 2, startDataColIndex++].Value = line.ProjectActivity.Name;
                    //        worksheet.Cells[index + 2, startDataColIndex++].Value = timesheet.User.Person.FullName;
                    //        worksheet.Cells[index + 2, startDataColIndex++].Value = line.Hours;
                    //        worksheet.Cells[index + 2, startDataColIndex++].Value = "";
                    //        worksheet.Cells[index + 2, startDataColIndex++].Value = "";
                    //    }
                    //}

                    //// Define the range for the table
                    //var dataRange = worksheet.Cells[1, 1, index + 2, 10];

                    //// Add the table without alternating row colors
                    //var table = worksheet.Tables.Add(dataRange, "TimesheetTable");
                    //table.TableStyle = OfficeOpenXml.Table.TableStyles.None; // No predefined table style


                    //// Optional: Apply a consistent style to the entire table
                    //using (var allCells = worksheet.Cells[1, 1, index + 2, 10])
                    //{
                    //    //allCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    //    allCells.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    //    allCells.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    //    allCells.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    //    allCells.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    //    //allCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White); // Apply white background
                    //}

                    //// Format headers
                    //using (var headerCells = worksheet.Cells[1, 1, 1, 10])
                    //{
                    //    headerCells.Style.Font.Bold = true; // Bold headers
                    //    headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    //    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // Light gray header background
                    //    headerCells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    //}
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("exportBillingTimesheet")]

        public async Task<IActionResult> ExportBillingTimesheet(TimesheetLinesModel model)
        {
            try
            {
                if (model.TimesheetDataModel != null && model.TimesheetDataModel.Count() > 0)
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var package = new ExcelPackage();
                    var worksheet = package.Workbook.Worksheets.Add("Timesheet Billing Report");

                    // Define Variables
                    int startColIndex = 1;
                    int startRowIndex = 2;
                    // Adding Headers
                    int colIndex = startColIndex;
                    worksheet.Cells[startRowIndex, colIndex++].Value = "Timesheet Date";
                    foreach (var column in model.Columns)
                    {
                        if (column.CheckedStatus && column.Type != "TD")
                        {
                            worksheet.Cells[startRowIndex, colIndex++].Value = column.Label;
                        }
                    }

                    // Format Headers
                    using (var headerCells = worksheet.Cells[startRowIndex, startColIndex, startRowIndex, colIndex - 1])
                    {
                        headerCells.Style.Font.Bold = true;
                        headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        headerCells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    // Adding Data
                    int dataRowIndex = startRowIndex + 1; // Start data rows after headers
                    foreach (var timesheet in model.TimesheetDataModel)
                    {
                        if (timesheet != null)
                        {
                            int dataColIndex = startColIndex; // Reset column index for each row

                            // Date Column
                            if (timesheet.Timesheet.TimesheetDate != null)
                            {
                                worksheet.Cells[dataRowIndex, dataColIndex].Value = timesheet.Timesheet.TimesheetDate;
                                worksheet.Cells[dataRowIndex, dataColIndex].Style.Numberformat.Format = "mm/dd/yyyy"; // Date formatting
                                worksheet.Cells[dataRowIndex, dataColIndex].Style.WrapText = true;
                            }
                            else
                            {
                                worksheet.Cells[dataRowIndex, dataColIndex].Value = "N/A"; // Handle null date
                            }

                            dataColIndex++;
                            // Dynamic Columns
                            foreach (var column in model.Columns)
                            {
                                if (column.CheckedStatus)
                                {
                                    switch (column.Type)
                                    {
                                        case "TD":
                                            continue;
                                        case "P":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = timesheet.Project.Name ?? "N/A";
                                            break;
                                        case "PM":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = timesheet.ProjectModule.Name ?? "N/A";
                                            break;
                                        case "PT":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = timesheet.Task.Name ?? "N/A";
                                            break;
                                        case "PA":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = timesheet.ProjectActivity.Name ?? "N/A";
                                            break;
                                        case "D":
                                            var Cell = worksheet.Cells[dataRowIndex, dataColIndex++];
                                            Cell.Value = ParseHtmlDescription(timesheet.Description);
                                            Cell.Style.WrapText = true;
                                            break;
                                        case "U":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = timesheet.Timesheet.User.Person.FullName ?? "N/A";
                                            break;
                                        case "H":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = timesheet.Hours;
                                            break;
                                        case "CH":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = timesheet.BillableHours;
                                            break;
                                        case "B":
                                            worksheet.Cells[dataRowIndex, dataColIndex++].Value = timesheet.BillableHours;
                                            break;
                                    }
                                }
                            }
                            dataRowIndex++; // Move to the next row
                        }
                    }

                    // Define the Range for the Table
                    var totalColumns = startColIndex + model.Columns.Count(c => c.CheckedStatus) - 1; // Dynamic total
                    var dataRange = worksheet.Cells[startRowIndex, startColIndex, dataRowIndex, totalColumns];

                    // Add the Table
                    var table = worksheet.Tables.Add(dataRange, "TimesheetTable");
                    table.TableStyle = OfficeOpenXml.Table.TableStyles.None;

                    // Apply Borders to All Cells
                    using (var allCells = worksheet.Cells[startRowIndex, startColIndex, dataRowIndex, totalColumns])
                    {
                        allCells.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        allCells.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        allCells.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        allCells.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        allCells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    }
                    // Auto-Fit Columns
                    if (worksheet.Dimension != null)
                    {
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    }
                    // Return the File
                    var stream = new MemoryStream(package.GetAsByteArray());
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DataExport.xlsx");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Task Timer To Timesheet
        [HttpPost("send-task-timer-to-timesheet")]
        public async Task<IActionResult> SendTaskTimerToTimesheet(SendTaskTimmerToTimesheetModel model)
        {
            if (string.IsNullOrEmpty(model.TaskId) || string.IsNullOrEmpty(model.ActivityId) || model.Hours <= 0)
                return BadRequest("Something Went Wrong");

            var ActivityData = await _projectActivityService.GetById(model.ActivityId);
            if (ActivityData == null)
                return BadRequest("No Task Activity Found");

            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                var DateOnly = Convert.ToDateTime(GetDateTime.ToShortDateString() + " 00:00:00");
                string TodaysTimesheetId = "";

                var TodaysTimesheetData = await _timesheetService.GetTimesheetByDate(SiteId, LoggedUserId, DateOnly);
                if (TodaysTimesheetData != null)
                    TodaysTimesheetId = TodaysTimesheetData.Id;
                else
                {
                    var NewTimesheet = new Timesheet()
                    {
                        SiteId = SiteId,
                        EmployeeId = model.EmployeeId,
                        TimesheetDate = GetDateTime,
                        CreatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedById = LoggedUserId,
                        UpdatedOnUtc = GetDateTime
                    };
                    _timesheetService.InsertTimesheet(NewTimesheet);
                    TodaysTimesheetId = NewTimesheet.Id;
                }

                var NewTimesheetLine = new TimesheetLines()
                {
                    TimesheetId = TodaysTimesheetId,
                    ProjectId = ActivityData.ProjectId,
                    ProjectModuleId = ActivityData.ProjectModuleId,
                    ProjectTaskId = ActivityData.TaskId,
                    ProjectActivityId = ActivityData.Id,
                    Description = model.Description,
                    Hours = model.Hours,
                    CreatedById = LoggedUserId,
                    CreatedOnUtc = GetDateTime,
                    UpdatedById = LoggedUserId,
                    UpdatedOnUtc = GetDateTime
                };
                _timesheetLinesService.InsertTimesheetLines(NewTimesheetLine);

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex.Message + ":- " + Ex.InnerException);
            }
        }
        #endregion

        #region Timesheet Billable Hrs
        //created for update job status from list page
        [HttpPut("{id}/{billableHrs}")]
        public async Task<IActionResult> UpdateBillableHrs(string id, decimal billableHrs)
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
                    var entity = await _timesheetLinesService.GetTimesheetLinesById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No timesheet found with the specified id."));

                    entity.BillableHours = billableHrs;
                    entity.BillableCreatedById = LoggedUserId;
                    entity.BillableCreatedOnUtc = GetDateTime;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _timesheetLinesService.UpdateTimesheetLines(entity);

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

        #region GetTimesheetLinesDetailsByMeetingUId
        // Title: GetTimesheetById
        // Description: This endpoint retrieves the details of a specific timesheet based on its unique identifier (ID). 
        [HttpGet("get-timesheet-line-by-meetingUId/{meetingUId}")]
        public async Task<IActionResult> GetTimesheetLinesDetailsByMeetingUId(string meetingUId)
        {
            try
            {
                // Fetch the timesheet entity by its ID from the service
                var entity = await _timesheetLinesService.GetTimesheetLinesDetailsByMeetingUId(meetingUId);
                // If the timesheet entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return NoContent();

                // Map the timesheet entity to a TimesheetModel object
                var model = _mapper.Map<TimesheetLinesModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteTimesheetLine
        // Title: DeleteTimesheet
        // Description: This endpoint deletes a timesheet based on the provided timesheet ID. It first retrieves the timesheet entity by ID, checks if it exists, and if so, deletes the timesheet. If the timesheet is not found, it returns a BadRequest response with an error message.
        [HttpDelete("line/{id}")]
        public async Task<IActionResult> DeleteTimesheetLine(string id)
        {
            try
            {
                // Fetch the timesheet entity by its ID
                var entity = await _timesheetLinesService.GetTimesheetLinesById(id);
                // If no timesheet is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No timesheet line found with the specified id."));

                // Delete the timesheet using the timesheet service
                _timesheetLinesService.DeleteTimesheetLines(entity);

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
