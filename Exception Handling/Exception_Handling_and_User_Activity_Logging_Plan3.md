# Centralized Exception Handling and User Activity Logging: Implementation Plan (v3)

**Status:** Final draft, awaiting your approval. No code, configuration, database or schema changes have been made.
**Date:** 5 November 2026
**Scope:** ASP.NET Core backend (`API/`), .NET 7, EF Core 7, SQL Server, IIS in-process. Two small web changes, plus one optional.
**Replaces:** Plan 1 and Plan 2 in this folder, once approved. Both are left unchanged for reference.

**Labels:** **[Existing]** works today and stays. **[Improve]** exists and gets changed. **[New]** is a new component.

---

## Confirmation

**The complete implementation can be done with no database schema change and no change to business logic, authorization rules or application workflows.** It adds no tables, columns, indexes or NuGet packages. Everything is stored in the existing `Log` table.

**It isn't entirely free of visible change.** Your original requirement needs the following, and each one is small:

| # | Visible change | Why it's needed | Who notices |
|---|---|---|---|
| C1 | Unhandled 500 body sends `message` instead of `Message` | The web app reads `message`, so the reference ID never reaches users today | Users see the reference ID instead of generic text |
| C2 | Two Power BI failure responses (`ReportsController.cs:106, 156`) return a safe message instead of the third-party response. Status stays 400 | Prevents exposing internal details | Users see a safer error |
| C3 | `EmailSender.cs:373` return value no longer includes `ex.Message`. Its only caller is `WorkflowMessageService.cs:637` | Same | No user-facing workflow change expected; to be reconfirmed when editing |
| C4 | New `POST /auth/logout`, called by the sign-out button | The API has no logout event to record | Nothing visible; failures are ignored |

**Limits that remain, by design:**

- Handled errors keep their current formats, so only unhandled errors get the corrected format. This follows your "preserve API response behavior" condition.
- The two `Ok(SafeErrorMessage(ex))` sites still return HTTP 200 on failure, but the failure is now traceable in the log.
- Older admin endpoints record the action and target, not before and after values.
- Logout is best effort: closing the browser sends nothing.

---

## How the plan meets your 10 conditions

| # | Condition | How it's met |
|---|---|---|
| 1 | Preserve functionality | Removed central error-format conversion, the `Ok` to `BadRequest` change, message-text cleanups and the 401 format change. All catch blocks keep their responses, apart from C2 |
| 2 | Centralized logging | `SecurityLog` is the single writer, with `ILogger` delegating to it. The planned `LogWriter` layer is removed |
| 3 | Simple architecture | **3 new files instead of 12,** all beside existing logging code. No new controller or service layer. No database code in controllers |
| 4 | Reliability | Writers never throw; database failures go to a fallback; the queue drains within a time limit at shutdown, and leftovers go to the fallback (section 7) |
| 5 | Performance | **Normal requests make no extra synchronous database calls.** Today's per-write insert moves to batches. Reads are off by default |
| 6 | Complete coverage | All 93 controllers, 858 actions and every service catch block classified (Appendix). Coverage comes from global middleware and filters, so new controllers are covered too |
| 7 | Security | No bodies, query strings, tokens or headers logged; exception text sanitized; exception details restricted on the server to System Super Admins (section 8) |
| 8 | No database changes | Confirmed. Retention is also removed from the application (D6) |
| 9 | Testing | Test project removed. Manual scenarios use the existing `/authorization/coverage` endpoint (section 9) |
| 10 | Deployment safety | Baseline required before any edit. Security files this plan touches or depends on have **no copy in git** (section 4) |

---

## 1. Existing architecture

### 1.1 `Log` table [Existing, unchanged]

`Vsky.Models/Log.cs`, `Vsky.Database/schema_default.sql:122`

| Column | Type | Current use |
|---|---|---|
| `Id` | `nvarchar(450)` PK | GUID text; shown to users as the reference ID |
| `ShortMessage` | `nvarchar(max)`, required | Event name. The audit screen **matches this exactly** |
| `IpAddress` | `nvarchar(200)` | Client address |
| `UserId` | `nvarchar(450)` | **Foreign key to `AspNetUsers`**, nullable |
| `LogLevelId` | `int` | 10 Debug, 20 Information, 30 Warning, 40 Error, 50 Fatal |
| `FullMessage` | `nvarchar(max)` | JSON, plain text or a stack trace, depending on the writer |
| `PageUrl` | `nvarchar(max)` | API path |
| `ReferrerUrl` | `nvarchar(max)` | Never populated |
| `CreatedOnUtc` | `datetime2(6)` | True UTC |

### 1.2 Two writers

| | `ILogger` / `DefaultLogger` | `SecurityLog` / `ISecurityLog` |
|---|---|---|
| Call sites | 9: `HelpDeskController` 5, `ReportsController` 3, `AzureBlobImageServices` 1 | `SafeErrorMessage`, global handler, filters, auth events, `LogChange` |
| DB context | **The request's own** | Its own scope |
| Fills in user, IP, path | **No** | Yes |
| Can throw | **Yes** | No |

`SecurityLog` already has the right safety properties, so this plan builds on it.

### 1.3 Exception handling

