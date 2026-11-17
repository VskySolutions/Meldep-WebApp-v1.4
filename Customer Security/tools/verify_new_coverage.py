"""Checks the newly covered endpoints and the Swagger site-header fix against the local API on Meldep_Security_Test."""
import json, sys, urllib.request, urllib.error

API = "http://localhost:5086"
acc = json.load(open(sys.argv[1], encoding="utf-8"))
A, B = acc["siteA"], acc["siteB"]
OTHER = {"site": "01cfa10a-e167-4965-a90b-c8ac2f782c5b", "req": "7D5A4FD2-5FF2-4C9F-8DA8-0CF538E626F5",
         "task": "1ce331b7-c4fb-472d-947c-3210a7f33854", "mod": "c1dab2a7-d95a-4edd-81fe-4e73337eea0d"}
OWN = {"mod": "E790E537-D5D3-4E3C-8DF2-0B6CFFFCB486", "req": "004223EC-081B-43C2-B54D-668E6F2BBF6D",
       "task": "034CDB08-1DBE-46DC-9EA5-88654A7A51BC", "emp": "0781d5d0-bdcb-4f29-a08a-da8b89846142"}
results = []


def call(method, path, token=None, site=A, body=None):
    r = urllib.request.Request(API + path, method=method)
    r.add_header("Content-Type", "application/json")
    if token: r.add_header("Authorization", "Bearer " + token)
    if site: r.add_header("X-Site-Id", site)
    data = json.dumps(body).encode() if body is not None else None
    try:
        with urllib.request.urlopen(r, data, timeout=180) as resp: status, raw = resp.status, resp.read()
    except urllib.error.HTTPError as e: status, raw = e.code, e.read()
    try: return status, (json.loads(raw) if raw else None)
    except ValueError: return status, raw.decode(errors="replace")


def check(name, ok, detail=""):
    results.append(ok)
    print(("PASS  " if ok else "FAIL  ") + name + ("" if ok else f"   [{detail}]"))


def login(u):
    s, b = call("POST", "/auth/login", site=None, body={"username": u, "password": acc["password"]})
    assert s == 200, (u, s, b)
    return b["token"]


def code(r): return r[1].get("code") if isinstance(r[1], dict) else None
def msg(r): return json.dumps(r[1])[:200]


users = {k: v["userName"] for k, v in acc["users"].items()}
real = {k: v["userName"] for k, v in acc["realUsers"].items()}
tok = {"employee": login(real["employee"]), "admin": login(users["sec.admin"]), "empty": login(users["sec.empty"]),
       "adminb": login(users["sec.adminb"]), "sysadmin": login(real["system-super-admin"])}

# ---- Swagger root cause: same token, with and without the site header ----
s, perms = call("GET", "/authorization/my-permissions", tok["employee"])
has_project = any(m.get("link") == "/project" and (m.get("view") or m.get("show")) for m in (perms or {}).get("menus", []))
check("employee holds Project menu access in site A", has_project, msg((s, perms)))
body = {"pageNumber": 1, "pageSize": 10}
app_like = call("POST", "/projects/list", tok["employee"], A, body)
check("Project list WITH X-Site-Id (as the web app sends) -> 200", app_like[0] == 200, f"{app_like[0]} {msg(app_like)}")
swagger_old = call("POST", "/projects/list", tok["employee"], None, body)
check("Project list WITHOUT X-Site-Id (token only) -> same result as the app", swagger_old[0] == app_like[0] and swagger_old[1] == app_like[1], msg(swagger_old))
other = call("POST", "/projects/list", tok["employee"], B, body)
check("Project list with a site the user isn't in -> 403, original message", other[0] == 403 and other[1].get("message") == "You do not have access to this site.", msg(other))
nomenu = call("POST", "/projects/list", tok["empty"], A, body)
check("user without Project menu, with site header -> 403 PERMISSION_DENIED", nomenu[0] == 403 and code(nomenu) == "PERMISSION_DENIED", msg(nomenu))
s, sw = call("GET", "/swagger/v1/swagger.json", site=None)
schemes = (sw or {}).get("components", {}).get("securitySchemes", {}) if isinstance(sw, dict) else {}
check("swagger.json declares X-Site-Id header scheme", schemes.get("SiteId", {}).get("name") == "X-Site-Id", str(schemes.keys()))
req = (sw or {}).get("security", [{}])[0] if isinstance(sw, dict) else {}
check("swagger.json requires Bearer + SiteId + SiteTimezone together", {"Bearer", "SiteId", "SiteTimezone"} <= set(req), str(req))
s, sites = call("GET", "/authorization/my-sites", tok["employee"], None)
check("my-sites works without a site header (to find the ID for Swagger)", s == 200 and any(x["siteId"] == A for x in sites), f"{s} {sites}")

