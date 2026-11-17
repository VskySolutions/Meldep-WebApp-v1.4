"""Checks the Requirement Status Tracking endpoints changed on 2026-11-17 (commit 6d0d687) against the local API
on Meldep_Security_Test: the two new GETs and the reworked bulk-add / bulk-save (RequirementId moved to the record).
Usage: python verify_status_tracking.py <accounts.json>"""
import json, subprocess, sys, time, urllib.request, urllib.error

API = "http://localhost:5086"
acc = json.load(open(sys.argv[1], encoding="utf-8"))
A, B = acc["siteA"], acc["siteB"]
OWN = {"req": "004223EC-081B-43C2-B54D-668E6F2BBF6D", "proj": "9F5D0649-EFA6-4E64-A02B-88DE43804CD2", "mod": "E790E537-D5D3-4E3C-8DF2-0B6CFFFCB486"}
OTHER = {"req": "7D5A4FD2-5FF2-4C9F-8DA8-0CF538E626F5", "proj": "fc55177f-5ffe-4bb6-9c8a-2b51008581d9", "mod": "c1dab2a7-d95a-4edd-81fe-4e73337eea0d"}
REQ_MENU_A = "ABEE41F2-28D8-43E6-8222-DA33E6E62C1B"  # site A's /requirement
EMPTY_ID = acc["users"]["sec.empty"]["id"]
ADMIN_ID = acc["users"]["sec.admin"]["id"]
BASE = "/requirement-status-tracking"
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


def sql(text, out=False):
    p = subprocess.run(["sqlcmd", "-S", "DESKTOP-M3PJFV7", "-E", "-C", "-b", "-h", "-1", "-W", "-d", "Meldep_Security_Test", "-Q",
                        "SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON; " + text], check=True, capture_output=True, text=True)
    return p.stdout.strip() if out else None


def code(r): return r[1].get("code") if isinstance(r[1], dict) else None
def msg(r): return json.dumps(r[1])[:200]
def blocked(r): return (r[0], code(r)) in ((403, "SITE_SCOPE_MISMATCH"), (404, "RECORD_NOT_FOUND"))
def avail(p, m): return f"{BASE}/available-requirements?projectId={p}&projectModuleId={m}"
def by_req(req): return f"{BASE}/get-requirement-status-tracking-by-requirementId?requirementId={req}"
def save_body(req, proj=OWN["proj"], mod=OWN["mod"]):
    return {"projectId": proj, "projectModuleId": mod, "requirementId": req,
            "rows": [{"trackingDate": "2026-11-17T00:00:00", "currentPercent": 10, "scope": "security test", "roadblock": "security test"}]}
def tracking_rows(req): return sql(f"SELECT COUNT(*) FROM Requirement_Status_Tracking WHERE RequirementId='{req}' AND Scope='security test'", out=True)


def wait_for_menu(token):
    for _ in range(14):
        s, p = call("GET", "/authorization/my-permissions", token)
        m = next((m for m in (p or {}).get("menus", []) if m.get("link") == "/requirement"), None)
        if m and m.get("view"):
            return True
        time.sleep(6)
    return False


CLEANUP = ("DELETE FROM UserMenuPermissions WHERE Reason='security test'; "
           "DELETE FROM Requirement_Status_Tracking WHERE Scope='security test'; ")
sql(CLEANUP)

