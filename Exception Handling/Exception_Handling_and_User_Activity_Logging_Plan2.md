# Centralized Exception Handling and User Activity Logging: Implementation Plan (v2)

**Status:** Awaiting review. No code, configuration, database or schema changes have been made.
**Date:** 5 November 2026
**Scope:** ASP.NET Core backend (`API/`), .NET 7, EF Core 7, SQL Server, IIS in-process. Three small web changes, called out where they appear.
**Save location:** `Exception Handling/Exception_Handling_and_User_Activity_Logging_Plan2.md`

**Approving this plan only saves this document.** Implementation needs your separate, explicit approval.

**Relationship to earlier plans:** This extends `Exception_Handling_and_User_Activity_Logging_Plan.md` (Plan 1), which it replaces once approved. Plan 1's design holds up when checked against the code, but it has one defect and doesn't cover several events this requirement names. Section 11 lists the differences. `Customer Security/User-Activity-Tracking-Implementation-Plan.md` proposed new tables, so it doesn't meet the no-schema-change requirement.

**Labels used below:** **[Existing]** works today and stays. **[Improve]** exists and gets changed. **[New]** is a proposed new component.

---

## 1. Existing logging and exception-handling architecture

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
| `PageUrl` | `nvarchar(max)` | API path, not the user's screen |
| `ReferrerUrl` | `nvarchar(max)` | Never populated |
| `CreatedOnUtc` | `datetime2(6)` | True UTC |

There are no columns for site, record, HTTP method, controller or status.

### 1.2 Two log writers

| | `ILogger` / `DefaultLogger` | `SecurityLog` / `ISecurityLog` |
|---|---|---|
| Location | `Vsky.Services/Logging/` | `Vsky.Api/Authorization/`; interface in `Vsky.Services/Authorization/` |
| Call sites | 9: `HelpDeskController` 5, `ReportsController` 3, `AzureBlobImageServices` 1 | `SafeErrorMessage`, the global handler, filters, auth events, `LogChange` |
| DB context | **The request's own** | Its own scope |
| Fills in user, IP, path | **No** | Yes |
| Can throw | **Yes** | No; returns null |

### 1.3 Exception handling

| Layer | Today |
|---|---|
| Global | Inline `UseExceptionHandler` at `Program.cs:684-708`: logs and returns a 500 with a reference ID |
| Controllers | About 672 catch blocks in 93 controllers. 650 call `SafeErrorMessage` (`BaseController.cs:74`), which logs and returns a safe message or an allow-listed one |
| Services | Very few catch blocks; exceptions normally rise to the controller, which is the right shape. A few swallow them (section 2, G8) |
| Background work | No `Task.Run` or fire-and-forget calls in `Vsky.Api` or `Vsky.Services`. The only hosted service is `EndpointClassificationScan`. Service failures therefore happen inside a request |

### 1.4 Activity already logged

| Required activity | Status today | Evidence |
|---|---|---|
| Login succeeded, failed, locked; Microsoft login rejected | **[Existing], partial** | `LogAuthEvent`, `AuthController.cs:128` |
| Password reset requested, completed | **[Existing]** | `AuthController.cs:534, 585` |
| **Logout** | **Not captured: the API has no logout endpoint.** Sign-out only clears browser storage | `WEB/src/stores/auth.js:135` |
| Create, update, delete | **[Existing], partial:** action, method, status and site, but no record ID | `ActivityLogFilter.cs:50` |
| **Record access** | **Not captured:** reads are skipped | `ActivityLogFilter.cs:41` |
| **Module or menu access** | **Not captured** | No server record. The router has only a `beforeEach` guard |
| Role and permission changes (Security Admin) | **[Existing], strong:** with before and after values | `LogChange`, `SecurityAdminService.cs:1095` |
| **Role and permission changes (older endpoints)** | **Only a generic activity row**, with no target or values | Section 2, G3 |
| Group create, update, delete, membership, permissions | **[Existing], strong** | `SecurityAdminService.cs:410-628` |
| Site switching | **[Existing]** | `AuthorizationController.cs:123` |
| Permission denied by filters | **[Existing]**, limited to one row per minute per user, site, endpoint and reason | `AuthorizationAuditLog.cs:47` |
| **Denied by `[RequireSiteAdmin]` / `[RequireSystemAdmin]`** | **Not logged** | `AdminDenied.Result`, `RequireAdminAttributes.cs:72` |
| **401 Unauthorized** | **Not logged** | `OnChallenge`, `Program.cs:325` |

### 1.5 Pipeline and review screen

**Pipeline order:** dev exception page, response compression, exception handler, signed uploads, static files, forwarded headers, CORS, routing, rate limiter, `GlobalVariableMiddleware`, authentication, authorization, controllers.

**Global MVC filters (`Program.cs:170-173`):** `AuthorizationEnforcementFilter`, `SiteScopeActionFilter`, `PermissionCacheInvalidationFilter`, `ActivityLogFilter`.

**Endpoint tiers [Existing]:** every protected action declares `[RequireMenu]`, `[SiteMember]`, `[AuthenticatedOnly]`, `[RequireSiteAdmin]`, `[RequireSystemAdmin]` or `[ServiceKey]`. `EndpointPolicies.For(action)` exposes the tier and menu links, so a module can be identified without editing controllers.

