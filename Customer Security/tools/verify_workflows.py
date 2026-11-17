"""Existing workflows with enforcement on. Writes happen, so it only runs against the test copy.

Usage: python verify_workflows.py <accounts-json>
"""
import base64
import hashlib
import json
import secrets
import struct
import subprocess
import sys
import urllib.error
import urllib.request

API = "http://localhost:5086"
DATABASE = "Meldep_Security_Test"
accounts = json.load(open(sys.argv[1], encoding="utf-8"))
SITE_A, SITE_B = accounts["siteA"], accounts["siteB"]
results = []


def check(name, passed, detail=""):
    results.append((name, bool(passed), detail))
    print(f"{'PASS' if passed else 'FAIL'}  {name}" + (f"   [{detail}]" if detail and not passed else ""))


def db(text):
    output = subprocess.run(
        ["sqlcmd", "-S", "DESKTOP-M3PJFV7", "-E", "-C", "-b", "-W", "-h-1", "-s", "|", "-d", DATABASE, "-Q", "SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON; " + text],
        capture_output=True, text=True, check=True).stdout
    return [line for line in output.splitlines() if line.strip()]


def call(method, path, token=None, site=SITE_A, body=None):
    request = urllib.request.Request(API + path, method=method)
    request.add_header("Content-Type", "application/json")
    if token:
        request.add_header("Authorization", f"Bearer {token}")
    if site:
        request.add_header("X-Site-Id", site)
    data = json.dumps(body).encode() if body is not None else None
    try:
        with urllib.request.urlopen(request, data, timeout=120) as response:
            status, raw = response.status, response.read()
    except urllib.error.HTTPError as error:
        status, raw = error.code, error.read()
    try:
        return status, json.loads(raw) if raw else None
    except ValueError:
        return status, raw.decode(errors="replace")


def login(user_name):
    status, body = call("POST", "/auth/login", site=None, body={"username": user_name, "password": accounts["password"]})
    if status != 200:
        raise SystemExit(f"Login failed for {user_name}: {status} {body}")
    return body["token"]


def identity_hash(password):
    salt = secrets.token_bytes(16)
    key = hashlib.pbkdf2_hmac("sha512", password.encode(), salt, 100000, 32)
    return base64.b64encode(b"\x01" + struct.pack(">III", 2, 100000, 16) + salt + key).decode()


if db("SELECT DB_NAME();")[0].strip() != DATABASE:
    raise SystemExit("Not the test database. Stopping.")

users = accounts["users"]
stranger = login(users["sec.employee"]["userName"])
real_admin = login(accounts["realUsers"]["admin"]["userName"])

