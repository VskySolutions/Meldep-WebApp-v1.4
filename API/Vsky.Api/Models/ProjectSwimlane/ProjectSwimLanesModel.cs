using System.Collections.Generic;
using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vsky.Api.Models.ProjectSwimlane
{
    public record ProjectSwimLanesModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public bool Deleted { get; set; }
        public int ProjectSwimlaneTaskCount { get; set; }
        public int TotalProjectSwimlaneTaskCount { get; set; }
        public int CompletedProjectSwimlaneTaskCount { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        
        [NotMapped]
        public ICollection<ProjectModule> ProjectModulesList { get; set; }
        public virtual ProjectModel Project { get; set; }
    }
    public record ProjectSwimLanesSearchModel : BaseSearchModel
    {
        public string ProjectId { get; set; }
        public string Year { get; set; }
        public string filterProjectSwimlane { get; set; }
    }

    public record ProjectSwimLanesListModel : BasePagedListModel<ProjectSwimLanesModel>
    {
    }
}
