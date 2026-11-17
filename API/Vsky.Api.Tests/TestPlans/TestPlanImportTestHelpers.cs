using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Moq;
using OfficeOpenXml;
using Vsky.Api.Models;
using Vsky.Api.Services.TestPlans;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.Employees;
using Vsky.Services.Projects;
using Vsky.Services.TestPlans;

namespace Vsky.Api.Tests.TestPlans
{
    // Mocked read services with one permitted project/employee/area/workspace per site, so tests run
    // without a database. "proj-other" / "emp-other" stand for records outside the user's context.
    public class ImportFixture
    {
        public const string SiteId = "site-1";
        public const string UserId = "user-1";
        public const string EmployeeId = "emp-self";

        public Mock<ITestPlanService> TestPlanService { get; } = new(MockBehavior.Loose);
        public Mock<IProjectService> ProjectService { get; } = new();
        public Mock<IEmployeeService> EmployeeService { get; } = new();
        public Mock<IDropDownTypeService> DropDownTypeService { get; } = new();
        public Mock<IDropDownService> DropDownService { get; } = new();
        public Mock<ICommonService> CommonService { get; } = new();

        public List<CommonDropDown> Projects { get; } = new()
        {
            new CommonDropDown { Value = "proj-1", Text = "Alpha Project" },
            new CommonDropDown { Value = "proj-2", Text = "Beta Project" },
        };

        public List<Employee> Employees { get; } = new()
        {
            new Employee { Id = "emp-1", Person = new Person { FullName = "Asha Rao" } },
            new Employee { Id = "emp-2", Person = new Person { FullName = "Ravi Kumar" } },
        };

        public ImportFixture()
        {
            CommonService.Setup(x => x.GetEmployeeIdByUserIdAndEmail(SiteId, UserId)).Returns(EmployeeId);
            ProjectService.Setup(x => x.GetProjectsListForDropdown(SiteId, UserId, EmployeeId, false, "false", false))
                .ReturnsAsync(() => Projects.ToList());
            EmployeeService.Setup(x => x.GetAllActiveEmployeeListForDropdown(SiteId, null))
                .ReturnsAsync(() => Employees.ToList());
            DropDownTypeService.Setup(x => x.GetDropDownType(SiteId, "Area")).ReturnsAsync(new DropDownType { Id = "type-area" });
            DropDownTypeService.Setup(x => x.GetDropDownType(SiteId, "Workspace")).ReturnsAsync(new DropDownType { Id = "type-ws" });
            DropDownService.Setup(x => x.GetDropDowns("type-area")).ReturnsAsync(new List<DropDown> { new() { Id = "area-1", DropDownValue = "Payments" } });
            DropDownService.Setup(x => x.GetDropDowns("type-ws")).ReturnsAsync(new List<DropDown> { new() { Id = "ws-1", DropDownValue = "Web" } });
            TestPlanService.Setup(x => x.GetTestPlanByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((TestPlan)null);
        }

        public TestPlanImportService CreateService() => new(
            TestPlanService.Object, ProjectService.Object, EmployeeService.Object,
            DropDownTypeService.Object, DropDownService.Object, CommonService.Object);

        public void VerifyNoWrites()
        {
            TestPlanService.Verify(x => x.InsertTestPlan(It.IsAny<TestPlan>()), Times.Never);
            TestPlanService.Verify(x => x.UpdateTestPlan(It.IsAny<TestPlan>()), Times.Never);
            TestPlanService.Verify(x => x.DeleteTestPlan(It.IsAny<TestPlan>()), Times.Never);
            TestPlanService.Verify(x => x.GetLastTestPlanNumber(), Times.Never);
        }

        public static TestPlanImportRowModel ValidRow(int rowNumber, string name = "Plan A", string projectId = "proj-1") => new()
        {
            RowNumber = rowNumber,
            ProjectId = projectId,
            Name = name,
            AreaId = "area-1",
            WorkspaceId = "ws-1",
            PlanMakerId = "emp-1",
            PlanReviewerId = "emp-2",
            Description = "Some description"
        };
    }

    public static class WorkbookBuilder
    {
        public static readonly string[] Headers = TestPlanImportField.All.Select(f => f.Header).ToArray();

        public static byte[] Build(string[] headers, params object[][] rows) => Build(TestPlanImportField.DataSheetName, headers, rows);

        public static byte[] Build(string sheetName, string[] headers, params object[][] rows)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add(sheetName);
            for (int c = 0; c < headers.Length; c++) sheet.Cells[1, c + 1].Value = headers[c];
            for (int r = 0; r < rows.Length; r++)
                for (int c = 0; c < rows[r].Length; c++)
                {
                    var v = rows[r][c];
                    if (v is Formula f) sheet.Cells[r + 2, c + 1].Formula = f.Text;
                    else sheet.Cells[r + 2, c + 1].Value = v;
                }
            return package.GetAsByteArray();
        }

        public static object[] ValidCells(string name = "Plan A") =>
            new object[] { "proj-1", name, "area-1", "ws-1", "emp-1", "emp-2", "Desc" };

        public static IFormFile ToFormFile(byte[] bytes, string fileName = "plans.xlsx")
        {
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "File", fileName);
        }
    }

    public record Formula(string Text);
}
