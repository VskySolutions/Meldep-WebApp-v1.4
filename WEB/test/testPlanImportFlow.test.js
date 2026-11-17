// Unit tests for the Test Plan Excel import flow (WO-70), using contract-aligned mocked API responses.
// Run with Node's built-in runner (no extra dependencies):  node --test test/
import { test } from "node:test";
import assert from "node:assert/strict";
import {
  IMPORT_FIELDS,
  IMPORT_MESSAGES,
  buildConfirmRows,
  canConfirm,
  checkFile,
  createImportFlow,
  createImportState,
  fieldErrors,
  rowLevelErrors
} from "../src/modules/test-plan/utils/importFlow.js";

const xlsx = (name = "plans.xlsx", size = 1024) => ({ name, size });

const validRow = (rowNumber, name = "Plan A") => ({
  rowNumber,
  projectId: "proj-1",
  projectName: "Alpha",
  name,
  areaId: "area-1",
  areaName: "Payments",
  workspaceId: null,
  workspaceName: null,
  planMakerId: "emp-1",
  planMakerName: "Asha",
  planReviewerId: "emp-2",
  planReviewerName: "Ravi",
  description: "<b>text</b>",
  isValid: true,
  errors: []
});

const validPreview = (rows = [validRow(2), validRow(3, "Plan B")]) => ({
  fileErrors: [],
  rows,
  totalRows: rows.length,
  validRows: rows.length,
  invalidRows: 0,
  canConfirm: true
});

function deferred () {
  let resolve, reject;
  const promise = new Promise((res, rej) => { resolve = res; reject = rej; });
  return { promise, resolve, reject };
}

function mockService (overrides = {}) {
  const calls = { preview: [], confirm: [] };
  return {
    calls,
    previewImport: async (file) => { calls.preview.push(file); return overrides.preview ? overrides.preview(file) : validPreview(); },
    confirmImport: async (rows) => { calls.confirm.push(rows); return overrides.confirm ? overrides.confirm(rows) : { success: true, importedCount: rows.length, created: [] }; }
  };
}

// ---------------------------------------------------------------------------------------------
// Fields / payload
// ---------------------------------------------------------------------------------------------
test("preview shows all seven editable fields in form order", () => {
  assert.deepEqual(IMPORT_FIELDS.map(f => f.label), ["Project Name", "Name", "Area", "Workspace", "Plan Maker", "Plan Reviewer", "Description"]);
  assert.deepEqual(IMPORT_FIELDS.filter(f => f.required).map(f => f.key), ["projectId", "name", "planMakerId", "planReviewerId"]);
});

test("confirm payload contains only allowlisted canonical fields", () => {
  const tampered = { ...validRow(2), isValid: true, siteId: "other-site", testPlanNumber: 99, createdById: "x", projectName: "Alpha" };
  const [row] = buildConfirmRows([tampered]);
  assert.deepEqual(Object.keys(row).sort(), ["areaId", "description", "name", "planMakerId", "planReviewerId", "projectId", "rowNumber", "workspaceId"]);
  assert.equal(row.description, "<b>text</b>"); // sent as data; backend encodes it
});

test("field and row-level errors are split for cell highlighting", () => {
  const row = { ...validRow(2), errors: [{ field: "name", message: "dup" }, { field: null, message: "row" }] };
  assert.equal(fieldErrors(row, "name").length, 1);
  assert.equal(fieldErrors(row, "projectId").length, 0);
  assert.deepEqual(rowLevelErrors(row).map(e => e.message), ["row"]);
});

// ---------------------------------------------------------------------------------------------
// Upload / preview
// ---------------------------------------------------------------------------------------------
test("client file pre-check mirrors API policy", () => {
  assert.equal(checkFile(null)[0].code, "file_empty");
  assert.equal(checkFile(xlsx("plans.csv"))[0].code, "file_type");
  assert.equal(checkFile(xlsx("plans.xlsx", 0))[0].code, "file_empty");
  assert.equal(checkFile(xlsx("plans.xlsx", 6 * 1024 * 1024))[0].code, "file_too_large");
  assert.deepEqual(checkFile(xlsx("PLANS.XLSX")), []);
});

