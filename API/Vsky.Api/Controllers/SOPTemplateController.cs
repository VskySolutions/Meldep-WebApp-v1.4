using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.Sites;
using Vsky.Services.SOPTemplates;

namespace Vsky.Api.Controllers
{
    [Route("sop-template")]
    public class SOPTemplateController : BaseController
    {
        #region Define Services and Initializations
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly ISiteService _siteService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly ISOPTemplateService _sOPTemplateService;
        private readonly ISOPTemplateSectionService _sOPTemplateSectionService;
        private readonly ISOPTemplateSectionItemsService _sOPTemplateSectionItemsService;
        public SOPTemplateController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ICommonService commonService,
            IDropDownService dropDownService,
            IDropDownTypeService dropDownTypeService,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices,
            ISOPTemplateService sOPTemplateService,
            ISOPTemplateSectionService sOPTemplateSectionService,
            ISOPTemplateSectionItemsService sOPTemplateSectionItemsService)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _commonService = commonService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
            _sOPTemplateService = sOPTemplateService;
            _sOPTemplateSectionService = sOPTemplateSectionService;
            _sOPTemplateSectionItemsService = sOPTemplateSectionItemsService;
        }
        #endregion

        [HttpPost("list")]
        public IActionResult GetAllSOPTemplates(SOPTemplateSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                
                var list = _sOPTemplateService.GetAllSOPTemplates(searchModel.SearchText, SiteId, searchModel.Name, searchModel.IsActive, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);

                var model = new SOPTemplateListModel
                {
                    Data = _mapper.Map<IList<SOPTemplateModel>>(list),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        #region GetAllSOPTemplateListForDropdown
        // Title: GetAllSOPTemplateListForDropdown
        // Description: This endpoint retrieves the details of a specific template based on its unique identifier (ID). 
        [HttpGet("dropdown/list")]
        public async Task<IActionResult> GetAllSOPTemplateListForDropdown()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _sOPTemplateService.GetSOPTemplatesListForDropdown(SiteId);
                var model = _mapper.Map<List<SOPTemplateModel>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        [HttpGet("{id}")]
        public IActionResult GetSOPTemplateById(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var entity = _sOPTemplateService.GetSOPTemplateById(SiteId, id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No SOP Template Found"));


                var model = _mapper.Map<SOPTemplateModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpGet("details/{id}")]
        public IActionResult GetSOPTemplateByIdInDetail(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var entity = _sOPTemplateService.GetSOPTemplateByIdInDetail(SiteId, id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No SOP Template Found"));

                var model = _mapper.Map<SOPTemplateModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpPost("createupdate")]
        public async Task<IActionResult> CreateUpdateSOPTemplate(SOPTemplateModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    var data = _sOPTemplateService.GetSOPTemplateById(SiteId, model.Id);
                    string SOPTemplateId = "";
                    if (data == null)
                    {
                        var template = new SOPTemplate();
                        template.Id = model.Id;
                        template.SiteId = SiteId;
                        template.Name = model.Name;
                        template.SortOrder = model.SortOrder;
                        template.Description = model.Description;
                        template.Version = model.Version;
                        template.IsActive = model.IsActive;
                        template.CreatedById = LoggedUserId;
                        template.UpdatedById = LoggedUserId;
                        template.CreatedOnUtc = GetDateTime;
                        template.UpdatedOnUtc = GetDateTime;
                        _sOPTemplateService.InsertSOPTemplate(template);

                        SOPTemplateId = template.Id;
                    }
                    else
                    {
                        data.Name = model.Name;
                        data.SortOrder = model.SortOrder;
                        data.Description = model.Description;
                        data.Version = model.Version;
                        data.IsActive = model.IsActive;
                        data.UpdatedById = LoggedUserId;
                        data.UpdatedOnUtc = GetDateTime;
                        _sOPTemplateService.UpdateSOPTemplate(data);

                        SOPTemplateId = data.Id;
                    }

                    if (model.SOPTemplateSections.Any())
                    {
                        string SOPTemplateSectionId = "";
                        foreach (var section in model.SOPTemplateSections)
                        {
                            var sectionData = _sOPTemplateSectionService.GetSOPTemplateSectionById(section.Id);
                            if (sectionData == null) 
                            {
                                // Create:- Insert new section
                                var newSectionData = new SOPTemplateSection();
                                newSectionData.Id = section.Id;
                                newSectionData.TemplateId = SOPTemplateId;
                                newSectionData.Name = section.Name;
                                newSectionData.Description = section.Description;
                                newSectionData.SortOrder = section.SortOrder;
                                newSectionData.CreatedById = LoggedUserId;
                                newSectionData.UpdatedById = LoggedUserId;
                                newSectionData.CreatedOnUtc = GetDateTime;
                                newSectionData.UpdatedOnUtc = GetDateTime;
                                _sOPTemplateSectionService.InsertSOPTemplateSection(newSectionData);

                                SOPTemplateSectionId = newSectionData.Id;
                            }
                            else
                            {
                                // Update :- Update section as deleted
                                if (sectionData.Deleted)
                                {
                                    sectionData.Deleted = true;
                                    sectionData.UpdatedById = LoggedUserId;
                                    sectionData.UpdatedOnUtc = GetDateTime;
                                    _sOPTemplateSectionService.UpdateSOPTemplateSection(sectionData);
                                }
                                else
                                {
                                    // Update :- Update section data
                                    sectionData.Name = section.Name;
                                    sectionData.Description = section.Description;
                                    sectionData.SortOrder = section.SortOrder;
                                    sectionData.UpdatedById = LoggedUserId;
                                    sectionData.UpdatedOnUtc = GetDateTime;
                                    _sOPTemplateSectionService.UpdateSOPTemplateSection(sectionData);
                                }

                                SOPTemplateSectionId = sectionData.Id;
                            }

                            if (section.SOPTemplateSectionItems.Any())
                            {
                                string SOPTemplateSectionItemId = "";
                                foreach (var item in section.SOPTemplateSectionItems)
                                {
                                    var itemData = _sOPTemplateSectionItemsService.GetSOPTemplateSectionItemById(item.Id);

                                    if (itemData == null) 
                                    {
                                        // Create :- Insert new section item
                                        var newItemData = new SOPTemplateSectionItems();

                                        newItemData.Id = item.Id;
                                        newItemData.TemplateId = SOPTemplateId;
                                        newItemData.SectionId = SOPTemplateSectionId;
                                        newItemData.Name = item.Name;
                                        newItemData.Description = item.Description;
                                        newItemData.InputTypeId = item.InputTypeId;
                                        newItemData.IsMandatory = item.IsMandatory;
                                        newItemData.IsRequiredEvidence = item.IsRequiredEvidence;
                                        newItemData.ValidationJson = item.ValidationJson;
                                        newItemData.SortOrder = item.SortOrder;
                                        newItemData.CreatedById = LoggedUserId;
                                        newItemData.UpdatedById = LoggedUserId;
                                        newItemData.CreatedOnUtc = GetDateTime;
                                        newItemData.UpdatedOnUtc = GetDateTime;
                                        _sOPTemplateSectionItemsService.InsertSOPTemplateSectionItem(newItemData);

                                        SOPTemplateSectionItemId = newItemData.Id;
                                    }
                                    else
                                    {
                                        // Update :- Update section as deleted
                                        if (itemData.Deleted)
                                        {
                                            itemData.Deleted = true;
                                            itemData.UpdatedById = LoggedUserId;
                                            itemData.UpdatedOnUtc = GetDateTime;
                                            _sOPTemplateSectionItemsService.UpdateSOPTemplateSectionItem(itemData);
                                        }
                                        else
                                        {
                                            // Update :- Update section data
                                            itemData.TemplateId = SOPTemplateId;
                                            itemData.SectionId = SOPTemplateSectionId;
                                            itemData.Name = item.Name;
                                            itemData.Description = item.Description;
                                            itemData.InputTypeId = item.InputTypeId;
                                            itemData.IsMandatory = item.IsMandatory;
                                            itemData.IsRequiredEvidence = item.IsRequiredEvidence;
                                            itemData.ValidationJson = item.ValidationJson;
                                            itemData.SortOrder = item.SortOrder;
                                            itemData.CreatedById = LoggedUserId;
                                            itemData.UpdatedById = LoggedUserId;
                                            itemData.CreatedOnUtc = GetDateTime;
                                            itemData.UpdatedOnUtc = GetDateTime;
                                            _sOPTemplateSectionItemsService.UpdateSOPTemplateSectionItem(itemData);
                                        }

                                        SOPTemplateSectionItemId =itemData.Id;
                                    }
                                }
                            }
                        }
                    }
                    return Ok();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + ":- " + ex.InnerException);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSOPTemplate(string id)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                // Fetch the project entity by its ID
                var entity = _sOPTemplateService.GetSOPTemplateById(SiteId, id);
                // If no project is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No SOP template found with the specified id."));

                // Delete the project using the project service
                _sOPTemplateService.DeleteSOPTemplate(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
