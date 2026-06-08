using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record TestPlanModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string PlanMakerId { get; set; }
        public string PlanReviewerId { get; set; }
        public string ProjectId { get; set; }
        public string AreaId { get; set; }
        public string WorkspaceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual Site Site { get; set; }
        public virtual DropDown Area { get; set; }
        public virtual DropDown Workspace { get; set; }
        public virtual Employee PlanMaker { get; set; }
        public virtual Employee PlanReviewer { get; set; }
        public virtual Project Project { get; set; }
        public int TestPlanNumber { get; set; }
    }

    public record TestPlanSearchModel : BaseSearchModel
    {
        public int TestPlanNumber { get; set; }
        public List<string> ProjectIds { get; set; }
        public string Name { get; set; }
        public string ProjectId { get; set; }
        public List<string> PlanMakerIds { get; set; }
        public List<string> PlanReviewerIds { get; set; }
        public string SearchText { get; set; }
    }

    public record TestPlanListModel : BasePagedListModel<TestPlanModel>
    {
        public bool editing { get; set; }
    }
    public record TestPlanUploadModel : BaseEntityModel
    {
    }
}