| Layer | Verified state |
|---|---|
| Global | Inline `UseExceptionHandler`, `Program.cs:684-708`; logs and returns a 500 with a reference ID |
| Controllers | 672 catch blocks: **648 use `SafeErrorMessage`**, 24 don't (Appendix). `Authorization`, `ModulesMenus` and `Roles` have no catches and rely on the global handler |
| Services, data, core | 30 catch blocks. Most rethrow or are expected; 7 hide a real failure (Appendix) |
| Background work | No `Task.Run` or fire-and-forget. All service failures occur inside a request |

### 1.4 Activity coverage today

| Required activity | Status | Evidence |
|---|---|---|
| Login succeeded, failed, locked; Microsoft login rejected | **[Existing], with gaps** | `LogAuthEvent`, `AuthController.cs:128` |
| **Logout** | **Not captured:** no API endpoint; sign-out only clears the browser | `WEB/src/stores/auth.js:135` |
| Create, update, delete | **[Existing]:** no record ID | `ActivityLogFilter.cs:50` |
| **Record access; module or menu access** | **Not captured** | `ActivityLogFilter.cs:41` skips reads |
| Roles, permissions and groups through Security Admin | **[Existing], strong:** before and after values | `LogChange`, `SecurityAdminService.cs:1095` |
| **Older role and permission endpoints** | **Generic row only** | `Roles`, `ModulesMenus`, `Modules`, `PersonSitesMapping`, `Users` |
| Site switching | **[Existing]** | `AuthorizationController.cs:123` |
| Denials by authorization filters | **[Existing],** throttled | `AuthorizationAuditLog.cs:47` |
| **Denials by `[RequireSiteAdmin]`, `[RequireSystemAdmin]`, some controller 403s; all 401s** | **Not logged** | `RequireAdminAttributes.cs:72`, `Program.cs:325` |

---

## 2. Gaps

| # | Gap | Evidence | Fix |
|---|---|---|---|
| G1 | Logging can hide the original exception | `DefaultLogger.InsertLog` can throw in a catch block | Phase 1 |
| G2 | Logging can save a failed request's pending changes | `DefaultLogger` shares the request's DB context | Phase 1 |
| G3 | One exception can write two rows | 5 `HelpDeskController` catches | Phase 1 |
| G4 | Exception rows lack site, method and action | `SecurityLog.cs:35` | Phase 1 |
| G5 | Log lost when the database is down | `SecurityLog.cs:53` | Phase 1 |
| G6 | Cancelled requests can look like errors | No filtering in `SafeErrorMessage` or the handler | Phase 1 |
| G7 | Reference ID doesn't reach users on unhandled errors | `Program.cs:705` sends `Message`; `boot/interceptors.js:186` reads `message` | Phase 2 |
| G8 | Internal details reach clients | `ReportsController.cs:106, 156`; `EmailSender.cs:373` | Phase 2 |
| G9 | Activity lacks record IDs; reads aren't recorded | `ActivityLogFilter.cs:41, 50` | Phase 3 |
| G10 | Requests stopped early aren't recorded | 401, 403, invalid model, 429 never reach an action filter | Phase 3 |
| G11 | Older admin endpoints aren't identifiable in permission reviews | Section 1.4 | Phase 3 |
| G12 | No logout event; session revocation not logged | `AuthorizationController.cs:79`, `UsersController.cs:631` | Phase 4 |
| G13 | Login gaps: no row for "no site access"; wrong password for a known user has no user | `AuthController.cs:326, 335, 391` | Phase 4 |
| G14 | Role changes in user screens lack before and after values | `UsersController.cs:333, 437, 450` | Phase 4 |
| G15 | 19 controller catches and 7 service catches hide failures | Appendix | Phase 5 |
| G16 | Exceptions aren't visible on any screen | `GetAuditLog:826` | Phase 6 |
| G17 | No retention | `Log` has no cleanup | D6 |

### Database limits and alternatives

| Limit | Alternative, with no database change | Cost |
|---|---|---|
| No site, record, method or status columns | Compact JSON in `FullMessage` | Can't be indexed; searches need a date range |
| GUID clustered key suits high insert volume poorly | Selective logging and batches | Logging every read isn't advisable |
| `UserId` foreign key | Retry with null `UserId`, keeping the value in JSON | Rare |
| API can't see the user's screen | Infer the module from the endpoint; optional header (D3) | Exact page needs the header |

---

## 3. Final architecture

```
 Request
    v
 ActivityLoggingMiddleware  [New]       after the response, decides whether a row is needed
    v
 Existing pipeline, in today's order
    UseExceptionHandler     [Improve]   stays built-in; three small fixes
    routing, rate limiter, GlobalVariableMiddleware, authentication, authorization
    v
 Global filters
    Authorization, site scope, permission cache   [Existing]
    ActivityLogFilter       [Improve]   stops writing; collects record IDs and result text
    v
 Controller catch { SafeErrorMessage }  [Existing] responses unchanged
    v
 Services                   [Existing]  log only where they hide a failure


 Single write path
    App code --> ILogger / DefaultLogger [Improve] --+
    Security events, SafeErrorMessage, handler ------+--> SecurityLog [Improve] --> Log table
    ActivityLogQueue [New] --- batches ---------------+         |
                                                       on failure: fallback file,
                                                       then Microsoft ILogger
```

