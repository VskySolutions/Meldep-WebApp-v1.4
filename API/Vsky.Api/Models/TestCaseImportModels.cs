using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Vsky.Api.Models
{
    // Title: TestCaseImportField
    // Description: Single allowlisted metadata contract shared by template generation and workbook parsing.
    // Only the 15 editable Test Case form fields are importable. SiteId, case Id/number, audit fields,
    // TestResult, release mappings and execution history are never read from a workbook.
    public static class TestCaseImportField
    {
        public const string ProjectId = "projectId";
        public const string PlanId = "planId";
        public const string ProjectModuleId = "projectModuleId";
        public const string RequirementId = "requirementId";
        public const string Name = "name";
        public const string AreaId = "areaId";
        public const string WorkspaceId = "workspaceId";
        public const string StatusId = "statusId";
        public const string EmployeeId = "employeeId";
        public const string TestedBy = "testedBy";
        public const string TestedDate = "testedDate";
        public const string Description = "description";
        public const string Steps = "steps";
        public const string ExpectedResult = "expectedResult";
        public const string ActualResult = "actualResult";

        public record Definition(string Key, string Header, bool Required, string Guidance);

        // Order defines the template column order (matches the Add Test Case form).
        public static readonly IReadOnlyList<Definition> All = new List<Definition>
        {
            new(ProjectId, "Project Name", true, "Required. Select a project from the dropdown (options are listed on the 'Projects' sheet)."),
            new(PlanId, "Test Plan Name", true, "Required. Select a test plan of the chosen project (dropdown lists only that project's plans)."),
            new(ProjectModuleId, "Project Module", true, "Required. Select a module of the chosen project (dropdown lists only that project's modules)."),
            new(RequirementId, "Requirement", true, "Required. Select a requirement of the chosen module (dropdown lists only that module's requirements)."),
            new(Name, "Name", true, "Required. 1-200 characters. Must be unique within the project, test plan and module."),
            new(AreaId, "Area", false, "Optional. Select an area from the dropdown."),
            new(WorkspaceId, "Workspace", false, "Optional. Select a workspace from the dropdown."),
            new(StatusId, "Status", false, "Optional. Select a status from the dropdown. Blank uses 'New', as the form does. 'Fail' creates an issue on import when no matching issue exists."),
            new(EmployeeId, "Assigned To", false, "Optional. Only when Status is Fail: select an active employee from the dropdown. Greyed out and not accepted for other statuses."),
            new(TestedBy, "Tested By", true, "Required. Select an active employee from the dropdown."),
            new(TestedDate, "Tested Date", false, "Optional. Enter or pick a date (date-formatted cell, MM/dd/yyyy). Blank uses today's date, as the form does."),
            new(Description, "Description", false, "Optional. Plain text; line breaks are kept. HTML is not interpreted."),
            new(Steps, "Steps", false, "Optional. Plain text; line breaks are kept. HTML is not interpreted."),
            new(ExpectedResult, "Expected Result", false, "Optional. Plain text; line breaks are kept. HTML is not interpreted."),
            new(ActualResult, "Actual Result", false, "Optional. Plain text; line breaks are kept. HTML is not interpreted."),
        };

        public static readonly string[] RichTextKeys = { Description, Steps, ExpectedResult, ActualResult };

        public const string TemplateVersion = "2";
        public const string DataSheetName = "Test Cases";
        public const string DateFormat = "MM/dd/yyyy";
        public const int NameMaxLength = 200;
        public const int TextMaxLength = 32767; // Excel cell text limit
        public const int MaxDataRows = 500;
        public const long MaxFileSizeBytes = 5 * 1024 * 1024;
    }

    public record TestCaseImportUploadModel
    {
        public IFormFile File { get; set; }
    }

    // Title: TestCaseImportRowModel
    // Description: Canonical allowlisted values for one source row (IDs after resolution; tested date as
    // MM/dd/yyyy; text fields as plain text). Re-submitted (untrusted) on confirmation.
    public record TestCaseImportRowModel
    {
        public int RowNumber { get; set; }
        public string ProjectId { get; set; }
        public string PlanId { get; set; }
        public string ProjectModuleId { get; set; }
        public string RequirementId { get; set; }
        public string Name { get; set; }
        public string AreaId { get; set; }
        public string WorkspaceId { get; set; }
        public string StatusId { get; set; }
        public string EmployeeId { get; set; }
        public string TestedBy { get; set; }
        public string TestedDate { get; set; }
        public string Description { get; set; }
        public string Steps { get; set; }
        public string ExpectedResult { get; set; }
        public string ActualResult { get; set; }
    }

    public record TestCaseImportValidationError
    {
        public int RowNumber { get; set; }
        public string Field { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }

    // Title: TestCaseImportPreviewRow
    // Description: Preview row with resolved labels, errors and the Fail-issue outcome the confirmation would
    // apply ("create", "exists", "created_in_batch"; null when the status is not Fail). Read-only.
    public record TestCaseImportPreviewRow : TestCaseImportRowModel
    {
        public string ProjectName { get; set; }
        public string PlanName { get; set; }
        public string ProjectModuleName { get; set; }
        public string RequirementName { get; set; }
        public string AreaName { get; set; }
        public string WorkspaceName { get; set; }
        public string StatusName { get; set; }
        public string EmployeeName { get; set; }
        public string TestedByName { get; set; }
        public string FailIssue { get; set; }
        public bool IsValid { get; set; }
        public List<TestCaseImportValidationError> Errors { get; set; } = new();
    }

    public record TestCaseImportPreviewModel
    {
        public List<TestCaseImportValidationError> FileErrors { get; set; } = new();
        public List<TestCaseImportPreviewRow> Rows { get; set; } = new();
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public int FailIssuesToCreate { get; set; }
        public bool CanConfirm { get; set; }
    }

    public record TestCaseImportConfirmModel
    {
        public List<TestCaseImportRowModel> Rows { get; set; } = new();
    }

    public record TestCaseImportCreatedItem
    {
        public int RowNumber { get; set; }
        public string Id { get; set; }
        public int TestCaseNumber { get; set; }
        public string Name { get; set; }
        public bool IssueCreated { get; set; }
        public int? IssueNumber { get; set; }
    }

    // Title: TestCaseImportResultModel
    // Description: Truthful confirmation outcome. On failure the whole batch (cases and issues) is rolled
    // back and ImportedCount/IssuesCreated are 0.
    public record TestCaseImportResultModel
    {
        public bool Success { get; set; }
        public int ImportedCount { get; set; }
        public int IssuesCreated { get; set; }
        public string Message { get; set; }
        public List<TestCaseImportCreatedItem> Created { get; set; } = new();
        public TestCaseImportPreviewModel Validation { get; set; }
    }
}
