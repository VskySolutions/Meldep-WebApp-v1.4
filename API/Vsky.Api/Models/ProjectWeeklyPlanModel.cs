using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public class ProjectWeeklyPlanModel
    {

    }

    public record ProjectWeeklyPlanSearchModel : BaseSearchModel
    {
        public string PlanTypeId { get; set; }
        public string SearchText { get; set; }
        public List<string> ProjectIds { get; set; }
        public List<string> ProjectCoordinatorIds { get; set; }
        public List<string> ProjectLeadsIds { get; set; }
        public List<string> ProjectStatusIds { get; set; }
        public string StatusId { get; set; }
        public List<string> ProjectPriorityIds { get; set; }
        public List<string> ProjectTypeIds { get; set; }
        public List<string> CustomerIds { get; set; }
        public List<string> CompanyContactIds { get; set; }
    }
}
