using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record RequirementGroupModel : BaseEntityModel
    {
        public string ProjectId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int RequirementGroupNumber { get; set; }

        public virtual Project Project { get; set; }
        //public virtual ICollection<ProjectUserMapping> ProjectUserMappings { get; set; } = new List<ProjectUserMapping>();
    }

    public record RequirementGroupSearchModel : BaseSearchModel
    {
        public int RequirementGroupNumber { get; set; }
        public List<string> ProjectIds { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string SearchText { get; set; }
    }

    public record RequirementGroupListModel : BasePagedListModel<RequirementGroupModel>
    {
        public bool editing { get; set; }
    }
    public record RequirementGroupUploadModel : BaseEntityModel
    {
    }
}
