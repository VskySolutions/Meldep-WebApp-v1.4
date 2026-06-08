using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.Employees;
using Vsky.Services.LeaveCredits;
using Vsky.Services.LeaveRule;
using Vsky.Services.LeaveRuleLine;
using Vsky.Services.Persons;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("leaveRules")]
    public class LeaveRulesController : BaseController
    {

        #region Define Services     
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ILeaveRulesService _leaveRulesService;
        private readonly ILeaveRuleLinesService _leaveRuleLinesService;
        private readonly IEmployeeService _employeeService;
        private readonly ILeaveCreditService _leaveCreditService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        #endregion

        #region Services Initializations      
        public LeaveRulesController(
            GlobalVariable globalVariable,
            IMapper mapper, 
            ILeaveRulesService leaveRulesService, 
            ILeaveRuleLinesService leaveRuleLinesService, 
            IEmployeeService employeeService, 
            ILeaveCreditService leaveCreditService, 
            ICommonService commonService,
            ISiteService siteService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _leaveRulesService = leaveRulesService;
            _leaveRuleLinesService = leaveRuleLinesService;
            _employeeService = employeeService;
            _leaveCreditService = leaveCreditService;
            _commonService = commonService;
            _siteService = siteService;
        }
        #endregion

        #region GetAllLeaveRules
        // Title: Get All LeaveRules
        // Description: This endpoint fetches a list of LeaveRules based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllLeaveRules(LeaveRulesSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of LeaveRules on search criteria (name, sorting, pagination)
                var list = _leaveRulesService.GetAllLeaveRules(SiteId, searchModel.SearchText, searchModel.Years, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new LeaveRulesListModel
                {
                    Data = _mapper.Map<IList<LeaveRulesModel>>(list),
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

        #region GetLeaveRulesDetailsById
        //Title: GetLeaveRulesDetailsById
        //Description: This endpoint retrieves the details of a specific LeaveRules based on its unique identifier(ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeaveRulesDetailsById(string id)
        {
            try
            {
                // Fetch the Leave Rules entity by its ID from the service
                var entity = await _leaveRulesService.GetLeaveRulesDetailsById(id);
                // If the Leave Rules entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Leave Rules found with the specified id."));

                // Map the Leave Rules entity to a LeaveRulesModel object
                var model = _mapper.Map<LeaveRulesModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateLeaveRules
        // Title: CreateLeaveRules
        // Description: This endpoint handles the creation of a new LeaveRules. It maps the LeaveRules model to the LeaveRules entity, sets the creation details, and inserts the LeaveRules into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateLeaveRules(LeaveRulesModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var leaverule = await _leaveRulesService.GetLeaveRulesByYear(SiteId, model.Year);
                    var LeaveRuleId = "";
                    if (leaverule != null)
                    {
                        // Set the created by and created on properties
                        leaverule.UpdatedById = LoggedUserId;
                        leaverule.UpdatedOnUtc = GetDateTime;
                        _leaveRulesService.UpdateLeaveRules(leaverule);

                        LeaveRuleId = leaverule.Id;
                    }
                    else
                    {
                        // Map the employee model to the employee entity
                        var entity = _mapper.Map<LeaveRules>(model);
                        entity.SiteId = SiteId;
                        entity.CreatedById = LoggedUserId;
                        entity.CreatedOnUtc = GetDateTime;
                        entity.UpdatedById = LoggedUserId;
                        entity.UpdatedOnUtc = GetDateTime;
                        _leaveRulesService.InsertLeaveRules(entity);

                        LeaveRuleId = entity.Id;
                    }                    

                    if (model.LeaveRuleLines != null && model.LeaveRuleLines.Count() > 0)
                    {
                        // Loop through each Project
                        foreach (var item in model.LeaveRuleLines)
                        {
                            // Fetch the LeaveRuleLine entity by its ID
                            var type = await _leaveRuleLinesService.GetLeaveRuleLineById(item.Id);
                            if (item.Flag != "Delete")
                            {
                                var leaveRuleLine = await _leaveRuleLinesService.GetLeaveRuleLinesByLeaveRuleId(LeaveRuleId, item.EmploymentTypeId);
                                // If no employee type is found with the given ID, continue
                                if (type != null)
                                    continue;

                                if (leaveRuleLine.Count() > 0)
                                    return BadRequest(new BadRequestError("You can not create a record for the same year"));
                                //continue;

                                var data = _mapper.Map<LeaveRuleLines>(item);
                                data.LeaveRuleId = LeaveRuleId;
                                data.EmploymentTypeId = item.EmploymentTypeId;
                                data.CasualLeaves = item.CasualLeaves;
                                data.SickLeaves = item.SickLeaves;
                                data.CreatedById = LoggedUserId;
                                data.CreatedOnUtc = GetDateTime;
                                data.UpdatedById = LoggedUserId;
                                data.UpdatedOnUtc = GetDateTime;
                                _leaveRuleLinesService.InsertLeaveRuleLine(data);
                            }
                        }
                    }
                    return Ok(leaverule);
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

        #region UpdateLeaveRule
        // Title: UpdateLeaveRule
        // Description: This endpoint updates an existing leaverule by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLeaveRule(string id, LeaveRulesModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the leave rule entity by its ID
                    var entity = await _leaveRulesService.GetLeaveRulesById(id);
                    var LeaveRuleId = "";
                    // If no leave rule is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No leave rule found with the specified id."));

                    // Set the created by and created on properties
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _leaveRulesService.UpdateLeaveRules(entity);

                    LeaveRuleId = entity.Id;
                    //save employee types
                    if (model.LeaveRuleLines.Count() > 0)
                    {
                        //var typeEntities = new List<EmployeeType>();
                        var addList = new List<LeaveRuleLines>();
                        var deleteList = new List<LeaveRuleLines>();
                        var updateList = new List<LeaveRuleLines>();

                        foreach (var item in model.LeaveRuleLines)
                        {
                            // Fetch the employee type entity by its ID
                            var type = await _leaveRuleLinesService.GetLeaveRuleLineById(item.Id);
                            if (item.Flag == "Edit")
                            {
                                // If no employee type is found with the given ID, continue
                                if (type == null)
                                    continue;

                                type.LeaveRuleId = LeaveRuleId;
                                type.EmploymentTypeId = item.EmploymentTypeId;
                                type.CasualLeaves = item.CasualLeaves;
                                type.SickLeaves = item.SickLeaves;
                                // Set the updated by and updated on properties
                                type.UpdatedById = LoggedUserId;
                                type.UpdatedOnUtc = GetDateTime;
                                updateList.Add(type);
                            }
                            else if (item.Flag == "New")
                            {
                                var leaveRuleLine = await _leaveRuleLinesService.GetLeaveRuleLinesByLeaveRuleId(LeaveRuleId, item.EmploymentTypeId);
                                // If no employee type is found with the given ID, continue
                                if (type != null)
                                    continue;

                                if (leaveRuleLine.Count() > 0)
                                    return BadRequest(new BadRequestError("You can not create a record for the same year"));
                                //continue;

                                var data = _mapper.Map<LeaveRuleLines>(item);
                                data.LeaveRuleId = LeaveRuleId;
                                data.EmploymentTypeId = item.EmploymentTypeId;
                                data.CasualLeaves = item.CasualLeaves;
                                data.SickLeaves = item.SickLeaves;
                                data.CreatedById = LoggedUserId;
                                data.CreatedOnUtc = GetDateTime;
                                data.UpdatedById = LoggedUserId;
                                data.UpdatedOnUtc = GetDateTime;
                                addList.Add(data);
                            }
                            else if (item.Flag == "Delete")
                            {
                                // If no employee type is found with the given ID, continue
                                if (type == null)
                                    continue;

                                deleteList.Add(type);
                            }
                        }

                        if (addList.Count > 0)
                            _leaveRuleLinesService.InsertLeaveRuleLinesList(addList);

                        if (updateList.Count > 0)
                            _leaveRuleLinesService.UpdateLeaveRuleLinesList(updateList);

                        if (deleteList.Count > 0)
                            _leaveRuleLinesService.DeleteLeaveRuleLinesList(deleteList);
                    }
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

        #region GenerateLeaveRule
        // Title: Generate Leave Rule
        // Description: This endpoint handles to Generate Leave Rules for employees. 
        [HttpPost("generate/{leaveRuleId}")]
        public async Task<IActionResult> GenerateLeaveRule(string LeaveRuleId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var SiteData = await _siteService.GetById(SiteId);
                var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                if (LeaveRuleId != null)
                {
                    var LeaveRule = await _leaveRulesService.GetLeaveRulesById(LeaveRuleId);
                    if (LeaveRule != null)
                    {
                        LeaveRule.IsGenerated = true;
                        _leaveRulesService.UpdateLeaveRules(LeaveRule);

                        var leavetype = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Leave Type", "Paid");
                        var lines = await _leaveRuleLinesService.GetLeaveRuleLinesByLeaveRuleId(LeaveRuleId);
                        if (lines.Count() > 0)
                        {
                            foreach (var item in lines)
                            {
                                if (item.CasualLeaves > 0 || item.SickLeaves > 0)
                                {
                                    var activeEmployeeStatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Employee Status", "Current");
                                    var exEmployeeStatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Employee Status", "Ex-Employee");
                                    //Get All Active Employees
                                    var Employees = _employeeService.GetAllEmployeesByEmployementType(SiteId, item.EmploymentTypeId, activeEmployeeStatus, exEmployeeStatus);
                                    var CreditReason = "Yearly";
                                    foreach (var employee in Employees)
                                    {
                                        //var existingCredit = await _leaveCreditService.GetLeaveCreditsByEmployeeData(employee.Id, CreditReason, LeaveRule.Year);
                                        var existingCredit = await _leaveCreditService.GetLeaveCreditsByEmployeeData(employee.Id, LeaveRule.Year);
                                        if (existingCredit != null)
                                        {
                                            existingCredit.CasualLeaves = item.CasualLeaves;
                                            existingCredit.SickLeaves = item.SickLeaves;
                                            existingCredit.CreditReason = CreditReason;
                                            existingCredit.UpdatedById = LoggedUserId;
                                            existingCredit.UpdatedOnUtc = GetDateTime;
                                            _leaveCreditService.UpdateLeaveCredit(existingCredit);
                                        }
                                        else
                                        {
                                            // Map the employee model to the employee entity
                                            var leaveCredit = new LeaveCredit();
                                            leaveCredit.EmployeeId = employee.Id;
                                            leaveCredit.LeaveTypeId = leavetype;
                                            leaveCredit.CasualLeaves = (decimal)item.CasualLeaves;
                                            leaveCredit.SickLeaves = (decimal)item.SickLeaves;
                                            leaveCredit.CreditReason = CreditReason;
                                            leaveCredit.LeaveCreditsforYear = LeaveRule.Year;
                                            leaveCredit.IsDefault = true;
                                            leaveCredit.CreatedById = LoggedUserId;
                                            leaveCredit.CreatedOnUtc = GetDateTime;
                                            leaveCredit.UpdatedById = LoggedUserId;
                                            leaveCredit.UpdatedOnUtc = GetDateTime;
                                            _leaveCreditService.InsertLeaveCredit(leaveCredit);
                                        }                                        
                                    }
                                }
                            }
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteLeaveRule
        // Title: DeleteLeaveRuleById
        // Description: This endpoint deletes a LeaveRules based on the provided LeaveRules ID. It first retrieves the LeaveRules entity by ID, checks if it exists, and if so, deletes the LeaveRules. If the LeaveRules is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeaveRule(string id)
        {
            try
            {
                // Fetch the LeaveRules entity by its ID
                var entity = await _leaveRulesService.GetLeaveRulesById(id);
                // If no LeaveRules is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Leave Rule found with the specified id."));

                // Delete the LeaveRules using the LeaveRules service
                _leaveRulesService.DeleteLeaveRules(entity);

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
