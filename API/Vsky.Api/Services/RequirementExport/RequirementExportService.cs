using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vsky.Api.Services.RequirementExport
{
    public interface IRequirementExportService
    {
        Task<RequirementExportFile> ExportAsync(string siteId, string userId, string requirementId, CancellationToken ct = default);
    }

    // Title: RequirementExportService (WO-73)
    // Description: Exports exactly one requirement. Order of work:
    //   1. resolve the requirement inside the active site (missing/deleted/other site => NotFound),
    //   2. apply the requirement's project record rule (denied => NotFound, like [RecordScope]),
    //   3. require View on every registered module before reading any module data (denied => Forbidden),
    //   4. fully collect every module, scoped to this requirement, then serialize.
    // Any retrieval or generation error propagates, so no partial workbook is ever returned.
    public sealed class RequirementExportService : IRequirementExportService
    {
        private readonly IRequirementExportDataSource _source;
        private readonly IRequirementExportAuthorizer _authorizer;
        private readonly IRequirementWorkbookWriter _writer;
        private readonly IReadOnlyList<IRequirementExportSheetDefinition> _definitions;

        public RequirementExportService(
            IRequirementExportDataSource source,
            IRequirementExportAuthorizer authorizer,
            IRequirementWorkbookWriter writer,
            IReadOnlyList<IRequirementExportSheetDefinition> definitions = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _authorizer = authorizer ?? throw new ArgumentNullException(nameof(authorizer));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _definitions = definitions ?? RequirementExportSheetDefinitions.All;
        }

        public async Task<RequirementExportFile> ExportAsync(string siteId, string userId, string requirementId, CancellationToken ct = default)
        {
            var data = await CollectAsync(siteId, userId, requirementId, ct);
            return new RequirementExportFile
            {
                Content = _writer.Write(data),
                FileName = BuildFileName(data.RequirementNumber, data.RequirementId)
            };
        }

        public async Task<RequirementExportData> CollectAsync(string siteId, string userId, string requirementId, CancellationToken ct = default)
        {
            requirementId = requirementId?.Trim();
            if (string.IsNullOrWhiteSpace(requirementId) || string.IsNullOrWhiteSpace(siteId))
                throw NotFound();

            var info = await _source.GetRequirementAsync(siteId, requirementId, ct);
            if (info == null || info.Id != requirementId || info.SiteId != siteId)
                throw NotFound();

            if (!await _authorizer.CanAccessRequirementAsync(userId, siteId, requirementId))
                throw NotFound();

            var denied = new List<string>();
            foreach (var definition in _definitions)
            {
                if (!await _authorizer.CanViewModuleAsync(userId, siteId, definition.MenuLinks))
                    denied.Add(definition.SheetTitle);
            }

            if (denied.Count > 0)
                throw new RequirementExportException(RequirementExportFailure.Forbidden,
                    "You don't have permission to export this requirement's " + string.Join(", ", denied) + " data.");

            var scope = new RequirementExportScope
            {
                SiteId = siteId,
                UserId = userId,
                RequirementId = info.Id,
                ProjectId = info.ProjectId
            };

            // Sequential on purpose: the EF data source shares one request-scoped DbContext.
            var sheets = new List<RequirementExportSheet>(_definitions.Count);
            foreach (var definition in _definitions)
            {
                ct.ThrowIfCancellationRequested();
                sheets.Add(await definition.CollectAsync(_source, scope, ct));
            }

            return new RequirementExportData
            {
                RequirementId = info.Id,
                RequirementNumber = info.RequirementNumber,
                RequirementTitle = info.Title,
                Info = RequirementExportSheetDefinitions.BuildInfo(info),
                Sheets = sheets
            };
        }

        /// <summary>"Requirement-{number}.xlsx"; falls back to the ID (sanitized, untruncated) when there is no number.</summary>
        public static string BuildFileName(int requirementNumber, string requirementId)
        {
            var identity = requirementNumber > 0 ? requirementNumber.ToString(System.Globalization.CultureInfo.InvariantCulture) : requirementId ?? "export";
            var sb = new StringBuilder();
            foreach (var ch in identity)
                sb.Append(char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' ? ch : '_');
            return "Requirement-" + sb + ".xlsx";
        }

        private static RequirementExportException NotFound() =>
            new(RequirementExportFailure.NotFound, "No Requirement found with the specified id.");
    }
}
