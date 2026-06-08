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
using Vsky.Services.InfraFTPs;
using Vsky.Api.Models;

namespace Vsky.Api.Controllers
{
    [Route("infra-ftp")]
    public class InfraFTPController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IInfraFTPService _infraFTPService;
        private readonly IInfraFTPsProjectInstanceMappingService _infraFTPsProjectInstanceMappingService;
        #endregion

        #region Services Initializations
        public InfraFTPController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISiteService siteService,
            ICommonService commonService,
            IInfraFTPService infraFTPService,
            IInfraFTPsProjectInstanceMappingService infraFTPsProjectInstanceMappingService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _siteService = siteService;
            _commonService = commonService;
            _infraFTPService = infraFTPService;
            _infraFTPsProjectInstanceMappingService = infraFTPsProjectInstanceMappingService;
        }
        #endregion

        #region GetAllInfraFTPForList
        // Title: GetAllInfraFTPForList
        // Description: This endpoint retrieves the list of infra ftp.
        [HttpPost("list")]
        public IActionResult GetAllInfraFTPForList(InfraFTPSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of infra FTP based on search criteria (name, sorting, pagination)
                var list = _infraFTPService.GetAllInfraFTPForList(
                    SiteId,
                    searchModel.InfraServiceIds,
                    searchModel.ProtocolTypeIds,
                    searchModel.EncryptionTypeIds,
                    searchModel.Name,
                    searchModel.SearchText,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new InfraFTPList
                {
                    InfraFTPsList = list,
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

        #region GetInfraFTPInDetailById
        // Title: GetInfraFTPInDetailById
        // Description: This endpoint retrieves the details of a specific infra ftp based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetInfraFTPInDetailById(string id)
        {
            try
            {
                var entity = await _infraFTPService.GetInfraFTPInDetailById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra FTP found with the specified id."));

                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Add & Update Infra FTP
        [HttpPost()]
        public async Task<IActionResult> AddUpdateInfraFTP(SaveInfraFTPList model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.InfraFTPLines != null && model.InfraFTPLines.Count() > 0)
                    {
                        foreach (var infraFTP in model.InfraFTPLines)
                        {
                            var existing = await _infraFTPService.GetInfraFTPById(infraFTP.Id);
                            if (existing != null && !infraFTP.Deleted)
                            {
                                // update existing
                                existing.InfraServiceId = infraFTP.InfraServiceId;
                                existing.ProtocolTypeId = infraFTP.ProtocolTypeId;
                                existing.EncryptionTypeId = infraFTP.EncryptionTypeId;
                                existing.WalletTypeId = !string.IsNullOrEmpty(infraFTP.WalletTypeId) ? infraFTP.WalletTypeId : null;
                                existing.WalletNumber = !string.IsNullOrEmpty(infraFTP.WalletNumber) ? infraFTP.WalletNumber : null;
                                existing.Name = infraFTP.Name;
                                existing.Host = infraFTP.Host;
                                existing.Port = infraFTP.Port;
                                existing.Instructions = !string.IsNullOrEmpty(infraFTP.Instructions) ? infraFTP.Instructions : null;
                                existing.UpdatedOnUtc = GetDateTime;
                                existing.UpdatedById = LoggedUserId;

                                _infraFTPService.UpdateInfraFTP(existing);
                            }
                            else if (existing == null && !infraFTP.Deleted)
                            {
                                // insert new
                                var ftp = new InfraFTP
                                {
                                    Id = infraFTP.Id ?? Guid.NewGuid().ToString(),
                                    SiteId = SiteId,
                                    InfraServiceId = infraFTP.InfraServiceId,
                                    ProtocolTypeId = infraFTP.ProtocolTypeId,
                                    EncryptionTypeId = infraFTP.EncryptionTypeId,
                                    WalletTypeId = !string.IsNullOrEmpty(infraFTP.WalletTypeId) ? infraFTP.WalletTypeId : null,
                                    WalletNumber = !string.IsNullOrEmpty(infraFTP.WalletNumber) ? infraFTP.WalletNumber : null,
                                    Name = infraFTP.Name,
                                    Host = infraFTP.Host,
                                    Port = infraFTP.Port,
                                    Instructions = !string.IsNullOrEmpty(infraFTP.Instructions) ? infraFTP.Instructions : null,
                                    CreatedOnUtc = GetDateTime,
                                    CreatedById = LoggedUserId,
                                    UpdatedOnUtc = GetDateTime,
                                    UpdatedById = LoggedUserId
                                };
                                _infraFTPService.InsertInfraFTP(ftp);
                            }
                        }
                    }

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
            return ModelStateError(ModelState);
        }
        #endregion

        #region InfraFTPAssignToProjectInstance
        //Infra FTP Assign To Project Instance
        [HttpPost("assign-to-project-instance")]
        public async Task<IActionResult> InfraFTPAssignToProjectInstance(string id, string projectInstanceId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the infra ftp entity by its ID
                    var entity = await _infraFTPService.GetInfraFTPById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Infra FTP found with the specified id."));

                    var InfraFTPsProjectInstanceMapping = new InfraFTPsProjectInstanceMapping
                    {
                        InfraFTPId = id,
                        ProjectInstanceId = projectInstanceId,
                        CreatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                    };
                    _infraFTPsProjectInstanceMappingService.InsertInfraFTPsProjectInstanceMapping(InfraFTPsProjectInstanceMapping);

                    return Ok(InfraFTPsProjectInstanceMapping.Id);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region AddorUpdateInstructions
        //created for update Instructions from list page
        [HttpPut("instructions/{id}")]
        public async Task<IActionResult> AddorUpdateInstructions(string id, SaveInfraFTP model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the infra ftp entity by its ID
                    var entity = await _infraFTPService.GetInfraFTPById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Infra FTP found with the specified id."));

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
                    _infraFTPService.UpdateInfraFTP(entity);

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

        #region DeleteInfraFTPsProjectInstanceMapping
        // Title: DeleteInfraFTPsProjectInstanceMapping
        // Description: This endpoint deletes a Infra FTPs Project Instance Mapping based on the provided Infra FTPs Project Instance Mapping ID. It first retrieves the Infra FTPs Project Instance Mapping entity by ID, checks if it exists, and if so, deletes the Infra FTPs Project Instance Mapping. If the Infra FTPs Project Instance Mapping is not found, it returns a BadRequest response with an error message.
        [HttpDelete("assignProjectInstance/{id}")]
        public async Task<IActionResult> DeleteInfraFTPsProjectInstanceMapping(string id)
        {
            try
            {
                // Fetch the Infra FTPs Project Instance Mapping entity by its ID
                var entity = await _infraFTPsProjectInstanceMappingService.GetInfraFTPsProjectInstanceMappingById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra FTPs project instance mapping found with the specified id."));

                _infraFTPsProjectInstanceMappingService.DeleteInfraFTPsProjectInstanceMapping(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteInfraFTP
        // Title: DeleteInfraFTP
        // Description: This endpoint deletes a infra FTP based on the provided infra FTP ID. It first retrieves the infra FTP entity by ID, checks if it exists, and if so, deletes the infra FTP. If the infra FTP is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInfraFTP(string id)
        {
            try
            {
                // Fetch the infra FTP entity by its ID
                var entity = await _infraFTPService.GetInfraFTPById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra FTP found with the specified id."));

                _infraFTPService.DeleteInfraFTP(entity);

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
