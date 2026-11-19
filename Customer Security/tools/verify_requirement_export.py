"""Checks GET /requirement/{id}/export (commit a67690a, 2026-11-18) against the local API on Meldep_Security_Test:
menu tier, site gate, record scope, per-sheet module permissions, other HTTP methods, safe errors, and the activity-log row.
Needs Features:RequirementExport:Enabled=true (appsettings.Development.json sets it).
Usage: python verify_requirement_export.py <accounts.json>"""
import json, subprocess, sys, time, urllib.request, urllib.error

API = "http://localhost:5086"
acc = json.load(open(sys.argv[1], encoding="utf-8"))
A, B = acc["siteA"], acc["siteB"]
OWN_REQ = "004223EC-081B-43C2-B54D-668E6F2BBF6D"     # site A requirement
OTHER_REQ = "7D5A4FD2-5FF2-4C9F-8DA8-0CF538E626F5"   # another site's requirement
REQ_MENU_A = "ABEE41F2-28D8-43E6-8222-DA33E6E62C1B"  # site A's /requirement
EMPTY_ID = acc["users"]["sec.empty"]["id"]
ADMIN_ID = acc["users"]["sec.admin"]["id"]
XLSX = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
# Each sheet needs View on any one of its links (RequirementExportSheetDefinitions).
SHEET_LINKS = {
    "Tasks": ["/project", "/project-tasks", "/my-tasks", "/all-project-planner"],
    "Test Cases": ["/test-case"],
    "Issues": ["/issue"],
    "Timesheets": ["/timesheet", "/billing-processing-employee", "/dashboard"],
}
results = []


def call(method, path, token=None, site=A, body=None):
    r = urllib.request.Request(API + path, method=method)
    r.add_header("Content-Type", "application/json")
    if token: r.add_header("Authorization", "Bearer " + token)
    if site: r.add_header("X-Site-Id", site)
    data = json.dumps(body).encode() if body is not None else None
    try:
        with urllib.request.urlopen(r, data, timeout=180) as resp: status, raw, headers = resp.status, resp.read(), resp.headers
    except urllib.error.HTTPError as e: status, raw, headers = e.code, e.read(), e.headers
    if (headers.get("Content-Type") or "").startswith(XLSX):
        return status, raw, headers
    try: return status, (json.loads(raw) if raw else None), headers
    except ValueError: return status, raw.decode(errors="replace"), headers


def check(name, ok, detail=""):
    results.append(ok)
    print(("PASS  " if ok else "FAIL  ") + name + ("" if ok else f"   [{detail}]"))


def login(u):
    s, b, _ = call("POST", "/auth/login", site=None, body={"username": u, "password": acc["password"]})
    assert s == 200, (u, s, b)
    return b["token"]


def sql(text, out=False):
    p = subprocess.run(["sqlcmd", "-S", "DESKTOP-M3PJFV7", "-E", "-C", "-b", "-h", "-1", "-W", "-d", "Meldep_Security_Test", "-Q",
                        "SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON; " + text], check=True, capture_output=True, text=True)
    return p.stdout.strip() if out else None


def code(r): return r[1].get("code") if isinstance(r[1], dict) else None
def msg(r): return (f"<{len(r[1])} bytes xlsx>" if isinstance(r[1], bytes) else json.dumps(r[1])[:200])
def is_xlsx(r): return r[0] == 200 and isinstance(r[1], bytes) and r[1][:2] == b"PK"
def no_leak(r): return isinstance(r[1], bytes) or ("Exception" not in json.dumps(r[1]) and " at Vsky" not in json.dumps(r[1]))
def export(req=OWN_REQ, token=None, site=A, method="GET"): return call(method, f"/requirement/{req}/export", token, site)


def menus(token):
    _, p, _ = call("GET", "/authorization/my-permissions", token)
    return {m["link"]: m for m in (p or {}).get("menus", []) if m.get("view")}


def wait_for_view(token, link):
    # The API caches permission snapshots for 60 s; grants written by SQL show up once it expires.
    for _ in range(15):
        if link in menus(token):
            return True
        time.sleep(6)
    return False


CLEANUP = "DELETE FROM UserMenuPermissions WHERE Reason='security test'; "
sql(CLEANUP)

