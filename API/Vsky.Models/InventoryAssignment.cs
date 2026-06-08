using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class InventoryAssignment : BaseEntity
{
    public string InventoryId { get; set; }
    public string EmployeeId { get; set; }
    public DateTime? AssignDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string ReturnReson { get; set; }
    public string CreatedById { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public bool Deleted { get; set; }

    public virtual Inventory Inventory { get; set; }
    public virtual Employee Employee { get; set; }

    public int Count()
    {
        throw new NotImplementedException();
    }
}

