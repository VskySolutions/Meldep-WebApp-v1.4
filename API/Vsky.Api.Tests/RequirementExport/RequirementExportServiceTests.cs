using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Vsky.Api.Services.RequirementExport;
using Vsky.Models;
using Xunit;
using static Vsky.Api.Tests.RequirementExport.ExportFixture;

namespace Vsky.Api.Tests.RequirementExport
{
    public class RequirementExportServiceTests
    {
        private static readonly string[] AllModuleSheets = { "Tasks", "Test Cases", "Issues", "Timesheets", "Q&A", "Action Items", "Mappings", "Notes" };

        private static async Task<OfficeOpenXml.ExcelPackage> Export(ExportFixture fx, string id = ReqA)
        {
            var file = await fx.Service().ExportAsync(SiteId, UserId, id);
            return Open(file.Content);
        }

        [Fact]
        public async Task Workbook_starts_with_requirement_info_then_every_named_module_in_order()
        {
            using var package = await Export(new ExportFixture());

            var names = package.Workbook.Worksheets.Select(w => w.Name).ToList();
            Assert.Equal(new[] { "Requirement Info" }.Concat(AllModuleSheets), names);
        }

        [Fact]
        public async Task Requirement_info_lists_basic_fields_with_readable_labels()
        {
            using var package = await Export(new ExportFixture());
            var info = package.Workbook.Worksheets[0];

            Assert.Equal("Field", info.Cells[1, 1].Value);
            Assert.Equal("Value", info.Cells[1, 2].Value);
            var map = Enumerable.Range(2, info.Dimension.End.Row - 1)
                .ToDictionary(r => (string)info.Cells[r, 1].Value, r => info.Cells[r, 2].Value);

            Assert.Equal(42d, Convert.ToDouble(map["Requirement #"]));
            Assert.Equal("Login revamp", map["Title"]);
            Assert.Equal("Alpha", map["Project"]);
            Assert.Equal("Auth", map["Module"]);
            Assert.Equal("New", map["Status"]);
            Assert.Equal("Ria Owner", map["Requirement Owner"]);
            Assert.Equal(new DateTime(2026, 11, 1), DateTime.FromOADate(Convert.ToDouble(map["Identified Date"])));
            Assert.Equal("First line\n\n- one\n- two", map["Description"]);
            Assert.Null(map["Close Date"]);
        }

        [Fact]
        public async Task Every_sheet_contains_only_the_selected_requirements_records()
        {
            using var package = await Export(new ExportFixture());

            var text = AllText(package).ToList();
            Assert.DoesNotContain(text, t => t.StartsWith("B-") || t.StartsWith("X-"));
            Assert.Contains("A-Build login", text);
            Assert.Contains("A-Requirement note", text);
        }

        [Fact]
        public async Task Module_sheets_include_all_matching_records_but_not_deleted_ones()
        {
            using var package = await Export(new ExportFixture());
            var tasks = package.Workbook.Worksheets["Tasks"];

            Assert.Equal(new[] { "A-Design login", "A-Build login" }, Column(tasks, "Task Name")); // ordered by number
            var timesheets = package.Workbook.Worksheets["Timesheets"];
            Assert.Equal(new[] { "A-worked" }, Column(timesheets, "Description"));
        }

        [Fact]
        public async Task Exports_every_record_not_just_the_first_page()
        {
            var fx = new ExportFixture();
            for (var i = 0; i < 1200; i++)
                fx.Q.TaskList.Add(new ProjectTask { Id = "bulk-" + i, SiteId = SiteId, RequirementId = ReqA, ProjectTaskNumber = 100 + i, Name = "bulk " + i });

            using var package = await Export(fx);

            Assert.Equal(1202, package.Workbook.Worksheets["Tasks"].Dimension.End.Row - 1);
        }

        [Fact]
        public async Task Modules_without_records_for_this_requirement_are_omitted()
        {
            var fx = new ExportFixture(populateA: false);
            fx.Q.IssueList.Add(new Issue { Id = "only-issue", SiteId = SiteId, RequirementId = ReqA, IssueNumber = 1, Name = "A-only issue" });

            using var package = await Export(fx);

            Assert.Equal(new[] { "Requirement Info", "Issues" }, package.Workbook.Worksheets.Select(w => w.Name));
        }

        [Fact]
        public async Task Requirement_with_no_linked_data_exports_only_requirement_info()
        {
            using var package = await Export(new ExportFixture(populateA: false));

            Assert.Equal(new[] { "Requirement Info" }, package.Workbook.Worksheets.Select(w => w.Name));
        }

        [Theory]
        [InlineData(ReqX)]       // other site
        [InlineData("req-del")]  // soft-deleted
        [InlineData("missing")]
        [InlineData("")]
        public async Task Unknown_deleted_or_other_site_requirement_is_not_found(string id)
        {
            var ex = await Assert.ThrowsAsync<RequirementExportException>(() => new ExportFixture().Service().ExportAsync(SiteId, UserId, id));
            Assert.Equal(RequirementExportFailure.NotFound, ex.Failure);
        }

