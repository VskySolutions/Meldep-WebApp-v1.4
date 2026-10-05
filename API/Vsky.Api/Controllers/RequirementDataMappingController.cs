using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.ProjectActionItem;
using Vsky.Services.RequirementDataMappings;
using Vsky.Services.Sites;
using Vsky.Services.SitesModifiedLog;

namespace Vsky.Api.Controllers
{
    [Route("requirement-data-mappings")]
    public class RequirementDataMappingController : BaseController
    {
        #region Define Services      
        private readonly GlobalVariable _globalVariable;
        private readonly IRequirementDataMappingService _requirementDataMappingService;
        private readonly IRequirementDataMappingNotesService _requirementDataMappingNotesService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly ISitesModifiedLogsService _sitesModifiedLogsService;
        private readonly IDropDownService _dropDownService;
        #endregion

        #region Services Initializations      
        public RequirementDataMappingController(
            GlobalVariable globalVariable,
            IRequirementDataMappingService requirementDataMappingService,
            IRequirementDataMappingNotesService requirementDataMappingNotesService,
            ICommonService commonService,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices,
            ISitesModifiedLogsService sitesModifiedLogsService,
            IDropDownService dropDownService
        )
        {
            _globalVariable = globalVariable;
            _requirementDataMappingService = requirementDataMappingService;
            _requirementDataMappingNotesService = requirementDataMappingNotesService;
            _commonService = commonService;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
            _sitesModifiedLogsService = sitesModifiedLogsService;
            _dropDownService = dropDownService;
        }
        #endregion

        #region GetAllRequirementDataMappings
        [HttpPost("list")]
        public async Task<IActionResult> GetAllRequirementDataMappings(RequirementDataMappingsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteId, LoggedUserId);

