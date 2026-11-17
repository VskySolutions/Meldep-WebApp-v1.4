"""Checks PUT /requirement/update-dates (added 2026-11-16) against the local API on Meldep_Security_Test:
menu permission by role and by individual grant, site gate, record scope, safe errors, and the activity-log row.
Usage: python verify_requirement_dates.py <accounts.json>"""
import json, subprocess, sys, time, urllib.request, urllib.error

API = "http://localhost:5086"
acc = json.load(open(sys.argv[1], encoding="utf-8"))
A, B = acc["siteA"], acc["siteB"]
OWN_REQ = "004223EC-081B-43C2-B54D-668E6F2BBF6D"     # site A requirement
OTHER_REQ = "7D5A4FD2-5FF2-4C9F-8DA8-0CF538E626F5"   # another site's requirement
REQ_MENU_A = "ABEE41F2-28D8-43E6-8222-DA33E6E62C1B"  # site A's /requirement
EMPTY_ID = acc["users"]["sec.empty"]["id"]
ADMIN_ID = acc["users"]["sec.admin"]["id"]
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
def body(req, date="12/31/2026", field="plannedEnd"):
    b = {"requirementId": req}
    if date is not None:
        b[field + "DateStr"] = date
        b[field + "DateNote"] = "security test"
    return b


def wait_for_menu(token, want_manage):
    # The API caches permission snapshots for 60 s; grants written by SQL show up once it expires.
    for _ in range(14):
        s, p = call("GET", "/authorization/my-permissions", token)
        m = next((m for m in (p or {}).get("menus", []) if m.get("link") == "/requirement"), None)
        if m and m.get("view") and bool(m.get("manage")) == want_manage:
            return True
        time.sleep(6)
    return False


def last_activity_row():
    # sqlcmd -W cuts columns at 256 characters, so read the JSON fields instead of the whole message.
    return sql("SELECT TOP 1 ISNULL(UserId,'') + '|' + ISNULL(JSON_VALUE(FullMessage,'$.action'),'') + '|' + ISNULL(JSON_VALUE(FullMessage,'$.status'),'') "
               "+ '|' + CAST(CHARINDEX('security test', FullMessage) AS varchar(10)) FROM Log WHERE PageUrl LIKE '%/requirement/update-dates%' "
               "AND JSON_VALUE(FullMessage,'$.decision') IS NULL ORDER BY CreatedOnUtc DESC", out=True)


CLEANUP = (f"DELETE FROM UserMenuPermissions WHERE Reason='security test'; "
           f"DELETE FROM SitesModifiedLogs WHERE Note='security test'; ")
original = sql(f"SELECT ISNULL(CONVERT(varchar(30), PlannedEndDate, 126), 'NULL') + '|' + ISNULL(CONVERT(varchar(30), PlannedStartDate, 126), 'NULL') "
               f"FROM Requirement WHERE Id='{OWN_REQ}'", out=True)
other_original = sql(f"SELECT ISNULL(CONVERT(varchar(30), PlannedEndDate, 126), 'NULL') FROM Requirement WHERE Id='{OTHER_REQ}'", out=True)
sql(CLEANUP)

