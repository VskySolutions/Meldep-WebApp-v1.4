// Runs the web app's real permissionGuard, Pinia permission store and route files against the local API,
// for the Analytics registers and the menus they must not disturb (Dashboard, Requirement, Requirement
// Group, Requirement Data Mapping).
//
//   node --import ./hooks.mjs verify_web_guard.mjs <accounts.json> [Enforce|Audit]
//
// For each user it checks that a direct URL (cold load), a refresh, and in-app navigation give the same
// decision, and that the decision matches what the API returns for the page's own data call, so a page
// can't open and then fail, or be hidden while its API would work.
import fs from "node:fs";
import { execFileSync } from "node:child_process";
import { LocalStorage, newPageLoad, API } from "./stubs.mjs";
import { permissionGuard, CATCH_ALL_ROUTE } from "router/permissionGuard";
import { usePermissionStore } from "stores/permissions";
import analytics, { ATTENDANCE_REGISTER_LINK, TIMESHEET_REGISTER_LINK } from "modules/analytics/routes";
import dashboard from "modules/dashboard/routes";
import requirement from "modules/requirement/routes";
import requirementGroup from "modules/requirement-group/routes";
import dataMapping from "modules/requirement-data-mapping/routes";

const acc = JSON.parse(fs.readFileSync(process.argv[2], "utf8"));
const expectedMode = process.argv[3] || "Enforce";
const A = acc.siteA;
const REQ = "004223EC-081B-43C2-B54D-668E6F2BBF6D";
const routes = [...analytics, ...dashboard, ...requirement, ...requirementGroup, ...dataMapping];
let passed = 0, failed = 0;
const check = (name, ok, detail = "") => {
  ok ? passed++ : failed++;
  console.log((ok ? "PASS  " : "FAIL  ") + name + (ok ? "" : `   [${detail}]`));
};

// ---- Test grants (same as verify_new_coverage.py), removed at the end ----
const ATT_MENU_A = "c0273846-169f-43bb-9078-ff7e719aee3f";
const GROUP_ID = "5ec0a77e-0000-4000-8000-000000000a77";
const HR_ID = acc.users["sec.hr"].id, SUPER_ID = acc.users["sec.superadmin"].id, EMP_ID = acc.realUsers.employee.id;
const sql = text => execFileSync("sqlcmd", ["-S", "DESKTOP-M3PJFV7", "-E", "-C", "-b", "-d", "Meldep_Security_Test", "-Q",
  "SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON; " + text]);
const cleanup = `DELETE FROM UserMenuPermissions WHERE Reason='security test'; DELETE FROM SecurityGroupMenuPermissions WHERE SecurityGroupId='${GROUP_ID}';
  DELETE FROM SecurityGroupMembers WHERE SecurityGroupId='${GROUP_ID}'; DELETE FROM SecurityGroups WHERE Id='${GROUP_ID}'`;
const userGrant = (id, view) => `INSERT INTO UserMenuPermissions (Id, UserId, SiteId, SiteModuleMenuId, ShowInMenu, ViewAccess, ManageAccess, Reason, Deleted, CreatedById, CreatedOnUtc)
  VALUES (NEWID(), '${id}', '${A}', '${ATT_MENU_A}', 1, ${view}, 0, 'security test', 0, '${id}', SYSUTCDATETIME());`;

// ---- Minimal Vue Router matching: enough for these nested records ----
function resolve (fullPath) {
  for (const parent of routes) {
    for (const child of parent.children || []) {
      const full = child.path.startsWith("/") ? child.path : child.path ? `${parent.path}/${child.path}` : parent.path;
      if (full !== fullPath) continue;
      if (child.redirect) return resolve(child.redirect);
      // Vue Router normalizes every matched record so meta is always an object.
      const matched = [parent, child].map(record => ({ ...record, meta: record.meta || {} }));
      return { path: full, name: child.name, query: {}, hash: "", fullPath: full, matched };
    }
  }
  throw new Error("no route " + fullPath);
}

