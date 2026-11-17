using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Vsky.Api.Models
{
    // Title: TestPlanImportField
    // Description: Single allowlisted metadata contract shared by template generation and workbook parsing.
    // Only the seven editable Test Plan form fields are importable; SiteId, Id, TestPlanNumber and audit
    // fields are system-managed and are never read from a workbook.
    public static class TestPlanImportField
    {
        public const string ProjectId = "projectId";
        public const string Name = "name";
        public const string AreaId = "areaId";
        public const string WorkspaceId = "workspaceId";
        public const string PlanMakerId = "planMakerId";
        public const string PlanReviewerId = "planReviewerId";
        public const string Description = "description";

        public record Definition(string Key, string Header, bool Required, string Guidance);

        // Order defines the template column order.
        public static readonly IReadOnlyList<Definition> All = new List<Definition>
        {
            new(ProjectId, "Project Name", true, "Required. Select a project from the dropdown (options are listed on the 'Projects' sheet)."),
            new(Name, "Name", true, "Required. 1-200 characters. Must be unique within the project."),
            new(AreaId, "Area", false, "Optional. Select an area from the dropdown (options are listed on the 'Areas' sheet)."),
            new(WorkspaceId, "Workspace", false, "Optional. Select a workspace from the dropdown (options are listed on the 'Workspaces' sheet)."),
            new(PlanMakerId, "Plan Maker", true, "Required. Select an active employee from the dropdown (options are listed on the 'Employees' sheet)."),
            new(PlanReviewerId, "Plan Reviewer", true, "Required. Select an active employee from the dropdown (options are listed on the 'Employees' sheet)."),
            new(Description, "Description", false, "Optional. Plain text; line breaks are kept. HTML is not interpreted."),
        };

        public const string TemplateVersion = "2";
        public const string DataSheetName = "Test Plans";
        public const int NameMaxLength = 200;
        public const int MaxDataRows = 500;
        public const long MaxFileSizeBytes = 5 * 1024 * 1024;
    }

    // Title: TestPlanImportUploadModel
    // Description: Multipart upload for the preview endpoint.
    public record TestPlanImportUploadModel
    {
        public IFormFile File { get; set; }
    }

    // Title: TestPlanImportRowModel
    // Description: Canonical allowlisted values for one source row. Used in the preview response and
    // re-submitted (untrusted) on confirmation.
    public record TestPlanImportRowModel
    {
        public int RowNumber { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string AreaId { get; set; }
        public string WorkspaceId { get; set; }
        public string PlanMakerId { get; set; }
        public string PlanReviewerId { get; set; }
        public string Description { get; set; }
    }

    // Title: TestPlanImportValidationError
    // Description: A row/field validation error. RowNumber 0 means a file or structure level error.
    public record TestPlanImportValidationError
    {
        public int RowNumber { get; set; }
        public string Field { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }

    // Title: TestPlanImportPreviewRow
    // Description: Preview row with canonical values, resolved lookup labels and its errors.
    public record TestPlanImportPreviewRow : TestPlanImportRowModel
    {
        public string ProjectName { get; set; }
        public string AreaName { get; set; }
        public string WorkspaceName { get; set; }
        public string PlanMakerName { get; set; }
        public string PlanReviewerName { get; set; }
        public bool IsValid { get; set; }
        public List<TestPlanImportValidationError> Errors { get; set; } = new();
    }

    // Title: TestPlanImportPreviewModel
    // Description: Preview / validation response. CanConfirm is true only for a nonempty, fully valid batch.
    public record TestPlanImportPreviewModel
    {
        public List<TestPlanImportValidationError> FileErrors { get; set; } = new();
        public List<TestPlanImportPreviewRow> Rows { get; set; } = new();
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public bool CanConfirm { get; set; }
    }

    // Title: TestPlanImportConfirmModel
    // Description: Confirmation payload: canonical preview rows. Any validity flags are ignored.
    public record TestPlanImportConfirmModel
    {
        public List<TestPlanImportRowModel> Rows { get; set; } = new();
    }

    // Title: TestPlanImportCreatedItem
    public record TestPlanImportCreatedItem
    {
        public int RowNumber { get; set; }
        public string Id { get; set; }
        public int TestPlanNumber { get; set; }
        public string Name { get; set; }
    }

    // Title: TestPlanImportResultModel
    // Description: Truthful confirmation outcome. On failure ImportedCount is 0 (the batch is rolled back).
    public record TestPlanImportResultModel
    {
        public bool Success { get; set; }
        public int ImportedCount { get; set; }
        public string Message { get; set; }
        public List<TestPlanImportCreatedItem> Created { get; set; } = new();
        public TestPlanImportPreviewModel Validation { get; set; }
    }
}