print("\n--- Leave approval: assigned approver, as the email link does ---")
# Accounts sharing an email can't use the API at all today (see the report), so they're excluded.
rows = db(f"""
SELECT TOP 1 l.Id, l.LeaveStatusId, u.Id, u.UserName, l.LeaveApproverId, l.EmployeeId
FROM EmployeeLeave l
JOIN Employee e ON e.Id = l.LeaveApproverId
JOIN AspNetUsers u ON u.PersonId = e.PersonId AND u.Active = 1 AND u.Deleted = 0
JOIN Employee owner ON owner.Id = l.EmployeeId
WHERE e.SiteId = '{SITE_A}' AND owner.SiteId = '{SITE_A}'
  AND (SELECT COUNT(*) FROM AspNetUsers x WHERE x.NormalizedEmail = u.NormalizedEmail) = 1
  AND u.Id NOT IN (SELECT ur.UserId FROM AspNetUserRoles ur JOIN AspNetRoles r ON r.Id = ur.RoleId WHERE ur.SiteId = '{SITE_A}' AND r.NormalizedName IN ('admin','site-super-admin','system-super-admin','hr'))
ORDER BY l.CreatedOnUtc DESC;""")
if rows:
    leave_id, status_before, approver_id, approver_name, approver_employee, owner_employee = rows[0].split("|")
    leave_status = lambda: db(f"SELECT LeaveStatusId FROM EmployeeLeave WHERE Id='{leave_id}';")[0].strip()
    db(f"UPDATE AspNetUsers SET PasswordHash='{identity_hash(accounts['password'])}', LockoutEnd=NULL, AccessFailedCount=0 WHERE Id='{approver_id}';")
    approver = login(approver_name)
    hr = login(users["sec.hr"]["userName"])
    decline = {"flag": "AV", "leaveStatusFlag": "DC", "approverNote": "Security workflow test", "isPaidLeave": False, "employeeId": owner_employee}
    forward = {"flag": "FD", "leaveApproverId": approver_employee, "hrNote": "Security workflow test", "isPaidLeave": False, "employeeId": owner_employee}

    check("Anonymous can't read a leave (the old open GET)", call("GET", f"/employee-leave/{leave_id}")[0] == 401)
    check("Anonymous can't decide a leave (the old open PUT)", call("PUT", f"/employee-leave/{leave_id}", body=decline)[0] == 401)
    response = call("GET", f"/employee-leave/{leave_id}", approver)
    check("Assigned approver opens the leave", response[0] == 200, str(response[0]))
    response = call("PUT", f"/employee-leave/{leave_id}", stranger, body=decline)
    check("An ordinary employee can't approve or decline someone's leave", response[0] == 403, f"{response[0]} {str(response[1])[:160]}")
    response = call("PUT", f"/employee-leave/{leave_id}", stranger, body=forward)
    check("An ordinary employee can't forward someone's leave", response[0] == 403, f"{response[0]} {str(response[1])[:160]}")
    response = call("POST", f"/employee-leave/cancelleave/{leave_id}", stranger)
    check("An ordinary employee can't cancel someone's leave", response[0] in (403, 404), f"{response[0]} {str(response[1])[:160]}")
    response = call("PUT", f"/employee-leave/{leave_id}", hr, body=decline)
    check("HR can't approve or decline a leave assigned to someone else", response[0] == 403, f"{response[0]} {str(response[1])[:160]}")
    check("Leave status is unchanged after all refused attempts", leave_status() == status_before)
    # Email is disabled in the test copy, so the notification step can fail after the save.
    response = call("PUT", f"/employee-leave/{leave_id}", hr, body=forward)
    check("HR passes the forward check, and the forward is saved", response[0] != 403 and leave_status() != status_before, f"{response[0]} {leave_status()}")
    forwarded_status = leave_status()
    response = call("PUT", f"/employee-leave/{leave_id}", approver, body=decline)
    check("Assigned approver can decide it", response[0] < 400, f"{response[0]} {str(response[1])[:300]}")
    check("The decision was saved", leave_status() != forwarded_status, leave_status())

    # A deliberate individual grant is a real decision, so it does count.
    admin = login(users["sec.superadmin"]["userName"])
    temp_group = call("POST", "/security-admin/groups", admin, body={"name": "QA Leave " + secrets.token_hex(3)})[1]["id"]
    matrix = call("GET", f"/security-admin/groups/{temp_group}/permissions", admin)[1]
    approve_menu = next(row["siteModuleMenuId"] for row in matrix if row["link"] == "/approve-leaves")
    employee_id = users["sec.employee"]["id"]
    grant = call("PUT", f"/security-admin/users/{employee_id}/permissions", admin, body={"siteModuleMenuId": approve_menu, "showInMenu": True, "viewAccess": 1, "manageAccess": 1, "reason": "workflow test"})
    check("Grant Manage on Approve Leaves to that employee", grant[0] == 204, str(grant))
    response = call("PUT", f"/employee-leave/{leave_id}", stranger, body=decline)
    check("With a deliberate Manage grant, they can decide", response[0] < 400, f"{response[0]} {str(response[1])[:200]}")
    call("PUT", f"/security-admin/users/{employee_id}/permissions", admin, body={"siteModuleMenuId": approve_menu, "showInMenu": False, "viewAccess": 0, "manageAccess": 0})
    call("DELETE", f"/security-admin/groups/{temp_group}", admin)
    response = call("PUT", f"/employee-leave/{leave_id}", stranger, body=decline)
    check("Removing the grant takes the right away again", response[0] == 403, f"{response[0]}")
