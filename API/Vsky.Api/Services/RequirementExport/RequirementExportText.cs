using System;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Vsky.Api.Services.RequirementExport
{
    // Title: RequirementExportText
    // Description: The reviewed rich-text transformation for export. Editor HTML becomes plain text: block
    // elements and <br> become line breaks, list items get a "- " prefix, entities are decoded, and every
    // other tag (including img/script/style/iframe) is dropped, so no remote or active content is embedded.
    public static class RequirementExportText
    {
        /// <summary>Excel's per-cell character limit.</summary>
        public const int MaxCellLength = 32767;

        public const string TruncationMarker = " …[truncated: exceeds Excel cell limit]";

        private static readonly string[] DroppedElements = { "script", "style", "iframe", "object", "embed", "noscript", "head", "img", "svg" };
        private static readonly Regex ManyBlankLines = new(@"\n{3,}", RegexOptions.Compiled);
        private static readonly Regex TrailingSpaces = new(@"[ \t]+\n", RegexOptions.Compiled);

        public static string FromRichText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return null;

            // Plain text without markup: only decode entities.
            if (html.IndexOf('<') < 0)
                return Clean(HtmlEntity.DeEntitize(html));

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (var name in DroppedElements)
            {
                var nodes = doc.DocumentNode.SelectNodes("//" + name);
                if (nodes == null) continue;
                foreach (var node in nodes) node.Remove();
            }

            var sb = new StringBuilder();
            Walk(doc.DocumentNode, sb);
            return Clean(sb.ToString());
        }

        private static void Walk(HtmlNode node, StringBuilder sb)
        {
            foreach (var child in node.ChildNodes)
            {
                switch (child.NodeType)
                {
                    case HtmlNodeType.Text:
                        sb.Append(HtmlEntity.DeEntitize(((HtmlTextNode)child).Text));
                        break;
                    case HtmlNodeType.Element:
                        var name = child.Name.ToLowerInvariant();
                        if (name == "br") { sb.Append('\n'); break; }
                        if (name == "li") sb.Append("\n- ");
                        Walk(child, sb);
                        if (IsBlock(name)) sb.Append('\n');
                        break;
                }
            }
        }

        private static bool IsBlock(string name) => name switch
        {
            "p" or "div" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "ul" or "ol" or "tr" or "table"
                or "blockquote" or "pre" or "section" or "article" or "header" or "footer" => true,
            _ => false
        };

        private static string Clean(string text)
        {
            if (text == null) return null;
            text = text.Replace("\r\n", "\n").Replace('\r', '\n').Replace(' ', ' ');
            text = TrailingSpaces.Replace(text, "\n");
            text = ManyBlankLines.Replace(text, "\n\n").Trim();
            return text.Length == 0 ? null : text;
        }

        /// <summary>
        /// Makes a value safe to store as a literal text cell: removes characters XML 1.0 forbids and
        /// shortens over-limit text with a visible marker (never silently).
        /// </summary>
        public static string ToCellText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            StringBuilder sb = null;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                var valid = c == '\t' || c == '\n' || c == '\r' || (c >= 0x20 && c <= 0xD7FF) || (c >= 0xE000 && c <= 0xFFFD);
                if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
                {
                    sb?.Append(c).Append(value[i + 1]);
                    i++;
                    continue;
                }

                if (!valid)
                {
                    sb ??= new StringBuilder(value, 0, i, value.Length);
                    continue;
                }

                sb?.Append(c);
            }

            var result = sb?.ToString() ?? value;
            if (result.Length > MaxCellLength)
            {
                var keep = MaxCellLength - TruncationMarker.Length;
                if (char.IsHighSurrogate(result[keep - 1])) keep--;
                result = result.Substring(0, keep) + TruncationMarker;
            }

            return result;
        }
    }
}
