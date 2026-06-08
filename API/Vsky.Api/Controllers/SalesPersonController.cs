using System;
using System.Collections.Generic;
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
using Vsky.Services.Common;
using Vsky.Services.Companies;
using Vsky.Services.SalesPersons;
using Vsky.Services.Sites;
using static Vsky.Api.Models.SalesPersonModel;

namespace Vsky.Api.Controllers
{
    [Route("salesperson")]
    public class SalesPersonController : BaseController
    {
        #region Services Initialization
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISalesPersonService _salesPersonService;
        private readonly ApplicationDbContext _db;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        public SalesPersonController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISalesPersonService salesPersonService, 
            ISiteService siteService,
            ApplicationDbContext db, 
            ICommonService commonService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _salesPersonService = salesPersonService;
            _siteService = siteService;
            _db = db;
            _commonService = commonService;
        }
        #endregion

        #region GetAllSalesPersons
        [HttpPost("list")]
        public IActionResult GetAllSalesPersons([FromQuery] SalesPersonSearchModel searchModel)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = _salesPersonService.GetAllSalesPersons(SiteId, searchModel.Name, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

            var model = new SalesPersonListModel
            {
                Data = _mapper.Map<IList<SalesPersonModel>>(list),
                Total = list.TotalCount
            };

            return Ok(model);
        }
        #endregion

        #region GetSalesPersonById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSalesPersonById(string id)
        {
            var entity = await _salesPersonService.GetById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No sales person found with the specified id."));

            var model = _mapper.Map<SalesPersonModel>(entity);
            return Ok(model);
        }
        #endregion

        #region GetSalesPersonDetailsById
        [HttpGet("{id}/salespersondetails")]
        public async Task<IActionResult> GetSalesPersonDetailsById(string id)
        {
            var entity = await _salesPersonService.GetSalesPersonDetailsById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No person found with the specified id."));

            var model = _mapper.Map<SalesPersonModel>(entity);
            return Ok(model);
        }
        #endregion

        #region CreateCompany
        [HttpPost]
        public async Task<IActionResult> CreateSalesPerson(SalesPersonModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var exists = _db.Company.Any(x => x.Id == model.Id && !x.Deleted);
                    if (exists)
                        return BadRequest(new BadRequestError("Sales Person already exists."));

                    var entity = _mapper.Map<SalesPerson>(model);
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _salesPersonService.InsertSalesPerson(entity);

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
            }
            return ModelStateError(ModelState);
        }
        #endregion
    }
}