**Audit screen [Existing]:** `SecurityAdminService.GetAuditLog` (`:808`) and `security-admin/pages/audit.vue`, with Denied, Would deny, Permission, Session and Activity categories. Its site filter searches `FullMessage` for the exact text `"siteId":"<id>"`.

---

## 2. Gaps and limitations

| # | Gap | Evidence | Consequence |
|---|---|---|---|
| G1 | **Logging can hide the original exception** | `DefaultLogger.InsertLog` can throw inside a catch block | The caller sees the logging error, and the real cause is lost |
| G2 | **Logging can save business data** | `DefaultLogger` saves through the request's DB context | Pending changes from a failed operation may be committed |
| G3 | **Older role and permission endpoints lack audit rows** | `RolesController` POST, PUT, DELETE; `ModulesMenusController` `savesitemenupermission`, `assignRolesToMenu`, `delete-role-access`; `ModulesController.savesitemodules`; `PersonSitesMappingController` `save-site-share-user`, DELETE; role changes in `UsersController:333, 437, 450` | These appear as "User activity", not "Permission change", so **a permission review misses them** |
| G4 | **No logout record** | Section 1.4 | Requirement can't be met without a small new endpoint and web change |
| G5 | **Session revocation isn't logged** | `AuthorizationController.cs:79`, `UsersController.cs:631` | A security-relevant action has no specific event |
| G6 | **Login events have gaps** | No row for "no site access" (`AuthController.cs:326, 335`). A wrong password for a known user writes "Login failed" with no user (`:391`) | Failed attempts against a real account can't be traced to it |
| G7 | **Some denials and all 401s are unlogged** | Section 1.4; also `SecurityAdminController.cs:586`, `AuthorizationController.cs:102` | Unauthorized attempts are incomplete |
| G8 | **Silent failures** | About 20 controller catches, plus `WorkflowMessageService.cs:597, 641, 704, 758`, `NotificationService.cs:224`, `EmailSender.cs:370`, and `Console.WriteLine` in `EmbedService.cs` | Failed emails and notifications leave no evidence |
| G9 | **Duplicate rows for one exception** | 5 `HelpDeskController` catches call both `_loggerService.Error` and `SafeErrorMessage` | Two rows, one with no user, IP or endpoint |
| G10 | **Exceptions lack context** | `SecurityLog.cs:35` stores no site, method, controller or record | Hard to tell what failed |
| G11 | **Activity lacks a record ID** | `ActivityLogFilter.cs:50` | Can't tell which record was changed |
| G12 | **The reference ID doesn't reach users on unhandled errors** | `Program.cs:705` serializes `AuthError` without MVC's JSON options, sending `Message`. The web app reads `message` (`boot/interceptors.js:186`) and falls back to generic text | Support can't look up the failure |
| G13 | **Inconsistent error formats** | Plain strings (about 640 sites), camelCase and PascalCase `ApiError`, `{code, message, referenceId}`, anonymous objects | Clients can't handle errors uniformly |
| G14 | **Failures returned as success** | `Ok(SafeErrorMessage(ex))` at `AllProjectPlannerController.cs:97`, `ProjectSwimlaneController.cs:94` | HTTP 200 on a failure |
| G15 | **Internal details reach clients** | `ReportsController.cs:105, 155` return Power BI response content; `EmailSender.cs:373` returns `ex.Message` | Exposure of internal information |
| G16 | **Log lost if the database is down** | `SecurityLog.cs:53` returns null | No record and no reference ID during an outage |
| G17 | **Exceptions aren't visible on any screen** | `GetAuditLog:826` shows only listed messages | Reference ID lookup needs SQL |
| G18 | **No retention** | `Log` has no cleanup process | Growth without limit once volume increases |

### Limits of the existing database structure

Each of these can be handled without a schema change, with the stated cost.

| Limitation | Alternative | Cost |
|---|---|---|
| No site, record, method or status columns | Compact JSON in `FullMessage` | Can't be indexed; searches need a date range |
| Site or record search is slow | Always filter by date, and by user where possible; `CreatedOnUtc` and `UserId` are indexed | Large unfiltered searches aren't supported |
| `UserId` foreign key | Retry with a null `UserId`, and keep the value in JSON | Rare; affects only a hard-deleted user |
| GUID clustered key suits high insert rates poorly | Log selectively; batch writes; retention | Logging all reads isn't advisable |
| API can't see the user's screen | Infer the module from the endpoint; optionally send a header (D3) and store it in `ReferrerUrl` | Backend-only gives the module, not always the exact page |

---

## 3. Proposed architecture

```
 Request
    v
 ActivityLoggingMiddleware     [New]      RequestId; timer; after the response, decides
    |                                     whether a row is needed and writes it
    v
 Response compression          [Existing]
    v
 ExceptionHandlingMiddleware   [New]      replaces Program.cs:684-708
    v
 Static files, forwarded headers, CORS, routing, rate limiter,
 GlobalVariableMiddleware, authentication, authorization       [Existing]
    v
 Filters
    AuthorizationEnforcementFilter, SiteScopeActionFilter      [Existing]
    ActivityLogFilter          [Improve]  collects record IDs and result text
    v
 Controller action             [Existing] catch { SafeErrorMessage } unchanged
    v
 Business services             [Existing] log only where they swallow an exception


 Writing rows
    ILogger / DefaultLogger    [Improve]  the one service for app code; never throws
    SecurityLog                [Improve]  same API and row format, for security events
    AuthorizationAuditLog      [Improve]  reports denials even when throttled
             |
             v
    LogWriter                  [New]      own DB context --> Log table
             |                            failure --> fallback file + Microsoft ILogger
             ^
    ActivityLogQueue           [New]      batched routine activity
```

