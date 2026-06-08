using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models
{
    public class MessageTemplateList
    {
        public virtual ICollection<MessageTemplate> MessageTemplateLists { get; set; } = new List<MessageTemplate>();
        public int Total { get; set; }
    }
    public class MessageTemplate : BaseEntity
    {
        public string Name { get; set; }

        public string BccEmailAddresses { get; set; }

        public string Subject { get; set; }

        public string EmailAccountId { get; set; }

        public string Body { get; set; }

        public bool Active { get; set; }

        public int? DelayBeforeSend { get; set; }

        public int? DelayPeriodId { get; set; }

        public string AttachedDownloadId { get; set; }

        public virtual EmailAccount EmailAccount { get; set; }
    }
}