try:
    tok = {"admin": login(acc["users"]["sec.admin"]["userName"]), "empty": login(acc["users"]["sec.empty"]["userName"]),
           "adminb": login(acc["users"]["sec.adminb"]["userName"]), "sysadmin": login(acc["realUsers"]["system-super-admin"]["userName"])}

    # ---- Role grant (admin holds Requirement Manage through its role) ----
    r = call("PUT", "/requirement/update-dates", tok["admin"], A, body(OWN_REQ))
    check("admin (role grant) updates planned end date -> 204", r[0] == 204, f"{r[0]} {msg(r)}")
    saved = sql(f"SELECT CONVERT(varchar(10), PlannedEndDate, 101) FROM Requirement WHERE Id='{OWN_REQ}'", out=True)
    check("planned end date saved (business logic unchanged)", saved == "12/31/2026", saved)
    logged = sql(f"SELECT COUNT(*) FROM SitesModifiedLogs WHERE ModuleId IS NOT NULL AND SubModuleId='{OWN_REQ}' AND Note='security test' "
                 f"AND ColumnName='Planned End Date'", out=True)
    check("date change log written with the note", logged == "1", logged)
    time.sleep(3)  # activity rows are written by a background queue
    row = last_activity_row() or ""
    parts = row.split("|")
    check("activity log row: user, action, status 204",
          len(parts) == 4 and parts[0].lower() == ADMIN_ID.lower() and parts[1] == "UpdateRequirementDates" and parts[2] == "204", row)
    check("activity log row has no request body (no note text)", len(parts) == 4 and parts[3] == "0", row)

    # A system super admin gets no bypass of menu grants; whatever the existing PUT /requirement/{id} decides, this matches.
    existing = call("PUT", f"/requirement/{OWN_REQ}", tok["sysadmin"], A, {})
    r = call("PUT", "/requirement/update-dates", tok["sysadmin"], A, body(OWN_REQ, "12/30/2026"))
    same = (r[0] == 403) == (existing[0] == 403)
    check(f"system super admin: same menu decision as PUT /requirement/{{id}} ({existing[0]})", same, f"{r[0]} {msg(r)}")

    # ---- Authentication / menu permission ----
    r = call("PUT", "/requirement/update-dates", None, A, body(OWN_REQ))
    check("no token -> 401", r[0] == 401, f"{r[0]} {msg(r)}")
    r = call("PUT", "/requirement/update-dates", tok["empty"], A, body(OWN_REQ))
    check("user without Requirement menu -> 403 PERMISSION_DENIED", r[0] == 403 and code(r) == "PERMISSION_DENIED", f"{r[0]} {msg(r)}")

    # ---- Site gate ----
    r = call("PUT", "/requirement/update-dates", tok["adminb"], A, body(OWN_REQ))
    check("site B admin sending site A header -> 403", r[0] == 403, f"{r[0]} {msg(r)}")
    r = call("PUT", "/requirement/update-dates", tok["admin"], A, body(OTHER_REQ))
    check("another site's requirement ID -> blocked (403 SITE_SCOPE_MISMATCH / 404 RECORD_NOT_FOUND)",
          (r[0], code(r)) in ((403, "SITE_SCOPE_MISMATCH"), (404, "RECORD_NOT_FOUND")), f"{r[0]} {msg(r)}")
    after = sql(f"SELECT ISNULL(CONVERT(varchar(30), PlannedEndDate, 126), 'NULL') FROM Requirement WHERE Id='{OTHER_REQ}'", out=True)
    check("another site's requirement left unchanged", after == other_original, after)

    # ---- Validation and safe errors ----
    r = call("PUT", "/requirement/update-dates", tok["admin"], A, body(OWN_REQ, "31-31-2026"))
    check("invalid date -> 400 user message", r[0] == 400 and "Invalid date format" in str(r[1]), f"{r[0]} {msg(r)}")
    r = call("PUT", "/requirement/update-dates", tok["admin"], A, body(OWN_REQ, None))
    check("no date -> 400 user message", r[0] == 400 and "Please provide a date" in str(r[1]), f"{r[0]} {msg(r)}")
    r = call("PUT", "/requirement/update-dates", tok["admin"], A, body("00000000-0000-0000-0000-000000000000"))
    check("unknown requirement -> 400 user message", r[0] == 400 and "No requirement found" in str(r[1]), f"{r[0]} {msg(r)}")
    for rr in (r,):
        check("error body carries no exception/stack text", "Exception" not in json.dumps(rr[1]) and " at Vsky" not in json.dumps(rr[1]), msg(rr))

    # ---- Other HTTP methods on the same path never run the action ----
    for method in ("GET", "POST", "DELETE", "PATCH"):
        r = call(method, "/requirement/update-dates", tok["admin"], A, body(OWN_REQ) if method != "GET" else None)
        check(f"{method} /requirement/update-dates -> not 2xx", not 200 <= r[0] < 300, f"{r[0]} {msg(r)}")

    # ---- Individual-user grants: View only is not enough; Manage passes the menu, then record scope applies ----
    sql(f"INSERT INTO UserMenuPermissions (Id, UserId, SiteId, SiteModuleMenuId, ShowInMenu, ViewAccess, ManageAccess, Reason, Deleted, CreatedById, CreatedOnUtc) "
        f"VALUES (NEWID(), '{EMPTY_ID}', '{A}', '{REQ_MENU_A}', 1, 1, 0, 'security test', 0, '{EMPTY_ID}', SYSUTCDATETIME())")
    tok["empty"] = login(acc["users"]["sec.empty"]["userName"])
    if wait_for_menu(tok["empty"], False):
        r = call("PUT", "/requirement/update-dates", tok["empty"], A, body(OWN_REQ))
        check("individual View-only grant -> 403 (PUT needs Manage)", r[0] == 403 and code(r) == "PERMISSION_DENIED", f"{r[0]} {msg(r)}")
        r = call("GET", f"/requirement/details/{OWN_REQ}", tok["empty"], A)
        check("same View grant still reads (GET) -> not 403 PERMISSION_DENIED", code(r) != "PERMISSION_DENIED", f"{r[0]} {msg(r)}")
    else:
        check("View grant visible in my-permissions", False, "timed out")

    sql(f"UPDATE UserMenuPermissions SET ManageAccess=1 WHERE Reason='security test'")
    if wait_for_menu(tok["empty"], True):
        before = sql(f"SELECT CONVERT(varchar(10), PlannedEndDate, 101) FROM Requirement WHERE Id='{OWN_REQ}'", out=True)
        r = call("PUT", "/requirement/update-dates", tok["empty"], A, body(OWN_REQ, "12/29/2026"))
        check("individual Manage grant, user not on the project -> 404 (record scope)", r[0] == 404 and code(r) == "RECORD_NOT_FOUND", f"{r[0]} {msg(r)}")
        saved = sql(f"SELECT CONVERT(varchar(10), PlannedEndDate, 101) FROM Requirement WHERE Id='{OWN_REQ}'", out=True)
        check("out-of-scope update did not change the record", saved == before, f"{before} -> {saved}")
        reason = sql("SELECT TOP 1 FullMessage FROM Log WHERE FullMessage LIKE '%RECORD_OUT_OF_SCOPE%' AND FullMessage LIKE '%UpdateRequirementDates%' "
                     "ORDER BY CreatedOnUtc DESC", out=True)
        check("denial recorded in Log as RECORD_OUT_OF_SCOPE", bool(reason), "no row")
    else:
        check("Manage grant visible in my-permissions", False, "timed out")
finally:
    end, start = original.split("|")
    sql(CLEANUP + f"UPDATE Requirement SET PlannedEndDate={'NULL' if end == 'NULL' else repr(end)}, "
                  f"PlannedStartDate={'NULL' if start == 'NULL' else repr(start)} WHERE Id='{OWN_REQ}'")

print(f"\n{sum(results)}/{len(results)} passed")
sys.exit(0 if all(results) else 1)