try:
    tok = {"admin": login(acc["users"]["sec.admin"]["userName"]), "empty": login(acc["users"]["sec.empty"]["userName"]),
           "adminb": login(acc["users"]["sec.adminb"]["userName"]), "sysadmin": login(acc["realUsers"]["system-super-admin"]["userName"]),
           "employee": login(acc["realUsers"]["employee"]["userName"])}

    # ---- Authentication ----
    r = export(token=None)
    check("no token -> 401", r[0] == 401, f"{r[0]} {msg(r)}")
    r = export(token="not-a-jwt")
    check("garbage token -> 401", r[0] == 401, f"{r[0]} {msg(r)}")

    # ---- Menu tier: [RequireMenu(/requirement)], GET needs View ----
    r = export(token=tok["empty"])
    check("user without Requirement menu -> 403 PERMISSION_DENIED", r[0] == 403 and code(r) == "PERMISSION_DENIED", f"{r[0]} {msg(r)}")

    # ---- Role grant: outcome follows the user's sheet permissions ----
    held = menus(tok["admin"])
    if "/requirement" in held:
        missing = [s for s, links in SHEET_LINKS.items() if not any(l in held for l in links)]
        r = export(token=tok["admin"])
        if missing:
            check(f"admin missing View on {missing} -> 403 naming the sheets",
                  r[0] == 403 and all(m in str(r[1]) for m in missing), f"{r[0]} {msg(r)}")
        else:
            check("admin (role grant, all sheet menus) -> 200 .xlsx", is_xlsx(r), f"{r[0]} {msg(r)}")
            if is_xlsx(r):
                cd = r[2].get("Content-Disposition") or ""
                check("download is named Requirement-<n>.xlsx and not cached",
                      "Requirement-" in cd and ".xlsx" in cd and "no-store" in (r[2].get("Cache-Control") or ""), cd)
        time.sleep(3)  # activity rows are written by a background queue
        row = sql("SELECT TOP 1 ISNULL(UserId,'') + '|' + ISNULL(JSON_VALUE(FullMessage,'$.action'),'') + '|' + ISNULL(JSON_VALUE(FullMessage,'$.status'),'') "
                  "FROM Log WHERE PageUrl LIKE '%/requirement/%/export%' AND JSON_VALUE(FullMessage,'$.decision') IS NULL "
                  "ORDER BY CreatedOnUtc DESC", out=True) or ""
        parts = row.split("|")
        check("activity log row: user, action ExportRequirement, status",
              len(parts) == 3 and parts[0].lower() == ADMIN_ID.lower() and parts[1] == "ExportRequirement" and parts[2] == str(r[0]), row)
    else:
        check("sec.admin holds /requirement View in site A", False, sorted(held))

    # System super admin passes every check (filter bypass + authorizer), so this is the full-workbook path.
    r = export(token=tok["sysadmin"])
    check("system super admin -> 200 .xlsx", is_xlsx(r), f"{r[0]} {msg(r)}")

    # ---- Site gate and cross-site IDs ----
    r = export(token=tok["adminb"], site=A)
    check("site B admin sending site A header -> 403", r[0] == 403, f"{r[0]} {msg(r)}")
    r = export(token=tok["employee"], site=B)
    check("member of A sending site B header -> 403", r[0] == 403, f"{r[0]} {msg(r)}")
    r = export(OTHER_REQ, token=tok["admin"])
    check("another site's requirement ID -> blocked (403 SITE_SCOPE_MISMATCH / 404)",
          r[0] in (403, 404) and not is_xlsx(r), f"{r[0]} {msg(r)}")
    r = export(OTHER_REQ, token=tok["sysadmin"])
    check("system super admin, another site's requirement under site A -> not exported", not is_xlsx(r), f"{r[0]} {msg(r)}")
    r = export("00000000-0000-0000-0000-000000000000", token=tok["admin"])
    check("unknown requirement -> 404, no exception text", r[0] in (403, 404) and no_leak(r), f"{r[0]} {msg(r)}")
    r = export("'%20OR%201=1--", token=tok["admin"])
    check("injection-shaped ID -> 4xx, no exception text", 400 <= r[0] < 500 and no_leak(r), f"{r[0]} {msg(r)}")

    # ---- Other HTTP methods never run the action ----
    for method in ("POST", "PUT", "DELETE", "PATCH"):
        r = export(token=tok["admin"], method=method)
        check(f"{method} /requirement/{{id}}/export -> not 2xx", not 200 <= r[0] < 300, f"{r[0]} {msg(r)}")
        r = export(token=None, method=method)
        check(f"{method} /requirement/{{id}}/export without token -> 401/404/405", r[0] in (401, 404, 405), f"{r[0]} {msg(r)}")

    # ---- Individual-user grant: passes the menu tier, then record scope applies (user is not on the project) ----
    sql(f"INSERT INTO UserMenuPermissions (Id, UserId, SiteId, SiteModuleMenuId, ShowInMenu, ViewAccess, ManageAccess, Reason, Deleted, CreatedById, CreatedOnUtc) "
        f"VALUES (NEWID(), '{EMPTY_ID}', '{A}', '{REQ_MENU_A}', 1, 1, 0, 'security test', 0, '{EMPTY_ID}', SYSUTCDATETIME())")
    if wait_for_view(tok["empty"], "/requirement"):
        r = export(token=tok["empty"])
        check("individual View grant, user not on the project -> 404 RECORD_NOT_FOUND", r[0] == 404 and code(r) == "RECORD_NOT_FOUND", f"{r[0]} {msg(r)}")
        time.sleep(3)
        reason = sql("SELECT TOP 1 1 FROM Log WHERE FullMessage LIKE '%RECORD_OUT_OF_SCOPE%' AND FullMessage LIKE '%ExportRequirement%' "
                     "AND CreatedOnUtc > DATEADD(minute, -5, SYSUTCDATETIME())", out=True)
        check("denial recorded in Log as RECORD_OUT_OF_SCOPE", bool(reason), "no row")
    else:
        check("View grant visible in my-permissions", False, "timed out")
finally:
    sql(CLEANUP)

print(f"\n{sum(results)}/{len(results)} passed")
sys.exit(0 if all(results) else 1)
