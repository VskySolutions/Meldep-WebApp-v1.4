using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class HelpDeskEmailRepliesMapping : BaseEntity
    {
        public string HelpDeskId { get; set; }
        public string EmailRepliesId { get; set; }

        public virtual HelpDesk HelpDesk { get; set; }
        public virtual EmailReplies EmailReplies { get; set; }
    }
}