### Where each mechanism is used

| Mechanism | Responsible for | Why here |
|---|---|---|
| **Middleware** | Unhandled exceptions; final outcome of every request, including 401, 403, 404 and 429 | Only it sees requests that never reach a filter |
| **Action filter** | Record IDs, new record IDs, result message | Only it can see action arguments and results |
| **Attribute** | Marking older role and permission endpoints as permission changes; opting actions in or out | One line per action, with no business logic touched |
| **Service or controller call** | Facts only that code knows: before and after values, swallowed failures, login outcome | Middleware can't infer these |

### Key decisions

| Decision | Choice | Reason |
|---|---|---|
| Reusable service | Extend `ILogger` | Already the injected pattern; existing calls keep working |
| Exception handling | Custom middleware | .NET 7 has no `IExceptionHandler`. Middleware also keeps route details, which `UseExceptionHandler` clears |
| Existing catch blocks | Keep | Removing them turns 400 responses into 500s, which changes behavior |
| Routine activity writes | Background queue | Adds no database insert to the request |
| Exception and security writes | Immediate | The reference ID must be real, and these rows must survive a crash |
| New logging products | None | The requirement is to reuse `Log`. No NuGet packages needed: Humanizer and the ASP.NET framework reference are already present |

### Event names

The audit screen matches `ShortMessage` exactly, so **names stay fixed and the detail goes in JSON.** This keeps old rows and existing filters working. Plan 1's `Activity: Create Help Desk` would have required changing that query.

| `ShortMessage` | Level | Status | Audit category |
|---|---|---|---|
| Login succeeded, Login failed, Login blocked: account locked, Microsoft login rejected | Information | [Existing] | session |
| Password reset requested, Password reset completed | Information | [Existing] | session |
| Site switched (`FullMessage` must remain the plain site ID: `SecurityAdminService.cs:835`) | Information | [Existing] | session |
| Login denied: no site access | Warning | [New] | session |
| Logout | Information | [New] | session |
| Sessions revoked | Warning | [New] | session |
| Permission change | Warning | [Existing], more sources | permission |
| Authorization denied | Warning | [Existing], more sources | denied |
| Authorization audit: would deny | Warning | [Existing] | would-deny |
| Authentication failed | Warning | [New] | denied |
| User activity | Information; Warning on failure | [Existing], enriched | activity |
| Record access | Information | [New] | activity |
| Module access | Information | [New] | activity |
| *Description of the failure* | Error | [Improve] | errors [New] |

### Field mapping

| Required field | Stored in |
|---|---|
| User | `UserId`; also `actorId` where the actor differs from the target |
| IP address | `IpAddress` |
| Endpoint | `PageUrl`: path only, never the query string |
| HTTP method, controller, action, status, duration, request ID | `FullMessage` JSON |
| Activity | `ShortMessage`, plus `activity` in JSON (for example "Create Help Desk", via Humanizer) |
| Site | `FullMessage` JSON as `"siteId":"..."`: the format the audit filter already searches for |
| Record | `FullMessage` JSON: `recordType`, `recordIds` |
| Timestamp | `CreatedOnUtc`: taken when the event happened, not when the queue saved it |
| Exception | `FullMessage` JSON: type, message, sanitized stack trace |

**Example activity row:**

```
ShortMessage: User activity          PageUrl: /help-desk
FullMessage:  {"siteId":"7f3c...","requestId":"b41e...","method":"POST","controller":"HelpDesk",
               "action":"CreateHelpDesk","activity":"Create Help Desk","module":"Help Desk",
               "recordType":"HelpDesk","recordIds":["a91d..."],"status":200,"durationMs":412}
```

---

## 4. Implementation phases

**Prerequisite:** commit or branch the current work first. The security work is uncommitted on `main`, and this plan changes the same files. `appsettings.Development.json` is untracked and `appsettings.json` is git-ignored, so configuration changes need applying per environment.

### Phase 1: Safe logging foundation (2 to 3 days)

1. Add `LogWriter`: its own DB scope, a generated ID before the write, a rolling file fallback outside the deployment folder, and a 30-second database skip after a failure.
2. Add `LogSanitizer` and `LogDetails` (the JSON shape).
3. Add `RequestLogContext` (scoped): request ID, record IDs, reference ID and a "security event already written" flag.
4. Change `DefaultLogger` to write through `LogWriter` and never throw. Read and delete methods stay.
5. Change `SecurityLog` to write through `LogWriter`:
   - Same API.
   - Stores non-empty `FullMessage` values exactly as passed.
   - Empty values get the standard context JSON.
   - Sets the "already written" flag for non-error events.
6. Add exception de-duplication (section 7) and route `SafeErrorMessage` through `ILogger`.

### Phase 2: Centralized exception handling (2 days)

1. Add `ExceptionHandlingMiddleware`, replacing `Program.cs:684-708` in the same position.
2. Add `ReferenceId` to `ApiError`, and serialize with the app's JSON options to fix G12.
3. Standard error format, **with the reference ID kept in `message`.** `getErrorMessage` displays only `message`, so otherwise the ID would never reach the user:

   ```json
   { "code": 500, "message": "An error occurred while processing your request. Reference id is a1b2...", "scoped": false, "referenceId": "a1b2..." }
   ```

