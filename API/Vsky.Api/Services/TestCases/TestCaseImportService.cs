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
using Vsky.Services.Issues;
using Vsky.Services.ProjectModules;
using Vsky.Services.Projects;
using Vsky.Services.Requirements;
using Vsky.Services.Sites;
using Vsky.Services.TestCases;
using Vsky.Services.TestPlans;

namespace Vsky.Api.Services.TestCases
{
    public class TestCaseImportParseResult
    {
        public List<TestCaseImportValidationError> FileErrors { get; } = new();
        public List<TestCaseImportValidationError> CellErrors { get; } = new();
        public List<TestCaseImportRowModel> Rows { get; } = new();
    }

    // Title: TestCaseImportContext
    // Description: Lookups the current user may reference, loaded with the same services that back the Add
    // Test Case form dropdowns. Project-dependent lists (plans/modules) and module-dependent requirements are
    // loaded lazily and cached per request. Read-only.
    public class TestCaseImportContext
    {
        private readonly Func<string, Task<ExcelImportLookupSet>> _loadPlans;
        private readonly Func<string, Task<ExcelImportLookupSet>> _loadModules;
        private readonly Func<string, string, Task<ExcelImportLookupSet>> _loadRequirements;
        private readonly Dictionary<string, ExcelImportLookupSet> _plans = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ExcelImportLookupSet> _modules = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ExcelImportLookupSet> _requirements = new(StringComparer.OrdinalIgnoreCase);

        public TestCaseImportContext(
            Func<string, Task<ExcelImportLookupSet>> loadPlans,
            Func<string, Task<ExcelImportLookupSet>> loadModules,
            Func<string, string, Task<ExcelImportLookupSet>> loadRequirements)
        {
            _loadPlans = loadPlans;
            _loadModules = loadModules;
            _loadRequirements = loadRequirements;
        }

        public ExcelImportLookupSet Projects { get; set; } = new();
        public ExcelImportLookupSet Employees { get; set; } = new();
        public ExcelImportLookupSet Areas { get; set; } = new();
        public ExcelImportLookupSet Workspaces { get; set; } = new();
        public ExcelImportLookupSet Statuses { get; set; } = new();
        public string SiteToday { get; set; }

        public async Task<ExcelImportLookupSet> PlansFor(string projectId)
        {
            if (!_plans.TryGetValue(projectId, out var set)) _plans[projectId] = set = await _loadPlans(projectId);
            return set;
        }

        public async Task<ExcelImportLookupSet> ModulesFor(string projectId)
        {
            if (!_modules.TryGetValue(projectId, out var set)) _modules[projectId] = set = await _loadModules(projectId);
            return set;
        }

        public async Task<ExcelImportLookupSet> RequirementsFor(string projectId, string moduleId)
        {
            var key = projectId + "|" + moduleId;
            if (!_requirements.TryGetValue(key, out var set)) _requirements[key] = set = await _loadRequirements(projectId, moduleId);
            return set;
        }
    }

    public interface ITestCaseImportService
    {
        Task<byte[]> BuildTemplateAsync(string siteId, string loggedUserId);
        TestCaseImportParseResult ParseWorkbook(IFormFile file);
        Task<TestCaseImportPreviewModel> ValidateAsync(IList<TestCaseImportRowModel> rows, string siteId, string loggedUserId, IEnumerable<TestCaseImportValidationError> cellErrors = null);
        string ToRichTextHtml(string plainText);
    }

    // Title: TestCaseImportService
    // Description: Template generation, in-memory workbook parsing and server-side validation for Test Case
    // Excel import. Strictly read-only: no case/issue/lookup/mapping/execution writes, no number allocation,
    // no image processing and no SaveChanges — including for Fail rows. Persistence is done by the controller
    // through the shared manual creation helper after a fully valid batch.
    public class TestCaseImportService : ITestCaseImportService
    {
        public static readonly string[] AcceptedDateFormats = { "MM/dd/yyyy", "M/d/yyyy" };

        private readonly ITestCaseService _testCaseService;
        private readonly ITestPlanService _testPlanService;
        private readonly IProjectService _projectService;
        private readonly IProjectModuleService _projectModuleService;
        private readonly IRequirementService _requirementService;
        private readonly IEmployeeService _employeeService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly IDropDownService _dropDownService;
        private readonly IIssueService _issueService;
        private readonly ISiteService _siteService;
        private readonly ICommonService _commonService;

        public TestCaseImportService(
            ITestCaseService testCaseService,
            ITestPlanService testPlanService,
            IProjectService projectService,
            IProjectModuleService projectModuleService,
            IRequirementService requirementService,
            IEmployeeService employeeService,
            IDropDownTypeService dropDownTypeService,
            IDropDownService dropDownService,
            IIssueService issueService,
            ISiteService siteService,
            ICommonService commonService)
        {
            _testCaseService = testCaseService;
            _testPlanService = testPlanService;
            _projectService = projectService;
            _projectModuleService = projectModuleService;
            _requirementService = requirementService;
            _employeeService = employeeService;
            _dropDownTypeService = dropDownTypeService;
            _dropDownService = dropDownService;
            _issueService = issueService;
            _siteService = siteService;
            _commonService = commonService;
        }

