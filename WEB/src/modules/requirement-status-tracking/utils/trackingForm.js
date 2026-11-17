// Pure helpers for the Requirement Status Tracking Add/Edit form (no Vue/Quasar imports so they can be tested in isolation).
// Limits mirror the API validator: completion decimal(5,2) in [0,100], hours decimal(10,2) >= 0, at most 2 decimals.
import { format } from "date-fns";

export const MAX_HOURS = 99999999.99;

export const HOUR_FIELDS = [
  { key: "timeSpentAsOfToday", label: "Time Spent as of Now (Hours)" },
  { key: "estimatedRemainingTime", label: "Estimated Remaining Time Required (Hours)" },
  { key: "estimatedTimeForCompletion", label: "Estimated Time for Completion of Additional Scope (Hours)" }
];

// ----------------------------------------------------------------------------------------------------------------
// Numbers: blank stays null (not 0); anything non-finite is reported as invalid.
// ----------------------------------------------------------------------------------------------------------------

export function toNumberOrNull (value) {
  if (value === null || value === undefined) return null;
  if (typeof value === "string" && value.trim() === "") return null;
  return typeof value === "number" ? value : Number(String(value).trim());
}

function validateCompletion(value) {
  if (value === null || value === undefined || value === "") {
    return "";
  }

  if (!/^\d+(\.\d{1,2})?$/.test(String(value))) {
    return "Enter a valid percentage.";
  }

  const percentage = Number(value);

  if (percentage < 0 || percentage > 100) {
    return "Enter a value between 0 and 100.";
  }

  return "";
}

// ----------------------------------------------------------------------------------------------------------------
// Rows
// ----------------------------------------------------------------------------------------------------------------

export function createRow () {
  return {
    id: "",
    trackingDate: format(new Date(), "MM/dd/yyyy"),
    currentPercent: null,
    timeSpentAsOfToday: null,
    estimatedRemainingTime: null,
    scope: "",
    roadblock: "",
    estimatedTimeForCompletion: null
  };
}

// One row per distinct requirement id (duplicate lookup entries never create duplicate rows).
export function rowsFromRequirements (options) {
  const seen = new Set();
  return (options || []).filter(option => {
    if (!option?.value || seen.has(option.value)) return false;
    seen.add(option.value);
    return true;
  }).map(option => createRow(option));
}

export function dedupeOptions (options) {
  const seen = new Set();
  return (options || []).filter(option => option?.value && !seen.has(option.value) && seen.add(option.value));
}

// Saved rows keep their ids and values; nothing is regenerated from lookups.
export function rowsFromRecord (record) {
  return (record?.rows || []).map(row => ({
    key: row.id,
    id: row.id,
    trackingDate: row.trackingDate,
    currentPercent: row.currentPercent ?? null,
    timeSpentAsOfToday: row.timeSpentAsOfToday ?? null,
    estimatedRemainingTime: row.estimatedRemainingTime ?? null,
    scope: row.scope || "",
    roadblock: row.roadblock || "",
    estimatedTimeForCompletion: row.estimatedTimeForCompletion ?? null
  }));
}

// ----------------------------------------------------------------------------------------------------------------
// Whole-draft validation
// ----------------------------------------------------------------------------------------------------------------

export function validateDraft (model, rows) {
  const header = {
    projectId: model.projectId ? "" : "Project Name is required.",
    projectModuleId: model.projectModuleId ? "" : "Project Module is required.",
    requirementId: model.requirementId ? "" : "Requirement is required.",
  };
  const rowErrors = {};
  const messages = [];

  if (!rows.length) messages.push("Add at least one requirement row.");

  rows.forEach((row, index) => {
    const errors = {
      trackingDate: row.trackingDate ? "" : "Tracking Date is required.",
      currentPercent: validateCompletion(row.currentPercent)
    };
    if (Object.values(errors).some(Boolean)) {
      rowErrors[row.key] = errors;
      // messages.push(`Row ${index + 1}: ${Object.values(errors).filter(Boolean).join(" ")}`);
    }
  });

  const isValid = !Object.values(header).some(Boolean) && Object.keys(rowErrors).length === 0 && rows.length > 0;
  return { isValid, header, rows: rowErrors, messages };
}

// ----------------------------------------------------------------------------------------------------------------
// Payload: the entire grid plus staged removals, sent once to /bulk-add (no recordId) or /bulk-save.
// ----------------------------------------------------------------------------------------------------------------

export function buildPayload (model, rows, removedRowIds) {
  return {
    recordId: model.recordId || null,
    projectId: model.projectId,
    projectModuleId: model.projectModuleId,
    requirementId: model.requirementId,
    rows: rows.map(row => ({
      id: row.id || null,
      trackingDate: row.trackingDate,
      currentPercent: toNumberOrNull(row.currentPercent),
      timeSpentAsOfToday: row.timeSpentAsOfToday,
      estimatedRemainingTime: row.estimatedRemainingTime,
      scope: (row.scope || "").trim() || null,
      roadblock: (row.roadblock || "").trim() || null,
      estimatedTimeForCompletion: row.estimatedTimeForCompletion
    })),
    removedRowIds: model.recordId ? [...new Set(removedRowIds)] : []
  };
}
// The API joins validation messages with "~" (ApiError); split them for inline display.
export function parseServerErrors (error) {
  const message = error?.response?.data?.message || (typeof error?.response?.data === "string" ? error.response.data : "");
  return message ? message.split("~").map(m => m.trim()).filter(Boolean) : [];
}