4. Write the 401 response (`Program.cs:334`) in the same format.
5. Fix exposures at `ReportsController.cs:87, 103-106, 119, 153-156, 161` and `EmailSender.cs:370`.
6. Authorization denials keep their current format, because `boot/interceptors.js:128` depends on their text `code`.
7. Optional, D2: add `StandardErrorResultFilter`, which converts plain-string errors centrally behind a switch.

### Phase 3: Activity logging across the backend (3 to 4 days)

1. Add `ActivityLogQueue` and `ActivityLogBackgroundService`: a bounded `Channel`, written every 2 seconds or 200 rows, draining on shutdown.
2. Add `ActivityLoggingMiddleware`:
   - Runs first.
   - Writes the row in `finally`, so an exception rethrown after the response has started still gets one.
   - Sets `X-Request-Id` through `Response.OnStarting`, because the exception middleware's `Response.Clear()` would remove a header set directly.
   - Copies all values before queuing, because `HttpContext` isn't valid in the background writer.
   - Falls back to the `X-Site-Id` header, because `UseRateLimiter` runs before `GlobalVariableMiddleware` and a 429 never populates it.
3. Change `ActivityLogFilter` to stop writing rows and instead collect record IDs and result messages.
4. Add `[LogActivity]` and `[NoActivityLog]`.
5. Add field-name-only capture for validation failures at `Program.cs:205`.
6. Keep `Authorization:ActivityLogging` working as an off switch, alongside the new `ActivityLogging` section.

### Phase 4: Required events not yet covered (2 to 3 days)

1. **Older permission endpoints (G3):** add `[LogActivity(Category = Permission, Action = "...")]`, which writes "Permission change" with the target ID.
2. **Role changes in `UsersController`:** call the logger with before and after roles at `:333, 437, 450`.
3. **Logout (G4):** add `POST /auth/logout` with `[AuthenticatedOnly]`. It only records the event. The web app calls it on deliberate sign-out, ignoring any failure and before clearing the token.
4. **Sessions revoked (G5):** log in both `revoke-sessions` actions.
5. **Login gaps (G6):** log the two no-site-access branches, and pass `userData.Id` on a wrong password at `:391`.
6. **Denials (G7):** in the middleware, any 403 with no security row yet writes "Authorization denied"; any 401 writes "Authentication failed". No change to `RequireAdminAttributes`.
7. **Module access:** on a GET to a `[RequireMenu]` endpoint, write one "Module access" row per user, site and module every 30 minutes.

### Phase 5: Silent failures (1 to 2 days)

1. Add `BaseController.LogHandled(ex, description)` and use it in the catches that log nothing today, keeping their responses.
2. Add logging to the service catches in G8, keeping their current behavior.
3. Apply D5 to the two `Ok(SafeErrorMessage(ex))` sites, if approved.

### Phase 6: Review and retention (1 day)

1. Add new event names to `AuditCategories`.
2. Add an "errors" category filtered by level, **rejected on the server for anyone but System Super Admins** (D4), and left out of the default "all categories" view.
3. Make `search` match `Id`, so a reference ID finds its row.
4. Add the "Errors" option to `audit.vue`.
5. Add retention, off until D6 is confirmed.

### Phase 7: Testing and rollout (2 to 3 days)

Section 9 has the scenarios. Roll out as follows:

1. Deploy phases 1 and 2 to Dev and run the exception scenarios.
2. Turn on activity logging with record and module access off, and measure row counts.
3. Turn those on and watch `Log` growth for 2 to 3 days.
4. Repeat on production.

**Rollback:** set `ActivityLogging:Enabled` to false, or redeploy the previous build. There's no schema change to undo, and new rows stay readable by old code.

**Total:** about 2.5 to 3 weeks for one developer. Each phase can ship separately.

### Configuration

```json
"ActivityLogging": {
  "Enabled": true,
  "LogRecordAccess": true,
  "LogModuleAccess": true,
  "ModuleAccessWindowMinutes": 30,
  "ExcludedPaths": [ "/auth/refresh", "/swagger", "/profiler" ],
  "QueueCapacity": 10000,
  "FallbackPath": "<a folder outside the deployment>",
  "StandardizeStringErrors": false,
  "RetentionEnabled": false,
  "ActivityRetentionDays": 90,
  "ErrorRetentionDays": 365
}
```

---

## 5. Files

Paths are relative to `API/` unless shown otherwise.

### New files (12) [New]

| File | Reason |
|---|---|
| `Vsky.Api/Middleware/ExceptionHandlingMiddleware.cs` | Central handling of unhandled exceptions |
| `Vsky.Api/Middleware/ActivityLoggingMiddleware.cs` | Final outcome of every request; request ID; denial fallback |
| `Vsky.Api/Authorization/LogActivityAttribute.cs` | `[LogActivity]` and `[NoActivityLog]` |
| `Vsky.Api/ApiErrors/StandardErrorResultFilter.cs` | Optional conversion of plain-string errors (D2) |
| `Vsky.Services/Logging/ILogWriter.cs` | Write contract |
| `Vsky.Services/Logging/LogWriter.cs` | The single database writer, with fallback |
| `Vsky.Services/Logging/LogDetails.cs` | `FullMessage` JSON shape |
| `Vsky.Services/Logging/LogSanitizer.cs` | Masking and size limits |
| `Vsky.Services/Logging/RequestLogContext.cs` | Per-request state |
| `Vsky.Services/Logging/ActivityLogQueue.cs` | Bounded queue |
| `Vsky.Services/Logging/ActivityLogBackgroundService.cs` | Batch writer; optional retention |
| `Vsky.Services/Logging/LoggingOptions.cs` | Configuration |

