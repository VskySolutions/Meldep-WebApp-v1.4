using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record DailyPlannerLineModel : BaseEntityModel
    {
        public string DailyPlannerId { get; set; }
        public string Description { get; set; }
        public decimal Hours { get; set; }
        public string ProjectId { get; set; }
        public string ProjectModuleId { get; set; }
        public string ProjectTaskId { get; set; }
        public string ProjectActivityId { get; set; }
        public string Flag { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public string ActivityNameDescription { get; set; }
        public virtual ProjectModel Project { get; set; }
        public virtual DailyPlannerModel DailyPlanner { get; set; }
        public virtual ProjectModuleModel ProjectModule { get; set; }
        public virtual ProjectTaskModel ProjectTask { get; set; }
        public virtual ProjectActivityModel ProjectActivity { get; set; }

    }

    public record DailyPlannerLineSearchModel : BaseSearchModel
    {       
    }

    public record DailyPlannerLineListModel : BasePagedListModel<DailyPlannerLineModel>
    {
    }

    public record DailyPlannerLineUploadModel : BaseEntityModel
    {
    }
}
