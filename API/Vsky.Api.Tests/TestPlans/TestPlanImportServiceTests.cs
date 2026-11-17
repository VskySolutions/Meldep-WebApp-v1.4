using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using OfficeOpenXml;
using Vsky.Api.Models;
using Vsky.Models;
using Xunit;

namespace Vsky.Api.Tests.TestPlans
{
    public class TestPlanImportServiceTests
    {
        private readonly ImportFixture _fx = new();

        #region Template
        [Fact]
        public async Task Template_ContainsExactlyTheSevenEditableFields_InFormOrder()
        {
            var bytes = await _fx.CreateService().BuildTemplateAsync(ImportFixture.SiteId, ImportFixture.UserId);

            using var package = new ExcelPackage(new MemoryStream(bytes));
            var sheet = package.Workbook.Worksheets[TestPlanImportField.DataSheetName];
            var headers = Enumerable.Range(1, sheet.Dimension.End.Column).Select(c => sheet.Cells[1, c].Text).ToArray();

            Assert.Equal(new[] { "Project Name", "Name", "Area", "Workspace", "Plan Maker", "Plan Reviewer", "Description" }, headers);
            Assert.DoesNotContain(headers, h => h.Contains("Site") || h.Contains("Number") || h.Contains("Created") || h.Contains("Updated") || h == "Id");
            Assert.Equal(1, sheet.Dimension.End.Row); // headers only, no data
            _fx.VerifyNoWrites();
        }

        [Fact]
        public void FieldMetadata_RequiredFlags_MatchForm()
        {
            var required = TestPlanImportField.All.Where(f => f.Required).Select(f => f.Key).ToArray();
            Assert.Equal(new[] { "projectId", "name", "planMakerId", "planReviewerId" }, required);
        }

        [Fact]
        public async Task Template_GuidanceSheets_ListOnlyScopedLookups()
        {
            var bytes = await _fx.CreateService().BuildTemplateAsync(ImportFixture.SiteId, ImportFixture.UserId);
            using var package = new ExcelPackage(new MemoryStream(bytes));

            string[] Col(string sheet, int col) => Enumerable.Range(2, package.Workbook.Worksheets[sheet].Dimension.End.Row - 1)
                .Select(r => package.Workbook.Worksheets[sheet].Cells[r, col].Text).ToArray();

            Assert.Equal(new[] { "Alpha Project", "Beta Project" }, Col("Projects", 1));
            Assert.Equal(new[] { "proj-1", "proj-2" }, Col("Projects", 2));
            Assert.Equal(new[] { "Asha Rao", "Ravi Kumar" }, Col("Employees", 1));
            Assert.Equal(new[] { "emp-1", "emp-2" }, Col("Employees", 2));
            Assert.Equal(new[] { "Payments" }, Col("Areas", 1));
            Assert.Equal(new[] { "Web" }, Col("Workspaces", 1));
            Assert.NotNull(package.Workbook.Worksheets["Instructions"]);
        }

        [Fact]
        public async Task Template_LookupColumns_AreInCellDropdowns_BoundToFormLists()
        {
            var bytes = await _fx.CreateService().BuildTemplateAsync(ImportFixture.SiteId, ImportFixture.UserId);
            using var package = new ExcelPackage(new MemoryStream(bytes));
            var sheet = package.Workbook.Worksheets[TestPlanImportField.DataSheetName];

            var byColumn = sheet.DataValidations
                .OfType<OfficeOpenXml.DataValidation.Contracts.IExcelDataValidationList>()
                .ToDictionary(v => v.Address.Start.Column, v => v);

            // Project Name(1), Area(3), Workspace(4), Plan Maker(5), Plan Reviewer(6); Name(2)/Description(7) stay free text.
            Assert.Equal(new[] { 1, 3, 4, 5, 6 }, byColumn.Keys.OrderBy(k => k).ToArray());
            Assert.Equal("'Projects'!$A$2:$A$3", byColumn[1].Formula.ExcelFormula);
            Assert.Equal("'Areas'!$A$2:$A$2", byColumn[3].Formula.ExcelFormula);
            Assert.Equal("'Workspaces'!$A$2:$A$2", byColumn[4].Formula.ExcelFormula);
            Assert.Equal("'Employees'!$A$2:$A$3", byColumn[5].Formula.ExcelFormula);
            Assert.Equal("'Employees'!$A$2:$A$3", byColumn[6].Formula.ExcelFormula);
            foreach (var v in byColumn.Values)
            {
                Assert.Equal(2, v.Address.Start.Row);
                Assert.Equal(TestPlanImportField.MaxDataRows + 1, v.Address.End.Row);
                Assert.True(v.ShowErrorMessage);
                Assert.Equal(OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop, v.ErrorStyle);
            }
        }

