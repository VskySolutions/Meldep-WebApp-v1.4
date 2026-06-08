using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record FilePathDetailsModel : BaseEntityModel
    {
        public string ModuleId { get; set; }

        public string ModuleName { get; set; }

        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Note { get; set; }
        public string Flag { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public string UpdatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public bool Deleted { get; set; }

        public virtual Requirement Requirement { get; set; }
    }
}
