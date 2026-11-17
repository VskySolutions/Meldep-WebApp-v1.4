using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using Vsky.Api.Models;
using Vsky.Api.Services.Import;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.Employees;
using Vsky.Services.Projects;
using Vsky.Services.TestPlans;

namespace Vsky.Api.Services.TestPlans
{
    // Title: TestPlanImportParseResult
    // Description: In-memory result of reading an uploaded workbook. Nothing is persisted.
    public class TestPlanImportParseResult
    {
        public List<TestPlanImportValidationError> FileErrors { get; } = new();
        public List<TestPlanImportValidationError> CellErrors { get; } = new();
        public List<TestPlanImportRowModel> Rows { get; } = new();
    }

    // Title: TestPlanImportLookups
    // Description: Lookup values the current user may reference, resolved with the same services that back
    // the manual Test Plan form dropdowns.
    public class TestPlanImportLookups
    {
        public ExcelImportLookupSet Projects { get; set; } = new();
        public ExcelImportLookupSet Employees { get; set; } = new();
        public ExcelImportLookupSet Areas { get; set; } = new();
        public ExcelImportLookupSet Workspaces { get; set; } = new();
    }

    public interface ITestPlanImportService
    {
        Task<byte[]> BuildTemplateAsync(string siteId, string loggedUserId);
        TestPlanImportParseResult ParseWorkbook(IFormFile file);
        Task<TestPlanImportPreviewModel> ValidateAsync(IList<TestPlanImportRowModel> rows, string siteId, string loggedUserId, IEnumerable<TestPlanImportValidationError> cellErrors = null);
        string ToDescriptionHtml(string plainText);
    }

    // Title: TestPlanImportService
    // Description: Template generation, in-memory workbook parsing and server-side validation for Test Plan
    // Excel import. This service is read-only: it never inserts/updates records, allocates plan numbers,
    // processes images or calls SaveChanges. Persistence is done by the controller through the shared
    // manual creation helper after a fully valid batch.
    public class TestPlanImportService : ITestPlanImportService
    {
        public const int DescriptionMaxLength = 32767; // Excel cell text limit

        private readonly ITestPlanService _testPlanService;
        private readonly IProjectService _projectService;
        private readonly IEmployeeService _employeeService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly IDropDownService _dropDownService;
        private readonly ICommonService _commonService;

        public TestPlanImportService(
            ITestPlanService testPlanService,
            IProjectService projectService,
            IEmployeeService employeeService,
            IDropDownTypeService dropDownTypeService,
            IDropDownService dropDownService,
            ICommonService commonService)
        {
            _testPlanService = testPlanService;
            _projectService = projectService;
            _employeeService = employeeService;
            _dropDownTypeService = dropDownTypeService;
            _dropDownService = dropDownService;
            _commonService = commonService;
        }

        #region Lookups
        // Title: LoadLookupsAsync
        // Description: Projects use the same scoped query as the manual form (non-template, non-archived,
        // admin or creator/permission-mapped); employees are active employees of the site; Area/Workspace
        // are the site's dropdown values. Nothing from another site is returned.
        public virtual async Task<TestPlanImportLookups> LoadLookupsAsync(string siteId, string loggedUserId)
        {
            var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(siteId, loggedUserId);
            var lookups = new TestPlanImportLookups();

            var projects = await _projectService.GetProjectsListForDropdown(siteId, loggedUserId, employeeId, false, "false", false);
            foreach (var p in projects ?? new())
                lookups.Projects.Add(p.Value, p.Text);

            var employees = await _employeeService.GetAllActiveEmployeeListForDropdown(siteId);
            foreach (var e in employees ?? new())
                lookups.Employees.Add(e.Id, e.Person?.FullName?.Trim());

            lookups.Areas = await LoadDropDownAsync(siteId, "Area");
            lookups.Workspaces = await LoadDropDownAsync(siteId, "Workspace");
            return lookups;
        }

