# Meldep Phase 2: Industrial Security Architecture and Implementation Plan

**Status:** finalized and approved for phase-by-phase implementation on 2026-11-03. Updated with the customer's decisions on 2026-11-04. Database changes still need separate approval.

## Decisions of 2026-11-04 (these override earlier sections where they differ)

**Priority:** complete security implementation with existing functionality fully preserved. Existing workflows, business rules and calculations stay as they are. Code changes are limited to what security needs.

| # | Decision | How the plan applies it |
|---|---|---|
| 1 | Full role, group and individual-user permissions | Option B in Section 4B.2. Exact schema, reasons and rollback are in `Proposed-Database-Changes.sql`. **Nothing is created until separately approved.** The code path (`IPermissionSource`, precedence rules) already exists; only the two new sources and admin screens remain |
| 2 | Keep the email leave workflow, and close the open API | The email link (`/approve-leaves?id=`) already opens a login-protected page, and no screen calls the API anonymously. So the anonymous `GET` and `PUT` become login-only, with no change to the email. Approve, decline and forward also require approval rights or being the assigned approver |
| 3 | Don't assume production status | `Deployment-and-Secret-Rotation-Plan.md` lists every production-sensitive change, with a safe order and rollback |
| 4 | Implement all security requirements | Sections 5 to 8, plus the additions below |
| 5 | Preserve functionality | New protections default to Audit or off where they could change behavior, with config rollback |
| 6 | Secret rotation plan, no values exposed | `Deployment-and-Secret-Rotation-Plan.md` |
| 7 | Complete approved phases | Everything not needing a database change is implemented now. The rest is listed for approval |

**Additions to the plan**

| Area | Approach | Default on deployment |
|---|---|---|
| Centralized exception handling | One `SafeErrorMessage` helper replaces every `ex.Message` returned to clients. HTTP status and response shape stay the same. Known business messages still show, and everything else shows a reference ID. Details go to the log | Active |
| User activity and audit log | Uses the existing `Log` table and its `UserId`, `IpAddress` and `PageUrl` columns: logins, password resets, site switches, denials, and every write action (who, where, result; never request bodies). Written separately, so a log row can't save a request's unfinished changes | Active, can be turned off in config |
| Session security | `POST /auth/refresh` keeps active users signed in, which allows a much shorter JWT lifetime with no new table. `POST /authorization/revoke-sessions` ends all of a user's sessions | Lifetime unchanged until `JwtTokenConfig:ValidFor` is set |
| Password policy | Minimum length raised from 6 to 8 for new passwords only, matching what the reset screen already requires. Existing passwords keep working | Active |
| Secure Azure Blob access | `BlobAccess:Mode = Signed` adds short-lived read signatures to file URLs in API responses, with no change to stored data or screens. The container's public access is then turned off in Azure | `Public` (today's behavior) until switched |
| Linked IDs | `ProjectId`, `RequirementId` and similar in any request are checked against the active site, in one central filter | Audit |
| More record rules | `[RecordScope]` extended using each module's own existing list rule | Audit |
| Webhooks | Inbound help-desk email endpoints can require the service key | Enforced only with `EnforceAllLinks` |
| SOP assistant | The n8n webhook call moves behind an authenticated API endpoint | Active |
| Browser headers | Content Security Policy in report-only mode for the Vue app, so violations are visible without blocking anything | Active (reports only) |

**Required database and data changes (for separate approval):** see `Proposed-Database-Changes.sql`.

| ID | Change | Needed for | Type |
|---|---|---|---|
| D1 | `SecurityGroups`, `SecurityGroupMembers`, `SecurityGroupMenuPermissions` | Real group permissions and group deny | 3 new tables |
| D2 | `UserMenuPermissions` | Individual-user permissions and user deny | 1 new table |
| D3 | `ModulesMenus` rows for pages with no menu | View and Manage on those pages (77 write actions can only require site membership today) | Data only; candidates listed for review |
| D4 | Encrypt stored credentials: `Domain`, `Server`, `EmailAccount`, `Infra_ProjectInstance_Role_Users`, `ReportSettings`, `TwilioEmailSettings` | Plaintext secrets at rest | Data rewrite plus code; separate project |
| D5 | Fix records for the 12 users with no valid site | Enforcing the site rule without locking them out | Data only; needs business review |

## Requirement coverage

| Requirement | How it is met | Section | Status under the current database |
|---|---|---|---|
| Application-wide role-based authorization | Role → menu permissions through the central `PermissionService` | 4.4, 4B | Available |
| Group-based authorization | A site role acts as a group (Option A); real groups with Option B | 4B.2 | Available through roles; real groups need database approval |
| Individual user permissions | A one-person role for rare exceptions (Option A); direct grants and deny with Option B | 4B.2, 4B.3 | **Limited.** Full support needs database approval |
| Module, menu, View and Manage permissions | Tiers, `[RequireMenu]`, `[ReadAccess]`, `[ManageAccess]`, per menu | 4.5, 4B.4 | Available |
| Multi-site and tenant isolation | Site gate, site-scope filter, `[SiteOwned]` | 4.3, 4A | Available |
| Record-level data security | `[SiteOwned]` and `[RecordScope]`, reusing existing rules | 4B.5 | Available |
| Vue route protection and backend API authorization | `permissionGuard`, route metadata, global filter | 4.5, 4.7 | Available |
| Direct URL and Postman protection | Route guard for URLs; API filter for all direct calls | 4A | Available |
| Custom 404 - Unauthorized Access page | `not_authorized.vue` rendered through the catch-all route | 4.7, 4A | Available |
| Centralized authorization on all protected controllers and APIs | Global filter, deny by default, startup scan, all 91 controllers classified | 4A, 4B.6 | Available |
**Companion document:** `Customer Security/Phase-1-Current-Security-Analysis-Plan.md` holds the full gap analysis, audit SQL and controller-to-menu mapping. This plan builds on it and corrects it where noted.

## 1. Context

Meldep hides menus by role, but the server does not enforce those permissions. A logged-in user can open restricted pages by URL, call any API directly, and select any site by changing a header. Several anonymous endpoints also allow account takeover.

**Target outcome:** every API request in the application is authorized on the server, per site, and denied by default, using View and Manage permissions drawn from roles and groups. Vue mirrors those rules for usability, and users without permission see a custom "404 - Unauthorized Access" page.

**Read Section 4B first.** The database has no general security groups and no per-user menu permissions, so that requirement can't be fully met without a database change. Section 4B explains what works today and what needs your separate approval.

**Scope limits**
- No test framework. Phase 1's `Vsky.Api.Tests` is dropped, and a startup scan plus a manual checklist take its place (Sections 4.5 and 11).
- **The database structure stays unchanged:** no new tables, columns, indexes or migrations. No permission data is rewritten either (see the legacy-row rule in Section 4.4).
- **Business logic stays unchanged:** workflows, calculations, and project and owner filters are not altered. Requests that are valid today keep working, except where authorization now denies them. The Section 6 security fixes change behavior only for invalid or malicious requests. Decisions 6, 7 and 8 would change user-visible behavior, so they apply only if you approve them.

**Evidence:** code was read or searched, but nothing was run and no database was queried. Items marked *Inferred* are not confirmed by running the code. `ModulesMenus.Link` values are not in the repo, so they must come from Audit Query 1.

## 2. Current architecture, as verified

| Area | Actual state | Evidence |
|---|---|---|
| Tenant model | **Site is the tenant.** No `TenantId` or parent level exists, so "multi-tenant" and "multi-site" are one boundary | Search found no matches |
| Site selection | Client header copied before authentication, with no validation | `GlobalVariableMiddleware.cs:21`, `Program.cs:663-666` |
| Authentication | Identity + JWT (HS256). Validation is sound, and inactive users are rejected on each request. 30-day expiry, no refresh or revocation | `Program.cs:263-299`, `JwtIssuerConfig.cs:16` |
| Token claims | `sub`, email and roles. **Roles are not site-scoped, and there is no site claim** | `AuthController.cs:98-109` |
| Authorization | `[Authorize]` on `BaseController` only. No policies, handlers or filters | `BaseController.cs:12-14` |
| Permission use | `IsShowMenu` filters menus. `IsView` and `IsManage` are not enforced | `SitesModulesService.cs:84-90` |
| Data isolation | Manual `SiteId` filters only, no global filter, and many lookups by ID only | Search; `IssueService`, `ExpenseService:209` |
| Write path | `EfRepository` calls `Update()` and `SaveChanges()` on each call | `EfRepository.cs:87-88,115-116` |
| Vue guards | Two guards check only that a token exists. No route has permission metadata | `router/index.js:173-208`, `boot/auth.js:6-38` |
| Vue error handling | The 401 redirect fails (`logout()` returns nothing). No 403 handling | `boot/interceptors.js:56-64`, `stores/auth.js:130` |
| Vue state | Token and user in LocalStorage, read per request, so tabs can silently change site | `boot/axios.js:29-41` |

### Permission and shared-user relationships