### What was simplified from Plan 2

| Plan 2 proposal | Now | Why it's safe |
|---|---|---|
| `LogWriter`, `ILogWriter` | **Removed:** `SecurityLog` is the writer | It already has its own DB scope and never throws |
| `ExceptionHandlingMiddleware` | **Removed:** keep `UseExceptionHandler` | In .NET 6 and later, its feature object still exposes the matched endpoint, so details aren't lost. It also handles response-started and clearing correctly |
| `[LogActivity]`, `[NoActivityLog]`, edits to 4 controllers | **Removed:** use `EndpointPolicies` tiers | Tier metadata identifies all 36 admin writes, including ones added later |
| New `ILogger` methods | **Removed:** interface unchanged | No caller needs them |
| `LoggingOptions` | **Removed:** 3 settings added to `SecurityAuthorizationOptions` | That class already holds `ActivityLogging` |
| `RequestLogContext` service | **Replaced** by a small `HttpContext.Items` helper | No DI registration or lifetime concerns |
| `LogDetails`, `LogSanitizer` | **Folded into `SecurityLog`;** JSON uses anonymous objects | Matches existing code |
| `X-Request-Id` header and CORS change | **Removed:** `TraceIdentifier` stored in JSON | Users already get a reference ID |
| `ApiError.ReferenceId` | **Removed:** ID stays in `message` | The web app displays only `message` |
| `StandardErrorResultFilter`, retention job, test project | **Removed** | Conditions 1, 8 and 9 |

### Responsibilities

| Mechanism | Used for |
|---|---|
| **Middleware** | Final outcome of every request; early exits; record and module access; exceptions after the response has started |
| **`UseExceptionHandler`** | Unhandled exceptions |
| **Action filter** | Record IDs and result text, which only a filter can see |
| **Direct call** | Facts only that code knows: login outcomes, before and after roles, swallowed failures |

### Event names

The audit screen matches `ShortMessage` exactly, so names stay fixed and the detail goes in JSON.

| `ShortMessage` | Level | Status | Audit category |
|---|---|---|---|
| Login succeeded, Login failed, Login blocked: account locked, Microsoft login rejected | Information | [Existing] | session |
| Password reset requested, Password reset completed | Information | [Existing] | session |
| Site switched (`FullMessage` stays the plain site ID: `SecurityAdminService.cs:835`) | Information | [Existing] | session |
| Login denied: no site access | Warning | [New] | session |
| Logout | Information | [New] | session |
| Sessions revoked | Warning | [New] | session |
| Permission change | Warning | [Existing] | permission |
| Administrative change | Warning | [New] | permission |
| Authorization denied | Warning | [Existing], more sources | denied |
| Authorization audit: would deny | Warning | [Existing] | would-deny |
| Authentication failed | Warning | [New] | denied |
| User activity | Information; Warning on failure | [Existing], enriched | activity |
| Record access, Module access | Information | [New] | activity |
| *Failure description* | Error | [Improve] | errors [New] |

### Field mapping

| Field | Stored in |
|---|---|
| User | `UserId` |
| IP address | `IpAddress` |
| Endpoint | `PageUrl`: path only. I checked route templates, and no secret-bearing value appears in a path |
| Activity | `ShortMessage`, plus `activity` in JSON |
| Timestamp | `CreatedOnUtc`: taken when the event happened, not when the queue saved it |
| Site, method, controller, action, status, duration, request ID, record | `FullMessage` JSON |
| Exception | `FullMessage` JSON: type and sanitized stack trace |

```
ShortMessage: User activity          PageUrl: /help-desk
FullMessage:  {"siteId":"7f3c...","requestId":"0HN...","method":"POST","controller":"HelpDesk",
               "action":"CreateHelpDesk","activity":"Create Help Desk","recordIds":["a91d..."],
               "status":200,"durationMs":412}
```

`siteId` stays in this exact form because the audit screen's site filter searches for it.

---

## 4. Implementation phases

### Phase 0: Protect existing work (before any edit)

**The risk is real:** `main` has 404 modified files, 1 deleted and 20 untracked entries, and all the security work is uncommitted. These files, which this plan changes or depends on, are **untracked, so git can't restore them if they're damaged:**

`ActivityLogFilter.cs`, `SecurityLog.cs`, `AuthorizationAuditLog.cs`, `EndpointTiers.cs`, `SecurityAuthorizationOptions.cs`, `SecurityAdminService.cs`, `AuthorizationController.cs`, `audit.vue`.

**Required first (D4):**

1. You commit the security work as its own baseline, then logging work happens on a new branch from it.
2. If you'd rather not commit yet, I take a dated backup of all in-scope files outside the repo first. This is weaker, because logging and security changes will be mixed in one diff.

**Rules during implementation:**

- No `git checkout --`, `reset`, `clean`, `stash` or `restore`.
- Targeted edits only: no reformatting, renaming or line-ending changes.
- Review each file's diff before and after editing, and touch only files listed in section 5.
- No commits or pushes unless you ask.
- **Deploy after the security release,** following `Customer Security/Deployment-and-Secret-Rotation-Plan.md`, because this work depends on its filters and tiers.

### Phase 1: Safe single writer (2 days)

