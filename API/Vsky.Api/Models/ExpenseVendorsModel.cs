using System.Collections.Generic;
using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ExpenseVendorsModel : BaseEntityModel
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
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string StateProvinceId { get; set; }
        public string City { get; set; }
        public string CountryId { get; set; }
        public string ZipCode { get; set; }
        public bool IsBankAccount { get; set; }
        public bool BankAccount { get; set; }
        public string BankAccountType { get; set; }
        public string UpiAccountType { get; set; }
        public string ByCashType { get; set; }
        public virtual Address Address { get; set; }
        public virtual Person Person { get; set; }
        public virtual Site Site { get; set; }
        public virtual ICollection<ExpenseVendorBankAccountsModel> BankAccountList { get; set; } = new List<ExpenseVendorBankAccountsModel>();
        public virtual ICollection<ExpenseVendorBankAccounts> ExpenseVendorBankAccounts { get; set; } = new List<ExpenseVendorBankAccounts>();
    }
    public record ExpenseVendorsSearchModel : BaseSearchModel
    {
        public string VendorName { get; set; }
        public List<string> VendorIds { get; set; }
        public string VendorEmail { get; set; }
        public string SearchText { get; set; }
    }

    public record ExpenseVendorsListModel : BasePagedListModel<ExpenseVendorsModel>
    {
        public bool editing { get; set; }
    }
    public record ExpenseVendorsUploadModel : BaseEntityModel
    {
    }
}
