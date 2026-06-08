using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class Inventory : BaseEntity
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

    [NotMapped]
    public int InventoryNotesCount { get; set; }

    public virtual InventoryItemType ItemType { get; set; }
    public virtual DropDown InventoryStatus { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual DropDown AssignmentType { get; set; }
    public virtual DropDown InventoryAssign { get; set; }
    public virtual DropDown OfficeLocation { get; set; }
    public virtual Site Site { get; set; }
    public virtual ApplicationUser CreatedBy { get; set; }
    public virtual ApplicationUser UpdatedBy { get; set; }
    public virtual ICollection<InventoryAssignment> InventoryAssignmentList { get; set; } = new List<InventoryAssignment>();
}