### Existing files: logging and pipeline (12) [Improve]

| File | Reason |
|---|---|
| `Vsky.Api/Program.cs` | Registrations; replace the inline handler; add middleware; validation hook; 401 format |
| `Vsky.Api/Authorization/ActivityLogFilter.cs` | Collect record IDs and result text; stop writing rows |
| `Vsky.Api/Authorization/SecurityLog.cs` | Write through `LogWriter`; set the flag |
| `Vsky.Api/Authorization/AuthorizationAuditLog.cs` | Set the flag **before** the throttle check, so a throttled denial doesn't create a second row |
| `Vsky.Api/Controllers/BaseController.cs` | `SafeErrorMessage` uses `ILogger`; add `LogHandled` |
| `Vsky.Api/ApiErrors/ApiError.cs` | Add `ReferenceId` |
| `Vsky.Api/Extensions/ServiceExtensions.cs` | Expose `X-Request-Id` through CORS |
| `Vsky.Services/Logging/ILogger.cs` | Additive methods: `Activity`, `Audit`, `SetRecord`, `Error` with a record |
| `Vsky.Services/Logging/DefaultLogger.cs` | Never throws; writes through `LogWriter` |
| `Vsky.Services/SecurityAdmins/SecurityAdminService.cs` | Audit categories; errors gate; reference ID search |
| `Vsky.Api/appsettings.json` | `ActivityLogging` section; git-ignored, so set per environment |
| `Vsky.Api/appsettings.Development.json` | Same section; currently untracked |

### Existing files: controllers (24 of 93) [Improve]

| Controller | Reason |
|---|---|
| `AuthController` | Logout endpoint; login gaps; Warning log for the token failure at `:495`. Line 222 unchanged |
| `AuthorizationController` | Log session revocation |
| `UsersController` | Log role changes and session revocation |
| `RolesController`, `ModulesMenusController`, `ModulesController`, `PersonSitesMappingController` | Permission-change attributes only |
| `ReportsController` | Fix exposure; log |
| `AllProjectPlannerController`, `ProjectSwimlaneController` | D5 |
| `HelpDeskController` | Correct the "Email Send Failed" label at `:698` |
| `ExpenseController` | Log; remove doubled message text |
| `CompanyContactController`, `CompanyController`, `CronjobController`, `CustomerFilesController`, `DailyPlannerController`, `DomainController`, `LeadUserGroupMappingController`, `ProjectActionItemsController`, `ProjectModulesController`, `SalesPersonController`, `ServerController`, `TagsController` | Log catches that log nothing today; same responses |

**Plan 1's catch-block classification** is carried over here. Its totals match my counts, but exact lines must be reconfirmed during implementation, because the code is still changing.

### Existing files: services (4) [Improve]

| File | Reason |
|---|---|
| `Vsky.Services/Messages/WorkflowMessageService.cs` | Log 4 swallowed catches |
| `Vsky.Services/Notifications/NotificationService.cs` | Log 1 swallowed catch |
| `Vsky.Services/Messages/EmailSender.cs` | Log; remove `ex.Message` from the returned text |
| `Vsky.Services/PowerBI/EmbedService.cs` | Replace `Console.WriteLine` with the logger |

### Existing files: web (2, plus 1 optional) [Improve]

| File | Reason |
|---|---|
| `WEB/src/stores/auth.js` | Call `/auth/logout` on deliberate sign-out |
| `WEB/src/modules/security-admin/pages/audit.vue` | Add the "Errors" option |
| `WEB/src/boot/axios.js` | **Optional, D3:** send `X-Client-Page` |

### Not changed

- **By requirement:** `Vsky.Models/Log.cs`, `Vsky.Data/ApplicationDbContext.cs`, `Vsky.Database/*`.
- **Controllers:** the other 69, which are covered by middleware, filters and `SafeErrorMessage`.
- **Authorization logic:** `AuthorizationEnforcementFilter`, `SiteScopeActionFilter`, `RecordAttributes`, `RequireAdminAttributes`, `PermissionCacheInvalidationFilter`.
- **Other infrastructure:** `EfRepository`, `GlobalVariableMiddleware`.
- **Business history tables:** `HelpDeskStatusLog`, `IssueStatusChangedLog`, `RequirementChangeLog`, `ProjectTaskStatusLog`, `LeadActivityLogs` and similar. These are features, not diagnostics.

---

## 6. How each scenario is captured

### Exceptions

| Scenario | Captured by | User sees |
|---|---|---|
| Controller catch using `SafeErrorMessage` | `BaseController` → `ILogger.Error` | Same status; safe message and reference ID |
| Controller with no catch | `ExceptionHandlingMiddleware` | 500, standard format, reference ID |
| Service exception that propagates | The controller catch, or the middleware | As above |
| Service exception swallowed on purpose | `ILogger.Error` in the service | Operation continues as today |
| Exception in a filter or in middleware | `ExceptionHandlingMiddleware`, for anything inside it | 500, standard format |
| Exception after the response has started | Logged, then rethrown; activity row still written | Connection ends; the body can't be replaced |
| Client cancels the request | Not an error; activity row marked cancelled | Nothing |
| Validation failure | Activity row with field names only | Existing 400 |
| Database down during an exception | Fallback file, under the same reference ID | 500 with reference ID |

