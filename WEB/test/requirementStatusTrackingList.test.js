// Unit tests for the requirement-grouped Requirement Status Tracking list helpers.
// Run with:  node --test test/requirementStatusTrackingList.test.js
import { test } from "node:test";
import assert from "node:assert/strict";
import {
  groupRequirementRows,
  buildListPayload,
  htmlToText,
  toApiDate,
  fromApiDate,
  SORTABLE_COLUMNS,
  SORT_OPTIONS
} from "../src/modules/requirement-status-tracking/utils/trackingList.js";

// Shape returned by POST /requirement-status-tracking/list (verified against the dev DB).
const apiGroups = [
  {
    requirementId: "req-168", requirementNumber: 168, requirementTitle: "Add max length 200 for short description",
    isRequirementUnavailable: false, projectId: "p1", projectName: "Meldep 1.4", projectModuleId: "m1", projectModuleName: "PROJECT SUPPORT",
    latestTrackingDate: "2026-11-02", rowCount: 2,
    rows: [
      { id: "r2", trackingDate: "2026-11-02", currentPercent: 40, timeSpentAsOfToday: "07:30", estimatedRemainingTime: "04:00", scope: "<p>More&nbsp;scope &amp; tests</p>", roadblock: "<p>Waiting <b>API</b></p>", estimatedTimeForCompletion: "06:00", createdByName: "Samiksha Narvekar", createdOnUtc: "2026-11-02T10:00:00", updatedByName: "Samiksha Narvekar", updatedOnUtc: "2026-11-02T11:00:00" },
      { id: "r1", trackingDate: "2026-10-30", currentPercent: 20, timeSpentAsOfToday: "05:00", estimatedRemainingTime: "08:00", scope: null, roadblock: null, estimatedTimeForCompletion: "06:00", createdByName: "Samiksha Narvekar", createdOnUtc: "2026-10-30T13:58:41", updatedByName: null, updatedOnUtc: null }
    ]
  },
  {
    requirementId: "req-156", requirementNumber: 156, requirementTitle: "Team Meeting", isRequirementUnavailable: true,
    projectId: "p1", projectName: "Meldep 1.4", projectModuleId: "m1", projectModuleName: "PROJECT SUPPORT",
    latestTrackingDate: "2026-10-30", rowCount: 1,
    rows: [{ id: "r3", trackingDate: "2026-10-30", currentPercent: 5, scope: "<br>" }]
  }
];

test("each requirement gets a header row followed by its tracking records, in server order", () => {
  const rows = groupRequirementRows(apiGroups);
  assert.deepEqual(rows.map(r => (r.isGroup ? `H:${r.requirementNumber}` : r.id)), ["H:168", "r2", "r1", "H:156", "r3"]);
  const header = rows[0];
  assert.equal(header.requirementTitle, "Add max length 200 for short description");
  assert.equal(header.rowCount, 2);
  assert.equal(rows[3].isRequirementUnavailable, true);
});

test("tracking rows carry all requested columns, with MM/DD/YYYY tracking dates", () => {
  const row = groupRequirementRows(apiGroups)[1];
  for (const key of ["trackingDate", "currentPercent", "timeSpentAsOfToday", "estimatedRemainingTime", "scope", "roadblock",
    "estimatedTimeForCompletion", "createdByName", "createdOnUtc", "updatedByName", "updatedOnUtc"]) {
    assert.ok(key in row, key);
  }
  assert.equal(row.trackingDate, "11/02/2026");
  assert.equal(row.recordId, "r2"); // delete targets this single tracking record
  assert.equal(row.requirementTitle, "Add max length 200 for short description");
});

test("row keys are unique across header and record rows", () => {
  const keys = groupRequirementRows(apiGroups).map(r => r.key);
  assert.equal(new Set(keys).size, keys.length);
});

test("rich-text Scope/Roadblock render as plain text", () => {
  assert.equal(htmlToText("<p>More&nbsp;scope &amp; tests</p>"), "More scope & tests");
  assert.equal(htmlToText("<p>Waiting <b>API</b></p>"), "Waiting API");
  assert.equal(htmlToText("<br>"), "");
  assert.equal(htmlToText(null), "");
  assert.equal(htmlToText("&lt;script&gt;x&lt;/script&gt;"), "<script>x</script>"); // shown via text interpolation, never HTML
});

test("date helpers convert between MM/DD/YYYY and yyyy-MM-dd and reject impossible dates", () => {
  assert.equal(toApiDate("11/17/2026"), "2026-11-17");
  assert.equal(toApiDate("3/4/2026"), "2026-03-04");
  assert.equal(toApiDate("02/30/2026"), null);
  assert.equal(toApiDate("2026-11-17"), null);
  assert.equal(fromApiDate("2026-11-17"), "11/17/2026");
  assert.equal(fromApiDate("2026-11-17T10:00:00"), "11/17/2026");
});

