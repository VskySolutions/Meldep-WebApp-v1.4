using Vsky.Models;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record TimesheetAISummaryModel : BaseEntityModel
    {
        public string TaskId { get; set; }
        public string TimesheetLineId { get; set; }
        public string SummaryId { get; set; }
        public string Summary { get; set; }

        public virtual ProjectTask Task { get; set; }
        public virtual TimesheetLines TimesheetLine { get; set; }
    }

    public record TimesheetAISummarySearchModel : BaseSearchModel
    {
    }

    public record TimesheetAISummaryListModel : BasePagedListModel<TimesheetAISummaryModel>
    {
        public bool editing { get; set; }
    }
}
