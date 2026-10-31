// Pure helpers for the Requirement Status Tracking Add/Edit form (no Vue/Quasar imports so they can be tested in isolation).
// Limits mirror the API validator: completion decimal(5,2) in [0,100], hours decimal(10,2) >= 0, at most 2 decimals.

export const MAX_HOURS = 99999999.99;

export const HOUR_FIELDS = [
  { key: "timeSpentAsOfToday", label: "Time Spent as of Now (Hours)" },
  { key: "estimatedRemainingTime", label: "Estimated Remaining Time Required (Hours)" },
  { key: "estimatedTimeForCompletion", label: "Estimated Time for Completion of Additional Scope (Hours)" }
];

let rowSeq = 0;
const newRowKey = () => `new-${++rowSeq}`;

// ----------------------------------------------------------------------------------------------------------------
// Dates: the form uses MM/DD/YYYY (formDate), the API uses yyyy-MM-dd. Pure string conversion so no timezone shift.
// ----------------------------------------------------------------------------------------------------------------

function isRealDate (year, month, day) {
  if (year < 1900 || year > 9999 || month < 1 || month > 12 || day < 1) return false;
  const daysInMonth = [31, (year % 4 === 0 && year % 100 !== 0) || year % 400 === 0 ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
  return day <= daysInMonth[month - 1];
}

export function toApiDate (displayDate) {
  const match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec((displayDate || "").trim());
  if (!match) return null;
  const [, mm, dd, yyyy] = match;
  return isRealDate(Number(yyyy), Number(mm), Number(dd)) ? `${yyyy}-${mm}-${dd}` : null;
}

export function fromApiDate (apiDate) {
  const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(apiDate || "");
  return match ? `${match[2]}/${match[3]}/${match[1]}` : "";
}

// ----------------------------------------------------------------------------------------------------------------
// Numbers: blank stays null (not 0); anything non-finite is reported as invalid.
// ----------------------------------------------------------------------------------------------------------------

export function toNumberOrNull (value) {
  if (value === null || value === undefined) return null;
  if (typeof value === "string" && value.trim() === "") return null;
  return typeof value === "number" ? value : Number(String(value).trim());
}

const hasAtMostTwoDecimals = (value) => Math.round(value * 100) / 100 === value;

function validateHours (value, label) {
  const number = toNumberOrNull(value);
  if (number === null) return "";
  if (!Number.isFinite(number)) return `${label} must be a number.`;
  if (number < 0) return `${label} cannot be negative.`;
  if (number > MAX_HOURS) return `${label} must not exceed ${MAX_HOURS}.`;
  if (!hasAtMostTwoDecimals(number)) return `${label} allows at most 2 decimal places.`;
  return "";
}

function validateCompletion (value) {
  const number = toNumberOrNull(value);
  if (number === null) return "";
  if (!Number.isFinite(number)) return "Completion must be a number.";
  if (number < 0 || number > 100) return "Completion must be between 0 and 100.";
  if (!hasAtMostTwoDecimals(number)) return "Completion allows at most 2 decimal places.";
  return "";
}

// ----------------------------------------------------------------------------------------------------------------
// Rows
// ----------------------------------------------------------------------------------------------------------------

export function createRow (requirement = null, isManual = false) {
  return {
    key: newRowKey(),
    id: "",
    requirementId: requirement?.value || "",
    requirementTitle: requirement?.text || "",
    isRequirementUnavailable: false,
    isManual,
    currentPercent: null,
    timeSpentAsOfToday: null,
    estimatedRemainingTime: null,
    scope: "",
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
    requirementId: row.requirementId,
    requirementTitle: row.requirementTitle || "",
    isRequirementUnavailable: !!row.isRequirementUnavailable,
    isManual: false,
    currentPercent: row.currentPercent ?? null,
    timeSpentAsOfToday: row.timeSpentAsOfToday ?? null,
    estimatedRemainingTime: row.estimatedRemainingTime ?? null,
    scope: row.scope || "",
    estimatedTimeForCompletion: row.estimatedTimeForCompletion ?? null
  }));
}

export function rowHasEnteredValues (row) {
  return row.isManual ||
    [row.currentPercent, row.timeSpentAsOfToday, row.estimatedRemainingTime, row.estimatedTimeForCompletion].some(v => toNumberOrNull(v) !== null) ||
    !!(row.scope || "").trim();
}

// Index of another row already using the requirement, or -1.
export function findDuplicateRowIndex (rows, rowKey, requirementId) {
  if (!requirementId) return -1;
  return rows.findIndex(row => row.key !== rowKey && row.requirementId === requirementId);
}

// ----------------------------------------------------------------------------------------------------------------
// Whole-draft validation
// ----------------------------------------------------------------------------------------------------------------

export function validateDraft (model, rows) {
  const header = {
    projectId: model.projectId ? "" : "Project Name is required.",
    projectModuleId: model.projectModuleId ? "" : "Project Module is required.",
    trackingDate: !model.trackingDate
      ? "Date is required."
      : (toApiDate(model.trackingDate) ? "" : "Enter a valid date (MM/DD/YYYY).")
  };
  const rowErrors = {};
  const messages = [];

  if (!rows.length) messages.push("Add at least one requirement row.");

  rows.forEach((row, index) => {
    const errors = {
      requirementId: row.requirementId ? "" : "Requirement is required.",
      currentPercent: validateCompletion(row.currentPercent)
    };
    HOUR_FIELDS.forEach(field => { errors[field.key] = validateHours(row[field.key], field.label); });

    const duplicateIndex = findDuplicateRowIndex(rows, row.key, row.requirementId);
    if (duplicateIndex !== -1) errors.requirementId = `Already selected in row ${duplicateIndex + 1}.`;

    if (Object.values(errors).some(Boolean)) {
      rowErrors[row.key] = errors;
      messages.push(`Row ${index + 1}: ${Object.values(errors).filter(Boolean).join(" ")}`);
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
    trackingDate: toApiDate(model.trackingDate),
    rows: rows.map(row => ({
      id: row.id || null,
      requirementId: row.requirementId,
      currentPercent: toNumberOrNull(row.currentPercent),
      timeSpentAsOfToday: toNumberOrNull(row.timeSpentAsOfToday),
      estimatedRemainingTime: toNumberOrNull(row.estimatedRemainingTime),
      scope: (row.scope || "").trim() || null,
      estimatedTimeForCompletion: toNumberOrNull(row.estimatedTimeForCompletion)
    })),
    removedRowIds: model.recordId ? [...new Set(removedRowIds)] : []
  };
}

// The API joins validation messages with "~" (ApiError); split them for inline display.
export function parseServerErrors (error) {
  const message = error?.response?.data?.message || (typeof error?.response?.data === "string" ? error.response.data : "");
  return message ? message.split("~").map(m => m.trim()).filter(Boolean) : [];
}
