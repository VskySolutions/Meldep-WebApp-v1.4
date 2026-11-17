using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation.Contracts;
using Vsky.Api.Models;
using Vsky.Api.Services.TestCases;
using Vsky.Models;
using Xunit;

namespace Vsky.Api.Tests.TestCases
{
    public class TestCaseImportServiceTests
    {
        private readonly CaseFixture _fx = new();

        #region Template
        [Fact]
        public async Task Template_HasExactlyThe15EditableFormFields_InOrder()
        {
            var bytes = await _fx.CreateService().BuildTemplateAsync(CaseFixture.SiteId, CaseFixture.UserId);
            using var package = new ExcelPackage(new MemoryStream(bytes));
            var sheet = package.Workbook.Worksheets[TestCaseImportField.DataSheetName];
            var headers = Enumerable.Range(1, sheet.Dimension.End.Column).Select(c => sheet.Cells[1, c].Text).ToArray();

            Assert.Equal(new[]
            {
                "Project Name", "Test Plan Name", "Project Module", "Requirement", "Name", "Area", "Workspace", "Status",
                "Assigned To", "Tested By", "Tested Date", "Description", "Steps", "Expected Result", "Actual Result"
            }, headers);
            Assert.DoesNotContain(headers, h => h.Contains("Number") || h.Contains("Site") || h.Contains("Created") || h.Contains("Release") || h.Contains("Test Result") || h == "Id");
            _fx.VerifyNoWrites();
        }

        [Fact]
        public void Metadata_RequiredFields_MatchForm()
        {
            var required = TestCaseImportField.All.Where(f => f.Required).Select(f => f.Key).ToArray();
            Assert.Equal(new[] { "projectId", "planId", "projectModuleId", "requirementId", "name", "testedBy" }, required);
        }

        [Fact]
        public async Task Template_LookupColumns_AreDropdowns_WithCascadingPlanModuleRequirement()
        {
            var bytes = await _fx.CreateService().BuildTemplateAsync(CaseFixture.SiteId, CaseFixture.UserId);
            using var package = new ExcelPackage(new MemoryStream(bytes));
            var sheet = package.Workbook.Worksheets[TestCaseImportField.DataSheetName];
            var byCol = sheet.DataValidations.OfType<IExcelDataValidationList>().ToDictionary(v => v.Address.Start.Column, v => v.Formula.ExcelFormula);

            // A Project, B Plan, C Module, D Requirement, F Area, G Workspace, H Status, I Assigned, J Tested By
            Assert.Equal(new[] { 1, 2, 3, 4, 6, 7, 8, 9, 10 }, byCol.Keys.OrderBy(k => k).ToArray());
            Assert.Equal("ProjectOptions", byCol[1]);
            Assert.Equal("INDIRECT(\"TP_\"&MATCH($A2,ProjectOptions,0))", byCol[2]);
            Assert.Equal("INDIRECT(\"PM_\"&MATCH($A2,ProjectOptions,0))", byCol[3]);
            Assert.Equal("INDIRECT(VLOOKUP($A2&\"|\"&$C2,ModuleKeys,2,FALSE))", byCol[4]);
            Assert.Equal("'Statuses'!$A$2:$A$4", byCol[8]);
            Assert.Equal("'Employees'!$A$2:$A$3", byCol[10]);
            // Assigned To lists employees only when the row's Status is Fail
            Assert.Equal("IF($H2=\"Fail\",'Employees'!$A$2:$A$3,_Keys!$Z$1)", byCol[9]);

            // Named ranges point at each project's own plans/modules (Alpha = 1, Beta = 2)
            var names = package.Workbook.Names;
            string Addr(string name) => names[name].FullAddressAbsolute.Replace("'", ""); // EPPlus quotes sheet names only when needed
            Assert.Equal("Test Plans!$B$2", Addr("TP_1"));
            Assert.Equal("Test Plans!$B$3", Addr("TP_2"));
            Assert.Equal("Project Modules!$B$3:$B$4", Addr("PM_2"));

            // Module key table maps "Project|Module" to that module's requirement list
            var keys = package.Workbook.Worksheets["_Keys"];
            Assert.Equal(eWorkSheetHidden.Hidden, keys.Hidden);
            var keyRows = Enumerable.Range(2, 3).Select(r => (keys.Cells[r, 1].Text, keys.Cells[r, 2].Text)).ToArray();
            Assert.Contains(("Alpha|Login", "RQ_1"), keyRows);
            Assert.Contains(("Beta|Billing", "RQ_2"), keyRows);
            Assert.Equal("Requirements!$C$2", Addr("RQ_1"));
            Assert.Equal("_Keys!$Z$1", Addr("RQ_2")); // module without requirements -> blank list
            Assert.Equal("Requirements!$C$3", Addr("RQ_3"));
        }