# ---- Analytics (new controller) ----
rng = {"fromDate": "2026-10-01", "toDate": "2026-10-07"}
r = call("POST", "/Analytics/timesheet-register", tok["admin"], A, rng)
check("Analytics register: site admin -> 200", r[0] == 200, f"{r[0]} {msg(r)}")
def find_emp(o):
    if isinstance(o, dict):
        if o.get("employeeId"): return o["employeeId"]
        o = list(o.values())
    if isinstance(o, list):
        for x in o:
            e = find_emp(x)
            if e: return e
OWN["emp"] = find_emp(r[1]) or OWN["emp"]
r = call("POST", "/Analytics/timesheet-register", tok["sysadmin"], A, rng)
check("Analytics register: system super admin -> 200", r[0] == 200, f"{r[0]} {msg(r)}")
r = call("POST", "/Analytics/timesheet-register", tok["employee"], A, rng)
check("Analytics register: employee -> 403 PERMISSION_DENIED", r[0] == 403 and code(r) == "PERMISSION_DENIED", msg(r))
r = call("POST", "/Analytics/timesheet-register", tok["adminb"], A, rng)
check("Analytics register: admin of site B using site A header -> 403", r[0] == 403, msg(r))
r = call("POST", "/Analytics/timesheet-register", None, A, rng)
check("Analytics register: no token -> 401", r[0] == 401, msg(r))
r = call("GET", f"/Analytics/timesheet-register/day?employeeId={OWN['emp']}&date=2026-10-01", tok["admin"])
check("Analytics day: site admin -> 200", r[0] == 200, f"{r[0]} {msg(r)}")
r = call("GET", f"/Analytics/timesheet-register/day?employeeId={OWN['emp']}&date=2026-10-01", tok["employee"])
check("Analytics day: employee -> 403", r[0] == 403, msg(r))
r = call("GET", f"/Analytics/timesheet-register/employee?employeeId={OWN['emp']}&fromDate=2026-10-01&toDate=2026-10-31", tok["admin"])
check("Analytics employee: site admin -> 200", r[0] == 200, f"{r[0]} {msg(r)}")
r = call("GET", f"/Analytics/timesheet-register/employee?employeeId={OWN['emp']}&fromDate=2026-10-01&toDate=2026-10-31", tok["empty"])
check("Analytics employee: user with no role rights -> 403", r[0] == 403, msg(r))
r = call("POST", "/Analytics/timesheet-register", tok["admin"], A, {"fromDate": "2020-01-01", "toDate": "2026-01-01"})
check("Analytics: deliberate validation message still reaches the user", r[0] == 400 and "narrow the date range" in str(r[1]), msg(r))
r = call("GET", "/Analytics/timesheet-register/employee?employeeId=nope&fromDate=2026-10-01&toDate=2026-10-02", tok["admin"])
check("Analytics: unknown employee -> 400 user message", r[0] == 400 and "not found" in str(r[1]), msg(r))

