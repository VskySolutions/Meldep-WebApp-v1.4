using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record DomainModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }
        public string DomainServerId { get; set; }
        public string HostingServerId { get; set; }
        public string DomainTypeId { get; set; }
        public string DomainMappingId { get; set; }
        public string Url { get; set; }
        public string ExternalMappingNote { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUsername { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseHostname { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public string FtpHostname { get; set; }
        public string FtpPort { get; set; }
        public string WebsiteLoginUrl { get; set; }
        public string WebsiteLoginId { get; set; }
        public string WebsiteLoginPassword { get; set; }
        public string Instructions { get; set; }
        public string Notes { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public string Tab { get; set; }

        public int DomainNotesCount { get; set; }
        public virtual SiteModel Site { get; set; }
        public virtual ProjectModel Project { get; set; }
        public virtual ServerModel DomainServer { get; set; }
        public virtual ServerModel HostingServer { get; set; }
        public virtual DropDownModel DomainType { get; set; }
        public virtual DropDownModel DomainMapping { get; }

        public virtual ICollection<DomainAttributesModel> DomainAttributesModel { get; set; } = new List<DomainAttributesModel>();
        public virtual ICollection<DomainAttributesModel> DomainAttributes { get; set; } = new List<DomainAttributesModel>();
    }
    public record DomainSearchModel : BaseSearchModel
    {
        public List<string> ProjectIds { get; set; }
        public List<string> DomainServerIds { get; set; }
        public List<string> HostingServerIds { get; set; }
        public List<string> DomainTypeIds { get; set; }
        public string Url { get; set; }
        public string SearchText { get; set; }
    }

    public record DomainListModel : BasePagedListModel<DomainModel>
    {
    }
}
