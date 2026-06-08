using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.EmployeeClientLocations;
using Vsky.Services.EmployeeDepartments;
using Vsky.Services.EmployeeDesignations;
using Vsky.Services.EmployeeOrgLocations;
using Vsky.Services.Employees;
using Vsky.Services.EmployeeStatuses;
using Vsky.Services.EmployeeTypes;
using Vsky.Services.Messages;
using Vsky.Services.Persons;
using Vsky.Services.ProjectTasks;
using Vsky.Services.Sites;
using Vsky.Services.Users;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Vsky.Api.Models.EmployeeModel;

namespace Vsky.Api.Controllers
{
    [Route("employees")]
    public class EmployeesController : BaseController
    {

        #region Define Services      
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPersonService _personService;
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeTypeService _employeeTypeService;
        private readonly IEmployeeStatusService _employeeStatusService;
        private readonly IEmployeeDepartmentService _employeeDepartmentService;
        private readonly IEmployeeDesignationService _employeeDesignationService;
        private readonly IEmployeeOrgLocationService _employeeOrgLocationService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IDropDownService _dropDownService;
        #endregion

        #region Services Initializations      
        public EmployeesController(
            IMapper mapper,
            GlobalVariable globalVariable,
            UserManager<ApplicationUser> userManager,
            IEmployeeService employeeService,
            IEmployeeStatusService employeeStatusService,
            IEmployeeDepartmentService employeeDepartmentService,
            IEmployeeDesignationService employeeDesignationService,
            IEmployeeTypeService employeeTypeService,
            IPersonService personService,
            ICommonService commonService,
            IEmployeeOrgLocationService employeeOrgLocationService,
            ISiteService siteService,
            IDropDownService dropDownService
           )
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _userManager = userManager;
            _personService = personService;
            _employeeService = employeeService;
            _employeeStatusService = employeeStatusService;
            _employeeDepartmentService = employeeDepartmentService;
            _employeeDesignationService = employeeDesignationService;
            _employeeTypeService = employeeTypeService;
            _employeeOrgLocationService = employeeOrgLocationService;
            _commonService = commonService;
            _siteService = siteService;
            _dropDownService = dropDownService;

        }
        #endregion

        #region GetAllEmployees
        // Title: Get All Employee
        // Description: This endpoint fetches a list of employees based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllEmployee(EmployeeSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;


                // Fetch a list of employees on search criteria (name, sorting, pagination)
                var list = _employeeService.GetAllEmployees(
                    SiteId,
                    searchModel.SearchText,
                    searchModel.EmployeeCode,
                    searchModel.EmployeeIds,
                    searchModel.PrimaryEmailAddress,
                    searchModel.EmployeeTypeIds,
                    searchModel.EmployeeDepartmentIds,
                    searchModel.EmployeeDesignationIds,
                    searchModel.OrgLocationIds,
                    searchModel.EmployeeStatus,
                    searchModel.AllStatusId,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                // Map the fetched list to a model suitable for the response
                var model = new EmployeeListModel
                {
                    Data = _mapper.Map<IList<EmployeeModel>>(list),
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

        #region GetAllEmployeeListForDropdown
        // Title: GetAllEmployeeListForDropdown
        // Description: This endpoint retrieves the list of a employees. 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllEmployeeListForDropdown(string siteId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
            var list = await _employeeService.GetAllEmployeeListForDropdown(SiteId);
            var model = _mapper.Map<List<EmployeeModel>>(list);
            return Ok(model);
        }
        #endregion

        #region getDefaultLeaveApproverNameForDropdown
        // Title: getDefaultLeaveApproverNameForDropdown
        // Description: This endpoint retrieves the employee list for the Leave Approver dropdown 
        [HttpGet("leave-approver/list")]
        public async Task<IActionResult> getDefaultLeaveApproverNameForDropdown(string siteId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
            //var employeeId = _commonService.GetEmployeeIdByUserId(SiteId, LoggedUserId);
            var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);
            var list = await _employeeService.GetAllActiveEmployeeListForDropdown(SiteId);
            var model = _mapper.Map<List<EmployeeModel>>(list);
            var leaveapprover = await _employeeDesignationService.GetEmployeeDesignationByEmployeeId(employeeId);

            return Ok(new
            {
                employees = list,
                leaveApproverId = leaveapprover?.LeaveApproverId
            });
        }
        #endregion

