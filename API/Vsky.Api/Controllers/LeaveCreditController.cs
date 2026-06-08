using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.EmployeeLeaves;
using Vsky.Services.Employees;
using Vsky.Services.LeaveCredits;
using Vsky.Services.LeaveRule;
using Vsky.Services.Persons;
using Vsky.Services.Sites;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Api.Controllers
{
    [Route("leaveCredits")]
    public class LeaveCreditController : BaseController
    {

        #region Define Services    
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ILeaveCreditService _leaveCreditService;
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeLeaveService _employeeLeaveService;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations      
        public LeaveCreditController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            ILeaveCreditService leaveCreditService, 
            IEmployeeService employeeService, 
            IEmployeeLeaveService employeeLeaveService, 
            ISiteService siteService, 
            ICommonService commonService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _leaveCreditService = leaveCreditService;
            _employeeService = employeeService;
            _employeeLeaveService = employeeLeaveService;
            _siteService = siteService;
            _commonService = commonService;
        }
        #endregion

        #region GetAllLeaveCredits
        // Title: Get All LeaveCredits
        // Description: This endpoint fetches a list of LeaveCredit based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public async Task<IActionResult> GetAllLeaveCredits(LeaveCreditSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _leaveCreditService.GetAllLeaveCredits(SiteId, searchModel.SearchText, searchModel.EmployeeIds, searchModel.Years, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);         
                var leaveModelList = new List<LeaveCreditModel>();
                foreach (var leaveCredit in list)
                {
                    var employee = await _employeeService.GetEmployeeDetailsById(leaveCredit.EmployeeId);
                    var leavecredits = _leaveCreditService.GetLeaveCreditsByEmployeeId(leaveCredit.EmployeeId,leaveCredit.LeaveCreditsforYear);
                    var usedleaves = _employeeLeaveService.GetUsedLeaveByEmployeeId(leaveCredit.EmployeeId, leaveCredit.LeaveCreditsforYear);
                    // Get the active employee status
                    //var activeEmployeeStatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Employee Status", "Current");
                    //var activeStatus = employee.EmployeeStatuses.Where(s => !s.Deleted).OrderByDescending(s => s.StartDate).FirstOrDefault().EmployeeStatusId == activeEmployeeStatus;

                    var leaveCreditModel = new LeaveCreditModel
                    {
                        Id = leaveCredit.Id,
                        EmployeeId = leaveCredit.EmployeeId,
                        EmployeeActiveStatus = employee.Active,
                        EmployeeName = employee.Person.FirstName + " " + employee.Person.LastName,
                        CasualLeaves = leavecredits,
                        UsedLeaves = usedleaves,    // Initialize or calculate used leaves
                        RemainingLeaves = leavecredits - usedleaves,  // Initialize or calculate remaining leaves
                        UpdatedOnUtc = leaveCredit.UpdatedOnUtc
                    };

                    leaveModelList.Add(leaveCreditModel);
                }
               
                // Apply sorting to the leaveModelList
                leaveModelList = searchModel.Descending
                    ? leaveModelList.OrderByDescending(x => GetSortingValue(x, searchModel.SortBy)).ToList()
                    : leaveModelList.OrderBy(x => GetSortingValue(x, searchModel.SortBy)).ToList();

               
                // Helper method to get the sorting value based on the SortBy field
                object GetSortingValue(LeaveCreditModel model, string sortBy)
                {
                    return sortBy switch
                    {
                        "employee.person.firstName" => model.EmployeeName,
                        "casualLeaves" => model.CasualLeaves,
                        "usedLeaves" => model.UsedLeaves,
                        "remainingLeaves" => model.RemainingLeaves,
                        "updatedOnUtc" => model.UpdatedOnUtc,
                        _ => model.Id // Default sorting by Id
                    };
                }

                // Map the fetched list to a model suitable for the response
                var model = new LeaveCreditListModel
                {
                    Data = _mapper.Map<IList<LeaveCreditModel>>(leaveModelList),
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

        #region GetLeaveCreditDetailsById
        //Title: GetLeaveCreditDetailsById
        //Description: This endpoint retrieves the details of a specific leave credit based on its unique identifier(ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeaveCreditDetailsById(string id)
        {
            try
            {
                // Fetch the leave credit entity by its ID from the service
                var entity = await _leaveCreditService.GetLeaveCreditDetailsById(id);
                // If the leave credit entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No leave credit found with the specified id."));

                var employee = await _employeeService.GetEmployeeDetailsById(entity.EmployeeId);
                var leavecredits = _leaveCreditService.GetLeaveCreditsByEmployeeId(entity.EmployeeId,entity.LeaveCreditsforYear);
                var usedleaves = _employeeLeaveService.GetUsedLeaveByEmployeeId(entity.EmployeeId,entity.LeaveCreditsforYear);
               
                // Map the leave credit entity to a LeaveCreditModel object
                var model = _mapper.Map<LeaveCreditModel>(entity);
                model.EmployeeId = entity.EmployeeId;
                model.EmployeeName = employee.Person.FirstName + " " + employee.Person.LastName;
                model.CreditLeaves = leavecredits;
                model.UsedLeaves = usedleaves;    // Initialize or calculate used leaves
                model.RemainingLeaves = leavecredits - usedleaves;  // Initialize or calculate remaining leaves
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetLeaveCreditByEmployeeId
        //Title: GetLeaveCreditByEmployeeId
        //Description: This endpoint retrieves the details of a specific leave credit based on employeeId. 
        [HttpGet("details/{employeeId}/{year}")]
        public async Task<IActionResult> GetLeaveCreditByEmployeeId(string employeeId, int year)
        {
            try
            {
                // Fetch the leave credit entity by its ID from the service
                var leaveCredits = await _leaveCreditService.GetLeaveCreditByEmployeeId(employeeId, year);
                // Map the fetched list to a model suitable for the response
                var model = new LeaveCreditListModel
                {
                    Data = _mapper.Map<IList<LeaveCreditModel>>(leaveCredits),
                    //Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateCreditLeave
        // Title: CreateCreditLeave
        // Description: This endpoint handles the creation of a new CreditLeave. It maps the CreditLeave model to the CreditLeave entity, sets the creation details, and inserts the CreditLeave into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateCreditLeave(LeaveCreditModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var leaveRuleData = await _leaveCreditService.GetLeaveCreditById(model.LeaveCreditId);

                    // Map the employee model to the employee entity
                    var entity = _mapper.Map<LeaveCredit>(model);
                    int currentYear = DateTime.UtcNow.Year;
                    entity.LeaveCreditsforYear = currentYear;
                    entity.EmployeeId = model.EmployeeId;
                    entity.CasualLeaves = model.CasualLeaves != null ? model.CasualLeaves : 0;
                    entity.SickLeaves = model.SickLeaves != null ? model.SickLeaves : 0;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _leaveCreditService.InsertLeaveCredit(entity);
                    return NoContent();
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

        #region CreateEmployeeCreditLeaves
        // Title: CreateCreditLeave
        // Description: This endpoint handles the creation of a new CreditLeave. It maps the CreditLeave model to the CreditLeave entity, sets the creation details, and inserts the CreditLeave into the database. 
        [HttpPost("credits")]
        public async Task<IActionResult> CreateEmployeeCreditLeaves(LeaveCreditModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check already add leaveCredit or not
                    var existEmployeeLeaves = await _leaveCreditService.GetLeaveCreditByEmployeeIdByType(model.EmployeeId, model.LeaveCreditsforYear, model.LeaveTypeId);
                    if (existEmployeeLeaves != null && existEmployeeLeaves.Count > 0)
                        return BadRequest(new BadRequestError("Already added leave credits for this employee"));

                    // Map the employee model to the employee entity
                    var entity = _mapper.Map<LeaveCredit>(model);
                    entity.LeaveCreditsforYear = model.LeaveCreditsforYear;
                    entity.EmployeeId = model.EmployeeId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _leaveCreditService.InsertLeaveCredit(entity);
                    
                    return NoContent();
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
    }
}