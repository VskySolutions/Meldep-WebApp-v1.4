// Unit tests for the Test Case Excel import flow (WO-72), using contract-aligned mocked API responses
// (WO-71 shapes). Run with:  node --test test/testCaseImportFlow.test.js
import { test } from "node:test";
import assert from "node:assert/strict";
import {
  IMPORT_FIELDS,
  IMPORT_MESSAGES,
  FAIL_ISSUE_NOTES,
  TEST_CASE_IMPORT_CONFIG,
  buildConfirmRows,
  checkFile,
  createImportFlow
} from "../src/modules/test-case/utils/importFlow.js";
import { fieldErrors, rowLevelErrors } from "../src/composables/excelImport/excelImportFlow.js";

const xlsx = (name = "cases.xlsx", size = 2048) => ({ name, size });

const validRow = (rowNumber, name = "Case A", extra = {}) => ({
  rowNumber,
  projectId: "proj-1", projectName: "Alpha",
  planId: "plan-1", planName: "Smoke Plan",
  projectModuleId: "mod-1", projectModuleName: "Login",
  requirementId: "req-1", requirementName: "User can log in",
  name,
  areaId: "area-1", areaName: "Payments",
  workspaceId: null, workspaceName: null,
  statusId: "st-pass", statusName: "Pass",
  employeeId: "emp-1", employeeName: "Asha",
  testedBy: "emp-2", testedByName: "Ravi",
  testedDate: "11/15/2026",
  description: "<script>alert(1)</script>",
  steps: "1. Open\n2. Login",
  expectedResult: "Logged in",
  actualResult: "Logged in",
  failIssue: null,
  isValid: true,
  errors: [],
  ...extra
});

const preview = (rows, extra = {}) => ({
  fileErrors: [],
  rows,
  totalRows: rows.length,
  validRows: rows.filter(r => !r.errors.length).length,
  invalidRows: rows.filter(r => r.errors.length).length,
  failIssuesToCreate: rows.filter(r => r.failIssue === "create").length,
  canConfirm: rows.length > 0 && rows.every(r => !r.errors.length),
  ...extra
});

function deferred () {
  let resolve, reject;
  const promise = new Promise((res, rej) => { resolve = res; reject = rej; });
  return { promise, resolve, reject };
}

function mockService ({ previewFn, confirmFn } = {}) {
  const calls = { preview: [], confirm: [] };
  return {
    calls,
    previewImport: async (file) => { calls.preview.push(file); return previewFn ? previewFn(file) : preview([validRow(2)]); },
    confirmImport: async (rows) => { calls.confirm.push(rows); return confirmFn ? confirmFn(rows) : { success: true, importedCount: rows.length, issuesCreated: 0 }; }
  };
}

test("shows all 15 editable fields in form order, without TestResult or release data", () => {
  assert.deepEqual(IMPORT_FIELDS.map(f => f.label), [
    "Project Name", "Test Plan Name", "Project Module", "Requirement", "Name", "Area", "Workspace", "Status",
    "Assigned To", "Tested By", "Tested Date", "Description", "Steps", "Expected Result", "Actual Result"
  ]);
  assert.deepEqual(IMPORT_FIELDS.filter(f => f.required).map(f => f.key), ["projectId", "planId", "projectModuleId", "requirementId", "name", "testedBy"]);
  assert.ok(!IMPORT_FIELDS.some(f => /testResult|mapping|release/i.test(f.key)));
});

test("lookup fields carry resolved-label keys; rich-text fields render as multiline text", () => {
  const withLabels = IMPORT_FIELDS.filter(f => f.labelKey).map(f => f.key);
  assert.deepEqual(withLabels, ["projectId", "planId", "projectModuleId", "requirementId", "areaId", "workspaceId", "statusId", "employeeId", "testedBy"]);
  assert.deepEqual(IMPORT_FIELDS.filter(f => f.multiline).map(f => f.key), ["description", "steps", "expectedResult", "actualResult"]);
});

