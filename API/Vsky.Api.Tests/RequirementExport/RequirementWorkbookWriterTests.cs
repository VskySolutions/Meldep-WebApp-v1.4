using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Vsky.Api.Controllers;
using Vsky.Api.Services.RequirementExport;
using Vsky.Models;
using Xunit;
using static Vsky.Api.Tests.RequirementExport.ExportFixture;

namespace Vsky.Api.Tests.RequirementExport
{
    public class RequirementWorkbookWriterTests
    {
        private static RequirementExportSheet Sheet(string title, params object[][] rows) => new()
        {
            ModuleKey = title,
            Title = title,
            Columns = new[]
            {
                new RequirementExportColumn("Text", RequirementExportCellKind.Text),
                new RequirementExportColumn("Number", RequirementExportCellKind.Number),
                new RequirementExportColumn("Date", RequirementExportCellKind.Date),
                new RequirementExportColumn("Flag", RequirementExportCellKind.Boolean)
            },
            Rows = rows
        };

        private static OfficeOpenXml.ExcelPackage Write(params RequirementExportSheet[] sheets) =>
            Open(new RequirementWorkbookWriter().Write(new RequirementExportData
            {
                RequirementId = "r",
                Info = new[] { new RequirementExportField("Title", "=cmd|' /C calc'!A0") },
                Sheets = sheets
            }));

        [Fact]
        public void Formula_like_text_is_stored_as_literal_text()
        {
            using var package = Write(Sheet("Tasks", new object[] { "=HYPERLINK(\"http://evil\",\"x\")", 1, null, null }));

            var cell = package.Workbook.Worksheets["Tasks"].Cells[2, 1];
            Assert.True(string.IsNullOrEmpty(cell.Formula));
            Assert.Equal("=HYPERLINK(\"http://evil\",\"x\")", cell.Value);
            Assert.True(string.IsNullOrEmpty(package.Workbook.Worksheets[0].Cells[2, 2].Formula));
            Assert.Equal("=cmd|' /C calc'!A0", package.Workbook.Worksheets[0].Cells[2, 2].Value);
        }

        [Fact]
        public void Typed_values_keep_their_types_and_nulls_stay_blank()
        {
            var date = new DateTime(2026, 11, 18);
            using var package = Write(Sheet("Tasks", new object[] { null, 4.5m, date, true }));
            var ws = package.Workbook.Worksheets["Tasks"];

            Assert.Null(ws.Cells[2, 1].Value);
            Assert.Equal(4.5d, Convert.ToDouble(ws.Cells[2, 2].Value));
            Assert.IsNotType<string>(ws.Cells[2, 2].Value);
            Assert.IsType<double>(ws.Cells[2, 3].Value); // xlsx stores dates as serial numbers...
            Assert.Equal(date, ws.Cells[2, 3].GetValue<DateTime>()); // ...that read back as the same date
            Assert.Equal("yyyy-mm-dd", ws.Cells[2, 3].Style.Numberformat.Format);
            Assert.Equal(true, ws.Cells[2, 4].Value);
        }

        [Fact]
        public void Long_identifiers_are_not_truncated_and_control_characters_are_removed()
        {
            var id = new string('7', 40);
            using var package = Write(Sheet("Tasks", new object[] { id + "\u0001\u0008ok", 1, null, null }));

            Assert.Equal(id + "ok", package.Workbook.Worksheets["Tasks"].Cells[2, 1].Value);
        }

        [Fact]
        public void Over_limit_text_is_shortened_visibly()
        {
            var text = RequirementExportText.ToCellText(new string('a', 40000));

            Assert.Equal(RequirementExportText.MaxCellLength, text.Length);
            Assert.EndsWith(RequirementExportText.TruncationMarker, text);
        }

        [Fact]
        public void Empty_sheets_are_omitted_and_info_is_always_first()
        {
            using var package = Write(Sheet("Tasks"), Sheet("Issues", new object[] { "x", 1, null, null }));

            Assert.Equal(new[] { "Requirement Info", "Issues" }, package.Workbook.Worksheets.Select(w => w.Name));
        }