### Activities

| Required activity | Trigger | Row | Web change? |
|---|---|---|---|
| Login succeeded, failed, locked | `LogAuthEvent` [Existing, improved] | Session event | No |
| Microsoft login rejected | `LogAuthEvent` [Existing] | Session event | No |
| Logout | New endpoint | `Logout` | **Yes** |
| Session ends by expiry | 401 row only; **not treated as a logout** | `Authentication failed` | No |
| Create, update, delete, on all 93 controllers | Middleware and filter | `User activity` with record ID | No |
| POST actions that only read | Marked read with `[ReadAccess]`: treated as reads; otherwise as writes | Per classification | No |
| Record access | Middleware: single-record GET with a route ID | `Record access` | No |
| Module or menu access | Middleware: `[RequireMenu]` GET, once per window | `Module access` | No; exact page needs D3 |
| Role and permission changes, Security Admin | `LogChange` [Existing] | `Permission change` with before and after | No |
| Role and permission changes, older endpoints | `[LogActivity]` attribute | `Permission change` with target; **no before and after** (D7) | No |
| User's roles changed in `UsersController` | Explicit logger call | `Permission change` with before and after | No |
| Group create, update, delete | `LogChange` [Existing] | `Permission change` | No |
| Group membership | `LogChange` [Existing] | `Permission change` | No |
| Site switching | `AuthorizationController.cs:123` [Existing] | `Site switched` | No |
| Denied by enforcement, site scope or record filters | `AuthorizationAuditLog` [Existing] | `Authorization denied` | No |
| Denied by admin attributes or a controller 403 | Middleware fallback | `Authorization denied` | No |
| Unauthenticated, expired or invalid token | Middleware, throttled | `Authentication failed` | No |
| Cron and webhooks | Middleware; `"actor":"service"` | `User activity` | No |

`LeadUserGroupMapping` and `RequirementGroup` are business groupings, not security groups. They get standard `User activity` rows.

---

## 7. Preventing duplicates and logging failures

### Duplicates

| Situation | Rule | Result |
|---|---|---|
| Same exception logged twice, or wrapped by `EfRepository` | The first call stores its ID in `ex.Data`. Later calls check the `InnerException` chain and return that ID without writing | One row. Fixes the 5 `HelpDeskController` sites without editing them, and keeps the more specific description |
| Request already wrote a security event (login, site switch, permission change) | `SecurityLog` sets a flag; the middleware then skips its success row | One row per event |
| Filter denial followed by the middleware's 403 check | `AuthorizationAuditLog` sets the flag **even when its throttle skips the write** | Plan 1 would have defeated the throttle by adding a row for every denied call |
| Audit-mode "would deny", where the request continues | Flag not set for the denial | Audit row and normal activity row are both correct |
| Failed request | One error row and one activity row, sharing `requestId` | Intentional: one records what the user did, the other what failed |
| Polling or retry storms producing 401, 403, 404 or 429 | One row per user or IP, path and status, per minute, using the existing `IMemoryCache` pattern | Expired tokens on polling (`layout.vue:755`) don't flood the table |
| Module access | One row per user, site and module, per window | No row for every call on a page |
| List GETs, dropdowns, `/auth/refresh`, static files, `OPTIONS` | Not logged unless failing | Controls volume |

**Implementation note:** the throttle is per server process. A web farm could produce one extra row per server per window, which is acceptable.

### Logging failures

| Failure | Protection |
|---|---|
| Logger throws in a catch block | All `ILogger` write methods catch everything, and writes never use the request's DB context |
| Database unavailable | ID generated first; row goes to the fallback file under the same ID; database skipped for 30 seconds, so an outage doesn't add a timeout to each error |
| Fallback file also fails | Microsoft `ILogger<T>`; then the failure is ignored. The request still completes |
| `UserId` fails the foreign key | Retry with a null `UserId`; keep the value in JSON |
| One bad row in a batch | Batch retried row by row, so one row can't lose up to 199 others |
| Queue full | New activity rows are dropped and counted; the count is logged once a minute. Exceptions and security events bypass the queue |
| Middleware fails while writing the error response | Separately guarded; the original exception's status is still returned where possible |
| Shutdown or app pool recycle | Queue drains. Up to about 2 seconds of routine activity could be lost in a hard crash, which is accepted |

---

## 8. Security, risks, dependencies and decisions

### Never stored

- Request and response bodies.
- Query strings. `token` on `auth/user-by-token`, `serviceKey` and `sig` travel this way.
- `Authorization` and cookie headers.
- Field **values** from validation failures.

**Paths are safe to store:** I checked route templates, and no secret-bearing value appears in a path.

**Exception text:** `LogSanitizer` masks `Password=`, `Pwd=`, `AccountKey=`, `Bearer ...` and `SharedAccessSignature=`, and caps `FullMessage` at 64 KB. `ShortMessage` uses the description or exception type, not `ex.Message`, because `EfRepository` puts the full exception text in `Message`.

