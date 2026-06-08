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
using Vsky.Services.InfraAccounts;
using Vsky.Api.Models;
using Vsky.Services.DropDowns;

namespace Vsky.Api.Controllers
{
    [Route("infra-account")]
    public class InfraAccountController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IDropDownService _dropDownService;
        private readonly IInfraAccountService _infraAccountService;
        private readonly IInfraAccountServicesService _infraAccountServicesService;
        #endregion

        #region Services Initializations
        public InfraAccountController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISiteService siteService,
            ICommonService commonService,
            IDropDownService dropDownService,
            IInfraAccountService infraAccountService,
            IInfraAccountServicesService infraAccountServicesService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _siteService = siteService;
            _commonService = commonService;
            _dropDownService = dropDownService;
            _infraAccountService = infraAccountService;
            _infraAccountServicesService = infraAccountServicesService;
        }
        #endregion

        #region GetAllInfraAccountList
        // Title: GetAllInfraAccountList
        // Description: This endpoint retrieves the list of infra account.
        [HttpPost("list")]
        public IActionResult GetAllInfraAccountList(InfraAccountSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of infra account based on search criteria (name, sorting, pagination)
                var list = _infraAccountService.GetAllInfraAccountList(
                    SiteId,
                    searchModel.SearchText, 
                    searchModel.ProviderIds,
                    searchModel.InfraAccountIds,
                    searchModel.CCLast4Digits,
                    searchModel.SortBy, 
                    searchModel.Descending, 
                    searchModel.Page, 
                    searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new InfraAccountList
                {
                    InfraAccountsList = list,
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

        #region GetAllInfraAccountListForDropdown
        // Title: GetAllInfraAccountListForDropdown
        // Description: This endpoint retrieves the list of Infra Account for dropdown. 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllInfraAccountListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _infraAccountService.GetAllInfraAccountListForDropdown(SiteId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetInfraAccountDetailsById
        // Title: GetInfraAccountDetailsById
        // Description: This endpoint retrieves the details of a specific infra account based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetInfraAccountDetailsById(string id)
        {
            try
            {
                var entity = await _infraAccountService.GetInfraAccountDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra account found with the specified id."));

                var model = _mapper.Map<InfraAccount>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CheckAccountCanBeDeleted
        [HttpGet("checkAccountCanBeDeleted/{accountId}")]
        public async Task<IActionResult> CheckAccountCanBeDeleted(string accountId)
        {
            var hasActiveServices = await _infraAccountServicesService.HasActiveServices(accountId);

            return Ok(new { canDelete = !hasActiveServices });
        }
        #endregion

        #region CreateInfraAccount
        // Title: CreateInfraAccount
        // Description: This endpoint handles the creation of a new infra account. It maps the infra account model to the infra account entity, sets the creation details, and inserts the infra account into the database.
        [HttpPost]
        public async Task<IActionResult> CreateInfraAccount(SaveInfraAccount model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    //Check if the Infra Account already exists
                    //var exists = await _infraAccountService.GetInfraAccountByCustomerId(SiteId, model.CustomerId);
                    //if (exists != null)
                    //    return BadRequest(new BadRequestError("Infra account customer Id already exists, try with another."));

                    var InfraAccount = new InfraAccount
                    {
                        SiteId = SiteId,
                        Name = model.Name,
                        ProviderId = model.ProviderId,
                        WalletTypeId = model.WalletTypeId,
                        WalletNumber = model.WalletNumber,
                        URL = model.URL,
                        CustomerId = model.CustomerId,
                        CCLast4Digits = model.CCLast4Digits,
                        Instructions = model.Instructions,
                        CreatedById = LoggedUserId,
                        UpdatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                        UpdatedOnUtc = GetDateTime,
                    };
                    _infraAccountService.InsertInfraAccount(InfraAccount);

                    //Add Account Services
                    if (model.InfraAccountServicesList.Count() > 0)
                    {
                        var addList = new List<InfraAccountServices>();
                        foreach (var item in model.InfraAccountServicesList)
                        {
                            //Check if the Infra Account already exists
                            var serviceExists = await _infraAccountServicesService.GetInfraAccountServicesByName(SiteId, item.Name, item.InfraAccountId);
                            if (serviceExists != null)
                                continue;

                            var infraAccountServicesEntity = _mapper.Map<InfraAccountServices>(item);
                            infraAccountServicesEntity.InfraAccountId = InfraAccount.Id;
                            infraAccountServicesEntity.URL = !string.IsNullOrWhiteSpace(item.URL) ? item.URL : null;

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

                            infraAccountServicesEntity.CreatedById = LoggedUserId;
                            infraAccountServicesEntity.UpdatedById = LoggedUserId;
                            infraAccountServicesEntity.CreatedOnUtc = GetDateTime;
                            infraAccountServicesEntity.UpdatedOnUtc = GetDateTime;
                            addList.Add(infraAccountServicesEntity);
                        }

                        if (addList.Count > 0)
                            _infraAccountServicesService.InsertInfraAccountServicesList(addList);
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

        #region UpdateInfraAccount
        // Title: UpdateInfraAccount
        // Description: This endpoint updates an existing infra account by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInfraAccount(string id, SaveInfraAccount model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _infraAccountService.GetInfraAccountById(id);
                    // If no infra account is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No infra account found with the specified id."));

                    //Check if the Infra Account already exists
                    //var exists = await _infraAccountService.GetInfraAccountByCustomerId(SiteId, model.CustomerId, id);
                    //if (exists != null)
                    //    return BadRequest(new BadRequestError("Infra account customer Id already exists, try with another."));

                    entity.ProviderId = model.ProviderId;
                    entity.WalletTypeId = model.WalletTypeId;
                    entity.WalletNumber = model.WalletNumber;
                    entity.Name = model.Name;
                    entity.URL = model.URL;
                    entity.CustomerId = model.CustomerId;
                    entity.CCLast4Digits = model.CCLast4Digits;
                    entity.Instructions = model.Instructions;
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _infraAccountService.UpdateInfraAccount(entity);

                    var addList = new List<InfraAccountServices>();

                    var deleteList = new List<InfraAccountServices>();

                    var updateList = new List<InfraAccountServices>();

                    if (model.InfraAccountServicesList.Count() > 0)
                    {
                        foreach (var item in model.InfraAccountServicesList)
                        {
                            //Check if the Infra Account already exists
                            var serviceExists = await _infraAccountServicesService.GetInfraAccountServicesByName(SiteId, item.Name, id, item.Id);
                            if (serviceExists != null)
                                continue;

                            var existinginfraAccountServices = await _infraAccountServicesService.GetInfraAccountServicesById(item.Id);
                            DateTime? startDate = null;
                            DateTime? endDate = null;

                            // Calculate StartDate & EndDate
                            if (!string.IsNullOrEmpty(item.StartDateStr))
                            {
                                startDate = DateTime.ParseExact(item.StartDateStr, "MM/dd/yyyy", null);

                                var paymentTerm = await _dropDownService.GetDropDownById(item.PaymentTermId);

                                if (!string.IsNullOrEmpty(paymentTerm?.DropDownValue))
                                {
                                    switch (paymentTerm.DropDownValue.ToLower())
                                    {
                                        case "monthly":
                                            endDate = startDate.Value.AddMonths(1);
                                            break;

                                        case "yearly":
                                            endDate = startDate.Value.AddYears(1);
                                            break;

                                        case "one-time":
                                            endDate = startDate;
                                            break;
                                    }
                                }
                            }
                            if (item.Flag == "Edit")
                            {
                                if (existinginfraAccountServices == null)
                                    continue;

                                existinginfraAccountServices.ItemTypeId = item.ItemTypeId;
                                existinginfraAccountServices.OwnerShipTypeId = item.OwnerShipTypeId;
                                existinginfraAccountServices.Name = item.Name;
                                existinginfraAccountServices.URL = !string.IsNullOrWhiteSpace(item.URL) ? item.URL : null;
                                existinginfraAccountServices.PaymentTermId = item.PaymentTermId;
                                existinginfraAccountServices.PriceInDollar = item.PriceInDollar;
                                existinginfraAccountServices.WalletTypeId = item.WalletTypeId;
                                existinginfraAccountServices.WalletNumber = item.WalletNumber;
                                existinginfraAccountServices.Instructions = item.Instructions;
                                if (startDate.HasValue)
                                {
                                    existinginfraAccountServices.StartDate = startDate.Value;
                                }

                                if (endDate.HasValue)
                                {
                                    existinginfraAccountServices.EndDate = endDate.Value;
                                }

                                existinginfraAccountServices.UpdatedOnUtc = GetDateTime;
                                existinginfraAccountServices.UpdatedById = LoggedUserId;
                                updateList.Add(existinginfraAccountServices);
                            }
                            else if (item.Flag == "New" && !item.Deleted)
                            {
                                var infraAccountServicesEntity = _mapper.Map<InfraAccountServices>(item);
                                infraAccountServicesEntity.InfraAccountId = id;
                                infraAccountServicesEntity.URL = !string.IsNullOrWhiteSpace(item.URL) ? item.URL : null;

                                if (startDate.HasValue)
                                {
                                    infraAccountServicesEntity.StartDate = startDate.Value;
                                }

                                if (endDate.HasValue)
                                {
                                    infraAccountServicesEntity.EndDate = endDate.Value;
                                }

                                infraAccountServicesEntity.CreatedById = LoggedUserId;
                                infraAccountServicesEntity.UpdatedById = LoggedUserId;
                                infraAccountServicesEntity.CreatedOnUtc = GetDateTime;
                                infraAccountServicesEntity.UpdatedOnUtc = GetDateTime;
                                addList.Add(infraAccountServicesEntity);
                            }
                            else if (item.Flag == "Delete")
                            {
                                if (existinginfraAccountServices == null)
                                    continue;

                                deleteList.Add(existinginfraAccountServices);
                            }
                        }
                    }

                    if (addList.Count > 0)
                        _infraAccountServicesService.InsertInfraAccountServicesList(addList);

                    if (updateList.Count > 0)
                        _infraAccountServicesService.UpdateInfraAccountServicesList(updateList);

                    if (deleteList.Count > 0)
                        _infraAccountServicesService.DeleteInfraAccountServicesList(deleteList);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }
        #endregion

        #region DeleteInfraAccount
        // Title: DeleteInfraAccount
        // Description: This endpoint deletes a infra account based on the provided infra account ID. It first retrieves the infra account entity by ID, checks if it exists, and if so, deletes the infra account. If the infra account is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInfraAccount(string id)
        {
            try
            {
                // Fetch the infra account entity by its ID
                var entity = await _infraAccountService.GetInfraAccountById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra account found with the specified id."));

                _infraAccountService.DeleteInfraAccount(entity);

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
