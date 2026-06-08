using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models;

public class Company : BaseEntity
{
    public string Name { get; set; }
    public string EmployeeId { get; set; }
    public string SiteId { get; set; }
    public string ContactName { get; set; }
    public string PhoneNumber { get; set; }
    public string AlternativePhoneNumber { get; set; }
    public string EmailAddress { get; set; }
    public string AlternativeEmailAddress { get; set; }
    public string Website { get; set; }
    public string BusinessTypeId { get; set; }
    public string AddressId { get; set; }
    public string ServiceProvidedDetails { get; set; }
    public string ProductDetails { get; set; }
    public string ProfileLink { get; set; }
    public string Description { get; set; }
    public DateTime? ServiceProviderDate { get; set; }
    public DateTime? ComapnyCreatedDate { get; set; }
    public DateTime? CreatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public bool Active { get; set; }
    public bool Deleted { get; set; }
    public string StatusId { get; set; }

    [NotMapped]
    public int CompanyCount { get; set; }

    //public virtual Site Site { get; set; }
    public virtual DropDown BusinessType { get; set; }
    public virtual DropDown Status { get; set; }
    public virtual Address Address { get; set; }
    public virtual Employee Employee { get; set; }

    public virtual ApplicationUser? CreatedBy { get; set; }
    public virtual ApplicationUser? UpdatedBy { get; set; }
    //public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    // public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    public virtual ICollection<CompanyContacts> CompanyContacts { get; set; } = new List<CompanyContacts>();
}