```
AspNetUsers ── PersonId ──> Person ──< PersonSitesMapping(SiteId, IsSharedUser, LastUsed, Deleted)   = membership
AspNetUsers ──< AspNetUserRoles(UserId, RoleId, SiteId)  = role in that site
AspNetRoles ──< SitesRoles(SiteId, RoleId)
SitesRoles.Id = SitesModulesMenusPermissions.SiteRoleId (IsShowMenu, IsView, IsManage)
SitesModulesMenus(SiteId, SiteModuleId, MenuId) ──> ModulesMenus(Link)   SitesModules ──> Modules
```

**A shared user** is one login with several `PersonSitesMapping` rows and separate role rows per site. Sharing therefore creates the same access records as normal membership, and `IsSharedUser` only affects list filtering today.

**Rule for this plan:** `IsSharedUser` never grants or removes rights. A shared user passes the same gate as any member, using only their roles in the active site. Securing the sharing endpoints is essential, because they create the records the gate trusts.

## 3. Critical findings driving the order of work

**From Phase 1, reconfirmed:** forged Microsoft login (`AuthController:319-363`), password reset that ignores the token (`:464-473`), anonymous user lookup exposing the password hash (`:442-450`), forged-token profile lookup (`:131-136`), anonymous `RolesController`, and unprotected permission writes.

**New in Phase 2**

| # | Finding | Location | Impact |
|---|---|---|---|
| N1 | `CreateSiteShareUser` has no caller-rights check, takes `SiteId` from the body, and resolves `RoleIds` with no site filter | `PersonSitesMappingController.cs:144,178-181` | **Any logged-in user can add a user, including themselves if they exist elsewhere, to any site with chosen roles.** This would defeat a site gate |
| N2 | `DeleteSiteShare` loads the mapping by ID only, while removing roles from the header site | `:237-256` | Cross-site removal of access, and orphaned roles that a later re-share could revive (*Inferred*) |
| N3 | `UsersController` resets a password by user ID with no visible authorization | `UsersController.cs:489-501` | Account takeover by any logged-in user |
| N4 | `CalendarController` fetches a client-supplied URL anonymously | `CalendarController.cs:48-58` | Server-side request forgery (SSRF) |
| N5 | About 215 `v-html` uses with no sanitizer, while the JWT is in LocalStorage | Vue sweep; includes public form input in `contactUsList.vue:187,190` | Stored XSS leading to session theft |
| N6 | Secrets exposure: tracked AI key values (`env.*.cjs`, comment at `layout.vue:724`), tracked default encryption key (`EncryptionService.cs:92`), real-looking credentials in `appsettings.json` | Values deliberately omitted | Rotation required |

**Assume compromise.** If this code is deployed, the takeover paths may already have been used, and a forged token could stay valid for 30 days. Fixing the code alone doesn't remove that risk, so Phase 2.0 includes rotating the JWT key and reviewing logs.

## 4. Target architecture

### 4.1 Security layers

| Layer | Control | Trust |
|---|---|---|
| 1. Edge | HTTPS, CORS allowlist, rate limits, security headers, production environment settings | Baseline |
| 2. Authentication | JWT validation, active user check, security stamp, shorter lifetime | **Enforced** |
| 3. Tenant gate | `X-Site-Id` validated on every request | **Enforced** |
| 4. Function | Global filter + `PermissionService`, deny by default | **Enforced** |
| 5. Data | Site-scope filter, `[SiteOwned]` record check, scoped lookups | **Enforced** |
| 6. Vue | Route guard, `usePermission`, 404 page | **Usability only, not a security boundary** |
| 7. Audit | Denials, bypasses, logins, permission changes | Detection |

### 4.2 Request flow

```
Request → routing → JWT validation (401) → AuthorizationEnforcementFilter
  1. Read the tier from endpoint metadata (none: deny)
  2. Tier needs a site? Validate X-Site-Id (403 SITE_NOT_MEMBER)
  3. Tier needs a menu? PermissionService.Evaluate(user, site, links, level) (403 PERMISSION_DENIED)
  → model binding → SiteScopeActionFilter (body/query/route SiteId must match, else 403)
  → SiteOwnedFilter (record in another site: 404) → action
```

The filter runs before binding, so denied requests never reach action code or the database write path.

### 4.3 Site gate (one rule, used everywhere)

A user may act in a site only if **all** of these hold:
1. The site exists, is active, and is not deleted.
2. A non-deleted `PersonSitesMapping` row links the user's person to the site.
3. At least one `AspNetUserRoles` row exists for that user and site, where the `AspNetRoles` row is not deleted and a matching `SitesRoles` row is not deleted.

Empty, malformed or unknown site IDs are denied. `my-share-sites-list`, login, and `switch-site` return or select only sites that pass this gate. This also removes broken choices such as role-less shares.

**`GlobalVariable` stays.** Its 570 uses don't change, because the filter validates the value before any action reads it. On T0/T1 endpoints the site value is blanked, so anonymous callers can't choose a site by header.

### 4.4 Permission evaluation

The rules come from Phase 1, Sections D and G:
- Module, menu and role must be valid. `Link` matches exactly after normalization, never with `StartsWith` or `Contains`.
- Several roles combine as a per-flag union. Conflicting duplicate rows for the same role fail closed and are logged. A missing row means deny.
- View needs `IsView` or `IsManage`. Create, update, delete, approve and import need `IsManage`. Navigation needs `IsShowMenu` plus View or Manage.

**Legacy-row rule (replaces the Phase 1 backfill).** `assignRolesToMenu` writes `IsView=false, IsManage=false`, and the live admin screen can't set them, so many rows are probably `1/0/0` (*Inferred; Audit Query 3 confirms*). Enforcing those flags literally would lock those users out, and rewriting them would change your data. With no data change, the config switch `Authorization:LegacyShowOnlyAccess = true` handles this:

| Show / View / Manage | View | Manage | Meaning |
|---|---|---|---|
| 1 / 0 / 0 | Yes | Yes | Not yet configured, so today's access is kept |
| 0 / 0 / 0, no row, or deleted row | No | No | **No permission** |
| Any row with View or Manage set | As set | As set | Deliberately configured |

This keeps today's access for those rows, but it is **not least privilege**: a `1/0/0` row means full access until an admin sets View or Manage on it through the new toggles. The switch is turned off once those rows are configured.

Permissions stay out of the JWT so revocation works. The service is the only decision logic, and the menu API and Vue permission API use it too.

### 4.5 Backend components

**Changes from the Phase 1 baseline**

| Phase 1 proposal | Phase 2 decision | Reason |
|---|---|---|
| One attribute on each of 830 actions | **Controller-level default with action overrides.** GET means View, other verbs mean Manage, and `[ReadAccess]` marks POST reads | Less manual work. A wrong POST default fails closed, and Audit mode reveals it. Mixed controllers such as `ProjectsController` stay per-action |
| ASP.NET requirement + handler | **Removed.** The filter calls the service directly | Adds a layer with no benefit here |
| `xUnit` reflection test | **Startup scan** with `IActionDescriptorCollectionProvider` | Testing framework excluded |
| EF interceptor for cache invalidation | **Per-site generation counter**, updated explicitly by the admin write paths, plus a 60-second TTL | A global bump would flush on every `LastUsed` write |
| `SiteWriteGuardInterceptor` | **Replaced by `[SiteOwned]` and scoped lookups** | `EfRepository.Update` marks entities from no-tracking reads as modified, so the interceptor can't see the database's `SiteId`, and partial commits would occur |
| 403 redirects to the page | **The route guard alone decides navigation** | One widget's 403 must not eject the user from a page |

**Endpoint tiers.** The most specific attribute wins, and an explicit tier outranks `[AllowAnonymous]`.

| Tier | Attribute | Requirement | Examples |
|---|---|---|---|
| T0 | `[AllowAnonymous]` (reviewed allowlist) | None | `auth/login`, public website forms |
| T1 | `[ServiceKey]` **NEW** | Shared secret from config | `cron-job/*`, email webhooks |
| T2 | `[AuthenticatedOnly]` **NEW** | Valid user | `switch-site`, `my-permissions`, own profile |
| T3 | `[SiteMember]` **NEW** | T2 + site gate | Shared dropdown reads, notifications, dashboard shell |
| T4 | `[RequireMenu(links...)]` **NEW**, with `[ReadAccess]` / `[ManageAccess]` | T3 + menu permission | Most business APIs |
| T5 | `[RequireSiteAdmin]`, `[RequireSystemAdmin]` **NEW** | T3 + admin role **in this site** | Roles, permissions, modules, sites, sharing |

**`[Authorize(Roles = ...)]` must not be used.** Role claims are not site-scoped, so an admin on site A would pass on site B.

**Modes:** `Authorization:Mode = Off | Audit | Enforce`, plus `EnforcedLinks` for gradual rollout. Audit mode logs what would be denied and allows the request. Rollback is a config change.

**Startup scan:** lists every action without a tier.
- Audit mode logs a warning.
- Enforce mode stops startup in Development and Staging.
- Enforce mode in Production logs a critical error and denies those actions at request time. It doesn't stop the app, because that would turn one missed attribute into a full outage.

**`[SiteOwned(typeof(Issue))]`** gives central protection against cross-site record access (IDOR) on routes with an `{id}`:
- It reads the record's `SiteId` through `EF.Property` and returns 404 on a mismatch, so existence isn't revealed.
- It doesn't require rewriting each service.
- Child entities without `SiteId` (`Expense_Lines`, `ProjectFiles`, `EmployeeLeave`, `HelpDeskFiles`) and IDs in request bodies need explicit scoped lookups through the parent.