        #region Lookups
        // Title: LoadContextAsync
        // Description: Same sources as the form: scoped non-template/non-archived projects; that project's test
        // plans and open (non-moved, non-closed) modules; that module's requirements; active site employees; and
        // the site's Area / Workspace / Test Case Status dropdown values.
        public virtual async Task<TestCaseImportContext> LoadContextAsync(string siteId, string loggedUserId)
        {
            var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(siteId, loggedUserId);

            var context = new TestCaseImportContext(
                async projectId => ToSet(await _testPlanService.GetAllTestPlansListForDropdown(siteId, projectId)),
                async projectId => ToSet(await _projectModuleService.GetAllProjectModuleListForDropdown(siteId, false, false, projectId)),
                async (projectId, moduleId) => ToSet(await _requirementService.GetAllRequirementListForDropdown(siteId, moduleId, projectId)));

            context.Projects = ToSet(await _projectService.GetProjectsListForDropdown(siteId, loggedUserId, employeeId, false, "false", false));

            foreach (var e in await _employeeService.GetAllActiveEmployeeListForDropdown(siteId) ?? new())
                context.Employees.Add(e.Id, e.Person?.FullName?.Trim());

            context.Areas = await LoadDropDownAsync(siteId, "Area");
            context.Workspaces = await LoadDropDownAsync(siteId, "Workspace");
            context.Statuses = await LoadDropDownAsync(siteId, "Test Case Status");

            var site = await _siteService.GetById(siteId);
            context.SiteToday = _siteService.GetDateTime(site?.TimeZone).Date.ToString(TestCaseImportField.DateFormat, CultureInfo.InvariantCulture);
            return context;
        }

        private static ExcelImportLookupSet ToSet(IEnumerable<Vsky.Models.CommonDropDown> items)
        {
            var set = new ExcelImportLookupSet();
            foreach (var i in items ?? Enumerable.Empty<Vsky.Models.CommonDropDown>()) set.Add(i.Value, i.Text);
            return set;
        }

        private async Task<ExcelImportLookupSet> LoadDropDownAsync(string siteId, string type)
        {
            var result = new ExcelImportLookupSet();
            var dropDownType = await _dropDownTypeService.GetDropDownType(siteId, type);
            if (dropDownType == null) return result;
            foreach (var d in await _dropDownService.GetDropDowns(dropDownType.Id) ?? new List<Vsky.Models.DropDown>())
                result.Add(d.Id, d.DropDownValue);
            return result;
        }

        private static string FindStatusId(ExcelImportLookupSet statuses, string label)
        {
            var match = statuses.Resolve(label);
            return match.Status == ExcelImportLookupStatus.Found ? match.Id : null;
        }
        #endregion

