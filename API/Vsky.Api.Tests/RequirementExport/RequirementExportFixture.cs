using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using Vsky.Api.Services.RequirementExport;
using Vsky.Models;

namespace Vsky.Api.Tests.RequirementExport
{
    // In-memory query roots. No database: every IQueryable is a LINQ-to-objects list.
    public sealed class InMemoryExportQueries : IRequirementExportQueries
    {
        public List<Requirement> RequirementList { get; } = new();
        public List<ProjectTask> TaskList { get; } = new();
        public List<TestCase> TestCaseList { get; } = new();
        public List<Issue> IssueList { get; } = new();
        public List<Timesheet> TimesheetList { get; } = new();
        public List<TimesheetLines> TimesheetLineList { get; } = new();
        public List<ProjectQuestionsAnswers> QuestionList { get; } = new();
        public List<ProjectActionItems> ActionItemList { get; } = new();
        public List<RequirementDataMapping> MappingList { get; } = new();
        public List<Notes> NoteList { get; } = new();

        public IQueryable<Requirement> Requirements => RequirementList.AsQueryable();
        public IQueryable<ProjectTask> Tasks => TaskList.AsQueryable();
        public IQueryable<TestCase> TestCases => TestCaseList.AsQueryable();
        public IQueryable<Issue> Issues => IssueList.AsQueryable();
        public IQueryable<Timesheet> Timesheets => TimesheetList.AsQueryable();
        public IQueryable<TimesheetLines> TimesheetLines => TimesheetLineList.AsQueryable();
        public IQueryable<ProjectQuestionsAnswers> Questions => QuestionList.AsQueryable();
        public IQueryable<ProjectActionItems> ActionItems => ActionItemList.AsQueryable();
        public IQueryable<RequirementDataMapping> Mappings => MappingList.AsQueryable();
        public IQueryable<Notes> Notes => NoteList.AsQueryable();
    }

    public sealed class FakeExportAuthorizer : IRequirementExportAuthorizer
    {
        public bool RequirementAllowed { get; set; } = true;
        public HashSet<string> DeniedLinks { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task<bool> CanAccessRequirementAsync(string userId, string siteId, string requirementId) => Task.FromResult(RequirementAllowed);

        public Task<bool> CanViewModuleAsync(string userId, string siteId, IReadOnlyList<string> menuLinks) =>
            Task.FromResult(menuLinks.Any(l => !DeniedLinks.Contains(l)));
    }

    /// <summary>
    /// Two requirements (A, B) in the same project and site, a third (X) in another site, plus unrelated
    /// project records. Every module has rows for A and for B, so leaks are detectable by the "B-"/"X-" prefixes.
    /// </summary>
    public sealed class ExportFixture
    {
        public const string SiteId = "site-1";
        public const string OtherSiteId = "site-2";
        public const string UserId = "user-1";
        public const string ProjectId = "proj-1";
        public const string ReqA = "req-A";
        public const string ReqB = "req-B";
        public const string ReqX = "req-X";

        public InMemoryExportQueries Q { get; } = new();
        public FakeExportAuthorizer Authorizer { get; } = new();

        public static Employee Emp(string first, string last) => new() { Person = new Person { FirstName = first, LastName = last } };
        public static DropDown Dd(string value) => new() { Id = "dd-" + value, DropDownValue = value };

        public ExportFixture(bool populateA = true)
        {
            Q.RequirementList.Add(new Requirement
            {
                Id = ReqA, SiteId = SiteId, ProjectId = ProjectId, RequirementNumber = 42, Title = "Login revamp",
                Project = new Project { Id = ProjectId, Name = "Alpha" },
                ProjectModule = new ProjectModule { Name = "Auth" },
                Status = Dd("New"), Priority = Dd("High"), RequirementType = Dd("Feature"),
                RequirementOwner = Emp("Ria", "Owner"),
                Description = "<p>First line</p><ul><li>one</li><li>two</li></ul><script>alert(1)</script>",
                IdentifiedDate = new DateTime(2026, 11, 1),
                CreatedOnUtc = new DateTime(2026, 11, 2, 10, 30, 0)
            });
            Q.RequirementList.Add(new Requirement { Id = ReqB, SiteId = SiteId, ProjectId = ProjectId, RequirementNumber = 43, Title = "B-Other requirement" });
            Q.RequirementList.Add(new Requirement { Id = ReqX, SiteId = OtherSiteId, ProjectId = "proj-x", RequirementNumber = 9, Title = "X-Foreign requirement" });
            Q.RequirementList.Add(new Requirement { Id = "req-del", SiteId = SiteId, ProjectId = ProjectId, RequirementNumber = 44, Title = "Deleted", Deleted = true });

            AddForRequirement(ReqB, SiteId, "B-");
            AddForRequirement(ReqX, OtherSiteId, "X-");
            if (populateA) AddForRequirement(ReqA, SiteId, "A-");

            // Same project, no requirement: shared project ownership is not an association.
            Q.TaskList.Add(new ProjectTask { Id = "t-proj", SiteId = SiteId, ProjectId = ProjectId, RequirementId = null, Name = "B-Project-only task" });
            Q.IssueList.Add(new Issue { Id = "i-proj", SiteId = SiteId, ProjectId = ProjectId, RequirementId = null, Name = "B-Project-only issue" });
            Q.NoteList.Add(new Notes { Id = "n-proj", SiteId = SiteId, Type = "Project", ModuleId = ProjectId, SubModuleId = ReqA, Note = "B-Project note with same SubModuleId" });
        }