1. Add `ISecurityLog.WriteError(description, exception, userId)`. In `SecurityLog`:
   - Duplicate protection and cancellation handling (section 7).
   - Sanitized exception JSON with request context.
   - `ShortMessage` uses the description or innermost exception type, because `EfRepository` puts the entire exception text in `Message`.
2. Add `WriteBatch` for the queue, with the same failure handling.
3. Add fallback: an ID generated first, a daily file if `LogFallbackPath` is set, then Microsoft `ILogger`. After a database failure, skip the database for 30 seconds.
4. `SecurityLog.Write` keeps storing non-empty `FullMessage` values exactly as passed. Empty values get request-context JSON.
5. `DefaultLogger` write methods call `ISecurityLog`. Read and delete methods stay, and the `ILogger` interface doesn't change.
6. `SafeErrorMessage` and the global handler call `WriteError`.

### Phase 2: Exception handler and exposure fixes (1 day)

1. In the existing handler (`Program.cs:684-708`):
   - Skip the error row for client-aborted requests.
   - Serialize with the app's JSON options (C1).
   - Keep the same text: `An error occurred while processing your request. Reference id is <id>`.
2. Fix `ReportsController.cs:106, 156` (C2) and `EmailSender.cs:373` (C3), logging the details.

### Phase 3: Activity logging (2 to 3 days)

1. Add `RequestLogState`, using `HttpContext.Items`: record IDs, result text, reference ID, handled flag.
2. Add `ActivityLogQueue`: a bounded `Channel` and a hosted service in one class. It writes every 2 seconds or 200 rows.
3. Change `ActivityLogFilter` to stop writing and start collecting:
   - Record IDs, reusing `RecordIds.From`.
   - New record's `Id` from an `Ok(entity)` result.
   - Text of a `BadRequest` string or `ApiError`, capped at 500 characters.
4. Add `ActivityLoggingMiddleware` as the first pipeline step. All logic runs after the response:
   - Find the action from endpoint metadata. On handled exceptions the handler clears the endpoint, so read it from `IExceptionHandlerFeature`.
   - Apply the rules in the table below.
   - Copy all values, then queue the row. `HttpContext` isn't valid in the background.
   - Read `X-Site-Id` directly as a fallback, because a 429 never reaches `GlobalVariableMiddleware`.
   - In a `catch`, write exceptions rethrown after the response started, then rethrow.
5. Add field-name-only capture for validation failures at `Program.cs:205`.
6. `AuthorizationAuditLog.Record` sets the handled flag for `Denied` **before** its throttle check.

| Request | Row | Default |
|---|---|---|
| Write action, including anonymous and service calls | `User activity` | On |
| Non-GET action at `Admin` tier | `Administrative change` | On |
| GET with a route ID | `Record access`; 1 per user and record, per minute | **Off** |
| GET at `RequireMenu` tier | `Module access`; 1 per user, site and module, per 30 minutes | **Off** |
| 401 | `Authentication failed`; throttled | On |
| 403 not already recorded | `Authorization denied`; throttled | On |
| 400, 404, 429, 5xx on a matched action, any method | `User activity` at Warning or Error; 4xx throttled | On |
| Actions marked `[ReadAccess]`, list GETs, `OPTIONS`, successful `/auth/refresh` | None | |
| Unmatched URLs from anonymous callers (such as bots) | None | |

`Authorization:ActivityLogging` remains the off switch. Three settings are added to `SecurityAuthorizationOptions`: `LogRecordAccess` (false), `LogModuleAccess` (false) and `LogFallbackPath` (null). **So deployment needs no configuration change.**

### Phase 4: Specific events (1 day)

1. **Logout:** `POST /auth/logout` with `[AuthenticatedOnly]`, recording through `LogAuthEvent`. `WEB/src/shared/user_info.vue:189, 192` calls it before signing out, ignoring failures. Session-expiry sign-out (`interceptors.js:120`) doesn't call it, so an expiry is never recorded as a logout.
2. **Sessions revoked:** log in both `revoke-sessions` actions.
3. **Login gaps:** log the two no-site-access branches, and pass `userData.Id` at `:391`.
4. **User role changes:** at `UsersController.cs:333, 437, 450`, write `Permission change` with before and after. Before values come from the existing `_applicationUserRoleService.GetRoleIdsByUserAndSite`. This is one extra query, on a rare admin action, through an existing service.

### Phase 5: Silent failures (1 day)

1. Add `BaseController.LogHandled(ex, description)`, which uses `RequestServices` the way `SafeErrorMessage` does, so no controller constructor changes.
2. Add one line to each of the 19 silent controller catches. **Responses stay the same.**
3. Add `ILogger` to the 4 services and log the 7 hidden failures. They keep swallowing as they do today. These services are created only through DI, as I confirmed by searching for manual `new` calls.
4. Correct the `HelpDeskController.cs:698` label, because the first description is the one kept: it currently says "Email Send Failed" for any failure.

### Phase 6: Review (half a day)

1. Add new event names to `AuditCategories`.
2. Add an `errors` category, filtered by level and **allowed only for System Super Admins, enforced in `GetAuditLog`.** It's excluded from the default view.
3. Make `search` match `Id`, so a reference ID finds its row.
4. Add the "Errors" option to `audit.vue`.

