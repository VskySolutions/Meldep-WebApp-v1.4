// Test Plan Excel import configuration for the shared import flow (composables/excelImport/excelImportFlow.js).
// Keeps the module-level API used by the Test Plan dialog and tests (WEB/test/testPlanImportFlow.test.js).
import {
  createImportState,
  createExcelImportFlow,
  buildConfirmRows as sharedBuildConfirmRows,
  rowHasErrors,
  fieldErrors,
  rowLevelErrors as sharedRowLevelErrors,
  canConfirm,
  checkFile as sharedCheckFile,
  errorMessage
} from "../../../composables/excelImport/excelImportFlow.js";

// Mirrors TestPlanImportField in the API (.xlsx only, 5 MB, 500 rows).
export const IMPORT_POLICY = Object.freeze({
  extension: ".xlsx",
  accept: ".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  maxFileSizeBytes: 5 * 1024 * 1024,
  maxRows: 500
});

// The seven editable Test Plan form fields, in form/template order.
export const IMPORT_FIELDS = Object.freeze([
  { key: "projectId", label: "Project Name", labelKey: "projectName", required: true },
  { key: "name", label: "Name", required: true },
  { key: "areaId", label: "Area", labelKey: "areaName", required: false },
  { key: "workspaceId", label: "Workspace", labelKey: "workspaceName", required: false },
  { key: "planMakerId", label: "Plan Maker", labelKey: "planMakerName", required: true },
  { key: "planReviewerId", label: "Plan Reviewer", labelKey: "planReviewerName", required: true },
  { key: "description", label: "Description", required: false, multiline: true }
]);

export const IMPORT_MESSAGES = Object.freeze({
  previewFailed: "Unable to preview the file. Please check the file and try again.",
  importFailed: "Import failed. No test plans were imported. Please review the errors and try again.",
  templateFailed: "Unable to download the Excel template.",
  fileType: "Only Excel .xlsx files created from the Test Plan template are supported.",
  success: (data) => `${data.importedCount} test plan(s) imported successfully.`
});

export const TEST_PLAN_IMPORT_CONFIG = Object.freeze({
  title: "Import Test Plans",
  fields: IMPORT_FIELDS,
  policy: IMPORT_POLICY,
  messages: IMPORT_MESSAGES,
  guidance: "Select Project Name, Area, Workspace, Plan Maker and Plan Reviewer from the template's dropdowns."
});

export { createImportState, rowHasErrors, fieldErrors, canConfirm, errorMessage };

export const buildConfirmRows = (rows) => sharedBuildConfirmRows(rows, IMPORT_FIELDS);
export const rowLevelErrors = (row) => sharedRowLevelErrors(row, IMPORT_FIELDS);
export const checkFile = (file) => sharedCheckFile(file, IMPORT_POLICY, IMPORT_MESSAGES);

export function createImportFlow (service, state = createImportState()) {
  return createExcelImportFlow(service, TEST_PLAN_IMPORT_CONFIG, state);
}