        private async Task<ExcelImportLookupSet> LoadDropDownAsync(string siteId, string type)
        {
            var result = new ExcelImportLookupSet();
            var dropDownType = await _dropDownTypeService.GetDropDownType(siteId, type);
            if (dropDownType == null) return result;
            var values = await _dropDownService.GetDropDowns(dropDownType.Id);
            foreach (var d in values ?? new List<Vsky.Models.DropDown>())
                result.Add(d.Id, d.DropDownValue);
            return result;
        }
        #endregion

        #region Template
        // Title: BuildTemplateAsync
        // Description: Generates the import workbook from the allowlisted field metadata. Project Name, Area,
        // Workspace, Plan Maker and Plan Reviewer are Excel in-cell dropdowns whose options come from the same
        // scoped lists as the Add Test Plan form (listed on the guidance sheets with their record IDs).
        public async Task<byte[]> BuildTemplateAsync(string siteId, string loggedUserId)
        {
            var lookups = await LoadLookupsAsync(siteId, loggedUserId);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var data = package.Workbook.Worksheets.Add(TestPlanImportField.DataSheetName);
            for (int i = 0; i < TestPlanImportField.All.Count; i++)
            {
                var field = TestPlanImportField.All[i];
                var cell = data.Cells[1, i + 1];
                cell.Value = field.Header;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(field.Required ? System.Drawing.Color.LightSteelBlue : System.Drawing.Color.LightGray);
                cell.AddComment((field.Required ? "Required. " : "Optional. ") + field.Guidance.Replace("Required. ", "").Replace("Optional. ", ""), "Template");
                // Keep values as text so Excel does not reformat them.
                data.Column(i + 1).Style.Numberformat.Format = "@";
                data.Column(i + 1).Width = field.Key == TestPlanImportField.Description ? 60 : 40;
            }
            data.View.FreezePanes(2, 1);

            var info = package.Workbook.Worksheets.Add("Instructions");
            info.Cells[1, 1].Value = "Test Plan Import Template";
            info.Cells[1, 1].Style.Font.Bold = true;
            info.Cells[2, 1].Value = "Template version";
            info.Cells[2, 2].Value = TestPlanImportField.TemplateVersion;
            info.Cells[3, 1].Value = $"Fill one Test Plan per row on the '{TestPlanImportField.DataSheetName}' sheet, starting at row 2. Select Project Name, Area, Workspace, Plan Maker and Plan Reviewer from the cell dropdowns. Do not rename, remove or add columns. Formulas are not accepted. Maximum {TestPlanImportField.MaxDataRows} rows. If any row is invalid, nothing is imported.";
            info.Cells[5, 1].Value = "Column";
            info.Cells[5, 2].Value = "Required";
            info.Cells[5, 3].Value = "Guidance";
            info.Cells[5, 1, 5, 3].Style.Font.Bold = true;
            for (int i = 0; i < TestPlanImportField.All.Count; i++)
            {
                var f = TestPlanImportField.All[i];
                info.Cells[6 + i, 1].Value = f.Header;
                info.Cells[6 + i, 2].Value = f.Required ? "Yes" : "No";
                info.Cells[6 + i, 3].Value = f.Guidance;
            }
            info.Column(1).Width = 20;
            info.Column(2).Width = 12;
            info.Column(3).Width = 90;

            var projectList = AddLookupSheet(package, "Projects", "Project Name", "Project ID", lookups.Projects);
            var employeeList = AddLookupSheet(package, "Employees", "Employee Name", "Employee ID", lookups.Employees);
            var areaList = AddLookupSheet(package, "Areas", "Area", "Area ID", lookups.Areas);
            var workspaceList = AddLookupSheet(package, "Workspaces", "Workspace", "Workspace ID", lookups.Workspaces);

            AddDropdown(data, TestPlanImportField.ProjectId, projectList);
            AddDropdown(data, TestPlanImportField.AreaId, areaList);
            AddDropdown(data, TestPlanImportField.WorkspaceId, workspaceList);
            AddDropdown(data, TestPlanImportField.PlanMakerId, employeeList);
            AddDropdown(data, TestPlanImportField.PlanReviewerId, employeeList);

            return package.GetAsByteArray();
        }

