"""End-to-end authorization checks against a locally running API and the throwaway test database.

Usage: python verify_security.py <accounts-json> <audit|enforce>

Start the API with ConnectionStrings__DefaultConnection pointing at Meldep_Security_Test. It stops
unless the database is the test copy, the API is on localhost, and test accounts can log in (those
accounts exist only in the copy). Writes go through the real API, and every check prints PASS or FAIL.
"""
import json
import subprocess
import sys
import urllib.error
import urllib.request
import uuid

API = "http://localhost:5086"
DATABASE = "Meldep_Security_Test"
MISSING_ID = "00000000-0000-0000-0000-000000000000"

accounts = json.load(open(sys.argv[1], encoding="utf-8"))
stage = sys.argv[2]
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


def call(method, path, token=None, site=SITE_A, body=None, headers=None):
    request = urllib.request.Request(API + path, method=method)
    request.add_header("Content-Type", "application/json")
    if token:
        request.add_header("Authorization", f"Bearer {token}")
    if site:
        request.add_header("X-Site-Id", site)
    for key, value in (headers or {}).items():
        request.add_header(key, value)
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


def code(response):
    return response[1].get("code") if isinstance(response[1], dict) else None


def allowed(response):
    return response[0] not in (401, 403)


if db("SELECT DB_NAME();")[0].strip() != DATABASE:
    raise SystemExit("Not the test database. Stopping.")

# Each run starts clean, so a stopped earlier run can't change the results.
db("UPDATE SecurityGroupMembers SET Deleted=1; UPDATE SecurityGroupMenuPermissions SET Deleted=1; UPDATE SecurityGroups SET Deleted=1; UPDATE UserMenuPermissions SET Deleted=1;")

users = accounts["users"]
tokens = {name: login(info["userName"]) for name, info in users.items() if name != "sec.norole"}
empty_id, empty2_id = users["sec.empty"]["id"], users["sec.empty2"]["id"]
admin_id, super_id = users["sec.admin"]["id"], users["sec.superadmin"]["id"]
shared_id = users["sec.shared"]["id"]
super_token, admin_token, empty_token = tokens["sec.superadmin"], tokens["sec.admin"], tokens["sec.empty"]


def can_list(token, site=SITE_A):
    return call("POST", "/test-case/list", token, site, {"page": 1, "pageSize": 1})


def can_delete(token, site=SITE_A):
    # The record doesn't exist, so a request that passes authorization changes nothing.
    return call("DELETE", f"/test-case/{MISSING_ID}", token, site)


print(f"\n=== Stage: {stage} ===")

print("\n--- Authentication and anonymous access ---")
check("No token on a protected API: 401", call("POST", "/test-case/list", None, SITE_A, {})[0] == 401)
check("Forged token: 401", call("POST", "/test-case/list", "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ4In0.AAAA", SITE_A, {})[0] == 401)
for path in ["overview", "users", "roles", "groups", f"effective/{empty_id}", "audit", f"users/{empty_id}/permissions"]:
    check(f"Anonymous /security-admin/{path}: 401", call("GET", f"/security-admin/{path}")[0] == 401)
status, body = call("POST", "/auth/login", site=None, body={"username": users["sec.norole"]["userName"], "password": accounts["password"]})
if stage == "enforce":
    check("User with no role in any site can't log in", status == 400, f"{status}")
else:
    check("Audit mode: user with no role can still log in, as today", status == 200, f"{status}")

print("\n--- Security admin access ---")
for path in ["overview", "users", "roles", "groups", "audit", f"effective/{empty_id}"]:
    check(f"Non-admin /security-admin/{path}: 403", call("GET", f"/security-admin/{path}", tokens["sec.employee"])[0] == 403)
check("Admin of site B, header site A: 403", call("GET", "/security-admin/groups", tokens["sec.adminb"], SITE_A)[0] == 403)
check("Site admin can open the overview", call("GET", "/security-admin/overview", admin_token)[0] == 200)
check("Site admin can list only their site's users", call("GET", f"/security-admin/effective/{users['sec.adminb']['id']}", admin_token)[0] == 404)

