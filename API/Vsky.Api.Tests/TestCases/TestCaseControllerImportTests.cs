using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Vsky.Api.AutoMapper;
using Vsky.Api.Controllers;
using Vsky.Api.Models;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.ProjectReleaseTrackings;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.TestCases;
using Xunit;

namespace Vsky.Api.Tests.TestCases
{
    public class TestCaseControllerImportTests
    {
        private readonly CaseFixture _fx = new();
        private readonly Mock<IAzureBlobImageServices> _blob = new();
        private readonly Mock<IProjectReleaseTrackingReqPlanTaskIssueMappingService> _mappings = new(MockBehavior.Strict);
        private readonly Mock<ITestCaseExecutionLogExecutionLogService> _executionLogs = new(MockBehavior.Strict);
        private readonly List<TestCase> _cases = new();
        private readonly List<Issue> _issues = new();
        private int _lastCaseNumber = 100;
        private int _lastIssueNumber = 7;

        private TestCaseController CreateController()
        {
            _blob.Setup(x => x.ProcessHtmlAndManageImagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string html, string _, string _, string _, string _) => html);
            _fx.TestCaseService.Setup(x => x.GetLastTestCaseNumber()).ReturnsAsync(() => _lastCaseNumber);
            _fx.TestCaseService.Setup(x => x.InsertTestCase(It.IsAny<TestCase>())).Callback<TestCase>(e =>
            {
                e.Id ??= Guid.NewGuid().ToString();
                _lastCaseNumber = e.TestCaseNumber;
                _cases.Add(e);
            });
            _fx.IssueService.Setup(x => x.GetLastIssueNumber()).ReturnsAsync(() => _lastIssueNumber);
            _fx.IssueService.Setup(x => x.InsertIssue(It.IsAny<Issue>())).Callback<Issue>(i =>
            {
                _lastIssueNumber = i.IssueNumber;
                _issues.Add(i);
                // Mirror the real lookup (case-insensitive name, same project): a later check in the same
                // request/transaction sees the issue.
                _fx.IssueService.Setup(x => x.GetIssueByName(CaseFixture.SiteId,
                        It.Is<string>(n => string.Equals(n, i.Name, StringComparison.OrdinalIgnoreCase)), i.ProjectId, null))
                    .ReturnsAsync(i);
            });

            var mapper = new MapperConfiguration(c => c.AddProfile<AutoMapperConfiguration>()).CreateMapper();
            var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

