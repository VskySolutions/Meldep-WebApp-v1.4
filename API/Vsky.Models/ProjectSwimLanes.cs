using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models
{
    public class ProjectSwimLanes : BaseEntity
    {
        public string ProjectId { get; set; }
        public string SwimlaneTypeId { get; set; }
        public string Name { get; set; }

        public bool IsMoved { get; set; }
        public string IsMovedFromId { get; set; }
        public bool IsCopied { get; set; }
        public string IsCopiedFromId { get; set; }
        public bool IsDuplicate { get; set; }
        public string IsDuplicateFromId { get; set; }

        public string Color { get; set; }
        public int SortOrder { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Project Project { get; set; }
        public virtual DropDown SwimlaneType { get; set; }
        public virtual ICollection<ProjectSwimLanesList> ProjectSwimLanesList { get; set; } = new List<ProjectSwimLanesList>();
    }
}