print("\n--- Create group and find the test menu ---")
group_a = call("POST", "/security-admin/groups", super_token, body={"name": f"QA Allow {uuid.uuid4().hex[:6]}"})[1]["id"]
group_deny = call("POST", "/security-admin/groups", super_token, body={"name": f"QA Deny {uuid.uuid4().hex[:6]}"})[1]["id"]
duplicate = call("POST", "/security-admin/groups", super_token, body={"name": db(f"SELECT Name FROM SecurityGroups WHERE Id='{group_a}'")[0]})
check("Duplicate group name in a site is rejected", duplicate[0] == 400)
matrix = call("GET", f"/security-admin/groups/{group_a}/permissions", super_token)[1]
menu = next(row["siteModuleMenuId"] for row in matrix if row["link"] == "/test-case")
check("Matrix hides external links", all(not row["link"].startswith("http") for row in matrix))


def group_set(group, view, manage, show=False, token=super_token):
    return call("PUT", f"/security-admin/groups/{group}/permissions", token, body={"siteModuleMenuId": menu, "showInMenu": show, "viewAccess": view, "manageAccess": manage})


def user_set(user, view, manage, show=False, reason="verification", token=super_token, expires=None, site=SITE_A, menu_id=None):
    return call("PUT", f"/security-admin/users/{user}/permissions", token, site, {"siteModuleMenuId": menu_id or menu, "showInMenu": show, "viewAccess": view, "manageAccess": manage, "reason": reason, "expiresOnUtc": expires})


def effective(user, token=super_token, site=SITE_A):
    data = call("GET", f"/security-admin/effective/{user}", token, site)[1]
    return next(row for row in data["menus"] if row["link"] == "/test-case")


def my_access(token, site=SITE_A):
    data = call("GET", "/authorization/my-permissions", token, site)[1]
    return next((row for row in data["menus"] if row["link"] == "/test-case"), {"view": False, "manage": False, "show": False})


enforced = stage == "enforce"

print("\n--- Permission precedence ---")
result = can_list(empty_token)
if enforced:
    check("No grants: View denied (403)", result[0] == 403 and code(result) == "PERMISSION_DENIED", str(result[0]))
    check("No grants: Manage denied (403)", can_delete(empty_token)[0] == 403)
else:
    check("Audit mode: missing View is logged but allowed", allowed(result), str(result[0]))
    check("Audit mode: missing Manage is logged but allowed", allowed(can_delete(empty_token)))
    rows = db(f"SELECT COUNT(*) FROM [Log] WHERE ShortMessage='Authorization audit: would deny' AND UserId='{empty_id}';")
    check("Audit mode: a would-deny row was saved to the Log table", int(rows[0]) > 0, rows[0])

call("POST", f"/security-admin/groups/{group_a}/members", super_token, body={"userIds": [empty_id]})
call("POST", f"/security-admin/groups/{group_deny}/members", super_token, body={"userIds": [empty_id]})
check("Group save returns success", group_set(group_a, 1, 0, True)[0] == 204)

access = my_access(empty_token)
check("Group Allow View: my-permissions shows View only", access["view"] and not access["manage"], str(access))
check("Group Allow View: list API allowed", allowed(can_list(empty_token)))
if enforced:
    check("Group Allow View: delete API denied", can_delete(empty_token)[0] == 403)

group_set(group_a, 1, 1, True)
check("Group Allow Manage: delete API allowed", allowed(can_delete(empty_token)))
check("Group Allow Manage: my-permissions shows Manage", my_access(empty_token)["manage"])

group_set(group_deny, 0, 2)
access = my_access(empty_token)
check("Group Deny Manage beats group Allow, View stays", access["view"] and not access["manage"], str(access))
if enforced:
    check("Group Deny Manage: delete API denied", can_delete(empty_token)[0] == 403)
    check("Group Deny Manage: list API allowed", allowed(can_list(empty_token)))

