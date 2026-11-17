using System;
using System.Collections.Generic;
using System.Linq;

namespace Vsky.Api.Services.Import
{
    public enum ExcelImportLookupStatus { Found, NotFound, Ambiguous }

    public record ExcelImportLookupMatch(ExcelImportLookupStatus Status, string Id = null, string Label = null);

    // Title: ExcelImportLookupSet
    // Description: One lookup's permitted records (ID -> readable label). Produces the Excel dropdown options
    // (the readable label, made unique with the record ID only when two records share a label) and resolves
    // a submitted cell value back to a record: by ID, by dropdown option, or by a plain label that matches
    // exactly one record. A plain label shared by several records is reported as ambiguous, never guessed.
    public class ExcelImportLookupSet
    {
        private readonly Dictionary<string, string> _labelsById = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _canonicalIds = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<string>> _idsByLabel = new(StringComparer.OrdinalIgnoreCase);

        public void Add(string id, string label)
        {
            if (string.IsNullOrWhiteSpace(id) || _labelsById.ContainsKey(id)) return;
            label = string.IsNullOrWhiteSpace(label) ? id : label.Trim();
            _labelsById[id] = label;
            _canonicalIds[id] = id;
            if (!_idsByLabel.TryGetValue(label, out var ids)) _idsByLabel[label] = ids = new List<string>();
            ids.Add(id);
        }

        public int Count => _labelsById.Count;

        public string DisplayLabel(string id)
        {
            var label = _labelsById[id];
            return _idsByLabel[label].Count > 1 ? $"{label} ({_canonicalIds[id]})" : label;
        }

        // Dropdown options in label order: (option text shown in Excel, record ID).
        public IReadOnlyList<KeyValuePair<string, string>> Options() =>
            _canonicalIds.Values
                .Select(id => new KeyValuePair<string, string>(DisplayLabel(id), id))
                .OrderBy(o => o.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

        public ExcelImportLookupMatch Resolve(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return new(ExcelImportLookupStatus.NotFound);
            value = value.Trim();

            if (_canonicalIds.TryGetValue(value, out var byId))
                return new(ExcelImportLookupStatus.Found, byId, _labelsById[byId]);

            foreach (var id in _canonicalIds.Values)
                if (string.Equals(DisplayLabel(id), value, StringComparison.OrdinalIgnoreCase))
                    return new(ExcelImportLookupStatus.Found, id, _labelsById[id]);

            if (_idsByLabel.TryGetValue(value, out var ids))
                return ids.Count == 1
                    ? new(ExcelImportLookupStatus.Found, _canonicalIds[ids[0]], _labelsById[ids[0]])
                    : new(ExcelImportLookupStatus.Ambiguous);

            return new(ExcelImportLookupStatus.NotFound);
        }
    }

}