**No EF global query filter for now.** It could silently empty results for cron, anonymous and super-admin flows. Revisit after Phase 2.8.

**Denial response:** `{ code, message, referenceId }` with a real HTTP status. Messages don't reveal menu names or which check failed.

**Audit logging:** use the existing `Logs` table, with JSON in `FullMessage`: user, site, endpoint, decision and reason. It has no site column, so per-site reports need to search that JSON.

### 4.6 Shared-user and site-switch design

| Scenario | Design |
|---|---|
| Share a user | T5 on the **header site**. Ignore `model.SiteId` unless System Super Admin. Filter `RoleIds` by `SiteId` and not deleted, and reject a count mismatch. **Callers can't grant a role above their own.** Require at least one role |
| Remove a share | Load by ID **and** active site, using the mapping's own person and site. Confirm `IsSharedUser`. Clear the cached permissions |
| Switch site | New atomic `POST /authorization/switch-site` (T2): validates the target with the gate, derives identity from the JWT, updates `LastUsed`, and returns site data with roles for that site. It doesn't depend on the current header, so a user whose current site was revoked can still recover |
| Login | One site resolver for all login paths: `LastUsed` if it passes the gate, else the oldest passing site in a fixed order, else a "no site access" response |
| Multiple tabs | Vue pins the user and site in memory at boot. A `storage` listener reloads other tabs when either changes, so a write can't silently land in another site |
| Stale landing page | `last_route` is stored per user and site, and checked by the guard before use |
| Role claims in JWT | Stop issuing them after the SSO check (Decision 8). `GetLoggedInUserRole` has no callers |

### 4.7 Frontend design

**Single guard.** Replace the two existing guards with `router/permissionGuard.js`, which runs in this order:
1. Handle a `?token=` SSO link, as limited by Decision 8.
2. Routes with `public: true` pass. This includes `/auth/*`, `/approve-leave` per Decision 7, and website routes.
3. No token or an expired token: go to login with a validated same-origin `redirect`.
4. Await the permission store. **If the fetch fails, show a retry state, never allow.**
5. Check **every** record in `to.matched`, because merged `meta` would let a child override its parent.
6. No declaration on a protected route: deny.
7. Denied: record the denied path in the permission store and navigate by name to the existing catch-all route, which keeps the URL. That route renders the 404 page.

**No data flash:** pages are lazy-loaded, and the page component and its API calls don't start until the guard passes.

**Route metadata (72 `routes.js` files, about 128 leaf routes)**
```js
meta: { requiresAuth: true, permission: { menu: MenuLinks.ISSUE, level: "view" } }
meta: { requiresAuth: true, siteMember: true }  // dashboard, profile
meta: { public: true }                          // explicit
```
Detail and child routes declare their parent's link. For example, `/customer/customer-center` uses `/customer`.

**Same rollout switch as the API.** `my-permissions` returns the mode and enforced links. In Audit mode the guard only logs, so the UI and API change together and share the same rollback.

**Permission store:** `stores/permissions.js`, memory only, keyed by user and site. It is cleared on logout and site switch, and refreshed after a 403. `usePermission().can(link, level)` and a small `v-can` directive gate buttons, starting with Add, Edit, Delete and Export on admin, finance and HR pages. The 184 hardcoded role-check lines migrate gradually. **That migration isn't a security requirement**, since the API enforces access.

**API error handling (`boot/interceptors.js`)**

| Response | Behavior |
|---|---|
| 401 | Single logout, then login with `redirect` (uses the boot `router`, fixes the current bug) |
| 403 `SITE_NOT_MEMBER` | Reload the site list and move the user to a valid site, or to the "no site access" state |
| 403 `PERMISSION_DENIED` | Debounced permission refresh. **Redirect only if the current route is now denied.** User actions get one toast, and widgets show a denied state. Background calls never navigate |
| 404 | Handled by the calling page as "not found" |

**Custom page: `pages/not_authorized.vue`** (restyled with the `vsky-ui-standards` skill)

| User state | Result |
|---|---|
| Not logged in, protected URL | Login, then back to the requested URL |
| Logged in, no permission | **"404 - Unauthorized Access"** as a standalone page, so `layout.vue` makes no API calls behind it |
| Logged in, unknown URL | Existing catch-all page `pages/error.vue`, with its null-crash fixed |

The page keeps the browser URL, so a refresh re-evaluates access after permissions change. Its Home button goes to a landing page the user **can** open, never to a stale `last_route`. It shows no module names or reasons. The HTTP layer still returns real 401/403/404 codes, and "404" is the user-facing wording.

**Direct URL prevention, summarized:** URL entry, bookmark and refresh all pass through the guard. Calls from Postman or DevTools skip Vue, and the API filter stops them.

## 4A. Worked scenario: Dashboard allowed, Test Case denied

**The user:** logged in, active site = Site A. Their roles in Site A have a permission row for `/dashboard`, but no valid row for `/test-case`. `/test-case` is the expected `ModulesMenus.Link`, to be confirmed by Query 1.

**The code today** (read directly)
- Route: `modules/test-case/routes.js:3-6`, with `requiresAuth` only.
- API: `TestCaseController` inherits only `[Authorize]`, so any logged-in user can call it.
- `GetTestCaseById` filters on ID only (`TestCaseService.cs:437-439`), and `DeleteTestCase` deletes whatever it loads (`TestCaseController.cs:684-690`). **Any logged-in user with an ID can delete a test case in any site today.**

### Question 1: the user types `/test-case` in the browser

```
Browser loads the app at /test-case
  → permissionGuard (router beforeEach)
      token present and not expired? yes
      await permissionStore.load()  → GET /authorization/my-permissions  (X-Site-Id: A)
                                    ← { siteId:"A", mode:"Enforce", enforcedLinks:[...],
                                        menus:[{ link:"/dashboard", show:true, view:true, manage:false }] }
      every record in to.matched checked:
        meta.permission = { menu:"/test-case", level:"view" }
        can("/test-case","view") → no entry → false
      permissionStore.deniedPath = "/test-case"
      next({ name:"catch-all", params:{ catchAll:["test-case"] }, replace:true })
  → pages/error.vue sees deniedPath matches → renders not_authorized.vue
```

```js
// router/permissionGuard.js (outline)
if (to.matched.some(r => r.meta.public)) return next();
if (!auth.hasValidToken()) return next({ path: "/auth/login", query: { redirect: to.fullPath } });
try { await permissions.load(); } catch { return next({ name: "catch-all", params: retryParams(to) }); }
const required = to.matched.map(r => r.meta.permission).filter(Boolean);
const declared = required.length || to.matched.some(r => r.meta.siteMember);
const allowed = declared && required.every(p => permissions.can(p.menu, p.level));
if (allowed || !permissions.isEnforced(required)) return next();   // Audit mode logs only
permissions.deniedPath = to.path;
return next({ name: "catch-all", params: { catchAll: to.path.slice(1).split("/") }, replace: true });
```

**Result**
- The browser shows the custom page, and the address bar still says `/test-case`.
- `pages/index.vue` for Test Case never mounts, because Vue Router resolves lazy components after `beforeEach`. So `POST /test-case/list` is never sent, and no data flashes.
- Refreshing runs the same check, so access appears as soon as it is granted.
- "Go Home" sends the user to a page they can open, here `/dashboard`.

The Test Case JavaScript file is still downloadable by anyone, and a user can tamper with browser state to get past this guard. **That is why this check is only for usability.** Tampering gets the user an empty screen, because every API call still fails as described next.

### Question 2: the user calls the API from Postman

**How the controller declares it**
```csharp
[Route("test-case")]
[RequireMenu(MenuLinks.TestCase)]        // default: GET = View, other verbs = Manage
public class TestCaseController : BaseController
{
    [HttpPost("list"), ReadAccess] ...                    // POST that only reads
    [HttpGet("details/{id}"), SiteOwned(typeof(TestCase))] ...
    [HttpDelete("{id}"), SiteOwned(typeof(TestCase))] ... // Manage by default
}
```
These are attributes only. The action bodies don't change.

**How a request is rejected:** `POST /test-case/list` with a valid token and `X-Site-Id: A`.
```
1. Routing matches TestCaseController.GetAll..., whose metadata holds [RequireMenu("/test-case")]
2. JWT bearer validates the signature, expiry and active user (existing Program.cs:263-299)
3. AuthorizationEnforcementFilter runs BEFORE model binding:
```
```csharp
public async Task OnAuthorizationAsync(AuthorizationFilterContext ctx)
{
    var tier = EndpointTier.Read(ctx.ActionDescriptor);          // most specific attribute wins
    if (tier == null) { Deny(ctx, 403, "PERMISSION_DENIED"); return; }  // unclassified = deny
    if (tier.IsPublic) { _globalVariable.SiteId = ""; return; }
    if (tier.IsService) { CheckServiceKey(ctx); return; }
    if (!ctx.HttpContext.User.Identity.IsAuthenticated) { Deny(ctx, 401, "UNAUTHENTICATED"); return; }
    if (tier.IsAuthenticatedOnly) return;

    var userId = ctx.HttpContext.User.GetLoggedInUserId<string>();   // from the token, never the request
    var snapshot = await _permissions.GetSnapshot(userId, _globalVariable.SiteId);
    if (!snapshot.IsSiteMember) { Deny(ctx, 403, "SITE_NOT_MEMBER"); return; }

    var level = tier.ResolveLevel(ctx.HttpContext.Request.Method);   // [ReadAccess], [ManageAccess] or verb default
    if (!snapshot.Allows(tier.Links, level)) Deny(ctx, 403, "PERMISSION_DENIED");
    // Deny logs user, site, endpoint and reason; in Audit mode it logs and allows
}
```
The API never looks at Vue, the referrer or the browser. A Postman call and a browser call take the same path.