### Phase 7: Verification and rollout (2 to 3 days)

Run section 9, then:

1. Deploy to test with a copy of the data, using defaults.
2. Measure row counts for 2 to 3 days.
3. Turn on `LogModuleAccess`, then `LogRecordAccess`, watching growth after each.
4. Repeat on production.

**Rollback:** set `Authorization:ActivityLogging` to false to stop activity rows, or redeploy the previous build. There's no schema change to undo, and new rows stay readable by old code.

**Total: about 2 weeks for one developer,** down from 2.5 to 3. Phases 1 to 6 can each ship separately, in order.

---

## 5. Affected files

Paths are relative to `API/`, unless shown otherwise. **Status** shows the current git state: **M** modified, **U** untracked, **C** clean. Files without a status weren't individually checked; Phase 0 confirms them.

### New (3) [New]

| File | Purpose |
|---|---|
| `Vsky.Api/Authorization/ActivityLoggingMiddleware.cs` | Final outcome of every request |
| `Vsky.Api/Authorization/ActivityLogQueue.cs` | Queue, batch writer and shutdown drain |
| `Vsky.Api/Authorization/RequestLogState.cs` | Per-request values in `HttpContext.Items` |

### Logging and pipeline (9) [Improve]

| File | Status | Change |
|---|---|---|
| `Vsky.Api/Program.cs` | M | Register queue; add middleware; 3 handler fixes; validation hook |
| `Vsky.Api/Authorization/SecurityLog.cs` | **U** | `WriteError`, `WriteBatch`, sanitizing, fallback, handled flag |
| `Vsky.Services/Authorization/ISecurityLog.cs` | | Add `WriteError` |
| `Vsky.Api/Authorization/ActivityLogFilter.cs` | **U** | Collect instead of write |
| `Vsky.Api/Authorization/AuthorizationAuditLog.cs` | **U** | Set the handled flag before the throttle |
| `Vsky.Services/Authorization/SecurityAuthorizationOptions.cs` | **U** | 3 settings |
| `Vsky.Api/Controllers/BaseController.cs` | M | `SafeErrorMessage` uses `WriteError`; add `LogHandled` |
| `Vsky.Services/Logging/DefaultLogger.cs` | C | Write through `ISecurityLog` |
| `Vsky.Services/SecurityAdmins/SecurityAdminService.cs` | **U** | Categories, errors gate, reference ID search |

### Controllers (18 of 93) [Improve]

| Controller | Change |
|---|---|
| `Auth` | Logout endpoint; login gaps |
| `Authorization`, `Users` | Log revocation; `Users` also logs role changes |
| `Reports` | C2; log silent catches at `:219, 450` |
| `HelpDesk` | Label at `:698` |
| `CompanyContact`, `Company`, `Cronjob`, `CustomerFiles`, `DailyPlanner`, `Domain`, `Expense`, `LeadUserGroupMapping`, `ProjectActionItems`, `ProjectModules`, `SalesPerson`, `Server`, `Tags` | One `LogHandled` line per silent catch |

### Services (4) [Improve]

| File | Status | Change |
|---|---|---|
| `Vsky.Services/Messages/WorkflowMessageService.cs` | M | Inject `ILogger`; log `:597, 641, 704, 758` |
| `Vsky.Services/Notifications/NotificationService.cs` | C | Inject `ILogger`; log `:224` |
| `Vsky.Services/Messages/EmailSender.cs` | C | Inject `ILogger`; log `:370`; C3 |
| `Vsky.Services/PowerBI/EmbedService.cs` | C | Inject `ILogger`; log `:80` |

### Web (2, plus 1 optional) [Improve]

| File | Change |
|---|---|
| `WEB/src/shared/user_info.vue` | Call `/auth/logout` on sign-out |
| `WEB/src/modules/security-admin/pages/audit.vue` | "Errors" option |
| `WEB/src/boot/axios.js` | **Optional, D3:** `X-Client-Page` header |

**Total: 3 new and 33 changed, plus 1 optional.** Plan 2: 12 new and 42 changed.

### Explicitly not changed

- **Database and models:** `Log.cs`, `ApplicationDbContext.cs`, `Vsky.Database/*`.
- **Configuration:** `appsettings.json`, `appsettings.Development.json`, `web.config`.
- **Authorization rules:** `AuthorizationEnforcementFilter`, `SiteScopeActionFilter`, `RecordAttributes`, `RequireAdminAttributes`, `EndpointTiers`, `PermissionCacheInvalidationFilter`.
- **Error types and logging contracts:** `ApiError`, `ILogger.cs`.
- **Other infrastructure:** `EfRepository`, `GlobalVariableMiddleware`, `ServiceExtensions`.
- **Controllers:** the other 75.
- **Business history tables:** `HelpDeskStatusLog`, `IssueStatusChangedLog`, `RequirementChangeLog` and similar.

---

## 6. How each scenario is captured

### Exceptions