        // Writes "option | ID" rows and returns the absolute option range (null when the list is empty).
        private static string AddLookupSheet(ExcelPackage package, string sheetName, string optionHeader, string idHeader, ExcelImportLookupSet set)
        {
            var sheet = package.Workbook.Worksheets.Add(sheetName);
            sheet.Cells[1, 1].Value = optionHeader;
            sheet.Cells[1, 2].Value = idHeader;
            sheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;
            sheet.Column(1).Style.Numberformat.Format = "@";
            sheet.Column(2).Style.Numberformat.Format = "@";
            int row = 2;
            foreach (var option in set.Options())
            {
                sheet.Cells[row, 1].Value = option.Key;
                sheet.Cells[row, 2].Value = option.Value;
                row++;
            }
            sheet.Column(1).Width = 45;
            sheet.Column(2).Width = 40;
            return row == 2 ? null : $"'{sheetName}'!$A$2:$A${row - 1}";
        }

        // Adds an in-cell list dropdown to every data row of the field's column.
        private static void AddDropdown(ExcelWorksheet data, string fieldKey, string listRange)
        {
            if (listRange == null) return; // nothing selectable; backend reports the value as invalid
            int col = TestPlanImportField.All.ToList().FindIndex(f => f.Key == fieldKey) + 1;
            var field = TestPlanImportField.All[col - 1];
            var range = data.Cells[2, col, TestPlanImportField.MaxDataRows + 1, col];
            var validation = data.DataValidations.AddListValidation(range.Address);
            validation.Formula.ExcelFormula = listRange;
            validation.AllowBlank = true;
            validation.ShowErrorMessage = true;
            validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
            validation.ErrorTitle = $"Invalid {field.Header}";
            validation.Error = $"Please select a {field.Header} from the dropdown list.";
            validation.ShowInputMessage = true;
            validation.PromptTitle = field.Header;
            validation.Prompt = field.Required ? "Required. Select from the list." : "Optional. Select from the list.";
        }
        #endregion

        #region Parse
        // Title: ParseWorkbook
        // Description: Reads the uploaded .xlsx in memory. Validates file type/size/structure, rejects formula
        // cells (cached values are never trusted) and returns canonical trimmed values per source row.
        public TestPlanImportParseResult ParseWorkbook(IFormFile file)
        {
            var result = new TestPlanImportParseResult();

            if (file == null || file.Length == 0)
            {
                result.FileErrors.Add(FileError("file_empty", "Please upload a non-empty Excel file."));
                return result;
            }
            if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                result.FileErrors.Add(FileError("file_type", "Only Excel .xlsx files created from the Test Plan template are supported."));
                return result;
            }
            if (file.Length > TestPlanImportField.MaxFileSizeBytes)
            {
                result.FileErrors.Add(FileError("file_too_large", $"The file exceeds the maximum size of {TestPlanImportField.MaxFileSizeBytes / (1024 * 1024)} MB."));
                return result;
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try
            {
                using var stream = file.OpenReadStream();
                using var package = new ExcelPackage(stream);
                ReadWorksheet(package, result);
            }
            catch (Exception)
            {
                result.FileErrors.Clear();
                result.CellErrors.Clear();
                result.Rows.Clear();
                result.FileErrors.Add(FileError("file_unreadable", "The file could not be read. Please upload a valid .xlsx file created from the template."));
            }
            return result;
        }

