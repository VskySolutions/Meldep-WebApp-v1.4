using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.Companies;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("company")] 
    public class CompanyController : BaseController
    {
        #region Services Initialization
        private readonly IMapper _mapper;
        private readonly GlobalVariable _globalVariable;
        private readonly ICompanyService _companyService;
        private readonly ICompanyContactsService _companyContactService;
        private readonly ICommonService _commonService;
        private readonly ICompanyClientsService _companyClientsService;
        private readonly ISiteService _siteService;
        private readonly ApplicationDbContext _db;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        public CompanyController(
            IMapper mapper,
            GlobalVariable globalVariable,
            ICompanyService companyService,
            ApplicationDbContext db,
            ICompanyContactsService companyContactsService,
            ISiteService siteService,
            ICommonService commonService,
            ICompanyClientsService companyClientsService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _companyService = companyService;
            _companyContactService = companyContactsService;
            _commonService = commonService;
            _siteService = siteService;
            _companyClientsService = companyClientsService;
            _db = db;
            _commonService = commonService;
            _azureBlobImageServices = azureBlobImageServices;
        }

        #endregion

        #region GetAllCompanies
        [HttpPost("list")]
        public IActionResult GetAllCompanies(CompanySearchModel searchModel)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = _companyService.GetAllCompanies(SiteId, searchModel.SearchText, searchModel.CompanyId, searchModel.BusinessTypeId, searchModel.EmployeeId, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

            var model = new CompanyListModel
            {
                Data = _mapper.Map<IList<CompanyModel>>(list),
                Total = list.TotalCount
            };

            return Ok(model);
        }
        #endregion

        #region GetCompanyById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompanyById(string id)
        {
            var entity = await _companyService.GetById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No company found with the specified id."));

            var model = _mapper.Map<CompanyModel>(entity);
            return Ok(model);
        }
        #endregion

        #region GetcompanydetailsById
        [HttpGet("{id}/companydetails")]
        public async Task<IActionResult> GetcompanydetailsById(string id)
        {
            var entity = await _companyService.GetcompanydetailsById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No company found with the specified id."));

            var model = _mapper.Map<CompanyModel>(entity);
            return Ok(model);
        }

        //[HttpGet("{id}/getCompanyDetailsForCustomer")]
        //public async Task<IActionResult> GetCompanyDetailsForCustomer(string id)
        //{
        //    var entity = await _companyService.GetCompanyDetailsForCustomer(id);
        //    if (entity == null)
        //        return BadRequest(new BadRequestError("No company found with the specified id."));

        //    var model = _mapper.Map<CompanyModel>(entity);
        //    return Ok(model);
        //}
        #endregion

        #region GetAllContacts
        [HttpGet("contacts")]
        public async Task<IActionResult> GetAllContacts(string companyId)
        {
            var list = await _companyContactService.GetAllContacts(companyId);
            var model = _mapper.Map<IList<CompanyContacts>>(list);
            return Ok(model);
        }
        #endregion

        #region CreateCompany
        [HttpPost]
        public async Task<IActionResult> CreateCompany(CompanyModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = string.IsNullOrWhiteSpace(model.SiteId) ? _globalVariable.SiteId : model.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = _db.Company.Any(x => x.Name == model.Name && !x.Deleted);
                    if (exists)
                        return BadRequest(new BadRequestError("Company name already exists."));

                    var convertedStatusId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Company Status", "Converted");
                    string AddressId = _commonService.AddUpdateAddress(model.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, LoggedUserId);

                    if (!string.IsNullOrWhiteSpace(model.SiteId))
                        model.IsCustomer = true;

                    var entity = _mapper.Map<Company>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    if (model.IsCustomer)
                    {
                        entity.StatusId = convertedStatusId;
                    }
                    else
                    {
                        entity.StatusId = model.StatusId;
                    }

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "company",
                                entity.Id
                            );
                    }

                    entity.AddressId = AddressId;
                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _companyService.InsertCompany(entity);

                    //Add Company Employees
                    string CompanyId = entity.Id;

                    var customerContactsEntities = new List<CompanyContacts>();
                    if (model.CompanyContactModel.Count() > 0)
                    {
                        foreach (var items in model.CompanyContactModel)
                        {
                            var customerContactEntity = _mapper.Map<CompanyContacts>(items);
                            customerContactEntity.CompanyId = entity.Id;
                            //customerContactEntity.SiteId = SiteId;
                            customerContactEntity.CreatedById = LoggedUserId;
                            customerContactEntity.UpdatedById = LoggedUserId;
                            customerContactEntity.CreatedOnUtc = GetDateTime;
                            customerContactEntity.UpdatedOnUtc = GetDateTime;
                            customerContactsEntities.Add(customerContactEntity);
                        }
                    }
                    if (customerContactsEntities.Count > 0)
                    {
                        _companyContactService.InsertCompanyContactList(customerContactsEntities);

                    }

                    if(model.IsCustomer)
                    {
                        var companyCustomer = new CompanyClients();
                        companyCustomer.CompanyId = entity.Id;
                        companyCustomer.SiteId = SiteId;
                        companyCustomer.CustomerTypeId = model.CustomerTypeId;
                        companyCustomer.AssignedToId = model.AssignedToId;
                        companyCustomer.AssignedDate = model.AssignedDate;
                        companyCustomer.CreatedById = LoggedUserId;
                        companyCustomer.UpdatedById = LoggedUserId;
                        companyCustomer.CreatedOnUtc = GetDateTime;
                        companyCustomer.UpdatedOnUtc = GetDateTime;
                        _companyClientsService.InsertCompanyClient(companyCustomer);
                    }
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateCompany
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(string id, CompanyModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = string.IsNullOrWhiteSpace(model.SiteId) ? _globalVariable.SiteId : model.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    
                    var companyStatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Company Status", "Converted");

                    var exists = _db.Company.Any(x => x.Name == model.Name && !x.Deleted && x.Id != id);
                    if (exists)
                        return BadRequest(new BadRequestError("Name already exists."));

                    string AddressId = _commonService.AddUpdateAddress(model.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, LoggedUserId);

                    var entity = await _companyService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No company found with the specified id."));

                    if (model.IsCustomer)
                    {
                        entity.StatusId = companyStatus;
                    }
                    else
                    {
                        entity.StatusId = model.StatusId;
                    }

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "company",
                                entity.Id,
                                entity.Description
                            );
                    }

                    entity.EmployeeId = model.EmployeeId;
                    entity.Name = model.Name;
                    entity.EmailAddress = model.EmailAddress;
                    entity.AlternativeEmailAddress = model.AlternativeEmailAddress;
                    entity.PhoneNumber = model.PhoneNumber;
                    entity.AlternativePhoneNumber = model.AlternativePhoneNumber;
                    entity.Website = model.Website;
                    entity.BusinessTypeId = model.BusinessTypeId;
                    entity.ProfileLink = model.ProfileLink;
                    entity.AddressId = AddressId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _companyService.UpdateCompany(entity);

                    var addList = new List<CompanyContacts>();

                    var deleteList = new List<CompanyContacts>();

                    var updateList = new List<CompanyContacts>();

                    if (model.CompanyContactModel.Count() > 0)
                    {
                        foreach (var item in model.CompanyContactModel)
                        {
                            item.CompanyId = id;
                            if (item.Flag == "Edit")
                            {
                                var existingConatct = await _companyContactService.GetById(item.Id);
                                if (existingConatct == null)
                                {
                                    continue;
                                }

                                existingConatct.CompanyId = item.CompanyId;
                                existingConatct.AlternateEmail = item.AlternateEmail;
                                existingConatct.AlternatePhoneNumber = item.AlternatePhoneNumber;
                                existingConatct.PersonId = item.PersonId;
                                existingConatct.CreatedOnUtc = item.CreatedOnUtc;
                                existingConatct.CreatedById = item.CreatedById;
                                existingConatct.UpdatedOnUtc = GetDateTime;
                                existingConatct.UpdatedById = LoggedUserId;
                                updateList.Add(existingConatct);
                            }
                            else if (item.Flag == "New")
                            {
                                var existingConatct = await _companyContactService.GetById(item.Id);
                                if (existingConatct != null)
                                {
                                    continue;
                                }
                                var newConatct = _mapper.Map<CompanyContacts>(item);
                                newConatct.CompanyId = item.CompanyId;
                                //newConatct.SiteId = SiteId;
                                newConatct.CreatedOnUtc = GetDateTime;
                                newConatct.UpdatedOnUtc = GetDateTime;
                                newConatct.CreatedById = LoggedUserId;
                                newConatct.UpdatedById = LoggedUserId;
                                addList.Add(newConatct);
                            }
                            else if (item.Flag == "Delete")
                            {
                                var existingConatct = await _companyContactService.GetById(item.Id);
                                if (existingConatct == null)
                                {
                                    continue;
                                }
                                existingConatct.CompanyId = item.CompanyId;
                                deleteList.Add(existingConatct);
                            }
                        }
                    }

                    if (addList.Count > 0)
                        _companyContactService.InsertCompanyContactList(addList);

                    if (updateList.Count > 0)
                        _companyContactService.UpdateCompanyContactList(updateList);

                    if (deleteList.Count > 0)
                        _companyContactService.DeleteCompanyContactList(deleteList);

                    if (model.StatusId == companyStatus)
                    {
                        var customerTypeId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Customer Type", "Business");

                        CompanyClients companyClients = new CompanyClients();
                        companyClients.CompanyId = id;
                        companyClients.SiteId = SiteId;
                        companyClients.CustomerTypeId = customerTypeId;
                        companyClients.CreatedOnUtc = GetDateTime;
                        companyClients.CreatedById = LoggedUserId;
                        companyClients.UpdatedById = LoggedUserId;
                        companyClients.UpdatedOnUtc = GetDateTime;
                        _companyClientsService.InsertCompanyClient(companyClients);
                    }

                    if (model.IsCustomer)
                    {
                        var companyCustomer = await _companyClientsService.GetById(model.CustomerId);
                        if (companyCustomer == null)
                            return BadRequest(new BadRequestError("No customer found with the specified id."));

                        companyCustomer.CustomerTypeId = model.CustomerTypeId;
                        companyCustomer.AssignedToId = model.AssignedToId;
                        companyCustomer.AssignedDate = model.AssignedDate;
                        companyCustomer.CreatedById = LoggedUserId;
                        companyCustomer.UpdatedById = LoggedUserId;
                        companyCustomer.CreatedOnUtc = GetDateTime;
                        companyCustomer.UpdatedOnUtc = GetDateTime;
                        _companyClientsService.UpdateCompanyClient(companyCustomer);
                    }

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateCompanyStatus
        [HttpPut("updateCompanyStatus/{id}/{statusId}")]
        public async Task<IActionResult> UpdateCompanyStatus(string id, string statusId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _companyService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No company found with the specified id."));

                    entity.StatusId = statusId;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _companyService.UpdateCompany(entity);

                    var companyStatus = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Company Status", "Converted");
                    if (statusId == companyStatus)
                    {
                        var customerTypeId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Customer Type", "Business");

                        CompanyClients companyClients = new CompanyClients();
                        companyClients.CompanyId = id;
                        companyClients.SiteId = SiteId;
                        companyClients.CustomerTypeId = customerTypeId;
                        companyClients.CreatedOnUtc = GetDateTime;
                        companyClients.CreatedById = LoggedUserId;
                        companyClients.UpdatedById = LoggedUserId;
                        companyClients.UpdatedOnUtc = GetDateTime;
                        _companyClientsService.InsertCompanyClient(companyClients);
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
        public async Task<IActionResult> DeleteCompany(string id)
        {
            var entity = await _companyService.GetById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No customer found with the specified id."));

            _companyService.DeleteCompany(entity);

            return NoContent();
        }
        #endregion

        #region GetAllCompanyListForDropdown
        [HttpGet("dropdownlist")]
        public async Task<IActionResult> GetAllCompanyListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _companyService.GetAllCompanyListForDropdown(SiteId);
                var model = _mapper.Map<List<CommonDropDown>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllCompanyListForDropdown
        [HttpGet("primaryemployeedropdownlist")]
        public async Task<IActionResult> GetAllPrimaryEmployeeListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _companyService.GetAllPrimaryEmployeeListForDropdown(SiteId);
                var model = _mapper.Map<List<CompanyModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllCompanyListForDropdown
        [HttpGet("client/dropdownlist")]
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
    }
}