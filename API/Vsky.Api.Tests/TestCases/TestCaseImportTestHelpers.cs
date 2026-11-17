using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Moq;
using OfficeOpenXml;
using Vsky.Api.Models;
using Vsky.Api.Services.TestCases;
using Vsky.Models;
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

namespace Vsky.Api.Tests.TestCases
{
    // Mocked read services. Two permitted projects, each with a plan "Smoke Plan" and module "Login"
    // (same labels, different IDs) to exercise project-scoped resolution. "proj-x" is out of scope.
    public class CaseFixture
    {
        public const string SiteId = "site-1";
        public const string UserId = "user-1";
        public const string EmployeeId = "emp-self";
        public static readonly DateTime SiteNow = new(2026, 11, 17, 9, 30, 0);

        public Mock<ITestCaseService> TestCaseService { get; } = new();
        public Mock<ITestPlanService> TestPlanService { get; } = new();
        public Mock<IProjectService> ProjectService { get; } = new();
        public Mock<IProjectModuleService> ModuleService { get; } = new();
        public Mock<IRequirementService> RequirementService { get; } = new();
        public Mock<IEmployeeService> EmployeeService { get; } = new();
        public Mock<IDropDownTypeService> DropDownTypeService { get; } = new();
        public Mock<IDropDownService> DropDownService { get; } = new();
        public Mock<IIssueService> IssueService { get; } = new();
        public Mock<ISiteService> SiteService { get; } = new();
        public Mock<ICommonService> CommonService { get; } = new();

