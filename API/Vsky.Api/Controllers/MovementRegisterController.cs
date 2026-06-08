using System.Threading.Tasks;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.MovementRegisters;
using Vsky.Services.Sites;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.TimeInTimeOuts;
using Vsky.Api.ApiErrors;
using Vsky.Services.SitesModifiedLog;

namespace Vsky.Api.Controllers
{
    [Route("movementRegister")]
    public class MovementRegisterController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ISiteService _siteService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly ICommonService _commonService;
        private readonly IDropDownService _dropDownService;
        private readonly ITimeInTimeOutService _timeInTimeOutService;
        private readonly IMovementRegisterServices _movementRegisterService;
        private readonly IMovementRegisterDetailsService _movementRegisterDetailsService;
        #endregion

        #region Services Initializations
        public MovementRegisterController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ApplicationDbContext context,
            ISiteService siteService,
            ISitesModifiedLogsService sitesModifiedLogsService,
            ICommonService commonService,
            IDropDownService dropDownService,
            ITimeInTimeOutService timeInTimeOutService,
            IMovementRegisterServices movementRegisterServices,
            IMovementRegisterDetailsService movementRegisterDetailsService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _context = context;
            _siteService = siteService;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _commonService = commonService;
            _dropDownService = dropDownService;
            _timeInTimeOutService = timeInTimeOutService;
            _movementRegisterService = movementRegisterServices;
            _movementRegisterDetailsService = movementRegisterDetailsService;
        }
        #endregion

        #region GetAllMovementRegistersAsync
        [HttpPost("list")]
        public async Task<IActionResult> GetAllMovementRegistersAsync(MovementRegisterSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";

                var List = _movementRegisterService.GetAllMovementRegister(
                    SiteId,
                    createdBy,
                    searchModel.SearchText,
                    searchModel.EmployeeId,
                    searchModel.TypeId,
                    searchModel.FromDate,
                    searchModel.ToDate,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var Data = new MovementRegisterList
                {
                    MoveRegisterList = List,
                    Total = List.TotalCount
                };

                return Ok(Data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region GetAllMovementRegistersForDashboard
        [HttpPost("dashboardlist")]
        public async Task<IActionResult> GetAllMovementRegistersForDashboard(MovementRegisterSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var createdBy = "";

                var List =  _movementRegisterService.GetAllMovementRegistersForDashboard(
                    SiteId,
                    SiteData.TimeZone,
                    createdBy,
                    searchModel.SearchText,
                    searchModel.EmployeeId,
                    searchModel.FromDate,
                    searchModel.ToDate,
                    searchModel.IsViewMore,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var Data = new MovementRegisterList
                {
                    MoveRegisterList = List,
                    Total = List.TotalCount
                };

                return Ok(Data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region GetMovementRegisterDateRange
        // Title: GetMovementRegisterDateRange
        // Description: This endpoint retrieves the start and end dates for movement registers.
        [HttpGet("daterange")]
        public async Task<IActionResult> GetMovementRegisterDateRange()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var (startDate, endDate) = await _movementRegisterService.GetMovementRegisterDateRange(SiteId);
                return Ok(new
                {
                    startDate,
                    endDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetMovementRegisterDetailsById
        // Title: GetMovementRegisterDetailsById
        // Description: This endpoint retrieves the details of a specific MovementRegister based on its unique identifier (ID). 
        [HttpGet("details/{id}/{detailId}")]
        public async Task<IActionResult> GetMovementRegisterDetailsById(string id, string detailId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var entity = await _movementRegisterService.GetMovementRegisterDetailsById(id, detailId);
                if (entity == null)
                    return BadRequest(new BadRequestError("No movement register found with the specified id."));

                var model = _mapper.Map<MovementRegister>(entity);
                
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Create Movement register
        [HttpPost]
        public async Task<IActionResult> CreateMovementRegister(SaveMovementRegister model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                if (model.DateStr != "" && model.DateStr != null)
                    model.Date = DateTime.Parse(model.DateStr);

                // get movementRegisterDate                
                var movementRegister = await _movementRegisterService.GetMovementRegisterByDate(SiteId, model.Date);
                if (movementRegister == null)
                {
                    movementRegister = new MovementRegister
                    {
                        SiteId = SiteId,
                        Date = model.Date,
                        CreatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime
                    };

                    _movementRegisterService.InsertMovementRegister(movementRegister);
                }
                var typeId = await _dropDownService.GetDropDownByTypeNameAndName(SiteId, "Type", model.Type);

                if (model.Type == "Work From Home")
                {
                    var existWorkFromHome = await _movementRegisterDetailsService
                        .GetMovementRegisterDetailByTypeId(SiteId, EmployeeId, model.Date, typeId);

                    if (existWorkFromHome != null)
                        return BadRequest("Work From Home for this date is already added.");
                }


                var movementRegisterDetails = new MovementRegisterDetails
                  {
                       MomentRegisterId = movementRegister.Id,
                       TypeId = typeId,
                       EmployeeId = EmployeeId,
                       BreakTimeId = model.BreakTimeId,
                       WFHDurationId = model.WFHDurationId,
                       ApproverById = model.ApproverById,
                       Message = model.Message,
                       TimeInMinutes = model.TimeInMinutes,
                       NotifyToStakeholders = model.NotifyToStakeholders,
                       CreatedById = LoggedUserId,
                       CreatedOnUtc = GetDateTime,
                       UpdatedById = LoggedUserId,
                       UpdatedOnUtc = GetDateTime
                  };
                  _movementRegisterDetailsService.InsertMovementRegisterDetails(movementRegisterDetails);

                return Ok(movementRegister.Id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region UpdateMovementRegister
        // Title: UpdateMovementRegister
        // Description: This endpoint updates an existing movement register by its ID. 
        [HttpPut("{detailId}")]
        public async Task<IActionResult> UpdateMovementRegister(string detailId, SaveMovementRegister model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
                    var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                    var entity = await _movementRegisterDetailsService.GetMovementRegisterDetailsById(detailId);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No movement register detail found with the specified id."));

                    bool IsMessageChanged = model.Message != entity.Message;
                    bool IsTimeInMinutesChanged = model.WFHDurationId == null && model.TimeInMinutes != entity.TimeInMinutes;
                    bool IsWFHDurationChanged = model.WFHDurationId != null && model.WFHDurationId != entity.WFHDurationId;


                    if (IsMessageChanged)
                    {
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "MovementRegisterDetails", entity.MomentRegisterId, model.Date?.ToString("MM/dd/yyyy"), entity.Id, model.Type, "Message", entity.Message, LoggedUserId, entity.UpdatedOnUtc);
                    }

                    if (IsTimeInMinutesChanged)
                    {
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "MovementRegisterDetails", entity.MomentRegisterId, model.Date?.ToString("MM/dd/yyyy"), entity.Id, model.Type, "Time In Minutes", entity.TimeInMinutes.ToString(), LoggedUserId, entity.UpdatedOnUtc);
                    }

                    if (IsWFHDurationChanged)
                    {
                        var WFHDuration = await _dropDownService.GetDropDownById(model.WFHDurationId);
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "MovementRegisterDetails", entity.MomentRegisterId, model.Date?.ToString("MM/dd/yyyy"), entity.Id, model.Type, "WFH Duration", WFHDuration.DropDownText, LoggedUserId, entity.UpdatedOnUtc);
                    }

                    entity.WFHDurationId = model.WFHDurationId;
                    entity.BreakTimeId = model.BreakTimeId;
                    entity.Message = model.Message;
                    entity.TimeInMinutes = model.TimeInMinutes;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    _movementRegisterDetailsService.UpdateMovementRegisterDetails(entity);
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
       
    }
}