        [Fact]
        public async Task Template_AssignedTo_IsGreyedOutUnlessStatusIsFail()
        {
            var bytes = await _fx.CreateService().BuildTemplateAsync(CaseFixture.SiteId, CaseFixture.UserId);
            using var package = new ExcelPackage(new MemoryStream(bytes));
            var sheet = package.Workbook.Worksheets[TestCaseImportField.DataSheetName];

            var rule = Assert.Single(sheet.ConditionalFormatting.OfType<OfficeOpenXml.ConditionalFormatting.Contracts.IExcelConditionalFormattingExpression>());
            Assert.Equal("NOT($H2=\"Fail\")", rule.Formula);
            Assert.Equal(9, rule.Address.Start.Column);
            Assert.Equal(2, rule.Address.Start.Row);
            Assert.Equal(TestCaseImportField.MaxDataRows + 1, rule.Address.End.Row);
        }

        [Fact]
        public async Task Template_TestedDate_IsDateColumnWithDateValidation()
        {
            var bytes = await _fx.CreateService().BuildTemplateAsync(CaseFixture.SiteId, CaseFixture.UserId);
            using var package = new ExcelPackage(new MemoryStream(bytes));
            var sheet = package.Workbook.Worksheets[TestCaseImportField.DataSheetName];
            var col = TestCaseImportService.ColumnIndex(TestCaseImportField.TestedDate);

            var dv = Assert.Single(sheet.DataValidations.OfType<IExcelDataValidationDateTime>());
            Assert.Equal(col, dv.Address.Start.Column);
            Assert.Equal(new DateTime(2000, 1, 1), dv.Formula.Value);
            Assert.Equal(new DateTime(2099, 12, 31), dv.Formula2.Value);
            Assert.Equal("mm/dd/yyyy", sheet.Column(col).Style.Numberformat.Format);
        }

        [Fact]
        public async Task Template_RoundTrip_PickedDateAndFailAssignee_PreviewValid()
        {
            var svc = _fx.CreateService();
            var bytes = await svc.BuildTemplateAsync(CaseFixture.SiteId, CaseFixture.UserId);
            byte[] filled;
            using (var package = new ExcelPackage(new MemoryStream(bytes)))
            {
                var sheet = package.Workbook.Worksheets[TestCaseImportField.DataSheetName];
                var values = new object[] { "Beta", "Smoke Plan", "Login", "User can log in", "Round trip", null, null, "Fail", "Asha Rao", "Ravi Kumar" };
                for (int c = 0; c < values.Length; c++) sheet.Cells[2, c + 1].Value = values[c];
                // A date picked/typed in Excel is stored as a serial number in the date-formatted cell.
                sheet.Cells[2, TestCaseImportService.ColumnIndex(TestCaseImportField.TestedDate)].Value = new DateTime(2026, 3, 4).ToOADate();
                filled = package.GetAsByteArray();
            }

            var parsed = svc.ParseWorkbook(CaseWorkbook.ToFormFile(filled));
            Assert.Empty(parsed.FileErrors);
            Assert.Empty(parsed.CellErrors);
            var preview = await svc.ValidateAsync(parsed.Rows, CaseFixture.SiteId, CaseFixture.UserId, parsed.CellErrors);

            Assert.True(preview.CanConfirm, string.Join("; ", preview.Rows.SelectMany(r => r.Errors).Select(e => e.Message)));
            Assert.Equal("03/04/2026", preview.Rows[0].TestedDate);
            Assert.Equal(("emp-1", "st-fail"), (preview.Rows[0].EmployeeId, preview.Rows[0].StatusId));
        }

        [Fact]
        public async Task Template_GuidanceSheets_OnlyListScopedRecords()
        {
            var bytes = await _fx.CreateService().BuildTemplateAsync(CaseFixture.SiteId, CaseFixture.UserId);
            using var package = new ExcelPackage(new MemoryStream(bytes));
            var projects = package.Workbook.Worksheets["Projects"];
            Assert.Equal(new[] { "proj-1", "proj-2" }, new[] { projects.Cells[2, 2].Text, projects.Cells[3, 2].Text });
            Assert.Equal("", projects.Cells[4, 1].Text);
            Assert.NotNull(package.Workbook.Worksheets["Instructions"]);
        }
        #endregion

