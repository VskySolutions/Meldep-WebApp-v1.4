using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ProjectsMessages : BaseEntity
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }

        public string ParentMessageId { get; set; }

        public string SentBy { get; set; }

        public string Reaction { get; set; }

        public DateTime? SentDate { get; set; }

        public string Message { get; set; }

        public DateTime? CreatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public string UpdatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public bool IsSent { get; set; }
        [NotMapped]
        public string MessageTime { get; set; }

        public virtual Project Project { get; set; }
        public virtual Site Sites { get; set; }
        public virtual ApplicationUser SentByUser { get; set; }

    }
}
