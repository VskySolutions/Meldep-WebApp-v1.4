using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class TestCase : BaseEntity
{
    public string SiteId { get; set; }
    public string PlanId { get; set; }
    public string StatusId { get; set; }
    public string EmployeeId { get; set; }
    public string ProjectId { get; set; }
    public string AreaId { get; set; }
    public string WorkspaceId { get; set; }
    public string TestedBy { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Steps { get; set; }
    public string ExpectedResult { get; set; }
    public string ActualResult { get; set; }
    public string TestResult { get; set; }
    public DateTime TestedDate { get; set; }
    public int TestCaseNumber { get; set; }
    public string CreatedById { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    public virtual Site Site { get; set; }
    public virtual TestPlan TestPlan { get; set; }
    public virtual DropDown Area { get; set; }
    public virtual DropDown Workspace { get; set; }
    public virtual DropDown Status { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual ApplicationUser CreatedByUser { get; set; }
    public virtual Employee TestedByEmployee { get; set; }
    public virtual Project Project { get; set; }
}
