// Test Case Excel import configuration for the shared import flow (composables/excelImport/excelImportFlow.js)
// and dialog (components/excel-import/_excelImportDialog.vue). Mirrors TestCaseImportField in the API.
import {
  createImportState,
  createExcelImportFlow,
  buildConfirmRows as sharedBuildConfirmRows,
  checkFile as sharedCheckFile
} from "../../../composables/excelImport/excelImportFlow.js";

export const IMPORT_POLICY = Object.freeze({
  extension: ".xlsx",
  accept: ".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  maxFileSizeBytes: 5 * 1024 * 1024,
  maxRows: 500
});

// The 15 editable Test Case form fields, in form/template order. TestResult, release mappings and
// execution history are intentionally absent.
export const IMPORT_FIELDS = Object.freeze([
  { key: "projectId", label: "Project Name", labelKey: "projectName", required: true },
  { key: "planId", label: "Test Plan Name", labelKey: "planName", required: true },
  { key: "projectModuleId", label: "Project Module", labelKey: "projectModuleName", required: true },
  { key: "requirementId", label: "Requirement", labelKey: "requirementName", required: true },
  { key: "name", label: "Name", required: true },
  { key: "areaId", label: "Area", labelKey: "areaName", required: false },
  { key: "workspaceId", label: "Workspace", labelKey: "workspaceName", required: false },
  { key: "statusId", label: "Status", labelKey: "statusName", required: false },
  { key: "employeeId", label: "Assigned To", labelKey: "employeeName", required: false },
  { key: "testedBy", label: "Tested By", labelKey: "testedByName", required: true },
  { key: "testedDate", label: "Tested Date", required: false },
  { key: "description", label: "Description", required: false, multiline: true },
  { key: "steps", label: "Steps", required: false, multiline: true },
  { key: "expectedResult", label: "Expected Result", required: false, multiline: true },
  { key: "actualResult", label: "Actual Result", required: false, multiline: true }
]);

export const IMPORT_MESSAGES = Object.freeze({
  previewFailed: "Unable to preview the file. Please check the file and try again.",
  importFailed: "Import failed. No test cases were imported. Please review the errors and try again.",
  templateFailed: "Unable to download the Excel template.",
  fileType: "Only Excel .xlsx files created from the Test Case template are supported.",
  success: (data) => {
    const issues = Number(data.issuesCreated) || 0;
    return `${data.importedCount} test case(s) imported successfully.` +
      (issues > 0 ? ` ${issues} issue(s) created for failed test cases.` : "");
  }
});

// Read-only preview of the existing Fail rule, as predicted by the backend.
export const FAIL_ISSUE_NOTES = Object.freeze({
  create: "Fail: a new issue will be created on import.",
  exists: "Fail: an issue with this name already exists in the project; no new issue will be created.",
  created_in_batch: "Fail: the issue is created by an earlier row in this file; no new issue will be created."
});

export const TEST_CASE_IMPORT_CONFIG = Object.freeze({
  title: "Import Test Cases",
  fields: IMPORT_FIELDS,
  policy: IMPORT_POLICY,
  messages: IMPORT_MESSAGES,
  guidance: "Choose Project Name first; Test Plan Name and Project Module list that project's values, and Requirement lists the module's values. Assigned To is only used when Status is Fail. Tested Date is a date cell (MM/dd/yyyy, blank = today); blank Status = New.",
  rowNote: (row) => FAIL_ISSUE_NOTES[row?.failIssue] || null,
  summary: (raw) => (Number(raw?.failIssuesToCreate) > 0 ? [`Issues to create for failed cases: ${raw.failIssuesToCreate}`] : [])
});

export const buildConfirmRows = (rows) => sharedBuildConfirmRows(rows, IMPORT_FIELDS);
export const checkFile = (file) => sharedCheckFile(file, IMPORT_POLICY, IMPORT_MESSAGES);

export function createImportFlow (service, state = createImportState()) {
  return createExcelImportFlow(service, TEST_CASE_IMPORT_CONFIG, state);
}