        #region Template
        // Title: BuildTemplateAsync
        // Description: Generates the workbook from the allowlisted field metadata. Lookup columns are Excel
        // dropdowns using the form's lists; Test Plan and Project Module lists depend on the chosen Project and
        // Requirement on the chosen Module (named ranges + INDIRECT), mirroring the form's cascading dropdowns.
        public async Task<byte[]> BuildTemplateAsync(string siteId, string loggedUserId)
        {
            var ctx = await LoadContextAsync(siteId, loggedUserId);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var data = package.Workbook.Worksheets.Add(TestCaseImportField.DataSheetName);
            for (int i = 0; i < TestCaseImportField.All.Count; i++)
            {
                var field = TestCaseImportField.All[i];
                var cell = data.Cells[1, i + 1];
                cell.Value = field.Header;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(field.Required ? System.Drawing.Color.LightSteelBlue : System.Drawing.Color.LightGray);
                cell.AddComment(field.Guidance, "Template");
                // Keep values as text so Excel does not reformat them; Tested Date is a real date column.
                data.Column(i + 1).Style.Numberformat.Format = field.Key == TestCaseImportField.TestedDate ? "mm/dd/yyyy" : "@";
                data.Column(i + 1).Width = TestCaseImportField.RichTextKeys.Contains(field.Key) ? 50 : 30;

                // Lock header
                cell.Style.Locked = true;

                // Allow users to edit data rows
                data.Cells[2, i + 1, TestCaseImportField.MaxDataRows + 1, i + 1].Style.Locked = false;
            }
            data.View.FreezePanes(2, 1);

            AddInstructions(package);

            var blank = package.Workbook.Worksheets.Add("_Keys");
            blank.Hidden = eWorkSheetHidden.Hidden;
            blank.Cells[1, 1].Value = "Project|Module key";
            blank.Cells[1, 2].Value = "Requirement list";
            var blankCell = blank.Cells["Z1"];

            // Projects (flat)
            var projectOptions = ctx.Projects.Options();
            var projectsSheet = AddOptionSheet(package, "Projects", new[] { "Project Name", "Project ID" });
            for (int i = 0; i < projectOptions.Count; i++)
            {
                projectsSheet.Cells[i + 2, 1].Value = projectOptions[i].Key;
                projectsSheet.Cells[i + 2, 2].Value = projectOptions[i].Value;
            }
            if (projectOptions.Count > 0)
                package.Workbook.Names.Add("ProjectOptions", projectsSheet.Cells[2, 1, projectOptions.Count + 1, 1]);

            // Project-dependent plans/modules and module-dependent requirements
            var plansSheet = AddOptionSheet(package, "Test Plans", new[] { "Project Name", "Test Plan Name", "Test Plan ID" });
            var modulesSheet = AddOptionSheet(package, "Project Modules", new[] { "Project Name", "Project Module", "Project Module ID" });
            var reqSheet = AddOptionSheet(package, "Requirements", new[] { "Project Name", "Project Module", "Requirement", "Requirement ID" });
            int planRow = 2, moduleRow = 2, reqRow = 2, keyRow = 2, reqListIndex = 0;

            for (int p = 0; p < projectOptions.Count; p++)
            {
                var (projectLabel, projectId) = (projectOptions[p].Key, projectOptions[p].Value);

                var planStart = planRow;
                foreach (var plan in (await ctx.PlansFor(projectId)).Options())
                {
                    plansSheet.Cells[planRow, 1].Value = projectLabel;
                    plansSheet.Cells[planRow, 2].Value = plan.Key;
                    plansSheet.Cells[planRow, 3].Value = plan.Value;
                    planRow++;
                }
                package.Workbook.Names.Add($"TP_{p + 1}", planRow > planStart ? plansSheet.Cells[planStart, 2, planRow - 1, 2] : blankCell);

                var moduleStart = moduleRow;
                foreach (var module in (await ctx.ModulesFor(projectId)).Options())
                {
                    modulesSheet.Cells[moduleRow, 1].Value = projectLabel;
                    modulesSheet.Cells[moduleRow, 2].Value = module.Key;
                    modulesSheet.Cells[moduleRow, 3].Value = module.Value;
                    moduleRow++;

                    var reqStart = reqRow;
                    foreach (var req in (await ctx.RequirementsFor(projectId, module.Value)).Options())
                    {
                        reqSheet.Cells[reqRow, 1].Value = projectLabel;
                        reqSheet.Cells[reqRow, 2].Value = module.Key;
                        reqSheet.Cells[reqRow, 3].Value = req.Key;
                        reqSheet.Cells[reqRow, 4].Value = req.Value;
                        reqRow++;
                    }
                    var reqName = $"RQ_{++reqListIndex}";
                    package.Workbook.Names.Add(reqName, reqRow > reqStart ? reqSheet.Cells[reqStart, 3, reqRow - 1, 3] : blankCell);
                    blank.Cells[keyRow, 1].Value = projectLabel + "|" + module.Key;
                    blank.Cells[keyRow, 2].Value = reqName;
                    keyRow++;
                }
                package.Workbook.Names.Add($"PM_{p + 1}", moduleRow > moduleStart ? modulesSheet.Cells[moduleStart, 2, moduleRow - 1, 2] : blankCell);
            }
            package.Workbook.Names.Add("ModuleKeys", blank.Cells[2, 1, Math.Max(keyRow - 1, 2), 2]);

            var statusRange = AddFlatOptionSheet(package, "Statuses", "Status", "Status ID", ctx.Statuses);
            var employeeRange = AddFlatOptionSheet(package, "Employees", "Employee Name", "Employee ID", ctx.Employees);
            var areaRange = AddFlatOptionSheet(package, "Areas", "Area", "Area ID", ctx.Areas);
            var workspaceRange = AddFlatOptionSheet(package, "Workspaces", "Workspace", "Workspace ID", ctx.Workspaces);

            string Col(string key) => ColumnLetter(ColumnIndex(key));
            var projectCell = $"${Col(TestCaseImportField.ProjectId)}2";
            var moduleCell = $"${Col(TestCaseImportField.ProjectModuleId)}2";

            if (projectOptions.Count > 0)
            {
                AddDropdown(data, TestCaseImportField.ProjectId, "ProjectOptions");
                AddDropdown(data, TestCaseImportField.PlanId, $"INDIRECT(\"TP_\"&MATCH({projectCell},ProjectOptions,0))", "Select the Project Name first.");
                AddDropdown(data, TestCaseImportField.ProjectModuleId, $"INDIRECT(\"PM_\"&MATCH({projectCell},ProjectOptions,0))", "Select the Project Name first.");
                AddDropdown(data, TestCaseImportField.RequirementId, $"INDIRECT(VLOOKUP({projectCell}&\"|\"&{moduleCell},ModuleKeys,2,FALSE))", "Select the Project Name and Project Module first.");
            }
            AddDropdown(data, TestCaseImportField.AreaId, areaRange);
            AddDropdown(data, TestCaseImportField.WorkspaceId, workspaceRange);
            AddDropdown(data, TestCaseImportField.StatusId, statusRange);
            AddDropdown(data, TestCaseImportField.TestedBy, employeeRange);
            AddFailOnlyAssignedTo(data, ctx, employeeRange, "_Keys!$Z$1");
            AddTestedDatePicker(data);

            // Hide all sheets except Test Case and Instructions
            foreach (var sheet in package.Workbook.Worksheets)
            {
                if (sheet.Name != TestCaseImportField.DataSheetName &&
                    sheet.Name != "Instructions")
                {
                    sheet.Hidden = eWorkSheetHidden.Hidden;
                }
            }

            // Protect worksheet
            data.Protection.IsProtected = true;
            data.Protection.AllowSelectLockedCells = false;
            data.Protection.AllowSelectUnlockedCells = true;

            data.Protection.AllowInsertRows = false;
            data.Protection.AllowDeleteRows = false;
            data.Protection.AllowInsertColumns = false;
            data.Protection.AllowDeleteColumns = false;

            data.Protection.AllowFormatCells = false;
            data.Protection.AllowFormatColumns = false;
            data.Protection.AllowFormatRows = false;

            data.Protection.AllowSort = false;
            data.Protection.AllowAutoFilter = false;

            data.Protection.AllowEditObject = false;
            data.Protection.AllowEditScenarios = false;

            return package.GetAsByteArray();
        }

