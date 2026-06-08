using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record TestCaseModel : BaseEntityModel
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
        public string TestedDateStr { get; set; }
        public int TestCaseNumber { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public virtual Site Site { get; set; }
        public virtual DropDown Area { get; set; }
        public virtual DropDown Workspace { get; set; }
        public virtual TestPlan TestPlan { get; set; }
        public virtual DropDown Status { get; set; }
        public virtual Employee Employee { get; set; }

        public virtual ApplicationUser CreatedByUser { get; set; }
        public virtual Employee TestedByEmployee { get; set; }
        public virtual Project Project { get; set; }
    }

    public record TestCaseSearchModel : BaseSearchModel
    {
        public int TestCaseNumber { get; set; }
        public List<string> ProjectIds { get; set; }
        public string ProjectId { get; set; }
        public List<string> PlanIds { get; set; }
        public List<string> TestedBys { get; set; }
        public List<string> StatusIds { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchText { get; set; }
    }

    public record TestCaseListModel : BasePagedListModel<TestCaseModel>
    {
        public bool editing { get; set; }
    }
    public record TestCaseUploadModel : BaseEntityModel
    {
    }
}