        [Fact]
        public async Task Template_RoundTrip_FilledWithDropdownOptions_PreviewsValid()
        {
            var svc = _fx.CreateService();
            var bytes = await svc.BuildTemplateAsync(ImportFixture.SiteId, ImportFixture.UserId);

            byte[] filled;
            using (var package = new ExcelPackage(new MemoryStream(bytes)))
            {
                var sheet = package.Workbook.Worksheets[TestPlanImportField.DataSheetName];
                var values = new object[] { "Beta Project", "Round trip", "Payments", "Web", "Ravi Kumar", "Asha Rao", "Notes" };
                for (int c = 0; c < values.Length; c++) sheet.Cells[2, c + 1].Value = values[c];
                filled = package.GetAsByteArray();
            }

            var parsed = svc.ParseWorkbook(WorkbookBuilder.ToFormFile(filled));
            Assert.Empty(parsed.FileErrors);
            var preview = await svc.ValidateAsync(parsed.Rows, ImportFixture.SiteId, ImportFixture.UserId, parsed.CellErrors);

            Assert.True(preview.CanConfirm);
            Assert.Equal("proj-2", preview.Rows[0].ProjectId);
            Assert.Equal("emp-2", preview.Rows[0].PlanMakerId);
            Assert.Equal("emp-1", preview.Rows[0].PlanReviewerId);
        }

        [Fact]
        public async Task Template_DuplicateLabels_AreMadeUnique_WithRecordId()
        {
            _fx.Employees.Add(new Employee { Id = "emp-3", Person = new Person { FullName = "asha rao" } });
            var bytes = await _fx.CreateService().BuildTemplateAsync(ImportFixture.SiteId, ImportFixture.UserId);
            using var package = new ExcelPackage(new MemoryStream(bytes));
            var sheet = package.Workbook.Worksheets["Employees"];
            var options = Enumerable.Range(2, sheet.Dimension.End.Row - 1).Select(r => sheet.Cells[r, 1].Text).ToArray();

            Assert.Contains("Asha Rao (emp-1)", options);
            Assert.Contains("asha rao (emp-3)", options);
            Assert.Contains("Ravi Kumar", options);
        }

        [Fact]
        public async Task Template_EmptyLookup_HasNoDropdown()
        {
            _fx.DropDownTypeService.Setup(x => x.GetDropDownType(ImportFixture.SiteId, "Workspace")).ReturnsAsync((DropDownType)null);
            var bytes = await _fx.CreateService().BuildTemplateAsync(ImportFixture.SiteId, ImportFixture.UserId);
            using var package = new ExcelPackage(new MemoryStream(bytes));
            var cols = package.Workbook.Worksheets[TestPlanImportField.DataSheetName].DataValidations.Select(v => v.Address.Start.Column).ToArray();
            Assert.DoesNotContain(4, cols);
        }
        #endregion

        #region Parse / structure
        [Fact]
        public void Parse_ValidWorkbook_ReturnsCanonicalRowsWithSourceRowNumbers()
        {
            var file = WorkbookBuilder.ToFormFile(WorkbookBuilder.Build(WorkbookBuilder.Headers,
                WorkbookBuilder.ValidCells("  Plan A  "),
                new object[] { null, null, null, null, null, null, null }, // blank row is skipped
                WorkbookBuilder.ValidCells("Plan B")));

            var result = _fx.CreateService().ParseWorkbook(file);

            Assert.Empty(result.FileErrors);
            Assert.Equal(2, result.Rows.Count);
            Assert.Equal(2, result.Rows[0].RowNumber);
            Assert.Equal("Plan A", result.Rows[0].Name);
            Assert.Equal(4, result.Rows[1].RowNumber);
        }