        #region Parse
        [Fact]
        public void Parse_ValidWorkbook_ReturnsRows()
        {
            var result = _fx.CreateService().ParseWorkbook(CaseWorkbook.ToFormFile(CaseWorkbook.Build(CaseWorkbook.Headers, CaseWorkbook.ValidCells(), CaseWorkbook.ValidCells("Case B"))));
            Assert.Empty(result.FileErrors);
            Assert.Equal(2, result.Rows.Count);
            Assert.Equal("11/15/2026", result.Rows[0].TestedDate);
            Assert.Equal(3, result.Rows[1].RowNumber);
        }

        [Theory]
        [InlineData("missing", "column_missing")]
        [InlineData("unknown", "column_unknown")]
        [InlineData("duplicate", "column_duplicate")]
        public void Parse_HeaderProblems_AreFileErrors(string kind, string code)
        {
            var headers = CaseWorkbook.Headers.ToList();
            if (kind == "missing") headers.Remove("Requirement");
            else if (kind == "unknown") headers.Add("Test Result");
            else headers.Add("Steps");
            var result = _fx.CreateService().ParseWorkbook(CaseWorkbook.ToFormFile(CaseWorkbook.Build(headers.ToArray(), CaseWorkbook.ValidCells())));
            Assert.Contains(result.FileErrors, e => e.Code == code);
            Assert.Empty(result.Rows);
        }

        [Fact]
        public void Parse_UnsupportedEmptyCorrupt_AreRejected()
        {
            var svc = _fx.CreateService();
            Assert.Contains(svc.ParseWorkbook(null).FileErrors, e => e.Code == "file_empty");
            Assert.Contains(svc.ParseWorkbook(CaseWorkbook.ToFormFile(new byte[] { 1, 2, 3 }, "cases.xls")).FileErrors, e => e.Code == "file_type");
            Assert.Contains(svc.ParseWorkbook(CaseWorkbook.ToFormFile(new byte[] { 1, 2, 3 })).FileErrors, e => e.Code == "file_unreadable");
            Assert.Contains(svc.ParseWorkbook(CaseWorkbook.ToFormFile(CaseWorkbook.Build(CaseWorkbook.Headers))).FileErrors, e => e.Code == "no_rows");
        }

        [Fact]
        public void Parse_RealExcelDate_UsesCalendarDate_AndBareNumberIsRejected()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            byte[] bytes;
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add(TestCaseImportField.DataSheetName);
                for (int c = 0; c < CaseWorkbook.Headers.Length; c++) sheet.Cells[1, c + 1].Value = CaseWorkbook.Headers[c];
                var dateCol = TestCaseImportService.ColumnIndex(TestCaseImportField.TestedDate);
                var cells = CaseWorkbook.ValidCells();
                for (int c = 0; c < cells.Length; c++) { sheet.Cells[2, c + 1].Value = cells[c]; sheet.Cells[3, c + 1].Value = cells[c]; }
                sheet.Cells[3, 5].Value = "Case B";
                sheet.Cells[2, dateCol].Value = new DateTime(2026, 3, 4, 23, 59, 0);
                sheet.Cells[2, dateCol].Style.Numberformat.Format = "dd/mm/yyyy";
                sheet.Cells[3, dateCol].Value = 46000d; // bare serial, General format
                bytes = package.GetAsByteArray();
            }

            var result = _fx.CreateService().ParseWorkbook(CaseWorkbook.ToFormFile(bytes));

