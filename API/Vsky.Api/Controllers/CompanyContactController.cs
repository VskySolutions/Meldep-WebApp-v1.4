using System;
using System.Collections.Generic;
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
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("company-contact")]
    public class CompanyContactController : BaseController
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
        public CompanyContactController(
            IMapper mapper,
            GlobalVariable globalVariable,
            ICompanyService companyService,
            ApplicationDbContext db,
            ICompanyContactsService companyContactsService,
            ICommonService commonService, ISiteService siteService)
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _companyService = companyService;
            _companyContactService = companyContactsService;
            _commonService = commonService;
            _siteService = siteService;
            _db = db;
        }
        #endregion

        #region GetAllContactList
        [HttpPost("list")]
        public IActionResult GetAllContactList(CompanyContactSearchModel searchModel)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = _companyContactService.GetAllContactList(SiteId, searchModel.SearchText, searchModel.CompanyId, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
            var model = new CompanyContactListModel
            {
                Data = _mapper.Map<IList<CompanyContactsModels>>(list),
                Total = list.TotalCount
            };

            return Ok(model);
        }
        #endregion

        #region GetCompanyById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompanyContactById(string id)
        {
            var entity = await _companyContactService.GetById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No company found with the specified id."));

            var model = _mapper.Map<CompanyModel>(entity);
            return Ok(model);
        }
        #endregion

        #region GetcompanydetailsById
        [HttpGet("{id}/companycontactdetails")]
        public async Task<IActionResult> GetCompanyContactdetailsById(string id)
        {
            var entity = await _companyContactService.GetCompanyContactdetailsById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No company found with the specified id."));

            var model = _mapper.Map<CompanyContactsModels>(entity);
            return Ok(model);
        }
        #endregion

        #region CreateCompanyContact
        [HttpPost]
        public async Task<IActionResult> CreateCompanyContact(CompanyContactsModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _companyContactService.GetByPersonAndCompanyId(model.CompanyId, model.PersonId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Contact person already exists."));

                    var entity = _mapper.Map<CompanyContacts>(model);
                    //entity.SiteId = SiteId;
                    entity.PersonId = model.PersonId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _companyContactService.InsertCompanyContact(entity);

                    return Ok(entity);
                }
            }
            catch (Exception ex)
            {
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateCompanyContact
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompanyContact(string id, CompanyContactsModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = await _companyContactService.GetByPersonAndCompanyId(model.CompanyId, model.PersonId, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Contact person already exists."));

                    var entity = await _companyContactService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No company contact found with the specified id."));

                    entity.PersonId = model.PersonId;
                    entity.CompanyId = model.CompanyId;
                    entity.AlternatePhoneNumber = model.AlternatePhoneNumber;
                    entity.AlternateEmail = model.AlternateEmail;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _companyContactService.UpdateCompanyContact(entity);

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

        #region DeleteCompanyContact
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompanyContact(string id)
        {
            var entity = await _companyContactService.GetById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No customer found with the specified id."));

            _companyContactService.DeleteCompanyContact(entity);

            return NoContent();
        }
        #endregion
    }
}