        public CaseFixture()
        {
            CommonService.Setup(x => x.GetEmployeeIdByUserIdAndEmail(SiteId, UserId)).Returns(EmployeeId);
            CommonService.Setup(x => x.GetDrownValueIdByTypeandValue(SiteId, "Test Case Status", "Fail")).Returns("st-fail");
            CommonService.Setup(x => x.GetDrownValueIdByTypeandValue(SiteId, "Issue Status", "New from Test Plan")).Returns("is-new");
            CommonService.Setup(x => x.GetDrownValueIdByTypeandValue(SiteId, "Issue Priority", "Medium")).Returns("ip-medium");
            CommonService.Setup(x => x.GetDrownValueIdByTypeandValue(SiteId, "Issue Type", "Bug")).Returns("it-bug");

            ProjectService.Setup(x => x.GetProjectsListForDropdown(SiteId, UserId, EmployeeId, false, "false", false))
                .ReturnsAsync(() => new List<CommonDropDown> { new() { Value = "proj-1", Text = "Alpha" }, new() { Value = "proj-2", Text = "Beta" } });

            TestPlanService.Setup(x => x.GetAllTestPlansListForDropdown(SiteId, "proj-1")).ReturnsAsync(() => new List<CommonDropDown> { new() { Value = "plan-1", Text = "Smoke Plan" } });
            TestPlanService.Setup(x => x.GetAllTestPlansListForDropdown(SiteId, "proj-2")).ReturnsAsync(() => new List<CommonDropDown> { new() { Value = "plan-2", Text = "Smoke Plan" } });

            ModuleService.Setup(x => x.GetAllProjectModuleListForDropdown(SiteId, false, false, "proj-1")).ReturnsAsync(() => new List<CommonDropDown> { new() { Value = "mod-1", Text = "Login" } });
            ModuleService.Setup(x => x.GetAllProjectModuleListForDropdown(SiteId, false, false, "proj-2")).ReturnsAsync(() => new List<CommonDropDown> { new() { Value = "mod-2", Text = "Login" }, new() { Value = "mod-3", Text = "Billing" } });

            RequirementService.Setup(x => x.GetAllRequirementListForDropdown(SiteId, "mod-1", "proj-1", null)).ReturnsAsync(() => new List<CommonDropDown> { new() { Value = "req-1", Text = "User can log in" } });
            RequirementService.Setup(x => x.GetAllRequirementListForDropdown(SiteId, "mod-2", "proj-2", null)).ReturnsAsync(() => new List<CommonDropDown> { new() { Value = "req-2", Text = "User can log in" } });
            RequirementService.Setup(x => x.GetAllRequirementListForDropdown(SiteId, "mod-3", "proj-2", null)).ReturnsAsync(() => new List<CommonDropDown>());

            EmployeeService.Setup(x => x.GetAllActiveEmployeeListForDropdown(SiteId, null)).ReturnsAsync(() => new List<Employee>
            {
                new() { Id = "emp-1", Person = new Person { FullName = "Asha Rao" } },
                new() { Id = "emp-2", Person = new Person { FullName = "Ravi Kumar" } }
            });

            SetupDropDown("Area", "type-area", ("area-1", "Payments"));
            SetupDropDown("Workspace", "type-ws", ("ws-1", "Web"));
            SetupDropDown("Test Case Status", "type-status", ("st-new", "New"), ("st-pass", "Pass"), ("st-fail", "Fail"));

            SiteService.Setup(x => x.GetById(SiteId)).ReturnsAsync(new Site { Id = SiteId, Name = "Acme", TimeZone = "UTC" });
            SiteService.Setup(x => x.GetDateTime(It.IsAny<string>())).Returns(SiteNow);

            TestCaseService.Setup(x => x.GetTestCaseByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((TestCase)null);
            IssueService.Setup(x => x.GetIssueByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((Issue)null);
        }

        private void SetupDropDown(string type, string typeId, params (string Id, string Value)[] values)
        {
            DropDownTypeService.Setup(x => x.GetDropDownType(SiteId, type)).ReturnsAsync(new DropDownType { Id = typeId });
            DropDownService.Setup(x => x.GetDropDowns(typeId)).ReturnsAsync(values.Select(v => new DropDown { Id = v.Id, DropDownValue = v.Value }).ToList());
        }

        public TestCaseImportService CreateService() => new(
            TestCaseService.Object, TestPlanService.Object, ProjectService.Object, ModuleService.Object, RequirementService.Object,
            EmployeeService.Object, DropDownTypeService.Object, DropDownService.Object, IssueService.Object, SiteService.Object, CommonService.Object);

        public void VerifyNoWrites()
        {
            TestCaseService.Verify(x => x.InsertTestCase(It.IsAny<TestCase>()), Times.Never);
            TestCaseService.Verify(x => x.UpdateTestCase(It.IsAny<TestCase>()), Times.Never);
            TestCaseService.Verify(x => x.GetLastTestCaseNumber(), Times.Never);
            IssueService.Verify(x => x.InsertIssue(It.IsAny<Issue>()), Times.Never);
            IssueService.Verify(x => x.GetLastIssueNumber(), Times.Never);
        }

        public static TestCaseImportRowModel ValidRow(int rowNumber, string name = "Case A", string status = null) => new()
        {
            RowNumber = rowNumber,
            ProjectId = "Alpha",
            PlanId = "Smoke Plan",
            ProjectModuleId = "Login",
            RequirementId = "User can log in",
            Name = name,
            AreaId = "Payments",
            WorkspaceId = "Web",
            StatusId = status,
            EmployeeId = status == "Fail" ? "Asha Rao" : null, // Assigned To applies only to failed cases
            TestedBy = "Ravi Kumar",
            TestedDate = "11/15/2026",
            Description = "Desc",
            Steps = "1. Open\n2. Login",
            ExpectedResult = "Logged in",
            ActualResult = "Logged in"
        };
    }

    public static class CaseWorkbook
    {
        public static readonly string[] Headers = TestCaseImportField.All.Select(f => f.Header).ToArray();

        public static object[] ValidCells(string name = "Case A") => new object[]
        {
            "Alpha", "Smoke Plan", "Login", "User can log in", name, "Payments", "Web", "Pass", null, "Ravi Kumar", "11/15/2026",
            "Desc", "Steps", "Expected", "Actual"
        };

        public static byte[] Build(string[] headers, params object[][] rows)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add(TestCaseImportField.DataSheetName);
            for (int c = 0; c < headers.Length; c++) sheet.Cells[1, c + 1].Value = headers[c];
            for (int r = 0; r < rows.Length; r++)
                for (int c = 0; c < rows[r].Length; c++)
                    sheet.Cells[r + 2, c + 1].Value = rows[r][c];
            return package.GetAsByteArray();
        }

        public static IFormFile ToFormFile(byte[] bytes, string fileName = "cases.xlsx") =>
            new FormFile(new MemoryStream(bytes), 0, bytes.Length, "File", fileName);
    }
}
