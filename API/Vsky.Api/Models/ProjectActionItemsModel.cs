using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record ProjectActionItemsSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public List<string> ProjectIds { get; set; }
        public string ProjectId { get; set; }
        public string RequirementId { get; set; }
        public List<string> RequirementIds { get; set; }
        public List<string> PriorityIds { get; set; }
        public string Title { get; set; }
        public List<string> CustomerIds { get; set; }
        public List<string> EmployeeIds { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