test("confirm payload is allowlisted canonical fields only", () => {
  const tampered = { ...validRow(2), siteId: "x", testCaseNumber: 5, testResult: "Pass", projectReleaseTrackingReqPlanTaskIssueMappingId: "m1", failIssue: "create" };
  const [row] = buildConfirmRows([tampered]);
  assert.deepEqual(Object.keys(row).sort(), ["rowNumber", ...IMPORT_FIELDS.map(f => f.key)].sort());
  assert.equal(row.description, "<script>alert(1)</script>"); // data only; server encodes; UI renders as text
});

test("client file pre-check", () => {
  assert.equal(checkFile(xlsx("cases.csv"))[0].code, "file_type");
  assert.match(checkFile(xlsx("cases.csv"))[0].message, /Test Case template/);
  assert.equal(checkFile(xlsx("cases.xlsx", 6 * 1024 * 1024))[0].code, "file_too_large");
  assert.deepEqual(checkFile(xlsx()), []);
});

test("structural errors block confirmation", async () => {
  const flow = createImportFlow(mockService({ previewFn: () => ({ fileErrors: [{ code: "column_missing", message: "Template column 'Requirement' is missing." }], rows: [], canConfirm: false }) }));
  await flow.upload(xlsx());
  assert.equal(flow.state.preview, null);
  assert.equal(flow.canConfirm(), false);
});

test("any invalid row blocks the whole import; field errors map to cells", async () => {
  const bad = validRow(3, "Case B", { errors: [{ field: "planId", code: "lookup_invalid", message: "does not belong to the selected project" }, { field: null, code: "row_number_invalid", message: "Row number is missing" }] });
  const flow = createImportFlow(mockService({ previewFn: () => preview([validRow(2), bad], { canConfirm: true }) }));
  await flow.upload(xlsx());
  assert.equal(flow.canConfirm(), false);
  assert.equal(fieldErrors(flow.state.preview.rows[1], "planId").length, 1);
  assert.equal(rowLevelErrors(flow.state.preview.rows[1], IMPORT_FIELDS).length, 1);
  assert.equal((await flow.confirm()).skipped, true);
});

test("pending upload, empty preview and stale responses never enable confirmation", async () => {
  const first = deferred();
  let n = 0;
  const flow = createImportFlow(mockService({ previewFn: () => (++n === 1 ? first.promise : preview([])) }));
  const p1 = flow.upload(xlsx("old.xlsx"));
  assert.equal(flow.canConfirm(), false);
  await flow.upload(xlsx("new.xlsx")); // empty preview
  first.resolve(preview([validRow(2)]));
  assert.equal((await p1).ignored, true);
  assert.equal(flow.state.preview.rows.length, 0);
  assert.equal(flow.canConfirm(), false);
});

test("Fail issue outcomes are surfaced from the preview (read-only)", async () => {
  const rows = [
    validRow(2, "Known", { statusName: "Fail", failIssue: "exists" }),
    validRow(3, "New bug", { statusName: "Fail", failIssue: "create" }),
    validRow(4, "new BUG", { statusName: "Fail", failIssue: "created_in_batch" })
  ];
  const svc = mockService({ previewFn: () => preview(rows) });
  const flow = createImportFlow(svc);
  await flow.upload(xlsx());
  assert.deepEqual(flow.state.preview.rows.map(r => TEST_CASE_IMPORT_CONFIG.rowNote(r)), [FAIL_ISSUE_NOTES.exists, FAIL_ISSUE_NOTES.create, FAIL_ISSUE_NOTES.created_in_batch]);
  assert.deepEqual(TEST_CASE_IMPORT_CONFIG.summary(flow.state.preview.raw), ["Issues to create for failed cases: 1"]);
  assert.equal(TEST_CASE_IMPORT_CONFIG.rowNote(validRow(5)), null);
  assert.equal(svc.calls.confirm.length, 0);
});