**The permission check itself** (`PermissionService.GetSnapshot`, using only existing tables)
```csharp
// 1. Site gate
var site     = await db.Sites.AnyAsync(s => s.Id == siteId && s.Active && !s.Deleted);
var member   = await db.PersonSitesMapping.AnyAsync(p => p.PersonId == personId && p.SiteId == siteId && !p.Deleted);
var siteRoles = await (from ur in db.UserRoles
                       where ur.UserId == userId && ur.SiteId == siteId      // this site's roles only
                       join r  in db.Roles on ur.RoleId equals r.Id where !r.Deleted
                       join sr in db.SitesRoles on ur.RoleId equals sr.RoleId
                       where sr.SiteId == siteId && !sr.Deleted
                       select sr.Id).ToListAsync();
// IsSiteMember = site && member && siteRoles.Any()

// 2. Permission rows, valid for this site only
var rows = await db.SitesModulesMenusPermissions
    .Where(p => p.SiteId == siteId && !p.Deleted && siteRoles.Contains(p.SiteRoleId)
             && p.SitesModulesMenus.SiteId == siteId && !p.SitesModulesMenus.Deleted
             && !p.SitesModulesMenus.ModulesMenus.Deleted)
    .Select(p => new { p.SitesModulesMenus.ModulesMenus.Link, p.SiteRoleId, p.IsShowMenu, p.IsView, p.IsManage })
    .ToListAsync();
// 3. Group by normalized Link; union flags across roles; duplicate rows for one role fail closed
// Allows(links, level): any listed link grants the level; no entry = false
```
Property names are illustrative and will be matched to the entities during implementation. The result is cached for 60 seconds per user and site.

**Expected responses for Test Case APIs**

| Request | Response | Does the action run? |
|---|---|---|
| No token, bad signature, expired token, or inactive user | **401** | No |
| Valid user, `X-Site-Id` missing, malformed, or a site they don't belong to | **403** `SITE_NOT_MEMBER` | No |
| Member of the site, no Test Case permission (any verb) | **403** `PERMISSION_DENIED` | No |
| View only: `POST /test-case/list`, `GET /test-case/details/{id}` | **200** | Yes |
| View only: `POST`, `PUT` or `DELETE /test-case...` | **403** `PERMISSION_DENIED` | No |
| Has permission, record belongs to another site or doesn't exist | **404** | No |
| Has Manage, record in this site: `DELETE /test-case/{id}` | **204**, as today | Yes |

```json
HTTP/1.1 403 Forbidden
{ "code": "PERMISSION_DENIED", "message": "You do not have permission to perform this action.", "referenceId": "..." }
```

### Question 3: the user changes the request

| The user changes | Example | What stops it | Response |
|---|---|---|---|
| Vue state | Edits LocalStorage `roles` or the permission store | The API ignores client role data | 403 |
| JWT contents | Adds an admin role | Signature check. Authorization doesn't read role claims | 401 |
| URL | Other path or casing; `/test-case/../issue/list` | The check uses the attributes on the **endpoint that was matched**, never URL text. Each endpoint has its own rule, and no match means 404 | Per endpoint |
| `X-Site-Id` to a site they don't belong to | `X-Site-Id: B` | Site gate, on every request | 403 `SITE_NOT_MEMBER` |
| `X-Site-Id` to a site they do belong to | Shared user selects B | Allowed, and permissions and data come from B only | Per B's rules |
| Header removed | No `X-Site-Id` | Site gate | 403 |
| `siteId` in body, query or route | `{ "siteId":"B" }` | `SiteScopeActionFilter` compares it with the validated site. This also blocks a `SiteId` being written through a mapped model on update | 403 |
| Record ID in the route | `GET /test-case/details/{B's id}`, `PUT`, `DELETE` | `[SiteOwned(typeof(TestCase))]` reads the record's `SiteId` before the action runs | 404 |
| Record IDs in the body | `history-by-testCaseIds` with B's IDs | `[SiteOwned(typeof(TestCase), BodyList = "TestCaseIds")]`: every ID must belong to the site | 404 |
| Linked IDs in the body | Create or update with `ProjectId`, `PlanId` or `ProjectModuleId` from B | `[SiteOwned(typeof(Project), Body = "ProjectId")]`, and the same for the others | 404 |
| Child ID with no `SiteId` | `change-log?mappingId=`, `updateTestCaseStatus/.../{mappingId}` | Scoped lookup through the parent (`mapping.TestCase.SiteId == siteId`) in the service | 404 or empty result |

The filter returns 404 for both "other site" and "doesn't exist", so an attacker can't test whether an ID exists. Today a missing test case returns 400, so this one response changes.

**What this doesn't cover**
- **Within one site:** `[SiteOwned]` alone doesn't stop a user with Test Case permission opening another project's test case by ID. `GetAllTestCases` filters the list by project and role (`TestCaseService.cs:85-103`), but the by-ID lookups don't. Section 4B.5 closes this by applying the same existing rule to by-ID requests.
- **Public file URLs:** files in public Azure blobs bypass the API altogether (Decision 9).

### Question 4: a shared user

**Example:** one login, with Site A as home and Site B shared.

| Check | Site A selected | Site B selected |
|---|---|---|
| `PersonSitesMapping` row | Yes (`IsSharedUser=false`) | Yes (`IsSharedUser=true`) |
| `AspNetUserRoles` for this site | QA Engineer | Viewer |
| Permission row for `/test-case` | View + Manage | None |
| Vue on `/test-case` | Page opens | 404 page |
| `POST /test-case/list` | 200, Site A data | 403 `PERMISSION_DENIED` |

**How it works**
1. The JWT holds no site or role decisions, so the same token works for both sites and can't carry Site A's rights into Site B.
2. The selected site arrives in `X-Site-Id` and is validated on every request.
3. The query only uses roles where `ur.SiteId == siteId`, then only `SitesRoles` and permission rows for that same site. A Site A role can't count in Site B.
4. The cache key includes user and site, so the two sites never share results.
5. `IsSharedUser` is never read in the decision.

**Switching site**
```
user_info.vue → POST /authorization/switch-site { siteId:"B" }    (identity comes from the JWT)
  API: site gate on B fails → 403, nothing changes
  API: passes → LastUsed updated, site data and B's roles returned
Vue: updates stored user → clears permission store → full page load of B's landing page
Other open tabs: storage event → reload with the new site
```

**If the share is removed mid-session:** the next request to Site B is denied within 60 seconds, or immediately on the same server, because removing a share clears that cache. Vue then moves the user to another valid site.

### Question 5: files involved

**Backend, new**

| File | Type | Role in this scenario |
|---|---|---|
| `API/Vsky.Api/Authorization/AuthorizationEnforcementFilter.cs` | Global authorization filter | Returns the 401/403 before binding |
| `API/Vsky.Api/Authorization/Attributes.cs` | Attributes | `[RequireMenu]`, `[ReadAccess]`, `[ManageAccess]`, `[SiteMember]` and others |
| `API/Vsky.Api/Authorization/MenuLinks.cs` | Constants | `MenuLinks.TestCase = "/test-case"` |
| `API/Vsky.Api/Authorization/SiteScopeActionFilter.cs` | Global action filter | Rejects a mismatched `siteId` |
| `API/Vsky.Api/Authorization/SiteOwnedFilter.cs` | Action filter + attribute | Record and linked-ID checks |
| `API/Vsky.Api/Authorization/EndpointClassificationScan.cs` | Startup service | Finds unclassified actions |
| `API/Vsky.Api/Authorization/AuthorizationOptions.cs` | Config | Mode, `EnforcedLinks`, `LegacyShowOnlyAccess` |
| `API/Vsky.Services/Authorization/PermissionService.cs` (+ interface, snapshot) | Service | Site gate and permission decision |
| `API/Vsky.Api/Controllers/AuthorizationController.cs` | Controller | `my-permissions`, `switch-site` |

**Backend, modified**

| File | Change |
|---|---|
| `API/Vsky.Api/Program.cs:165` | `options.Filters.Add<AuthorizationEnforcementFilter>()` and `SiteScopeActionFilter`; register the service |
| `API/Vsky.Api/Controllers/TestCaseController.cs` | Attributes only |
| `API/Vsky.Api/Controllers/ProjectDashboardController.cs:248`, `RequirementDashboardController.cs:59` | Declare their own links (`testCasesList`, `test-case-list`), per Decision 14 |
| `API/Vsky.Services/TestCases/TestCaseService.cs:610` | Site filter on `GetStatusChangeLog`, for the mapping lookup |
| `API/Vsky.Services/GlobalVariables/GlobalVariableMiddleware.cs` | **Unchanged.** It still copies the header, and the filter validates it before any action reads it |
| `API/Vsky.Api/Controllers/BaseController.cs` | **Unchanged.** `[Authorize]` stays as a second layer |
| `TestCase` entity and all database tables | **Unchanged** |