        public void AddForRequirement(string reqId, string siteId, string p)
        {
            var taskId = p + "task-1";
            Q.TaskList.Add(new ProjectTask
            {
                Id = taskId, SiteId = siteId, ProjectId = ProjectId, RequirementId = reqId, ProjectTaskNumber = 2,
                Name = p + "Build login", Status = Dd("Open"), AssignedTo = Emp("Ann", "Dev"), EstimateTime = 4.5m,
                DueDate = new DateTime(2026, 12, 1), CreatedOnUtc = new DateTime(2026, 11, 3)
            });
            Q.TaskList.Add(new ProjectTask { Id = p + "task-0", SiteId = siteId, RequirementId = reqId, ProjectTaskNumber = 1, Name = p + "Design login" });
            Q.TaskList.Add(new ProjectTask { Id = p + "task-del", SiteId = siteId, RequirementId = reqId, ProjectTaskNumber = 3, Name = p + "Deleted task", Deleted = true });

            Q.TestCaseList.Add(new TestCase { Id = p + "tc", SiteId = siteId, RequirementId = reqId, TestCaseNumber = 5, Name = p + "Valid login", TestPlan = new TestPlan { Name = "Smoke" } });
            Q.IssueList.Add(new Issue { Id = p + "is", SiteId = siteId, RequirementId = reqId, IssueNumber = 8, Name = p + "Crash on login" });

            var sheetId = p + "ts";
            Q.TimesheetList.Add(new Timesheet { Id = sheetId, SiteId = siteId, TimesheetDate = new DateTime(2026, 11, 10), Employee = Emp("Tim", "Worker") });
            Q.TimesheetLineList.Add(new TimesheetLines
            {
                Id = p + "tl", TimesheetId = sheetId, ProjectTaskId = taskId, Hours = 2.25m, BillableHours = 2m,
                Description = p + "worked", Task = new ProjectTask { ProjectTaskNumber = 2, Name = p + "Build login" }
            });
            Q.TimesheetLineList.Add(new TimesheetLines { Id = p + "tl-del", TimesheetId = sheetId, ProjectTaskId = taskId, Hours = 9, Description = p + "deleted line", Deleted = true });
            // Line on a task of this requirement but recorded on a deleted timesheet.
            Q.TimesheetList.Add(new Timesheet { Id = sheetId + "-del", SiteId = siteId, Deleted = true });
            Q.TimesheetLineList.Add(new TimesheetLines { Id = p + "tl-delsheet", TimesheetId = sheetId + "-del", ProjectTaskId = taskId, Description = p + "deleted sheet" });

            Q.QuestionList.Add(new ProjectQuestionsAnswers { Id = p + "qa", SiteId = siteId, RequirementId = reqId, Title = p + "Which IdP?" });
            Q.ActionItemList.Add(new ProjectActionItems { Id = p + "ai", SiteId = siteId, RequirementId = reqId, Title = p + "Confirm IdP" });
            Q.MappingList.Add(new RequirementDataMapping { Id = p + "map", RequirementId = reqId, ProjectId = siteId == SiteId ? ProjectId : "proj-x", Source = p + "src", Target = p + "tgt" });
            Q.NoteList.Add(new Notes { Id = p + "note", SiteId = siteId, Type = RequirementExportDataSource.RequirementNoteType, SubModuleId = reqId, Note = p + "Requirement note" });
        }

        public RequirementExportService Service(IRequirementExportDataSource source = null) =>
            new(source ?? new RequirementExportDataSource(Q), Authorizer, new RequirementWorkbookWriter());

        public static ExcelPackage Open(byte[] bytes)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            return new ExcelPackage(new MemoryStream(bytes));
        }

        public static IEnumerable<string> AllText(ExcelPackage package) =>
            package.Workbook.Worksheets.SelectMany(ws => ws.Cells.Where(c => c.Value != null).Select(c => Convert.ToString(c.Value)));

        public static List<string> Column(ExcelWorksheet ws, string header)
        {
            var col = Enumerable.Range(1, ws.Dimension.End.Column).First(c => (string)ws.Cells[1, c].Value == header);
            return Enumerable.Range(2, ws.Dimension.End.Row - 1).Select(r => Convert.ToString(ws.Cells[r, col].Value)).ToList();
        }
    }
}
