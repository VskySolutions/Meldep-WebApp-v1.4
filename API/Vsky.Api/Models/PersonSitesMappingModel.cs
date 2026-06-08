using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record PersonSitesMappingModel : BaseEntityModel
    {
        public string PersonId { get; set; }
        public string SiteId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public string UpdatedById { get; set; }
        public bool IsSharedUser { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Sites { get; set; }
        public virtual Person Person { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
    }

    public record PersonSitesSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public List<string> PersonIds { get; set; }
        public string PrimaryEmailAddress { get; set; }
    }

    public record PersonSitesListModel : BasePagedListModel<PersonSitesMappingModel>
    {
    }

    public class SaveSiteSharing
    {
        //public List<string> RoleIds { get; set; }

        public string[] RoleIds { get; set; }
        public string Email { get; set; }
    }
}