| Scenario | Captured by | User sees |
|---|---|---|
| Controller catch with `SafeErrorMessage` | `WriteError` | Unchanged |
| Controller with no catch; filter or middleware inside the handler | `UseExceptionHandler` → `WriteError` | 500 with reference ID |
| Service exception that propagates | Controller catch or handler | As above |
| Service failure swallowed on purpose | `ILogger.Error` | Operation continues as today |
| Silent controller catch | `LogHandled` | Unchanged |
| After the response has started | Middleware `catch`, then rethrow | Connection ends, as today |
| Client cancels | No error row; activity marked cancelled | Nothing |
| Database down | Fallback, under the same reference ID | 500 with reference ID |

### Activities

| Required activity | Trigger | Row |
|---|---|---|
| Login | `LogAuthEvent` | Session events |
| Logout | New endpoint | `Logout` |
| Session expiry | 401 only | `Authentication failed` |
| Create, update, delete across all 93 controllers | Middleware and filter | `User activity` with record ID |
| Record access | Middleware (when enabled) | `Record access` |
| Module or menu access | Middleware (when enabled) | `Module access` |
| Role and permission changes, Security Admin | `LogChange` [Existing] | `Permission change` |
| Role changes in user screens | Direct call | `Permission change` with before and after |
| Older admin endpoints (28 actions in `Roles`, `ModulesMenus`, `Modules`, `PersonSitesMapping`, `Site`, `Users`) | Middleware, by tier | `Administrative change` |
| Group create, update, delete, membership, permissions | `LogChange` [Existing] | `Permission change` |
| Site switching | Existing call | `Site switched` |
| Denied by filters | `AuthorizationAuditLog` [Existing] | `Authorization denied` |
| Denied by admin attributes or a controller 403 | Middleware | `Authorization denied` |
| Unauthenticated | Middleware | `Authentication failed` |
| Cron and webhooks | Middleware; `"actor":"service"` | `User activity` |

---

## 7. Duplicate prevention and failure handling

### Duplicates

| Situation | Rule | Result |
|---|---|---|
| Same exception logged twice, or wrapped by `EfRepository` | First write stores its ID in `ex.Data`. Later calls check the `InnerException` chain and return that ID | One row, keeping the most specific description. Fixes 5 sites without editing them |
| Event already written (login, site switch, permission change, denial) | `SecurityLog.Write` sets the flag for a **fixed list of event names;** the middleware then writes nothing | One row |
| Denial skipped by the existing throttle | Flag set before the throttle check | Throttling keeps working |
| Audit-mode "would deny" | Not on the list | Audit row and normal activity row, which is correct: the request continued |
| Failed request | One error row and one activity row, sharing `requestId` | Intentional: what failed, and what the user did |
| Repeated 401, 403, 404, 429 | One row per user or IP, path and status, per minute, using the existing `IMemoryCache` pattern | Polling with an expired token can't flood the table |

### Failures

| Failure | Handling |
|---|---|
| Any logging error | All writers catch everything. The original exception always continues |
| Database down | Row goes to fallback under the pre-generated ID; database skipped for 30 seconds, so an outage doesn't add a timeout to every error |
| Fallback file fails or isn't set | Microsoft `ILogger`, then ignored |
| `UserId` fails the foreign key | Retry with null `UserId`; value kept in JSON |
| One bad row in a batch | Retry row by row |
| Queue full (10,000 rows) | Routine activity is dropped and the count logged once a minute. Exceptions and security events bypass the queue |

### Shutdown

1. The queue is registered as a hosted service, so ASP.NET Core should stop accepting and finish requests before stopping it (confirmed by V21).
2. `StopAsync` completes the channel and drains it, **limited to 5 seconds.** This stays inside the IIS module's default 10-second `shutdownTimeLimit`, which `web.config` doesn't override.
3. Rows left after 5 seconds, or arriving after the channel closes, go to the fallback.
4. In a hard crash or power loss, up to about 2 seconds of routine activity is lost. This is accepted. Exceptions and security events are written immediately, so they aren't at risk.

### Performance

| Item | Effect |
|---|---|
| Normal successful request | **Zero extra synchronous database calls.** Today's write-action insert is removed from the request |
| Row decision | In-memory only; `EndpointPolicies` is already cached |
| Bodies | Never read or buffered |
| Writes | Batched; at most one background write every 2 seconds under light load |
| Reads | Off by default; throttled when on |

---

## 8. Security, risks and decisions

### Security rules

- **Never logged:** request and response bodies; query strings (`token`, `serviceKey` and `sig` travel this way); `Authorization` and cookie headers; validation values (field names only).
- **Exception text** is sanitized before storing: `Password=`, `Pwd=`, `AccountKey=`, `Bearer ...` and `SharedAccessSignature=` are masked, and `FullMessage` is capped at 64 KB. Result text gets the same treatment.
- **Record IDs:** simple values only; named `id` or ending in `Id`; 64 characters or fewer; up to 20 per row.
- **Exception access:** enforced on the server in `GetAuditLog`, not just hidden in the UI. `DefaultLogger`'s read methods have no callers outside `Vsky.Services/Logging`, so the audit endpoint is the only API route to these rows. I also confirmed `SecurityAdminController` requires `[RequireSiteAdmin]`.
- **Fallback file:** outside the web root and deployment folder, restricted to the app pool identity. `FolderProfile.pubxml` deletes existing files on publish, so `App_Data` isn't safe.
- **Failed logins:** the attempted username isn't stored for unknown accounts, because people sometimes type passwords into that field.

