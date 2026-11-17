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
using Vsky.Services.Common;
using Vsky.Services.Sites;
using Xunit;

namespace Vsky.Api.Tests.TestPlans
{
    public class TestPlanControllerImportTests
    {
        private readonly ImportFixture _fx = new();
        private readonly Mock<ISiteService> _siteService = new();
        private readonly Mock<IAzureBlobImageServices> _blob = new();
        private readonly List<TestPlan> _inserted = new();
        private readonly DateTime _now = new(2026, 11, 17, 10, 0, 0);
        private int _lastNumber = 41;

        private TestPlanController CreateController()
        {
            _siteService.Setup(x => x.GetById(ImportFixture.SiteId)).ReturnsAsync(new Site { Id = ImportFixture.SiteId, Name = "Acme", TimeZone = "UTC" });
            _siteService.Setup(x => x.GetDateTime(It.IsAny<string>())).Returns(_now);
            _blob.Setup(x => x.ProcessHtmlAndManageImagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string html, string _, string _, string _, string _) => html);
            _fx.TestPlanService.Setup(x => x.GetLastTestPlanNumber()).ReturnsAsync(() => _lastNumber);
            _fx.TestPlanService.Setup(x => x.InsertTestPlan(It.IsAny<TestPlan>())).Callback<TestPlan>(e =>
            {
                e.Id ??= Guid.NewGuid().ToString();
                _lastNumber = e.TestPlanNumber;
                _inserted.Add(e);
            });

            var mapper = new MapperConfiguration(c => c.AddProfile<AutoMapperConfiguration>()).CreateMapper();
            var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