async function navigate (fullPath) {
  const to = resolve(fullPath);
  let result;
  await permissionGuard(to, value => { result = value; }, null);
  if (result === undefined) return { allowed: true, path: to.path };
  if (result === false) return { allowed: false, reason: "aborted" };
  if (result.name === CATCH_ALL_ROUTE) return { allowed: false, reason: "access-denied page", keepsUrl: result.params.catchAll.join("/") === to.path.slice(1) };
  return { allowed: false, reason: result.path || result.name, redirect: result };
}

async function api (method, url, token, body) {
  const headers = { "Content-Type": "application/json", "X-Site-Id": A };
  if (token) headers.Authorization = `Bearer ${token}`;
  const response = await fetch(API + url, { method, headers, body: body ? JSON.stringify(body) : undefined });
  return response.status;
}

const range = { fromDate: "2026-10-01", toDate: "2026-10-07" };
// The page's own data call; a 403 there means the page is unusable.
const pages = {
  [TIMESHEET_REGISTER_LINK]: t => api("POST", "/Analytics/timesheet-register", t, range),
  [ATTENDANCE_REGISTER_LINK]: t => api("POST", "/Analytics/attendance-register", t, range),
  "/requirement": t => api("POST", "/requirement/list", t, { pageNumber: 1, pageSize: 5 }),
  "/requirement-group": t => api("POST", "/requirement-group/list", t, { pageNumber: 1, pageSize: 5 }),
  "/requirement-data-mapping": t => api("GET", `/requirement-data-mappings/get-data-mappings/${REQ}`, t),
  "/dashboard": null
};

// What the attendance page must do for each user in Enforce mode, from the grants above.
const attendanceExpected = { "system-super-admin": true, "sec.hr": true, employee: true, "sec.admin": false, "sec.superadmin": false, "sec.empty": false };

async function login (userName) {
  const response = await fetch(API + "/auth/login", { method: "POST", headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username: userName, password: acc.password }) });
  const body = await response.json();
  if (!response.ok) throw new Error(`login ${userName}: ${response.status}`);
  return body.token;
}

sql(cleanup);
sql(userGrant(HR_ID, 1) + userGrant(SUPER_ID, 2) +
  `INSERT INTO SecurityGroups (Id, SiteId, Name, Description, Active, Deleted, CreatedById, CreatedOnUtc) VALUES ('${GROUP_ID}', '${A}', 'sec-attendance-viewers', 'security test', 1, 0, '${EMP_ID}', SYSUTCDATETIME());
   INSERT INTO SecurityGroupMembers (Id, SecurityGroupId, UserId, SiteId, Deleted, CreatedById, CreatedOnUtc) VALUES (NEWID(), '${GROUP_ID}', '${EMP_ID}', '${A}', 0, '${EMP_ID}', SYSUTCDATETIME());
   INSERT INTO SecurityGroupMenuPermissions (Id, SecurityGroupId, SiteId, SiteModuleMenuId, ShowInMenu, ViewAccess, ManageAccess, Deleted, CreatedById, CreatedOnUtc) VALUES (NEWID(), '${GROUP_ID}', '${A}', '${ATT_MENU_A}', 1, 1, 0, 0, '${EMP_ID}', SYSUTCDATETIME());`);

// Grants written straight to SQL skip the cache reset the admin screens trigger, so wait out the API's
// 60-second permission cache until the API reports all three test grants.
async function waitForGrants () {
  const attendanceView = async userName => {
    const response = await fetch(API + "/authorization/my-permissions",
      { headers: { Authorization: `Bearer ${await login(userName)}`, "X-Site-Id": A } });
    const body = await response.json();
    return !!(body.menus || []).find(menu => menu.link === ATTENDANCE_REGISTER_LINK)?.view;
  };
  const deadline = Date.now() + 75000;
  while (Date.now() < deadline) {
    if (await attendanceView(acc.realUsers.employee.userName) && await attendanceView(acc.users["sec.hr"].userName)
        && !(await attendanceView(acc.users["sec.superadmin"].userName))) return;
    await new Promise(resolve => setTimeout(resolve, 3000));
  }
}

