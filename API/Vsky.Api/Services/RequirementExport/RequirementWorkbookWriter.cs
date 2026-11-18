using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Vsky.Api.Services.RequirementExport
{
    public interface IRequirementWorkbookWriter
    {
        byte[] Write(RequirementExportData data);
    }

    // Title: RequirementWorkbookWriter
    // Description: Pure serializer (no lookups) from RequirementExportData to one .xlsx. Requirement Info is
    // always the first sheet; module sheets with zero rows are omitted. Strings are written as literal cell
    // values (never formulas), numbers/dates/booleans keep their types, and null stays blank.
    public sealed class RequirementWorkbookWriter : IRequirementWorkbookWriter
    {
        public const string InfoSheetName = "Requirement Info";
        public const int MaxSheetNameLength = 31;

        private const string DateFormat = "yyyy-mm-dd";
        private const string DateTimeFormat = "yyyy-mm-dd hh:mm";
        private const double MaxColumnWidth = 60;
        private static readonly char[] ForbiddenSheetChars = { '[', ']', ':', '*', '?', '/', '\\' };

        public byte[] Write(RequirementExportData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            WriteInfo(package.Workbook.Worksheets.Add(InfoSheetName), data.Info);

            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { InfoSheetName };
            foreach (var sheet in data.Sheets ?? Array.Empty<RequirementExportSheet>())
            {
                if (sheet?.Rows == null || sheet.Rows.Count == 0)
                    continue;

                var name = UniqueSheetName(sheet.Title, usedNames);
                WriteTable(package.Workbook.Worksheets.Add(name), sheet);
            }

            return package.GetAsByteArray();
        }

        private static void WriteInfo(ExcelWorksheet ws, IReadOnlyList<RequirementExportField> info)
        {
            WriteHeader(ws, new[] { "Field", "Value" });
            var row = 2;
            foreach (var field in info ?? Array.Empty<RequirementExportField>())
            {
                SetCell(ws.Cells[row, 1], field.Label, RequirementExportCellKind.Text);
                SetCell(ws.Cells[row, 2], field.Value, field.Kind);
                row++;
            }

            ws.Column(1).Width = 28;
            ws.Column(2).Width = 80;
            ws.Column(2).Style.WrapText = true;
            ws.Cells[1, 1, Math.Max(row - 1, 1), 2].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            ws.View.FreezePanes(2, 1);
        }

        private static void WriteTable(ExcelWorksheet ws, RequirementExportSheet sheet)
        {
            var columns = sheet.Columns;
            WriteHeader(ws, columns.Select(c => c.Header).ToArray());

            var widths = columns.Select(c => (double)Math.Max(10, (c.Header ?? string.Empty).Length + 2)).ToArray();
            var row = 2;
            foreach (var values in sheet.Rows)
            {
                for (var c = 0; c < columns.Count; c++)
                {
                    var value = values != null && c < values.Length ? values[c] : null;
                    SetCell(ws.Cells[row, c + 1], value, columns[c].Kind);
                    widths[c] = Math.Min(MaxColumnWidth, Math.Max(widths[c], EstimateWidth(value, columns[c].Kind)));
                }
                row++;
            }

            for (var c = 0; c < columns.Count; c++)
            {
                ws.Column(c + 1).Width = widths[c];
                if (columns[c].Kind == RequirementExportCellKind.Text)
                    ws.Column(c + 1).Style.WrapText = true;
            }

            ws.Cells[1, 1, row - 1, columns.Count].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            ws.Cells[1, 1, row - 1, columns.Count].AutoFilter = true;
            ws.View.FreezePanes(2, 1);
        }

        private static void WriteHeader(ExcelWorksheet ws, IReadOnlyList<string> headers)
        {
            for (var i = 0; i < headers.Count; i++)
                SetCell(ws.Cells[1, i + 1], headers[i], RequirementExportCellKind.Text);

            using var range = ws.Cells[1, 1, 1, Math.Max(headers.Count, 1)];
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0xE8, 0xEC, 0xF2));
        }

        /// <summary>Writes Value only — never Formula — so text such as "=HYPERLINK(...)" stays inert text.</summary>
        internal static void SetCell(ExcelRange cell, object value, RequirementExportCellKind kind)
        {
            switch (value)
            {
                case null:
                    return;
                case string s:
                    var text = RequirementExportText.ToCellText(s);
                    if (string.IsNullOrEmpty(text)) return;
                    cell.Value = text;
                    return;
                case DateTime dt:
                    cell.Value = dt;
                    cell.Style.Numberformat.Format = kind == RequirementExportCellKind.Date ? DateFormat : DateTimeFormat;
                    return;
                case DateTimeOffset dto:
                    cell.Value = dto.UtcDateTime;
                    cell.Style.Numberformat.Format = DateTimeFormat;
                    return;
                case bool b:
                    cell.Value = b;
                    return;
                case int or long or short or decimal or double or float:
                    cell.Value = value;
                    return;
                default:
                    cell.Value = RequirementExportText.ToCellText(Convert.ToString(value, CultureInfo.InvariantCulture));
                    return;
            }
        }

        private static double EstimateWidth(object value, RequirementExportCellKind kind)
        {
            return kind switch
            {
                RequirementExportCellKind.Date => 12,
                RequirementExportCellKind.DateTime => 18,
                RequirementExportCellKind.Boolean => 8,
                RequirementExportCellKind.Number => Math.Max(8, Convert.ToString(value, CultureInfo.InvariantCulture)?.Length + 2 ?? 8),
                _ => (value as string)?.Split('\n').Max(l => l.Length) + 2 ?? 10
            };
        }

        /// <summary>
        /// Excel-safe, unique (case-insensitive) worksheet name: forbidden characters become "-", leading and
        /// trailing apostrophes are trimmed, the reserved "History" and "Requirement Info" names are avoided,
        /// length is capped at 31, and collisions get " (2)", " (3)"... in registry order.
        /// </summary>
        public static string UniqueSheetName(string title, ISet<string> usedNames)
        {
            var sb = new StringBuilder();
            foreach (var ch in title ?? string.Empty)
                sb.Append(Array.IndexOf(ForbiddenSheetChars, ch) >= 0 || char.IsControl(ch) ? '-' : ch);

            var baseName = sb.ToString().Trim().Trim('\'').Trim();
            if (baseName.Length == 0) baseName = "Sheet";
            if (string.Equals(baseName, "History", StringComparison.OrdinalIgnoreCase)) baseName = "History Data";
            if (baseName.Length > MaxSheetNameLength) baseName = baseName.Substring(0, MaxSheetNameLength).TrimEnd();

            var candidate = baseName;
            for (var n = 2; usedNames.Contains(candidate); n++)
            {
                var suffix = $" ({n})";
                var stem = baseName.Length + suffix.Length > MaxSheetNameLength
                    ? baseName.Substring(0, MaxSheetNameLength - suffix.Length).TrimEnd()
                    : baseName;
                candidate = stem + suffix;
            }

            usedNames.Add(candidate);
            return candidate;
        }
    }
}
