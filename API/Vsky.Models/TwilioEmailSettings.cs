using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class TwilioEmailSettings : BaseEntity
    {
        public string SiteId { get; set; }

        public string TwilioAccountSid { get; set; }

        public string TwilioAuthToken { get; set; }

        public string TwilioFromMobileNo { get; set; }

        public string SendGridApiKey { get; set; }

        public string FromEmail { get; set; }

        public bool Active { get; set; }

        public DateTime? CreatedOnUtc { get; set; }

        public bool Deleted { get; set; }

        public bool IsDomainValidated { get; set; }

        public string AgencyId { get; set; }

        public virtual Site Site { get; set; }
    }
}