            Assert.Equal("03/04/2026", result.Rows[0].TestedDate); // March 4 regardless of display format
            Assert.Contains(result.CellErrors, e => e.RowNumber == 3 && e.Field == "testedDate" && e.Code == "date_invalid");
        }
        #endregion

        #region Validate
        [Fact]
        public async Task Validate_LabelsResolveWithinProjectContext_ToCanonicalIds()
        {
            var row = CaseFixture.ValidRow(2);
            row.ProjectId = "Beta"; // same plan/module/requirement labels exist in both projects
            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, CaseFixture.SiteId, CaseFixture.UserId);

            Assert.True(preview.CanConfirm, string.Join("; ", preview.Rows[0].Errors.Select(e => e.Message)));
            var r = preview.Rows[0];
            Assert.Equal(("proj-2", "plan-2", "mod-2", "req-2"), (r.ProjectId, r.PlanId, r.ProjectModuleId, r.RequirementId));
            Assert.Null(r.EmployeeId); // not a Fail row
            Assert.Equal("emp-2", r.TestedBy);
            Assert.Equal("Smoke Plan", r.PlanName);
            _fx.VerifyNoWrites();
        }

        [Fact]
        public async Task Validate_RawIdsStillAccepted()
        {
            var row = new TestCaseImportRowModel
            {
                RowNumber = 2, ProjectId = "proj-1", PlanId = "plan-1", ProjectModuleId = "mod-1", RequirementId = "req-1",
                Name = "By ID", StatusId = "st-pass", TestedBy = "emp-2"
            };
            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, CaseFixture.SiteId, CaseFixture.UserId);
            Assert.True(preview.CanConfirm);
        }

        [Fact]
        public async Task Validate_PlanModuleRequirementFromAnotherProject_AreContextErrors()
        {
            var row = new TestCaseImportRowModel
            {
                RowNumber = 2, ProjectId = "proj-1", PlanId = "plan-2", ProjectModuleId = "mod-2", RequirementId = "req-2",
                Name = "Mixed", TestedBy = "emp-2"
            };
            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, CaseFixture.SiteId, CaseFixture.UserId);

            var errors = preview.Rows[0].Errors;
            Assert.Contains(errors, e => e.Field == "planId" && e.Code == "lookup_invalid");
            Assert.Contains(errors, e => e.Field == "projectModuleId" && e.Code == "lookup_invalid");
            Assert.Contains(errors, e => e.Field == "requirementId" && e.Code == "context_invalid");
            Assert.False(preview.CanConfirm);
        }

        [Fact]
        public async Task Validate_RequirementFromAnotherModuleOfSameProject_IsRejected()
        {
            var row = CaseFixture.ValidRow(2);
            row.ProjectId = "Beta";
            row.ProjectModuleId = "Billing"; // req "User can log in" belongs to Beta/Login, not Beta/Billing
            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, CaseFixture.SiteId, CaseFixture.UserId);
            Assert.Contains(preview.Rows[0].Errors, e => e.Field == "requirementId" && e.Code == "lookup_invalid");
        }

        [Fact]
        public async Task Validate_UnauthorizedProject_IsRejected_AndDependentsNotResolved()
        {
            var row = CaseFixture.ValidRow(2); row.ProjectId = "proj-x";
            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, CaseFixture.SiteId, CaseFixture.UserId);
            var errors = preview.Rows[0].Errors;
            Assert.Contains(errors, e => e.Field == "projectId" && e.Code == "lookup_invalid");
            Assert.Contains(errors, e => e.Field == "planId" && e.Code == "context_invalid");
            _fx.TestPlanService.Verify(x => x.GetAllTestPlansListForDropdown(It.IsAny<string>(), "proj-x"), Times.Never);
        }

        [Fact]
        public async Task Validate_RequiredFields_AndNameBoundary()
        {
            var empty = new TestCaseImportRowModel { RowNumber = 2 };
            var longName = CaseFixture.ValidRow(3, new string('x', 201));
            var okName = CaseFixture.ValidRow(4, new string('y', 200));
            var preview = await _fx.CreateService().ValidateAsync(new[] { empty, longName, okName }, CaseFixture.SiteId, CaseFixture.UserId);

            var required = preview.Rows[0].Errors.Where(e => e.Code == "required").Select(e => e.Field).OrderBy(x => x).ToArray();
            Assert.Equal(new[] { "name", "planId", "projectId", "projectModuleId", "requirementId", "testedBy" }, required);
            Assert.Contains(preview.Rows[1].Errors, e => e.Field == "name" && e.Code == "max_length");
            Assert.True(preview.Rows[2].IsValid);
            Assert.False(preview.CanConfirm);
        }

        [Fact]
        public async Task Validate_Defaults_BlankStatusIsNew_BlankDateIsSiteToday()
        {
            var row = CaseFixture.ValidRow(2); row.StatusId = null; row.TestedDate = null; row.AreaId = null; row.EmployeeId = null;
            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, CaseFixture.SiteId, CaseFixture.UserId);
            var r = preview.Rows[0];
            Assert.True(r.IsValid);
            Assert.Equal(("st-new", "New"), (r.StatusId, r.StatusName));
            Assert.Equal("11/17/2026", r.TestedDate);
        }

        [Theory]
        [InlineData("11/15/2026", "11/15/2026", true)]
        [InlineData("3/4/2026", "03/04/2026", true)]
        [InlineData("2026-03-04", null, false)]
        [InlineData("13/01/2026", null, false)]
        [InlineData("02/30/2026", null, false)]
        public async Task Validate_TestedDate_StrictMonthDayYear(string input, string canonical, bool valid)
        {
            var row = CaseFixture.ValidRow(2); row.TestedDate = input;
            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, CaseFixture.SiteId, CaseFixture.UserId);
            Assert.Equal(valid, preview.Rows[0].IsValid);
            if (valid) Assert.Equal(canonical, preview.Rows[0].TestedDate);
        }

        [Fact]
        public async Task Validate_Duplicates_InFileAndExisting_UsePlanModuleScope()
        {
            _fx.TestCaseService.Setup(x => x.GetTestCaseByName(CaseFixture.SiteId, "proj-1", "plan-1", "mod-1", "Existing", null)).ReturnsAsync(new TestCase());

            var rows = new[]
            {
                CaseFixture.ValidRow(2, "Dup"),
                CaseFixture.ValidRow(3, "DUP"),
                CaseFixture.ValidRow(4, "Existing"),
            };
            var other = CaseFixture.ValidRow(5, "dup"); other.ProjectId = "Beta"; // different project: allowed
            var preview = await _fx.CreateService().ValidateAsync(rows.Append(other).ToList(), CaseFixture.SiteId, CaseFixture.UserId);

            Assert.Contains(preview.Rows[0].Errors, e => e.Code == "duplicate_in_file");
            Assert.Contains(preview.Rows[1].Errors, e => e.Code == "duplicate_in_file");
            Assert.Contains(preview.Rows[2].Errors, e => e.Code == "duplicate_existing");
            Assert.True(preview.Rows[3].IsValid);
            Assert.False(preview.CanConfirm);
        }

        [Fact]
        public async Task Validate_FailRows_PredictIssueOutcome_WithoutWrites()
        {
            _fx.IssueService.Setup(x => x.GetIssueByName(CaseFixture.SiteId, "Known bug", "proj-1", null)).ReturnsAsync(new Issue());
            var rows = new[]
            {
                CaseFixture.ValidRow(2, "Known bug", "Fail"),
                CaseFixture.ValidRow(3, "New bug", "Fail"),
                CaseFixture.ValidRow(4, "Passing", "Pass"),
            };
            // Same project + case-insensitive name as row 3 (same issue scope) but a different plan, so it is
            // not a duplicate test case; only one issue may be created for the pair.
            _fx.TestPlanService.Setup(x => x.GetAllTestPlansListForDropdown(CaseFixture.SiteId, "proj-1"))
                .ReturnsAsync(new System.Collections.Generic.List<CommonDropDown> { new() { Value = "plan-1", Text = "Smoke Plan" }, new() { Value = "plan-9", Text = "Regression" } });
            var sameNameOtherPlan = CaseFixture.ValidRow(5, "new BUG", "Fail");
            sameNameOtherPlan.PlanId = "Regression";

            var preview = await _fx.CreateService().ValidateAsync(rows.Append(sameNameOtherPlan).ToList(), CaseFixture.SiteId, CaseFixture.UserId);

            Assert.True(preview.CanConfirm, string.Join("; ", preview.Rows.SelectMany(r => r.Errors).Select(e => e.Message)));
            Assert.Equal(new[] { "exists", "create", null, "created_in_batch" }, preview.Rows.Select(r => r.FailIssue).ToArray());
            Assert.Equal(1, preview.FailIssuesToCreate);
            _fx.VerifyNoWrites();
        }

        [Theory]
        [InlineData("Pass", false)]
        [InlineData(null, false)] // blank status defaults to New
        [InlineData("Fail", true)]
        [InlineData("fail", true)]
        public async Task Validate_AssignedTo_OnlyAcceptedForFail(string status, bool valid)
        {
            var row = CaseFixture.ValidRow(2, status: status);
            row.EmployeeId = "Asha Rao";
            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, CaseFixture.SiteId, CaseFixture.UserId);

            Assert.Equal(valid, preview.Rows[0].IsValid);
            if (valid) Assert.Equal("emp-1", preview.Rows[0].EmployeeId);
            else Assert.Contains(preview.Rows[0].Errors, e => e.Field == "employeeId" && e.Code == "not_applicable");
        }

        [Fact]
        public async Task Validate_UnknownStatusOrEmployee_AreFieldErrors()
        {
            var row = CaseFixture.ValidRow(2, status: "Blocked"); row.TestedBy = "Nobody";
            var preview = await _fx.CreateService().ValidateAsync(new[] { row }, CaseFixture.SiteId, CaseFixture.UserId);
            Assert.Contains(preview.Rows[0].Errors, e => e.Field == "statusId");
            Assert.Contains(preview.Rows[0].Errors, e => e.Field == "testedBy");
        }
        #endregion

        [Fact]
        public void RichText_IsEncoded()
        {
            var html = _fx.CreateService().ToRichTextHtml("a\n<img src=x onerror=alert(1)>");
            Assert.Equal("<p>a<br>&lt;img src=x onerror=alert(1)&gt;</p>", html);
        }
    }
}