else:
    print("SKIP  No leave with a non-admin approver in the data")

print("\n--- Session and site switching ---")
shared = login(users["sec.shared"]["userName"])
response = call("POST", "/auth/refresh", shared)
check("Session renewal returns a working token", response[0] == 200 and call("GET", "/authorization/my-permissions", (response[1] or {}).get("token"))[0] == 200, f"{response[0]} {str(response[1])[:160]}")
response = call("GET", "/authorization/my-sites", shared)
check("my-sites lists both shared sites", {x["siteId"] for x in response[1]} == {SITE_A, SITE_B}, str(response[1]))
response = call("POST", "/authorization/switch-site", shared, SITE_A, {"siteId": SITE_B})
check("Switching to a shared site works", response[0] == 200 and response[1]["siteId"] == SITE_B, str(response[0]))
check("Switch records the last-used site", db(f"SELECT SiteId FROM PersonSitesMapping WHERE PersonId='{users['sec.shared']['personId']}' AND LastUsed=1 AND Deleted=0;")[0].strip() == SITE_B)
check("Logging in again opens the last-used site", call("POST", "/auth/login", site=None, body={"username": users["sec.shared"]["userName"], "password": accounts["password"]})[1]["siteId"] == SITE_B)
call("POST", "/authorization/switch-site", shared, SITE_B, {"siteId": SITE_A})

revoked = login(users["sec.admin2"]["userName"])
check("Token works before revoking sessions", call("GET", "/authorization/my-permissions", revoked)[0] == 200)
check("Revoke sessions succeeds", call("POST", "/authorization/revoke-sessions", revoked)[0] == 204)
check("Revoked token stops working", call("GET", "/authorization/my-permissions", revoked)[0] == 401)
check("Logging in again works", call("GET", "/authorization/my-permissions", login(users["sec.admin2"]["userName"]))[0] == 200)
status, _ = call("POST", "/auth/login", site=None, body={"username": users["sec.admin2"]["userName"], "password": "wrong-password"})
check("Wrong password is refused", status == 400, str(status))

print("\n--- A real create, read and delete with a real user's role ---")
name = "Security Test " + secrets.token_hex(3)
created = call("POST", "/departments", real_admin, body={"name": name, "active": True, "description": "temporary"})
check("Real admin creates a department", created[0] < 400, f"{created[0]} {str(created[1])[:300]}")
row = db(f"SELECT TOP 1 Id, SiteId FROM Department WHERE Name='{name}';") if created[0] < 400 else []
if row:
    department_id, site = row[0].split("|")
    check("It was saved in the active site", site == SITE_A, site)
    check("Reading it works", call("GET", f"/departments/{department_id}", real_admin)[0] == 200)
    empty = login(users["sec.empty"]["userName"])
    check("A user without permission can't delete it", call("DELETE", f"/departments/{department_id}", empty)[0] == 403)
    other_site_admin = login(users["sec.adminb"]["userName"])
    response = call("DELETE", f"/departments/{department_id}", other_site_admin, SITE_B)
    check("Another site's admin can't delete it", response[0] in (403, 404), str(response[0]))
    check("Real admin deletes it", call("DELETE", f"/departments/{department_id}", real_admin)[0] < 400)

failed = [item for item in results if not item[1]]
print(f"\n=== workflows: {len(results) - len(failed)} passed, {len(failed)} failed ===")
for name, _, detail in failed:
    print(f"  FAILED: {name}  {detail}")
sys.exit(1 if failed else 0)
