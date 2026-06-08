using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record RequirementChangeLogModel : BaseEntityModel
    {
        public string RequirementId { get; set; }
        public string EmployeeId { get; set; }
        public DateTime RequirementLogDate { get; set; }
        public string Description { get; set; }
        public string Flag { get; set; }
        public string RequirementLogDateStr { get; set; }

        public string RequirementName { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public string UpdatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public bool Deleted { get; set; }

        public virtual Requirement Requirement { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