**Record IDs:** simple values only; named `id` or ending in `Id`; 64 characters or fewer; up to 20 per row.

### Risks

| # | Risk | Mitigation |
|---|---|---|
| R1 | **Wrong IP address.** `UseForwardedHeaders` (`Program.cs:717`) sets no known proxies, so behind another machine every row records the proxy | D1 |
| R2 | **Slow searches and table growth** | Date ranges required; no list reads; batching; retention (D6) |
| R3 | **Audit query slows with volume.** `GetAuditLog` runs `CountAsync` with text matching | Check in the load test |
| R4 | **Spoofed site ID.** `X-Site-Id` is client-supplied, especially on 401 rows, so a caller could place a row in another site's view | Treat as context. User, IP and time come from the server |
| R5 | **Secrets in logs** | Allow-lists, sanitizer, scenario V14 |
| R6 | **Stack traces shown to site admins** | Server-side gate (D4) |
| R7 | **Changing `FullMessage` breaks old filters** | "Site switched" stays plain; non-empty values stored exactly |
| R8 | **Constructor changes in 4 services** | All scoped; check for circular dependencies at startup |
| R9 | **Standard error conversion affects non-browser callers** such as n8n | Behind a switch (D2) |
| R10 | **Logout is best effort.** A closed browser sends nothing, and JWTs stay valid until expiry | Documented limit. The endpoint records intent; it doesn't revoke |
| R11 | **Personal data.** Named activity history is sensitive | D8; restricted access; retention |
| R12 | **Stack traces lack line numbers in production** | Publish `.pdb` files; scenario V18 |

### Dependencies

- No new NuGet packages or database objects.
- A writable fallback folder outside the deployment. `FolderProfile.pubxml` deletes existing files on publish, so `App_Data` isn't safe.
- Configuration applied per environment.
- .NET 7 is out of support. This plan doesn't depend on upgrading, but `IExceptionHandler` would become available on .NET 8.

### Decisions needed

| # | Decision | Recommendation | Trade-off |
|---|---|---|---|
| D1 | How does traffic reach the API: direct to IIS, or through a load balancer, CDN or proxy, and at which addresses? | *Needs your answer* | Determines whether `IpAddress` is correct |
| D2 | Convert about 640 plain-string errors to the standard format centrally? | **Yes, behind a switch, off at first** | Consistent format; non-browser callers need checking |
| D3 | Add the `X-Client-Page` header for the exact screen? | **Yes;** about half a day | Without it, module comes from the API, and an endpoint shared by several menus is ambiguous |
| D4 | Who can see exception details? | **System Super Admins only** | Stack traces reveal internals |
| D5 | Change `Ok(SafeErrorMessage(ex))` to `BadRequest` at 2 sites? | **Yes** | Users now see an error where they saw false success |
| D6 | Delete old rows? | **Activity after 90 days; errors and security events after 1 year,** subject to customer or audit requirements | Deletion is permanent; the job stays off until you confirm |
| D7 | Capture before and after values on older permission endpoints? | **Target and action now; before and after only for `UsersController` roles** | Full values on the rest means reading current state in each action, a larger and riskier change |
| D8 | Do employees need to be told about activity logging? | **Confirm with your legal or HR contact before production** | Not a technical decision |
| D9 | Store the attempted username when a login fails for an unknown account? | **No.** Known accounts record `UserId`; unknown ones record only IP | People sometimes type passwords in the username field |
| D10 | How much record access to log? | **Single-record GETs and module access only, not lists** | Logging everything gives more detail, but `Log` isn't built for that volume |

---

## 9. Verification scenarios

**Automated tests:** the repo has no test project. I recommend a small one for duplicate protection, sanitizing, record ID extraction and the suppression rules, since mistakes there fail silently.

### Exceptions

| # | Scenario | Expected |
|---|---|---|
| V1 | Force an exception in `CreateHelpDesk` | **One** error row with user, IP, endpoint, site and stack trace; the reference ID matches |
| V2 | Force an uncaught exception in `RolesController`, from the browser | 500, standard format, **reference ID visible in the toast;** CORS headers present |
| V3 | Throw from a service method with no catch | Same as V1 or V2, according to the controller |
| V4 | Stop SQL Server, then force an exception | 500 with reference ID; exception in the fallback file under that ID; no logging error replaces it |
| V5 | Exception after a pending, unsaved change | The change **isn't** saved by logging |
| V6 | Client cancels a request | No error row |
| V7 | Force a Power BI error | Safe message to the client; details in the log only |
| V8 | Fail an email send | Row exists; the user's action still succeeds |

### Activities

| # | Scenario | Expected |
|---|---|---|
| V9 | Successful login; wrong password; locked account; no site access; rejected Microsoft token | Exactly **one** correct session row each, with no extra `User activity` row |
| V10 | Log out; close the browser; let a token expire | `Logout` row only for the first; 401 row for an expired token; no false logout |
| V11 | Create, update and delete a ticket | One `User activity` row each, with record ID, including the new ticket's ID |
| V12 | Open a record; browse a module for an hour | One `Record access` row per open; one `Module access` row per window; no rows for lists |
| V13 | Every permission path: Security Admin roles, groups, members, permissions; `RolesController`; `savesitemenupermission`; `assignRolesToMenu`; `delete-role-access`; `savesitemodules`; `save-site-share-user`; `UsersController` role edit | Each appears in the **Permission changes** filter, once |
| V14 | Log in, reset a password, call `user-by-token`, use a service key | **Search `Log` and the fallback file: no passwords, tokens or keys** |
| V15 | Switch site, successfully and to a site not allowed | `Site switched` with plain site ID; denial row for the second |
| V16 | Denied by enforcement filter; by `[RequireSystemAdmin]`; repeat 100 times in a minute | One `Authorization denied` row per key per minute, **no extras** |
| V17 | Expired token on polling; unknown URL; rate limit | Throttled rows with 401, 404, 429; the 429 row has the site |

