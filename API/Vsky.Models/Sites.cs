using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class Site : BaseEntity
{
    public string Name { get; set; }
    public string UserName { get; set; }
    public string PersonId { get; set; }
    public string AddressId { get; set; }
    public string SiteLogoId { get; set; }
    public string SiteLogoPath { get; set; }
    public string SiteFaviconId { get; set; }
    public string SiteFaviconPath { get; set; }
    public string TicketNoPrefix { get; set; }
    public string TicketGenerationEmail { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public bool Active { get; set; }
    public bool Deleted { get; set; }
    public string TimeZone { get; set; }
    public bool IsDropdownGenerated { get; set; }
    public virtual Person Person { get; set; }
    public virtual Address Address { get; set; }
    public virtual ICollection<SitesRoles> SitesRoles { get; set; } = new List<SitesRoles>();
}