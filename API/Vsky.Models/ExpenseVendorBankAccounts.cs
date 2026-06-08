using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;
public class ExpenseVendorBankAccounts : BaseEntity
{
    public string VendorId { get; set; }
    public string AccountTypeId { get; set; }
    public string PaymentTypeId { get; set; }
    public string AccountNumber { get; set; }
    public string BankName { get; set; }
    public string IFSCCode { get; set; }
    public string BranchName { get; set; }
    public string UPI_ID { get; set; }
    public bool IsBankAccount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public bool Deleted { get; set; }
    public virtual ExpenseVendors Vendor { get; set; }
    public virtual DropDown AccountType { get; set; }
    public virtual DropDown PaymentType { get; set; }
}
