"""Same user, same token: the web app's request vs every request shape Swagger UI or Postman can send.

Usage: python verify_swagger_parity.py <accounts-json>. Runs against the local API on Meldep_Security_Test.
"""
import json, sys, urllib.request, urllib.error

API = "http://localhost:5086"
acc = json.load(open(sys.argv[1], encoding="utf-8"))
A, B = acc["siteA"], acc["siteB"]
results = []


def call(method, path, headers, body=None):
    r = urllib.request.Request(API + path, method=method)
    r.add_header("Content-Type", "application/json")
    for k, v in headers.items():
        r.add_header(k, v)
    data = json.dumps(body).encode() if body is not None else None
    try:
        with urllib.request.urlopen(r, data, timeout=180) as resp: status, raw = resp.status, resp.read()
    except urllib.error.HTTPError as e: status, raw = e.code, e.read()
    try: return status, (json.loads(raw) if raw else None)
    except ValueError: return status, raw.decode(errors="replace")


def check(name, ok, detail=""):
    results.append(ok)
    print(("PASS  " if ok else "FAIL  ") + name + ("" if ok else f"   [{str(detail)[:300]}]"))


def login(user):
    s, b = call("POST", "/auth/login", {}, {"username": user, "password": acc["password"]})
    assert s == 200, (user, s, b)
    return b


def fingerprint(resp):
    """Status plus the data itself, so 'same result' means the same rows, not just 200. Ignores values that
    differ between two identical app calls: the audit referenceId (only the first denial per minute gets one)
    and the summary rows' id, which the project-list stored procedure generates with NEWID() on every call."""
    def clean(o, key=None):
        if isinstance(o, dict):
            return {k: clean(v, k) for k, v in o.items() if k != "referenceId" and not (k == "id" and key == "summary")}
        if isinstance(o, list):
            return [clean(x, "summary" if key and key.endswith("Summary") else None) for x in o]
        return o
    return resp[0], json.dumps(clean(resp[1]), sort_keys=True)


real = {k: v["userName"] for k, v in acc["realUsers"].items()}
users = {k: v["userName"] for k, v in acc["users"].items()}
LIST = {"pageNumber": 1, "pageSize": 25}

for role in ["employee", "admin", "site-super-admin", "system-super-admin"]:
    name = real.get(role) or users.get("sec." + role.replace("site-super-admin", "superadmin"))
    if not name:
        continue
    session = login(name)
    token, app_site, app_tz = session["token"], session["siteId"], session.get("siteTimeZone") or ""
    bearer = {"Authorization": "Bearer " + token}
    app = {**bearer, "X-Site-Id": app_site, "X-Site-Timezone": app_tz, "X-Site-Name": session.get("siteName") or ""}

    shapes = {
        "Swagger/Postman: token only (no site header)": bearer,
        "Swagger: token + site ID from login": {**bearer, "X-Site-Id": app_site},
        "Swagger: token + site ID with stray spaces": {**bearer, "X-Site-Id": "  " + app_site + " "},
        "Postman: token + site + timezone (all app headers)": dict(app),
    }
    for path, body in [("/projects/list", LIST), ("/projects/dropdown/list", None), ("/authorization/my-permissions", None)]:
        method = "POST" if body is not None else "GET"
        expected = call(method, path, app, body)
        for label, headers in shapes.items():
            got = call(method, path, headers, body)
            check(f"[{role}] {method} {path} | {label} == app ({expected[0]})",
                  fingerprint(got) == fingerprint(expected), f"app={expected[0]} got={got[0]} {got[1]}")

    # Today's logged failure: the token pasted into the site field. Still denied (never guessed), with a clear message.
    pasted = call("POST", "/projects/list", {**bearer, "X-Site-Id": token}, LIST)
    if role == "system-super-admin":
        # Since 54256d0 a system super admin skips AuthorizationEnforcementFilter (site gate included);
        # an invalid site ID then simply matches no rows.
        check(f"[{role}] token pasted as X-Site-Id -> no rows returned (system admin bypass, 54256d0)",
              pasted[0] == 200 and not pasted[1].get("data"), pasted)
    else:
        check(f"[{role}] token pasted as X-Site-Id -> 403 SITE_NOT_MEMBER, message explains it",
              pasted[0] == 403 and pasted[1].get("code") == "SITE_NOT_MEMBER" and "doesn't contain a site ID" in pasted[1].get("message", ""), pasted)