        private static void ReadWorksheet(ExcelPackage package, TestPlanImportParseResult result)
        {
            var sheet = package.Workbook.Worksheets[TestPlanImportField.DataSheetName];
            if (sheet == null)
            {
                result.FileErrors.Add(FileError("sheet_missing", $"The '{TestPlanImportField.DataSheetName}' sheet was not found. Please use the downloaded template."));
                return;
            }
            if (sheet.Dimension == null)
            {
                result.FileErrors.Add(FileError("file_empty", "The template contains no headers or data."));
                return;
            }

            int lastCol = sheet.Dimension.End.Column;
            int lastRow = sheet.Dimension.End.Row;

            // Header mapping: column index -> field key
            var columnMap = new Dictionary<int, string>();
            var byHeader = TestPlanImportField.All.ToDictionary(f => f.Header, f => f, StringComparer.OrdinalIgnoreCase);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var blankHeaderColumns = new List<int>();
            for (int c = 1; c <= lastCol; c++)
            {
                var header = CellText(sheet.Cells[1, c].Value);
                if (string.IsNullOrEmpty(header)) { blankHeaderColumns.Add(c); continue; }
                if (!byHeader.TryGetValue(header, out var field))
                {
                    result.FileErrors.Add(FileError("column_unknown", $"Column '{header}' is not part of the Test Plan template."));
                    continue;
                }
                if (!seen.Add(field.Header))
                {
                    result.FileErrors.Add(FileError("column_duplicate", $"Column '{field.Header}' appears more than once."));
                    continue;
                }
                columnMap[c] = field.Key;
            }
            foreach (var missing in TestPlanImportField.All.Where(f => !seen.Contains(f.Header)))
                result.FileErrors.Add(new TestPlanImportValidationError { RowNumber = 0, Field = missing.Key, Code = "column_missing", Message = $"Required template column '{missing.Header}' is missing." });

            if (result.FileErrors.Any()) return;

            int dataRows = 0;
            for (int r = 2; r <= lastRow; r++)
            {
                bool any = false;
                for (int c = 1; c <= lastCol; c++)
                {
                    var cell = sheet.Cells[r, c];
                    if (!string.IsNullOrEmpty(cell.Formula) || !string.IsNullOrEmpty(CellText(cell.Value))) { any = true; break; }
                }
                if (!any) continue; // skip fully blank rows

                dataRows++;
                if (dataRows > TestPlanImportField.MaxDataRows)
                {
                    result.Rows.Clear();
                    result.CellErrors.Clear();
                    result.FileErrors.Add(FileError("too_many_rows", $"The file contains more than {TestPlanImportField.MaxDataRows} data rows. Please split it into smaller files."));
                    return;
                }

                foreach (var c in blankHeaderColumns)
                {
                    if (!string.IsNullOrEmpty(sheet.Cells[r, c].Formula) || !string.IsNullOrEmpty(CellText(sheet.Cells[r, c].Value)))
                    {
                        result.FileErrors.Add(FileError("column_unnamed", $"Column {c} contains data but has no header. Remove the data or use the template columns."));
                        result.Rows.Clear();
                        result.CellErrors.Clear();
                        return;
                    }
                }

                var row = new TestPlanImportRowModel { RowNumber = r };
                foreach (var (col, key) in columnMap)
                {
                    var cell = sheet.Cells[r, col];
                    if (!string.IsNullOrEmpty(cell.Formula))
                    {
                        result.CellErrors.Add(new TestPlanImportValidationError { RowNumber = r, Field = key, Code = "formula_not_allowed", Message = "Formulas are not supported. Enter the value directly." });
                        continue;
                    }
                    SetField(row, key, CellText(cell.Value));
                }
                result.Rows.Add(row);
            }

            if (result.Rows.Count == 0)
                result.FileErrors.Add(FileError("no_rows", "The file contains no Test Plan rows to import."));
        }

        private static void SetField(TestPlanImportRowModel row, string key, string value)
        {
            switch (key)
            {
                case TestPlanImportField.ProjectId: row.ProjectId = value; break;
                case TestPlanImportField.Name: row.Name = value; break;
                case TestPlanImportField.AreaId: row.AreaId = value; break;
                case TestPlanImportField.WorkspaceId: row.WorkspaceId = value; break;
                case TestPlanImportField.PlanMakerId: row.PlanMakerId = value; break;
                case TestPlanImportField.PlanReviewerId: row.PlanReviewerId = value; break;
                case TestPlanImportField.Description: row.Description = value; break;
            }
        }

        private static string CellText(object value)
        {
            if (value == null) return null;
            var text = value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
            text = text?.Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }

        private static TestPlanImportValidationError FileError(string code, string message) =>
            new() { RowNumber = 0, Field = null, Code = code, Message = message };
        #endregion