**No new middleware is added.** Middleware can't easily see action attributes, and a filter runs at the right point for both the check and the audit log.

**Frontend**

| File | Status | Role |
|---|---|---|
| `WEB/src/router/permissionGuard.js` | New | Decides navigation |
| `WEB/src/stores/permissions.js` | New | Holds `my-permissions`, `can()`, `deniedPath` |
| `WEB/src/composables/usePermission.js`, `directives/can.js` | New | Hide Add, Edit and Delete buttons |
| `WEB/src/security/menuLinks.js` | New | `TEST_CASE: "/test-case"` |
| `WEB/src/modules/test-case/routes.js:6` | Modified | `meta.permission: { menu: MenuLinks.TEST_CASE, level: "view" }` |
| `WEB/src/modules/test-case/pages/index.vue` | Modified | `v-can` on action buttons |
| `WEB/src/router/index.js`, `boot/auth.js` | Modified | One guard only |
| `WEB/src/router/routes.js:14` | Modified | Name the catch-all route |
| `WEB/src/pages/error.vue`, `pages/not_authorized.vue` | Modified | Render the custom page |
| `WEB/src/boot/axios.js`, `boot/interceptors.js` | Modified | Pinned site; 401/403 handling |
| `WEB/src/shared/user_info.vue`, `stores/auth.js` | Modified | Site switch, clear permissions |

### Every protected API, not only Test Case

1. **It applies automatically.** The filter is registered once in `AddControllers` (`Program.cs:165`), so it runs on all 830 actions in 91 controllers, and on any added later. No controller opts in.
2. **Deny by default.** An action with no tier gets 403 in Enforce mode, so a forgotten attribute closes access instead of opening it.
3. **Each controller declares once.** A class-level attribute covers the controller, and actions override only where needed:

| Controller | Declaration | Exceptions |
|---|---|---|
| `IssueController` | `[RequireMenu(MenuLinks.Issue)]` | `POST list` is `[ReadAccess]`, and also accepts `/project-center` |
| `TimesheetController` | Per action | `/timesheet`, `/weekly-timesheet`, `/weekly-timesheet-approval` |
| `RolesController`, `ModulesMenusController` | `[RequireSiteAdmin]` | None |
| `CommonController` | `[SiteMember]` | None |
| `CronjobController` | `[ServiceKey]` | Replaces class-level `[AllowAnonymous]` |

4. **Side doors are found, not assumed.** Test Case data is also reachable from `POST /project-dashboard/testCasesList` and `POST /requirement-dashboard/test-case-list`, and the release tracking screens call `history-by-testCaseIds`. Each endpoint gets its own rule, and Audit logs show any others that real usage reveals.
5. **Coverage is checked at startup.** The scan lists every unclassified action, so gaps are visible without a test framework.
6. **Rollout is safe.** Audit mode logs would-be denials first, then enforcement is enabled menu by menu, with rollback through config.

**Outside the filter:** `MapGet("/")` returns only a fixed string, static files in `wwwroot` and public blobs are not API actions (Section 8, Decision 9), and no SignalR hubs exist.

## 4B. Application-wide role and group authorization

### 4B.1 What the database supports today

| Permission source you asked for | Exists today? | Evidence |
|---|---|---|
| **Role → menu View/Manage** | **Yes** | `SitesModulesMenusPermissions` (`SiteRoleId`, `IsShowMenu`, `IsView`, `IsManage`) |
| **Security group → menu permission** | **No** | No such table. Search found no general group, group-member or group-permission entity |
| **Individual user → menu permission** | **No** | That table is keyed by `SiteRoleId` only, with no user column |
| **Explicit deny** | **No** | Flags are true/false, and `false` means "not granted". `assignRolesToMenu` writes `false` by default |

**The existing "group" and per-user tables serve other purposes.** They control which records a user sees, not which modules they can open, and all of them stay as they are:

| Table | What it controls | Used in this plan as |
|---|---|---|
| `LeadUserGroupMapping` (`UserId`, `LeadGroupId`) | Which lead groups a user works with | Record scope |
| `ReportRoleGroupMapping` (`SiteRoleId`, `ReportGroupId`) | Which report groups a role sees | Record scope |
| `ReportUserMapping` (`AspNetUserId`, `FullAccess`, `ViewOnly`) | One user's access to one report | Record scope |
| `SitesProjectRolesPermissions` (`FullAccess`, `ViewOnly`, `Notes`) | A person's rights inside a project | Record scope |
| `ProjectModulesUserMapping`, `ProjectEmployeeMappings` | Project and module assignment | Record scope |

Their group values are `DropDown` rows, so reusing them for menu security would mix business categories with security rules. I don't recommend that.

### 4B.2 How groups and user permissions can be delivered

| | **Option A: roles as groups (no database change)** | **Option B: new security tables (needs your separate approval)** |
|---|---|---|
| Group permissions | Yes: a site role such as "QA Team" acts as a group, assigned through `AspNetUserRoles` | Yes: real groups with their own membership and permissions |
| Per-user permissions | Only by a one-person role, which suits rare exceptions only | Yes: direct grants per user, per site and per menu |
| Explicit deny | **No** | **Yes** |
| Database change | None | New tables only; existing tables untouched |
| Limits | Groups appear in role lists, and one label covers two ideas | More admin screens and a migration |

**Recommendation:** start with Option A, because it delivers group-based access now with no database change. Approve Option B only if you need per-user grants or explicit deny.

**Option B outline, for information only. Nothing here is approved or built:**
```
SecurityGroups(Id, SiteId, Name, Active, Deleted, audit columns)
SecurityGroupMembers(Id, SecurityGroupId, UserId, SiteId, Deleted, audit columns)
SecurityGroupMenuPermissions(Id, SecurityGroupId, SiteId, SiteModuleMenuId, ViewAccess, ManageAccess, Deleted, audit columns)
UserMenuPermissions(Id, UserId, SiteId, SiteModuleMenuId, ViewAccess, ManageAccess, Reason, Deleted, audit columns)
-- ViewAccess / ManageAccess: Inherit | Allow | Deny
```

**The code is ready for either choice.** `PermissionService` collects grants through an `IPermissionSource` interface. Only `RolePermissionSource` is built now, and Option B would add `GroupPermissionSource` and `UserPermissionSource` without changing the filter, attributes, controllers or Vue.

### 4B.3 Precedence and conflict rules

View and Manage are decided **separately, per menu, for the active site**. Rules apply top-down, and the first match decides:

| Order | Rule | Result | Available |
|---|---|---|---|
| 1 | **Hard gates fail:** not a site member, site, module, menu or role inactive or deleted, `Link` unknown or ambiguous | **Deny.** No grant can override this | Now |
| 2 | Conflicting duplicate rows for the same role and menu | **Deny** that flag, and log it | Now |
| 3 | **User Deny** on this flag | **Deny** | Option B |
| 4 | **User Allow** on this flag | **Allow.** The most specific rule beats groups and roles | Option B |
| 5 | **Group Deny** from any group | **Deny.** Deny beats every group or role Allow | Option B |
| 6 | **Any Allow** from a role or group | **Allow** (union) | Now |
| 7 | Nothing matched | **Deny** | Now |

**Supporting rules**
- `false` on a role row means "not granted", **never** deny, so adding a role can't remove access.
- Manage implies View. Denying View also blocks Manage, but denying Manage leaves View intact.
- `IsShowMenu` only affects navigation and never grants API access, apart from the legacy `1/0/0` handling in Section 4.4, which is applied when role rows are read.
- A grant from another site never counts, so a shared user's rights are evaluated separately in each site.
- Admins can't grant above their own level or change their own permissions.
- System Super Admin is outside this table: it follows Decision 3, and each bypass is logged.

**Conflict examples**

| User has | Manage result | View result | Available |
|---|---|---|---|
| Role A: View. Role B: Manage | Allow | Allow | Now |
| Role A: View. "QA Team" role-group: none | Deny | Allow | Now |
| No role or group row | Deny | Deny | Now |
| Valid grant, but the module is deleted | Deny | Deny (rule 1) | Now |
| Role: Manage. Group: Deny Manage | Deny | Allow | Option B |
| Group: Deny Manage. User: Allow Manage | Allow | Allow | Option B |
| Role: Manage. User: Deny View | Deny | Deny | Option B |

```csharp
// PermissionEvaluator: a pure function, used for both View and Manage
Decision Resolve(Flag flag, Grants g)
{
    if (g.HardGateFailed || g.HasRoleConflict(flag)) return Deny;
    if (g.User(flag) == Deny)  return Deny;   // Option B only
    if (g.User(flag) == Allow) return Allow;  // Option B only
    if (g.AnyGroup(flag, Deny)) return Deny;  // Option B only
    return g.AnyRoleOrGroupAllow(flag) ? Allow : Deny;
}
// Manage implies View: canView = Resolve(View) == Allow || (Resolve(Manage) == Allow && !g.Denies(View))
```

### 4B.4 Complete permission-checking flow (all controllers)

