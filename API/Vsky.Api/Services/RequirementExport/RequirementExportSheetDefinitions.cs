using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Vsky.Api.Services.RequirementExport
{
    public sealed class RequirementExportColumnMapping<TRow>
    {
        public RequirementExportColumnMapping(string header, RequirementExportCellKind kind, Func<TRow, object> value)
        {
            Header = header;
            Kind = kind;
            Value = value;
        }

        public string Header { get; }
        public RequirementExportCellKind Kind { get; }
        public Func<TRow, object> Value { get; }
    }

    public interface IRequirementExportSheetDefinition
    {
        string ModuleKey { get; }
        string SheetTitle { get; }

        /// <summary>Menu links that grant View on the owning module (any one is enough).</summary>
        IReadOnlyList<string> MenuLinks { get; }

        Task<RequirementExportSheet> CollectAsync(IRequirementExportDataSource source, RequirementExportScope scope, CancellationToken ct);
    }

    // Title: RequirementExportSheetDefinition
    // Description: Module key, sheet title, ordered column mappings, deterministic row ordering and the
    // scoped loader for one requirement-linked module. Rows that the loader returns are mapped through
    // the explicit columns only; nothing else from the record reaches the workbook.
    public sealed class RequirementExportSheetDefinition<TRow> : IRequirementExportSheetDefinition
    {
        private readonly Func<IRequirementExportDataSource, RequirementExportScope, CancellationToken, Task<List<TRow>>> _load;
        private readonly Func<IEnumerable<TRow>, IOrderedEnumerable<TRow>> _order;

        public RequirementExportSheetDefinition(
            string moduleKey,
            string sheetTitle,
            IReadOnlyList<string> menuLinks,
            Func<IRequirementExportDataSource, RequirementExportScope, CancellationToken, Task<List<TRow>>> load,
            Func<IEnumerable<TRow>, IOrderedEnumerable<TRow>> order,
            params RequirementExportColumnMapping<TRow>[] columns)
        {
            ModuleKey = moduleKey;
            SheetTitle = sheetTitle;
            MenuLinks = menuLinks;
            _load = load;
            _order = order;
            Columns = columns;
        }

        public string ModuleKey { get; }
        public string SheetTitle { get; }
        public IReadOnlyList<string> MenuLinks { get; }
        public IReadOnlyList<RequirementExportColumnMapping<TRow>> Columns { get; }

        public async Task<RequirementExportSheet> CollectAsync(IRequirementExportDataSource source, RequirementExportScope scope, CancellationToken ct)
        {
            var rows = await _load(source, scope, ct)
                       ?? throw new InvalidOperationException($"The {SheetTitle} data could not be retrieved.");
            return Build(rows);
        }

        public RequirementExportSheet Build(IEnumerable<TRow> rows)
        {
            return new RequirementExportSheet
            {
                ModuleKey = ModuleKey,
                Title = SheetTitle,
                Columns = Columns.Select(c => new RequirementExportColumn(c.Header, c.Kind)).ToList(),
                Rows = _order(rows).Select(r => Columns.Select(c => c.Value(r)).ToArray()).ToList()
            };
        }
    }

    // Title: RequirementExportSheetDefinitions
    // Description: Registry of requirement-linked modules, in workbook order. Columns are drafted from the
    // user-facing list/detail screens and are pending product-owner confirmation (WO-73 field-mapping gate).
    public static class RequirementExportSheetDefinitions
    {
        private const RequirementExportCellKind Text = RequirementExportCellKind.Text;
        private const RequirementExportCellKind Number = RequirementExportCellKind.Number;
        private const RequirementExportCellKind Date = RequirementExportCellKind.Date;
        private const RequirementExportCellKind DateTime = RequirementExportCellKind.DateTime;
        private const RequirementExportCellKind Boolean = RequirementExportCellKind.Boolean;

        private static string Rich(string html) => RequirementExportText.FromRichText(html);

        // Menu links mirror the RequireMenu tiers on each module's own controller.
        private static readonly string[] ProjectWorkLinks = Split(Authorization.MenuLinks.ProjectWork);
        private static readonly string[] TestCaseLinks = { Authorization.MenuLinks.TestCase };
        private static readonly string[] IssueLinks = { Authorization.MenuLinks.Issue };
        private static readonly string[] TimesheetLinks = { Authorization.MenuLinks.Timesheet, Authorization.MenuLinks.BillingProcessing, Authorization.MenuLinks.Dashboard };
        private static readonly string[] MappingLinks = { Authorization.MenuLinks.RequirementDataMapping, Authorization.MenuLinks.Requirement };
        private static readonly string[] RequirementLinks = { Authorization.MenuLinks.Requirement };

        public static readonly RequirementExportSheetDefinition<RequirementTaskRow> Tasks = new(
            "tasks", "Tasks", ProjectWorkLinks,
            (s, scope, ct) => s.GetTasksAsync(scope, ct),
            rows => rows.OrderBy(x => x.TaskNumber).ThenBy(x => x.CreatedOnUtc).ThenBy(x => x.Id, StringComparer.Ordinal),
            new("Task #", Number, x => x.TaskNumber),
            new("Task Name", Text, x => x.Name),
            new("Module", Text, x => x.ProjectModule),
            new("Type", Text, x => x.Type),
            new("Status", Text, x => x.Status),
            new("Priority", Text, x => x.Priority),
            new("Assigned To", Text, x => x.AssignedTo),
            new("Start Date", Date, x => x.StartDate),
            new("End Date", Date, x => x.EndDate),
            new("Due Date", Date, x => x.DueDate),
            new("Estimate (Hours)", Number, x => x.EstimateTime),
            new("Description", Text, x => Rich(x.Description)),
            new("Created On (UTC)", DateTime, x => x.CreatedOnUtc));

        public static readonly RequirementExportSheetDefinition<RequirementTestCaseRow> TestCases = new(
            "test-cases", "Test Cases", TestCaseLinks,
            (s, scope, ct) => s.GetTestCasesAsync(scope, ct),
            rows => rows.OrderBy(x => x.TestCaseNumber).ThenBy(x => x.CreatedOnUtc).ThenBy(x => x.Id, StringComparer.Ordinal),
            new("Test Case #", Number, x => x.TestCaseNumber),
            new("Test Case Name", Text, x => x.Name),
            new("Test Plan", Text, x => x.TestPlan),
            new("Module", Text, x => x.ProjectModule),
            new("Status", Text, x => x.Status),
            new("Assigned To", Text, x => x.AssignedTo),
            new("Tested By", Text, x => x.TestedBy),
            new("Tested Date", Date, x => x.TestedDate == default ? null : x.TestedDate),
            new("Test Result", Text, x => Rich(x.TestResult)),
            new("Description", Text, x => Rich(x.Description)),
            new("Steps", Text, x => Rich(x.Steps)),
            new("Expected Result", Text, x => Rich(x.ExpectedResult)),
            new("Actual Result", Text, x => Rich(x.ActualResult)),
            new("Created On (UTC)", DateTime, x => x.CreatedOnUtc));

        public static readonly RequirementExportSheetDefinition<RequirementIssueRow> Issues = new(
            "issues", "Issues", IssueLinks,
            (s, scope, ct) => s.GetIssuesAsync(scope, ct),
            rows => rows.OrderBy(x => x.IssueNumber).ThenBy(x => x.CreatedOnUtc).ThenBy(x => x.Id, StringComparer.Ordinal),
            new("Issue #", Number, x => x.IssueNumber),
            new("Issue Name", Text, x => x.Name),
            new("Module", Text, x => x.ProjectModule),
            new("Type", Text, x => x.Type),
            new("Status", Text, x => x.Status),
            new("Priority", Text, x => x.Priority),
            new("Assigned To", Text, x => x.AssignedTo),
            new("Reported By", Text, x => x.ReportedBy),
            new("Due Date", Date, x => x.DueDate),
            new("Close Date", Date, x => x.CloseDate),
            new("Description", Text, x => Rich(x.Description)),
            new("Created On (UTC)", DateTime, x => x.CreatedOnUtc));

        public static readonly RequirementExportSheetDefinition<RequirementTimesheetRow> Timesheets = new(
            "timesheets", "Timesheets", TimesheetLinks,
            (s, scope, ct) => s.GetTimesheetsAsync(scope, ct),
            rows => rows.OrderBy(x => x.TimesheetDate).ThenBy(x => x.Employee, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.CreatedOnUtc).ThenBy(x => x.Id, StringComparer.Ordinal),
            new("Date", Date, x => x.TimesheetDate),
            new("Employee", Text, x => x.Employee),
            new("Task #", Number, x => x.TaskNumber),
            new("Task", Text, x => x.Task),
            new("Activity", Text, x => x.Activity),
            new("Hours", Number, x => x.Hours),
            new("Billable Hours", Number, x => x.BillableHours),
            new("Approved", Boolean, x => x.IsApproved),
            new("Description", Text, x => Rich(x.Description)));

        public static readonly RequirementExportSheetDefinition<RequirementQuestionRow> Questions = new(
            "questions-answers", "Q&A", ProjectWorkLinks,
            (s, scope, ct) => s.GetQuestionsAsync(scope, ct),
            rows => rows.OrderBy(x => x.CreatedOnUtc).ThenBy(x => x.Id, StringComparer.Ordinal),
            new("Question", Text, x => x.Title),
            new("Description", Text, x => Rich(x.Description)),
            new("Created By", Text, x => x.CreatedBy),
            new("Created On (UTC)", DateTime, x => x.CreatedOnUtc),
            new("Updated On (UTC)", DateTime, x => x.UpdatedOnUtc == default ? null : x.UpdatedOnUtc));

        public static readonly RequirementExportSheetDefinition<RequirementActionItemRow> ActionItems = new(
            "action-items", "Action Items", ProjectWorkLinks,
            (s, scope, ct) => s.GetActionItemsAsync(scope, ct),
            rows => rows.OrderBy(x => x.CreatedOnUtc).ThenBy(x => x.Id, StringComparer.Ordinal),
            new("Title", Text, x => x.Title),
            new("Priority", Text, x => x.Priority),
            new("Employee", Text, x => x.Employee),
            new("Customer", Text, x => x.Customer),
            new("Due Date", Date, x => x.DueDate),
            new("Description", Text, x => Rich(x.Description)),
            new("Created On (UTC)", DateTime, x => x.CreatedOnUtc));

        public static readonly RequirementExportSheetDefinition<RequirementMappingRow> Mappings = new(
            "mappings", "Mappings", MappingLinks,
            (s, scope, ct) => s.GetMappingsAsync(scope, ct),
            rows => rows.OrderBy(x => x.CreatedOnUtc).ThenBy(x => x.Id, StringComparer.Ordinal),
            new("Module", Text, x => x.ProjectModule),
            new("Source", Text, x => x.Source),
            new("Target", Text, x => x.Target),
            new("Integration Direction", Text, x => x.IntegrationDirection),
            new("Created On (UTC)", DateTime, x => x.CreatedOnUtc),
            new("Updated On (UTC)", DateTime, x => x.UpdatedOnUtc));

        public static readonly RequirementExportSheetDefinition<RequirementNoteRow> Notes = new(
            "notes", "Notes", RequirementLinks,
            (s, scope, ct) => s.GetNotesAsync(scope, ct),
            rows => rows.OrderBy(x => x.CreatedOnUtc).ThenBy(x => x.Id, StringComparer.Ordinal),
            new("Note Type", Text, x => x.NoteType),
            new("Note", Text, x => Rich(x.Note)),
            new("Created By", Text, x => x.CreatedBy),
            new("Created On (UTC)", DateTime, x => x.CreatedOnUtc));

        /// <summary>Workbook order after Requirement Info.</summary>
        public static IReadOnlyList<IRequirementExportSheetDefinition> All { get; } = new IRequirementExportSheetDefinition[]
        {
            Tasks, TestCases, Issues, Timesheets, Questions, ActionItems, Mappings, Notes
        };

        public static IReadOnlyList<RequirementExportField> BuildInfo(RequirementInfoRow r)
        {
            return new List<RequirementExportField>
            {
                new("Requirement #", r.RequirementNumber, Number),
                new("Title", r.Title),
                new("Project", r.Project),
                new("Module", r.ProjectModule),
                new("Requirement Group", r.RequirementGroup),
                new("Requirement Type", r.RequirementType),
                new("Status", r.Status),
                new("Priority", r.Priority),
                new("Area", r.Area),
                new("Workspace", r.Workspace),
                new("Identified By Type", r.IdentifiedUserType),
                new("Identified By (Employee)", r.IdentifiedEmployee),
                new("Identified By (Customer)", r.IdentifiedCustomer),
                new("Identified Date", r.IdentifiedDate, Date),
                new("Requirement Owner", r.RequirementOwner),
                new("Customer Owner", r.CustomerOwner),
                new("Entered By", r.EnteredBy),
                new("Confirmed By", r.ConfirmedBy),
                new("Approved By", r.ApprovedBy),
                new("Approval Status", r.ApprovalStatus),
                new("Planned Start Date", r.PlannedStartDate, Date),
                new("Planned End Date", r.PlannedEndDate, Date),
                new("Actual Start Date", r.ActualStartDate, Date),
                new("Actual End Date", r.ActualEndDate, Date),
                new("Close Date", r.CloseDate, Date),
                new("Source Label", r.SourceLabel),
                new("Target Label", r.TargetLabel),
                new("Short Description", Rich(r.ShortDescription)),
                new("Description", Rich(r.Description)),
                new("Scope", Rich(r.Scope)),
                new("Created On (UTC)", r.CreatedOnUtc, DateTime),
                new("Updated On (UTC)", r.UpdatedOnUtc, DateTime)
            };
        }

        private static string[] Split(string group) => group.Split('|', StringSplitOptions.RemoveEmptyEntries);
    }
}
