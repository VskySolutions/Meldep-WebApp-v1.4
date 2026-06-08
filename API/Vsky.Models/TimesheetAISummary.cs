using System;
using Vsky.Core;

namespace Vsky.Models;

public class TimesheetAISummary : BaseEntity
{
    public string TaskId { get; set; }
    public string TimesheetLineId { get; set; }
    public string SummaryId { get; set; }
    public string Summary { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    public virtual ProjectTask Task { get; set; }
    public virtual TimesheetLines TimesheetLine { get; set; }
}