check("Individual permission needs a reason", user_set(empty_id, 0, 1, reason="")[0] == 400)
check("Allow Manage with Deny View is rejected", user_set(empty_id, 2, 1)[0] == 400)
check("Individual save returns success", user_set(empty_id, 0, 1)[0] == 204)
check("User Allow beats group Deny", my_access(empty_token)["manage"])
check("User Allow: delete API allowed", allowed(can_delete(empty_token)))
effective_row = effective(empty_id)
check("Effective screen explains individual Allow", "individual Allow" in effective_row["manageReason"], effective_row["manageReason"])
check("Effective screen lists all three source kinds or the two present", {s["kind"] for s in effective_row["sources"]} >= {"Group", "User"}, str(effective_row["sources"]))

user_set(empty_id, 2, 0)
access = my_access(empty_token)
check("User Deny View blocks View and Manage", not access["view"] and not access["manage"], str(access))
check("User Deny View hides the menu", not access["show"], str(access))
if enforced:
    check("User Deny View: list API denied", can_list(empty_token)[0] == 403)
    check("User Deny View: delete API denied", can_delete(empty_token)[0] == 403)
effective_row = effective(empty_id)
check("Effective screen agrees with the API", effective_row["view"] == access["view"] and effective_row["manage"] == access["manage"])
check("Effective screen explains individual Deny", "individual Deny" in effective_row["viewReason"], effective_row["viewReason"])

check("Past end date is rejected", user_set(empty_id, 0, 2, expires="2020-01-01T00:00:00Z")[0] == 400)
user_set(empty_id, 0, 2, expires="2099-01-01T00:00:00Z")
check("User Deny Manage with future end date applies", not my_access(empty_token)["manage"])
db(f"UPDATE UserMenuPermissions SET ExpiresOnUtc='2020-01-01' WHERE UserId='{empty_id}' AND Deleted=0;")
fresh = effective(empty_id)
check("Expired individual rule is ignored", not any(s["kind"] == "User" for s in fresh["sources"]), str(fresh["sources"]))

check("Clearing an individual rule needs no reason", user_set(empty_id, 0, 0, reason=None)[0] == 204)
check("Cleared rule is soft deleted, not removed", int(db(f"SELECT COUNT(*) FROM UserMenuPermissions WHERE UserId='{empty_id}' AND SiteModuleMenuId='{menu}' AND Deleted=1;")[0]) >= 1)

print("\n--- Menu visibility ---")
visible = lambda token: any(item["link"] == "/test-case" for module in call("GET", "/modules/activemodulesmenusbysite", token)[1] for item in module["customSiteModuleMenuList"])
group_set(group_deny, 0, 0)
group_set(group_a, 1, 1, True)
check("Group Allow with Show: appears in the user's menu", visible(empty_token))
check("Another user without grants: not in their menu", not visible(tokens["sec.empty2"]))
user_set(empty_id, 2, 0)
check("User Deny: removed from the user's menu", not visible(empty_token))
user_set(empty_id, 0, 0, reason=None)

print("\n--- Group status and removal ---")
group_name = db(f"SELECT Name FROM SecurityGroups WHERE Id='{group_a}'")[0]
check("Deactivate group", call("PUT", f"/security-admin/groups/{group_a}", super_token, body={"name": group_name, "active": False})[0] == 204)
check("Inactive group grants nothing", not my_access(empty_token)["view"])
call("PUT", f"/security-admin/groups/{group_a}", super_token, body={"name": group_name, "active": True})
check("Reactivated group grants again", my_access(empty_token)["view"])
check("Remove member", call("DELETE", f"/security-admin/groups/{group_a}/members/{empty_id}", super_token)[0] == 204)
check("Removed member loses access", not my_access(empty_token)["view"])
check("Removing again: 404", call("DELETE", f"/security-admin/groups/{group_a}/members/{empty_id}", super_token)[0] == 404)
call("POST", f"/security-admin/groups/{group_a}/members", super_token, body={"userIds": [empty_id]})
check("Re-added member regains access", my_access(empty_token)["view"])
check("Delete group", call("DELETE", f"/security-admin/groups/{group_deny}", super_token)[0] == 204)
check("Deleted group is gone from the list", all(g["id"] != group_deny for g in call("GET", "/security-admin/groups", super_token)[1]))
check("Deleted group row is kept for audit", int(db(f"SELECT COUNT(*) FROM SecurityGroups WHERE Id='{group_deny}' AND Deleted=1;")[0]) == 1)