### Risks

| # | Risk | Mitigation |
|---|---|---|
| R1 | **Loss of uncommitted security work** | Phase 0 |
| R2 | **Wrong IP address.** `UseForwardedHeaders` (`Program.cs:717`) sets no known proxies | D1 |
| R3 | **`Log` growth and slower audit searches** | Reads off by default; date ranges; measured rollout; D6 |
| R4 | **Spoofed site ID.** `X-Site-Id` is client-supplied, especially on 401 rows | Treated as context; user, IP and time come from the server |
| R5 | **Secrets in logs** | Allow-lists, sanitizer, V12 |
| R6 | **Handler's endpoint details unavailable** after clearing | Read from `IExceptionHandlerFeature`; V2 confirms |
| R7 | **Stack traces lack line numbers** | Publish `.pdb` files; V3 |
| R8 | **Module name is ambiguous** on endpoints shared by several menus | All links stored; D3 resolves it |
| R9 | **Personal data:** named activity history | D7 |

### Decisions needing your approval

| # | Decision | Recommendation | Blocks |
|---|---|---|---|
| D1 | How does traffic reach the API: directly to IIS, or through a load balancer, CDN or proxy, and at which addresses? | *Needs your answer* | Correct IPs in production |
| D2 | Who can see exception details? | **System Super Admins only** | Phase 6 |
| D3 | Add the `X-Client-Page` header for the exact screen? | **Yes;** about half a day. Without it, you get the module only | Nothing |
| D4 | How should existing work be protected? | **You commit the security work as a baseline; logging work goes on a new branch** | **Start of implementation** |
| D5 | Where should the fallback file go? | A folder outside the deployment, such as `D:\Logs\Meldep`. Until set, failures go only to Microsoft `ILogger` | Production rollout |
| D6 | How should old rows be deleted? | **Outside the application:** a DBA-scheduled job. Activity after 90 days; errors and security events after 1 year, subject to customer or audit requirements | Nothing now; needed as volume grows |
| D7 | Do employees need to be told about activity logging? | **Confirm with your legal or HR contact** | Turning on record and module access in production |
| D8 | Accept the visible changes C1 to C4? | **Yes** | Phases 2 and 4 |

**Dependencies:**

- The security release deploys first.
- No new NuGet packages: Humanizer and the ASP.NET framework reference are present.
- .NET 7 is out of support, but nothing here depends on upgrading.

---

## 9. Verification

**No test framework or test project is added.** Scenarios run against the test environment using the API, the existing audit screen and SQL. Forced exceptions use a **temporary local change that's never kept or committed.**

### Exceptions

| # | Scenario | Expected |
|---|---|---|
| V1 | Force an exception in `CreateHelpDesk` | **One** error row with user, IP, endpoint, site and stack trace; reference ID matches; same 400 response |
| V2 | Force an uncaught exception in `RolesController`, from the browser | 500; **reference ID visible in the toast;** CORS headers present; row names the controller and action |
| V3 | Inspect a production stack trace | Line numbers present |
| V4 | Exception thrown from a service with no catch | Same as V1 or V2 |
| V5 | Exception with a pending unsaved change | Logging **doesn't** save it |
| V6 | Client cancels a request | No error row |
| V7 | Force a Power BI error | Safe message; details in the log only |
| V8 | Fail an email send | Row exists; user's action still succeeds |

### Activities

| # | Scenario | Expected |
|---|---|---|
| V9 | Successful login; wrong password; locked account; no site access; rejected Microsoft token | **One** correct session row each |
| V10 | Sign out; close the browser; let a token expire | `Logout` for the first only |
| V11 | Create, update and delete a record | One `User activity` row each, with record ID, including the new record's ID |
| V12 | Log in, reset a password, call `user-by-token`, use a service key | **Search `Log` and the fallback file: no passwords, tokens or keys** |
| V13 | Each Security Admin change; each older admin endpoint; a user role edit | Each appears **once** in the permission filter; role edit has before and after |
| V14 | Switch site, allowed and not allowed | `Site switched` with plain site ID; denial row for the second |
| V15 | Denied by a filter; by `[RequireSystemAdmin]`; repeat 100 times in a minute | One row per key, per minute |
| V16 | Expired token while polling; unknown URL; rate limit | Throttled rows for 401 and 429; row for an authenticated 404; none for an anonymous 404 |
| V17 | Turn on record and module access | Rows appear, throttled; list pages write nothing |

### Failure handling

| # | Scenario | Expected |
|---|---|---|
| V18 | Stop SQL Server, force an exception | 500 with reference ID; exception in the fallback under that ID; original error not replaced |
| V19 | Restart SQL Server | Writes resume within 30 seconds |
| V20 | Remove the fallback path | Requests still succeed |
| V21 | Recycle the app pool under load | Queue drains within 5 seconds; leftovers in the fallback; no errors |

### Existing functionality and coverage

