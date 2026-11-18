using System;

namespace Vsky.Api.Services.RequirementExport
{
    // Title: Requirement export row projections (WO-73)
    // Description: Read-only projections drawn from the user-facing detail/list screens. They carry readable
    // labels instead of raw entity graphs. Internal IDs are kept only where needed for scoping or ordering.

    public sealed class RequirementInfoRow
    {
        public string Id { get; set; }
        public string SiteId { get; set; }
        public string ProjectId { get; set; }
        public int RequirementNumber { get; set; }
        public string Title { get; set; }
        public string Project { get; set; }
        public string ProjectModule { get; set; }
        public string RequirementGroup { get; set; }
        public string RequirementType { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Area { get; set; }
        public string Workspace { get; set; }
        public string IdentifiedUserType { get; set; }
        public string IdentifiedEmployee { get; set; }
        public string IdentifiedCustomer { get; set; }
        public DateTime? IdentifiedDate { get; set; }
        public string RequirementOwner { get; set; }
        public string CustomerOwner { get; set; }
        public string EnteredBy { get; set; }
        public string ConfirmedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public string SourceLabel { get; set; }
        public string TargetLabel { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Scope { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }

    public sealed class RequirementTaskRow
    {
        public string Id { get; set; }
        public int TaskNumber { get; set; }
        public string Name { get; set; }
        public string ProjectModule { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal EstimateTime { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    public sealed class RequirementTestCaseRow
    {
        public string Id { get; set; }
        public int TestCaseNumber { get; set; }
        public string Name { get; set; }
        public string TestPlan { get; set; }
        public string ProjectModule { get; set; }
        public string Status { get; set; }
        public string AssignedTo { get; set; }
        public string TestedBy { get; set; }
        public DateTime TestedDate { get; set; }
        public string TestResult { get; set; }
        public string Description { get; set; }
        public string Steps { get; set; }
        public string ExpectedResult { get; set; }
        public string ActualResult { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    public sealed class RequirementIssueRow
    {
        public string Id { get; set; }
        public int IssueNumber { get; set; }
        public string Name { get; set; }
        public string ProjectModule { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string AssignedTo { get; set; }
        public string ReportedBy { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    public sealed class RequirementTimesheetRow
    {
        public string Id { get; set; }
        public DateTime? TimesheetDate { get; set; }
        public string Employee { get; set; }
        public int TaskNumber { get; set; }
        public string Task { get; set; }
        public string Activity { get; set; }
        public decimal Hours { get; set; }
        public decimal BillableHours { get; set; }
        public bool IsApproved { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    public sealed class RequirementQuestionRow
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }

    public sealed class RequirementActionItemRow
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Priority { get; set; }
        public string Employee { get; set; }
        public string Customer { get; set; }
        public DateTime? DueDate { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    public sealed class RequirementMappingRow
    {
        public string Id { get; set; }
        public string ProjectModule { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public string IntegrationDirection { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }

    public sealed class RequirementNoteRow
    {
        public string Id { get; set; }
        public string NoteType { get; set; }
        public string Note { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