        #region Validate
        // Title: ValidateAsync
        // Description: Applies Test Plan creation rules to every row (required fields, name 1-200, lookup
        // eligibility within the current site/user context, in-file and existing site/project case-insensitive
        // duplicates). Used by both preview and confirmation; client-supplied flags are never consulted.
        public async Task<TestPlanImportPreviewModel> ValidateAsync(IList<TestPlanImportRowModel> rows, string siteId, string loggedUserId, IEnumerable<TestPlanImportValidationError> cellErrors = null)
        {
            var preview = new TestPlanImportPreviewModel();
            rows ??= new List<TestPlanImportRowModel>();

            if (rows.Count == 0)
            {
                preview.FileErrors.Add(FileError("no_rows", "There are no Test Plan rows to import."));
                return preview;
            }
            if (rows.Count > TestPlanImportField.MaxDataRows)
            {
                preview.FileErrors.Add(FileError("too_many_rows", $"A maximum of {TestPlanImportField.MaxDataRows} rows can be imported at once."));
                return preview;
            }

            var lookups = await LoadLookupsAsync(siteId, loggedUserId);
            var cellErrorsByRow = (cellErrors ?? Enumerable.Empty<TestPlanImportValidationError>())
                .GroupBy(e => e.RowNumber).ToDictionary(g => g.Key, g => g.ToList());
            var rowNumberCounts = rows.Where(r => r != null).GroupBy(r => r.RowNumber).ToDictionary(g => g.Key, g => g.Count());

            foreach (var source in rows)
            {
                if (source == null) continue;
                var row = new TestPlanImportPreviewRow
                {
                    RowNumber = source.RowNumber,
                    ProjectId = Clean(source.ProjectId),
                    Name = Clean(source.Name),
                    AreaId = Clean(source.AreaId),
                    WorkspaceId = Clean(source.WorkspaceId),
                    PlanMakerId = Clean(source.PlanMakerId),
                    PlanReviewerId = Clean(source.PlanReviewerId),
                    Description = Clean(source.Description),
                };
                if (cellErrorsByRow.TryGetValue(row.RowNumber, out var ce)) row.Errors.AddRange(ce);

                if (row.RowNumber <= 0 || rowNumberCounts[source.RowNumber] > 1)
                    AddError(row, null, "row_number_invalid", "Row number is missing or duplicated.");

                // Project
                if (row.ProjectId == null) { if (!HasError(row, TestPlanImportField.ProjectId)) AddError(row, TestPlanImportField.ProjectId, "required", "Project is required"); }
                else ApplyLookup(row, TestPlanImportField.ProjectId, row.ProjectId, lookups.Projects, "Project", "was not found, is archived, or you do not have access to it",
                    (id, label) => { row.ProjectId = id; row.ProjectName = label; });

                // Name
                if (row.Name == null) { if (!HasError(row, TestPlanImportField.Name)) AddError(row, TestPlanImportField.Name, "required", "Name is required"); }
                else if (row.Name.Length > TestPlanImportField.NameMaxLength)
                    AddError(row, TestPlanImportField.Name, "max_length", $"Name must be at most {TestPlanImportField.NameMaxLength} characters.");

                // Optional dropdowns
                if (row.AreaId != null)
                    ApplyLookup(row, TestPlanImportField.AreaId, row.AreaId, lookups.Areas, "Area", "was not found for this site",
                        (id, label) => { row.AreaId = id; row.AreaName = label; });
                if (row.WorkspaceId != null)
                    ApplyLookup(row, TestPlanImportField.WorkspaceId, row.WorkspaceId, lookups.Workspaces, "Workspace", "was not found for this site",
                        (id, label) => { row.WorkspaceId = id; row.WorkspaceName = label; });

                // Employees
                if (row.PlanMakerId == null) { if (!HasError(row, TestPlanImportField.PlanMakerId)) AddError(row, TestPlanImportField.PlanMakerId, "required", "Plan Maker is required"); }
                else ApplyLookup(row, TestPlanImportField.PlanMakerId, row.PlanMakerId, lookups.Employees, "Plan Maker", "was not found or is not an active employee",
                    (id, label) => { row.PlanMakerId = id; row.PlanMakerName = label; });

                if (row.PlanReviewerId == null) { if (!HasError(row, TestPlanImportField.PlanReviewerId)) AddError(row, TestPlanImportField.PlanReviewerId, "required", "Plan Reviewer is required"); }
                else ApplyLookup(row, TestPlanImportField.PlanReviewerId, row.PlanReviewerId, lookups.Employees, "Plan Reviewer", "was not found or is not an active employee",
                    (id, label) => { row.PlanReviewerId = id; row.PlanReviewerName = label; });

                if (row.Description != null && row.Description.Length > DescriptionMaxLength)
                    AddError(row, TestPlanImportField.Description, "max_length", $"Description must be at most {DescriptionMaxLength} characters.");

                preview.Rows.Add(row);
            }

            // In-file duplicates: same project + case-insensitive name (existing rule).
            var candidates = preview.Rows.Where(r => r.ProjectName != null && r.Name != null && !HasError(r, TestPlanImportField.Name)).ToList();
            foreach (var group in candidates.GroupBy(r => (Project: r.ProjectId.ToLowerInvariant(), Name: r.Name.ToLowerInvariant())).Where(g => g.Count() > 1))
            {
                var rowList = string.Join(", ", group.Select(r => r.RowNumber));
                foreach (var r in group)
                    AddError(r, TestPlanImportField.Name, "duplicate_in_file", $"Duplicate test plan name for this project in the file (rows {rowList}).");
            }

            // Existing active plans in the same site/project (reuses manual creation duplicate check).
            foreach (var r in candidates)
            {
                var exists = await _testPlanService.GetTestPlanByName(siteId, r.Name, r.ProjectId);
                if (exists != null)
                    AddError(r, TestPlanImportField.Name, "duplicate_existing", "The test plan already exists");
            }

            foreach (var r in preview.Rows) r.IsValid = r.Errors.Count == 0;
            preview.TotalRows = preview.Rows.Count;
            preview.ValidRows = preview.Rows.Count(r => r.IsValid);
            preview.InvalidRows = preview.TotalRows - preview.ValidRows;
            preview.CanConfirm = preview.FileErrors.Count == 0 && preview.TotalRows > 0 && preview.InvalidRows == 0;
            return preview;
        }

