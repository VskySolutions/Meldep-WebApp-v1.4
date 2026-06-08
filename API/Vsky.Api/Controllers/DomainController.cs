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
using Vsky.Services.Domains;
using Vsky.Services.Servers;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("domain")]
    public class DomainController : BaseController
    {
        #region Services Initialization
        private readonly IMapper _mapper;
        private readonly GlobalVariable _globalVariable;
        private readonly ICommonService _commonService;
        private readonly IDomainService _domainService;
        private readonly IDomainAttributeService _domainAttributeService;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;

        public DomainController(
            IMapper mapper,
            GlobalVariable globalVariable,
            ICommonService commonService,
            IDomainService domainService,
            IDomainAttributeService domainAttributeService,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _mapper = mapper;
            _globalVariable = globalVariable;
            _commonService = commonService;
            _domainService = domainService;
            _domainAttributeService = domainAttributeService;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region GetAllDomains
        [HttpPost("list")]
        public IActionResult GetAllDomains(DomainSearchModel searchModel)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;
            var list = _domainService.GetAllDomains(SiteId, searchModel.SearchText, searchModel.ProjectIds, searchModel.Url, searchModel.DomainTypeIds, searchModel.DomainServerIds, searchModel.HostingServerIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
            var model = new DomainListModel
            {
                Data = _mapper.Map<IList<DomainModel>>(list),
                Total = list.TotalCount
            };

            return Ok(model);
        }
        #endregion

        #region GetDomainById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDomainById(string id)
        {
            var entity = await _domainService.GetDomainDetailsById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No domain found with the specified id."));

            var model = _mapper.Map<DomainModel>(entity);
            return Ok(model);
        }
        #endregion

        #region GetDomainDetailsById
        [HttpGet("domaindetails/{id}")]
        public async Task<IActionResult> GetDomainDetailsById(string id)
        {
            var entity = await _domainService.GetDomainDetailsById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No domain found with the specified id."));

            var model = _mapper.Map<DomainModel>(entity);
            return Ok(model);
        }
        #endregion

        #region CreateDomain
        [HttpPost]
        public async Task<IActionResult> CreateDomain(DomainModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = _mapper.Map<Domain>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    entity.Notes = await _azureBlobImageServices
                             .ProcessHtmlAndManageImagesAsync(
                                 model.Notes,
                                 SiteData.Name,
                                 "infrastructure-domain",
                                 entity.Id
                             );

                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _domainService.InsertDomain(entity);

                    return Ok(entity);
                }
            }
            catch (Exception ex)
            {
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateDomain
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDomain(string id, DomainModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var entity = await _domainService.GetById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No domain found with the specified id."));

                    if (model.Tab == "1_tab")
                    {
                        //Domain Info
                        entity.ProjectId = model.ProjectId;
                        entity.Url = model.Url;
                        entity.DomainTypeId = model.DomainTypeId;
                        entity.DomainServerId = model.DomainServerId;
                        entity.HostingServerId = model.HostingServerId;
                        entity.DomainMappingId = model.DomainMappingId;
                        entity.ExternalMappingNote = model.ExternalMappingNote;

                        entity.Notes = await _azureBlobImageServices
                                .ProcessHtmlAndManageImagesAsync(
                                    model.Notes,
                                    SiteData.Name,
                                    "infrastructure-domain",
                                    entity.Id,
                                    entity.Notes
                                );

                        //Database Info
                        entity.DatabaseUsername = model.DatabaseUsername;
                        entity.DatabaseName = model.DatabaseName;
                        entity.DatabasePassword = model.DatabasePassword;
                        entity.DatabaseHostname = model.DatabaseHostname;
                    }
                    else if (model.Tab == "2_tab")
                    {
                        //FTP Details
                        entity.FtpUsername = model.FtpUsername;
                        entity.FtpPassword = model.FtpPassword;
                        entity.FtpPort = model.FtpPort;
                        entity.FtpHostname = model.FtpHostname;

                        //Website Info
                        entity.WebsiteLoginUrl = model.WebsiteLoginUrl;
                        entity.WebsiteLoginId = model.WebsiteLoginId;
                        entity.WebsiteLoginPassword = model.WebsiteLoginPassword;

                        entity.Instructions = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Instructions,
                                SiteData.Name,
                                "infrastructure-domain",
                                entity.Id,
                                entity.Instructions
                            );

                        entity.Notes = await _azureBlobImageServices
                                .ProcessHtmlAndManageImagesAsync(
                                    model.Notes,
                                    SiteData.Name,
                                    "infrastructure-domain",
                                    entity.Id,
                                    entity.Notes
                                );
                    }
                    else if (model.Tab == "3_tab")
                    {
                        if (model.DomainAttributesModel.Count() > 0)
                        {
                            foreach (var item in model.DomainAttributesModel)
                            {
                                var domainAttributeEntity = await _domainAttributeService.GetById(item.Id);
                                if (item.Flag == "Edit")
                                {
                                    if (domainAttributeEntity == null)
                                        continue;

                                    domainAttributeEntity.DomainId = entity.Id;
                                    domainAttributeEntity.DomainAttributeId = item.DomainAttributeId;
                                    if (item.StartDateStr != null && item.StartDateStr != "")
                                        domainAttributeEntity.StartDate = DateTime.ParseExact(item.StartDateStr, "MM/dd/yyyy", null);
                                    if (item.EndDateStr != null && item.EndDateStr != "")
                                        domainAttributeEntity.EndDate = DateTime.ParseExact(item.EndDateStr, "MM/dd/yyyy", null);
                                    domainAttributeEntity.Amount = item.Amount;
                                    domainAttributeEntity.Duration = item.Duration;
                                    domainAttributeEntity.Notes = item.Notes;
                                    domainAttributeEntity.UpdatedOnUtc = GetDateTime;
                                    domainAttributeEntity.UpdatedById = LoggedUserId;
                                    _domainAttributeService.UpdateDomainAttributes(domainAttributeEntity);
                                }
                                else if (item.Flag == "New")
                                {
                                    if (domainAttributeEntity != null)
                                        continue;

                                    var data = _mapper.Map<DomainAttributes>(item);
                                    data.DomainId = entity.Id;
                                    if (item.StartDateStr != null && item.StartDateStr != "")
                                        data.StartDate = DateTime.ParseExact(item.StartDateStr, "MM/dd/yyyy", null);
                                    if (item.EndDateStr != null && item.EndDateStr != "")
                                        data.EndDate = DateTime.ParseExact(item.EndDateStr, "MM/dd/yyyy", null);
                                    data.CreatedOnUtc = GetDateTime;
                                    data.UpdatedOnUtc = GetDateTime;
                                    data.CreatedById = LoggedUserId;
                                    data.UpdatedById = LoggedUserId;
                                    _domainAttributeService.InsertDomainAttributes(data);
                                }
                                else if (item.Flag == "Delete")
                                {
                                    if (domainAttributeEntity == null)
                                        continue;

                                    _domainAttributeService.DeleteDomainAttributes(domainAttributeEntity);
                                }
                            }
                        }
                    }
                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _domainService.UpdateDomain(entity);
                    return Ok(entity);
                }
            }
            catch (Exception ex)
            {
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region DeleteDomain
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDomain(string id)
        {
            try
            {
                var entity = await _domainService.GetById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No domain found with the specified id."));

                _domainService.DeleteDomain(entity);

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
