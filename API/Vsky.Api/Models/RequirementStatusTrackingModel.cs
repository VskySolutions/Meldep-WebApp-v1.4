using System.Collections.Generic;
using System.Text.Json;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record RequirementStatusTrackingSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public List<string> ProjectIds { get; set; }
        public List<string> ProjectModuleIds { get; set; }
        public List<string> RequirementIds { get; set; }
        // Calendar dates in yyyy-MM-dd.
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }

    // Distinct complete-record identities for bulk actions.
    public record RequirementStatusTrackingRecordIdsModel
    {
        public List<string> RecordIds { get; set; } = new();
    }

    public record RequirementStatusTrackingBulkEditModel
    {
        public List<string> RecordIds { get; set; } = new();
        // Explicit field patch; only the listed fields would change.
        public Dictionary<string, JsonElement> Fields { get; set; } = new();
    }
}
