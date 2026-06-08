using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class ExpenseVendors : BaseEntity
{
    public string SiteId { get; set; }
    public string AddressId { get; set; }
    public string PersonId { get; set; }
    public string VendorName { get; set; }
    public string Vendor_Phone { get; set; }
    public string Vendor_Email { get; set; }
    public string VendorCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public bool Deleted { get; set; }
    public virtual Address Address { get; set; }
    public virtual Person Person { get; set; }
    public virtual Site Site { get; set; }
    public virtual ICollection<ExpenseVendorBankAccounts> ExpenseVendorBankAccounts { get; set; } = new List<ExpenseVendorBankAccounts>();
}