# ---- Other new endpoints ----
r = call("GET", "/sites/check-mobile-version?currentVersion=1.0.0", tok["employee"])
check("check-mobile-version: site member -> 200", r[0] == 200, f"{r[0]} {msg(r)}")
r = call("GET", "/sites/check-mobile-version?currentVersion=1.0.0", None)
check("check-mobile-version: no token -> 401", r[0] == 401, msg(r))
r = call("GET", "/sites/check-mobile-version?currentVersion=1.0.0", tok["employee"], B)
check("check-mobile-version: not a member of the site -> 403", r[0] == 403, msg(r))
r = call("GET", f"/requirement/getRequirementsByProjectModuleId/{OWN['mod']}", tok["admin"])
check("getRequirementsByProjectModuleId own module -> 200", r[0] == 200, f"{r[0]} {msg(r)}")
r = call("GET", f"/requirement/getRequirementsByProjectModuleId/{OTHER['mod']}", tok["admin"])
check("getRequirementsByProjectModuleId other site's module -> 403", r[0] == 403, f"{r[0]} {msg(r)}")
r = call("GET", f"/requirement/getRequirementsByProjectModuleId/{OWN['mod']}", tok["empty"])
check("getRequirementsByProjectModuleId without Requirement menu -> 403", r[0] == 403, msg(r))
r = call("GET", f"/requirement/data-mapping-labels/{OWN['req']}", tok["admin"])
check("data-mapping-labels own requirement -> 200", r[0] == 200, f"{r[0]} {msg(r)}")
r = call("GET", f"/requirement/data-mapping-labels/{OTHER['req']}", tok["admin"])
check("data-mapping-labels other site's requirement -> 403", r[0] == 403, f"{r[0]} {msg(r)}")
r = call("GET", f"/requirement-status-tracking/get-requirement-time-spent?ids={OWN['req']}&trackingDate=2026-10-01", tok["admin"])
check("get-requirement-time-spent: admin -> 200", r[0] == 200, f"{r[0]} {msg(r)}")
r = call("GET", f"/requirement-status-tracking/get-requirement-time-spent?ids={OWN['req']}&trackingDate=2026-10-01", tok["empty"])
check("get-requirement-time-spent: no menu -> 403", r[0] == 403, msg(r))

# copyModuleToTask: empty ProjectIds, so a passing request writes nothing.
copy = lambda req, tasks, mod=OWN["mod"]: {"projectModuleId": mod, "projectIds": [], "selectedTaskList": True,
                                           "requirements": [{"requirementId": req, "taskIds": tasks}]}
r = call("POST", "/project-tasks/copyModuleToTask", tok["admin"], A, copy(OWN["req"], [OWN["task"]]))
check("copyModuleToTask with own-site nested IDs -> 204 (no writes)", r[0] == 204, f"{r[0]} {msg(r)}")
r = call("POST", "/project-tasks/copyModuleToTask", tok["admin"], A, copy(OWN["req"], [OTHER["task"]]))
check("copyModuleToTask nested TaskIds from another site -> 403 SITE_SCOPE_MISMATCH", r[0] == 403 and code(r) == "SITE_SCOPE_MISMATCH", msg(r))
r = call("POST", "/project-tasks/copyModuleToTask", tok["admin"], A, copy(OTHER["req"], []))
check("copyModuleToTask nested RequirementId from another site -> 403", r[0] == 403 and code(r) == "SITE_SCOPE_MISMATCH", msg(r))
r = call("POST", "/project-tasks/copyModuleToTask", tok["admin"], A, copy(OWN["req"], [], OTHER["mod"]))
check("copyModuleToTask top-level module from another site -> 403", r[0] == 403, msg(r))
r = call("POST", "/project-tasks/copyModuleToTask", tok["empty"], A, copy(OWN["req"], []))
check("copyModuleToTask without project menu -> 403 PERMISSION_DENIED", r[0] == 403 and code(r) == "PERMISSION_DENIED", msg(r))

