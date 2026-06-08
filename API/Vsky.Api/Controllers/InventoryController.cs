using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.Inventories;
using Vsky.Services.Sites;

namespace Vsky.Api.Controllers
{
    [Route("inventory")]
    public class InventoryController : BaseController
    {

        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly IInventoryService _inventoryService;
        private readonly IInventoryAssignmentService _inventoryAssignmentService;
        private readonly IInventoryItemTypeService _inventoryItemTypeService;
        private readonly ISiteService _siteService;
        private readonly ApplicationDbContext _db;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        #endregion

        #region Services Initializations      
        public InventoryController(
            GlobalVariable globalVariable,
            IMapper mapper,
            ICommonService commonService,
            IInventoryService inventoryService,
            IInventoryAssignmentService inventoryAssignmentService,
            IInventoryItemTypeService inventoryItemTypeService,
            ApplicationDbContext db,
            ISiteService siteService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _globalVariable = globalVariable;
            _mapper = mapper;
            _commonService = commonService;
            _inventoryService = inventoryService;
            _inventoryAssignmentService = inventoryAssignmentService;
            _inventoryItemTypeService = inventoryItemTypeService;
            _db = db;
            _siteService = siteService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        [HttpGet("inventory-prefix/{itemTypeId}")]
        public async Task<IActionResult> GetNextInventoryCode(string itemTypeId)
        {
            var LoggedUserId = User.GetLoggedInUserId<string>();
            var SiteId = _globalVariable.SiteId;

            // Fetch item type details
            var lastInventoryCode = await _inventoryService.GetInventoryCode(SiteId, itemTypeId);
            string inventoryCode;

            // If no previous inventory code exists, start with "001"
            if (lastInventoryCode == null)
            {
                inventoryCode = "001";
            }
            else
            {
                inventoryCode = (Convert.ToInt32(lastInventoryCode.Inventorycode.Substring(lastInventoryCode.Inventorycode.Length - 3)) + 1).ToString("D3");
            }

            var itemType = await _inventoryItemTypeService.GetInventoryItemType(SiteId, itemTypeId);
            if (itemType == null)
                return BadRequest(new { message = "Invalid Item Type" });

            string generatedItemCode = $"VS/{itemType.Prefix}/{inventoryCode}";

            return Ok(new { itemCode = generatedItemCode });
        }

        #region GetAllInventory
        // Title: Get All Inventory
        // Description: This endpoint fetches a list of Inventory based on the provided search criteria such as name, sorting, and pagination. 
        [HttpPost("list")]
        public IActionResult GetAllInventory(InventorySearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = _inventoryService.GetAllInventory(
                    SiteId,
                    searchModel.SearchText,
                    searchModel.ItemTypeIds,
                    searchModel.Code,
                    searchModel.InventoryStatusIds,
                    searchModel.EmployeeIds,
                    searchModel.OfficeLocationIds,
                    searchModel.SortBy,
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var model = new InventoryListModel
                {
                    Data = _mapper.Map<IList<InventoryModel>>(list),
                    Total = list.TotalCount
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("item-types")]
        public async Task<IActionResult> GetAllItemType()
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var list = await _inventoryItemTypeService.GetAllItemType(SiteId);
                var model = _mapper.Map<List<InventoryItemTypeModels>>(list);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetInventoryDetailsById
        // Title: GetInventoryDetailsById
        // Description: This endpoint retrieves the details of a specific Inventory based on its unique identifier (ID). 
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetInventoryDetailsById(string id)
        {
            try
            {
                // Fetch the Inventory entity by its ID from the service
                var entity = await _inventoryService.GetInventoryDetailsById(id);
                if (entity == null)
                    return BadRequest(new BadRequestError("No Inventory found with the specified id."));

                var model = _mapper.Map<InventoryModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetInventoryAssignmentsByInventoryId
        // Title: GetInventoryAssignmentsByInventoryId
        // Description: This endpoint retrieves the details of a specific InventoryAssignment based on its unique identifier (ID). 
        [HttpGet("{inventoryId}")]
        public async Task<IActionResult> GetInventoryAssignmentsByInventoryId(string inventoryId)
        {
            try
            {
                // Fetch the InventoryAssignment entity by its ID from the service
                var entity = await _inventoryAssignmentService.GetInventoryAssignmentsByInventoryId(inventoryId);
                // If the InventoryAssignment entity is not found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Inventory Assignment found with the specified id."));

                // Map the InventoryAssignment entity to a InventoryAssignmentModel object
                var model = _mapper.Map<List<InventoryAssignmentModel>>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region CreateInventory
        // Title: CreateInventory
        // Description: This endpoint handles the creation of a new Inventory. It maps the Inventory model to the Inventory entity, sets the creation details, and inserts the Inventory into the database. 
        [HttpPost]
        public async Task<IActionResult> CreateInventory(InventoryModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Map the Inventory model to the Inventory entity
                    var entity = _mapper.Map<Inventory>(model);
                    entity.Id = Guid.NewGuid().ToString();

                    if (model.WarrantyExpiryDateStr != "" && model.WarrantyExpiryDateStr != null)
                        entity.WarrantyExpiryDate = DateTime.ParseExact(model.WarrantyExpiryDateStr, "MM/dd/yyyy", null);

                    if (model.DateofPurchaseStr != "" && model.DateofPurchaseStr != null)
                        entity.DateofPurchase = DateTime.ParseExact(model.DateofPurchaseStr, "MM/dd/yyyy", null);

                    if (!string.IsNullOrWhiteSpace(model.OfficeLocationId))
                        entity.OfficeLocationId = model.OfficeLocationId;
                    else
                        entity.OfficeLocationId = null;

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "infrastructure-inventory",
                                entity.Id
                            );
                    }

                    entity.SiteId = SiteId;
                    entity.CreatedById = LoggedUserId;
                    entity.UpdatedById = LoggedUserId;
                    entity.CreatedOnUtc = GetDateTime;
                    entity.UpdatedOnUtc = GetDateTime;
                    _inventoryService.InsertInventory(entity);

                    //save Inventory Assignment
                    if (model.InventoryAssignments != null && model.InventoryAssignments.Count() > 0)
                    {
                        var addList = new List<InventoryAssignment>();
                        var deleteList = new List<InventoryAssignment>();
                        var updateList = new List<InventoryAssignment>();

                        foreach (var item in model.InventoryAssignments)
                        {
                            // Fetch the Inventory Assignment entity by its ID
                            var type = await _inventoryAssignmentService.GetInventoryAssignmentById(item.Id);
                            if (item.Flag == "Edit")
                            {
                                // If no Inventory Assignment is found with the given ID, continue
                                if (type == null)
                                    continue;

                                type.InventoryId = entity.Id;
                                type.EmployeeId = item.EmployeeId;
                                if (item.AssignDateStr != "" && item.AssignDateStr != null)
                                    type.AssignDate = DateTime.ParseExact(item.AssignDateStr, "MM/dd/yyyy", null);

                                if (item.ReturnDateStr != "" && item.ReturnDateStr != null)
                                    type.ReturnDate = DateTime.ParseExact(item.ReturnDateStr, "MM/dd/yyyy", null);

                                type.ReturnReson = item.ReturnReson;
                                type.UpdatedById = LoggedUserId;
                                type.UpdatedOnUtc = GetDateTime;
                                updateList.Add(type);
                            }
                            else if (item.Flag == "New")
                            {
                                var inventoryAssignments = await _inventoryAssignmentService.GetInventoryAssignmentByInventoryId(item.EmployeeId, entity.Id);
                                // If no Inventory Assignment is found with the given ID, continue
                                if (type != null)
                                    continue;

                                var data = _mapper.Map<InventoryAssignment>(item);
                                data.InventoryId = entity.Id;
                                data.EmployeeId = item.EmployeeId;
                                if (item.AssignDateStr != "" && item.AssignDateStr != null)
                                    data.AssignDate = DateTime.ParseExact(item.AssignDateStr, "MM/dd/yyyy", null);

                                if (item.ReturnDateStr != "" && item.ReturnDateStr != null)
                                    data.ReturnDate = DateTime.ParseExact(item.ReturnDateStr, "MM/dd/yyyy", null);

                                data.ReturnReson = item.ReturnReson;
                                data.CreatedById = LoggedUserId;
                                data.UpdatedById = LoggedUserId;
                                data.CreatedOnUtc = GetDateTime;
                                data.UpdatedOnUtc = GetDateTime;
                                addList.Add(data);
                            }
                            else if (item.Flag == "Delete")
                            {
                                // If no Assignment is found with the given ID, continue
                                if (type == null)
                                    continue;

                                deleteList.Add(type);
                            }
                        }

                        if (addList.Count > 0)
                            _inventoryAssignmentService.InsertInventoryAssignmentList(addList);

                        if (updateList.Count > 0)
                            _inventoryAssignmentService.UpdateInventoryAssignmentList(updateList);

                        if (deleteList.Count > 0)
                            _inventoryAssignmentService.DeleteInventoryAssignmentList(deleteList);
                    }
                    return Ok(entity);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            // Return model state errors if the model state is not valid
            return ModelStateError(ModelState);
        }
        #endregion

        #region UpdateInventory
        // Title: UpdateInventory
        // Description: This endpoint updates an existing Inventory by its ID. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventory(string id, InventoryModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var LoggedUserId = User.GetLoggedInUserId<string>();
                    var SiteId = _globalVariable.SiteId;
                    var SiteData = await _siteService.GetById(SiteId);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);
                    
                    // Fetch the Inventory entity by its ID
                    var entity = await _inventoryService.GetInventoryById(id);
                    // If no Inventory is found with the given ID, return a bad request with an error message
                    if (entity == null)
                        return BadRequest(new BadRequestError("No Inventory found with the specified id."));

                    entity.ItemTypeId = model.ItemTypeId;
                    entity.InventoryStatusId = model.InventoryStatusId;
                    entity.Description = model.Description;
                    entity.EmployeeId = model.EmployeeId;
                    entity.AssignmentTypeId = model.AssignmentTypeId;
                    entity.Inventorycode = model.Inventorycode;
                    entity.Warranty = model.Warranty;
                    entity.Guaranty = model.Guaranty;
                    entity.InventoryAssignId = model.InventoryAssignId;
                    entity.ServiceCode = model.ServiceCode;
                    entity.Notes = model.Notes;
                    entity.OperatingSystem = model.OperatingSystem;
                    entity.ProcessorType = model.ProcessorType;
                    entity.MemoryORRAM = model.MemoryORRAM;
                    entity.HardDriveORStorageCapacity = model.HardDriveORStorageCapacity;
                    entity.GraphicsCard = model.GraphicsCard;
                    entity.VirusProtection = model.VirusProtection;
                    entity.ModelNameORNumber = model.ModelNameORNumber;
                    entity.AssignDate = model.AssignDate;
                    entity.ReturnDate = model.ReturnDate;
                    entity.ReturnReson = model.ReturnReson;
                    entity.OfficeLocationId = model.OfficeLocationId;

                    if (model.WarrantyExpiryDateStr != "" && model.WarrantyExpiryDateStr != null)
                        entity.WarrantyExpiryDate = DateTime.ParseExact(model.WarrantyExpiryDateStr, "MM/dd/yyyy", null);

                    if (model.DateofPurchaseStr != "" && model.DateofPurchaseStr != null)
                        entity.DateofPurchase = DateTime.ParseExact(model.DateofPurchaseStr, "MM/dd/yyyy", null);

                    if (!string.IsNullOrEmpty(model.Description))
                    {
                        entity.Description = await _azureBlobImageServices
                            .ProcessHtmlAndManageImagesAsync(
                                model.Description,
                                SiteData.Name,
                                "infrastructure-inventory",
                                entity.Id,
                                entity.Description
                            );
                    }

                    entity.UpdatedById = LoggedUserId;
                    entity.UpdatedOnUtc = GetDateTime;
                    _inventoryService.UpdateInventory(entity);

                    //save Inventory Assignment
                    if (model.InventoryAssignments.Count() > 0)
                    {
                        var addList = new List<InventoryAssignment>();
                        var deleteList = new List<InventoryAssignment>();
                        var updateList = new List<InventoryAssignment>();

                        foreach (var item in model.InventoryAssignments)
                        {
                            // Fetch the Inventory Assignment entity by its ID
                            var type = await _inventoryAssignmentService.GetInventoryAssignmentById(item.Id);
                            if (item.Flag == "Edit")
                            {
                                // If no Inventory Assignment is found with the given ID, continue
                                if (type == null)
                                    continue;

                                type.InventoryId = entity.Id;
                                type.EmployeeId = item.EmployeeId;
                                if (item.AssignDateStr != "" && item.AssignDateStr != null)
                                    type.AssignDate = DateTime.ParseExact(item.AssignDateStr, "MM/dd/yyyy", null);

                                if (item.ReturnDateStr != "" && item.ReturnDateStr != null)
                                    type.ReturnDate = DateTime.ParseExact(item.ReturnDateStr, "MM/dd/yyyy", null);

                                type.ReturnReson = item.ReturnReson;
                                type.UpdatedById = LoggedUserId;
                                type.UpdatedOnUtc = GetDateTime;
                                updateList.Add(type);
                            }
                            else if (item.Flag == "New")
                            {
                                var inventoryAssignments = await _inventoryAssignmentService.GetInventoryAssignmentByInventoryId(item.EmployeeId, item.InventoryId);
                                // If no Inventory Assignment is found with the given ID, continue
                                if (type != null)
                                    continue;

                                //if (inventoryAssignments.Count() > 0)
                                //    continue;

                                var data = _mapper.Map<InventoryAssignment>(item);
                                data.InventoryId = entity.Id;
                                data.EmployeeId = item.EmployeeId;
                                if (item.AssignDateStr != "" && item.AssignDateStr != null)
                                    data.AssignDate = DateTime.ParseExact(item.AssignDateStr, "MM/dd/yyyy", null);

                                if (item.ReturnDateStr != "" && item.ReturnDateStr != null)
                                    data.ReturnDate = DateTime.ParseExact(item.ReturnDateStr, "MM/dd/yyyy", null);

                                data.ReturnReson = item.ReturnReson;
                                data.CreatedById = LoggedUserId;
                                data.CreatedOnUtc = GetDateTime;
                                data.UpdatedById = LoggedUserId;
                                data.UpdatedOnUtc = GetDateTime;
                                addList.Add(data);
                            }
                            else if (item.Flag == "Delete")
                            {
                                // If no Assignment is found with the given ID, continue
                                if (type == null)
                                    continue;

                                deleteList.Add(type);
                            }
                        }
                        if (addList.Count > 0)
                            _inventoryAssignmentService.InsertInventoryAssignmentList(addList);

                        if (updateList.Count > 0)
                            _inventoryAssignmentService.UpdateInventoryAssignmentList(updateList);

                        if (deleteList.Count > 0)
                            _inventoryAssignmentService.DeleteInventoryAssignmentList(deleteList);
                    }
                    return Ok(entity);
                }
                return ModelStateError(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteInventory
        // Title: DeleteInventoryById
        // Description: This endpoint deletes a test case based on the provided Inventory ID. It first retrieves the Inventory entity by ID, checks if it exists, and if so, deletes the Inventory. If the Inventory is not found, it returns a BadRequest response with an error message.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(string id)
        {
            try
            {
                // Fetch the Inventory entity by its ID
                var entity = await _inventoryService.GetInventoryById(id);
                // If no Inventory is found, return a BadRequest response with an error message
                if (entity == null)
                    return BadRequest(new BadRequestError("No Ad found with the specified id."));

                // Delete the Inventory using the Inventory service
                _inventoryService.DeleteInventory(entity);

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