| # | Scenario | Expected |
|---|---|---|
| V22 | **Coverage:** call `GET /authorization/coverage`, which reports all actions by tier. Then exercise one write and one failure per module | 858 actions, none unclassified; rows appear for each |
| V23 | **Behavior:** compare status and body before and after on a sample from each module, including the 19 silent catches | Identical, apart from C1 to C4 |
| V24 | **Authorization:** repeat allow and deny checks in both Enforce and Audit modes | Identical outcomes |
| V25 | Audit screen: old rows, all filters, "Site switched", reference ID search | All work |
| V26 | Request the `errors` category directly by API, as a site admin | Refused |
| V27 | Compare response times with logging on and off | Added latency under 5 ms at the 95th percentile |
| V28 | `git diff` review against the baseline | Only files in section 5 changed; no unrelated changes |

---

## 10. Implementation checklist

### Exception handling

- [ ] Controllers, endpoints and services covered (V1, V2, V4, V8)
- [ ] Unhandled errors standardized; handled errors preserved (V2, V23)
- [ ] No internal details exposed (V7)
- [ ] Enough detail for troubleshooting (V1, V3)

### Activity logging

- [ ] Login and logout (V9, V10)
- [ ] Create, update, delete (V11)
- [ ] Record and module or menu access (V17)
- [ ] Role and permission changes (V13)
- [ ] Group changes and membership (V13)
- [ ] Site switching (V14)
- [ ] Unauthorized and denied attempts (V15, V16)

### Your 10 conditions

- [ ] 1. Existing functionality preserved (V23, V24)
- [ ] 2. Single writer; no duplicates (V1, V9, V13, V15)
- [ ] 3. Simple architecture: 3 new files; no database code in controllers
- [ ] 4. Failures and shutdown handled (V18 to V21)
- [ ] 5. Performance within target (V27)
- [ ] 6. Complete coverage (V22, Appendix)
- [ ] 7. No sensitive data; restricted details (V12, V26)
- [ ] 8. No database changes (V28)
- [ ] 9. Verified without a new test framework (section 9)
- [ ] 10. Existing work protected (Phase 0, V28)

### Sign-off

- [ ] D1 to D8 answered
- [ ] Baseline in place (D4)
- [ ] Explicit approval to begin

---

## Appendix: Backend coverage audit

Counted from the source, with comments excluded.

### Controllers

| Measure | Count |
|---|---|
| Controllers (excluding `BaseController`) | **93** |
| Actions | **858:** 308 GET, 550 write |
| POST routes that look like reads | 137; 135 already marked `[ReadAccess]`, so activity classification is reliable |
| GET routes with a route value | 155 |
| Admin-tier write actions | 36: 28 older, 8 in `SecurityAdmin` |
| Catch blocks | **672:** 648 use `SafeErrorMessage` |

**The 24 catch blocks that don't use `SafeErrorMessage`:**

| Type | Count | Location | Action |
|---|---|---|---|
| Empty | 13 | `CompanyContact:124`, `Company:223, 392`, `Cronjob:241`, `CustomerFiles:286`, `DailyPlanner:567`, `Domain:133, 273`, `ProjectActionItems:212`, `SalesPerson:121`, `Server:163, 272`, `Tags:140` | `LogHandled` |
| Replacement response | 6 | `DailyPlanner:358`, `Expense:458`, `LeadUserGroupMapping:65`, `ProjectModules:218`, `Reports:219, 450` | `LogHandled`; same response |
| Exposes third-party content | 2 | `Reports:103, 153` | C2 |
| Logs through `ILogger` | 1 | `Reports:158` | None |
| Expected token failure | 2 | `Auth:222, 495` | None; `:495` gets a 401 row automatically |

**Coverage by controller:**

- **75 controllers need no edit:** middleware, filters and `SafeErrorMessage` cover them.
- **`Authorization`, `ModulesMenus`, `Roles`** have no catches, so the global handler covers exceptions. `ExpenseEmployee` has no actions.

### Services, data and core (30 catch blocks)

| Result | Location | Action |
|---|---|---|
| **Hides a real failure (7)** | `WorkflowMessageService:597, 641, 704, 758`; `NotificationService:224`; `EmailSender:370`; `EmbedService:80` | Log; behavior unchanged |
| Rethrows; logged upstream (12) | `SmtpBuilder:65`; `EmbedService:303, 411, 503`; `RequirementStatusTrackingService:486, 557`; `EfRepository:90, 104, 118, 132, 146, 160` | None |
| Already logs (1) | `AzureBlobImageServices:181` | None; gains context automatically |
| Expected or best effort (10) | `EmailSender:337, 393`; `Tokenizer:132`; `WorkflowMessageService:1848`; `AadService:55`; `EfRepository:43, 56`; `CommonHelper:368`; `AppFileProvider:145, 149` | None |

### Middleware and authorization flows

| Flow | Coverage |
|---|---|
| `GlobalVariableMiddleware` | Unchanged; 429 handled by a header fallback |
| JWT `OnChallenge` | Response unchanged; 401 row from middleware |
| `AuthorizationEnforcementFilter`, `SiteScopeActionFilter`, `RecordAttributes` | Existing rows; handled flag prevents duplicates |
| `RequireSiteAdmin`, `RequireSystemAdmin` | Unchanged; 403 row from middleware |
| `PermissionCacheInvalidationFilter` | Unchanged |
| `EndpointClassificationScan` | Unchanged; used for V22 |
| Rate limiter | Unchanged; throttled 429 row |
