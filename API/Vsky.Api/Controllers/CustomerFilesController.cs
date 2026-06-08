using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.CustomersFile;
using Vsky.Services.Sites;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Vsky.Api.ApiErrors;
using System.Numerics;

namespace Vsky.Api.Controllers
{
    [Route("customer-file")]
    public class CustomerFilesController : BaseController
    {
        #region Define service
        private readonly IMapper _mapper;
        private readonly GlobalVariable _globalVariable;
        private readonly ICustomerFilesService _customerFilesService;
        private readonly ICustomerFilesLinesService _customerFilesLinesService;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;

        public CustomerFilesController(
           IMapper mapper,
           GlobalVariable globalVariable,
           ICustomerFilesService customerFilesService,
           ICustomerFilesLinesService customerFilesLinesService,
           ISiteService siteService, ICommonService commonService)
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _customerFilesService = customerFilesService;
            _customerFilesLinesService = customerFilesLinesService;
            _siteService = siteService;
            _commonService = commonService;
        }
        #endregion

        #region Get All
        [HttpPost("list")]
        public IActionResult GetAllCustomerFiles(CustomerFilesSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";

                var list = _customerFilesService.GetAllCustomerFiles(SiteId,searchModel.note, createdBy, searchModel.CustomerId, searchModel.Year, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                // Map the fetched list to a model
                var model = new CustomerFilesListModel
                {
                    Data = _mapper.Map<IList<CustomerFilesModel>>(list),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("listVW")]
        public IActionResult GetAllCustomerFilesFromVW(CustomerFilesSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var createdBy = searchModel.CreatedBy == "Created By Me" ? LoggedUserId : "";

                var list = _customerFilesService.GetAllCustomerFilesFromVW(SiteId, searchModel.CustomerId, searchModel.Year, searchModel.Page, searchModel.PageSize);

                // Map the fetched list to a model
                var model = new VW_CustomerFilesListModel
                {
                    Data = _mapper.Map<IList<VW_CustomerFilesModel>>(list),
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

        #region
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerFileDetailsById(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var entity = await _customerFilesService.GetCustomerFileDetailsById(id);

                if (entity == null)
                    return BadRequest(new BadRequestError("No file found with the specified id."));

                var model = _mapper.Map<CustomerFilesModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetCustomerFilesDetailsByIdAndYear
        [HttpGet("get-customerfiles-by-year-id/{year}/{customerId}")]
        public async Task<IActionResult> GetCustomerFilesDetailsByIdAndYear(int year, string customerId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var entity = await _customerFilesService.GetCustomerFilesDetailsByYearAndId(SiteId, year, customerId);

                if (entity == null)
                    return BadRequest(new BadRequestError("No file found with the specified id."));

                var model = _mapper.Map<List<VW_CustomerFilesModel>>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region save files
        //Manish Dhuri
        [HttpPost]
        public async Task<IActionResult> CreateCustomerFile(CustomerFilesModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (!string.IsNullOrEmpty(model.CustomerId))
                    {
                        string CustomerFileId = "";
                        var data = await _customerFilesService.GetCustomerFilesDetailsByYearAndIdAndNote(SiteId, model.Year, model.CustomerId, model.Note);
                        if (data == null)
                        {
                            // var entity = _mapper.Map<CustomerFiles>(model);
                            var customerFileEntity = new CustomerFiles();
                            customerFileEntity.SiteId = SiteId;
                            customerFileEntity.Note = model.Note;
                            customerFileEntity.CustomerId = model.CustomerId;
                            customerFileEntity.Year = model.Year;
                            customerFileEntity.SortOrder = model.SortOrder;

                            customerFileEntity.CreatedById = LoggedUserId;
                            customerFileEntity.CreatedOnUtc = GetDateTime;
                            customerFileEntity.UpdatedById = LoggedUserId;
                            customerFileEntity.UpdatedOnUtc = GetDateTime;
                            _customerFilesService.InsertCustomerFiles(customerFileEntity);
                            CustomerFileId = customerFileEntity.Id;
                        }
                        else
                            CustomerFileId = data.Id;

                        if (model.CustomerFilesLines != null)
                        {
                            var files = new List<CustomerFilesLines>();
                            foreach (var file in model.CustomerFilesLines)
                            {
                                if(!string.IsNullOrEmpty(file.FileName))
                                {
                                    var customerFileLinesEntity = new CustomerFilesLines();
                                    customerFileLinesEntity.CustomerFileId = CustomerFileId;
                                    customerFileLinesEntity.FileName = file.FileName;
                                    customerFileLinesEntity.SortOrder = file.SortOrder;

                                    customerFileLinesEntity.CreatedById = LoggedUserId;
                                    customerFileLinesEntity.CreatedOnUtc = GetDateTime;
                                    customerFileLinesEntity.UpdatedOnUtc = GetDateTime;
                                    customerFileLinesEntity.UpdatedById = LoggedUserId;
                                    files.Add(customerFileLinesEntity);
                                }
                            }

                            if (files.Count > 0)
                                _customerFilesLinesService.InsertCustomerFilesLinesList(files);
                        }
                    }

                    return NoContent();
                }
                // Return model state errors if the model state is not valid
                return ModelStateError(ModelState);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        #region UpdateCustomerFiles
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomerFiles(string id, CustomerFilesModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _customerFilesService.GetCustomerFileDetailsById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No files found with the specified id."));

                    entity.Year = model.Year;
                    entity.CustomerId = model.CustomerId;

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;

                    if (model.CustomerFilesLines != null && model.CustomerFilesLines.Count() > 0)
                    {
                        foreach (var file in model.CustomerFilesLines)
                        {
                            var existingCustomerFileline = await _customerFilesLinesService.GetById(file.Id);
                            if (existingCustomerFileline == null)
                            {
                                var customerLineEntity = _mapper.Map<CustomerFilesLines>(file);
                                customerLineEntity.CustomerFileId = entity.Id;
                                customerLineEntity.CreatedById = LoggedUserId;
                                customerLineEntity.UpdatedById = LoggedUserId;
                                customerLineEntity.CreatedOnUtc = GetDateTime;
                                customerLineEntity.UpdatedOnUtc = GetDateTime;
                                _customerFilesLinesService.InsertCustomerFilesLines(customerLineEntity);

                            }
                            else
                            {
                                existingCustomerFileline.FileName = file.FileName;
                                existingCustomerFileline.SortOrder = file.SortOrder;
                                existingCustomerFileline.Deleted = file.Deleted;

                                existingCustomerFileline.UpdatedOnUtc = GetDateTime;
                                existingCustomerFileline.UpdatedById = LoggedUserId;
                                _customerFilesLinesService.UpdateCustomerFilesLines(existingCustomerFileline);
                            }
                        }

                    }


                    _customerFilesService.UpdateCustomerFiles(entity);

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteCustomerFiles
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomerFiles(string id)
        {
            var entity = await _customerFilesService.GetById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No daily planner found with the specified id."));

            _customerFilesService.DeleteCustomerFiles(entity);

            var existingCustomerFileLine = await _customerFilesLinesService.GetCustomerFileLinesByCustomerFileId(id);
            if (existingCustomerFileLine.Count() > 0)
            {
                foreach (var item in existingCustomerFileLine)
                {
                    var existingLine = await _customerFilesLinesService.GetById(item.Id);
                    existingLine.CustomerFileId = id;
                    _customerFilesLinesService.DeleteCustomerFilesLines(existingLine);
                }
            }
            return NoContent();
        }
        #endregion
        #endregion
    }
}
