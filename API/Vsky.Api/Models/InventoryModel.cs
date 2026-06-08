using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Microsoft.AspNetCore.Http;

namespace Vsky.Api.Models
{
    public record InventoryModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string ItemTypeId { get; set; }
        public string InventoryStatusId { get; set; }
        public string EmployeeId { get; set; }
        public string AssignmentTypeId { get; set; }
        public string Inventorycode { get; set; }
        public DateTime? DateofPurchase { get; set; }
        public string Warranty { get; set; }
        public string Guaranty { get; set; }
        public string InventoryAssignId { get; set; }
        public string ServiceCode { get; set; }
        public string Notes { get; set; }
        public string OperatingSystem { get; set; }
        public string ProcessorType { get; set; }
        public string MemoryORRAM { get; set; }
        public string HardDriveORStorageCapacity { get; set; }
        public string GraphicsCard { get; set; }
        public string VirusProtection { get; set; }
        public string ModelNameORNumber { get; set; }
        public string Description { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public DateTime? AssignDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string ReturnReson { get; set; }
        public string OfficeLocationId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public int InventoryNotesCount { get; set; }
        public string WarrantyExpiryDateStr { get; set; }
        public string DateofPurchaseStr { get; set; }
        public string AssignDateStr { get; set; }
        public string ReturnDateStr { get; set; }

        public virtual InventoryItemType ItemType { get; set; }
        public virtual DropDownModel InventoryStatus { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual DropDown AssignmentType { get; set; }
        public virtual DropDown InventoryAssign { get; set; }
        public virtual DropDown OfficeLocation { get; set; }
        public virtual Site Site { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<InventoryAssignmentModel> InventoryAssignments { get; set; } = new List<InventoryAssignmentModel>();
        public virtual ICollection<InventoryAssignment> InventoryAssignmentList { get; set; } = new List<InventoryAssignment>();
    }

    public record InventorySearchModel : BaseSearchModel
    {
        public List<string> ItemTypeIds { get; set; }
        public string Code { get; set; }
        public List<string> EmployeeIds { get; set; }
        public List<string> InventoryStatusIds { get; set; }
        public List<string> OfficeLocationIds { get; set; }
        public string SearchText { get; set; }
    }

    public record InventoryListModel : BasePagedListModel<InventoryModel>
    {
        public bool editing { get; set; }
    }
    public record InventoryUploadModel : BaseEntityModel
    {
    }
}