        [Fact]
        public async Task Requirement_outside_users_project_scope_is_not_found()
        {
            var fx = new ExportFixture();
            fx.Authorizer.RequirementAllowed = false;

            var ex = await Assert.ThrowsAsync<RequirementExportException>(() => fx.Service().ExportAsync(SiteId, UserId, ReqA));
            Assert.Equal(RequirementExportFailure.NotFound, ex.Failure);
        }

        [Fact]
        public async Task Missing_module_permission_rejects_the_whole_export_before_reading_module_data()
        {
            var fx = new ExportFixture();
            fx.Authorizer.DeniedLinks.Add(Authorization.MenuLinks.Issue);
            var source = new Mock<IRequirementExportDataSource>(MockBehavior.Strict);
            source.Setup(x => x.GetRequirementAsync(SiteId, ReqA, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RequirementInfoRow { Id = ReqA, SiteId = SiteId });

            var ex = await Assert.ThrowsAsync<RequirementExportException>(() => fx.Service(source.Object).ExportAsync(SiteId, UserId, ReqA));

            Assert.Equal(RequirementExportFailure.Forbidden, ex.Failure);
            Assert.Contains("Issues", ex.Message);
            source.VerifyAll(); // strict mock: no module read was attempted
        }

        [Fact]
        public async Task Retrieval_failure_is_an_error_not_an_empty_sheet()
        {
            var fx = new ExportFixture();
            var source = new Mock<IRequirementExportDataSource>();
            source.Setup(x => x.GetRequirementAsync(SiteId, ReqA, It.IsAny<CancellationToken>())).ReturnsAsync(new RequirementInfoRow { Id = ReqA, SiteId = SiteId });
            source.Setup(x => x.GetTasksAsync(It.IsAny<RequirementExportScope>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<RequirementTaskRow>());
            source.Setup(x => x.GetTestCasesAsync(It.IsAny<RequirementExportScope>(), It.IsAny<CancellationToken>())).ThrowsAsync(new TimeoutException("db timeout"));

            await Assert.ThrowsAsync<TimeoutException>(() => fx.Service(source.Object).ExportAsync(SiteId, UserId, ReqA));
        }

        [Fact]
        public async Task Scope_passed_to_module_reads_is_server_derived()
        {
            var fx = new ExportFixture();
            var captured = new List<RequirementExportScope>();
            var inner = new RequirementExportDataSource(fx.Q);
            var source = new Mock<IRequirementExportDataSource>();
            source.Setup(x => x.GetRequirementAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns((string s, string r, CancellationToken c) => inner.GetRequirementAsync(s, r, c));
            source.Setup(x => x.GetNotesAsync(It.IsAny<RequirementExportScope>(), It.IsAny<CancellationToken>()))
                .Callback((RequirementExportScope s, CancellationToken _) => captured.Add(s))
                .ReturnsAsync(new List<RequirementNoteRow>());
            foreach (var setup in new Action[]
            {
                () => source.Setup(x => x.GetTasksAsync(It.IsAny<RequirementExportScope>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<RequirementTaskRow>()),
                () => source.Setup(x => x.GetTestCasesAsync(It.IsAny<RequirementExportScope>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<RequirementTestCaseRow>()),
                () => source.Setup(x => x.GetIssuesAsync(It.IsAny<RequirementExportScope>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<RequirementIssueRow>()),
                () => source.Setup(x => x.GetTimesheetsAsync(It.IsAny<RequirementExportScope>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<RequirementTimesheetRow>()),
                () => source.Setup(x => x.GetQuestionsAsync(It.IsAny<RequirementExportScope>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<RequirementQuestionRow>()),
                () => source.Setup(x => x.GetActionItemsAsync(It.IsAny<RequirementExportScope>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<RequirementActionItemRow>()),
                () => source.Setup(x => x.GetMappingsAsync(It.IsAny<RequirementExportScope>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<RequirementMappingRow>()),
            }) setup();

            await fx.Service(source.Object).ExportAsync(SiteId, UserId, "  " + ReqA + " ");

            var scope = Assert.Single(captured);
            Assert.Equal(SiteId, scope.SiteId);
            Assert.Equal(ReqA, scope.RequirementId);
            Assert.Equal(ProjectId, scope.ProjectId);
        }

        [Fact]
        public async Task File_name_uses_the_requirement_number()
        {
            var file = await new ExportFixture().Service().ExportAsync(SiteId, UserId, ReqA);

            Assert.Equal("Requirement-42.xlsx", file.FileName);
            Assert.Equal("Requirement-req_A_.xlsx", RequirementExportService.BuildFileName(0, "req/A?"));
        }
    }
}