# ---- Every verb on a menu-protected controller, for a user without the menu ----
for method, path, b in [("GET", f"/requirement/details/{OWN['req']}", None), ("POST", "/requirement", {}),
                        ("PUT", f"/requirement/{OWN['req']}", {}), ("DELETE", f"/requirement/{OWN['req']}", None),
                        ("PATCH", f"/requirement/{OWN['req']}", {})]:
    r = call(method, path, tok["empty"], A, b)
    expect = (403, 404, 405) if method == "PATCH" else (403,)
    check(f"{method} {path.split('/')[1]} without permission -> {expect}", r[0] in expect, f"{r[0]} {msg(r)}")
    r = call(method, path, None, A, b)
    check(f"{method} {path.split('/')[1]} without token -> 401/405", r[0] in (401, 405), f"{r[0]} {msg(r)}")

# ---- Exceptions still return a reference id, never exception text ----
r = call("GET", "/sites/check-mobile-version?currentVersion=1", tok["employee"], A)
check("check-mobile-version body has no stack/exception text", "Exception" not in json.dumps(r[1]), msg(r))

# ---- Attendance Register (/analytics/attendance-register): its own menu, independent of Timesheet Register ----
# Test copy grants (site A): Timesheet Register View -> admin, site-super-admin, system-super-admin.
# Attendance Register View -> site-super-admin, system-super-admin only (not admin).
# For these checks only: sec.hr gets an individual Attendance View grant, sec.employee gets it through a
# security group, and sec.superadmin gets an individual Attendance deny. All are removed afterwards.
import subprocess
B_EMP = "17e3a181-8ca9-483f-acfa-c61e5525b6cf"   # an employee of site B
ATT_MENU_A = "c0273846-169f-43bb-9078-ff7e719aee3f"   # site A's /analytics/attendance-register
TS_MENU_A = "68397ddb-4cc2-4a82-8c9a-c969f4e761bc"    # site A's /analytics/timesheet-register
HR_ID = acc["users"]["sec.hr"]["id"]
SUPER_ID = acc["users"]["sec.superadmin"]["id"]
EMP_ID = acc["realUsers"]["employee"]["id"]
GROUP_ID = "5ec0a77e-0000-4000-8000-000000000a77"


def test_sql(text):
    # -b makes sqlcmd fail on SQL errors; the filtered index on this table needs QUOTED_IDENTIFIER.
    subprocess.run(["sqlcmd", "-S", "DESKTOP-M3PJFV7", "-E", "-C", "-b", "-d", "Meldep_Security_Test", "-Q",
                    "SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON; " + text], check=True, capture_output=True)


CLEANUP = (f"DELETE FROM UserMenuPermissions WHERE Reason='security test'; "
           f"DELETE FROM SecurityGroupMenuPermissions WHERE SecurityGroupId='{GROUP_ID}'; "
           f"DELETE FROM SecurityGroupMembers WHERE SecurityGroupId='{GROUP_ID}'; "
           f"DELETE FROM SecurityGroups WHERE Id='{GROUP_ID}'")


def user_grant(user_id, menu, view, manage=0):
    return (f"INSERT INTO UserMenuPermissions (Id, UserId, SiteId, SiteModuleMenuId, ShowInMenu, ViewAccess, ManageAccess, Reason, Deleted, CreatedById, CreatedOnUtc) "
            f"VALUES (NEWID(), '{user_id}', '{A}', '{menu}', 1, {view}, {manage}, 'security test', 0, '{user_id}', SYSUTCDATETIME()); ")


test_sql(CLEANUP)
test_sql(user_grant(HR_ID, ATT_MENU_A, 1) + user_grant(SUPER_ID, ATT_MENU_A, 2) +
         f"INSERT INTO SecurityGroups (Id, SiteId, Name, Description, Active, Deleted, CreatedById, CreatedOnUtc) "
         f"VALUES ('{GROUP_ID}', '{A}', 'sec-attendance-viewers', 'security test', 1, 0, '{EMP_ID}', SYSUTCDATETIME()); "
         f"INSERT INTO SecurityGroupMembers (Id, SecurityGroupId, UserId, SiteId, Deleted, CreatedById, CreatedOnUtc) "
         f"VALUES (NEWID(), '{GROUP_ID}', '{EMP_ID}', '{A}', 0, '{EMP_ID}', SYSUTCDATETIME()); "
         f"INSERT INTO SecurityGroupMenuPermissions (Id, SecurityGroupId, SiteId, SiteModuleMenuId, ShowInMenu, ViewAccess, ManageAccess, Deleted, CreatedById, CreatedOnUtc) "
         f"VALUES (NEWID(), '{GROUP_ID}', '{A}', '{ATT_MENU_A}', 1, 1, 0, 0, '{EMP_ID}', SYSUTCDATETIME())")
