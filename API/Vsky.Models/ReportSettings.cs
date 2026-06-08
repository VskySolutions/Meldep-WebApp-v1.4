using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ReportSettings : BaseEntity
    {
        public string SiteId { get; set; }
        public string Scope { get; set; }
        public string AuthorityUrl { get; set; }
        public string UrlPowerBiServiceApiRoot { get; set; }
        public string AuthenticationType { get; set; }
        public string ApplicationId { get; set; }
        public string WorkspaceId { get; set; }
        public string ApplicationSecret { get; set; }
        public string Tenant { get; set; }
        public bool Active { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Sites { get; set; }
    }
}
