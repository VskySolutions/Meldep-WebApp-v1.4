// Unit tests for the Requirement List "Export Requirement" row action (WO-74), using mocked responses
// that follow the WO-73 contract (GET /requirement/{id}/export). Run with:
//   node --test test/requirementExport.test.js
import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import {
  XLSX_MIME,
  createRequirementExportAction,
  describeExportError,
  fallbackExportFilename,
  filenameFromContentDisposition,
  sanitizeExportFilename
} from "../src/modules/requirement/utils/requirementExport.js";

const workbookBlob = () => new Blob([new Uint8Array([0x50, 0x4b, 0x03, 0x04, 1, 2, 3])], { type: XLSX_MIME });
const jsonBlob = (body) => new Blob([JSON.stringify(body)], { type: "application/json" });

const ok = (headers = {}) => ({ status: 200, data: workbookBlob(), headers: { "content-type": XLSX_MIME, ...headers } });
const httpError = (status, data) => Object.assign(new Error(`HTTP ${status}`), { response: { status, data, headers: {} } });

function deferred () {
  let resolve, reject;
  const promise = new Promise((res, rej) => { resolve = res; reject = rej; });
  return { promise, resolve, reject };
}

function harness (exportImpl) {
  const calls = { export: [], download: [], error: [], success: [] };
  const action = createRequirementExportAction({
    exportRequirement: (id) => { calls.export.push(id); return exportImpl(id, calls.export.length); },
    download: (blob, filename) => calls.download.push({ blob, filename }),
    notifyError: (n) => calls.error.push(n.message),
    notifySuccess: (n) => calls.success.push(n.message)
  });
  return { action, calls };
}

// Three visible rows; row B is also "bulk selected" to prove selection is never used.
const rows = [
  { id: "req-A", requirementNumber: 41, title: "A", checkboxStatus: false },
  { id: "req-B", requirementNumber: 42, title: "B", checkboxStatus: true },
  { id: "req-C", requirementNumber: 43, title: "C", checkboxStatus: true }
];

test("exports only the clicked row's requirement, ignoring other visible or selected rows", async () => {
  const { action, calls } = harness(async () => ok());

  assert.equal(await action.exportRow(rows[0]), true);

  assert.deepEqual(calls.export, ["req-A"]);
  assert.equal(calls.download.length, 1);
  assert.equal(calls.download[0].filename, "Requirement-41.xlsx");
  assert.equal(calls.error.length, 0);
});

test("different rows can export independently and each gets its own file", async () => {
  const { action, calls } = harness(async () => ok());

  await Promise.all([action.exportRow(rows[1]), action.exportRow(rows[2])]);

  assert.deepEqual(calls.export.sort(), ["req-B", "req-C"]);
  assert.deepEqual(calls.download.map(d => d.filename).sort(), ["Requirement-42.xlsx", "Requirement-43.xlsx"]);
});

test("a second click on the same row while in flight is ignored, and loading clears afterwards", async () => {
  const pendingCall = deferred();
  const { action, calls } = harness(() => pendingCall.promise);

  const first = action.exportRow(rows[0]);
  assert.equal(action.isExporting("req-A"), true);
  assert.equal(action.isExporting("req-B"), false);
  assert.equal(await action.exportRow(rows[0]), false);
  assert.deepEqual(calls.export, ["req-A"]);

  pendingCall.resolve(ok());
  assert.equal(await first, true);
  assert.equal(action.isExporting("req-A"), false);
});