            var controller = new TestCaseController(
                new GlobalVariable { SiteId = CaseFixture.SiteId },
                mapper,
                _fx.TestCaseService.Object,
                _fx.CommonService.Object,
                _fx.IssueService.Object,
                _fx.SiteService.Object,
                _blob.Object,
                _mappings.Object,
                _executionLogs.Object,
                new Mock<ISitesModifiedLogsService>(MockBehavior.Strict).Object,
                _fx.DropDownService.Object,
                db,
                _fx.CreateService());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, CaseFixture.UserId) }, "test"))
                }
            };
            return controller;
        }

        private async Task<List<TestCaseImportRowModel>> PreviewRows(params TestCaseImportRowModel[] rows)
        {
            // Confirmation submits the canonical (ID) rows returned by preview.
            var preview = await _fx.CreateService().ValidateAsync(rows, CaseFixture.SiteId, CaseFixture.UserId);
            Assert.True(preview.CanConfirm, string.Join("; ", preview.Rows.SelectMany(r => r.Errors).Select(e => e.Message)));
            return preview.Rows.Select(r => (TestCaseImportRowModel)r with { }).ToList();
        }

        [Fact]
        public async Task Preview_WithFailRows_WritesNothing_AndProcessesNoImages()
        {
            var cells = CaseWorkbook.ValidCells("Broken login");
            cells[7] = "Fail";
            var file = CaseWorkbook.ToFormFile(CaseWorkbook.Build(CaseWorkbook.Headers, cells));

            var result = await CreateController().PreviewImport(new TestCaseImportUploadModel { File = file });

            var preview = Assert.IsType<TestCaseImportPreviewModel>(Assert.IsType<OkObjectResult>(result).Value);
            Assert.True(preview.CanConfirm);
            Assert.Equal("create", preview.Rows[0].FailIssue);
            _fx.VerifyNoWrites();
            _blob.VerifyNoOtherCalls();
            _mappings.VerifyNoOtherCalls();
            _executionLogs.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Confirm_CreatesBaseCases_WithManualParity_AndNoReleaseData()
        {
            var rows = await PreviewRows(CaseFixture.ValidRow(2, "Case A", "Pass"));

            var result = await CreateController().ConfirmImport(new TestCaseImportConfirmModel { Rows = rows });

            var body = Assert.IsType<TestCaseImportResultModel>(Assert.IsType<OkObjectResult>(result).Value);
            Assert.True(body.Success);
            Assert.Equal(1, body.ImportedCount);
            Assert.Equal(0, body.IssuesCreated);
            Assert.Equal(101, body.Created[0].TestCaseNumber);

            var c = _cases.Single();
            Assert.Equal(CaseFixture.SiteId, c.SiteId);
            Assert.Equal(CaseFixture.UserId, c.CreatedById);
            Assert.Equal(CaseFixture.UserId, c.UpdatedById);
            Assert.Equal(CaseFixture.SiteNow, c.CreatedOnUtc);
            Assert.Equal(("proj-1", "plan-1", "mod-1", "req-1"), (c.ProjectId, c.PlanId, c.ProjectModuleId, c.RequirementId));
            Assert.Equal(("area-1", "ws-1", "st-pass"), (c.AreaId, c.WorkspaceId, c.StatusId));
            Assert.Equal(((string)null, "emp-2"), (c.EmployeeId, c.TestedBy)); // Assigned To only for Fail
            Assert.Equal(new DateTime(2026, 11, 15), c.TestedDate);
            Assert.Equal("<p>1. Open<br>2. Login</p>", c.Steps);
            Assert.Null(c.TestResult);
            Assert.Empty(_issues);
            _blob.Verify(x => x.ProcessHtmlAndManageImagesAsync(It.IsAny<string>(), "Acme", "test-case", "101", null), Times.Exactly(4));
            _mappings.VerifyNoOtherCalls();
            _executionLogs.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Confirm_FailWithoutMatchingIssue_CreatesBugMediumNewFromTestPlan()
        {
            var rows = await PreviewRows(CaseFixture.ValidRow(2, "Broken login", "Fail"));

            var result = await CreateController().ConfirmImport(new TestCaseImportConfirmModel { Rows = rows });

            var body = Assert.IsType<TestCaseImportResultModel>(Assert.IsType<OkObjectResult>(result).Value);
            Assert.Equal(1, body.IssuesCreated);
            Assert.True(body.Created[0].IssueCreated);
            Assert.Equal(8, body.Created[0].IssueNumber);

            var issue = _issues.Single();
            Assert.Equal(("it-bug", "ip-medium", "is-new"), (issue.TypeId, issue.PriorityId, issue.StatusId));
            Assert.Equal(_cases.Single().Id, issue.TestCaseId);
            Assert.Equal(("proj-1", "mod-1", "req-1"), (issue.ProjectId, issue.ProjectModuleId, issue.RequirementId));
            Assert.Equal(("emp-1", "area-1", "ws-1"), (issue.EmployeeId, issue.AreaId, issue.WorkspaceId));
            Assert.Equal(CaseFixture.EmployeeId, issue.ReportedById);
            Assert.Equal("Broken login", issue.Name);
        }

        [Fact]
        public async Task Confirm_FailWithExistingIssue_DoesNotCreateAnother_AndBatchCreatesOnlyOnce()
        {
            _fx.IssueService.Setup(x => x.GetIssueByName(CaseFixture.SiteId, "Known", "proj-1", null)).ReturnsAsync(new Issue { Name = "Known" });
            // Row 4 shares project + name (issue scope) with row 3 but uses another plan, so it is a separate case.
            _fx.TestPlanService.Setup(x => x.GetAllTestPlansListForDropdown(CaseFixture.SiteId, "proj-1"))
                .ReturnsAsync(new List<CommonDropDown> { new() { Value = "plan-1", Text = "Smoke Plan" }, new() { Value = "plan-9", Text = "Regression" } });
            var second = CaseFixture.ValidRow(4, "New One", "Fail");
            second.PlanId = "Regression";

            var rows = await PreviewRows(
                CaseFixture.ValidRow(2, "Known", "Fail"),
                CaseFixture.ValidRow(3, "new one", "Fail"),
                second);

            var result = await CreateController().ConfirmImport(new TestCaseImportConfirmModel { Rows = rows });

            var body = Assert.IsType<TestCaseImportResultModel>(Assert.IsType<OkObjectResult>(result).Value);
            Assert.Equal(3, body.ImportedCount);
            Assert.Equal(1, body.IssuesCreated);
            Assert.Equal(new[] { false, true, false }, body.Created.Select(c => c.IssueCreated).ToArray());
        }

        [Fact]
        public async Task Confirm_TamperedOrStaleRows_AreRevalidated_AndNothingIsWritten()
        {
            var rows = await PreviewRows(CaseFixture.ValidRow(2), CaseFixture.ValidRow(3, "Case B"));
            rows[1] = rows[1] with { PlanId = "plan-2" }; // plan of another project

            var result = await CreateController().ConfirmImport(new TestCaseImportConfirmModel { Rows = rows });

            var body = Assert.IsType<TestCaseImportResultModel>(Assert.IsType<BadRequestObjectResult>(result).Value);
            Assert.False(body.Success);
            Assert.Equal(0, body.ImportedCount);
            Assert.Contains(body.Validation.Rows[1].Errors, e => e.Field == "planId");
            _fx.VerifyNoWrites();
        }

        [Fact]
        public async Task Confirm_DuplicateCreatedSincePreview_BlocksBatch()
        {
            var rows = await PreviewRows(CaseFixture.ValidRow(2));
            _fx.TestCaseService.Setup(x => x.GetTestCaseByName(CaseFixture.SiteId, "proj-1", "plan-1", "mod-1", "Case A", null)).ReturnsAsync(new TestCase());

            var result = await CreateController().ConfirmImport(new TestCaseImportConfirmModel { Rows = rows });

            Assert.IsType<BadRequestObjectResult>(result);
            _fx.VerifyNoWrites();
        }

        [Fact]
        public async Task Confirm_RuntimeFailure_ReportsZero_NotPartialSuccess()
        {
            var rows = await PreviewRows(CaseFixture.ValidRow(2), CaseFixture.ValidRow(3, "Case B"));
            var controller = CreateController();
            int calls = 0;
            _fx.TestCaseService.Setup(x => x.InsertTestCase(It.IsAny<TestCase>())).Callback<TestCase>(_ =>
            {
                if (++calls == 2) throw new InvalidOperationException("db down");
            });

            var result = await controller.ConfirmImport(new TestCaseImportConfirmModel { Rows = rows });

            var body = Assert.IsType<TestCaseImportResultModel>(Assert.IsType<BadRequestObjectResult>(result).Value);
            Assert.False(body.Success);
            Assert.Equal(0, body.ImportedCount);
            Assert.Equal(0, body.IssuesCreated);
            Assert.Empty(body.Created);
            Assert.Contains("No test cases were imported", body.Message);
        }

        [Fact]
        public async Task ManualCreate_Unchanged_IncludingFailIssue()
        {
            var model = new TestCaseModel
            {
                ProjectId = "proj-1", PlanId = "plan-1", ProjectModuleId = "mod-1", RequirementId = "undefined",
                Name = "Manual fail", StatusId = "st-fail", TestedBy = "emp-2", TestedDateStr = "11/10/2026", Description = "<p>d</p>",
                SiteId = "spoofed", TestCaseNumber = 999
            };

            var result = await CreateController().CreateTestCase(model);

            var entity = Assert.IsType<TestCase>(Assert.IsType<OkObjectResult>(result).Value);
            Assert.Equal(101, entity.TestCaseNumber);
            Assert.Equal(CaseFixture.SiteId, entity.SiteId);
            Assert.Null(entity.RequirementId);
            Assert.Equal(new DateTime(2026, 11, 10), entity.TestedDate);
            Assert.Single(_issues);
        }

        [Fact]
        public async Task ManualCreate_Duplicate_StillRejected()
        {
            _fx.TestCaseService.Setup(x => x.GetTestCaseByName(CaseFixture.SiteId, "proj-1", "plan-1", "mod-1", "Dup", null)).ReturnsAsync(new TestCase());
            var result = await CreateController().CreateTestCase(new TestCaseModel { ProjectId = "proj-1", PlanId = "plan-1", ProjectModuleId = "mod-1", Name = "Dup" });
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(_cases);
        }
    }
}
