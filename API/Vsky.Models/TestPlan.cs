using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class TestPlan : BaseEntity
{
    public string SiteId { get; set; }
    public string PlanMakerId { get; set; }
    public string PlanReviewerId { get; set; }
    public string ProjectId { get; set; }
    public string AreaId { get; set; }
    public string WorkspaceId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int TestPlanNumber { get; set; }
    public string CreatedById { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    public virtual Site Site { get; set; }
    public virtual DropDown Area { get; set; }
    public virtual DropDown Workspace { get; set; }
    public virtual Employee PlanMaker { get; set; }
    public virtual Employee PlanReviewer { get; set; }
    public virtual Project Project { get; set; }
    public virtual ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
}