            var controller = new TestPlanController(
                new GlobalVariable { SiteId = ImportFixture.SiteId },
                mapper,
                _fx.TestPlanService.Object,
                _siteService.Object,
                db,
                _fx.CommonService.Object,
                _blob.Object,
                _fx.CreateService());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, ImportFixture.UserId) }, "test"))
                }
            };
            return controller;
        }

        [Fact]
        public async Task Preview_ReturnsRows_AndPerformsNoWritesOrImageProcessing()
        {
            var file = WorkbookBuilder.ToFormFile(WorkbookBuilder.Build(WorkbookBuilder.Headers, WorkbookBuilder.ValidCells("A"), WorkbookBuilder.ValidCells("B")));

            var result = await CreateController().PreviewImport(new TestPlanImportUploadModel { File = file });

            var preview = Assert.IsType<TestPlanImportPreviewModel>(Assert.IsType<OkObjectResult>(result).Value);
            Assert.True(preview.CanConfirm);
            Assert.Equal(2, preview.TotalRows);
            _fx.VerifyNoWrites();
            _blob.VerifyNoOtherCalls();
            _siteService.Verify(x => x.GetById(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Preview_StructuralError_ReturnsFileErrors_AndCannotConfirm()
        {
            var file = WorkbookBuilder.ToFormFile(WorkbookBuilder.Build(new[] { "Name" }, new object[] { "x" }));
            var result = await CreateController().PreviewImport(new TestPlanImportUploadModel { File = file });

            var preview = Assert.IsType<TestPlanImportPreviewModel>(Assert.IsType<OkObjectResult>(result).Value);
            Assert.False(preview.CanConfirm);
            Assert.NotEmpty(preview.FileErrors);
            Assert.Empty(preview.Rows);
            _fx.VerifyNoWrites();
        }

        [Fact]
        public async Task Confirm_ValidBatch_CreatesThroughManualPath_WithParity()
        {
            var row = ImportFixture.ValidRow(2);
            row.Description = "Step 1\n<b>bold?</b>";
            var model = new TestPlanImportConfirmModel { Rows = { row, ImportFixture.ValidRow(3, "Plan B") } };

            var result = await CreateController().ConfirmImport(model);

            var body = Assert.IsType<TestPlanImportResultModel>(Assert.IsType<OkObjectResult>(result).Value);
            Assert.True(body.Success);
            Assert.Equal(2, body.ImportedCount);
            Assert.Equal(new[] { 42, 43 }, body.Created.Select(c => c.TestPlanNumber));
            Assert.Equal(new[] { 2, 3 }, body.Created.Select(c => c.RowNumber));

            var first = _inserted[0];
            Assert.Equal(ImportFixture.SiteId, first.SiteId);
            Assert.Equal(ImportFixture.UserId, first.CreatedById);
            Assert.Equal(ImportFixture.UserId, first.UpdatedById);
            Assert.Equal(_now, first.CreatedOnUtc);
            Assert.Equal(_now, first.UpdatedOnUtc);
            Assert.Equal("proj-1", first.ProjectId);
            Assert.Equal("area-1", first.AreaId);
            Assert.Equal("ws-1", first.WorkspaceId);
            Assert.Equal("emp-1", first.PlanMakerId);
            Assert.Equal("emp-2", first.PlanReviewerId);
            Assert.Equal("<p>Step 1<br>&lt;b&gt;bold?&lt;/b&gt;</p>", first.Description);
            // Description goes through the same image-processing call as manual creation.
            _blob.Verify(x => x.ProcessHtmlAndManageImagesAsync(It.IsAny<string>(), "Acme", "test-plan", "42", null), Times.Once);
        }

        [Fact]
        public async Task Confirm_IgnoresTamperedServerOwnedValues_AndRevalidates()
        {
            // Client changed a project to one outside the user's scope after preview.
            var tampered = ImportFixture.ValidRow(2);
            tampered.ProjectId = "proj-other";

            var result = await CreateController().ConfirmImport(new TestPlanImportConfirmModel { Rows = { ImportFixture.ValidRow(3, "OK"), tampered } });

            var body = Assert.IsType<TestPlanImportResultModel>(Assert.IsType<BadRequestObjectResult>(result).Value);
            Assert.False(body.Success);
            Assert.Equal(0, body.ImportedCount);
            Assert.Contains(body.Validation.Rows.Single(r => r.RowNumber == 2).Errors, e => e.Field == "projectId");
            _fx.TestPlanService.Verify(x => x.InsertTestPlan(It.IsAny<TestPlan>()), Times.Never);
        }

        [Fact]
        public async Task Confirm_DuplicateCreatedSincePreview_BlocksWholeBatch()
        {
            _fx.TestPlanService.Setup(x => x.GetTestPlanByName(ImportFixture.SiteId, "Plan B", "proj-1", null)).ReturnsAsync(new TestPlan { Id = "existing" });

            var result = await CreateController().ConfirmImport(new TestPlanImportConfirmModel { Rows = { ImportFixture.ValidRow(2), ImportFixture.ValidRow(3, "Plan B") } });

            Assert.IsType<BadRequestObjectResult>(result);
            _fx.TestPlanService.Verify(x => x.InsertTestPlan(It.IsAny<TestPlan>()), Times.Never);
        }

        [Fact]
        public async Task Confirm_RuntimeFailure_ReportsNoImport_NotPartialSuccess()
        {
            var controller = CreateController();
            int calls = 0;
            _fx.TestPlanService.Setup(x => x.InsertTestPlan(It.IsAny<TestPlan>())).Callback<TestPlan>(_ =>
            {
                if (++calls == 2) throw new InvalidOperationException("db down");
            });

            var result = await controller.ConfirmImport(new TestPlanImportConfirmModel { Rows = { ImportFixture.ValidRow(2), ImportFixture.ValidRow(3, "Plan B") } });

            var body = Assert.IsType<TestPlanImportResultModel>(Assert.IsType<BadRequestObjectResult>(result).Value);
            Assert.False(body.Success);
            Assert.Equal(0, body.ImportedCount);
            Assert.Empty(body.Created);
            Assert.Contains("No test plans were imported", body.Message);
        }

        [Fact]
        public async Task Confirm_EmptyBatch_IsRejected()
        {
            var result = await CreateController().ConfirmImport(new TestPlanImportConfirmModel());
            Assert.IsType<BadRequestObjectResult>(result);
            _fx.VerifyNoWrites();
        }

        [Fact]
        public async Task ManualCreate_BehaviourUnchanged_AfterHelperExtraction()
        {
            var controller = CreateController();
            var model = new TestPlanModel
            {
                ProjectId = "proj-1", Name = "Manual", PlanMakerId = "emp-1", PlanReviewerId = "emp-2",
                Description = "<p>x</p>", SiteId = "spoofed-site", TestPlanNumber = 999
            };

            var result = await controller.CreateTestPlan(model);

            var entity = Assert.IsType<TestPlan>(Assert.IsType<OkObjectResult>(result).Value);
            Assert.Equal(42, entity.TestPlanNumber);
            Assert.Equal(ImportFixture.SiteId, entity.SiteId);
            Assert.Equal(ImportFixture.UserId, entity.CreatedById);
            Assert.Single(_inserted);
        }

        [Fact]
        public async Task ManualCreate_Duplicate_StillReturnsExistingMessage()
        {
            _fx.TestPlanService.Setup(x => x.GetTestPlanByName(ImportFixture.SiteId, "Dup", "proj-1", null)).ReturnsAsync(new TestPlan());
            var result = await CreateController().CreateTestPlan(new TestPlanModel { ProjectId = "proj-1", Name = "Dup" });
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(_inserted);
        }
    }
}
