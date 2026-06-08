using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record PersonModel : BaseEntityModel
    {   
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string GenderId { get; set; }
        public string ProfileLink { get; set; }
        public string Title { get; set; }
        public DateTime? DOB { get; set; }
        public string PrimaryPhoneNumber { get; set; }
        public string PrimaryEmailAddress { get; set; }

        public DateTime? IdentifiedDate { get; set; }
        public string IdentifiedDateStr { get; set; }
        public string IdentifiedById { get; set; }
        public string IdentificationNote { get; set; }

        public string PictureId { get; set; }
        public string AddressId { get; set; }
        public string AddressTypeId { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CountryId { get; set; }
        public string StateProvinceId { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Relation { get; set; }
        public string RelationFullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Color { get; set; }
        public string BgColor { get; set; }

        public string Tab { get; set; }

        public string PersonFileId { get; set; }
        public IFormFile PersonPic { get; set; }
        public string PersonChangeFlag { get; set; }
        public string EmployeeCode { get; set; }

        // for company customer
        public bool IsCustomer { get; set; }
        public string CustomerTypeId { get; set; }
        public string AssignedToId { get; set; }
        public string ClientId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string CustomerId { get; set; }
        public string SiteId { get; set; }
        public bool PersonSiteFlag { get; set; }

        public bool IsSharedUser { get; set; }
        public virtual Picture Picture { get; set; }
        public virtual DropDown Gender { get; set; }
        public virtual DropDown AddressType { get; set; }
        public virtual Address Address { get; set; }
        public virtual Person IdentifiedBy { get; set; }
    }

    public record PersonSearchModel : BaseSearchModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrimaryPhoneNumber { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string CountryId { get; set; }
        public string StateProvinceId { get; set; }
        public string City { get; set; }
        public string SearchText { get; set; }
    }

    public record PersonListModel : BasePagedListModel<PersonModel>
    {
    }

    public record PersonUploadModel : BaseEntityModel
    {
    }
}