test("unsupported file is rejected locally without calling the API", async () => {
  const svc = mockService();
  const flow = createImportFlow(svc);
  await flow.upload(xlsx("plans.xls"));
  assert.equal(svc.calls.preview.length, 0);
  assert.equal(flow.state.fileErrors[0].code, "file_type");
  assert.equal(flow.canConfirm(), false);
});

test("valid preview enables confirmation", async () => {
  const flow = createImportFlow(mockService());
  await flow.upload(xlsx());
  assert.equal(flow.state.phase, "previewed");
  assert.equal(flow.state.preview.totalRows, 2);
  assert.equal(flow.canConfirm(), true);
});

test("structural file errors (missing column, unreadable, empty) block confirmation", async () => {
  for (const code of ["column_missing", "file_unreadable", "no_rows"]) {
    const flow = createImportFlow(mockService({ preview: () => ({ fileErrors: [{ code, message: code }], rows: [], canConfirm: false }) }));
    await flow.upload(xlsx());
    assert.equal(flow.state.preview, null);
    assert.equal(flow.state.fileErrors[0].code, code);
    assert.equal(flow.canConfirm(), false, code);
  }
});

test("any invalid row blocks the entire import, even if the server flag says otherwise", async () => {
  const rows = [validRow(2), { ...validRow(3), isValid: false, errors: [{ field: "name", code: "duplicate_existing", message: "The test plan already exists" }] }];
  const flow = createImportFlow(mockService({ preview: () => ({ ...validPreview(rows), canConfirm: true }) }));
  await flow.upload(xlsx());
  assert.equal(flow.canConfirm(), false);
  const result = await flow.confirm();
  assert.equal(result.skipped, true);
});

test("empty preview cannot be confirmed", async () => {
  const flow = createImportFlow(mockService({ preview: () => validPreview([]) }));
  await flow.upload(xlsx());
  assert.equal(flow.canConfirm(), false);
});

test("confirmation is unavailable while upload/preview is processing", async () => {
  const d = deferred();
  const flow = createImportFlow(mockService({ preview: () => d.promise }));
  const pending = flow.upload(xlsx());
  assert.equal(flow.state.phase, "uploading");
  assert.equal(flow.canConfirm(), false);
  d.resolve(validPreview());
  await pending;
  assert.equal(flow.canConfirm(), true);
});

test("replacing the file invalidates the previous preview and ignores stale responses", async () => {
  const first = deferred();
  const second = deferred();
  let n = 0;
  const flow = createImportFlow(mockService({ preview: () => (++n === 1 ? first.promise : second.promise) }));

  const p1 = flow.upload(xlsx("old.xlsx"));
  const p2 = flow.upload(xlsx("new.xlsx"));
  second.resolve(validPreview([validRow(2, "New")]));
  await p2;
  first.resolve(validPreview([validRow(2, "Old")]));
  const r1 = await p1;

  assert.equal(r1.ignored, true);
  assert.equal(flow.state.preview.rows[0].name, "New");
  assert.equal(flow.state.file.name, "new.xlsx");
});

test("selecting a new file after a preview clears it immediately", async () => {
  const d = deferred();
  let n = 0;
  const flow = createImportFlow(mockService({ preview: () => (++n === 1 ? validPreview() : d.promise) }));
  await flow.upload(xlsx());
  assert.equal(flow.canConfirm(), true);
  flow.upload(xlsx("replacement.xlsx"));
  assert.equal(flow.state.preview, null);
  assert.equal(flow.canConfirm(), false);
  d.resolve(validPreview());
});

test("preview request failure shows the API message and blocks confirmation", async () => {
  const flow = createImportFlow(mockService({ preview: () => { throw { response: { data: "Bad things" } }; } }));
  await flow.upload(xlsx());
  assert.equal(flow.state.fileErrors[0].message, "Bad things");
  assert.equal(flow.canConfirm(), false);
});