        [Fact]
        public void Colliding_and_unsafe_sheet_titles_get_unique_valid_names()
        {
            using var package = Write(
                Sheet("Requirement Info", new object[] { "a", 1, null, null }),
                Sheet("Q&A", new object[] { "b", 1, null, null }),
                Sheet("q&a", new object[] { "c", 1, null, null }),
                Sheet("A/B:C*D?[E]\\F", new object[] { "d", 1, null, null }),
                Sheet(new string('N', 40), new object[] { "e", 1, null, null }),
                Sheet(new string('N', 40), new object[] { "f", 1, null, null }));

            var names = package.Workbook.Worksheets.Select(w => w.Name).ToList();
            Assert.Equal("Requirement Info", names[0]);
            Assert.Equal("Requirement Info (2)", names[1]);
            Assert.Equal("Q&A", names[2]);
            Assert.Equal("q&a (2)", names[3]);
            Assert.Equal("A-B-C-D--E--F", names[4]);
            Assert.Equal(new string('N', 31), names[5]);
            Assert.Equal(new string('N', 27) + " (2)", names[6]);
            Assert.All(names, n => Assert.True(n.Length <= 31));
        }

        [Fact]
        public void Rich_text_is_normalized_and_active_content_dropped()
        {
            var text = RequirementExportText.FromRichText("<h2>Title</h2><p>a&amp;b<br>c</p><img src=\"http://x/y.png\"><iframe src=\"http://x\"></iframe><style>p{}</style>");

            Assert.Equal("Title\na&b\nc", text);
            Assert.Null(RequirementExportText.FromRichText("<p> </p>"));
        }
    }

    public class RequirementExportControllerTests
    {
        private static RequirementExportController Controller(IRequirementExportService service)
        {
            var http = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, UserId) }, "test"))
            };
            return new RequirementExportController(new GlobalVariable { SiteId = SiteId }, service)
            {
                ControllerContext = new ControllerContext { HttpContext = http }
            };
        }

        [Fact]
        public async Task Success_returns_xlsx_attachment_with_no_store_caching()
        {
            var fx = new ExportFixture();
            var controller = Controller(fx.Service());

            var result = Assert.IsType<FileContentResult>(await controller.ExportRequirement(ReqA, CancellationToken.None));

            Assert.Equal(RequirementExportFile.ContentType, result.ContentType);
            Assert.Equal("Requirement-42.xlsx", result.FileDownloadName);
            Assert.Equal("private, no-store", controller.Response.Headers["Cache-Control"].ToString());
            using var package = Open(result.FileContents);
            Assert.Equal("Requirement Info", package.Workbook.Worksheets[0].Name);
        }

        [Fact]
        public async Task Not_found_and_forbidden_map_to_404_and_403_without_a_workbook()
        {
            var notFound = await Controller(new ExportFixture().Service()).ExportRequirement(ReqB + "-nope", CancellationToken.None);
            Assert.Equal(404, Assert.IsType<NotFoundObjectResult>(notFound).StatusCode);

            var fx = new ExportFixture();
            fx.Authorizer.DeniedLinks.Add(Authorization.MenuLinks.TestCase);
            var forbidden = await Controller(fx.Service()).ExportRequirement(ReqA, CancellationToken.None);
            Assert.Equal(403, Assert.IsType<ObjectResult>(forbidden).StatusCode);
        }

        [Fact]
        public async Task Generation_error_is_a_server_error_not_a_file()
        {
            var service = new Mock<IRequirementExportService>();
            service.Setup(x => x.ExportAsync(SiteId, UserId, ReqA, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("boom"));
            var controller = Controller(service.Object);
            controller.ControllerContext.HttpContext.RequestServices = Mock.Of<IServiceProvider>();

            var result = Assert.IsType<ObjectResult>(await controller.ExportRequirement(ReqA, CancellationToken.None));
            Assert.Equal(500, result.StatusCode);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("false", false)]
        [InlineData("true", true)]
        public void Feature_flag_defaults_to_off(string value, bool expected)
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(value == null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string> { [RequirementExportController.FeatureFlagKey] = value }).Build();

            Assert.Equal(expected, RequirementExportFeatureGateAttribute.IsEnabled(config));
        }
    }
}
