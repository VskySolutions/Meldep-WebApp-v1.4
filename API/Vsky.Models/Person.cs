using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models;

public class Person : BaseEntity
{
    public string PictureId { get; set; }
    public string AddressId { get; set; }
    public string AddressTypeId { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string ProfileLink { get; set; }
    public string Title { get; set; }
    public DateTime? DOB { get; set; }
    public string GenderId { get; set; }
    public string PrimaryPhoneNumber { get; set; }
    public string PrimaryEmailAddress { get; set; }
    public DateTime? IdentifiedDate { get; set; }
    public string IdentifiedById { get; set; }
    public string IdentificationNote { get; set; }
    public string Relation { get; set; }
    public string RelationFullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Color { get; set; }
    public string BgColor { get; set; }
    public DateTime? CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public string UpdatedById { get; set; }
    public bool Deleted { get; set; }

    [NotMapped]
    public bool IsCustomer { get; set; }

    [NotMapped]
    public string FullName { get; set; }

    [NotMapped]
    public bool IsSharedUser { get; set; }
    public virtual Picture Picture { get; set; }
    public virtual DropDown Gender { get; set; }
    public virtual DropDown AddressType { get; set; }
    public virtual Address Address { get; set; }
    public virtual Person IdentifiedBy { get; set; }

    public virtual ICollection<PersonSitesMapping> PersonSitesMapping { get; set; } = new List<PersonSitesMapping>();
}
