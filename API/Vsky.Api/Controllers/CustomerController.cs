using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.Companies;
using Vsky.Services.DropDowns;
using Vsky.Services.Note;
using Vsky.Services.Sites;
using System.Linq.Dynamic.Core;
using System.ComponentModel.Design;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.Employees;
using System.Diagnostics;

namespace Vsky.Api.Controllers
{
    [Route("customer")]
    public class CustomerController : BaseController
    {
        #region Services Initialization
        private readonly IMapper _mapper;
        private readonly GlobalVariable _globalVariable;
        private readonly ICompanyContactsService _companyContactService;
        private readonly ICommonService _commonService;
        private readonly ICompanyClientsService _companyClientsService;
        private readonly IDropDownService _dropDownService;
        private readonly ISiteService _siteService;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly IEmployeeService _employeeService;

        public CustomerController(
            IMapper mapper,
            GlobalVariable globalVariable,
            ICompanyContactsService companyContactsService,
            ICommonService commonService,
            ICompanyClientsService companyClientsService,
            IDropDownService dropDownService,
            ISiteService siteService, ISitesModifiedLogsService sitesModifiedLogsService, IEmployeeService employeeService)
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _companyContactService = companyContactsService;
            _commonService = commonService;
            _companyClientsService = companyClientsService;
            _dropDownService = dropDownService;
            _siteService = siteService;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _employeeService = employeeService;
        }
        #endregion