print("\n--- Admin escalation rules ---")
check("Admin can't change their own individual permissions", user_set(admin_id, 0, 1, token=admin_token)[0] == 403)
check("Admin can't change their own roles", call("PUT", f"/security-admin/users/{admin_id}/roles", admin_token, body={"siteRoleIds": ["x"]})[0] == 403)
check("Admin can't add themselves to a group", call("POST", f"/security-admin/groups/{group_a}/members", admin_token, body={"userIds": [admin_id]})[0] == 403)
check("Admin can't change a higher-ranked user", user_set(super_id, 2, 0, token=admin_token)[0] == 403)

# A group with a super admin in it can't be changed by a lower admin.
group_high = call("POST", "/security-admin/groups", tokens["sec.superadmin"], body={"name": f"QA High {uuid.uuid4().hex[:6]}"})[1]["id"]
system_token = login(accounts["realUsers"]["system-super-admin"]["userName"])
call("POST", f"/security-admin/groups/{group_high}/members", system_token, body={"userIds": [super_id]})
check("Admin can't change a group containing a higher-ranked user", call("PUT", f"/security-admin/groups/{group_high}/permissions", admin_token, body={"siteModuleMenuId": menu, "viewAccess": 2})[0] == 403)
check("Super admin can't change a group they belong to", group_set(group_high, 0, 2)[0] == 403)
call("DELETE", f"/security-admin/groups/{group_high}", system_token)

admin_perms = {row["link"]: row for row in call("GET", "/authorization/my-permissions", admin_token)[1]["menus"]}
missing = next((row for row in matrix if not admin_perms.get(row["link"], {}).get("manage")), None)
if missing:
    above = user_set(empty2_id, 0, 1, token=admin_token, menu_id=missing["siteModuleMenuId"])
    check(f"Admin can't grant Manage they don't hold ({missing['link']})", above[0] == 403 and code(above) == "SECURITY_ADMIN_DENIED", str(above))
    check("Admin can still Deny what they don't hold", user_set(empty2_id, 0, 2, token=admin_token, menu_id=missing["siteModuleMenuId"])[0] == 204)
    user_set(empty2_id, 0, 0, reason=None, token=admin_token, menu_id=missing["siteModuleMenuId"])
    group_of_admin = call("POST", "/security-admin/groups", admin_token, body={"name": f"QA Admin {uuid.uuid4().hex[:6]}"})[1]["id"]
    db(f"INSERT SecurityGroupMenuPermissions (Id, SecurityGroupId, SiteId, SiteModuleMenuId, ViewAccess, ManageAccess, CreatedById, CreatedOnUtc) VALUES (NEWID(), '{group_of_admin}', '{SITE_A}', '{missing['siteModuleMenuId']}', 1, 1, '{super_id}', SYSUTCDATETIME());")
    add = call("POST", f"/security-admin/groups/{group_of_admin}/members", admin_token, body={"userIds": [empty2_id]})
    check("Admin can't add someone to a group granting what they don't hold", add[0] == 403, str(add))
    call("DELETE", f"/security-admin/groups/{group_of_admin}", super_token)
else:
    print("SKIP  Admin holds Manage on every menu, so the grant-above rule isn't exercised here")

print("\n--- Multi-site isolation ---")
adminb = tokens["sec.adminb"]
check("Site B admin can't see site A's group", call("GET", f"/security-admin/groups/{group_a}/members", adminb, SITE_B)[0] == 404)
check("Site B admin can't change site A's group", call("PUT", f"/security-admin/groups/{group_a}", adminb, SITE_B, {"name": "hijack"})[0] == 404)
check("Site B admin can't see a site A user", call("GET", f"/security-admin/users/{empty_id}/permissions", adminb, SITE_B)[0] == 404)
check("Site B admin can't grant to a site A user", user_set(empty_id, 1, 1, token=adminb, site=SITE_B)[0] == 404)
group_b = call("POST", "/security-admin/groups", adminb, SITE_B, {"name": f"QA B {uuid.uuid4().hex[:6]}"})[1]["id"]
foreign = call("PUT", f"/security-admin/groups/{group_b}/permissions", adminb, SITE_B, {"siteModuleMenuId": menu, "viewAccess": 2})
check("A site A menu can't be used in a site B group", foreign[0] == 404, str(foreign))
foreign_member = call("POST", f"/security-admin/groups/{group_b}/members", adminb, SITE_B, {"userIds": [empty_id]})
check("A site A user can't join a site B group", foreign_member[0] == 404, str(foreign_member))