test("list payload: date filters in API format, requirement-group sort keys allowed", () => {
  const search = { searchText: "", projectIds: [], projectModuleIds: [], requirementIds: [], fromDate: "10/01/2026", toDate: "11/17/2026" };
  const { payload, error } = buildListPayload(search, { page: 2, rowsPerPage: 20, sortBy: "trackingDate", descending: true },
    [{ column: "requirementTitle", direction: "asc" }, { column: "bogus", direction: "asc" }]);
  assert.equal(error, undefined);
  assert.equal(payload.fromDate, "2026-10-01");
  assert.equal(payload.toDate, "2026-11-17");
  assert.equal(payload.sortBy, "trackingDate");
  assert.deepEqual(payload.sorts, { requirementTitle: "asc" });

  assert.match(buildListPayload({ ...search, fromDate: "12/01/2026" }, { page: 1, rowsPerPage: 20 }, []).error, /cannot be before/);
  assert.match(buildListPayload({ ...search, fromDate: "13/01/2026" }, { page: 1, rowsPerPage: 20 }, []).error, /valid date/);
});

test("sort options match the API's group sort allowlist", () => {
  assert.ok(SORT_OPTIONS.every(o => SORTABLE_COLUMNS.includes(o.field)));
  assert.ok(SORTABLE_COLUMNS.includes("requirementTitle"));
});

// ---------------------------------------------------------------------------------------------
// Requirement-level Delete: all records of the selected Requirement ID, none of other requirements
// ---------------------------------------------------------------------------------------------
import { trackingIdsForRequirement } from "../src/modules/requirement-status-tracking/utils/trackingList.js";

test("requirement delete targets all records of that Requirement ID and nothing else", () => {
  const records = [
    { id: "a1", requirementId: "REQ-1", projectModuleId: "m1" },
    { id: "a2", requirementId: "req-1", projectModuleId: "M1" },
    { id: "a3", requirementId: "req-1", projectModuleId: "m-old" }, // same requirement, earlier module
    { id: "b1", requirementId: "req-2", projectModuleId: "m1" },   // other requirement: untouched
    { id: "a1", requirementId: "req-1", projectModuleId: "m1" }    // duplicate id
  ];
  assert.deepEqual(trackingIdsForRequirement(records, { requirementId: "req-1", projectModuleId: "m1" }), ["a1", "a2", "a3"]);
  assert.deepEqual(trackingIdsForRequirement(null, { requirementId: "req-1" }), []);
  assert.deepEqual(trackingIdsForRequirement(records, { requirementId: "" }), []);
});

// ---------------------------------------------------------------------------------------------
// Search filters: dependent Project Module / Requirement selections and the Tracking Date range
// ---------------------------------------------------------------------------------------------
import { keepAvailable, dateRangeErrors } from "../src/modules/requirement-status-tracking/utils/trackingList.js";

test("dependent filters keep only selections offered for the selected parent(s)", () => {
  const modulesOfSelectedProjects = [{ value: "M1", text: "Login" }, { value: "m2", text: "Billing" }];
  assert.deepEqual(keepAvailable(["m1", "m9", "m2"], modulesOfSelectedProjects), ["m1", "m2"]);
  assert.deepEqual(keepAvailable(["r1"], []), []);
  assert.deepEqual(keepAvailable(null, modulesOfSelectedProjects), []);
});

test("date range: invalid dates and To before From are field errors; equal dates are allowed", () => {
  assert.deepEqual(dateRangeErrors("", ""), { fromDate: "", toDate: "" });
  assert.deepEqual(dateRangeErrors("11/17/2026", "11/17/2026"), { fromDate: "", toDate: "" });
  assert.match(dateRangeErrors("13/01/2026", "").fromDate, /valid date/);
  assert.match(dateRangeErrors("", "02/30/2026").toDate, /valid date/);
  assert.match(dateRangeErrors("11/18/2026", "11/17/2026").toDate, /cannot be before/);
});

test("filter payload sends selected projects, modules, requirements and the date range", () => {
  const search = { searchText: "", projectIds: ["p1"], projectModuleIds: ["m1", "m2"], requirementIds: ["r1"], fromDate: "11/01/2026", toDate: "11/30/2026" };
  const { payload } = buildListPayload(search, { page: 1, rowsPerPage: 20, sortBy: "updatedOnUtc", descending: true }, []);
  assert.deepEqual([payload.projectIds, payload.projectModuleIds, payload.requirementIds], [["p1"], ["m1", "m2"], ["r1"]]);
  assert.deepEqual([payload.fromDate, payload.toDate], ["2026-11-01", "2026-11-30"]);
  const empty = buildListPayload({ ...search, projectIds: [], projectModuleIds: [], requirementIds: [], fromDate: "", toDate: "" }, { page: 1, rowsPerPage: 20 }, []).payload;
  assert.deepEqual([empty.projectIds, empty.projectModuleIds, empty.requirementIds, empty.fromDate, empty.toDate], [null, null, null, null, null]);
});
