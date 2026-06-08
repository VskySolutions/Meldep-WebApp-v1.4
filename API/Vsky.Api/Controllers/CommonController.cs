using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.Leads;
using Vsky.Services.Sites;
using static NodaTime.TimeZones.TzdbZone1970Location;

namespace Vsky.Api.Controllers
{
    [Route("common")]
    public class CommonController : BaseController
    {
        #region Fields
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;

        #endregion

        #region Services Initializations
        public CommonController(ICommonService commonService,            
            IMapper mapper, ISiteService siteService)
        {
            _commonService = commonService;
            _mapper = mapper;
            _siteService = siteService;
        }
        #endregion

        #region GetAllCountries
        // Title: Get All Countries
        // Description: This endpoint fetches a list of countries. 
        [HttpGet("countries")]
        public async Task<IActionResult> GetAllCountries()
        {
            var list = await _commonService.GetAllCountries();
            var model = _mapper.Map<IList<CountryModel>>(list);

            return Ok(model);
        }
        #endregion

        #region GetAllStateProvinces
        // Title: Get All state-provinces
        // Description: This endpoint fetches a list of state-provinces. 
        [HttpGet("state-provinces")]
        public async Task<IActionResult> GetAllStateProvinces(string countryId)
        {
            var list = await _commonService.GetAllStateProvinces(countryId);
            var model = _mapper.Map<IList<StateProvinceModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetStateById
        // Title: GetStateById
        // Description: This endpoint retrieves the details of a specific state based on its unique identifier (ID). 
        [HttpGet("{stateid}")]
        public async Task<IActionResult> GetStateById(string stateid)
        {
            var entity = await _commonService.GetByStateId(stateid);
            if (entity == null)
                return BadRequest(new BadRequestError("No State found with the specified id."));

            var model = _mapper.Map<StateProvinceModel>(entity);
            return Ok(model);
        }
        #endregion

        #region GetCountryid
        // Title: GetCountryid
        // Description: This endpoint retrieves the details of a specific country based on its unique identifier (ID). 
        [HttpGet("countries/{countryid}")]
        public async Task<IActionResult> GetCountryid(string countryid)
        {
            var entity = await _commonService.GetByCountryId(countryid);
            if (entity == null)
                return BadRequest(new BadRequestError("No Country found with the specified id."));

            var model = _mapper.Map<CountryModel>(entity);
            return Ok(model);
        }
        #endregion

        #region GetValidationDetailsByCountryId
        // Title: GetValidationDetailsByCountryId
        // Description: This endpoint retrieves the country code and phone number pattern for a specific country based on its unique identifier (ID).
        [HttpGet("validation-details/{countryid}")]
        public async Task<IActionResult> GetValidationDetailsByCountryId(string countryid)
        {
            var entity = await _commonService.GetValidationDetailsByCountryId(countryid);
            if (entity == null)
                return BadRequest(new BadRequestError("No Country found with the specified id."));

           var model = _mapper.Map<CustomCountry>(entity);
            return Ok(model);
        }
        #endregion

        #region
        [HttpGet("employeeId")]
        public IActionResult GetEmployeeId(string siteId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var employeeId = _commonService.GetEmployeeIdByUserId(siteId, LoggedUserId);

                return Ok(new
                {
                    success = true,
                    employeeId = employeeId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        #endregion

    }
}