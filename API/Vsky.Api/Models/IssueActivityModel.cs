using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record IssueActivityModel : BaseEntityModel
    {
        public string IssueId { get; set; }
        public string ActivityName { get; set; }
        public DateTime? DueDate { get; set; }
        public string PriorityId { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual Issue Issue { get; set; }
        public virtual DropDown Priority { get; set; }
        public virtual Employee AssignedToEmployee { get; set; }
    }
    public record IssueActivitySearchModel : BaseSearchModel
    {
    }
    public record IssueActivityListModel : BasePagedListModel<IssueActivityModel>
    {
        public bool editing { get; set; }
    }
}