        [Fact]
        public void Parse_HeadersAreMatchedByName_NotPosition()
        {
            var headers = WorkbookBuilder.Headers.Reverse().ToArray();
            var cells = WorkbookBuilder.ValidCells().Reverse().ToArray();
            var result = _fx.CreateService().ParseWorkbook(WorkbookBuilder.ToFormFile(WorkbookBuilder.Build(headers, cells)));

            Assert.Empty(result.FileErrors);
            Assert.Equal("proj-1", result.Rows[0].ProjectId);
            Assert.Equal("emp-2", result.Rows[0].PlanReviewerId);
        }

        [Theory]
        [InlineData("missing")]
        [InlineData("unknown")]
        [InlineData("duplicate")]
        public void Parse_StructuralHeaderProblems_AreFileErrors(string kind)
        {
            var headers = WorkbookBuilder.Headers.ToList();
            string expected;
            switch (kind)
            {
                case "missing": headers.Remove("Plan Reviewer"); expected = "column_missing"; break;
                case "unknown": headers.Add("Plan Number"); expected = "column_unknown"; break;
                default: headers.Add("Name"); expected = "column_duplicate"; break;
            }

            var result = _fx.CreateService().ParseWorkbook(WorkbookBuilder.ToFormFile(WorkbookBuilder.Build(headers.ToArray(), WorkbookBuilder.ValidCells())));

            Assert.Contains(result.FileErrors, e => e.Code == expected);
            Assert.Empty(result.Rows);
        }

        [Fact]
        public void Parse_UnsupportedExtension_IsRejected()
        {
            var result = _fx.CreateService().ParseWorkbook(WorkbookBuilder.ToFormFile(WorkbookBuilder.Build(WorkbookBuilder.Headers, WorkbookBuilder.ValidCells()), "plans.csv"));
            Assert.Contains(result.FileErrors, e => e.Code == "file_type");
        }

        [Fact]
        public void Parse_EmptyAndCorruptFiles_AreRejected()
        {
            var svc = _fx.CreateService();
            Assert.Contains(svc.ParseWorkbook(null).FileErrors, e => e.Code == "file_empty");
            Assert.Contains(svc.ParseWorkbook(WorkbookBuilder.ToFormFile(new byte[] { 1, 2, 3, 4 })).FileErrors, e => e.Code == "file_unreadable");
        }

        [Fact]
        public void Parse_HeadersOnly_ReportsNoRows()
        {
            var result = _fx.CreateService().ParseWorkbook(WorkbookBuilder.ToFormFile(WorkbookBuilder.Build(WorkbookBuilder.Headers)));
            Assert.Contains(result.FileErrors, e => e.Code == "no_rows");
        }

        [Fact]
        public void Parse_WrongSheetName_IsRejected()
        {
            var result = _fx.CreateService().ParseWorkbook(WorkbookBuilder.ToFormFile(WorkbookBuilder.Build("Sheet1", WorkbookBuilder.Headers, WorkbookBuilder.ValidCells())));
            Assert.Contains(result.FileErrors, e => e.Code == "sheet_missing");
        }

        [Fact]
        public void Parse_OverRowLimit_IsRejected()
        {
            var rows = Enumerable.Range(0, TestPlanImportField.MaxDataRows + 1).Select(i => WorkbookBuilder.ValidCells("Plan " + i)).ToArray();
            var result = _fx.CreateService().ParseWorkbook(WorkbookBuilder.ToFormFile(WorkbookBuilder.Build(WorkbookBuilder.Headers, rows)));
            Assert.Contains(result.FileErrors, e => e.Code == "too_many_rows");
            Assert.Empty(result.Rows);
        }

        [Fact]
        public void Parse_FormulaCell_IsReportedAndNotEvaluated()
        {
            var cells = WorkbookBuilder.ValidCells();
            cells[1] = new Formula("CONCAT(\"Plan\",\"X\")");
            var result = _fx.CreateService().ParseWorkbook(WorkbookBuilder.ToFormFile(WorkbookBuilder.Build(WorkbookBuilder.Headers, cells)));

            Assert.Empty(result.FileErrors);
            Assert.Null(result.Rows[0].Name);
            Assert.Contains(result.CellErrors, e => e.RowNumber == 2 && e.Field == "name" && e.Code == "formula_not_allowed");
        }
        #endregion

