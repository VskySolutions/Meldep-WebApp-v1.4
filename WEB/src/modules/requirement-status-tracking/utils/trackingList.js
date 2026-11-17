// Requirement-group sort keys accepted by the API allowlist. trackingDate/createdOnUtc/updatedOnUtc sort groups by
// their latest/earliest/latest row; rows inside a group are always newest tracking date first.
export const SORTABLE_COLUMNS = ["project.name", "projectModule.name", "requirementTitle", "trackingDate", "createdOnUtc", "updatedOnUtc"];

// Options for the multi-sort dialog (group level).
export const SORT_OPTIONS = [
  { name: "requirementTitle", label: "Requirement Title", field: "requirementTitle" },
  { name: "projectName", label: "Project Name", field: "project.name" },
  { name: "projectModuleName", label: "Module Name", field: "projectModule.name" },
  { name: "trackingDate", label: "Latest Tracking Date", field: "trackingDate" },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc" },
  { name: "updatedOnUtc", label: "Updated Date", field: "updatedOnUtc" }
];

// ----------------------------------------------------------------------------------------------------------------
// Requirement grouping: the API returns requirement groups (already in final sort/page order), each with its
// tracking rows (newest tracking date first). The table renders one header row per requirement followed by
// that requirement's tracking rows.
// ----------------------------------------------------------------------------------------------------------------

export function groupRequirementRows (requirements) {
  const displayRows = [];
  (requirements || []).forEach(group => {
    const groupKey = `${group.requirementId}|${group.projectModuleId}`;
    displayRows.push({
      key: `group:${groupKey}`,
      isGroup: true,
      groupKey,
      requirementId: group.requirementId,
      requirementNumber: group.requirementNumber,
      requirementTitle: group.requirementTitle,
      isRequirementUnavailable: !!group.isRequirementUnavailable,
      projectId: group.projectId,
      projectName: group.projectName,
      projectModuleId: group.projectModuleId,
      projectModuleName: group.projectModuleName,
      rowCount: group.rowCount ?? (group.rows || []).length
    });
    (group.rows || []).forEach(row => {
      displayRows.push({
        key: row.id,
        isGroup: false,
        groupKey,
        id: row.id,
        recordId: row.id,
        requirementId: group.requirementId,
        requirementNumber: group.requirementNumber,
        requirementTitle: group.requirementTitle,
        projectId: group.projectId,
        projectName: group.projectName,
        projectModuleId: group.projectModuleId,
        projectModuleName: group.projectModuleName,
        trackingDate: fromApiDate(row.trackingDate),
        currentPercent: row.currentPercent,
        timeSpentAsOfToday: row.timeSpentAsOfToday,
        estimatedRemainingTime: row.estimatedRemainingTime,
        scope: row.scope,
        roadblock: row.roadblock,
        estimatedTimeForCompletion: row.estimatedTimeForCompletion,
        createdByName: row.createdByName,
        createdOnUtc: row.createdOnUtc,
        updatedByName: row.updatedByName,
        updatedOnUtc: row.updatedOnUtc
      });
    });
  });
  return displayRows;
}

// ----------------------------------------------------------------------------------------------------------------
// Dates: the page uses MM/DD/YYYY, the API uses yyyy-MM-dd (calendar dates, no time zone shifts).
// ----------------------------------------------------------------------------------------------------------------

// "MM/DD/YYYY" (M/D/YYYY accepted) -> "YYYY-MM-DD", or null when it is not a real calendar date.
export function toApiDate (value) {
  const m = /^\s*(\d{1,2})\/(\d{1,2})\/(\d{4})\s*$/.exec(String(value ?? ""));
  if (!m) return null;
  const [month, day, year] = [Number(m[1]), Number(m[2]), Number(m[3])];
  const d = new Date(Date.UTC(year, month - 1, day));
  if (d.getUTCFullYear() !== year || d.getUTCMonth() !== month - 1 || d.getUTCDate() !== day) return null;
  return `${year}-${String(month).padStart(2, "0")}-${String(day).padStart(2, "0")}`;
}

// "YYYY-MM-DD" (optionally followed by a time part) -> "MM/DD/YYYY"; other values are returned unchanged.
export function fromApiDate (value) {
  const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(String(value ?? ""));
  return m ? `${m[2]}/${m[3]}/${m[1]}` : (value ?? "");
}

// Rich-text (q-editor HTML) to single-line plain text for list cells; never rendered as HTML.
export function htmlToText (html) {
  if (!html) return "";
  return String(html)
    .replace(/<br\s*\/?>/gi, " ")
    .replace(/<[^>]*>/g, " ")
    .replace(/&nbsp;/gi, " ")
    .replace(/&lt;/gi, "<")
    .replace(/&gt;/gi, ">")
    .replace(/&quot;/gi, "\"")
    .replace(/&#39;/gi, "'")
    .replace(/&amp;/gi, "&")
    .replace(/\s+/g, " ")
    .trim();
}

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
        createdByName: record.createdByName,
        createdOnUtc: record.createdOnUtc,
        updatedByName: record.updatedByName,
        updatedOnUtc: record.updatedOnUtc,
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

// All tracking record ids of one Requirement ID (every project module it was tracked under). Records of any
// other requirement are excluded even if the server response contained them. Used by the requirement Delete.
export function trackingIdsForRequirement (records, group) {
  const same = (a, b) => String(a ?? "").toLowerCase() === String(b ?? "").toLowerCase();
  if (!group?.requirementId) return [];
  return distinctRecordIds((records || [])
    .filter(r => r && same(r.requirementId, group.requirementId))
    .map(r => r.id));
}

// ----------------------------------------------------------------------------------------------------------------
// List request payload
// ----------------------------------------------------------------------------------------------------------------

// Field messages for the Tracking Date range filter (both dates inclusive); empty strings when valid.
export function dateRangeErrors (fromValue, toValue) {
  const fromDate = fromValue ? toApiDate(fromValue) : null;
  const toDate = toValue ? toApiDate(toValue) : null;
  const errors = { fromDate: "", toDate: "" };
  if (fromValue && !fromDate) errors.fromDate = "From Date must be a valid date (MM/DD/YYYY).";
  if (toValue && !toDate) errors.toDate = "To Date must be a valid date (MM/DD/YYYY).";
  if (fromDate && toDate && fromDate > toDate) errors.toDate = "To Date cannot be before From Date.";
  return errors;
}

// Keeps only the selected ids that are still offered by a dependent dropdown (e.g. modules of the selected
// projects); ids no longer available are dropped instead of silently filtering the list.
export function keepAvailable (selectedIds, options) {
  const available = new Set((options || []).map(o => String(o?.value ?? "").toLowerCase()));
  return (selectedIds || []).filter(id => available.has(String(id ?? "").toLowerCase()));
}

// Returns { payload } or { error } when a date filter is not a valid MM/DD/YYYY date.
export function buildListPayload (search, pagination, multiSort) {
  const fromDate = search.fromDate ? toApiDate(search.fromDate) : null;
  const toDate = search.toDate ? toApiDate(search.toDate) : null;
  const dateErrors = dateRangeErrors(search.fromDate, search.toDate);
  if (dateErrors.fromDate || dateErrors.toDate) return { error: dateErrors.fromDate || dateErrors.toDate };

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
