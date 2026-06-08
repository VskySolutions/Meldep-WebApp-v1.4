using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record InventoryAssignmentModel : BaseEntityModel
    {
        public string InventoryId { get; set; }
        public string EmployeeId { get; set; }
        public DateTime? AssignDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string ReturnReson { get; set; }
        public string AssignDateStr { get; set; }
        public string ReturnDateStr { get; set; }
        public string Flag { get; set; }

        public virtual Inventory Inventory { get; set; }
        public virtual Employee Employee { get; set; }
        //public virtual ICollection<InventoryAssignmentModel> InventoryAssignments { get; set; } = new List<InventoryAssignmentModel>();
    }

    public record InventoryAssignmentSearchModel : BaseSearchModel
    {
    }

    public record InventoryAssignmentListModel : BasePagedListModel<InventoryAssignmentModel>
    {
        public bool editing { get; set; }
    }
    public record InventoryAssignmentUploadModel : BaseEntityModel
    {
    }
}