spoof = can_list(empty_token, SITE_B)
if enforced:
    check("Changed X-Site-Id to a site they don't belong to: 403", spoof[0] == 403 and code(spoof) == "SITE_NOT_MEMBER", str(spoof[0]))
    # No header means the user's own current site (the one login opens), never a wider scope.
    no_header, own_site = can_list(empty_token, ""), can_list(empty_token, SITE_A)
    check("Missing X-Site-Id: same result as the user's own site", no_header[0] == own_site[0] and code(no_header) == code(own_site), f"{no_header[0]} vs {own_site[0]}")
    mismatch = call("POST", "/test-case/list", tokens["sec.shared"], SITE_A, {"siteId": SITE_B})
    check("Body siteId differs from the active site: 403", mismatch[0] == 403, str(mismatch[0]))
else:
    rows = db(f"SELECT COUNT(*) FROM [Log] WHERE ShortMessage='Authorization audit: would deny' AND UserId='{empty_id}' AND FullMessage LIKE '%SITE_NOT_MEMBER%';")
    check("Audit mode: foreign-site request logged as would-deny", int(rows[0]) > 0, rows[0])

print("\n--- Shared user: rights stay per site ---")
shared_token = tokens["sec.shared"]
call("POST", f"/security-admin/groups/{group_a}/members", super_token, body={"userIds": [shared_id]})
check("Group in site A: View in site A", my_access(shared_token, SITE_A)["view"])
check("Group in site A: nothing in site B", not my_access(shared_token, SITE_B)["view"])
if enforced:
    check("Group in site A: site B API denied", can_list(shared_token, SITE_B)[0] == 403)
menu_b = next(row["siteModuleMenuId"] for row in call("GET", f"/security-admin/groups/{group_b}/permissions", adminb, SITE_B)[1] if row["link"] == "/test-case")
check("Site B admin grants Deny in B", user_set(shared_id, 2, 2, token=adminb, site=SITE_B, menu_id=menu_b)[0] == 204)
check("Deny in site B doesn't affect site A", my_access(shared_token, SITE_A)["view"])
sites = call("GET", f"/security-admin/users/{shared_id}/sites", adminb, SITE_B)[1]
check("Site admin sees only their own site for a shared user", [s["siteId"] for s in sites] == [SITE_B], str(sites))
sites = call("GET", f"/security-admin/users/{shared_id}/sites", system_token, SITE_A)[1]
check("System admin sees all the shared user's sites", {s["siteId"] for s in sites} == {SITE_A, SITE_B}, str(sites))
switch = call("POST", "/authorization/switch-site", tokens["sec.empty"], SITE_A, {"siteId": SITE_B})
check("Can't switch to a site without membership", switch[0] == 403, str(switch[0]))

print("\n--- Removing a share ends group and individual access ---")
call("POST", f"/security-admin/groups/{group_b}/members", adminb, SITE_B, {"userIds": [shared_id]})
person = users["sec.shared"]["personId"]
mapping = db(f"SELECT Id FROM PersonSitesMapping WHERE PersonId='{person}' AND SiteId='{SITE_B}' AND Deleted=0;")[0]
removed = call("DELETE", f"/person-site-mapping/{mapping}", adminb, SITE_B)
check("Share removed", removed[0] in (200, 204), str(removed))
check("Group membership in that site ended", int(db(f"SELECT COUNT(*) FROM SecurityGroupMembers WHERE UserId='{shared_id}' AND SiteId='{SITE_B}' AND Deleted=0;")[0]) == 0)
check("Individual permissions in that site ended", int(db(f"SELECT COUNT(*) FROM UserMenuPermissions WHERE UserId='{shared_id}' AND SiteId='{SITE_B}' AND Deleted=0;")[0]) == 0)
check("Access in the other site is untouched", my_access(shared_token, SITE_A)["view"])
if enforced:
    check("Removed site is refused on the next request", can_list(shared_token, SITE_B)[0] == 403)