        #region GetAllActivityOwnerListForDropdown
        // Title: GetAllActivityOwnerListForDropdown
        // Description: This endpoint retrieves the list of a activity owners. 
        [HttpGet("ownerdropdown/list")]
        public async Task<IActionResult> GetAllActivityOwnerListForDropdown(string TargetMonthStr = null)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            //var activeEmployeeStatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Employee Status", "Active");
            //var exEmployeeStatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Employee Status", "Ex-Employee");
            //var list = await _employeeService.GetAllActivityOwnerListForDropdown(SiteId, activeEmployeeStatus, exEmployeeStatus);

            //Set target month
            DateTime? TargetMonth = null;
            if (!string.IsNullOrWhiteSpace(TargetMonthStr) && TargetMonthStr != "undefined")
            {
                var targetMonth = TargetMonthStr;
                if (targetMonth != null)
                {
                    var targetMonthArr = targetMonth.Split('-');
                    int month = DateTimeFormatInfo.CurrentInfo.MonthNames.ToList().IndexOf(targetMonthArr[0]) + 1;
                    if (month > 0)
                    {
                        string monthStr = month >= 10 ? month.ToString() : "0" + month.ToString();
                        string targetMonthDate = "01/" + monthStr + "/" + targetMonthArr[1];
                        TargetMonth = DateTime.ParseExact(targetMonthDate, "dd/MM/yyyy", null);
                    }
                }
            }