### Whole-backend coverage and non-functional

| # | Scenario | Expected |
|---|---|---|
| V18 | Production stack trace | Has line numbers |
| V19 | **Coverage scan:** a startup or test routine lists every action through `EndpointPolicies` and reports how each is logged. Then call one write per controller | All 93 controllers appear; none unclassified |
| V20 | **Behavior check:** compare status codes and bodies before and after on a sample from each controller | Unchanged, apart from D2 and D5 |
| V21 | Authorization matrix after the change, in Enforce and Audit modes | Same allow and deny outcomes |
| V22 | Audit screen: older rows, all filters, reference ID search; errors category as a site admin | Older rows display; site admin is refused |
| V23 | Load test, logging off vs on | Added latency under 5 ms at the 95th percentile |
| V24 | Recycle the app pool under load | Queue drains; no errors |

---

## 10. Implementation checklist, mapped to the requirement

### 1. Centralized exception handling

- [ ] Controllers, endpoints and services covered: `ExceptionHandlingMiddleware`, `SafeErrorMessage`, swallowed catches logged (V1-V3, V8)
- [ ] Standardized responses: `ApiError` with `referenceId` (V2; D2)
- [ ] No internal details exposed: exposure fixes; safe messages only (V7)
- [ ] Enough detail for troubleshooting: enriched JSON with sanitized stack trace (V1, V18)

### 2. User activity logging

- [ ] Login: gaps closed (V9)
- [ ] Logout: new endpoint and web call (V10)
- [ ] Create, update, delete: middleware and filter (V11)
- [ ] Record and module or menu access (V12; D3, D10)
- [ ] Role and permission changes, including older endpoints (V13; D7)
- [ ] Group create, update, delete, membership (V13)
- [ ] Site switching (V15)
- [ ] Unauthorized and denied attempts, including admin attributes and 401s (V16, V17)

### 3. Reusable architecture

- [ ] Centralized service: extended `ILogger`, with `LogWriter` as the single writer
- [ ] Middleware, filter and service roles as in section 3
- [ ] User, IP, endpoint, method, activity, site and timestamp captured (V1, V11)
- [ ] `Log` table only; no new tables or schema changes

### Implementation considerations

- [ ] Entire backend covered (V19)
- [ ] No duplicate or excessive entries (V9, V13, V16, V17)
- [ ] Logging failures can't interrupt requests or hide exceptions (V4, V5)
- [ ] Business logic, authorization and behavior preserved (V20, V21)
- [ ] Existing conventions followed; `SecurityLog`, `AuthorizationAuditLog`, `EndpointPolicies`, `RecordIds` reused
- [ ] Sensitive data never stored (V14)

### Sign-off

- [ ] Decisions D1 to D10 answered
- [ ] Current uncommitted work committed or branched
- [ ] Explicit approval to begin implementation

---

## 11. Differences from Plan 1

| Area | Plan 1 | This plan | Why |
|---|---|---|---|
| Error format | `message` without the ID, in `referenceId` only | **ID stays in `message`** | `getErrorMessage` displays only `message`, so Plan 1's own test (reference ID in the toast) would fail without a web change |
| Activity `ShortMessage` | `Activity: Create Help Desk` | Fixed `User activity`, with description in JSON | Exact-match audit query; old and new rows share a filter |
| Logout | Not covered | New endpoint and web call | Named requirement; no server event exists |
| Older permission endpoints | Generic activity | `Permission change` rows | Otherwise permission reviews miss them |
| Module or menu access | Optional | Backend `Module access` row, with optional header | Named requirement |
| Admin-attribute denials and 401s | Generic rows | Named security events | Named requirement |
| Denial duplicates | Every 403 gets an activity row | Flag set before the throttle | Plan 1 would defeat the existing throttle |
| Failure storms | Not throttled | Throttled per minute | Polling with an expired token would flood `Log` |
| Login gaps | Not covered | No-site-access and known-user failures logged | Evidence in `AuthController` |
| Session revocation | Not covered | Logged | Security-relevant |
| Batch failure | Retry a single row | Row-by-row retry | One bad row shouldn't lose a batch |
| `X-Request-Id`, 429 site | Not addressed | `OnStarting`; header fallback | Otherwise lost on errors and rate limits |
| Scope | 34 existing files | 42, plus 1 optional | Items above |

**Confirmed from Plan 1 by direct check:** 9 `ILogger` call sites; about 672 catches and 650 `SafeErrorMessage` uses; the `Log` schema; the PascalCase error defect; Humanizer and framework references; no fire-and-forget work; 550 write actions.

**Small corrections:** the two `Ok(SafeErrorMessage(ex))` returns are at lines 97 and 94; `AuthorizationAuditLog` does need a change; and Plan 1's scenario "background email send" actually runs within the request.