        private static void AddInstructions(ExcelPackage package)
        {
            var info = package.Workbook.Worksheets.Add("Instructions");
            info.Cells[1, 1].Value = "Test Case Import Template";
            info.Cells[1, 1].Style.Font.Bold = true;
            info.Cells[2, 1].Value = "Template version";
            info.Cells[2, 2].Value = TestCaseImportField.TemplateVersion;
            info.Cells[3, 1].Value = $"Fill one Test Case per row on the '{TestCaseImportField.DataSheetName}' sheet, starting at row 2. Choose Project Name first, then Test Plan Name and Project Module, then Requirement. Do not rename, remove or add columns. Formulas are not accepted. Maximum {TestCaseImportField.MaxDataRows} rows. If any row is invalid, nothing is imported. Only new test cases are created; existing cases are never updated.";
            info.Cells[5, 1].Value = "Column";
            info.Cells[5, 2].Value = "Required";
            info.Cells[5, 3].Value = "Guidance";
            info.Cells[5, 1, 5, 3].Style.Font.Bold = true;
            for (int i = 0; i < TestCaseImportField.All.Count; i++)
            {
                var f = TestCaseImportField.All[i];
                info.Cells[6 + i, 1].Value = f.Header;
                info.Cells[6 + i, 2].Value = f.Required ? "Yes" : "No";
                info.Cells[6 + i, 3].Value = f.Guidance;
            }
            info.Column(1).Width = 20;
            info.Column(2).Width = 12;
            info.Column(3).Width = 110;
        }

        private static ExcelWorksheet AddOptionSheet(ExcelPackage package, string name, string[] headers)
        {
            var sheet = package.Workbook.Worksheets.Add(name);
            for (int c = 0; c < headers.Length; c++)
            {
                sheet.Cells[1, c + 1].Value = headers[c];
                sheet.Cells[1, c + 1].Style.Font.Bold = true;
                sheet.Column(c + 1).Style.Numberformat.Format = "@";
                sheet.Column(c + 1).Width = 40;
            }
            return sheet;
        }

        private static string AddFlatOptionSheet(ExcelPackage package, string name, string optionHeader, string idHeader, ExcelImportLookupSet set)
        {
            var sheet = AddOptionSheet(package, name, new[] { optionHeader, idHeader });
            int row = 2;
            foreach (var o in set.Options())
            {
                sheet.Cells[row, 1].Value = o.Key;
                sheet.Cells[row, 2].Value = o.Value;
                row++;
            }
            return row == 2 ? null : $"'{name}'!$A$2:$A${row - 1}";
        }

        // Assigned To applies only to failed cases: the dropdown offers employees only when the row's Status is
        // the site's "Fail" option (empty list otherwise), and the cell is greyed out / visually hidden by
        // conditional formatting while Status is not Fail. Excel cannot truly hide single cells; the backend
        // also rejects Assigned To on non-Fail rows.
        private static void AddFailOnlyAssignedTo(ExcelWorksheet data, TestCaseImportContext ctx, string employeeRange, string blankCell)
        {
            var failMatch = ctx.Statuses.Resolve("Fail");
            var failOption = failMatch.Status == ExcelImportLookupStatus.Found ? ctx.Statuses.DisplayLabel(failMatch.Id) : null;
            int col = ColumnIndex(TestCaseImportField.EmployeeId);
            var range = data.Cells[2, col, TestCaseImportField.MaxDataRows + 1, col];
            var statusCell = $"${ColumnLetter(ColumnIndex(TestCaseImportField.StatusId))}2";
            var isFail = failOption == null ? "FALSE" : $"{statusCell}=\"{failOption.Replace("\"", "\"\"")}\"";

            if (employeeRange != null)
            {
                AddDropdown(data, TestCaseImportField.EmployeeId, $"IF({isFail},{employeeRange},{blankCell})",
                    "Only available when Status is Fail.");
            }