            var list = await _employeeService.GetAllActiveEmployeeListForDropdown(SiteId, TargetMonth);
            var model = _mapper.Map<List<EmployeeModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetAllActiveEmployeeListForDropdown
        // Title: GetAllActiveEmployeeListForDropdown
        // Description: This endpoint retrieves the active employee of a specific employee based on its unique identifier (ID). 
        [HttpGet("activedropdown/list")]
        public async Task<IActionResult> GetAllActiveEmployeeListForDropdown(string siteId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
            var list = await _employeeService.GetAllActiveEmployeeListForDropdown(SiteId);
            var model = _mapper.Map<List<EmployeeModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetEmployeesByStatus
        [HttpGet("employees/byStatus/list")]
        public async Task<IActionResult> GetEmployeesByStatus(string statusId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _employeeService.GetEmployeesByStatus(SiteId, statusId);
            var model = _mapper.Map<List<EmployeeModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetEmployeeById
        //Title: GetEmployeeById
        //Description: This endpoint retrieves the details of a specific employee based on its unique identifier(ID). 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(string id)
        {
            try
            {
                // Fetch the employee entity by its ID from the service
                var entity = await _employeeService.GetEmployeeDetailsById(id);
                // If the employee entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No employee found with the specified id."));

                // Map the employee entity to a EmployeeModel object
                var model = _mapper.Map<EmployeeModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateEmployee
        // Title: CreateEmployee
        // Description: This endpoint handles the creation of a new employee. It maps the employee model to the employee entity, sets the creation details, and inserts the employee into the database. 
        [HttpPost]
        //public IActionResult CreateEmployee(EmployeeModel model)
        public async Task<IActionResult> CreateEmployee(EmployeeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = string.IsNullOrWhiteSpace(model.SiteId) ? _globalVariable.SiteId : model.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //var exists = await _employeeService.GetEmployeeByPersonId(model.PersonId);
                    var exists = await _employeeService.GetEmployeeByPersonIdBySiteId(model.PersonId, SiteId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("The person already exists as an employee"));

                    var existingCode = await _employeeService.GetEmployeeByEmployeeCode(SiteId, model.EmployeeCode);
                    if (existingCode != null)
                        return BadRequest(new BadRequestError("Employee code already exists. Please enter a unique employee code."));

                    // Map the employee model to the employee entity
                    var entity = _mapper.Map<Employee>(model);
                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;

                    if (model.SameASPermanentAddress == true)
                    {
                        var personDate = await _personService.GetPersonById(model.PersonId);
                        entity.SameASPermanentAddress = true;
                        entity.AddressId = personDate.AddressId;
                    }
                    else
                    {
                        string AddressId = _commonService.AddUpdateAddress(model.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, LoggedUserId);
                        entity.SameASPermanentAddress = false;
                        entity.AddressId = AddressId;
                    }

                    _employeeService.InsertEmployee(entity);

                    // Update User if exists
                    var ExistingEmployeeUserId = _commonService.GetLoggeduserIdByEmployeeId(SiteId, entity.Id);
                    if (ExistingEmployeeUserId != null)
                    {
                        var user = await _userManager.FindByIdAsync(ExistingEmployeeUserId);
                        if (user != null)
                        {
                            user.Active = entity.Active;
                            user.UpdatedById = LoggedUserId;
                            user.UpdatedOnUtc = GetDateTime;

                            await _userManager.SetLockoutEndDateAsync(user, entity.Active ? null : DateTimeOffset.UtcNow.AddYears(100));
                            await _userManager.UpdateAsync(user);
                        }
                    }

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

        #region UpdateEmployee
        // Title: UpdateEmployee
        // Description: This endpoint updates an existing employee by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(string id, EmployeeModel model)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = string.IsNullOrWhiteSpace(model.SiteId) ? _globalVariable.SiteId : model.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the employee entity by its ID
                    var entity = await _employeeService.GetById(id);
                    // If no employee is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No employee found with the specified id."));

                    var existingCode = await _employeeService.GetEmployeeByEmployeeCode(SiteId, model.EmployeeCode, id);
                    if (existingCode != null)
                        return BadRequest(new BadRequestError("Employee code already exists. Please enter a unique employee code."));

                    if (model.Tab == "1_tab" || model.Tab == "3_tab")
                    {
                        var isActivating = model.Active;
                        // Set the user who updated the employee and the current UTC time for tracking purposes
                        entity.EmployeeCode = model.EmployeeCode;
                        entity.OfficialEmail = model.OfficialEmail;
                        entity.EmergencyContactName = model.EmergencyContactName;
                        entity.EmergencyPhoneNo = model.EmergencyPhoneNo;
                        entity.SameASPermanentAddress = model.SameASPermanentAddress;
                        entity.AadhaarCardNo = model.AadhaarCardNo;
                        entity.PanCardNo = model.PanCardNo;
                        entity.EPFUANNo = model.EPFUANNo;
                        entity.Active = isActivating;

                        if (model.JoiningDateStr != "" && model.JoiningDateStr != null)
                            entity.JoiningDate = DateTime.ParseExact(model.JoiningDateStr, "MM/dd/yyyy", null);
                        else
                            entity.JoiningDate = null;

                        if (model.ReleaseDateStr != "" && model.ReleaseDateStr != null)
                            entity.ReleaseDate = DateTime.ParseExact(model.ReleaseDateStr, "MM/dd/yyyy", null);
                        else
                            entity.ReleaseDate = null;

                        entity.EducationDetail = model.EducationDetail;
                        entity.UpdatedById = LoggedUserId;
                        entity.UpdatedOnUtc = GetDateTime;

                        if (model.SameASPermanentAddress == true)
                        {
                            var personDate = await _personService.GetPersonById(model.PersonId);
                            entity.SameASPermanentAddress = true;
                            entity.AddressId = personDate.AddressId;
                        }
                        else
                        {
                            string AddressId = _commonService.AddUpdateAddress(model.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, LoggedUserId);
                            entity.SameASPermanentAddress = false;
                            entity.AddressId = AddressId;
                        }

                        _employeeService.UpdateEmployee(entity);

                        // Update User if exists
                        var ExistingEmployeeUserId = _commonService.GetLoggeduserIdByEmployeeId(SiteId, id);
                        if (ExistingEmployeeUserId != null)
                        {
                            var user = await _userManager.FindByIdAsync(ExistingEmployeeUserId);
                            if (user != null)
                            {
                                user.Active = isActivating;
                                user.UpdatedById = LoggedUserId;
                                user.UpdatedOnUtc = GetDateTime;

                                await _userManager.SetLockoutEndDateAsync(user, isActivating ? null : DateTimeOffset.UtcNow.AddYears(100));
                                await _userManager.UpdateAsync(user);
                            }
                        }
                    }
                    if (model.Tab == "2_tab")
                    {
                        //save employee types
                        if (model.EmployeeTypeModel.Count() > 0)
                        {
                            var addList = new List<EmployeeType>();
                            var deleteList = new List<EmployeeType>();
                            var updateList = new List<EmployeeType>();

                            foreach (var item in model.EmployeeTypeModel)
                            {
                                // Fetch the employee type entity by its ID
                                var type = await _employeeTypeService.GetEmployeeTypeById(item.Id);
                                if (item.Flag == "Edit")
                                {
                                    // If no employee type is found with the given ID, continue
                                    if (type == null)
                                        continue;

                                    type.EmployeeId = entity.Id;
                                    type.EmployeeTypeId = item.EmployeeTypeId;
                                    type.Note = item.Note;

                                    // Set custom properties
                                    if (item.StartDateStr != "" && item.StartDateStr != null)
                                        type.StartDate = DateTime.ParseExact(item.StartDateStr, "MM/dd/yyyy", null);

                                    if (item.EndDateStr != "" && item.EndDateStr != null)
                                    {
                                        type.Duration = item.Duration;
                                        type.EndDate = DateTime.ParseExact(item.EndDateStr, "MM/dd/yyyy", null);
                                    }
                                    else
                                        type.EndDate = null;

                                    // Set the Updated by and Updated on properties
                                    type.UpdatedById = LoggedUserId;
                                    type.UpdatedOnUtc = GetDateTime;
                                    updateList.Add(type);
                                }
                                else if (item.Flag == "New")
                                {
                                    // If no employee type is found with the given ID, continue
                                    if (type != null)
                                        continue;

                                    var data = _mapper.Map<EmployeeType>(item);

                                    data.EmployeeId = entity.Id;
                                    data.EmployeeTypeId = item.EmployeeTypeId;
                                    data.Note = item.Note;

                                    // Set custom properties
                                    if (item.StartDateStr != "" && item.StartDateStr != null)
                                        data.StartDate = DateTime.ParseExact(item.StartDateStr, "MM/dd/yyyy", null);
                                    if (item.EndDateStr != "" && item.EndDateStr != null)
                                    {
                                        data.Duration = item.Duration;
                                        data.EndDate = DateTime.ParseExact(item.EndDateStr, "MM/dd/yyyy", null);
                                    }

                                    // Set the created by and created on properties
                                    data.CreatedById = LoggedUserId;
                                    data.UpdatedById = LoggedUserId;
                                    data.CreatedOnUtc = GetDateTime;
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
                                _employeeTypeService.InsertEmployeeTypeList(addList);

                            if (updateList.Count > 0)
                                _employeeTypeService.UpdateEmployeeTypeList(updateList);

                            if (deleteList.Count > 0)
                                _employeeTypeService.DeleteEmployeeTypeList(deleteList);
                        }

                        //save employee statuses
                        if (model.EmployeeStatusModel.Count() > 0)
                        {
                            //var statusEntities = new List<EmployeeStatus>();
                            var addList = new List<EmployeeStatus>();
                            var deleteList = new List<EmployeeStatus>();
                            var updateList = new List<EmployeeStatus>();

                            foreach (var item in model.EmployeeStatusModel)
                            {
                                // Fetch the employee Status entity by its ID
                                var Status = await _employeeStatusService.GetEmployeeStatusById(item.Id);
                                if (item.Flag == "Edit")
                                {
                                    // If no employee Status is found with the given ID, continue
                                    if (Status == null)
                                        continue;

                                    Status.EmployeeId = entity.Id;
                                    Status.EmployeeStatusId = item.EmployeeStatusId;
                                    Status.Note = item.Note;

                                    // Set custom properties
                                    if (item.StatusStartDateStr != "" && item.StatusStartDateStr != null)
                                        Status.StartDate = DateTime.ParseExact(item.StatusStartDateStr, "MM/dd/yyyy", null);

                                    if (item.StatusEndDateStr != "" && item.StatusEndDateStr != null)
                                    {
                                        Status.Duration = item.Duration;
                                        Status.EndDate = DateTime.ParseExact(item.StatusEndDateStr, "MM/dd/yyyy", null);
                                    }
                                    else
                                        Status.EndDate = null;

                                    // Set the Updated by and Updated on properties
                                    Status.UpdatedById = LoggedUserId;
                                    Status.UpdatedOnUtc = GetDateTime;
                                    updateList.Add(Status);
                                }
                                else if (item.Flag == "New")
                                {
                                    // If no employee Status is found with the given ID, continue
                                    if (Status != null)
                                        continue;

                                    var data = _mapper.Map<EmployeeStatus>(item);
                                    data.EmployeeId = entity.Id;
                                    data.EmployeeStatusId = item.EmployeeStatusId;
                                    data.Note = item.Note;

                                    // Set custom properties
                                    if (item.StatusStartDateStr != "" && item.StatusStartDateStr != null)
                                        data.StartDate = DateTime.ParseExact(item.StatusStartDateStr, "MM/dd/yyyy", null);
                                    if (item.StatusEndDateStr != "" && item.StatusEndDateStr != null)
                                    {
                                        data.Duration = item.Duration;
                                        data.EndDate = DateTime.ParseExact(item.StatusEndDateStr, "MM/dd/yyyy", null);
                                    }
                                    // Set the created by and created on properties
                                    data.CreatedById = LoggedUserId;
                                    data.UpdatedById = LoggedUserId;
                                    data.CreatedOnUtc = GetDateTime;
                                    data.UpdatedOnUtc = GetDateTime;
                                    addList.Add(data);
                                }
                                else if (item.Flag == "Delete")
                                {
                                    // If no employee Status is found with the given ID, continue
                                    if (Status == null)
                                        continue;

                                    deleteList.Add(Status);
                                }
                            }

                            if (addList.Count > 0)
                                _employeeStatusService.InsertEmployeeStatusList(addList);

                            if (updateList.Count > 0)
                                _employeeStatusService.UpdateEmployeeStatusList(updateList);

                            if (deleteList.Count > 0)
                                _employeeStatusService.DeleteEmployeeStatusList(deleteList);
                        }

                        //save employee department
                        if (model.EmployeeDepartmentModel.Count() > 0)
                        {
                            //var departmentEntities = new List<EmployeeDepartment>();
                            var addList = new List<EmployeeDepartment>();
                            var deleteList = new List<EmployeeDepartment>();
                            var updateList = new List<EmployeeDepartment>();

                            foreach (var item in model.EmployeeDepartmentModel)
                            {
                                // Fetch the employee department entity by its ID
                                var department = await _employeeDepartmentService.GetEmployeeDepartmentById(item.Id);
                                if (item.Flag == "Edit")
                                {
                                    // If no employee department is found with the given ID, continue
                                    if (department == null)
                                        continue;

                                    department.EmployeeId = entity.Id;
                                    department.EmployeeDepartmentId = item.EmployeeDepartmentId;
                                    department.Note = item.Note;

                                    // Set custom properties
                                    if (item.DepartmentStartDateStr != "" && item.DepartmentStartDateStr != null)
                                        department.StartDate = DateTime.ParseExact(item.DepartmentStartDateStr, "MM/dd/yyyy", null);

                                    if (item.DepartmentEndDateStr != "" && item.DepartmentEndDateStr != null)
                                    {
                                        department.Duration = item.Duration;
                                        department.EndDate = DateTime.ParseExact(item.DepartmentEndDateStr, "MM/dd/yyyy", null);
                                    }
                                    else
                                        department.EndDate = null;

                                    // Set the Updated by and Updated on properties
                                    department.UpdatedById = LoggedUserId;
                                    department.UpdatedOnUtc = GetDateTime;
                                    updateList.Add(department);
                                }
                                else if (item.Flag == "New")
                                {
                                    // If no employee department is found with the given ID, continue
                                    if (department != null)
                                        continue;

                                    var data = _mapper.Map<EmployeeDepartment>(item);
                                    data.EmployeeId = id;
                                    data.EmployeeDepartmentId = item.EmployeeDepartmentId;
                                    data.Note = item.Note;

                                    // Set custom properties
                                    if (item.DepartmentStartDateStr != "" && item.DepartmentStartDateStr != null)
                                        data.StartDate = DateTime.ParseExact(item.DepartmentStartDateStr, "MM/dd/yyyy", null);
                                    if (item.DepartmentEndDateStr != "" && item.DepartmentEndDateStr != null)
                                    {
                                        data.Duration = item.Duration;
                                        data.EndDate = DateTime.ParseExact(item.DepartmentEndDateStr, "MM/dd/yyyy", null);
                                    }

                                    // Set the created by and created on properties
                                    data.CreatedById = LoggedUserId;
                                    data.UpdatedById = LoggedUserId;
                                    data.CreatedOnUtc = GetDateTime;
                                    data.UpdatedOnUtc = GetDateTime;
                                    addList.Add(data);
                                }
                                else if (item.Flag == "Delete")
                                {
                                    // If no employee department is found with the given ID, continue
                                    if (department == null)
                                        continue;

                                    deleteList.Add(department);
                                }
                            }

                            if (addList.Count > 0)
                                _employeeDepartmentService.InsertEmployeeDepartmentList(addList);

                            if (updateList.Count > 0)
                                _employeeDepartmentService.UpdateEmployeeDepartmentList(updateList);

                            if (deleteList.Count > 0)
                                _employeeDepartmentService.DeleteEmployeeDepartmentList(deleteList);
                        }

                        //save employee designation
                        if (model.EmployeeDesignationModel.Count() > 0)
                        {
                            var addList = new List<EmployeeDesignation>();
                            var deleteList = new List<EmployeeDesignation>();
                            var updateList = new List<EmployeeDesignation>();

                            foreach (var item in model.EmployeeDesignationModel)
                            {
                                // Fetch the employee designation entity by its ID
                                var designation = await _employeeDesignationService.GetEmployeeDesignationById(item.Id);
                                if (item.Flag == "Edit")
                                {
                                    // If no employee designation is found with the given ID, continue
                                    if (designation == null)
                                        continue;

                                    designation.EmployeeDesignationId = item.EmployeeDesignationId;
                                    designation.ShiftId = item.ShiftId;
                                    designation.LeaveApproverId = item.LeaveApproverId;
                                    designation.Note = item.Note;

                                    // Set custom properties
                                    if (item.DesignationStartDateStr != "" && item.DesignationStartDateStr != null)
                                        designation.StartDate = DateTime.ParseExact(item.DesignationStartDateStr, "MM/dd/yyyy", null);

                                    if (item.DesignationEndDateStr != "" && item.DesignationEndDateStr != null)
                                    {
                                        designation.Duration = item.Duration;
                                        designation.EndDate = DateTime.ParseExact(item.DesignationEndDateStr, "MM/dd/yyyy", null);
                                    }
                                    else
                                        designation.EndDate = null;

                                    // Set the Updated by and Updated on properties
                                    designation.UpdatedById = LoggedUserId;
                                    designation.UpdatedOnUtc = GetDateTime;
                                    updateList.Add(designation);
                                }
                                else if (item.Flag == "New")
                                {
                                    // If no employee designation is found with the given ID, continue
                                    if (designation != null)
                                        continue;

                                    var data = _mapper.Map<EmployeeDesignation>(item);
                                    data.EmployeeId = entity.Id;
                                    data.EmployeeDesignationId = item.EmployeeDesignationId;
                                    data.ShiftId = item.ShiftId;
                                    data.LeaveApproverId = item.LeaveApproverId;
                                    data.Note = item.Note;
                                    // Set custom properties
                                    if (item.DesignationStartDateStr != "" && item.DesignationStartDateStr != null)
                                        data.StartDate = DateTime.ParseExact(item.DesignationStartDateStr, "MM/dd/yyyy", null);
                                    if (item.DesignationEndDateStr != "" && item.DesignationEndDateStr != null)
                                    {
                                        data.Duration = item.Duration;
                                        data.EndDate = DateTime.ParseExact(item.DesignationEndDateStr, "MM/dd/yyyy", null);
                                    }
                                    // Set the created by and created on properties
                                    data.CreatedById = LoggedUserId;
                                    data.UpdatedById = LoggedUserId;
                                    data.CreatedOnUtc = GetDateTime;
                                    data.UpdatedOnUtc = GetDateTime;
                                    addList.Add(data);
                                }
                                else if (item.Flag == "Delete")
                                {
                                    // If no employee designation is found with the given ID, continue
                                    if (designation == null)
                                        continue;

                                    deleteList.Add(designation);
                                }
                            }

                            if (addList.Count > 0)
                                _employeeDesignationService.InsertEmployeeDesignationList(addList);

                            if (updateList.Count > 0)
                                _employeeDesignationService.UpdateEmployeeDesignationList(updateList);

                            if (deleteList.Count > 0)
                                _employeeDesignationService.DeleteEmployeeDesignationList(deleteList);
                        }

                        //save employee Org Location
                        if (model.EmployeeOrgLocationModel.Count() > 0)
                        {
                            var addList = new List<EmployeeOrgLocation>();
                            var deleteList = new List<EmployeeOrgLocation>();
                            var updateList = new List<EmployeeOrgLocation>();

                            foreach (var item in model.EmployeeOrgLocationModel)
                            {
                                // Fetch the employee Org Location entity by its ID
                                var orgLocation = await _employeeOrgLocationService.GetEmployeeOrgLocationById(item.Id);
                                if (item.Flag == "Edit")
                                {
                                    // If no employee Org Location is found with the given ID, continue
                                    if (orgLocation == null)
                                        continue;

                                    orgLocation.OrgLocationId = item.OrgLocationId;
                                    orgLocation.Note = item.Note;
                                    // Set custom properties
                                    if (item.OrgLocationStartDateStr != "" && item.OrgLocationStartDateStr != null)
                                        orgLocation.StartDate = DateTime.ParseExact(item.OrgLocationStartDateStr, "MM/dd/yyyy", null);

                                    if (item.OrgLocationEndDateStr != "" && item.OrgLocationEndDateStr != null)
                                    {
                                        orgLocation.Duration = item.Duration;
                                        orgLocation.EndDate = DateTime.ParseExact(item.OrgLocationEndDateStr, "MM/dd/yyyy", null);
                                    }
                                    else
                                        orgLocation.EndDate = null;

                                    // Set the Updated by and Updated on properties
                                    orgLocation.UpdatedById = LoggedUserId;
                                    orgLocation.UpdatedOnUtc = GetDateTime;
                                    updateList.Add(orgLocation);
                                }
                                else if (item.Flag == "New")
                                {
                                    // If no employee Org Location is found with the given ID, continue
                                    if (orgLocation != null)
                                        continue;

                                    var data = _mapper.Map<EmployeeOrgLocation>(item);
                                    data.EmployeeId = entity.Id;
                                    data.OrgLocationId = item.OrgLocationId;
                                    data.Note = item.Note;
                                    // Set custom properties
                                    if (item.OrgLocationStartDateStr != "" && item.OrgLocationStartDateStr != null)
                                        data.StartDate = DateTime.ParseExact(item.OrgLocationStartDateStr, "MM/dd/yyyy", null);
                                    if (item.OrgLocationEndDateStr != "" && item.OrgLocationEndDateStr != null)
                                    {
                                        data.Duration = item.Duration;
                                        data.EndDate = DateTime.ParseExact(item.OrgLocationEndDateStr, "MM/dd/yyyy", null);
                                    }
                                    // Set the created by and created on properties
                                    data.CreatedById = LoggedUserId;
                                    data.UpdatedById = LoggedUserId;
                                    data.CreatedOnUtc = GetDateTime;
                                    data.UpdatedOnUtc = GetDateTime;
                                    addList.Add(data);
                                }
                                else if (item.Flag == "Delete")
                                {
                                    // If no employee Org Location is found with the given ID, continue
                                    if (orgLocation == null)
                                        continue;

                                    deleteList.Add(orgLocation);
                                }
                            }

                            if (addList.Count > 0)
                                _employeeOrgLocationService.InsertEmployeeOrgLocationList(addList);

                            if (updateList.Count > 0)
                                _employeeOrgLocationService.UpdateEmployeeOrgLocationList(updateList);

                            if (deleteList.Count > 0)
                                _employeeOrgLocationService.DeleteEmployeeOrgLocationList(deleteList);
                        }
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

        #region DeleteEmployee
        // Title: DeleteEmployeeById
        // Description: This endpoint deletes a employee based on the provided employee ID. It first retrieves the employee entity by ID, checks if it exists, and if so, deletes the employee. If the employee is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            try
            {
                // Fetch the employee entity by its ID
                var entity = await _employeeService.GetById(id);
                // If no employee is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No employee found with the specified id."));

                // Delete the employee using the employee service
                _employeeService.DeleteEmployee(entity);

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