test("cancel/reset makes no confirmation request", async () => {
  const svc = mockService();
  const flow = createImportFlow(svc);
  await flow.upload(xlsx());
  flow.reset();
  assert.equal(svc.calls.confirm.length, 0);
  assert.equal(flow.state.preview, null);
  assert.equal(flow.canConfirm(), false);
});

// ---------------------------------------------------------------------------------------------
// Confirm
// ---------------------------------------------------------------------------------------------
test("successful confirmation reports the server's imported count", async () => {
  const flow = createImportFlow(mockService({ confirm: () => ({ success: true, importedCount: 2 }) }));
  await flow.upload(xlsx());
  const result = await flow.confirm();
  assert.equal(result.success, true);
  assert.equal(result.importedCount, 2);
  assert.equal(flow.state.phase, "imported");
  assert.equal(flow.state.resultMessage, "2 test plan(s) imported successfully.");
  assert.equal(flow.canConfirm(), false); // cannot import the same preview twice
});

test("repeat clicks while confirming send only one request", async () => {
  const d = deferred();
  const svc = mockService({ confirm: () => d.promise });
  const flow = createImportFlow(svc);
  await flow.upload(xlsx());
  const c1 = flow.confirm();
  const c2 = await flow.confirm();
  assert.equal(c2.skipped, true);
  assert.equal(flow.canConfirm(), false);
  const ignored = await flow.upload(xlsx("other.xlsx"));
  assert.equal(ignored.ignored, true);
  d.resolve({ success: true, importedCount: 2 });
  await c1;
  assert.equal(svc.calls.confirm.length, 1);
});

test("confirmation-time validation failure keeps reviewable errors and blocks re-confirm", async () => {
  const validation = validPreview([validRow(2), { ...validRow(3), errors: [{ rowNumber: 3, field: "projectId", code: "lookup_invalid", message: "Project ID was not found" }] }]);
  validation.canConfirm = false;
  validation.invalidRows = 1;
  validation.validRows = 1;
  const flow = createImportFlow(mockService({
    confirm: () => { throw { response: { status: 400, data: { success: false, importedCount: 0, message: "Import failed. No test plans were imported.", validation } } }; }
  }));
  await flow.upload(xlsx());
  const result = await flow.confirm();

  assert.equal(result.success, false);
  assert.equal(flow.state.importedCount, 0);
  assert.equal(flow.state.resultType, "negative");
  assert.equal(flow.state.preview.invalidRows, 1);
  assert.equal(fieldErrors(flow.state.preview.rows[1], "projectId")[0].code, "lookup_invalid");
  assert.equal(flow.canConfirm(), false);
});

test("runtime failure never reports success and does not auto-retry", async () => {
  const svc = mockService({ confirm: () => { throw { response: { status: 400, data: { success: false, importedCount: 0, message: "Import failed. No test plans were imported. db down" } } }; } });
  const flow = createImportFlow(svc);
  await flow.upload(xlsx());
  const result = await flow.confirm();
  assert.equal(result.success, false);
  assert.equal(svc.calls.confirm.length, 1);
  assert.match(flow.state.resultMessage, /No test plans were imported/);
  assert.notEqual(flow.state.phase, "imported");
  assert.ok(flow.state.preview.rows.length > 0); // results preserved for review
});

test("network failure without body uses the established import error message", async () => {
  const flow = createImportFlow(mockService({ confirm: () => { throw new Error("Network Error"); } }));
  await flow.upload(xlsx());
  await flow.confirm();
  assert.equal(flow.state.resultMessage, IMPORT_MESSAGES.importFailed);
});

test("a 200 response without success=true is treated as failure", async () => {
  const flow = createImportFlow(mockService({ confirm: () => ({ importedCount: 2 }) }));
  await flow.upload(xlsx());
  const result = await flow.confirm();
  assert.equal(result.success, false);
  assert.equal(flow.state.importedCount, 0);
});

test("state factory starts empty", () => {
  const s = createImportState();
  assert.equal(canConfirm(s), false);
  assert.equal(s.phase, "idle");
});