            var greyOut = data.ConditionalFormatting.AddExpression(range);
            greyOut.Formula = $"NOT({isFail})";
            greyOut.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            greyOut.Style.Fill.BackgroundColor.Color = System.Drawing.Color.FromArgb(0xE0, 0xE0, 0xE0);
            greyOut.Style.Font.Color.Color = System.Drawing.Color.FromArgb(0xE0, 0xE0, 0xE0);
        }

        // Tested Date is a date-formatted column with date validation, so Excel only accepts real dates (Excel
        // stores them unambiguously; the parser uses the stored calendar date). A pop-up calendar is not part
        // of the .xlsx format — some Excel versions offer one for date cells, desktop Excel does not.
        private static void AddTestedDatePicker(ExcelWorksheet data)
        {
            int col = ColumnIndex(TestCaseImportField.TestedDate);
            var validation = data.DataValidations.AddDateTimeValidation(data.Cells[2, col, TestCaseImportField.MaxDataRows + 1, col].Address);
            validation.Operator = OfficeOpenXml.DataValidation.ExcelDataValidationOperator.between;
            validation.Formula.Value = new DateTime(2000, 1, 1);
            validation.Formula2.Value = new DateTime(2099, 12, 31);
            validation.AllowBlank = true;
            validation.ShowErrorMessage = true;
            validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
            validation.ErrorTitle = "Invalid Tested Date";
            validation.Error = "Enter a valid date (MM/dd/yyyy), for example 11/17/2026.";
            validation.ShowInputMessage = true;
            validation.PromptTitle = "Tested Date";
            validation.Prompt = "Optional. Enter or pick a date (MM/dd/yyyy). Blank uses today's date.";
        }

        private static void AddDropdown(ExcelWorksheet data, string fieldKey, string listFormula, string hint = null)
        {
            if (listFormula == null) return; // nothing selectable; backend reports the value as invalid
            int col = ColumnIndex(fieldKey);
            var field = TestCaseImportField.All[col - 1];
            var validation = data.DataValidations.AddListValidation(data.Cells[2, col, TestCaseImportField.MaxDataRows + 1, col].Address);
            validation.Formula.ExcelFormula = listFormula;
            validation.AllowBlank = true;
            validation.ShowErrorMessage = true;
            validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
            validation.ErrorTitle = $"Invalid {field.Header}";
            validation.Error = $"Please select a {field.Header} from the dropdown list." + (hint == null ? "" : " " + hint);
            validation.ShowInputMessage = true;
            validation.PromptTitle = field.Header;
            validation.Prompt = (field.Required ? "Required. " : "Optional. ") + (hint ?? "Select from the list.");
        }

        public static int ColumnIndex(string key) => TestCaseImportField.All.Select((f, i) => (f, i)).First(x => x.f.Key == key).i + 1;

        private static string ColumnLetter(int index) => ExcelCellAddress.GetColumnLetter(index);
        #endregion

        #region Parse
        // Title: ParseWorkbook
        // Description: Reads the uploaded .xlsx in memory; validates type/size/structure; rejects formula cells;
        // converts real Excel date cells to MM/dd/yyyy and leaves text dates for validation.
        public TestCaseImportParseResult ParseWorkbook(IFormFile file)
        {
            var result = new TestCaseImportParseResult();

            if (file == null || file.Length == 0)
            {
                result.FileErrors.Add(FileError("file_empty", "Please upload a non-empty Excel file."));
                return result;
            }
            if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                result.FileErrors.Add(FileError("file_type", "Only Excel .xlsx files created from the Test Case template are supported."));
                return result;
            }
            if (file.Length > TestCaseImportField.MaxFileSizeBytes)
            {
                result.FileErrors.Add(FileError("file_too_large", $"The file exceeds the maximum size of {TestCaseImportField.MaxFileSizeBytes / (1024 * 1024)} MB."));
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

        private static void ReadWorksheet(ExcelPackage package, TestCaseImportParseResult result)
        {
            var sheet = package.Workbook.Worksheets[TestCaseImportField.DataSheetName];
            if (sheet == null)
            {
                result.FileErrors.Add(FileError("sheet_missing", $"The '{TestCaseImportField.DataSheetName}' sheet was not found. Please use the downloaded template."));
                return;
            }
            if (sheet.Dimension == null)
            {
                result.FileErrors.Add(FileError("file_empty", "The template contains no headers or data."));
                return;
            }

            int lastCol = sheet.Dimension.End.Column;
            int lastRow = sheet.Dimension.End.Row;

            var columnMap = new Dictionary<int, string>();
            var byHeader = TestCaseImportField.All.ToDictionary(f => f.Header, f => f, StringComparer.OrdinalIgnoreCase);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var blankHeaderColumns = new List<int>();
            for (int c = 1; c <= lastCol; c++)
            {
                var header = CellText(sheet.Cells[1, c].Value);
                if (string.IsNullOrEmpty(header)) { blankHeaderColumns.Add(c); continue; }
                if (!byHeader.TryGetValue(header, out var field))
                {
                    result.FileErrors.Add(FileError("column_unknown", $"Column '{header}' is not part of the Test Case template."));
                    continue;
                }
                if (!seen.Add(field.Header))
                {
                    result.FileErrors.Add(FileError("column_duplicate", $"Column '{field.Header}' appears more than once."));
                    continue;
                }
                columnMap[c] = field.Key;
            }
            foreach (var missing in TestCaseImportField.All.Where(f => !seen.Contains(f.Header)))
                result.FileErrors.Add(new TestCaseImportValidationError { RowNumber = 0, Field = missing.Key, Code = "column_missing", Message = $"Template column '{missing.Header}' is missing." });

            if (result.FileErrors.Any()) return;

            int dataRows = 0;
            for (int r = 2; r <= lastRow; r++)
            {
                bool any = false;
                for (int c = 1; c <= lastCol && !any; c++)
                {
                    var cell = sheet.Cells[r, c];
                    any = !string.IsNullOrEmpty(cell.Formula) || !string.IsNullOrEmpty(CellText(cell.Value));
                }
                if (!any) continue;

                if (++dataRows > TestCaseImportField.MaxDataRows)
                {
                    result.Rows.Clear();
                    result.CellErrors.Clear();
                    result.FileErrors.Add(FileError("too_many_rows", $"The file contains more than {TestCaseImportField.MaxDataRows} data rows. Please split it into smaller files."));
                    return;
                }

                foreach (var c in blankHeaderColumns)
                {
                    if (!string.IsNullOrEmpty(sheet.Cells[r, c].Formula) || !string.IsNullOrEmpty(CellText(sheet.Cells[r, c].Value)))
                    {
                        result.Rows.Clear();
                        result.CellErrors.Clear();
                        result.FileErrors.Add(FileError("column_unnamed", $"Column {c} contains data but has no header. Remove the data or use the template columns."));
                        return;
                    }
                }

                var row = new TestCaseImportRowModel { RowNumber = r };
                foreach (var (col, key) in columnMap)
                {
                    var cell = sheet.Cells[r, col];
                    if (!string.IsNullOrEmpty(cell.Formula))
                    {
                        result.CellErrors.Add(new TestCaseImportValidationError { RowNumber = r, Field = key, Code = "formula_not_allowed", Message = "Formulas are not supported. Enter the value directly." });
                        continue;
                    }

                    if (key == TestCaseImportField.TestedDate)
                    {
                        var (text, error) = ReadDateCell(cell);
                        if (error != null) result.CellErrors.Add(new TestCaseImportValidationError { RowNumber = r, Field = key, Code = "date_invalid", Message = error });
                        else SetField(row, key, text);
                        continue;
                    }
                    SetField(row, key, CellText(cell.Value));
                }
                result.Rows.Add(row);
            }

            if (result.Rows.Count == 0)
                result.FileErrors.Add(FileError("no_rows", "The file contains no Test Case rows to import."));
        }

        // Real Excel dates (DateTime, or a number in a date-formatted cell) become MM/dd/yyyy using the stored
        // calendar date only. Bare numbers are rejected as ambiguous serials. Text is validated later.
        private static (string Text, string Error) ReadDateCell(ExcelRange cell)
        {
            var value = cell.Value;
            switch (value)
            {
                case null: return (null, null);
                case DateTime dt: return (dt.Date.ToString(TestCaseImportField.DateFormat, CultureInfo.InvariantCulture), null);
                case double d:
                    var fmt = cell.Style?.Numberformat?.Format ?? "";
                    var looksLikeDate = fmt.IndexOfAny(new[] { 'y', 'Y', 'd', 'D' }) >= 0 && fmt != "@";
                    if (looksLikeDate && d >= 1 && d < 2958466)
                        return (DateTime.FromOADate(d).Date.ToString(TestCaseImportField.DateFormat, CultureInfo.InvariantCulture), null);
                    return (null, $"Tested Date must be entered as {TestCaseImportField.DateFormat} (for example 11/17/2026).");
                default:
                    return (CellText(value), null);
            }
        }

        private static void SetField(TestCaseImportRowModel row, string key, string value)
        {
            switch (key)
            {
                case TestCaseImportField.ProjectId: row.ProjectId = value; break;
                case TestCaseImportField.PlanId: row.PlanId = value; break;
                case TestCaseImportField.ProjectModuleId: row.ProjectModuleId = value; break;
                case TestCaseImportField.RequirementId: row.RequirementId = value; break;
                case TestCaseImportField.Name: row.Name = value; break;
                case TestCaseImportField.AreaId: row.AreaId = value; break;
                case TestCaseImportField.WorkspaceId: row.WorkspaceId = value; break;
                case TestCaseImportField.StatusId: row.StatusId = value; break;
                case TestCaseImportField.EmployeeId: row.EmployeeId = value; break;
                case TestCaseImportField.TestedBy: row.TestedBy = value; break;
                case TestCaseImportField.TestedDate: row.TestedDate = value; break;
                case TestCaseImportField.Description: row.Description = value; break;
                case TestCaseImportField.Steps: row.Steps = value; break;
                case TestCaseImportField.ExpectedResult: row.ExpectedResult = value; break;
                case TestCaseImportField.ActualResult: row.ActualResult = value; break;
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

        private static TestCaseImportValidationError FileError(string code, string message) =>
            new() { RowNumber = 0, Field = null, Code = code, Message = message };
        #endregion

        #region Validate
        // Title: ValidateAsync
        // Description: Applies the Add Test Case form rules to every row: required project/plan/module/
        // requirement/name/tested-by; name 1-200; plan & module belong to the project and requirement to the
        // project/module; eligible employees/status/area/workspace; MM/dd/yyyy tested date (blank = site today);
        // blank status = "New"; in-file and existing duplicates (site/project/plan/module + case-insensitive
        // name). Predicts the Fail-issue outcome read-only. Used by preview and by confirmation.
        public async Task<TestCaseImportPreviewModel> ValidateAsync(IList<TestCaseImportRowModel> rows, string siteId, string loggedUserId, IEnumerable<TestCaseImportValidationError> cellErrors = null)
        {
            var preview = new TestCaseImportPreviewModel();
            rows ??= new List<TestCaseImportRowModel>();

            if (rows.Count == 0)
            {
                preview.FileErrors.Add(FileError("no_rows", "There are no Test Case rows to import."));
                return preview;
            }
            if (rows.Count > TestCaseImportField.MaxDataRows)
            {
                preview.FileErrors.Add(FileError("too_many_rows", $"A maximum of {TestCaseImportField.MaxDataRows} rows can be imported at once."));
                return preview;
            }

            var ctx = await LoadContextAsync(siteId, loggedUserId);
            var newStatusId = FindStatusId(ctx.Statuses, "New");
            var failStatusId = FindStatusId(ctx.Statuses, "Fail");
            var cellErrorsByRow = (cellErrors ?? Enumerable.Empty<TestCaseImportValidationError>())
                .GroupBy(e => e.RowNumber).ToDictionary(g => g.Key, g => g.ToList());
            var rowNumberCounts = rows.Where(r => r != null).GroupBy(r => r.RowNumber).ToDictionary(g => g.Key, g => g.Count());

            foreach (var source in rows)
            {
                if (source == null) continue;
                var row = new TestCaseImportPreviewRow
                {
                    RowNumber = source.RowNumber,
                    ProjectId = Clean(source.ProjectId),
                    PlanId = Clean(source.PlanId),
                    ProjectModuleId = Clean(source.ProjectModuleId),
                    RequirementId = Clean(source.RequirementId),
                    Name = Clean(source.Name),
                    AreaId = Clean(source.AreaId),
                    WorkspaceId = Clean(source.WorkspaceId),
                    StatusId = Clean(source.StatusId),
                    EmployeeId = Clean(source.EmployeeId),
                    TestedBy = Clean(source.TestedBy),
                    TestedDate = Clean(source.TestedDate),
                    Description = Clean(source.Description),
                    Steps = Clean(source.Steps),
                    ExpectedResult = Clean(source.ExpectedResult),
                    ActualResult = Clean(source.ActualResult),
                };
                if (cellErrorsByRow.TryGetValue(row.RowNumber, out var ce)) row.Errors.AddRange(ce);

                if (row.RowNumber <= 0 || rowNumberCounts[source.RowNumber] > 1)
                    AddError(row, null, "row_number_invalid", "Row number is missing or duplicated.");

                // Project, then project-dependent plan/module, then module-dependent requirement
                bool projectOk = Required(row, TestCaseImportField.ProjectId, row.ProjectId, "Project name is required") &&
                    Apply(row, TestCaseImportField.ProjectId, row.ProjectId, ctx.Projects, "Project", "was not found, is archived, or you do not have access to it",
                        (id, label) => { row.ProjectId = id; row.ProjectName = label; });

                bool moduleOk = false;
                if (Required(row, TestCaseImportField.PlanId, row.PlanId, "Test plan is required"))
                {
                    if (projectOk)
                        Apply(row, TestCaseImportField.PlanId, row.PlanId, await ctx.PlansFor(row.ProjectId), "Test Plan", "does not belong to the selected project",
                            (id, label) => { row.PlanId = id; row.PlanName = label; });
                    else AddDependent(row, TestCaseImportField.PlanId, "Project Name");
                }
                if (Required(row, TestCaseImportField.ProjectModuleId, row.ProjectModuleId, "Project module is required"))
                {
                    if (projectOk)
                        moduleOk = Apply(row, TestCaseImportField.ProjectModuleId, row.ProjectModuleId, await ctx.ModulesFor(row.ProjectId), "Project Module", "does not belong to the selected project or is closed",
                            (id, label) => { row.ProjectModuleId = id; row.ProjectModuleName = label; });
                    else AddDependent(row, TestCaseImportField.ProjectModuleId, "Project Name");
                }
                if (Required(row, TestCaseImportField.RequirementId, row.RequirementId, "Requirement is required"))
                {
                    if (projectOk && moduleOk)
                        Apply(row, TestCaseImportField.RequirementId, row.RequirementId, await ctx.RequirementsFor(row.ProjectId, row.ProjectModuleId), "Requirement", "does not belong to the selected project module",
                            (id, label) => { row.RequirementId = id; row.RequirementName = label; });
                    else AddDependent(row, TestCaseImportField.RequirementId, "Project Name and Project Module");
                }

                // Name
                if (Required(row, TestCaseImportField.Name, row.Name, "Name is required") && row.Name.Length > TestCaseImportField.NameMaxLength)
                    AddError(row, TestCaseImportField.Name, "max_length", $"Name must be at most {TestCaseImportField.NameMaxLength} characters.");

                // Optional dropdowns
                if (row.AreaId != null)
                    Apply(row, TestCaseImportField.AreaId, row.AreaId, ctx.Areas, "Area", "was not found for this site", (id, label) => { row.AreaId = id; row.AreaName = label; });
                if (row.WorkspaceId != null)
                    Apply(row, TestCaseImportField.WorkspaceId, row.WorkspaceId, ctx.Workspaces, "Workspace", "was not found for this site", (id, label) => { row.WorkspaceId = id; row.WorkspaceName = label; });

                if (row.StatusId == null && !HasError(row, TestCaseImportField.StatusId))
                {
                    row.StatusId = newStatusId; // form default
                    row.StatusName = newStatusId != null ? ctx.Statuses.Resolve(newStatusId).Label : null;
                }
                else if (row.StatusId != null)
                    Apply(row, TestCaseImportField.StatusId, row.StatusId, ctx.Statuses, "Status", "is not a Test Case status for this site", (id, label) => { row.StatusId = id; row.StatusName = label; });

                // Employees (Assigned To applies only to failed test cases)
                if (row.EmployeeId != null && !HasError(row, TestCaseImportField.StatusId) &&
                    (failStatusId == null || !string.Equals(row.StatusId, failStatusId, StringComparison.OrdinalIgnoreCase)))
                    AddError(row, TestCaseImportField.EmployeeId, "not_applicable", "Assigned To can only be set when Status is Fail. Clear it or set Status to Fail.");
                else if (row.EmployeeId != null)
                    Apply(row, TestCaseImportField.EmployeeId, row.EmployeeId, ctx.Employees, "Assigned To", "was not found or is not an active employee", (id, label) => { row.EmployeeId = id; row.EmployeeName = label; });
                if (Required(row, TestCaseImportField.TestedBy, row.TestedBy, "Tested By is required"))
                    Apply(row, TestCaseImportField.TestedBy, row.TestedBy, ctx.Employees, "Tested By", "was not found or is not an active employee", (id, label) => { row.TestedBy = id; row.TestedByName = label; });

                // Tested date
                if (row.TestedDate == null)
                {
                    if (!HasError(row, TestCaseImportField.TestedDate)) row.TestedDate = ctx.SiteToday; // form default
                }
                else if (DateTime.TryParseExact(row.TestedDate, AcceptedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var testedDate))
                    row.TestedDate = testedDate.ToString(TestCaseImportField.DateFormat, CultureInfo.InvariantCulture);
                else
                    AddError(row, TestCaseImportField.TestedDate, "date_invalid", $"Tested Date '{row.TestedDate}' is not a valid date in {TestCaseImportField.DateFormat} format.");

                foreach (var key in TestCaseImportField.RichTextKeys)
                {
                    var text = key switch
                    {
                        TestCaseImportField.Description => row.Description,
                        TestCaseImportField.Steps => row.Steps,
                        TestCaseImportField.ExpectedResult => row.ExpectedResult,
                        _ => row.ActualResult
                    };
                    if (text != null && text.Length > TestCaseImportField.TextMaxLength)
                        AddError(row, key, "max_length", $"{TestCaseImportField.All.First(f => f.Key == key).Header} must be at most {TestCaseImportField.TextMaxLength} characters.");
                }

                preview.Rows.Add(row);
            }

            // Duplicates: site/project/plan/module + case-insensitive name (existing manual rule)
            var candidates = preview.Rows.Where(r => r.ProjectName != null && r.PlanName != null && r.ProjectModuleName != null && r.Name != null && !HasError(r, TestCaseImportField.Name)).ToList();
            foreach (var group in candidates.GroupBy(r => (r.ProjectId.ToLowerInvariant(), r.PlanId.ToLowerInvariant(), r.ProjectModuleId.ToLowerInvariant(), r.Name.ToLowerInvariant())).Where(g => g.Count() > 1))
            {
                var rowList = string.Join(", ", group.Select(r => r.RowNumber));
                foreach (var r in group)
                    AddError(r, TestCaseImportField.Name, "duplicate_in_file", $"Duplicate test case name for this project, test plan and module in the file (rows {rowList}).");
            }
            foreach (var r in candidates)
            {
                if (await _testCaseService.GetTestCaseByName(siteId, r.ProjectId, r.PlanId, r.ProjectModuleId, r.Name) != null)
                    AddError(r, TestCaseImportField.Name, "duplicate_existing", "The test case already exists");
            }

            foreach (var r in preview.Rows) r.IsValid = r.Errors.Count == 0;

            // Fail issue outcome (read-only prediction of the existing manual rule: site/project + case-insensitive name)
            if (failStatusId != null)
            {
                var issuesInBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in preview.Rows.Where(r => r.IsValid && string.Equals(r.StatusId, failStatusId, StringComparison.OrdinalIgnoreCase)).OrderBy(r => r.RowNumber))
                {
                    var key = r.ProjectId + "|" + r.Name.ToLowerInvariant();
                    if (await _issueService.GetIssueByName(siteId, r.Name, r.ProjectId) != null) r.FailIssue = "exists";
                    else if (!issuesInBatch.Add(key)) r.FailIssue = "created_in_batch";
                    else r.FailIssue = "create";
                }
            }

            preview.TotalRows = preview.Rows.Count;
            preview.ValidRows = preview.Rows.Count(r => r.IsValid);
            preview.InvalidRows = preview.TotalRows - preview.ValidRows;
            preview.FailIssuesToCreate = preview.Rows.Count(r => r.FailIssue == "create");
            preview.CanConfirm = preview.FileErrors.Count == 0 && preview.TotalRows > 0 && preview.InvalidRows == 0;
            return preview;
        }

        private static bool Required(TestCaseImportPreviewRow row, string field, string value, string message)
        {
            if (value != null) return true;
            if (!HasError(row, field)) AddError(row, field, "required", message);
            return false;
        }

        // Resolves an option, unique label or record ID within the given (already context-scoped) set.
        private static bool Apply(TestCaseImportPreviewRow row, string field, string value, ExcelImportLookupSet set,
            string fieldLabel, string notFoundText, Action<string, string> onFound)
        {
            var match = set.Resolve(value);
            switch (match.Status)
            {
                case ExcelImportLookupStatus.Found:
                    onFound(match.Id, match.Label);
                    return true;
                case ExcelImportLookupStatus.Ambiguous:
                    AddError(row, field, "lookup_ambiguous", $"{fieldLabel} '{value}' matches more than one record. Select the entry from the dropdown list.");
                    return false;
                default:
                    AddError(row, field, "lookup_invalid", $"{fieldLabel} '{value}' {notFoundText}. Select a value from the dropdown list.");
                    return false;
            }
        }

        private static void AddDependent(TestCaseImportPreviewRow row, string field, string dependsOn) =>
            AddError(row, field, "context_invalid", $"Cannot be validated until a valid {dependsOn} is selected.");

        private static string Clean(string value)
        {
            var v = value?.Trim();
            return string.IsNullOrEmpty(v) ? null : v;
        }

        private static bool HasError(TestCaseImportPreviewRow row, string field) => row.Errors.Any(e => e.Field == field);

        private static void AddError(TestCaseImportPreviewRow row, string field, string code, string message) =>
            row.Errors.Add(new TestCaseImportValidationError { RowNumber = row.RowNumber, Field = field, Code = code, Message = message });
        #endregion

        #region Rich text
        // Title: ToRichTextHtml
        // Description: Converts plain text to the rich-text HTML stored by the form. Content is HTML-encoded, so
        // workbook text can never inject markup or <img> data (no image uploads occur on import).
        public string ToRichTextHtml(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText)) return null;
            var lines = plainText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            return "<p>" + string.Join("<br>", lines.Select(WebUtility.HtmlEncode)) + "</p>";
        }
        #endregion
    }
}
