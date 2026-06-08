using System.Threading.Tasks;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Models;
using Vsky.Services.Sites;
using Vsky.Services.Common;
using System.Linq;
using Vsky.Api.ApiErrors;
using System.Collections.Generic;
using Vsky.Api.Models;
using AngleSharp.Dom;
using Vsky.Services.InfraAccounts;
using Vsky.Services.DropDowns;

namespace Vsky.Api.Controllers
{
    [Route("infra-account-services")]
    public class InfraAccountServicesServicesController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IDropDownService _dropDownService;
        private readonly IInfraAccountServicesService _infraAccountServicesService;
        private readonly IInfraProjectServicesService _infraProjectServicesService;
        private readonly IInfraAccountServiceCalculationService _infraAccountServiceCalculationService;
        #endregion

        #region Services Initializations
        public InfraAccountServicesServicesController(
            GlobalVariable globalVariable,
             IMapper mapper,
             ISiteService siteService,
             ICommonService commonService,
             IDropDownService dropDownService,
             IInfraAccountServicesService infraAccountServicesService,
             IInfraProjectServicesService infraProjectServicesService,
             IInfraAccountServiceCalculationService infraAccountServiceCalculationService
            )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _siteService = siteService;
            _commonService = commonService;
            _dropDownService = dropDownService;
            _infraAccountServicesService = infraAccountServicesService;
            _infraProjectServicesService = infraProjectServicesService;
            _infraAccountServiceCalculationService = infraAccountServiceCalculationService;
        }
        #endregion

        #region GetAllInfraAccountServicesList
        // Title: GetAllInfraAccountServicesList
        // Description: This endpoint retrieves the list of infra account.
        [HttpPost("list")]
        public IActionResult GetAllInfraAccountServicesList(InfraAccountServicesSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of infra account services based on search criteria (name, sorting, pagination)
                var list = _infraAccountServicesService.GetAllInfraAccountServicesList(
                    SiteId,
                    searchModel.ProjectIds,
                    searchModel.ItemTypeIds,
                    searchModel.InfraAccountIds,
                    searchModel.OwnerShipTypeIds,
                    searchModel.PaymentTermIds,
                    searchModel.SearchText,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new InfraAccountServiceList
                {
                    InfraAccountServicesList = list,
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

        #region GetAllInfraServiceListForDropdown
        // Title: GetAllInfraServiceListForDropdown
        // Description: This endpoint retrieves the list of Infra Account Services based on siteid. 
        [HttpGet("dropdown/list/{accountId}")]
        public async Task<IActionResult> GetAllInfraServiceListForDropdown(string accountId)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _infraAccountServicesService.GetAllInfraServiceListForDropdown(SiteId, accountId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetInfraAccountServicesInDetailById
        // Title: GetInfraAccountServicesInDetailById
        // Description: This endpoint retrieves the details of a specific infra account service based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetInfraAccountServicesInDetailById(string id)
        {
            try
            {
                var entity = await _infraAccountServicesService.GetInfraAccountServicesInDetailById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra account service found with the specified id."));

                var model = _mapper.Map<InfraAccountServices>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CheckAccountServiceCanBeDeleted
        [HttpGet("checkAccountServiceCanBeDeleted/{serviceId}")]
        public async Task<IActionResult> CheckAccountServiceCanBeDeleted(string serviceId)
        {
            var hasActiveProjects = await _infraProjectServicesService.HasActiveInfraProjects(serviceId);

            return Ok(new { canDelete = !hasActiveProjects });
        }
        #endregion

        #region CreateInfraAccountServices
        // Title: CreateInfraAccountServices
        // Description: This endpoint handles the creation of a new infra account services. It maps the infra account services model to the infra account services entity, sets the creation details, and inserts the infra account services into the database.
        [HttpPost]
        public async Task<IActionResult> CreateInfraAccountServices(SaveInfraAccountServicesList model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Add Account Services
                    if (model.InfraAccountServicesLines.Count() > 0)
                    {
                        var addList = new List<InfraAccountServices>();
                        foreach (var item in model.InfraAccountServicesLines)
                        {
                            //Check if the Infra Account already exists
                            var exists = await _infraAccountServicesService.GetInfraAccountServicesByName(SiteId, item.Name, item.InfraAccountId);
                            if (exists != null)
                                continue;

                            var infraAccountServicesEntity = new InfraAccountServices
                            {
                                InfraAccountId = model.InfraAccountId,
                                ItemTypeId = item.ItemTypeId,
                                OwnerShipTypeId = item.OwnerShipTypeId,
                                Name = item.Name,
                                URL = !string.IsNullOrWhiteSpace(item.URL) ? item.URL : null,
                                PaymentTermId = item.PaymentTermId,
                                PriceInDollar = item.PriceInDollar,
                                WalletTypeId = item.WalletTypeId,
                                WalletNumber = item.WalletNumber,
                                Instructions = item.Instructions,
                                CreatedById = LoggedUserId,
                                CreatedOnUtc = GetDateTime,
                                UpdatedById = LoggedUserId,
                                UpdatedOnUtc = GetDateTime
                            };


                            // Calculate StartDate & EndDate
                            if (!string.IsNullOrEmpty(item.StartDateStr))
                            {
                                infraAccountServicesEntity.StartDate = DateTime.ParseExact(item.StartDateStr, "MM/dd/yyyy", null);

                                var paymentTerm = await _dropDownService.GetDropDownById(item.PaymentTermId);

                                if (!string.IsNullOrEmpty(paymentTerm?.DropDownValue))
                                {
                                    switch (paymentTerm.DropDownValue.ToLower())
                                    {
                                        case "monthly":
                                            infraAccountServicesEntity.EndDate = infraAccountServicesEntity.StartDate.AddMonths(1);
                                            break;

                                        case "yearly":
                                            infraAccountServicesEntity.EndDate = infraAccountServicesEntity.StartDate.AddYears(1);
                                            break;

                                        case "one-time":
                                            infraAccountServicesEntity.EndDate = infraAccountServicesEntity.StartDate;
                                            break;
                                    }
                                }
                            }

                            addList.Add(infraAccountServicesEntity);
                        }

                        if (addList.Count > 0)
                        {
                            // Insert services
                            _infraAccountServicesService.InsertInfraAccountServicesList(addList);

                            var priceHistoryList = new List<InfraAccountServicesPriceHistory>();
                            foreach (var servicePriceHistory in addList)
                            {
                                var infraAccountServicesPriceHistory = new InfraAccountServicesPriceHistory
                                {
                                    InfraAccountServiceId = servicePriceHistory.Id,
                                    Price = servicePriceHistory.PriceInDollar,
                                    StartDate = servicePriceHistory.StartDate,
                                    CreatedOnUtc = GetDateTime,
                                    CreatedById = LoggedUserId,
                                    UpdatedById = LoggedUserId,
                                    UpdatedOnUtc = GetDateTime,
                                };
                                priceHistoryList.Add(infraAccountServicesPriceHistory);
                            }
                            _infraAccountServiceCalculationService.InsertInfraAccountServicesPriceHistoryList(priceHistoryList);
                        }

                    }

                    return Ok();
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region UpdateInfraAccountServices
        // Title: UpdateInfraAccountServices
        // Description: This endpoint updates an existing infra account services by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInfraAccountServices(string id, SaveInfraAccountServices model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _infraAccountServicesService.GetInfraAccountServicesById(id);
                    // If no infra account services is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No infra account service found with the specified id."));

                    //Check if the Infra Account Service already exists
                    var exists = await _infraAccountServicesService.GetInfraAccountServicesByName(SiteId, model.Name, model.InfraAccountId, id);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Name already exists, try with another."));

                    entity.InfraAccountId = model.InfraAccountId;
                    entity.InfraAccountServiceId = (!string.IsNullOrWhiteSpace(model.InfraAccountServiceId) ? model.InfraAccountServiceId : null);
                    entity.ItemTypeId = model.ItemTypeId;
                    entity.WalletTypeId = model.WalletTypeId;
                    entity.WalletNumber = model.WalletNumber;
                    entity.Name = model.Name;
                    entity.URL = !string.IsNullOrWhiteSpace(model.URL) ? model.URL : null;
                    entity.OwnerShipTypeId = model.OwnerShipTypeId;
                    entity.PaymentTermId = model.PaymentTermId;
                    entity.PriceInDollar = model.PriceInDollar;

                    // Calculate StartDate & EndDate
                    if (!string.IsNullOrEmpty(model.StartDateStr))
                    {
                        entity.StartDate = DateTime.ParseExact(model.StartDateStr, "MM/dd/yyyy", null);

                        var paymentTerm = await _dropDownService.GetDropDownById(model.PaymentTermId);

                        if (!string.IsNullOrEmpty(paymentTerm?.DropDownValue))
                        {
                            switch (paymentTerm.DropDownValue.ToLower())
                            {
                                case "monthly":
                                    entity.EndDate = entity.StartDate.AddMonths(1);
                                    break;

                                case "yearly":
                                    entity.EndDate = entity.StartDate.AddYears(1);
                                    break;

                                case "one-time":
                                    entity.EndDate = entity.StartDate;
                                    break;
                            }
                        }
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _infraAccountServicesService.UpdateInfraAccountServices(entity);

                    var existingPriceHistory = await _infraAccountServiceCalculationService.GetInfraAccountServicesPriceHistoryByAccountServiceId(id);
                    if (existingPriceHistory == null)
                        return BadRequest(new BadRequestError("No infra account service price history found for the specified account service"));

                    existingPriceHistory.Price = model.PriceInDollar;
                    existingPriceHistory.StartDate = entity.StartDate;
                    existingPriceHistory.UpdatedById = LoggedUserId;
                    existingPriceHistory.UpdatedOnUtc = GetDateTime;

                    _infraAccountServiceCalculationService.UpdateInfraAccountServicesPriceHistory(existingPriceHistory);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region AddorUpdateInstructions
        //created for update Instructions from list page
        [HttpPut("instructions/{id}")]
        public async Task<IActionResult> AddorUpdateInstructions(string id, SaveInfraAccountServices model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the infra account service entity by its ID
                    var entity = await _infraAccountServicesService.GetInfraAccountServicesById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Infra account found with the specified id."));

                    if (model.IsInstruction)
                    {
                        entity.Instructions = model.Instructions;
                    }
                    else
                    {
                        entity.WalletTypeId = model.WalletTypeId;
                        entity.WalletNumber = model.WalletNumber;
                    }
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _infraAccountServicesService.UpdateInfraAccountServices(entity);

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

        #region InfraServiceAssignToProject
        //created for update Instructions from list page
        [HttpPost("assign-to-project")]
        public async Task<IActionResult> InfraServiceAssignToProject(string id, string projectId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the infra account service entity by its ID
                    var entity = await _infraAccountServicesService.GetInfraAccountServicesById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Infra account found with the specified id."));

                    var InfraProjectServices = new InfraProjectServices
                    {
                        InfraServiceId = id,
                        InfraProjectId = projectId,
                        CreatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                    };
                    _infraProjectServicesService.InsertInfraProjectServices(InfraProjectServices);

                    return Ok(InfraProjectServices.Id);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteInfraAccountServices
        // Title: DeleteInfraAccountServices
        // Description: This endpoint deletes a infra account services based on the provided infra account services ID. It first retrieves the infra account services entity by ID, checks if it exists, and if so, deletes the infra account services. If the infra account services is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInfraAccountServices(string id)
        {
            try
            {
                // Fetch the infra account entity by its ID
                var entity = await _infraAccountServicesService.GetInfraAccountServicesById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra account service found with the specified id."));

                _infraAccountServicesService.DeleteInfraAccountServices(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteInfraProjectServices
        // Title: DeleteInfraProjectServices
        // Description: This endpoint deletes a infra account Project services based on the provided infra account Project services ID. It first retrieves the infra account Project services entity by ID, checks if it exists, and if so, deletes the infra account Project services. If the infra account Project services is not found, it returns a BadRequest response with an error message.
        [HttpDelete("assignProject/{id}")]
        public async Task<IActionResult> DeleteInfraProjectServices(string id)
        {
            try
            {
                // Fetch the infra account entity by its ID
                var entity = await _infraProjectServicesService.GetInfraAccountProjectServicesById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra project service found with the specified id."));

                _infraProjectServicesService.DeleteInfraProjectServices(entity);

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