        private static string Clean(string value)
        {
            var v = value?.Trim();
            return string.IsNullOrEmpty(v) ? null : v;
        }

        // Resolves a dropdown option, unique label or record ID to the canonical record ID; never guesses.
        private static void ApplyLookup(TestPlanImportPreviewRow row, string field, string value, ExcelImportLookupSet set,
            string fieldLabel, string notFoundText, Action<string, string> onFound)
        {
            var match = set.Resolve(value);
            switch (match.Status)
            {
                case ExcelImportLookupStatus.Found:
                    onFound(match.Id, match.Label);
                    break;
                case ExcelImportLookupStatus.Ambiguous:
                    AddError(row, field, "lookup_ambiguous", $"{fieldLabel} '{value}' matches more than one record. Select the entry from the dropdown list.");
                    break;
                default:
                    AddError(row, field, "lookup_invalid", $"{fieldLabel} '{value}' {notFoundText}. Select a value from the dropdown list.");
                    break;
            }
        }

        private static bool HasError(TestPlanImportPreviewRow row, string field) => row.Errors.Any(e => e.Field == field);

        private static void AddError(TestPlanImportPreviewRow row, string field, string code, string message) =>
            row.Errors.Add(new TestPlanImportValidationError { RowNumber = row.RowNumber, Field = field, Code = code, Message = message });
        #endregion

        #region Description
        // Title: ToDescriptionHtml
        // Description: Converts plain-text Description to the rich-text HTML representation stored by the
        // manual form. Content is HTML-encoded, so workbook text can never inject markup or <img> data.
        public string ToDescriptionHtml(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText)) return null;
            var lines = plainText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            return "<p>" + string.Join("<br>", lines.Select(WebUtility.HtmlEncode)) + "</p>";
        }
        #endregion
    }
}