db(f"UPDATE PersonSitesMapping SET Deleted=0 WHERE Id='{mapping}';")
db(f"INSERT AspNetUserRoles (UserId, RoleId, SiteId) SELECT '{shared_id}', 'sec-empty-role', '{SITE_B}' WHERE NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId='{shared_id}' AND RoleId='sec-empty-role' AND SiteId='{SITE_B}');")

print("\n--- Record-level isolation ---")
foreign_issue = db(f"SELECT TOP 1 Id FROM Issue WHERE SiteId <> '{SITE_A}';")
local_issue = db(f"SELECT TOP 1 Id FROM Issue WHERE SiteId = '{SITE_A}' AND Deleted = 0 ORDER BY CreatedOnUtc DESC;")
response = call("GET", f"/issue/{foreign_issue[0]}", super_token)
if enforced:
    check("Record in another site, by ID: refused", response[0] in (403, 404), str(response[0]))
else:
    check("Record in another site, audit mode: not blocked yet", response[0] not in (401, 403), str(response[0]))

# A menu grant opens the page, but the module's own record rule must still apply.
issue_menu = next(row["siteModuleMenuId"] for row in matrix if row["link"] == "/issue")
group_issue = call("POST", "/security-admin/groups", super_token, body={"name": f"QA Issue {uuid.uuid4().hex[:6]}"})[1]["id"]
call("PUT", f"/security-admin/groups/{group_issue}/permissions", super_token, body={"siteModuleMenuId": issue_menu, "viewAccess": 1})
call("POST", f"/security-admin/groups/{group_issue}/members", super_token, body={"userIds": [empty2_id]})
empty2_token = tokens["sec.empty2"]
check("Group grant on Issue opens the Issue list API", call("POST", "/issue/list", empty2_token, SITE_A, {"page": 1, "pageSize": 1})[0] not in (401, 403))
response = call("GET", f"/issue/{local_issue[0]}", empty2_token)
if enforced:
    check("Menu grant doesn't bypass the record rule: unassigned user refused", response[0] in (403, 404), str(response[0]))
else:
    check("Audit mode: out-of-scope record request isn't blocked yet", response[0] not in (401, 403), str(response[0]))
call("DELETE", f"/security-admin/groups/{group_issue}", super_token)

print("\n--- Roles and site access through the API ---")
roles = call("GET", "/security-admin/roles", super_token)[1]
employee_role = next(r["siteRoleId"] for r in roles if r["normalizedName"] == "employee")
empty_role = next(r["siteRoleId"] for r in roles if r["normalizedName"] == "sec-empty")
check("Role list shows user counts", any(r["userCount"] > 0 for r in roles))
check("Empty role list is rejected", call("PUT", f"/security-admin/users/{empty2_id}/roles", super_token, body={"siteRoleIds": []})[0] == 400)
check("Assign a role", call("PUT", f"/security-admin/users/{empty2_id}/roles", super_token, body={"siteRoleIds": [employee_role]})[0] == 204)
assigned = call("GET", f"/security-admin/users/{empty2_id}/roles", super_token)[1]
check("Role assignment replaces roles in this site", [r["siteRoleId"] for r in assigned] == [employee_role], str(assigned))
super_role = next(r["siteRoleId"] for r in roles if r["normalizedName"] == "site-super-admin")
above_role = call("PUT", f"/security-admin/users/{empty2_id}/roles", admin_token, body={"siteRoleIds": [super_role]})
check("Admin can't assign a role above their own", above_role[0] == 400, str(above_role))
call("PUT", f"/security-admin/users/{empty2_id}/roles", super_token, body={"siteRoleIds": [empty_role]})
blocked = call("GET", "/security-admin/users", super_token, SITE_A, None)[1]["total"]
blocked_only = call("GET", "/security-admin/users?blockedOnly=true&pageSize=200", super_token)[1]
check("Site Access finds users who can't open the site", any(u["userId"] == users["sec.norole"]["id"] and not u["canAccessSite"] for u in blocked_only["items"]) and blocked_only["total"] < blocked)

