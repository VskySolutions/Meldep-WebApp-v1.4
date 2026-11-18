using System;
using System.Collections.Generic;

namespace Vsky.Api.Services.RequirementExport
{
    // Title: Requirement export contracts (WO-73)
    // Description: In-memory contracts for the single-requirement Excel export. Nothing here is persisted.
    // RequirementExportData is fully collected and scoped before RequirementWorkbookWriter sees it, so the
    // writer never performs lookups and never receives another requirement's records.

    public enum RequirementExportCellKind
    {
        Text,
        Number,
        Date,
        DateTime,
        Boolean
    }

    public sealed class RequirementExportColumn
    {
        public RequirementExportColumn(string header, RequirementExportCellKind kind)
        {
            Header = header;
            Kind = kind;
        }

        public string Header { get; }
        public RequirementExportCellKind Kind { get; }
    }

    /// <summary>One labelled value on the Requirement Info sheet.</summary>
    public sealed class RequirementExportField
    {
        public RequirementExportField(string label, object value, RequirementExportCellKind kind = RequirementExportCellKind.Text)
        {
            Label = label;
            Value = value;
            Kind = kind;
        }

        public string Label { get; }
        public object Value { get; }
        public RequirementExportCellKind Kind { get; }
    }

    /// <summary>One module's scoped, ordered, already-mapped rows.</summary>
    public sealed class RequirementExportSheet
    {
        public string ModuleKey { get; init; }
        public string Title { get; init; }
        public IReadOnlyList<RequirementExportColumn> Columns { get; init; } = Array.Empty<RequirementExportColumn>();
        public IReadOnlyList<object[]> Rows { get; init; } = Array.Empty<object[]>();
    }

    public sealed class RequirementExportData
    {
        public string RequirementId { get; init; }
        public int RequirementNumber { get; init; }
        public string RequirementTitle { get; init; }
        public IReadOnlyList<RequirementExportField> Info { get; init; } = Array.Empty<RequirementExportField>();

        /// <summary>Module sheets in registry order. Empty sheets may be present; the writer omits them.</summary>
        public IReadOnlyList<RequirementExportSheet> Sheets { get; init; } = Array.Empty<RequirementExportSheet>();
    }

    /// <summary>Server-derived selection. The requirement ID is the only client input.</summary>
    public sealed class RequirementExportScope
    {
        public string SiteId { get; init; }
        public string UserId { get; init; }
        public string RequirementId { get; init; }
        public string ProjectId { get; init; }
    }

    public sealed class RequirementExportFile
    {
        public byte[] Content { get; init; }
        public string FileName { get; init; }
        public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    }

    public enum RequirementExportFailure
    {
        NotFound,
        Forbidden
    }

    /// <summary>
    /// Expected refusals. Anything else (retrieval or generation errors) propagates as-is so the caller
    /// returns an error instead of a partial workbook.
    /// </summary>
    public sealed class RequirementExportException : Exception
    {
        public RequirementExportException(RequirementExportFailure failure, string message) : base(message)
        {
            Failure = failure;
        }

        public RequirementExportFailure Failure { get; }
    }
}