try:
    tok["hr"] = login(users["sec.hr"])
    tok["superadmin"] = login(users["sec.superadmin"])
    tok["employee"] = login(real["employee"])   # fresh token after the group change
    att = {"fromDate": "2026-10-01", "toDate": "2026-10-07"}
    day = lambda emp: f"/Analytics/attendance-register/day?employeeId={emp}&date=2026-10-01"
    per = lambda emp: f"/Analytics/attendance-register/employee?employeeId={emp}&fromDate=2026-10-01&toDate=2026-10-31"
    ts_post = lambda t: call("POST", "/Analytics/timesheet-register", t, A, rng)

    # The permissions the web app's Pinia store loads, per user.
    def menu(t, link):
        s, p = call("GET", "/authorization/my-permissions", t)
        return next((m for m in (p or {}).get("menus", []) if m.get("link") == link), {})

    # Grants written straight to SQL skip the cache reset the admin screens trigger, so wait out the
    # API's 60-second permission cache until all three test grants are visible.
    import time
    deadline = time.time() + 75
    while time.time() < deadline and not (menu(tok["employee"], "/analytics/attendance-register").get("view")
                                          and menu(tok["hr"], "/analytics/attendance-register").get("view")
                                          and not menu(tok["superadmin"], "/analytics/attendance-register").get("view")):
        time.sleep(3)
    expect_view = {"sysadmin": (True, True), "superadmin": (True, False), "admin": (True, False),
                   "hr": (False, True), "employee": (False, True), "empty": (False, False)}
    for who, (ts_view, att_view) in expect_view.items():
        got = (bool(menu(tok[who], "/analytics/timesheet-register").get("view")), bool(menu(tok[who], "/analytics/attendance-register").get("view")))
        check(f"my-permissions {who}: Timesheet/Attendance view = {ts_view}/{att_view}", got == (ts_view, att_view), str(got))

    allowed = {"sysadmin": "role grant", "hr": "individual user grant", "employee": "security group grant"}
    for who, how in allowed.items():
        r = call("POST", "/Analytics/attendance-register", tok[who], A, att)
        check(f"Attendance POST search (View, [ReadAccess]) via {how}: {who} -> 200", r[0] == 200, f"{r[0]} {msg(r)}")
    att_emp = find_emp(call("POST", "/Analytics/attendance-register", tok["sysadmin"], A, att)[1]) or OWN["emp"]
    for who, how in allowed.items():
        for path in (day(att_emp), per(att_emp)):
            r = call("GET", path, tok[who])
            check(f"Attendance GET {path.split('?')[0]} via {how}: {who} -> 200", r[0] == 200, f"{r[0]} {msg(r)}")

    # Independence: a Timesheet grant doesn't open Attendance, and an Attendance grant doesn't open Timesheet.
    denied = {"admin": "Timesheet grant only", "superadmin": "role grant overridden by an individual deny",
              "empty": "no grants"}
    for who, why in denied.items():
        for method, path, b in (("POST", "/Analytics/attendance-register", att), ("GET", day(att_emp), None), ("GET", per(att_emp), None)):
            r = call(method, path, tok[who], A, b)
            check(f"Attendance {method} {path.split('?')[0]}: {who} ({why}) -> 403 PERMISSION_DENIED",
                  r[0] == 403 and code(r) == "PERMISSION_DENIED", f"{r[0]} {msg(r)}")
    for who in ("admin", "superadmin", "sysadmin"):
        r = ts_post(tok[who])
        check(f"Timesheet register still works for {who} (Timesheet grant)", r[0] == 200, f"{r[0]} {msg(r)}")
    for who in ("hr", "employee"):
        r = ts_post(tok[who])
        check(f"Timesheet register: {who} with only an Attendance grant -> 403 PERMISSION_DENIED", r[0] == 403 and code(r) == "PERMISSION_DENIED", f"{r[0]} {msg(r)}")

    r = call("POST", "/Analytics/attendance-register", tok["adminb"], A, att)
    check("Attendance: admin of site B with site A header -> 403 SITE_NOT_MEMBER", r[0] == 403 and code(r) == "SITE_NOT_MEMBER", msg(r))
    r = call("POST", "/Analytics/attendance-register", tok["adminb"], B, att)
    check("Attendance: site B admin in site B, no Attendance grant there -> 403", r[0] == 403, msg(r))
    for method, path, b in (("POST", "/Analytics/attendance-register", att), ("GET", day(att_emp), None), ("GET", per(att_emp), None)):
        r = call(method, path, None, A, b)
        check(f"Attendance {method} {path.split('?')[0]}: no token -> 401", r[0] == 401, f"{r[0]} {msg(r)}")

    r = call("GET", day(B_EMP), tok["employee"])
    check("Attendance day: employee of another site -> 403 SITE_SCOPE_MISMATCH", r[0] == 403 and code(r) == "SITE_SCOPE_MISMATCH", msg(r))
    # A System Super Admin may name another site's records (logged as a bypass); the register still only
    # reads the active site, so nothing from site B comes back.
    r = call("GET", day(B_EMP), tok["sysadmin"])
    check("Attendance day: system admin naming another site's employee -> no data (400 not found)", r[0] == 400 and "not found" in str(r[1]), msg(r))
    r = call("GET", per(B_EMP), tok["hr"])
    check("Attendance employee: employee of another site -> 403 SITE_SCOPE_MISMATCH", r[0] == 403 and code(r) == "SITE_SCOPE_MISMATCH", msg(r))
    r = call("POST", "/Analytics/attendance-register", tok["hr"], A, {**att, "employeeIds": [B_EMP]})
    check("Attendance search: employeeIds from another site -> 403 SITE_SCOPE_MISMATCH", r[0] == 403 and code(r) == "SITE_SCOPE_MISMATCH", msg(r))

    r = call("POST", "/Analytics/attendance-register", tok["hr"], A, {"fromDate": "2020-01-01", "toDate": "2026-01-01"})
    check("Attendance: deliberate range message reaches the user", r[0] == 400 and "narrow the date range" in str(r[1]), msg(r))
    r = call("GET", per("nope"), tok["hr"])
    check("Attendance: unknown employee -> 400 user message", r[0] == 400 and "not found" in str(r[1]), msg(r))
    r = call("POST", "/Analytics/attendance-register", tok["hr"], A, {"fromDate": "x"})
    check("Attendance: bad dates -> 400 validation message", r[0] == 400 and "required" in str(r[1]), msg(r))

    for method in ("PUT", "DELETE", "PATCH"):
        for path in ("/Analytics/attendance-register", "/Analytics/attendance-register/day", "/Analytics/attendance-register/employee"):
            r = call(method, path, tok["sysadmin"], A, att if method != "DELETE" else None)
            check(f"{method} {path}: no such action -> 404/405", r[0] in (404, 405), f"{r[0]} {msg(r)}")
    r = call("GET", "/Analytics/attendance-register", tok["sysadmin"])
    check("GET /Analytics/attendance-register (search is POST only) -> 404/405", r[0] in (404, 405), f"{r[0]} {msg(r)}")

    for r in (call("POST", "/Analytics/attendance-register", tok["hr"], A, att), call("GET", per("nope"), tok["hr"])):
        check("Attendance responses carry no exception or stack text", "Exception" not in json.dumps(r[1]) and " at Vsky." not in json.dumps(r[1]), msg(r))

finally:
    test_sql(CLEANUP)

print(f"\n{sum(results)} passed, {len(results) - sum(results)} failed")
