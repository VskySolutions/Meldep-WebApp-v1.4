using System;
using Vsky.Core;


namespace Vsky.Models
{
    public class ProjectSwimLanesListsTasks : BaseEntity
    {
        public string ProjectSwimlaneListId {  get; set; }
        public string ProjectTaskId {  get; set; }
        public int SortOrder {  get; set; }

        public string Color { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual ProjectSwimLanesList ProjectSwimlaneList { get; set; }
        public virtual ProjectTask ProjectTask { get; set; }
    }
}
