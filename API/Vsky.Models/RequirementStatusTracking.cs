using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models
{
    // One stored row per tracked requirement. Rows sharing a RecordId form one tracking record,
    // and the project/module/date context is repeated on every row of that record (single-table layout).
    public class RequirementStatusTracking : BaseEntity
    {
        public string ProjectId { get; set; }
        public string ProjectModuleId { get; set; }
        public string RequirementId { get; set; }
        public DateTime TrackingDate { get; set; }

        public decimal? CurrentPercent { get; set; }
        public decimal? TimeSpentAsOfToday { get; set; }
        public decimal? EstimatedRemainingTime { get; set; }
        public string Scope { get; set; }
        public decimal? EstimatedTimeForCompletion { get; set; }

        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Project Project { get; set; }
        public virtual ProjectModule ProjectModule { get; set; }
        public virtual Requirement Requirement { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
    }

    #region Save Models
    // Whole-batch save payload for /bulk-add (no RecordId) and /bulk-save (existing RecordId).
    public class RequirementStatusTrackingSaveModel
    {
        public string RecordId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectModuleId { get; set; }
        // Calendar date in yyyy-MM-dd; kept as text so no timezone conversion can shift it.
        public string TrackingDate { get; set; }
        public List<RequirementStatusTrackingRowSaveModel> Rows { get; set; } = new();
        // Persisted row ids staged for removal in the form.
        public List<string> RemovedRowIds { get; set; } = new();
    }

    public class RequirementStatusTrackingRowSaveModel
    {
        public string Id { get; set; }
        public string RequirementId { get; set; }
        public decimal? CurrentPercent { get; set; }
        public decimal? TimeSpentAsOfToday { get; set; }
        public decimal? EstimatedRemainingTime { get; set; }
        public string Scope { get; set; }
        public decimal? EstimatedTimeForCompletion { get; set; }
    }
    #endregion

    #region Read Models
    public class RequirementStatusTrackingRecord
    {
        public string RecordId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectModuleId { get; set; }
        public string ProjectModuleName { get; set; }
        public string TrackingDate { get; set; }
        public string CreatedByName { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public List<RequirementStatusTrackingRow> Rows { get; set; } = new();

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
    }

    public class RequirementStatusTrackingRow
    {
        public string Id { get; set; }
        public string RecordId { get; set; }
        public string RequirementId { get; set; }
        public int RequirementNumber { get; set; }
        public string RequirementTitle { get; set; }
        // True when the requirement was deleted or moved out of the record's module after the row was saved.
        public bool IsRequirementUnavailable { get; set; }
        public decimal? CurrentPercent { get; set; }
        public decimal? TimeSpentAsOfToday { get; set; }
        public decimal? EstimatedRemainingTime { get; set; }
        public string Scope { get; set; }
        public decimal? EstimatedTimeForCompletion { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }

    public class RequirementStatusTrackingRecordList
    {
        public List<RequirementStatusTrackingRecord> Records { get; set; } = new();
        // Count of complete matching records, not requirement rows.
        public int Total { get; set; }
    }

    public class RequirementStatusTrackingFilter
    {
        public string SearchText { get; set; }
        public List<string> ProjectIds { get; set; }
        public List<string> ProjectModuleIds { get; set; }
        public List<string> RequirementIds { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string SortBy { get; set; }
        public bool Descending { get; set; }
        public Dictionary<string, string> Sorts { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = int.MaxValue;
    }

    public class RequirementStatusTrackingResult
    {
        public bool NotFound { get; set; }
        public List<string> Errors { get; set; } = new();
        public RequirementStatusTrackingRecord Record { get; set; }
        public RequirementStatusTrackingRecordList List { get; set; }
        public bool Succeeded => !NotFound && Errors.Count == 0;
    }
    #endregion
}