test("success reports server case and issue counts", async () => {
  const flow = createImportFlow(mockService({ confirmFn: () => ({ success: true, importedCount: 3, issuesCreated: 1 }) }));
  await flow.upload(xlsx());
  const result = await flow.confirm();
  assert.equal(result.success, true);
  assert.equal(flow.state.resultMessage, "3 test case(s) imported successfully. 1 issue(s) created for failed test cases.");
  assert.equal(flow.state.phase, "imported");
  assert.equal(flow.canConfirm(), false);
});

test("success message without issues omits the issue sentence", () => {
  assert.equal(IMPORT_MESSAGES.success({ importedCount: 2, issuesCreated: 0 }), "2 test case(s) imported successfully.");
});

test("repeat clicks send one confirmation", async () => {
  const d = deferred();
  const svc = mockService({ confirmFn: () => d.promise });
  const flow = createImportFlow(svc);
  await flow.upload(xlsx());
  const c1 = flow.confirm();
  assert.equal((await flow.confirm()).skipped, true);
  d.resolve({ success: true, importedCount: 1, issuesCreated: 0 });
  await c1;
  assert.equal(svc.calls.confirm.length, 1);
});

test("confirmation-time validation failure keeps reviewable errors and reports zero", async () => {
  const validation = preview([validRow(2, "Case A", { errors: [{ field: "name", code: "duplicate_existing", message: "The test case already exists" }] })]);
  const flow = createImportFlow(mockService({
    confirmFn: () => { throw { response: { status: 400, data: { success: false, importedCount: 0, issuesCreated: 0, message: "Import failed. No test cases were imported. Please correct the errors and try again.", validation } } }; }
  }));
  await flow.upload(xlsx());
  const result = await flow.confirm();
  assert.equal(result.success, false);
  assert.equal(flow.state.importedCount, 0);
  assert.equal(fieldErrors(flow.state.preview.rows[0], "name")[0].code, "duplicate_existing");
  assert.equal(flow.canConfirm(), false);
  assert.match(flow.state.resultMessage, /No test cases were imported/);
});

test("runtime failure is not reported as success and is not retried", async () => {
  const svc = mockService({ confirmFn: () => { throw new Error("Network Error"); } });
  const flow = createImportFlow(svc);
  await flow.upload(xlsx());
  await flow.confirm();
  assert.equal(svc.calls.confirm.length, 1);
  assert.equal(flow.state.resultMessage, IMPORT_MESSAGES.importFailed);
  assert.notEqual(flow.state.phase, "imported");
  assert.ok(flow.state.preview.rows.length > 0);
});

test("cancel makes no confirmation request", async () => {
  const svc = mockService();
  const flow = createImportFlow(svc);
  await flow.upload(xlsx());
  flow.reset();
  assert.equal(svc.calls.confirm.length, 0);
  assert.equal(flow.canConfirm(), false);
});

// ---------------------------------------------------------------------------------------------
// Template download error reporting (blob error bodies)
// ---------------------------------------------------------------------------------------------
import { describeDownloadError } from "../src/composables/excelImport/downloadBlob.js";

test("download error: 404 explains the API must be restarted/redeployed", async () => {
  const msg = await describeDownloadError({ response: { status: 404, data: new Blob([""]) } }, "Unable to download the Excel template.");
  assert.match(msg, /restart or redeploy the API/);
});

test("download error: server message inside a Blob body is surfaced and HTML-escaped", async () => {
  const body = new Blob([JSON.stringify("Object reference <b>not</b> set")], { type: "application/json" });
  const msg = await describeDownloadError({ response: { status: 400, data: body } }, "Unable to download the Excel template.");
  assert.equal(msg, "Unable to download the Excel template. Object reference &lt;b&gt;not&lt;/b&gt; set");
});

test("download error: ApiError {message} and network errors", async () => {
  const api = await describeDownloadError({ response: { status: 400, data: new Blob([JSON.stringify({ code: 400, message: "Bad request." })]) } }, "X.");
  assert.equal(api, "X. Bad request.");
  const net = await describeDownloadError(new Error("Network Error"), "X.");
  assert.match(net, /could not be reached/);
});