```
Request reaches any protected endpoint
1. Authentication      JWT signature, expiry, active user                     fail: 401
2. Classification      Tier from the matched endpoint; none means deny        fail: 403
3. Active site         X-Site-Id passes the site gate (Section 4.3)           fail: 403 SITE_NOT_MEMBER
4. Function            Required level: [ReadAccess], [ManageAccess] or verb default
                       Grants from all sources → PermissionEvaluator          fail: 403 PERMISSION_DENIED
── model binding ──
5. Site scope          siteId in body, query or route equals the active site  fail: 403
6. Record, site        [SiteOwned]: record and linked IDs belong to the site  fail: 404
7. Record, user        [RecordScope]: existing business rule allows this user fail: 404
8. Action runs         Existing business logic, unchanged
```

**Vue applies steps 1, 3 and 4 for navigation and buttons, from `my-permissions`, which uses the same evaluator.** Any denied page shows the 404 page. Direct URL entry passes through the guard, and Postman calls skip Vue and meet the same eight steps.

**View and Manage across functions.** The database stores permissions per menu, so that is the finest level:

| Function | Requires | Notes |
|---|---|---|
| Open a page, list, detail, search, read-only `POST` | View | `POST` reads carry `[ReadAccess]` |
| Create, edit, delete, status change, assign, import | Manage | The default for non-GET verbs |
| Export | View | Decision 4 in Phase 1; `[ManageAccess]` where exports are sensitive |
| Approve or reject | Manage on the approval's own menu | For example `/weekly-timesheet-approval`, separate from `/timesheet` |
| A button in a page with no menu of its own | Manage on the parent menu | Finer action-level rights would need a database change |

### 4B.5 Record-level data isolation

| Level | Question | Mechanism | Applies to |
|---|---|---|---|
| **Site** | Does this record belong to the active site? | `[SiteOwned]` and scoped lookups for child records | Every by-ID route on site-owned data |
| **User** | Do existing rules allow this user this record? | `[RecordScope(typeof(Policy))]` | Modules that already limit their lists |

**`[RecordScope]` adds no new rules.** Many lists already limit what a user sees, but by-ID calls skip those limits. Each policy applies the **same existing rule** to by-ID requests, so a user can open by ID only what their list would show:

| Policy (new) | Reuses existing rule | Applies to |
|---|---|---|
| `ProjectRecordPolicy` | Project creator, record creator, `ProjectEmployeeMappings` with `SitesProjectRolesPermissions`; admins see all (`TestCaseService.cs:85-103`) | Test cases, issues, tasks, activities, requirements, project items |
| `ReportRecordPolicy` | `ReportRoleGroupMapping`, `ReportUserMapping` (`ReportsController.cs:189,248`) | Reports |
| `LeadRecordPolicy` | `LeadUserGroupMapping` | Leads |
| `OwnerRecordPolicy` | Existing owner and approver checks | Timesheets, leave, expenses, daily planner |

**Details to confirm per module when implementing**
- Each rule must be read in full and matched exactly. Only the Test Case rule has been read so far, so the others above are expected, not confirmed.
- Write checks reuse existing `CurrentUserManage` calculations where they exist. Other modules get the read rule only until you decide otherwise (Decision 10).
- `[RecordScope]` runs in Audit mode first, because a link from a notification or email may open a record the user's list wouldn't show. Blocking those would change current behavior.

### 4B.6 Controller coverage (all 91)

`BaseController` stays unchanged. Links are expected values, to be confirmed by Audit Query 1. **All 91 controllers get attributes only**, apart from the logic fixes listed in Section 6.

| Group | Tier | Controllers |
|---|---|---|
| Project (6) | T4, per action | `Projects`, `ProjectUserMapping`, `ProjectSwimlane`, `SiteProjectRoles`, `AllProjectPlanner`, `ProjectDashboard` |
| Work (7) | T4 | `ProjectTasks`, `ProjectActivities`, `ProjectModules`, `ProjectModulesUserMapping`, `ProjectActionItems`, `ProjectQuestionsAnswers`, `ProjectReleaseTracking` |
| QA (3) | T4 | `Issue`, `TestPlan`, `TestCase` |
| Requirements (5) | T4 | `Requirement`, `RequirementGroup`, `RequirementDashboard`, `RequirementDataMapping`, `RequirementStatusTracking` |
| Time (5) | T4 | `Timesheet`, `TimesheetAISummary`, `DailyPlanner`, `TimeInTimeOut`, `MovementRegister` |
| HR (8) | T4 | `Employees`, `EmployeeOrgStructures`, `Departments`, `Person`, `EmployeeLeave`, `LeaveCredit`, `LeaveRules`, `LeaveSchedule` |
| CRM (10) | T4; some T0 | `Company`, `CompanyContact`, `Customer`, `CustomerFiles`, `Candidate`, `Leads`, `LeadUserGroupMapping`, `SalesPerson`, `JobCreate`, `Contact` |
| Finance (6) | T4 | `Expense`, `ExpenseAdvanceRequest`, `ExpensePurchaseRequest`, `ExpenseBankAccount`, `ExpenseVendorBankAccounts`, `ExpenseEmployee` |
| Infrastructure (8) | T4 | `InfraAccount`, `InfraAccountServices`, `InfraDashboard`, `InfraDatabase`, `InfraFTP`, `InfraProjectInstance`, `Server`, `Domain` |
| SOP, marketing, other (16) | T4 | `SOPTemplate`, `SOPProcess`, `SOPAssignment`, `AdPost`, `AdPostChannel`, `Inventory`, `TrainingPortal`, `Tags`, `HelpDesk`, `HelpDeskTopicsQuestions`, `Reports`, `ReportUserMapping`, `ItemCategory`, `ItemSubCategoryAttributes`, `SitesItems`, `SitesItemSubCategoryAttributesMapping` |
| Administration (9) | T5 writes; T3 reads | `Modules`, `ModulesMenus`, `Roles`, `Site`, `Users`, `PersonSitesMapping`, `DropDownTypes`, `DropDowns`, `EmailNotification` |
| Shared (4) | T2/T3 | `Common`, `Notes`, `Notification`, `Account` |
| Public and service (4) | T0/T1/T2 | `Auth`, `WebsiteDemos`, `Cronjob`, `Calendar` |

File names follow `API/Vsky.Api/Controllers/<Name>Controller.cs`.

## 5. Implementation roadmap

Sizes are relative (S/M/L), not time estimates. No general permission denials begin until Phase 2.4, and T4 enforcement begins at Phase 2.8.

| Phase | Work | Size | Risk |
|---|---|---|---|
| **2.0 Emergency containment** | Fix takeover and escalation paths (Section 6), then rotate the keys | M | Medium: retest MS login and password reset |
| **2.1 Discovery** | Run read-only Phase 1 audit queries, confirm production settings, answer Decisions | S | None |
| **2.2 Vue plumbing** | Fix 401/403 handling, pin the tab's site, add `switch-site`, handle a "no site" state | S | Low |
| **2.3 Foundation in Audit** | `PermissionService`, filter, attributes, startup scan, `my-permissions`, safe exception output | M | Low: logs only |
| **2.4 Site gate** | Audit, review, then Enforce. Add the site-scope filter | M | Medium: users without mapping rows (Queries 6, 7) |
| **2.5 Admin UI** | Show/View/Manage toggles, stop flag resets on re-assign, turn on the legacy-row rule after Query 3 | M | **Highest: see Section 9** |
| **2.6 Classify** | Tier attributes by controller group, `[ReadAccess]` on POST reads, `[SiteOwned]` on high-risk routes, 1-2 business cycles in Audit including month-end and approvals | L | Low |
| **2.7 Vue guard and page** | Permission store, guard, route metadata, 404 page, button gating (Audit-aware) | L | Low |
| **2.8 Enforce per menu** | Admin → Finance/HR → CRM → Projects, Tasks, Timesheet last. Rollback through config | M | Medium |
| **2.8A Record scope** | `[RecordScope]` policies by module: Audit, review, then Enforce | M | Medium: links from notifications and email |
| **2.9 Hardening and finish** | Remaining record checks, XSS sanitizer, session hardening, edge controls (Section 8), remove Audit paths | L | Medium |

## 6. Phase 2.0 fixes in detail

