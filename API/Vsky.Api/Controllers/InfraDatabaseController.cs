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
using Vsky.Services.InfraDatabases;
using Vsky.Api.Models;

namespace Vsky.Api.Controllers
{
    [Route("infra-database")]
    public class InfraDatabaseController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;
        private readonly IInfraDatabaseService _infraDatabaseService;
        private readonly IInfraDatabaseProjectInstanceMappingService _infraDatabaseProjectInstanceMappingService;
        #endregion

        #region Services Initializations
        public InfraDatabaseController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ISiteService siteService,
            ICommonService commonService,
            IInfraDatabaseService infraDatabaseService,
            IInfraDatabaseProjectInstanceMappingService infraDatabaseProjectInstanceMappingService
        )
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _siteService = siteService;
            _commonService = commonService;
            _infraDatabaseService = infraDatabaseService;
            _infraDatabaseProjectInstanceMappingService = infraDatabaseProjectInstanceMappingService;
        }
        #endregion

        #region GetAllInfraDatabaseForList
        // Title: GetAllInfraDatabaseForList
        // Description: This endpoint retrieves the list of infra database.
        [HttpPost("list")]
        public IActionResult GetAllInfraDatabaseForList(InfraDatabaseSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                // Fetch a list of infra database based on search criteria (name, sorting, pagination)
                var list = _infraDatabaseService.GetAllInfraDatabaseForList(
                    SiteId,
                    searchModel.InfraServiceIds,
                    searchModel.SearchText,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize);
                // Map the fetched list to a model suitable for the response
                var model = new InfraDatabaseList
                {
                    InfraDatabasesList = list,
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

        #region GetInfraDatabaseInDetailById
        // Title: GetInfraDatabaseInDetailById
        // Description: This endpoint retrieves the details of a specific infra database based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetInfraDatabaseInDetailById(string id)
        {
            try
            {
                var entity = await _infraDatabaseService.GetInfraDatabaseInDetailById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra database found with the specified id."));

                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Add & Update infra database
        [HttpPost()]
        public async Task<IActionResult> AddUpdateInfraDatabase(SaveInfraDatabaseList model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    if (model.InfraDatabaseLines != null && model.InfraDatabaseLines.Count() > 0)
                    {
                        foreach (var InfraDatabase in model.InfraDatabaseLines)
                        {
                            var existing = await _infraDatabaseService.GetInfraDatabaseById(InfraDatabase.Id);
                            if (existing != null && !InfraDatabase.Deleted)
                            {
                                // update existing
                                existing.InfraServiceId = InfraDatabase.InfraServiceId;
                                existing.WalletTypeId = !string.IsNullOrEmpty(InfraDatabase.WalletTypeId) ? InfraDatabase.WalletTypeId : null;
                                existing.WalletNumber = !string.IsNullOrEmpty(InfraDatabase.WalletNumber) ? InfraDatabase.WalletNumber : null;
                                existing.Name = InfraDatabase.Name;
                                existing.ServerName = InfraDatabase.ServerName;
                                existing.IsReadOrWrite = InfraDatabase.IsReadOrWrite;
                                existing.Instructions = !string.IsNullOrEmpty(InfraDatabase.Instructions) ? InfraDatabase.Instructions : null;
                                existing.UpdatedOnUtc = GetDateTime;
                                existing.UpdatedById = LoggedUserId;

                                _infraDatabaseService.UpdateInfraDatabase(existing);
                            }
                            else if (existing == null && !InfraDatabase.Deleted)
                            {
                                // insert new
                                var ftp = new InfraDatabase
                                {
                                    Id = InfraDatabase.Id ?? Guid.NewGuid().ToString(),
                                    SiteId = SiteId,
                                    InfraServiceId = InfraDatabase.InfraServiceId,
                                    WalletTypeId = !string.IsNullOrEmpty(InfraDatabase.WalletTypeId) ? InfraDatabase.WalletTypeId : null,
                                    WalletNumber = !string.IsNullOrEmpty(InfraDatabase.WalletNumber) ? InfraDatabase.WalletNumber : null,
                                    Name = InfraDatabase.Name,
                                    ServerName = InfraDatabase.ServerName,
                                    IsReadOrWrite = InfraDatabase.IsReadOrWrite,
                                    Instructions = !string.IsNullOrEmpty(InfraDatabase.Instructions) ? InfraDatabase.Instructions : null,
                                    CreatedOnUtc = GetDateTime,
                                    CreatedById = LoggedUserId,
                                    UpdatedOnUtc = GetDateTime,
                                    UpdatedById = LoggedUserId
                                };
                                _infraDatabaseService.InsertInfraDatabase(ftp);
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

        #region InfraDatabaseAssignToProjectInstance
        //Infra Database Assign To Project Instance
        [HttpPost("assign-to-project-instance")]
        public async Task<IActionResult> InfraDatabaseAssignToProjectInstance(string id, string projectInstanceId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the infra Database entity by its ID
                    var entity = await _infraDatabaseService.GetInfraDatabaseById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Infra Database found with the specified id."));

                    var InfraDatabasesProjectInstanceMapping = new InfraDatabaseProjectInstanceMapping
                    {
                        InfraDatabaseId = id,
                        ProjectInstanceId = projectInstanceId,
                        CreatedById = LoggedUserId,
                        CreatedOnUtc = GetDateTime,
                    };
                    _infraDatabaseProjectInstanceMappingService.InsertInfraDatabaseProjectInstanceMapping(InfraDatabasesProjectInstanceMapping);

                    return Ok(InfraDatabasesProjectInstanceMapping.Id);
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
        public async Task<IActionResult> AddorUpdateInstructions(string id, SaveInfraDatabase model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the infra database entity by its ID
                    var entity = await _infraDatabaseService.GetInfraDatabaseById(id);
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Infra database found with the specified id."));

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
                    _infraDatabaseService.UpdateInfraDatabase(entity);

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

        #region DeleteInfraDatabaseProjectInstanceMapping
        // Title: DeleteInfraDatabaseProjectInstanceMapping
        // Description: This endpoint deletes a Infra Databases Project Instance Mapping based on the provided Infra Databases Project Instance Mapping ID. It first retrieves the Infra Databases Project Instance Mapping entity by ID, checks if it exists, and if so, deletes the Infra Databases Project Instance Mapping. If the Infra Databases Project Instance Mapping is not found, it returns a BadRequest response with an error message.
        [HttpDelete("assignProjectInstance/{id}")]
        public async Task<IActionResult> DeleteInfraDatabaseProjectInstanceMapping(string id)
        {
            try
            {
                // Fetch the Infra Databases Project Instance Mapping entity by its ID
                var entity = await _infraDatabaseProjectInstanceMappingService.GetInfraDatabaseProjectInstanceMappingById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra Databases project instance mapping found with the specified id."));

                _infraDatabaseProjectInstanceMappingService.DeleteInfraDatabaseProjectInstanceMapping(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteInfraDatabase
        // Title: DeleteInfraDatabase
        // Description: This endpoint deletes a infra database based on the provided infra database ID. It first retrieves the infra database entity by ID, checks if it exists, and if so, deletes the infra database. If the infra database is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInfraDatabase(string id)
        {
            try
            {
                // Fetch the infra database entity by its ID
                var entity = await _infraDatabaseService.GetInfraDatabaseById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No infra database found with the specified id."));

                _infraDatabaseService.DeleteInfraDatabase(entity);

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
