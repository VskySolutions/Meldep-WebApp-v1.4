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
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.Companies;
using Vsky.Services.Servers;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("server")]
    public class ServerController : BaseController
    {
        #region Services Initialization
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly IServerService _serverService;
        private readonly IServerPaymentsService _serverPaymentsService;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        public ServerController(
            GlobalVariable globalVariable,
            IMapper mapper,
            IServerService serverService,
            ICommonService commonService,
            IServerPaymentsService serverPaymentsService, 
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _serverService = serverService;
            _commonService = commonService;
            _serverPaymentsService = serverPaymentsService;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllServers

        [HttpPost("list")]
        public IActionResult GetAllServers(ServerSearchModel searchModel)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = _serverService.GetAllServers(SiteId, searchModel.SearchText, searchModel.ProviderId, searchModel.CustomerId, searchModel.ContractId, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

            var model = new ServerListModel
            {
                Data = _mapper.Map<IList<ServerModel>>(list),
                Total = list.TotalCount
            };

            return Ok(model);
        }
        #endregion

        #region GetAllServers
        [HttpPost("ftplist")]
        public IActionResult GetFTPList(ServerSearchModel searchModel)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = _serverService.GetFTPList(SiteId, searchModel.SearchText, searchModel.ContractId, searchModel.CustomerId, searchModel.FTPUsername, searchModel.FTPHostname, searchModel.FtpPort, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
            var model = new ServerListModel
            {
                Data = _mapper.Map<IList<ServerModel>>(list),
                Total = list.TotalCount
            };
            return Ok(model);
        }
        #endregion

        #region GetServerById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServerById(string id)
        {
            var entity = await _serverService.GetServerDetailsById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No server found with the specified id."));

            var model = _mapper.Map<ServerModel>(entity);
            return Ok(model);
        }
        #endregion

        #region GetServerDetailsById
        [HttpGet("{id}/serverdetails")]
        public async Task<IActionResult> GetServerDetailsById(string id)
        {
            var entity = await _serverService.GetServerDetailsById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No server found with the specified id."));

            var model = _mapper.Map<ServerModel>(entity);
            return Ok(model);
        }
        #endregion

        #region CreateServer
        [HttpPost]
        public async Task<IActionResult> CreateServer(ServerModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var exists = await _serverService.GetByCustomerId(model.CustomerId);
                    if (exists != null)
                        return BadRequest(new BadRequestError("Customer Id already exists."));

                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = _mapper.Map<Server>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    entity.Instructions = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Instructions,
                                SiteData.Name,
                                "infrastructure-server",
                                entity.Id
                            );

                    entity.Notes = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Notes,
                                SiteData.Name,
                                "infrastructure-server",
                                entity.Id
                            );

                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _serverService.InsertServer(entity);

                    return Ok(entity);
                }
            }
            catch (Exception ex)
            {
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateServer
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServer(string id, ServerModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _serverService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No server found with the specified id."));

                    if (model.Tab == "1_tab")
                    {
                        entity.ProviderId = model.ProviderId;
                        entity.CustomerId = model.CustomerId;
                        entity.ContractId = model.ContractId;
                        entity.Password = model.Password;
                        entity.PIN = model.PIN;
                        entity.FtpUsername = model.FtpUsername;
                        entity.FtpPassword = model.FtpPassword;
                        entity.FtpPort = model.FtpPort;
                        entity.FtpHostname = model.FtpHostname;

                        entity.Instructions = await _azureBlobImageServices
                               .ProcessHtmlAndManageImagesAsync(
                                   model.Instructions,
                                   SiteData.Name,
                                   "infrastructure-server",
                                   entity.Id,
                                   entity.Instructions
                               );

                        entity.Notes = await _azureBlobImageServices
                               .ProcessHtmlAndManageImagesAsync(
                                   model.Notes,
                                   SiteData.Name,
                                   "infrastructure-server",
                                   entity.Id,
                                   entity.Notes
                               );
                    }
                    else if (model.Tab == "2_tab")
                    {
                        if (model.ServerPaymentsModel.Count() > 0)
                        {
                            if (model.StartDateStr != null && model.StartDateStr != "")
                                entity.StartDate = Convert.ToDateTime(model.StartDateStr);
                            entity.CardDigit = model.CardDigit;
                            foreach (var item in model.ServerPaymentsModel)
                            {
                                var serverPaymentEntity = await _serverPaymentsService.GetById(item.Id);
                                if (item.Flag == "Edit")
                                {
                                    if (serverPaymentEntity == null)
                                        continue;

                                    serverPaymentEntity.ServerId = entity.Id;
                                    if (item.RenewDateStr != null && item.RenewDateStr != "")
                                        serverPaymentEntity.RenewDate = Convert.ToDateTime(item.RenewDateStr);
                                    serverPaymentEntity.Amount = item.Amount;
                                    serverPaymentEntity.Notes = item.Notes;
                                    serverPaymentEntity.UpdatedOnUtc = GetDateTime;
                                    serverPaymentEntity.UpdatedById = LoggedUserId;
                                    _serverPaymentsService.UpdateServerPayments(serverPaymentEntity);
                                }
                                else if (item.Flag == "New")
                                {
                                    if (serverPaymentEntity != null)
                                        continue;

                                    var data = _mapper.Map<ServerPayments>(item);
                                    data.ServerId = entity.Id;
                                    if (item.RenewDateStr != null && item.RenewDateStr != "")
                                        data.RenewDate = Convert.ToDateTime(item.RenewDateStr);
                                    data.CreatedOnUtc = GetDateTime;
                                    data.UpdatedOnUtc = GetDateTime;
                                    data.CreatedById = LoggedUserId;
                                    data.UpdatedById = LoggedUserId;
                                    _serverPaymentsService.InsertServerPayments(data);
                                }
                                else if (item.Flag == "Delete")
                                {
                                    if (serverPaymentEntity == null)
                                        continue;
                                    _serverPaymentsService.DeleteServerPayments(serverPaymentEntity);
                                }
                            }
                        }
                    }
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _serverService.UpdateServer(entity);
                    return Ok(entity);
                }
            }
            catch (Exception ex)
            {
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteServer
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServer(string id)
        {
            try
            {
                var entity = await _serverService.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No server found with the specified id."));

                _serverService.DeleteServer(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllServerListForDropdown
        // Title: GetAllServerListForDropdown
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllServerListForDropdown()
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _serverService.GetAllServerListForDropdown(SiteId);
            var model = _mapper.Map<List<ServerModel>>(list);
            return Ok(model);
        }
        #endregion

        #region GetAllTypeListForDropdown
        // Title: GetAllServerListForDropdown
        [HttpGet("dropdown/typelist")]
        public async Task<IActionResult> GetAllServerListForDropdown(string type)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = await _serverService.GetAllFTPListForDropdown(SiteId, type);
            var model = _mapper.Map<List<ServerModel>>(list);
            return Ok(model);
        }
        #endregion
    }
}