                var list = await _requirementDataMappingService.GetAllRequirementDataMappings(
                    SiteId,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.RequirementIds,
                    searchModel.Source,
                    searchModel.Target,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = new RequirementDataMappingList
                {
                    RequirementDataMappingsList = list,
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

        #region GetAllGroupRequirementDataMappings
        //[HttpGet("get-all-data-mappings-group")]
        //public async Task<IActionResult> GetAllRequirementDataMappingGroups()
        //{
        //    try
        //    {
        //        var SiteId = _globalVariable.SiteId;
        //        var data = await _requirementDataMappingService.GetAllRequirementDataMappingGroups(SiteId);

        //        var model = new RequirementDataMappingGroupList
        //        {
        //            DataMappingGroupList = data,
        //            Total = data.Count
        //        };

        //        return Ok(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        
        [HttpPost("get-all-data-mappings-group")]
        public async Task<IActionResult> GetAllRequirementDataMappingGroups(RequirementDataMappingsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                //var data = await _requirementDataMappingService.GetAllRequirementDataMappingGroups(SiteId);
                var data = await _requirementDataMappingService.GetAllRequirementDataMappingGroups(
                    SiteId,
                    LoggedUserId,
                    searchModel.SearchText,
                    searchModel.ProjectIds,
                    searchModel.ProjectModuleIds,
                    searchModel.RequirementIds,
                    searchModel.Source,
                    searchModel.Target,
                    searchModel.SortBy,
                    searchModel.Sorts,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = new RequirementDataMappingGroupList
                {
                    DataMappingGroupList = data,
                    Total = data.Count
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllDataMappingByRequirementId
        [HttpPost("data-mappings-list")]
        public IActionResult GetAllDataMappingByRequirementId(RequirementDataMappingsSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _requirementDataMappingService.GetAllDataMappingByRequirementId(
                    SiteId,
                    searchModel.RequirementId,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = new RequirementDataMappingList
                {
                    RequirementDataMappingsList = list,
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

        #region Get Data Mappings
        [HttpGet("get-data-mappings/{requirementId}")]
        public async Task<IActionResult> GetRequirementDataMappings(string requirementId)
        {
            try
            {
                var mappings = await _requirementDataMappingService.GetRequirementDataMappingsByRequirementId(requirementId);

                var model = new RequirementDataMappingList
                {
                    RequirementDataMappingsList = mappings,
                    Total = mappings.Count
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Create Data Mappings
        [HttpPost("save-data-mappings")]
        public async Task<IActionResult> AddUpdateRequirementDataMappings(SaveRequirementDataMappings model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var savedMappings = new List<RequirementDataMapping>();

                    if (model.DataMappings != null && model.DataMappings.Any())
                    {
                        foreach (var data in model.DataMappings)
                        {
                            RequirementDataMapping mapping;

                            // update mappings
                            if (!string.IsNullOrEmpty(data.Id))
                            {
                                mapping = await _requirementDataMappingService.GetRequirementDataMappingById(data.Id);

                                if (mapping == null)
                                    continue;

                                mapping.Source = data.Source;
                                mapping.Target = data.Target;
                                mapping.UpdatedById = LoggedUserId;
                                mapping.UpdatedOnUtc = GetDateTime;

                                if (data.Deleted)
                                {
                                    mapping.Deleted = true;
                                }

                                _requirementDataMappingService.UpdateRequirementDataMapping(mapping);
                            }
                            else
                            {
                                mapping = new RequirementDataMapping
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    RequirementId = model.RequirementId,
                                    Source = data.Source,
                                    Target = data.Target,
                                    CreatedById = LoggedUserId,
                                    CreatedOnUtc = GetDateTime,
                                    UpdatedById = LoggedUserId,
                                    UpdatedOnUtc = GetDateTime,
                                    Deleted = false
                                };

                                _requirementDataMappingService.InsertRequirementDataMapping(mapping);
                            }

                            //save notes
                            if(data.Notes != null && data.Notes.Any())
                            {
                                foreach (var noteItem in data.Notes)
                                {
                                    // Update notes
                                    if (!string.IsNullOrEmpty(noteItem.Id))
                                    {
                                        var note = await _requirementDataMappingNotesService.GetRequirementDataMappingNotesById(noteItem.Id);

                                        if (note == null)
                                            continue;

                                        if (!string.IsNullOrEmpty(noteItem.Note))
                                        {
                                            note.Note = await _azureBlobImageServices
                                                .ProcessHtmlAndManageImagesAsync(
                                                    noteItem.Note,
                                                    SiteData.Name,
                                                    "requirement-data-mapping-notes",
                                                    note.Id,
                                                    note.Note
                                                );
                                        }
                                        note.UpdatedById = LoggedUserId;
                                        note.UpdatedOnUtc = GetDateTime;

                                        if (noteItem.Deleted)
                                        {
                                            note.Deleted = true;
                                        }

                                        _requirementDataMappingNotesService.UpdateRequirementDataMappingNotes(note);
                                    }
                                    else
                                    {
                                        var note = new RequirementDataMappingNotes();
                                        note.Id = Guid.NewGuid().ToString();
                                        note.RequirementDataMappingId = mapping.Id;
                                            
                                        if (!string.IsNullOrEmpty(noteItem.Note))
                                        {
                                            note.Note = await _azureBlobImageServices
                                                .ProcessHtmlAndManageImagesAsync(
                                                    noteItem.Note,
                                                    SiteData.Name,
                                                    "requirement-data-mapping-notes",
                                                    note.Id
                                                );
                                        }

                                        note.CreatedById = LoggedUserId;
                                        note.CreatedOnUtc = GetDateTime;
                                        note.UpdatedById = LoggedUserId;
                                        note.UpdatedOnUtc = GetDateTime;
                                        note.Deleted = false;

                                        _requirementDataMappingNotesService.InsertRequirementDataMappingNotes(note);
                                    }
                                }
                            }

                            // Add saved mapping to return list
                            savedMappings.Add(mapping);
                        }
                        return Ok(savedMappings);
                    }
                }

                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Delete Data Mapping
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDataMapping(string id)
        {
            try
            {
                var entity = await _requirementDataMappingService.GetRequirementDataMappingById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No data mapping found with the specified id."));

                _requirementDataMappingService.DeleteRequirementDataMapping(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Data Mapping Notes

        #region Get Data Mapping Notes

        [HttpGet("get-data-mapping-notes/{requirementDataMappingId}")]
        public async Task<IActionResult> GetAllRequirementDataMappingNotes(string requirementDataMappingId)
        {
            try
            {
                var SiteId = _globalVariable.SiteId;

                var notes = await _requirementDataMappingNotesService.GetAllRequirementDataMappingNotesByRequirementMappingId(SiteId, requirementDataMappingId, true);

                var model = new RequirementDataMappingNoteList
                {
                    RequirementDataMappingNotesList = notes,
                    Total = notes.Count
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region AddNote
        [HttpPost("save-note")]
        public async Task<IActionResult> AddEditNote(SaveRequirementDataMappingNotes model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var Editentity = await _requirementDataMappingNotesService.GetRequirementDataMappingNotesById(model.Id);
                    if (Editentity != null)
                    {
                        Editentity.RequirementDataMappingId = model.RequirementDataMappingId;

                        if (!string.IsNullOrEmpty(model.Note))
                        {
                            Editentity.Note = await _azureBlobImageServices
                                .ProcessHtmlAndManageImagesAsync(
                                    model.Note,
                                    SiteData.Name,
                                    "requirement-data-mapping-notes",
                                    Editentity.Id,
                                    Editentity.Note
                                );
                        }

                        Editentity.UpdatedOnUtc = GetDateTime;
                        Editentity.UpdatedById = LoggedUserId;
                        _requirementDataMappingNotesService.UpdateRequirementDataMappingNotes(Editentity);
                    }
                    else
                    {
                        var Addentity = new RequirementDataMappingNotes();

                        Addentity.Id = Guid.NewGuid().ToString();
                        Addentity.RequirementDataMappingId = model.RequirementDataMappingId;

                        if (!string.IsNullOrEmpty(model.Note))
                        {
                            Addentity.Note = await _azureBlobImageServices
                                .ProcessHtmlAndManageImagesAsync(
                                    model.Note,
                                    SiteData.Name,
                                    "requirement-data-mapping-notes",
                                    Addentity.Id
                                );
                        }

                        Addentity.CreatedById = LoggedUserId;
                        Addentity.CreatedOnUtc = GetDateTime;
                        Addentity.UpdatedById = LoggedUserId;
                        Addentity.UpdatedOnUtc = GetDateTime;
                        _requirementDataMappingNotesService.InsertRequirementDataMappingNotes(Addentity);
                    }
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

        #region DeleteNote
        [HttpDelete("delete-note")]
        public async Task<IActionResult> DeleteNote(string id)
        {
            var entity = await _requirementDataMappingNotesService.GetRequirementDataMappingNotesById(id);
            if (entity == null)
                return BadRequest(new BadRequestError("No note found with the specified id."));

            _requirementDataMappingNotesService.DeleteRequirementDataMappingNotes(entity);

            return NoContent();
        }
        #endregion

        #endregion
    }
}
