using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record SiteModel : BaseEntityModel
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string PersonId { get; set; }
        public string AddressId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CountryId { get; set; }
        public string StateProvinceId { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string SiteLogoId { get; set; }
        public string SiteLogoPath { get; set; }
        public string SiteFaviconId { get; set; }
        public string SiteFaviconPath { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Active { get; set; }
        public string TimeZone { get; set; }
        public bool IsDropdownGenerated { get; set; }
        public bool Deleted { get; set; }

        public IFormFile File { get; set; }
        public string FileChangeFlag { get; set; }
        public IFormFile FileIcon { get; set; }
        public string FileIconChangeFlag { get; set; }
        public string PrimaryEmailAddress { get; set; }
        public string PrimaryPhoneNumber { get; set; }
        public string Password { get; set; }
        public string TicketNoPrefix { get; set; }
        public string TicketGenerationEmail { get; set; }
        public bool SendEmail { get; set; }
        public string[] RoleIds { get; set; }
        public virtual Person Person { get; set; }
        public virtual Address Address { get; set; }
        public virtual ICollection<SitesRoles> SitesRoles { get; set; } = new List<SitesRoles>();
    }

    public record SiteSearchModel : BaseSearchModel
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string SiteStatus { get; set; }
        public string SearchText { get; set; }
    }

    public record SiteListModel : BasePagedListModel<SiteModel>
    {
    }
}