print("\n--- Audit log ---")
for category, name in [("permission", "permission changes"), ("session", "sign-ins")]:
    entries = call("GET", f"/security-admin/audit?category={category}&pageSize=5", super_token)[1]
    check(f"Audit screen returns {name}", entries["total"] > 0, str(entries["total"]))
changes = call("GET", f"/security-admin/audit?category=permission&search={group_a}&pageSize=50", super_token)[1]
check("Permission change rows record actor and target", any(super_id in (row["details"] or "") and group_a in row["details"] for row in changes["items"]), str(changes["items"][:1]))
site_b_rows = call("GET", "/security-admin/audit?pageSize=200", adminb, SITE_B)[1]
leaks = [row for row in site_b_rows["items"] if SITE_A in (row["details"] or "")]
check("Site B admin's audit screen holds no site A rows", site_b_rows["total"] > 0 and not leaks, f"{site_b_rows['total']} rows, {len(leaks)} leaks")
check("Site B admin can't read site A's permission changes", call("GET", f"/security-admin/audit?category=permission&search={group_a}&pageSize=50", adminb, SITE_B)[1]["total"] == 0)
check("Site B admin asking for all sites still gets only site B", call("GET", "/security-admin/audit?allSites=true&pageSize=1", adminb, SITE_B)[1]["total"] == site_b_rows["total"])
check("System Super Admin can search all sites", call("GET", f"/security-admin/audit?allSites=true&category=permission&search={group_a}&pageSize=5", system_token)[1]["total"] > 0)
if enforced:
    denied = can_delete(tokens["sec.employee"])
    if denied[0] == 403:
        check("A denial gives a reference ID in the response", "referenceId" in denied[1], str(denied[1]))
    rows = db(f"SELECT COUNT(*) FROM [Log] WHERE ShortMessage='Authorization denied' AND UserId='{empty_id}';")
    check("Denied requests were saved to the Log table", int(rows[0]) > 0, rows[0])
rows = db(f"SELECT COUNT(*) FROM [Log] WHERE ShortMessage='Permission change' AND FullMessage LIKE '%{SITE_A}%';")
check("Permission changes were saved to the Log table", int(rows[0]) > 0, rows[0])
rows = db(f"SELECT COUNT(*) FROM [Log] WHERE ShortMessage='User activity' AND UserId='{super_id}';")
check("User activity was saved to the Log table", int(rows[0]) > 0, rows[0])
rows = db(f"SELECT COUNT(*) FROM [Log] WHERE ShortMessage='Login succeeded' AND UserId='{empty_id}';")
check("Sign-ins were saved to the Log table", int(rows[0]) > 0, rows[0])
rows = db(f"SELECT COUNT(*) FROM [Log] WHERE FullMessage LIKE '%{accounts['password']}%' OR FullMessage LIKE '%eyJhbGci%';")
check("No passwords or tokens in the Log table", int(rows[0]) == 0, rows[0])

print("\n--- Coverage ---")
coverage = call("GET", "/authorization/coverage", system_token)
body = coverage[1] if isinstance(coverage[1], dict) else {}
print("      coverage:", {key: value if not isinstance(value, list) else len(value) for key, value in body.items()})
unclassified = next((value for key, value in body.items() if "unclass" in key.lower()), None)
check("Every API action has a declared rule", unclassified is not None and (unclassified == 0 or unclassified == []), str(unclassified))

# Clean up what this run created, so the next stage starts the same way.
for group in [group_a, group_b]:
    call("DELETE", f"/security-admin/groups/{group}", super_token if group == group_a else adminb, SITE_A if group == group_a else SITE_B)

failed = [item for item in results if not item[1]]
print(f"\n=== {stage}: {len(results) - len(failed)} passed, {len(failed)} failed ===")
for name, _, detail in failed:
    print(f"  FAILED: {name}  {detail}")
sys.exit(1 if failed else 0)