| Fix | File | Change |
|---|---|---|
| Forged Microsoft login | `Controllers/AuthController.cs` `GetSecureData` | Validate signature, tenant, audience and active user. `auth/pages/login.vue:146-153` sends `idToken` |
| Password reset | `AuthController.ResetPassword`, `Models/AuthModels.cs`, `Vsky.Services/Messages/WorkflowMessageService.cs:445`, `auth/pages/reset_password.vue` | Require the emailed token and send it in the link |
| Hash exposure | `AuthController.GetUserById` | Token-validated lookup returning only the email, as a DTO |
| Forged profile lookup | `AuthController.getUserByToken` | Validate the token. `auth.service.js:26` stops sending it in a query string |
| User enumeration | `AuthController` forgot password | Same response whether or not the account exists |
| Anonymous roles | `Controllers/RolesController.cs` | Remove class-level `[AllowAnonymous]`, add a temporary admin check |
| Permission writes | `ModulesMenusController.cs`, `ModulesController.cs`, `SiteController.cs` | Temporary site-admin check; bind the site to a validated value |
| Sharing (N1, N2) | `PersonSitesMappingController.cs` | Section 4.6 |
| Last-used site | `PersonSitesMappingController.UpdateLastUsedSite:270-290` | Take identity from the JWT, validate the target |
| Admin password reset (N3) | `UsersController.cs:489-501` | Site-admin check, and the target user must be in that site |
| Anonymous leave write | `EmployeeLeaveController.cs:678` | Require auth, per Decision 7 |
| SSRF (N4) | `CalendarController.cs:48-58` | Require auth and an allowlist of hosts, or remove |
| Error leakage | `Program.cs:650` | Return the reference ID only, as the commented line at `:649` does, with a correct status code |
| Auth failure response | `Program.cs:300-308` | Move the body to `OnChallenge` so anonymous endpoints and clean 401s work (*Inferred, confirm when implementing*) |
| Environment | `Vsky.Api/web.config:18` | Remove `Development` so Swagger and the developer exception page aren't exposed; **check the server first** |
| Password in browser | `auth/pages/login.vue:122` | "Remember me" stores the username only |
| Key material | `.gitignore` | Ignore `API/Vsky.Api/App_Data/`, which holds data-protection keys |

**Temporary admin check:** Phase 2.0 needs one small `RequireSiteAdminFilter` based on `GetNormalizedRoleNamesByUserAndSite`. Phase 2.3 replaces it, so the critical fixes needn't wait for the full framework.

**Operations, performed by your team**
- Rotate the JWT key, which logs everyone out.
- Rotate the exposed or real-looking secrets: AI key, Azure Blob, `AppSettings.applicationSecret`, database password and encryption key. Re-encrypting existing data must be planned before changing the encryption key.
- Review production logs for `mslogin` and `reset-password` activity.
- Decide on forced password resets (Decision 2).

## 7. Files requiring changes

### New

| Path | Purpose |
|---|---|
| `API/Vsky.Services/Authorization/IPermissionService.cs`, `PermissionService.cs`, `PermissionSnapshot.cs` | Site gate and permission decisions; cache |
| `API/Vsky.Services/Authorization/IPermissionSource.cs`, `RolePermissionSource.cs`, `PermissionEvaluator.cs` | Grant collection and the precedence rules |
| `API/Vsky.Services/Authorization/RecordAccess/IRecordAccessPolicy.cs`, `ProjectRecordPolicy.cs`, `ReportRecordPolicy.cs`, `LeadRecordPolicy.cs`, `OwnerRecordPolicy.cs` | Existing record rules, applied to by-ID requests |
| `API/Vsky.Api/Authorization/RecordScopeFilter.cs` | `[RecordScope]` filter |
| `API/Vsky.Api/Authorization/AuthorizationEnforcementFilter.cs` | Global chokepoint, deny by default |
| `API/Vsky.Api/Authorization/Attributes.cs`, `MenuLinks.cs`, `AuthorizationOptions.cs` | Tiers, link constants, modes |
| `API/Vsky.Api/Authorization/SiteScopeActionFilter.cs`, `SiteOwnedFilter.cs` | Tenant isolation |
| `API/Vsky.Api/Authorization/EndpointClassificationScan.cs` | Startup scan |
| `API/Vsky.Api/Controllers/AuthorizationController.cs` | `my-permissions`, `switch-site` |
| `WEB/src/stores/permissions.js`, `WEB/src/composables/usePermission.js`, `WEB/src/directives/can.js` | Vue permission state and helpers |
| `WEB/src/router/permissionGuard.js`, `WEB/src/security/menuLinks.js` | Single guard, link constants |

### Existing: backend

| Path | Change |
|---|---|
| `API/Vsky.Api/Program.cs` | Register filters, services, `AddMemoryCache`, `AddRateLimiter`. Apply CORS allowlist. Fix `:300-308`, `:650` |
| `API/Vsky.Api/Extensions/ServiceExtensions.cs` | Replace `AllowAnyOrigin` |
| `API/Vsky.Api/web.config` | Environment, and strict CSP in place of `:33` |
| `API/Vsky.Api/appsettings.json` (untracked, updated per environment) | `Authorization`, Microsoft identity, service key. Move secrets out over time |
| `API/Vsky.Api/Controllers/AuthController.cs`, `PersonSitesMappingController.cs`, `UsersController.cs`, `RolesController.cs`, `ModulesMenusController.cs`, `ModulesController.cs`, `SiteController.cs`, `EmployeeLeaveController.cs`, `CalendarController.cs`, `CronjobController.cs`, `HelpDeskController.cs`, `IssueController.cs` | Logic changes from Section 6 and Phase 1 Section I |
| All other `API/Vsky.Api/Controllers/*Controller.cs` (every controller named in Section 4B.6) | **Attributes only** |
| Services for child records with no `SiteId`, expected: `Vsky.Services/ExpenseLines/Expense_LinesService.cs`, `Expences/ExpenseFileService.cs`, `Projects/ProjectFilesService.cs`, `ProjectTasks/ProjectTaskFilesService.cs`, `ProjectActivities/ProjectActivityFilesService.cs`, `ProjectModules/ProjectModuleFilesService.cs`, `Notes/NoteService.cs`, `EmployeeLeaves/EmployeeLeaveService.cs`, `HelpDesks/HelpDeskService.cs`, `TestCases/TestCaseService.cs`, `TestCases/TestCaseExecutionLogService.cs`, `ProjectReleaseTrackings/ProjectReleaseTrackingReqPlanTaskIssueMappingService.cs` | Scoped lookups through the parent record. **The final list comes from a by-ID sweep and Audit logs.** 137 service files contain `.Id == id`, and most need no change because `[SiteOwned]` runs first |
| `API/Vsky.Services/SitesModule/SitesModulesService.cs` | Menu follows the central rules |
| `API/Vsky.Services/SitesModulesMenusPermission/SitesModulesMenusPermissionsService.cs` | Fix wrong ID at `:70`; return all three flags |
| `API/Vsky.Services/ApplicationUserRoles/ApplicationUserRoleService.cs`, `SitesRole/SitesRolesService.cs` | Exclude deleted roles, add site filters |
| `API/Vsky.Services/SitesModulesMenu/SitesModulesMenusService.cs` | Role-aware, deterministic landing page |
| `API/Vsky.Services/PersonSitesMapping/PersonSitesMappingService.cs`, `Vsky.Services/Expences/ExpenseService.cs`, `IssueService.cs` and similar | Scoped lookups, fixed-order site selection |
| `API/Vsky.Services/Messages/WorkflowMessageService.cs`, `API/Vsky.Api/Models/AuthModels.cs` | Reset token |
| `API/Vsky.Services/Security/EncryptionService.cs`, `AzureBlobImage/AzureBlobImageServices.cs` | Remove default key; private blobs (Decision 9) |

### Existing: frontend

| Path | Change |
|---|---|
| `WEB/src/router/index.js`, `WEB/src/boot/auth.js` | One guard, remove duplicate |
| `WEB/src/router/routes.js` | Name the catch-all route, `/approve-leave` metadata |
| `WEB/src/boot/axios.js`, `boot/interceptors.js` | Pinned site, storage listener, 401/403 handling |
| `WEB/src/stores/auth.js` | `logout` returns correctly and clears permissions; validated `changeTenant`; SSO failure clears the token |
| `WEB/src/shared/user_info.vue` | Use `switch-site`, then a full-page load of the landing page |
| `WEB/src/pages/not_authorized.vue`, `pages/error.vue` | New 404 page; `error.vue` shows it when the path was denied, and gets a null-safe link |
| `WEB/src/modules/auth/pages/login.vue`, `reset_password.vue`, `modules/auth/auth.service.js` | Section 6; honor `redirect` |
| `WEB/src/layouts/layout.vue`, `components/app_menu.vue` | Server-filtered menu; tolerate 403; remove the AI key comment at `:724` |
| `WEB/src/modules/dashboard/pages/index.vue` | Denied state per widget |
| `WEB/src/modules/settings/pages/index.vue` | Route-level permission, in addition to the existing filter |
| `WEB/src/modules/sites/pages/siteMenuRolePermissions.vue`, `modules/module/module.service.js` | Show/View/Manage toggles |
| `WEB/src/modules/<module>/routes.js` (71 active files; `common/routes.js` is never imported, so it is unchanged). `sites` and `module` also lack `requiresAuth`. Modules: `account`, `auth`, `candidate`, `company`, `company-contacts`, `customer`, `customer-files`, `dashboard`, `department`, `dropdown`, `employee`, `employee-org-structure`, `finance-bank-account`, `finance-expense`, `finance-expense-advance`, `finance-expense-purchase-request`, `finance-expense-vendors`, `helpdesk`, `infra-account`, `infra-account-services`, `infra-dashboard`, `infra-database`, `infra-ftp`, `infra-project-instance`, `inventory`, `issue`, `items`, `job-post`, `lead`, `leave`, `leave-rules`, `leave-yearly-schedule`, `marketing-ad-post`, `marketing-ad-post-channel`, `module`, `movementRegister`, `my-daily-planner`, `notification`, `person`, `project`, `project-action-items`, `project-center`, `project-modules`, `project-planning`, `project-questions-answers`, `project-release-tracking`, `project-targetplan`, `project-tasks`, `project-tasks-activities`, `reports`, `requirement`, `requirement-center`, `requirement-data-mapping`, `requirement-group`, `requirement-status-tracking`, `roles`, `settings`, `sites`, `sites-items`, `sites-sharing`, `sop-assignment`, `sop-process`, `sop-template`, `tags`, `test-case`, `test-plan`, `timeInTimeOut`, `timesheet`, `training-portal`, `user-management`, `website` | Permission metadata; `auth` and `website` are marked `public` |
| `WEB/src/modules/<module>/pages/*.vue` and `components/*.vue` in those modules | `v-can` on Add, Edit, Delete, Export and Approve buttons, as usability only. Examples: `test-case/pages/index.vue`, `user-management/pages/index.vue`, `sites/pages/index.vue`. The exact list comes from a button sweep during Phase 2.7 |
| Pages with `v-html` | Replace with `v-safe-html` backed by DOMPurify (new dependency), highest risk first |
| `WEB/src/modules/sop-process/components/_sopChatAssistant.vue:94-95,181` | Proxy the webhook through the API |
| `WEB/config/env.*.cjs`, `quasar.config.js:61` | Remove secrets; pass only an explicit allowlist to the build |