try:
    tok = {"admin": login(acc["users"]["sec.admin"]["userName"]), "empty": login(acc["users"]["sec.empty"]["userName"]),
           "adminb": login(acc["users"]["sec.adminb"]["userName"])}

    # ---- GET available-requirements ----
    r = call("GET", avail(OWN["proj"], OWN["mod"]), tok["admin"])
    check("available-requirements: admin, own project/module -> 200 list", r[0] == 200 and isinstance(r[1], list), f"{r[0]} {msg(r)}")
    check("available-requirements: lists the module's requirement", any((x.get("value") or "").lower() == OWN["req"].lower() for x in (r[1] or [])), msg(r))
    r = call("GET", avail(OTHER["proj"], OTHER["mod"]), tok["admin"])
    check("available-requirements: other site's project/module -> blocked", blocked(r), f"{r[0]} {msg(r)}")
    r = call("GET", avail(OWN["proj"], OTHER["mod"]), tok["admin"])
    check("available-requirements: own project + other site's module -> blocked", blocked(r), f"{r[0]} {msg(r)}")
    r = call("GET", avail(OWN["proj"], OWN["mod"]), tok["empty"])
    check("available-requirements: no Requirement menu -> 403 PERMISSION_DENIED", r[0] == 403 and code(r) == "PERMISSION_DENIED", f"{r[0]} {msg(r)}")
    r = call("GET", avail(OWN["proj"], OWN["mod"]), None)
    check("available-requirements: no token -> 401", r[0] == 401, f"{r[0]} {msg(r)}")
    r = call("GET", avail(OWN["proj"], OWN["mod"]), tok["adminb"])
    check("available-requirements: site B admin with site A header -> 403", r[0] == 403, f"{r[0]} {msg(r)}")

    # ---- GET get-requirement-status-tracking-by-requirementId ----
    r = call("GET", by_req(OWN["req"]), tok["admin"])
    check("by-requirementId: admin, own requirement -> 200", r[0] == 200 and isinstance(r[1], dict) and "total" in r[1], f"{r[0]} {msg(r)}")
    r = call("GET", by_req(OTHER["req"]), tok["admin"])
    check("by-requirementId: other site's requirement -> blocked", blocked(r), f"{r[0]} {msg(r)}")
    r = call("GET", by_req(OWN["req"]), tok["empty"])
    check("by-requirementId: no Requirement menu -> 403", r[0] == 403 and code(r) == "PERMISSION_DENIED", f"{r[0]} {msg(r)}")
    r = call("GET", by_req(OWN["req"]), None)
    check("by-requirementId: no token -> 401", r[0] == 401, f"{r[0]} {msg(r)}")
    r = call("GET", f"{BASE}/get-requirement-status-tracking-by-requirementId", tok["admin"])
    check("by-requirementId: no id -> 200 empty (unchanged behaviour)", r[0] == 200 and (r[1] or {}).get("total") == 0, f"{r[0]} {msg(r)}")

    # ---- bulk-add / bulk-save with the record-level RequirementId ----
    r = call("POST", f"{BASE}/bulk-add", tok["admin"], A, save_body(OTHER["req"]))
    check("bulk-add: other site's requirement -> blocked", blocked(r), f"{r[0]} {msg(r)}")
    check("bulk-add: nothing saved for the other site's requirement", tracking_rows(OTHER["req"]) == "0", tracking_rows(OTHER["req"]))
    r = call("POST", f"{BASE}/bulk-save", tok["admin"], A, {**save_body(OTHER["req"]), "recordId": "x"})
    check("bulk-save: other site's requirement -> blocked", blocked(r), f"{r[0]} {msg(r)}")
    r = call("POST", f"{BASE}/bulk-add", tok["admin"], A, save_body(OWN["req"]))
    check("bulk-add: admin, own requirement -> 2xx (business logic unchanged)", 200 <= r[0] < 300, f"{r[0]} {msg(r)}")
    check("bulk-add: row saved with Roadblock", tracking_rows(OWN["req"]) == "1", tracking_rows(OWN["req"]))
    r = call("POST", f"{BASE}/bulk-add", tok["empty"], A, save_body(OWN["req"]))
    check("bulk-add: no Requirement menu -> 403", r[0] == 403 and code(r) == "PERMISSION_DENIED", f"{r[0]} {msg(r)}")
    r = call("POST", f"{BASE}/bulk-add", tok["admin"], A, {"projectId": OWN["proj"], "projectModuleId": OWN["mod"], "rows": []})
    check("bulk-add: validation messages still reach the user", r[0] == 400 and "Requirement is required" in json.dumps(r[1]), f"{r[0]} {msg(r)}")
    time.sleep(3)
    row = sql("SELECT TOP 1 ISNULL(UserId,'') + '|' + ISNULL(JSON_VALUE(FullMessage,'$.action'),'') + '|' + ISNULL(JSON_VALUE(FullMessage,'$.status'),'') "
              "+ '|' + CAST(CHARINDEX('security test', FullMessage) AS varchar(10)) FROM Log WHERE PageUrl LIKE '%requirement-status-tracking/bulk-add%' "
              "AND JSON_VALUE(FullMessage,'$.decision') IS NULL AND JSON_VALUE(FullMessage,'$.status') LIKE '2%' ORDER BY CreatedOnUtc DESC", out=True) or ""
    parts = row.split("|")
    check("activity log row for bulk-add: user, action, 2xx, no body", len(parts) == 4 and parts[0].lower() == ADMIN_ID.lower()
          and parts[1] == "BulkAdd" and parts[3] == "0", row)

    # ---- Other HTTP methods on the new paths never run the actions ----
    for path in ("available-requirements", "get-requirement-status-tracking-by-requirementId"):
        for method in ("POST", "PUT", "PATCH", "DELETE"):
            r = call(method, f"{BASE}/{path}", tok["admin"], A, {} if method != "DELETE" else None)
            check(f"{method} {path} -> not 2xx", not 200 <= r[0] < 300, f"{r[0]} {msg(r)}")

    # ---- Individual View grant, user not on the project: record scope applies ----
    sql(f"INSERT INTO UserMenuPermissions (Id, UserId, SiteId, SiteModuleMenuId, ShowInMenu, ViewAccess, ManageAccess, Reason, Deleted, CreatedById, CreatedOnUtc) "
        f"VALUES (NEWID(), '{EMPTY_ID}', '{A}', '{REQ_MENU_A}', 1, 1, 0, 'security test', 0, '{EMPTY_ID}', SYSUTCDATETIME())")
    tok["empty"] = login(acc["users"]["sec.empty"]["userName"])
    if wait_for_menu(tok["empty"]):
        r = call("GET", avail(OWN["proj"], OWN["mod"]), tok["empty"])
        check("available-requirements: View grant, not on the project -> 404 (record scope)", r[0] == 404 and code(r) == "RECORD_NOT_FOUND", f"{r[0]} {msg(r)}")
        r = call("GET", by_req(OWN["req"]), tok["empty"])
        check("by-requirementId: View grant, not on the project -> 404 (record scope)", r[0] == 404 and code(r) == "RECORD_NOT_FOUND", f"{r[0]} {msg(r)}")
        r = call("POST", f"{BASE}/bulk-add", tok["empty"], A, save_body(OWN["req"]))
        check("bulk-add: View-only grant -> 403 (needs Manage)", r[0] == 403 and code(r) == "PERMISSION_DENIED", f"{r[0]} {msg(r)}")
    else:
        check("View grant visible in my-permissions", False, "timed out")
finally:
    sql(CLEANUP)

print(f"\n{sum(results)}/{len(results)} passed")
sys.exit(0 if all(results) else 1)