test("failure restores the control, shows an actionable error and allows retry", async () => {
  const { action, calls } = harness(async (_id, n) => {
    if (n === 1) throw httpError(500, jsonBlob({ code: 500, message: "Export failed. Reference: ABC123" }));
    return ok();
  });

  assert.equal(await action.exportRow(rows[0]), false);
  assert.equal(action.isExporting("req-A"), false);
  assert.equal(calls.download.length, 0);
  assert.match(calls.error[0], /Couldn&#39;t export the requirement\. Export failed\. Reference: ABC123/);

  assert.equal(await action.exportRow(rows[0]), true);
  assert.equal(calls.download.length, 1);
});

test("binary-encoded error bodies are decoded for 403, and 404/401/network get specific guidance", async () => {
  assert.match(await describeExportError(httpError(403, jsonBlob({ code: 403, message: "You don't have permission to export this requirement's Issues data." }))), /Issues data/);
  assert.match(await describeExportError(httpError(403, jsonBlob({}))), /permission/);
  assert.match(await describeExportError(httpError(404, jsonBlob({ code: 404, message: "Resource not found." }))), /not found, or requirement export isn&#39;t enabled yet/);
  assert.match(await describeExportError(httpError(401, null)), /sign in again/);
  assert.match(await describeExportError(new Error("Network Error")), /could not be reached/);
  assert.match(await describeExportError(httpError(502, new Blob(["<html><body>Bad gateway</body></html>"]))), /HTTP 502/);
});

test("error text is HTML-escaped because notifications render HTML", async () => {
  const message = await describeExportError(httpError(500, jsonBlob({ message: "<img src=x onerror=alert(1)>" })));
  assert.doesNotMatch(message, /<img/);
  assert.match(message, /&lt;img/);
});

test("a 200 that is not a workbook is treated as an error and nothing is downloaded", async () => {
  const jsonAs200 = harness(async () => ({ status: 200, data: jsonBlob({ message: "oops" }), headers: { "content-type": "application/json" } }));
  assert.equal(await jsonAs200.action.exportRow(rows[0]), false);
  assert.equal(jsonAs200.calls.download.length, 0);
  assert.match(jsonAs200.calls.error[0], /unexpected response/);

  const notZip = harness(async () => ({ status: 200, data: new Blob(["not a zip"], { type: XLSX_MIME }), headers: { "content-type": XLSX_MIME } }));
  assert.equal(await notZip.action.exportRow(rows[0]), false);
  assert.equal(notZip.calls.download.length, 0);
});

test("server filename is used when exposed, sanitized, with a requirement-specific fallback", async () => {
  const { action, calls } = harness(async () => ok({ "content-disposition": "attachment; filename=\"Requirement-41.xlsx\"; filename*=UTF-8''Requirement-41%20%C3%A9.xlsx" }));
  await action.exportRow(rows[0]);
  assert.equal(calls.download[0].filename, "Requirement-41 é.xlsx");

  assert.equal(filenameFromContentDisposition("attachment; filename=\"..\\..\\evil.exe\""), "evil.xlsx");
  assert.equal(filenameFromContentDisposition("attachment; filename=report:1?.xlsx"), "report_1_.xlsx");
  assert.equal(filenameFromContentDisposition("attachment"), null);
  assert.equal(filenameFromContentDisposition(undefined), null);
  assert.equal(sanitizeExportFilename("   "), null);
  assert.equal(fallbackExportFilename({ id: "req/9?" }), "Requirement-req_9_.xlsx");
  assert.equal(fallbackExportFilename({ id: "x", requirementNumber: 7 }), "Requirement-7.xlsx");
});

test("rows without an ID do nothing", async () => {
  const { action, calls } = harness(async () => ok());
  assert.equal(await action.exportRow({}), false);
  assert.equal(await action.exportRow(null), false);
  assert.equal(calls.export.length, 0);
});

test("Requirement List keeps existing row actions and adds Export Requirement on every row", () => {
  const page = readFileSync(new URL("../src/modules/requirement/pages/index.vue", import.meta.url), "utf8");
  const exportButton = page.indexOf("data-test=\"export-requirement\"");
  assert.ok(exportButton > 0, "export button present");
  assert.match(page, /@click\.stop="requirementExport\.exportRow\(props\.row\)"/);
  assert.match(page, /:loading="requirementExport\.isExporting\(props\.row\.id\)"/);
  // Not gated on edit rights, and sits beside the existing More Options menu, which still has its items.
  const buttonBlock = page.slice(page.lastIndexOf("<q-btn", exportButton), exportButton);
  assert.doesNotMatch(buttonBlock, /v-if/);
  for (const label of ["Edit", "Convert into Task", "Edit Data Mapping", "Edit Status Tracking", "Delete", "More Options"]) {
    assert.ok(page.includes(label), `existing action "${label}" preserved`);
  }

  const service = readFileSync(new URL("../src/modules/requirement/requirement.service.js", import.meta.url), "utf8");
  assert.match(service, /\/requirement\/\$\{encodeURIComponent\(id\)\}\/export/);
  assert.match(service, /responseType: "blob"/);
});