try {
  if (expectedMode === "Enforce") await waitForGrants();
  const people = {
    "system-super-admin": acc.realUsers["system-super-admin"].userName, employee: acc.realUsers.employee.userName,
    "site-super-admin": acc.realUsers["site-super-admin"].userName, admin: acc.realUsers.admin.userName,
    "sec.admin": acc.users["sec.admin"].userName, "sec.superadmin": acc.users["sec.superadmin"].userName,
    "sec.hr": acc.users["sec.hr"].userName, "sec.empty": acc.users["sec.empty"].userName
  };

  for (const [who, userName] of Object.entries(people)) {
    const token = await login(userName);
    LocalStorage.clear();
    LocalStorage.set("token", token);
    LocalStorage.set("user", { userId: who, siteId: A, roles: [] });   // roles are no longer read by the Analytics routes

    newPageLoad();
    const store = usePermissionStore();
    await store.load();
    check(`${who}: permission store loaded in ${expectedMode} mode`, store.mode === expectedMode, store.mode);

    for (const [link, apiCall] of Object.entries(pages)) {
      newPageLoad();
      const direct = await navigate(link);            // URL typed or bookmarked: cold store
      newPageLoad();
      const refreshed = await navigate(link);         // F5 on the page: cold store again
      newPageLoad();
      await navigate("/dashboard");                   // in-app: store already loaded by an earlier page
      const inApp = await navigate(link);

      check(`${who} ${link}: direct URL, refresh and in-app agree (${direct.allowed ? "allowed" : "denied"})`,
        direct.allowed === refreshed.allowed && direct.allowed === inApp.allowed, JSON.stringify([direct, refreshed, inApp]));
      if (!direct.allowed)
        check(`${who} ${link}: denied shows the access-denied page and keeps the URL`, direct.keepsUrl === true, JSON.stringify(direct));

      if (apiCall) {
        const status = await apiCall(token);
        check(`${who} ${link}: page decision matches the API (${status})`, direct.allowed === (status !== 403), `page ${direct.allowed}, api ${status}`);
      }

      if (link === ATTENDANCE_REGISTER_LINK && who in attendanceExpected && expectedMode === "Enforce")
        check(`${who} /analytics/attendance-register: ${attendanceExpected[who] ? "allowed" : "denied"} as granted`, direct.allowed === attendanceExpected[who], JSON.stringify(direct));
      if (expectedMode === "Audit")
        check(`${who} ${link}: Audit mode never blocks a page`, direct.allowed, JSON.stringify(direct));
    }

    newPageLoad();
    const root = await navigate("/analytics");
    check(`${who} /analytics redirects to the Timesheet Register and is checked as it`,
      root.allowed === (await navigate(TIMESHEET_REGISTER_LINK)).allowed, JSON.stringify(root));
  }

  // Signed out: a direct URL or refresh goes to login and remembers the page.
  LocalStorage.clear();
  newPageLoad();
  const signedOut = await navigate(ATTENDANCE_REGISTER_LINK);
  check("signed out: /analytics/attendance-register -> login with redirect back",
    signedOut.redirect?.path === "/auth/login" && signedOut.redirect?.query?.redirect === ATTENDANCE_REGISTER_LINK, JSON.stringify(signedOut));

  // An expired or revoked token: the API answers 401, so the guard stops instead of showing the page.
  LocalStorage.set("token", "not-a-valid-token");
  LocalStorage.set("user", { userId: "x", siteId: A });
  newPageLoad();
  const badToken = await navigate(ATTENDANCE_REGISTER_LINK);
  check("invalid token: /analytics/attendance-register is not shown", badToken.allowed === false, JSON.stringify(badToken));
} finally {
  sql(cleanup);
}

console.log(`\n${passed} passed, ${failed} failed`);
process.exit(failed ? 1 : 0);
