using System;
using System.Collections;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models
{
    public class ProjectSwimLanesList : BaseEntity
    {
        public string ProjectSwimlaneId { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }

        public string Color { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ProjectSwimLanes ProjectSwimlane { get; set; }
        public virtual ICollection<ProjectSwimLanesListsTasks> ProjectSwimLanesListsTasks { get; set; } = new List<ProjectSwimLanesListsTasks>();
    }
}
