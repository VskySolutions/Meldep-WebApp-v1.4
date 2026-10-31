// Pure helpers for the Requirement Status Tracking list (no Vue/Quasar imports so they can be tested in isolation).
import { toApiDate, fromApiDate } from "./trackingForm.js";

// Record-level sort keys accepted by the API allowlist; requirement-row columns are not sortable.
export const SORTABLE_COLUMNS = ["project.name", "projectModule.name", "trackingDate", "createdOnUtc", "updatedOnUtc"];

// ----------------------------------------------------------------------------------------------------------------
// Flatten complete records (already in final server sort/page order) into display rows.
// Project/module labels are suppressed only when the previous displayed row has the same project AND module ids;
// every page starts with labels, and a group that reappears after another group shows them again.
// ----------------------------------------------------------------------------------------------------------------

export function flattenRecords (records) {
  const displayRows = [];
  (records || []).forEach(record => {
    (record.rows || []).forEach((row, rowIndex) => {
      const previous = displayRows[displayRows.length - 1];
      const sameGroup = !!previous &&
        previous.projectId === record.projectId &&
        previous.projectModuleId === record.projectModuleId;

      displayRows.push({
        key: row.id,
        id: row.id,
        recordId: record.recordId,
        projectId: record.projectId,
        projectName: record.projectName,
        projectModuleId: record.projectModuleId,
        projectModuleName: record.projectModuleName,
        trackingDate: fromApiDate(record.trackingDate),
        recordUpdatedOnUtc: record.updatedOnUtc,
        requirementId: row.requirementId,
        requirementNumber: row.requirementNumber,
        requirementTitle: row.requirementTitle,
        isRequirementUnavailable: !!row.isRequirementUnavailable,
        currentPercent: row.currentPercent,
        timeSpentAsOfToday: row.timeSpentAsOfToday,
        estimatedRemainingTime: row.estimatedRemainingTime,
        scope: row.scope,
        estimatedTimeForCompletion: row.estimatedTimeForCompletion,
        isRecordStart: rowIndex === 0,
        recordRowCount: record.rows.length,
        showProjectModule: !sameGroup
      });
    });
  });
  return displayRows;
}

// ----------------------------------------------------------------------------------------------------------------
// Selection is always by complete record id, never by requirement row or by project/module group.
// ----------------------------------------------------------------------------------------------------------------

export function toggleRecordSelection (selectedRecordIds, recordId) {
  return selectedRecordIds.includes(recordId)
    ? selectedRecordIds.filter(id => id !== recordId)
    : [...selectedRecordIds, recordId];
}

export function distinctRecordIds (ids) {
  return [...new Set((ids || []).filter(Boolean))];
}

// ----------------------------------------------------------------------------------------------------------------
// List request payload
// ----------------------------------------------------------------------------------------------------------------

// Returns { payload } or { error } when a date filter is not a valid MM/DD/YYYY date.
export function buildListPayload (search, pagination, multiSort) {
  const fromDate = search.fromDate ? toApiDate(search.fromDate) : null;
  const toDate = search.toDate ? toApiDate(search.toDate) : null;
  if (search.fromDate && !fromDate) return { error: "Date From must be a valid date (MM/DD/YYYY)." };
  if (search.toDate && !toDate) return { error: "Date To must be a valid date (MM/DD/YYYY)." };
  if (fromDate && toDate && fromDate > toDate) return { error: "Date From cannot be after Date To." };

  // Ordered multi-sort levels; insertion order is preserved in the JSON object.
  const sorts = {};
  (multiSort || []).forEach(level => {
    if (level.column && level.direction && SORTABLE_COLUMNS.includes(level.column)) sorts[level.column] = level.direction;
  });

  return {
    payload: {
      page: pagination.page,
      pageSize: pagination.rowsPerPage,
      sortBy: SORTABLE_COLUMNS.includes(pagination.sortBy) ? pagination.sortBy : "updatedOnUtc",
      descending: pagination.descending,
      sorts,
      searchText: search.searchText || "",
      projectIds: search.projectIds?.length ? search.projectIds : null,
      projectModuleIds: search.projectModuleIds?.length ? search.projectModuleIds : null,
      requirementIds: search.requirementIds?.length ? search.requirementIds : null,
      fromDate,
      toDate
    }
  };
}

export function formatNumber (value) {
  return value === null || value === undefined || value === "" ? "" : String(value);
}
