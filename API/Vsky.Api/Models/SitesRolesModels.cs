using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record SitesRolesModels : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string RoleId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public string[] RoleIds { get; set; }
        public virtual Site Site { get; set; }
        public virtual ApplicationRole ApplicationRole { get; set; }
    }

    public record SitesRolesSearchModel : BaseSearchModel
    {
        public List<string> SiteRoleIds { get; set; }
        public string SearchText { get; set; }
    }
    public record SitesRolesListModel : BasePagedListModel<SitesRolesModels>
    {
    }
}