        #region Validation
        [Fact]
        public async Task Validate_AllValid_CanConfirm_ResolvesLabels_AndWritesNothing()
        {
            var preview = await _fx.CreateService().ValidateAsync(new[] { ImportFixture.ValidRow(2), ImportFixture.ValidRow(3, "Plan B") }, ImportFixture.SiteId, ImportFixture.UserId);

            Assert.True(preview.CanConfirm);
            Assert.Equal(2, preview.ValidRows);
            var row = preview.Rows[0];
            Assert.Equal("Alpha Project", row.ProjectName);
            Assert.Equal("Asha Rao", row.PlanMakerName);
            Assert.Equal("Ravi Kumar", row.PlanReviewerName);
            Assert.Equal("Payments", row.AreaName);
            Assert.Equal("Web", row.WorkspaceName);
            _fx.VerifyNoWrites();
        }

        [Fact]
        public async Task Validate_MissingRequired_ProducesFieldErrors_AndBlocksBatch()
        {
            var bad = new TestPlanImportRowModel { RowNumber = 3 };
            var preview = await _fx.CreateService().ValidateAsync(new[] { ImportFixture.ValidRow(2), bad }, ImportFixture.SiteId, ImportFixture.UserId);

            Assert.False(preview.CanConfirm);
            Assert.Equal(1, preview.InvalidRows);
            var fields = preview.Rows[1].Errors.Where(e => e.Code == "required").Select(e => e.Field).OrderBy(x => x).ToArray();
            Assert.Equal(new[] { "name", "planMakerId", "planReviewerId", "projectId" }, fields);
            Assert.Empty(preview.Rows[1].Errors.Where(e => e.Field is "areaId" or "workspaceId" or "description"));
        }

        [Theory]
        [InlineData(200, true)]
        [InlineData(201, false)]
        public async Task Validate_NameLengthBoundary(int length, bool valid)
        {
            var preview = await _fx.CreateService().ValidateAsync(new[] { ImportFixture.ValidRow(2, new string('x', length)) }, ImportFixture.SiteId, ImportFixture.UserId);
            Assert.Equal(valid, preview.CanConfirm);
        }

        [Fact]
        public async Task Validate_UnknownOrUnauthorizedLookups_AreFieldErrors()
        {
            var row = ImportFixture.ValidRow(2);
            row.ProjectId = "proj-other"; // exists elsewhere but not in user's permitted list
            row.PlanMakerId = "emp-other";
            row.AreaId = "area-x";
            row.WorkspaceId = "ws-x";

            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, ImportFixture.SiteId, ImportFixture.UserId);

            var fields = preview.Rows[0].Errors.Where(e => e.Code == "lookup_invalid").Select(e => e.Field).OrderBy(x => x).ToArray();
            Assert.Equal(new[] { "areaId", "planMakerId", "projectId", "workspaceId" }, fields);
            Assert.False(preview.CanConfirm);
        }

        [Fact]
        public async Task Validate_DropdownLabels_ResolveToCanonicalIds()
        {
            var row = new TestPlanImportRowModel
            {
                RowNumber = 2, Name = "By label", ProjectId = "alpha project", AreaId = "Payments", WorkspaceId = "Web",
                PlanMakerId = "Asha Rao", PlanReviewerId = "Ravi Kumar"
            };

            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, ImportFixture.SiteId, ImportFixture.UserId);

