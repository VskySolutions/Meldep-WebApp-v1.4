using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class Domain : BaseEntity
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

        [NotMapped]
        public int DomainNotesCount { get; set; }

        public virtual Site Site { get; set; }
        public virtual Project Project { get; set; }
        public virtual Server DomainServer {  get; set; }
        public virtual Server HostingServer { get; set; }
        public virtual DropDown DomainType { get; set; }
        public virtual DropDown DomainMapping { get; set; }

        public virtual ICollection<DomainAttributes> DomainAttributes { get; set; } = new List<DomainAttributes>();

    }
}
