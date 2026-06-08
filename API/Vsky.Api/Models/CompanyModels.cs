using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record CompanyModel : BaseEntityModel
    {
        public string Name { get; set; }
        public string EmployeeId { get; set; }
        public string SiteId { get; set; }
        public string City { get; set; }
        public string CountryId { get; set; }
        public string StateProvinceId { get; set; }
        public string ZipCode { get; set; }
        public string ContactName { get; set; }
        public string PhoneNumber { get; set; }
        public string AlternativePhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string AlternativeEmailAddress { get; set; }
        public string Website{ get; set; }
        public string AddressId { get; set; }
        public string ServiceProvidedDetails { get; set; }
        public string ProductDetails { get; set; }
        public string ProfileLink { get; set; }
        public string Description { get; set; }
        public DateTime ServiceProviderDate { get; set; }
        public DateTime ComapnyCreatedDate { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string BusinessTypeId { get; set; }
        public bool Active { get; set; }
        public string[] ClientAdvisorIds { get; set; }
        public string StatusId { get; set; }
        public string StatusName { get; set; }

        //for company customer
        public bool IsCustomer {  get; set; }
        public string AssignedToId { get; set; }
        public string ClientId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string CustomerTypeId { get; set; }
        public string CustomerId { get; set; }
        public int CompanyCount { get; set; }

        //
        public AddressModels Address { get; set; }
        //public SiteModel Site { get; set; }
        public DropDownModel BusinessType { get; set; }
        public DropDownModel Status { get; set; }
        public EmployeeModel Employee { get; set; }
        //public virtual ICollection<ContactModel> Contacts { get; set; } = new List<ContactModel>();
        public virtual ICollection<ProjectModel> Projects { get; set; } = new List<ProjectModel>();
        public virtual ICollection<CompanyContactsModels> CompanyContactModel { get; set; } = new List<CompanyContactsModels>();
        public virtual ICollection<AddressModels> AddressModel { get; set; } = new List<AddressModels>();
        public virtual ICollection<CompanyContactsModels> CompanyContacts { get; set; } = new List<CompanyContactsModels>();
    }

    public record CompanySearchModel : BaseSearchModel
    {
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public string BusinessTypeId { get; set; }
        public string EmployeeId { get; set; }
        public string SearchText { get; set; }
    }

    public record CompanyListModel : BasePagedListModel<CompanyModel>
    {
    }
}