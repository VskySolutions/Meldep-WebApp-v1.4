using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class TrainingPortal : BaseEntity
    {
        public string SiteId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TrainingFileId { get; set; }
        public int TrainingPortalNumber { get; set; }
        public string Url { get; set; }

        // public string[] EmployeeDesignationIdsArray { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public virtual Picture File { get; set; }
        public virtual Site Site { get; set; }
        public virtual ICollection<Training_Portal_Mapping> TrainingPortalMappings { get; set; } = new List<Training_Portal_Mapping>();

    }
}