        #region GetAllCustomers
        [HttpPost("list")]
        public IActionResult GetAllCustomers(CompanyClientsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                // Fetch the list of customers based on search criteria
                var list = _companyClientsService.GetAllCustomers(SiteId, searchModel.SearchText, searchModel.CustomerTypeIds, searchModel.CustomerIds, searchModel.EmployeeIds, searchModel.EmailAddress, searchModel.PhoneNumber, searchModel.ParentCustomerIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                List<CustomerModel> customerList = new List<CustomerModel>();
                foreach (var item in list)
                {
                    var customerModel = new CustomerModel
                    {
                        CompanyId = item.CompanyId,
                        Id = item.Id,
                        PersonId = item.PersonId,
                        Name = item.Company?.Name ?? item.Person?.FirstName + " " + item.Person?.LastName ?? string.Empty,
                        Website = item.Company?.Website ?? string.Empty,
                        EmailAddress = item.Company?.EmailAddress ?? item.Person?.PrimaryEmailAddress ?? string.Empty,
                        PhoneNumber = item.Company?.PhoneNumber ?? item.Person?.PrimaryPhoneNumber ?? string.Empty,
                        CustomerType = item.CustomerType?.DropDownValue ?? string.Empty,
                        CustomerTypeId = item.CustomerType?.Id ?? string.Empty,
                        UpdatedOnUtc = item.UpdatedOnUtc,
                        AssignedDate = item.AssignedDate,
                        AssignedToId = item.AssignedToId,
                        AssignedToName = item.AssignedTo?.Person?.FirstName + " " + item.AssignedTo?.Person?.LastName ?? string.Empty,
                        ParentCustomerName = item.ParentCustomerName,
                        CustomerNoteCount = item.CustomerNoteCount
                    };

                    customerList.Add(customerModel);
                }

                // Convert to IQueryable for sorting
                var sortCustomer = customerList.AsQueryable();
                //sorting
                if (!string.IsNullOrWhiteSpace(searchModel.SortBy))
                {
                    sortCustomer = sortCustomer.OrderBy($"{searchModel.SortBy} {(searchModel.Descending ? "desc" : "asc")}");
                }
                else
                {
                    sortCustomer = sortCustomer.OrderBy(x => x.Name)
                                               .ThenBy(x => x.Website)
                                               .ThenBy(x => x.EmailAddress)
                                               .ThenBy(x => x.PhoneNumber)
                                               .ThenBy(x => x.CustomerType);
                }
                // Convert sorted list to a final list
                var sortedCustomerList = sortCustomer.ToList();

                // Map the fetched list to a model
                var model = new CustomerListModel
                {
                    Data = _mapper.Map<IList<CustomerModel>>(sortedCustomerList),
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

        #region get details by customer id
        [HttpGet("{id}/customerdetails")]
        public async Task<IActionResult> GetCustomerDetailsById(string id)
        {
            var entity = await _companyClientsService.GetCustomerDetailsById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No customer found with the specified id."));

            var model = _mapper.Map<CompanyClientsModel>(entity);
            return Ok(model);
        }
        #endregion

        #region create customer
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(CompanyClientsModel model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (!string.IsNullOrEmpty(model.PersonId))
                    {
                        var companyCustomer = await _companyClientsService.GetByPersonId(model.PersonId);
                        if (companyCustomer != null)
                            return BadRequest(new BadRequestError("Customer already exists."));
                    }

                    if (!string.IsNullOrEmpty(model.CompanyId))
                    {
                        var companyCustomer = await _companyClientsService.GetByCompanyId(model.CompanyId);
                        if (companyCustomer != null)
                            return BadRequest(new BadRequestError("Customer already exists."));
                    }

                    //var entity = _mapper.Map<CompanyClients>(model);
                    var entity = new CompanyClients
                    {
                        CompanyId = !string.IsNullOrEmpty(model.CompanyId) ? model.CompanyId : null,
                        PersonId = !string.IsNullOrEmpty(model.PersonId) ? model.PersonId : null,
                        SiteId = SiteId,
                        CustomerTypeId = model.CustomerTypeId,
                        AssignedToId = model.AssignedToId,
                        AssignedDate = model.AssignedDate,
                        ParentCustomerId = model.ParentCustomerId,
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime
                    };

                    _companyClientsService.InsertCompanyClient(entity);

                    var addCompanyContacts = new List<CompanyContacts>();
                    var updateComapnyContacts = new List<CompanyContacts>();

                    if(model.Company != null)
                    {
                        if (model.Company.CompanyContacts.Count() > 0 && model.Company.CompanyContacts.Any() && !string.IsNullOrEmpty(model.CompanyId))
                        {
                            foreach (var items in model.Company.CompanyContacts)
                            {
                                var contactExists = await _companyContactService.GetByPersonAndCompanyId(model.CompanyId, items.PersonId);
                                if (contactExists != null)
                                {
                                    // Update existing contact
                                    contactExists.AlternatePhoneNumber = items.AlternatePhoneNumber;
                                    contactExists.AlternateEmail = items.AlternateEmail;
                                    contactExists.UpdatedById = LoggedUserId;
                                    contactExists.UpdatedOnUtc = GetDateTime;
                                    updateComapnyContacts.Add(contactExists);
                                }
                                else if (contactExists == null && !items.Deleted)
                                {
                                    // Add new contact
                                    var customerContactEntity = new CompanyContacts
                                    {
                                        CompanyId = model.CompanyId,
                                        PersonId = items.PersonId,
                                        AlternatePhoneNumber = items.AlternatePhoneNumber,
                                        AlternateEmail = items.AlternateEmail,
                                        CreatedById = LoggedUserId,
                                        UpdatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime,
                                        UpdatedOnUtc = GetDateTime
                                    };
                                    addCompanyContacts.Add(customerContactEntity);
                                }
                            }
                        }

                        if (addCompanyContacts.Count > 0)
                        {
                            _companyContactService.InsertCompanyContactList(addCompanyContacts);
                        }

                        if (updateComapnyContacts.Count > 0)
                        {
                            _companyContactService.UpdateCompanyContactList(updateComapnyContacts);
                        }
                    }                  

                    if (!string.IsNullOrEmpty(model.AssignedToId) && model.AssignedToId != "null")
                    {
                        var customer = await _companyClientsService.GetCustomerDetailsById(entity.Id);
                        var customerName = customer.CompanyId != null ? customer.Company.Name : customer.Person.FirstName + " " + customer.Person.LastName;
                        //var companyOrPersonId = customer.CompanyId != null ? customer.CompanyId : customer.PersonId;

                        var assignedToName = await _employeeService.GetEmployeeDetailsById(model.AssignedToId);
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "CompanyCustomers", entity.Id, customerName, entity.Id, customerName, "Customer Advocate", assignedToName.Person.FirstName + " " + assignedToName.Person.LastName, LoggedUserId, GetDateTime);
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

        #region update customer
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(string id, CompanyClientsModel model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _companyClientsService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No customer found with the specified id."));

                    bool IsAssignedToChanged = (!string.IsNullOrEmpty(model.AssignedToId) && model.AssignedToId != "undefined" && model.AssignedToId != entity.AssignedToId) ? true : false;

                    entity.CustomerTypeId = model.CustomerTypeId;
                    entity.AssignedToId = model.AssignedToId;
                    entity.AssignedDate = model.AssignedDate;
                    entity.ParentCustomerId = model.ParentCustomerId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _companyClientsService.UpdateCompanyClient(entity);

                    //CompanyContacts Add/Update
                    var customerContactsEntitiesToAdd = new List<CompanyContacts>();
                    var customerContactsEntitiesToUpdate = new List<CompanyContacts>();
                    var customerContactsEntitiesToDelete = new List<CompanyContacts>();

                    if (model.Company != null)
                    {
                        if (model.Company.CompanyContacts.Count() > 0 && model.Company.CompanyContacts.Any() && !string.IsNullOrEmpty(model.CompanyId))
                        {
                            foreach (var items in model.Company.CompanyContacts)
                            {
                                var exisitingContactData = await _companyContactService.GetById(items.Id);

                                if (exisitingContactData != null && !items.Deleted)
                                {
                                    var existsContact = await _companyContactService.GetByPersonAndCompanyId(model.CompanyId, items.PersonId, items.Id);
                                    if (existsContact != null)
                                        continue;

                                    // Update existing contact
                                    exisitingContactData.PersonId = items.PersonId;
                                    exisitingContactData.CompanyId = model.CompanyId;
                                    exisitingContactData.AlternatePhoneNumber = items.AlternatePhoneNumber;
                                    exisitingContactData.AlternateEmail = items.AlternateEmail;
                                    exisitingContactData.Deleted = items.Deleted;
                                    exisitingContactData.UpdatedById = LoggedUserId;
                                    exisitingContactData.UpdatedOnUtc = GetDateTime;
                                    customerContactsEntitiesToUpdate.Add(exisitingContactData);
                                }
                                else if (exisitingContactData == null && !items.Deleted)
                                {
                                    var existsContact = await _companyContactService.GetByPersonAndCompanyId(model.CompanyId, items.PersonId, null);
                                    if (existsContact != null)
                                        continue;
                                    // Add new contact
                                    CompanyContacts customerContactEntity = new CompanyContacts
                                    {
                                        CompanyId = model.CompanyId,
                                        PersonId = items.PersonId,
                                        AlternatePhoneNumber = items.AlternatePhoneNumber,
                                        AlternateEmail = items.AlternateEmail,
                                        CreatedById = LoggedUserId,
                                        UpdatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime,
                                        UpdatedOnUtc = GetDateTime
                                    };
                                    customerContactsEntitiesToAdd.Add(customerContactEntity);

                                }
                                else if (exisitingContactData != null && items.Deleted)
                                {
                                    var contactData = await _companyContactService.GetById(items.Id);
                                    if (contactData == null)
                                        continue;
                                    customerContactsEntitiesToDelete.Add(contactData);
                                }
                            }
                        }
                        if (customerContactsEntitiesToAdd.Count > 0)
                        {
                            _companyContactService.InsertCompanyContactList(customerContactsEntitiesToAdd);
                        }

                        if (customerContactsEntitiesToUpdate.Count > 0)
                        {
                            _companyContactService.UpdateCompanyContactList(customerContactsEntitiesToUpdate);
                        }

                        if (customerContactsEntitiesToDelete.Count > 0)
                        {
                            _companyContactService.DeleteCompanyContactList(customerContactsEntitiesToDelete);
                        }
                    }                  

                    if (IsAssignedToChanged)
                    {
                        var customer = await _companyClientsService.GetCustomerDetailsById(entity.Id);
                        var customerName = customer.CompanyId != null ? customer.Company.Name : customer.Person.FirstName + " " + customer.Person.LastName;
                        //var companyOrPersonId = customer.CompanyId != null ? customer.CompanyId : customer.PersonId;

                        var assignedToName = await _employeeService.GetEmployeeDetailsById(model.AssignedToId);
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "CompanyCustomers", entity.Id, customerName, entity.Id, customerName, "Customer Advocate", assignedToName.Person.FirstName + " " + assignedToName.Person.LastName, LoggedUserId, GetDateTime);
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

        #region UpdateCustomerAdvocate
        [HttpPut("customer-advocate/{id}/{assignedToId}")]
        public async Task<IActionResult> UpdateCustomerAdvocate(string id, string assignedToId)
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
                    var entity = await _companyClientsService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Customer found with the specified id."));

                    entity.AssignedToId = assignedToId;
                    entity.AssignedDate = GetDateTime;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _companyClientsService.UpdateCompanyClient(entity);

                    if (!string.IsNullOrEmpty(assignedToId) && assignedToId != "null")
                    {
                        var customer = await _companyClientsService.GetCustomerDetailsById(id);
                        var customerName = customer.CompanyId != null ? customer.Company.Name : customer.Person.FirstName + " " + customer.Person.LastName;
                        //var companyOrPersonId = customer.CompanyId != null ? customer.CompanyId : customer.PersonId;

                        var assignedToName = await _employeeService.GetEmployeeDetailsById(assignedToId);
                        _sitesModifiedLogsService.AddSiteModifiedLogs(SiteId, "CompanyCustomers", entity.Id, customerName, entity.Id, customerName, "Customer Advocate", assignedToName.Person.FirstName + " " + assignedToName.Person.LastName, LoggedUserId, GetDateTime);
                    }

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

        #region DeleteCompany
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                var entity = await _companyClientsService.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No customer found with the specified id."));

                _companyClientsService.DeleteCustomer(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllCustomerContactListForDropdown
        // Title: GetAllProjectListForDropdown
        // Description: This endpoint retrieves the details of a specific project based on its unique identifier (ID). 
        [HttpGet("contactdropdown/list")]
        public async Task<IActionResult> GetAllCustomerContactListForDropdown(string siteId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
                var status =await _dropDownService.GetByName(SiteId, "Converted");
                var list = await _companyContactService.GetAllCustomerContactListForDropdown(SiteId, status.Id);
                var model = _mapper.Map<List<CompanyContactsModels>>(list);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        //new bs
        #region GetAllClientListForDropdown
        // Title: GetAllClientListForDropdown
        // Description: This endpoint retrieves the list of Client For Dropdown
        [HttpGet("customerdropdown/list")]
        public async Task<IActionResult> GetAllClientListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _companyClientsService.GetAllClientListForDropdown(SiteId);
                var model = _mapper.Map<List<CompanyClientsModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllParentCustomerListForDropdown
        [HttpGet("parentCustomerList/dropdownlist")]
        [HttpGet("parentCustomerList/dropdownlist/{customerId?}")]
        public async Task<IActionResult> GetAllParentCustomerListForDropdown(string customerId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _companyClientsService.GetAllParentCustomerListForDropdown(SiteId, customerId);
                var model = _mapper.Map<List<CompanyClientsModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("parentCustomerList/list")]
        public async Task<IActionResult> GetAllParentCustomerList()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _companyClientsService.GetAllParentCustomerList(SiteId);
                var model = _mapper.Map<List<CompanyClientsModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllContactListForDropdown
        // Title: GetAllContactListForDropdown
        // Description: This endpoint retrieves the list of company contacts For Dropdown. 
        [HttpGet("companycontactdropdown/list")]
        public async Task<IActionResult> GetAllContactListForDropdown(string customerId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _companyContactService.GetAllContactListForDropdown(SiteId, customerId);
                var model = _mapper.Map<List<CompanyContactsModels>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllContactListByCompanyIdForDropdown
        // Title: GetAllContactListByCompanyIdForDropdown
        // Description: This endpoint retrieves the list of company contacts For Dropdown. 
        [HttpGet("contactlistbycompanyidFordropdown/list")]
        public async Task<IActionResult> GetAllContactListByCompanyIdForDropdown(string companyId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _companyContactService.GetAllContactListByCompanyIdForDropdown(SiteId, companyId);
                var model = _mapper.Map<List<CompanyContactsModels>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("companyContactlistFordropdown/list")]
        public async Task<IActionResult> GetAllCompanyContactList(string customerId = null)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var comapnyByCustomerId = _companyClientsService.GetCompanyContactIdById(customerId);
                var list = await _companyContactService.GetAllContactListByCompanyIdForDropdown(SiteId, comapnyByCustomerId);
                var model = _mapper.Map<List<CompanyContactsModels>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllCompanyListForCustomerDropdown
        [HttpGet("companyCustomerDropdownList")]
        public async Task<IActionResult> GetAllCompanyListForCustomerDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _companyClientsService.GetAllCompanyListForCustomersDropdown(SiteId);
                var model = _mapper.Map<List<CompanyContactsModels>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