            Assert.True(preview.CanConfirm);
            var r = preview.Rows[0];
            Assert.Equal(("proj-1", "Alpha Project"), (r.ProjectId, r.ProjectName));
            Assert.Equal(("area-1", "ws-1"), (r.AreaId, r.WorkspaceId));
            Assert.Equal(("emp-1", "emp-2"), (r.PlanMakerId, r.PlanReviewerId));
        }

        [Fact]
        public async Task Validate_DuplicateLabels_DisambiguatedOptionResolves_PlainLabelIsAmbiguous()
        {
            _fx.Employees.Add(new Employee { Id = "emp-3", Person = new Person { FullName = "Asha Rao" } });
            var svc = _fx.CreateService();

            var ok = ImportFixture.ValidRow(2); ok.PlanMakerId = "Asha Rao (emp-3)";
            var ambiguous = ImportFixture.ValidRow(3, "Plan B"); ambiguous.PlanMakerId = "Asha Rao";

            var preview = await svc.ValidateAsync(new[] { ok, ambiguous }, ImportFixture.SiteId, ImportFixture.UserId);

            Assert.Equal("emp-3", preview.Rows[0].PlanMakerId);
            Assert.True(preview.Rows[0].IsValid);
            Assert.Contains(preview.Rows[1].Errors, e => e.Field == "planMakerId" && e.Code == "lookup_ambiguous");
            Assert.False(preview.CanConfirm);
        }

        [Fact]
        public async Task Validate_LabelOutsideUserScope_IsInvalid()
        {
            var row = ImportFixture.ValidRow(2); row.ProjectId = "Gamma Project (other site)";
            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, ImportFixture.SiteId, ImportFixture.UserId);
            Assert.Contains(preview.Rows[0].Errors, e => e.Field == "projectId" && e.Code == "lookup_invalid");
        }

        [Fact]
        public async Task Validate_OptionalBlanks_AreAllowed()
        {
            var row = ImportFixture.ValidRow(2);
            row.AreaId = null; row.WorkspaceId = "  "; row.Description = null;
            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, ImportFixture.SiteId, ImportFixture.UserId);
            Assert.True(preview.CanConfirm);
            Assert.Null(preview.Rows[0].WorkspaceId);
        }

        [Fact]
        public async Task Validate_InFileDuplicates_AreCaseInsensitive_PerProject()
        {
            var rows = new[]
            {
                ImportFixture.ValidRow(2, "Smoke Plan"),
                ImportFixture.ValidRow(3, "SMOKE plan"),
                ImportFixture.ValidRow(4, "Smoke Plan", "proj-2"), // other project: allowed
            };
            var preview = await _fx.CreateService().ValidateAsync(rows, ImportFixture.SiteId, ImportFixture.UserId);

            Assert.Contains(preview.Rows[0].Errors, e => e.Code == "duplicate_in_file");
            Assert.Contains(preview.Rows[1].Errors, e => e.Code == "duplicate_in_file");
            Assert.True(preview.Rows[2].IsValid);
            Assert.False(preview.CanConfirm);
        }

        [Fact]
        public async Task Validate_ExistingActivePlan_IsDuplicate()
        {
            _fx.TestPlanService.Setup(x => x.GetTestPlanByName(ImportFixture.SiteId, "Plan A", "proj-1", null))
                .ReturnsAsync(new TestPlan { Id = "tp-1" });

            var preview = await _fx.CreateService().ValidateAsync(new[] { ImportFixture.ValidRow(2) }, ImportFixture.SiteId, ImportFixture.UserId);

            Assert.Contains(preview.Rows[0].Errors, e => e.Code == "duplicate_existing");
            Assert.False(preview.CanConfirm);
            _fx.VerifyNoWrites();
        }

        [Fact]
        public async Task Validate_EmptyOrDuplicateRowNumbers_AreRejected()
        {
            var svc = _fx.CreateService();
            Assert.Contains((await svc.ValidateAsync(new TestPlanImportRowModel[0], ImportFixture.SiteId, ImportFixture.UserId)).FileErrors, e => e.Code == "no_rows");

            var preview = await svc.ValidateAsync(new[] { ImportFixture.ValidRow(2), ImportFixture.ValidRow(2, "Other") }, ImportFixture.SiteId, ImportFixture.UserId);
            Assert.All(preview.Rows, r => Assert.Contains(r.Errors, e => e.Code == "row_number_invalid"));
        }

        [Fact]
        public async Task Validate_CellErrorsFromParse_MakeRowInvalid_WithoutDoubleRequiredError()
        {
            var row = ImportFixture.ValidRow(2); row.Name = null;
            var cellErrors = new[] { new TestPlanImportValidationError { RowNumber = 2, Field = "name", Code = "formula_not_allowed", Message = "x" } };

            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, ImportFixture.SiteId, ImportFixture.UserId, cellErrors);

            Assert.Single(preview.Rows[0].Errors.Where(e => e.Field == "name"));
            Assert.False(preview.CanConfirm);
        }
        #endregion

        #region Description
        [Fact]
        public void DescriptionHtml_IsEncoded_AndCannotInjectImages()
        {
            var html = _fx.CreateService().ToDescriptionHtml("line 1\n<img src=\"data:image/png;base64,AAAA\">");
            Assert.Equal("<p>line 1<br>&lt;img src=&quot;data:image/png;base64,AAAA&quot;&gt;</p>", html);
            Assert.DoesNotContain("<img", html);
            Assert.Null(_fx.CreateService().ToDescriptionHtml("   "));
        }
        #endregion
    }
}