# Authorized vs unauthorized users, same request shapes.
for who, expect in [("sec.empty", 403), ("sec.hr", None)]:
    s = login(users[who])
    bearer = {"Authorization": "Bearer " + s["token"]}
    app = call("POST", "/projects/list", {**bearer, "X-Site-Id": s["siteId"]}, LIST)
    swg = call("POST", "/projects/list", bearer, LIST)
    check(f"[{who}] project list: Swagger result == app result ({app[0]})", fingerprint(app) == fingerprint(swg), (app, swg))
    if expect:
        check(f"[{who}] no Project permission -> 403 PERMISSION_DENIED in both", swg[0] == 403 and swg[1].get("code") == "PERMISSION_DENIED", swg)

# A site the user doesn't belong to is still denied, header or not.
s = login(real["employee"])
bearer = {"Authorization": "Bearer " + s["token"]}
r = call("POST", "/projects/list", {**bearer, "X-Site-Id": B}, LIST)
check("employee naming a site they don't belong to -> 403 'You do not have access to this site.'",
      r[0] == 403 and r[1].get("message") == "You do not have access to this site.", r)
r = call("POST", "/projects/list", {**bearer, "X-Site-Id": "00000000-0000-0000-0000-000000000000"}, LIST)
check("employee naming a non-existent site -> 403 SITE_NOT_MEMBER", r[0] == 403 and r[1].get("code") == "SITE_NOT_MEMBER", r)
r = call("POST", "/projects/list", {}, LIST)
check("no token at all -> 401", r[0] == 401, r)
r = call("POST", "/projects/list", {"X-Site-Id": A}, LIST)
check("site header but no token -> 401", r[0] == 401, r)
r = call("POST", f"/projects/list?siteId={B}", {**bearer, "X-Site-Id": A}, LIST)
check("query siteId of another site with the site header -> 403 SITE_SCOPE_MISMATCH",
      r[0] == 403 and r[1].get("code") == "SITE_SCOPE_MISMATCH", r)
r = call("POST", f"/projects/list?siteId={B}", bearer, LIST)
check("query siteId of another site with token only -> 403 SITE_SCOPE_MISMATCH (default site can't be steered)",
      r[0] == 403 and r[1].get("code") == "SITE_SCOPE_MISMATCH", r)

# Multi-site user: the header-less default follows the app's current site, including after a switch.
s = login(users["sec.shared"])
bearer = {"Authorization": "Bearer " + s["token"]}
for target in [B, A]:
    sw = call("POST", "/authorization/switch-site", {**bearer, "X-Site-Id": s["siteId"]}, {"siteId": target})
    check(f"[sec.shared] switch-site to {target[:8]} -> 2xx", sw[0] in (200, 204), sw)
    mp = call("GET", "/authorization/my-permissions", bearer)
    check(f"[sec.shared] token-only request now uses site {target[:8]} (as the app would after the switch)",
          mp[0] == 200 and mp[1].get("siteId") == target, mp)
    relog = login(users["sec.shared"])
    check(f"[sec.shared] a fresh login opens the same site {target[:8]}", relog["siteId"] == target, relog.get("siteId"))

# Swagger document.
s, sw = call("GET", "/swagger/v1/swagger.json", {})
schemes = sw.get("components", {}).get("securitySchemes", {})
check("swagger.json: Bearer + optional X-Site-Id + X-Site-Timezone declared",
      schemes.get("Bearer", {}).get("scheme") == "Bearer" and schemes.get("SiteId", {}).get("name") == "X-Site-Id"
      and "Optional" in schemes.get("SiteId", {}).get("description", ""), schemes)

print(f"\n{sum(results)} passed, {len(results) - sum(results)} failed")
