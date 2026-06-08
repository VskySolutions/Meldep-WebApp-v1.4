using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using ExcelDataReader.Log;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.Companies;
using Vsky.Services.Employees;
using Vsky.Services.Persons;
using Vsky.Services.Projects;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("person")]
    public class PersonController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly IPersonService _personService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IEmployeeService _employeeService;
        private readonly ICompanyClientsService _companyClientsService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations
        public PersonController(
            GlobalVariable globalVariable,
            IMapper mapper,
            IPersonService personService,
            ICommonService commonService,
            ISiteService siteService,
            IEmployeeService employeeService,
            ICompanyClientsService companyClients,
            IAzureBlobImageServices azureBlobImageServices
            )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _personService = personService;
            _commonService = commonService;
            _siteService = siteService;
            _employeeService = employeeService;
            _companyClientsService = companyClients;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllPerson
        // Title: Get All Persons
        // Description: This endpoint fetches a list of persons based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllPerson(PersonSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                 var SiteId = _globalVariable.SiteId;
                // Fetch a list of persons based on search criteria (name, sorting, pagination)
                var list = _personService.GetAllPerson(
                    SiteId,
                    searchModel.SearchText,
                    searchModel.FirstName,
                    searchModel.LastName,
                    searchModel.PrimaryPhoneNumber,
                    searchModel.FromDate,
                    searchModel.ToDate,
                    searchModel.CountryId,
                    searchModel.StateProvinceId,
                    searchModel.City,
                    searchModel.SortBy, 
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                // Map the fetched list to a model suitable for the response
                var model = new PersonListModel
                {
                    Data = _mapper.Map<IList<PersonModel>>(list),
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

        #region GetAllPersonListForDropdown
        // Title: GetAllPersonListForDropdown
        // Description: This endpoint retrieves the details of a specific person based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllPersonListForDropdown(string siteId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
            var list = await _personService.GetAllPersonListForDropdown(SiteId);
            var model = _mapper.Map<List<PersonModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetAllPersonPrimaryEmailAddressListForDropdown
        [HttpGet("dropdown/primaryEmailAddressList")]
        public async Task<IActionResult> GetAllPersonPrimaryEmailAddressListForDropdown(string siteId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
            var list = await _personService.GetAllPersonPrimaryEmailAddressListForDropdown(SiteId);
            var model = _mapper.Map<List<PersonModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetAllIsSharedPersonListForDropdown
        [HttpGet("dropdown/isSharedPersonList")]
        public async Task<IActionResult> GetAllIsSharedPersonListForDropdown(string siteId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = siteId == "undefined" ? _globalVariable.SiteId : siteId;
            var list = await _personService.GetAllIsSharedPersonListForDropdown(SiteId);
            var model = _mapper.Map<List<PersonModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetPersonById
        // Title: GetPersonById
        // Description: This endpoint retrieves the details of a specific person based on its unique identifier (ID). 
        [HttpGet("{id}")]       
        public async Task<IActionResult> GetPersonById(string id)
        {
            try
            {
                // Fetch the person entity by its ID from the service
                var entity = await _personService.GetPersonDetailsById(id);
                // If the person entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No person found with the specified id."));

                // Map the person entity to a PersonModel object
                var model = _mapper.Map<PersonModel>(entity);

                //Fetch employee code from the employee table
                var employee = _employeeService.GetEmployeeDetailsByPersonId(id);
                int newEmployeeCode = 0;
                if (employee.Result == null)
                {
                    var EmployeeCode = await _employeeService.GetEmployeeCode();
                    newEmployeeCode = EmployeeCode != null ? Convert.ToInt32(EmployeeCode.EmployeeCode) + 1 : 0;
                    model.EmployeeCode = newEmployeeCode.ToString();
                }
                else
                {
                    // Assign the new EmployeeCode to the model
                    model.EmployeeCode = employee.Result.EmployeeCode;
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreatePerson
        // Title: CreatePerson
        // Description: This endpoint handles the creation of a new person. It first checks if a person with the same name already exists for the specified customer. If not, it maps the person model to the person entity, sets the creation details, and inserts the person into the database.
        [HttpPost]
        public async Task<IActionResult> CreatePerson([FromForm] PersonModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    string SiteId = null;
                    Site SiteData = null;

                    if (!model.PersonSiteFlag)
                    {
                        SiteId = string.IsNullOrWhiteSpace(model.SiteId) ? _globalVariable.SiteId : model.SiteId;
                        SiteData = await _siteService.GetById(SiteId);
                    }

                    //var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData != null ? SiteData.TimeZone : null);

                    //Check if the person already exists
                    var exists = await _personService.GetPersonByEmail(model.PrimaryEmailAddress, model.Id, SiteId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Email address already exists, Please try with another."));

                    // Map the person model to the person entity
                    var entity = _mapper.Map<Person>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    if (model.PersonPic != null && model.PersonPic.Length > 0)
                    {
                        var file = model.PersonPic;
                        var originalFileName = Path.GetFileName(file.FileName);
                        var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                        var files = new List<IFormFile> { model.PersonPic };
                        var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData != null ? SiteData.Name : "Default", "profile", files, entity.Id);

                        if (urls != null && urls.Any())
                        {
                            foreach (var blobUrl in urls)
                            {
                                var picture = new Picture
                                {
                                    SeoFilename = originalFileName,
                                    MimeType = mimeType,
                                    VirtualPath = blobUrl,
                                    ModuleId = entity.Id,
                                    Module = $"{model.FirstName} {model.LastName}",
                                    SubModuleId = entity.Id,
                                    Sub_Module = $"{model.FirstName} {model.LastName}",
                                    Type = "Person",
                                    SiteId = SiteId,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime
                                };

                                _commonService.InsertPicture(picture);

                                entity.PictureId = picture.Id;
                            }
                        }
                    }

                    //Add/Update Address
                    string AddressId = _commonService.AddUpdateAddress(entity.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, LoggedUserId);

                    // Set the created by and created on properties
                    if (model.IdentifiedDateStr != "" && model.IdentifiedDateStr != null)
                        entity.IdentifiedDate = DateTime.ParseExact(model.IdentifiedDateStr, "MM/dd/yyyy", null);

                    entity.AddressId = AddressId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _personService.InsertPerson(entity);

                    if (!model.PersonSiteFlag)
                    {
                        // Map the site to person
                        var sites = new PersonSitesMapping();
                        sites.PersonId = entity.Id;
                        sites.SiteId = SiteId;
                        sites.CreatedById = LoggedUserId;
                        sites.UpdatedById = LoggedUserId;
                        sites.CreatedOnUtc = GetDateTime;
                        sites.UpdatedOnUtc = GetDateTime;
                        _personService.InsertPersonSites(sites);
                    }

                    if (model.IsCustomer)
                    {
                        CompanyClients companyCustomer = new CompanyClients();
                        companyCustomer.PersonId = entity.Id;
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

                    return Ok(entity.Id);
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

        #region UpdatePerson
        // Title: UpdatePerson
        // Description: This endpoint updates an existing person by its ID. It validates the person model, checks for duplicate person names within the same customer, updates the person's details.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePerson(string id, [FromForm] PersonModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Check if there is any person with the same email.
                    var exists = await _personService.GetPersonByEmail(model.PrimaryEmailAddress, id, SiteId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Email address already exists, Please try with another."));

                    // Fetch the person entity by its ID
                    var entity = await _personService.GetPersonById(id);
                    if (entity == null) // If no person is not found with the given ID, return a bad request with an error message
                        return BadRequest(new BadRequestError("No person found with the specified id."));

                    //Add/Update Address
                    string AddressId = _commonService.AddUpdateAddress(entity.AddressId, model.City, model.StateProvinceId, model.CountryId, model.AddressLine1, model.AddressLine2, model.ZipCode, LoggedUserId);

                    //Upload profile picture
                    var PersonFileId = "";
                    if (model.PersonChangeFlag == "edit")
                    {
                        // Delete old image first
                        if (!string.IsNullOrEmpty(entity.PictureId))
                        {
                            var oldPicture = await _commonService.GetByPictureId(entity.PictureId);

                            if (oldPicture != null && !string.IsNullOrEmpty(oldPicture.VirtualPath))
                            {
                                await _azureBlobImageServices.DeleteImage(oldPicture.VirtualPath);

                                _commonService.DeletePicture(oldPicture);
                            }
                        }

                        if (model.PersonPic != null && model.PersonPic.Length > 0)
                        {
                            var file = model.PersonPic;
                            var originalFileName = Path.GetFileName(file.FileName);
                            var mimeType = file.ContentType.Length > 50 ? file.ContentType.Substring(0, 50) : file.ContentType;

                            var files = new List<IFormFile> { model.PersonPic };
                            var urls = await _azureBlobImageServices.UploadFilesAsync(SiteData.Name, "profile", files, entity.Id);

                            if (urls != null && urls.Any())
                            {
                                foreach (var blobUrl in urls)
                                {
                                    var picture = new Picture
                                    {
                                        SeoFilename = originalFileName,
                                        MimeType = mimeType,
                                        VirtualPath = blobUrl,
                                        ModuleId = entity.Id,
                                        Module = $"{model.FirstName} {model.LastName}",
                                        SubModuleId = entity.Id,
                                        Sub_Module = $"{model.FirstName} {model.LastName}",
                                        Type = "Person",
                                        SiteId = SiteId,
                                        CreatedById = LoggedUserId,
                                        CreatedOnUtc = GetDateTime
                                    };

                                    _commonService.InsertPicture(picture);

                                    PersonFileId = picture.Id;
                                }
                            }
                        }
                    }
                    else if (model.PersonChangeFlag == "remove")
                    {
                        // Remove the logo file
                        if (!string.IsNullOrEmpty(entity.PictureId))
                        {
                            var oldPicture = await _commonService.GetByPictureId(entity.PictureId);

                            if (oldPicture != null && !string.IsNullOrEmpty(oldPicture.VirtualPath))
                            {
                                await _azureBlobImageServices.DeleteImage(oldPicture.VirtualPath);

                                _commonService.DeletePicture(oldPicture);
                            }
                        }

                        entity.PictureId = null;
                    }

                    //Update Entry
                    if (PersonFileId != "")
                        entity.PictureId = PersonFileId;
                    entity.AddressId = AddressId;
                    entity.FirstName = model.FirstName;
                    entity.MiddleName = model.MiddleName;
                    entity.LastName = model.LastName;
                    entity.GenderId = model.GenderId;
                    entity.DOB = model.DOB;
                    entity.AddressTypeId = model.AddressTypeId;
                    entity.PrimaryEmailAddress = model.PrimaryEmailAddress;
                    entity.PrimaryPhoneNumber = model.PrimaryPhoneNumber;
                    entity.IdentifiedById = model.IdentifiedById;
                    if (model.IdentifiedDateStr != "" && model.IdentifiedDateStr != null)
                        entity.IdentifiedDate = DateTime.ParseExact(model.IdentifiedDateStr, "MM/dd/yyyy", null);
                    else
                    {
                        entity.IdentifiedDate = null;
                    }
                    entity.IdentificationNote = model.IdentificationNote;
                    entity.Relation = model.Relation;
                    entity.RelationFullName = model.RelationFullName;
                    entity.PhoneNumber = model.PhoneNumber;
                    entity.Title = model.Title;
                    entity.ProfileLink = model.ProfileLink;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _personService.UpdatePerson(entity);

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

                    return Ok(id);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeletePerson
        // Title: DeletePersonById
        // Description: This endpoint deletes a person based on the provided person ID. It first retrieves the person entity by ID, checks if it exists, and if so, deletes the person. If the person is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(string id)
        {
            try
            {
                // Fetch the person entity by its ID
                var entity = await _personService.GetPersonById(id);
                // If no person is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No person found with the specified id."));

                // Delete the person using the person service
                _personService.DeletePerson(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region convert person to customer
        [HttpPut("convertPersonToCustomer/{id}")]
        public async Task<IActionResult> ConvertPersonToCustomer(string id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the issue entity by its personId
                    var entity = await _companyClientsService.GetByPersonId(id);
                    if (entity != null)
                        return BadRequest(new BadRequestError("Customer already exists."));

                    var customerTypeId = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Customer Type", "Individual");

                    CompanyClients companyClients = new CompanyClients();
                    companyClients.PersonId = id;
                    companyClients.SiteId = SiteId;
                    companyClients.CustomerTypeId = customerTypeId;
                    companyClients.CreatedOnUtc = GetDateTime;
                    companyClients.CreatedById = LoggedUserId;
                    companyClients.UpdatedById = LoggedUserId;
                    companyClients.UpdatedOnUtc = GetDateTime;
                    _companyClientsService.InsertCompanyClient(companyClients);

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
    }
}