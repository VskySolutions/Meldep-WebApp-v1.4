using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class EmailReplies : BaseEntity
    {
        public string OwnerId { get; set; }
        public string TwilioEmailId { get; set; }
        public string FromEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ToEmail { get; set; }
        public string ExternalToEmail { get; set; }
        public string CCEmail { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public bool IsRead { get; set; }
        public bool IsSystemEmail { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