## 8. Enterprise hardening (Phase 2.9)

| Control | Action |
|---|---|
| Session | Reduce JWT lifetime to 8-12 hours (Decision 6). Add a security-stamp claim checked in `OnTokenValidated`, so a password change ends sessions. Set `RequireHttpsMetadata = true` |
| Abuse protection | Built-in .NET 7 rate limiter on login, forgot password, public forms and uploads |
| Passwords | Minimum 12 characters, plus lockout review at `Program.cs:233-238`. Complete or remove the partial 2FA at `AuthController:274-285` |
| Browser | Strict CSP without `unsafe-eval`, CORS allowlist, sanitized rich text |
| Uploads | Extension and content allowlist, size limits. Private Azure blobs with short-lived SAS links (Decision 9). Stop sending file URLs to Google gview |
| Secrets | Azure Key Vault or environment-level configuration; remove from source. Stop returning plaintext credentials to `infra-project-instance` |
| Data | Review the two stored-procedure paths (`ProjectService.cs:2354`, `ProjectsController.cs:2274`), whose source isn't in the repo |
| Platform | `net7.0` is out of support. Plan a separate upgrade to a supported LTS version |
| Token storage | Keep LocalStorage until XSS work is done, then evaluate HttpOnly cookies with CSRF protection as a separate project |

**Standards coverage:** this plan targets OWASP API Security Top 10 items API1 (object-level), API2 (authentication), API3 (property-level), API5 (function-level), API4 (resource consumption), API7 (SSRF) and API8 (misconfiguration). It follows the principles of least privilege and deny by default. It is **not a compliance certification**. An independent penetration test after Phase 2.9 is recommended.

## 9. Risks

| Risk | Impact | Mitigation |
|---|---|---|
| **Existing `1/0/0` permission rows (riskiest step)** | Most rows are likely `show=1, view=0, manage=0` (*Inferred from code; Query 3 confirms*). Literal enforcement locks users out. The legacy-row rule keeps them working, but those rows stay at full access until configured | Run Query 3, ship the admin toggles first, have admins configure sensitive modules before turning the rule off |
| Sharing or membership data gaps | Users lose site access at the gate | Queries 6-7, Audit before Enforce |
| Shared APIs (dashboard, Project Center, dropdowns) | Partly permitted users see errors | Any-of links, T3 for lookups, per-widget handling, audit logs |
| Self-service functions | Employees need Manage on their own leave and timesheets | Admins grant Manage on those menus through the toggles before the legacy-row rule is turned off; existing record rules still apply |
| Admin lockout | No one can correct permissions | Audited break-glass path (Decision 3) |
| Unknown clients (`wwwroot/mobileApk`, scheduler, external SSO) | Break under the gate or service key | Audit logs; Decision 5 |
| `Link` edited in the admin screen | API and route keys stop matching | Startup comparison against `MenuLinks`; read-only once enforced |
| Global change across 91 controllers and 72 route files | Merge conflicts | Attribute-only commits, by controller group |
| Record-level rules | Menu permission doesn't decide which project's records a user can change | `[SiteOwned]` handles cross-site. Within-site rules are Decision 10 |
| Local uncommitted work (`.quasar/*`, `env.*`, `timesheet/pages/index.vue`) | Mixed into security commits | Keep out of these commits |
| Roles used as groups (Option A) | Role names get crowded, and new roles never match the hardcoded role-name checks in Vue and services | A naming convention, such as a `Group - ` prefix. Existing hardcoded checks are unaffected |
| `[RecordScope]` blocks links from notifications and email | Users can't open a record they could open before | Audit mode first; adjust per module before enforcing |

## 10. Decisions needed

Work can start on 2.0 with the recommended defaults, except Decisions 1-2.

| # | Decision | Recommendation |
|---|---|---|
| 1 | **Is this code deployed to production now?** Is it urgent? | If yes, start Phase 2.0 immediately |
| 2 | Force password resets after the fix? | Yes, if production logs can't rule out abuse. The anonymous endpoint exposed hashes |
| 3 | System Super Admin and Site Super Admin bypass | Bypass for admin screens, all logged. System Super Admin still needs a role in a site for business data |
| 4 | Are you comfortable with `1/0/0` rows keeping full access until an admin configures them? | Yes, with the legacy-row rule (Section 4.4). It is the only option that changes neither data nor current access |
| 5 | Mobile app or other API clients in use? How does the cron scheduler call the API? | Please list them. The scheduler should send the service key in a header |
| 6 | JWT lifetime | 8-12 hours, with re-login |
| 7 | Must email leave approval work without login? | Require login with a redirect back |
| 8 | Which systems issue `?token=` SSO links? | If only this API, validate it, then move to a short-lived exchange |
| 9 | Include private blobs for expense files in this project? | Yes, as part of 2.9 |
| 10 | With Manage, can a user change all records in the module, or only their projects and own records? | Only what existing rules allow them, using `[RecordScope]` on every module that already limits its lists (Section 4B.5) |
| 11 | One production server or several? | The 60-second cache limit is acceptable for either |
| 12 | Which anonymous endpoints stay public? | Login, forgot/reset password, website forms. Review `Candidate`, `Contact`, `Calendar`, `moduleslist`, `DropDownTypes:222`, `JobCreate`, `HelpDesk` |
| 13 | Page wording | "404 - Unauthorized Access", with HTTP codes staying 401/403/404 underneath |
| 15 | **Groups and per-user permissions: Option A or Option B?** (Section 4B.2) | Start with Option A, which needs no database change. Option B needs your separate database approval |
| 14 | A user has no Test Case permission but has Project Center or Requirement Center. Should those screens still show test cases? | Yes, because that is today's behavior and you asked to keep business logic. The alternative is requiring `/test-case` on those endpoints too |

**Database: nothing changes under this plan.** Option B is shown for information and needs your separate approval. A dedicated audit table, unique indexes and refresh tokens are all left out. Short JWTs plus a security stamp need no new table: the stamp column already exists in `AspNetUsers`.

## 11. Verification (no test framework)

**Automatic checks in the product**
- The startup scan lists unclassified actions.
- Audit logs list would-be denials before each enforcement step.

**Manual checks, using Postman and a browser, per phase**

| Area | Check | Expected |
|---|---|---|
| Takeover | Forged `mslogin` and `user-by-token`; reset with no or wrong token; anonymous `auth/user` | Rejected; no hash returned |
| Tenant | Valid user, `X-Site-Id` for a non-member, deleted, malformed or empty site | 403; no data |
| Body override | `siteId` in body or query differs from the header | 403 |
| IDOR | Site A user reads, updates or deletes a site B record ID | 404; **database unchanged** |
| Shared user | Different roles on A and B; switch sites; menus and API follow the active site | Permissions from the active site only |
| Sharing | Non-admin shares; role ID from another site; role above the caller's; share removed mid-session | Denied; next request denied |
| Function | View-only user calls POST, PUT, DELETE; POST list with View | Writes 403; read allowed |
| Role changes | Remove Manage; delete a role; deactivate a user | Effective within 60 seconds; user deactivation immediate |
| Direct URL | Paste a restricted URL; refresh; bookmark; child route without parent access | 404 page, URL kept, no data flash, no restricted API call |
| Not logged in | Protected URL | Login, then back to that URL |
| Partial access | Dashboard with some denied widgets | Page stays; widget shows denied state |
| Multi-tab | Tab B switches site, then Tab A acts | Tab A reloads; no write to the wrong site |
| Roles and groups | Role A View + Role B Manage; role removed; role-group assigned then removed; each row in the Section 4B.3 examples marked "Now" | Result matches the table |
| Record scope | Same-site user opens a project record by ID that their list doesn't show, by GET, PUT, DELETE | 404; **database unchanged**; admin still allowed |
| All controllers | Startup scan after classification; a sample View-only and no-permission call per controller group | No unclassified actions; responses match Section 4A |
| Regressions | MS login, email links, cron, help-desk mail, landing page per role, System Super Admin | Work as decided |

**Rollout check before each Enforce step:** no unexplained would-be denials in the Audit logs for that group, covering at least one business cycle.

---

**This plan stops here. Implementation starts only after your explicit approval.**
