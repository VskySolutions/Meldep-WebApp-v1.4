# Centralized Exception Handling and User Activity Logging: Implementation Plan

**Status:** Awaiting review and approval. No code, configuration or database changes have been made.
**Date:** 5 November 2026
**Scope:** ASP.NET Core backend (`API/`), .NET 7, EF Core 7, SQL Server

**Replaces:** `Customer Security/User-Activity-Tracking-Implementation-Plan.md`. That plan proposed a new table and new columns. This one follows the new requirement to keep the database structure as it is, so everything uses the existing `Log` table.

---

## 1. Summary

**Goal:** when a user reports an issue, the logs show who it happened to, what they did, which module and API they used, and what failed, without reproducing it.

**Approach:**

1. Keep the call style `CreateHelpDesk` already uses, `_loggerService.Error("description", ex)`, and make the existing `ILogger` the one logging service for the whole application.
2. Fix what's wrong underneath it. In `CreateHelpDesk`, one failure writes two rows, one of them with no user, IP or endpoint, and the logging call itself can hide the original exception.
3. Replace the inline exception handler in `Program.cs` with a dedicated middleware that returns one standard error format.
4. Add a second middleware that records one activity row per request, so all 93 working controllers are covered without editing each one.
5. Store site ID, record ID and other details as JSON in `Log.FullMessage`, because `Log` has no columns for them.

**Size of the change:**

| Item | Count |
|---|---|
| Database changes | **None** |
| New NuGet packages | **None** |
| New files | 12 |
| Existing files changed | 34: 11 logging and pipeline, 18 controllers, 4 services, 1 web page (section 9) |
| Controllers needing no edit | 75 of 93 |
| Rough effort | 2 to 2.5 weeks for one developer |

---

## 2. Existing logging approach

### 2.1 The `Log` table and entity

**Files:** `Vsky.Models/Log.cs`, `Vsky.Data/ApplicationDbContext.cs:329`, `Vsky.Database/schema_default.sql:122`

| Column | Type | Notes |
|---|---|---|
| `Id` | `nvarchar(450)` PK | GUID text; used as the reference ID shown to users |
| `ShortMessage` | `nvarchar(max)`, required | |
| `IpAddress` | `nvarchar(200)` | |
| `UserId` | `nvarchar(450)` | **Foreign key to `AspNetUsers`**, nullable |
| `LogLevelId` | `int` | 10 Debug, 20 Information, 30 Warning, 40 Error, 50 Fatal |
| `FullMessage` | `nvarchar(max)` | |
| `PageUrl` | `nvarchar(max)` | |
| `ReferrerUrl` | `nvarchar(max)` | **Never populated today** |
| `CreatedOnUtc` | `datetime2(6)` | |

**Existing indexes:** `CreatedOnUtc` and `UserId` (`schema_default.sql:261, 265`). These suit the main investigation, "this user, in this time range".

### 2.2 Two separate writers

| | `ILogger` / `DefaultLogger` | `SecurityLog` / `ISecurityLog` |
|---|---|---|
| Location | `Vsky.Services/Logging/` | `Vsky.Api/Authorization/`, interface in `Vsky.Services/Authorization/` |
| Call sites | 9 | `SafeErrorMessage`, `ActivityLogFilter`, `AuthorizationAuditLog`, auth events |
| DB context | **The request's own** | Its own, in a new scope |
| Fills in user, IP, path | **No** | Yes |
| Can throw | **Yes** | No, it returns null |

### 2.3 Reference implementation: `CreateHelpDesk`

`Vsky.Api/Controllers/HelpDeskController.cs:459-702`

```csharp
catch (Exception ex)
{
    _loggerService.Error("Email Send Failed", ex, null);   // line 698
    return BadRequest(SafeErrorMessage(ex));               // line 700
}
```

**What happens today:**

| Situation | What gets recorded |
|---|---|
| Success (line 690) | One `ActivityLogFilter` row: action, method, status, site. **No ticket ID**, though `entity.Id` is available |
| Exception (line 696) | **Two `Log` rows** for one failure (below) |
| `BadRequest("Help desk status not found")` (line 586) | Activity row with status 400, but **not the reason** |
| Invalid model (line 694) | Activity row with status 400, but no reason. A model invalid under `[ApiController]` never reaches the action or filter at all |
| Ticket history (line 588) | `AddHelpDeskStatusLog` writes to `HelpDeskStatusLog`. This is business history, separate from `Log`, and stays unchanged |

**The two rows from one exception:**

| Column | Row 1: `_loggerService.Error` | Row 2: `SafeErrorMessage` |
|---|---|---|
| `Id` | Generated, then discarded | Shown to the user as the reference ID |
| `ShortMessage` | `Email Send Failed` | `ex.Message` |
| `FullMessage` | `ex.ToString()` | `ex.ToString()` |
| `UserId`, `IpAddress`, `PageUrl` | **All null** | Filled in |
| `SiteId`, controller, action, ticket ID | Missing | Missing |

**What to keep from it, and what not to:**

| Keep as the standard | Don't carry forward |
|---|---|
| Injected `ILogger`, called from a catch block | Two rows per exception |
| A human-readable description with the exception | Saving through the request's DB context |
| Reference ID returned to the user | A logging failure replacing the real error |
| Safe message instead of exception details | The label "Email Send Failed" on any failure, including ones unrelated to email |

**Time values:** `CreatedOnUtc = GetDateTime` on business records holds the **site's local time**, despite the name. `Log.CreatedOnUtc` is true UTC and stays that way. Timelines should compare only `Log` rows, not mix them with business history times.

### 2.4 Exception handling across the backend

**Controllers:** I classified all 672 catch blocks in the 93 working controllers.

| Pattern | Count | Logged? | Action |
|---|---|---|---|
| `return BadRequest(SafeErrorMessage(ex))` | 632 | Once | None; improves automatically |
| `SafeErrorMessage` with another response shape | 11 | Once | Section 6.3 |
| `_loggerService.Error` **and** `SafeErrorMessage` | 5 | **Twice** | Fixed centrally |
| Empty catch or replacement response | 20 | **Never** | Add logging |
| Returns third-party error text to the client | 2 | **Never** | Fix; exposure risk |
| Logs through `_loggerService` only | 1 | Once | None |
| Expected failure, invalid token | 1 | No | None; correct as it is |

Four controllers have no catch blocks and rely on the global handler: `Authorization`, `ExpenseEmployee`, `ModulesMenus`, `Roles`.

**Services:** there are 169 service classes with only 30 catch blocks in `Vsky.Services`, `Vsky.Data` and `Vsky.Core` combined. Almost all exceptions rise to the controller, which is good: **services don't need logging in every method.** Only the ones that swallow exceptions need work.

---

## 3. Identified gaps

| # | Gap | Evidence | Consequence |
|---|---|---|---|
| G1 | **Logging can hide the original exception** | `DefaultLogger.InsertLog` can throw inside a catch block, for example when the database is failing | The caller gets the logging error, and the real cause is lost |
| G2 | **Logging can save business data** | `DefaultLogger` saves through the request's DB context, so pending changes from a failed operation can be committed with the log row | Risk of partial data after a failure |
| G3 | **Duplicate rows** | 5 sites in `HelpDeskController` | Misleading counts; one row has no context |
| G4 | **Exceptions lose their context** | `SecurityLog.cs:35` stores no site, controller or action or record | Can't tell what the failure relates to |
| G5 | **Activity has no record ID** | `ActivityLogFilter.cs:50` | Can't tell which ticket was touched |
| G6 | **Reads aren't recorded** | `ActivityLogFilter.cs:41` | Opening a record before the failure leaves no trace |
| G7 | **Requests stopped early aren't recorded** | 401, 403, invalid model, 404 and 429 end before an action filter runs | Common failures are missing |
| G8 | **Silent failures** | 20 controller catches, plus `WorkflowMessageService.cs:597, 641, 704, 758`, `NotificationService.cs:224` | Failed emails and notifications leave no evidence |
| G9 | **Exception rows aren't visible** | `SecurityAdminService.cs:826` shows only listed messages | A reference ID can only be looked up with SQL |
| G10 | **Log lost when the database is down** | `SecurityLog.cs:53` returns null | No record and no reference ID during an outage |
| G11 | **Reference ID never reaches the user on unhandled errors** | `Program.cs:705` serializes without the app's JSON options, so it sends `Message`. The web app reads `message` (`boot/interceptors.js:186`) and falls back to a generic text | Confirmed by reading the code; test scenario 2 will verify it |
| G12 | **Six different error formats** | Section 6.3 | Clients can't handle errors consistently |
| G13 | **Sensitive details reach clients** | `ReportsController.cs:105, 155` return the Power BI response content. `EmailSender.cs:373` returns `ex.Message` as a string | Internal details can be exposed |
| G14 | **Failures reported as success** | `AllProjectPlannerController.cs:95` and `ProjectSwimlaneController.cs:92` use `Ok(SafeErrorMessage(ex))` | HTTP 200 on a failure |
| G15 | **No retention** | Already noted in `Security-Implementation-Report.md` item 9 | `Log` grows without limit |
| G16 | **The screen's page isn't known to the API** | `PageUrl` holds the API path. The `Referer` header gives only the site's origin when the API is on another domain | Module comes from the API, not the screen; see section 7.3 |

---

## 4. Proposed architecture

```
 Request
    |
    v
 ActivityLoggingMiddleware     NEW  creates RequestId, starts timer; after the response,
    |                               queues one activity row with the final status
    v
 ResponseCompression                existing
    v
 ExceptionHandlingMiddleware   NEW  replaces the inline handler at Program.cs:684
    |                               catches unhandled exceptions; standard error response
    v
 Static files, Forwarded headers, CORS, Routing, Rate limiter     existing
 GlobalVariableMiddleware, Authentication, Authorization          existing
    v
 Filters
    AuthorizationEnforcementFilter, SiteScopeActionFilter         existing
    ActivityLogFilter          CHANGED  collects record IDs and the result message
    v
 Controller action
    catch { _loggerService.Error("...", ex) / SafeErrorMessage }   same as today
    v
 Business services
    log only where they swallow an exception


 Writing a row
    ILogger (DefaultLogger)   CHANGED  the one service for the app; never throws
       +-- Error, Warning, Exception --> LogWriter immediately
       +-- Activity -------------------> ActivityLogQueue --> background batch --> LogWriter
    SecurityLog               CHANGED  keeps its API; writes through LogWriter

    LogWriter                 NEW      own DB context --> Log table
                                       on failure --> fallback file, Microsoft ILogger
```

**Why these choices:**

| Decision | Choice | Reason |
|---|---|---|
| Reusable service | **Extend the existing `ILogger`** | It's the pattern in `CreateHelpDesk` and already injected in services; existing calls keep working |
| Exception handling | **Custom middleware** | .NET 7 has no `IExceptionHandler` (added in .NET 8). A custom middleware also keeps the route details, which `UseExceptionHandler` clears |
| Controller catch blocks | **Keep them** | Removing them changes 400 responses to 500, which changes what users and the web app see |
| Activity capture | **Middleware plus filter** | Middleware sees every outcome. Only the filter can see action arguments and results |
| Activity writes | **Background queue** | Avoids adding a database insert to each request |
| Exception writes | **Immediate** | The reference ID must be real, and these rows must survive a crash |
| Serilog or Application Insights | **Not used** | New dependency, and the requirement is to use the `Log` table |

---

## 5. How data maps to the existing `Log` columns

| Requirement | Column | Content |
|---|---|---|
| Reference ID | `Id` | GUID, created before the write so it exists even if the write fails |
| Activity description | `ShortMessage` | `Activity: Create Help Desk`, or the error description |
| Severity | `LogLevelId` | Information for success, Warning for 4xx, Error for exceptions |
| User | `UserId` | From the token. Anonymous and service calls are null |
| IP address | `IpAddress` | `Connection.RemoteIpAddress`; see risk R1 |
| Endpoint | `PageUrl` | Request path only, never the query string |
| Calling page | `ReferrerUrl` | `Referer` header, then `X-Client-Page` if the optional step is approved |
| UTC time | `CreatedOnUtc` | `DateTime.UtcNow`, taken when the event happened, not when the queue saved it |
| **Site ID** | `FullMessage` JSON | No column exists |
| **Record ID** | `FullMessage` JSON | No column exists |
| **Exception details** | `FullMessage` JSON | Type, message, stack trace, inner exceptions |
| Method, controller, status, duration, request ID | `FullMessage` JSON | No columns exist |

### Activity row example

```
ShortMessage: Activity: Create Help Desk
PageUrl:      /help-desk
FullMessage:
{"siteId":"7f3c...","requestId":"b41e...","method":"POST","controller":"HelpDesk",
 "action":"CreateHelpDesk","module":"Help Desk","recordType":"HelpDesk",
 "recordIds":["a91d..."],"status":200,"durationMs":412}
```

### Exception row example

```
ShortMessage: Help desk creation failed
LogLevelId:   40
FullMessage:
{"siteId":"7f3c...","requestId":"b41e...","method":"POST","controller":"HelpDesk",
 "action":"CreateHelpDesk","recordType":"HelpDesk","recordIds":[],
 "exception":{"type":"System.NullReferenceException","message":"...","details":"<ex.ToString()>"}}
```

**Why JSON:** the audit screen already shows `FullMessage` as formatted JSON (`audit.vue:76`), its site filter already looks for `"siteId":"..."`, and SQL Server can read it with `JSON_VALUE`.

**Existing security rows don't change format.** `SecurityAdminService.cs:835` compares `FullMessage` directly to the site ID for "Site switched" rows, so `SecurityLog.Write` must keep storing exactly what its callers pass.

**Rows looked up by user and date stay fast,** thanks to the existing indexes. Searching by site or record ID has to scan text, so those searches always need a date range. That's the main cost of not changing the schema; see risk R2.

---

## 6. Detailed design

### 6.1 Logging service

**Extend `Vsky.Services/Logging/ILogger.cs`.** All existing methods stay, and these are added:

```csharp
// Records an activity. Queued; returns immediately.
void Activity(string description, string recordType = null, string recordId = null);

// Records an exception with its affected record. Returns the reference ID.
string Error(string message, Exception exception, string recordType, string recordId);

// Adds the affected record to this request's activity row, for cases
// the automatic capture can't see.
void SetRecord(string recordType, params string[] recordIds);
```

**Rules for `DefaultLogger`:**

- Every method is wrapped so it **never throws**.
- All writes go through the new `LogWriter`, not the request's DB context. This fixes G1 and G2.
- Read and delete methods (`GetAllLogs`, `GetLogById` and so on) stay as they are.
- User, IP, endpoint, site and request ID come from the current request automatically, so callers don't pass them.

**`LogWriter` (new, singleton):**

- Uses its own DB scope, as `SecurityLog` does today.
- On failure, writes to a rolling fallback file and to Microsoft `ILogger<T>`.
- After a database failure, skips the database for 30 seconds, so an outage doesn't add a long timeout to every error.
- Handles a `UserId` that fails the foreign key by retrying with a null `UserId` and keeping the value in the JSON, so the row isn't lost.
- The fallback path is configurable and **should be outside the deployment folder.** `FolderProfile.pubxml` sets `DeleteExistingFiles` to true, so a file under `App_Data` could be deleted by a publish.

### 6.2 Exception-handling middleware

**New: `Vsky.Api/Middleware/ExceptionHandlingMiddleware.cs`**, replacing `Program.cs:684-708` in the same position.

```
try { await next }
catch (OperationCanceledException) when client aborted
    --> no error row; activity row records it as cancelled
catch (Exception ex)
    1. referenceId = logger.Error(description, ex)      never throws
    2. if the response has already started:
           record it and rethrow, because the body can't be replaced
    3. clear the response; set status 500, JSON
    4. write the standard error body, camelCase, with referenceId
```

**How the three requirements are met:**

| Requirement | How |
|---|---|
| **No duplicates** | The first log call stores its reference ID in `ex.Data`. Later calls for the same exception, or one wrapping it, find the ID through the `InnerException` chain and return it without writing. This fixes the 5 double-log sites **without editing them**, and the first description, the more specific one, is kept |
| **Logging failures don't hide exceptions** | `ILogger` methods can't throw. Steps 1 and 3 are also separately guarded, so the client gets a 500 with the reference ID regardless. If the database is down, the row goes to the fallback file under that same ID |
| **No sensitive exposure** | The client only ever gets the safe message and reference ID. Details stay in the log |

**`SafeErrorMessage` (`BaseController.cs:74`)** keeps its signature and behavior, including the `UserFacingMessages` list, but calls `ILogger.Error` so all 648 sites get full context and duplicate protection.

**A new `BaseController.LogHandled(ex, description)`** is added for the 20 catch blocks that log nothing today.

### 6.3 Standardized error responses

**Today's six formats:**

| Format | Example | Sites |
|---|---|---|
| Plain string | `BadRequest(SafeErrorMessage(ex))` | About 640 |
| `ApiError` family, camelCase | `{code, message, scoped}` | `BadRequestError`, `ValidationError` |
| `ApiError`, **PascalCase** | `{Code, Message, Scoped, Data}` | `Program.cs:334, 704` |
| Authorization denial | `{code:"SITE_NOT_MEMBER", message, referenceId}` | `AuthorizationAuditLog.cs:86` |
| Anonymous object | `{success:false, message}`, `{error}` | `Common:126`, `Site:292`, `HelpDesk:730` |
| Success status | `Ok("An error occurred...")` | 2 sites (G14) |

**Standard format, based on the existing `ApiError`:**

```json
{ "code": 500, "message": "An error occurred while processing your request.", "referenceId": "a1b2..." }
```

**Changes:**

1. Add a `ReferenceId` property to `ApiError`. Null values are already left out of MVC responses.
2. The new middleware and the 401 handler (`Program.cs:334`) write this format in camelCase. **This also fixes G11.**
3. **Optional, decision D2:** a new `StandardErrorResultFilter` converts `BadRequest("text")` to the standard format centrally, without editing about 640 sites. Checked for impact: apart from the central interceptor, only two places read error bodies directly (`finance-expense/components/addEdit.vue:831` and `requirement-status-tracking/utils/trackingForm.js:194`), and both read `.message`. Non-browser callers such as n8n need checking too, so it would have a config switch.
4. Authorization denials keep their format, because `boot/interceptors.js:128` depends on their text `code`.

**Status codes don't change,** except by decision D5.

**Sensitive exposure fixes:**

| Location | Change |
|---|---|
| `ReportsController.cs:103-106, 153-156` | Log the Power BI response; return a safe message with the reference ID |
| `ReportsController.cs:87, 119, 161` | Review `m_errorMessage`: it can hold configuration errors |
| `EmailSender.cs:370` | Log the exception. The returned string is kept without `ex.Message` |
| `ExpenseController.cs:697` and two similar | Remove the doubled "An error occurred: An error occurred..." text |

**Before storing,** `LogSanitizer` masks `Password=`, `Pwd=`, `AccountKey=`, `Bearer ...` and `SharedAccessSignature=` patterns in exception text, and caps `FullMessage` at 64 KB.

### 6.4 Activity logging

**New: `Vsky.Api/Middleware/ActivityLoggingMiddleware.cs`**, placed first in the pipeline, so it sees the final status of every request.

**What gets a row:**

| Request | Logged? | Level |
|---|---|---|
| All writes (550 POST, PUT and DELETE actions, including POST-based reads) | Yes | Information |
| All failures: 400, 401, 403, 404, 429, 5xx, on any method | Yes | Warning; Error for 5xx |
| GET of a single record (157 routes with a route value) | Yes, **decision D1** | Information |
| GET lists and dropdowns | No, **decision D1** | |
| `OPTIONS`, static files, `/auth/refresh`, polling, Swagger, profiler | No | Configurable list |
| Actions marked `[LogActivity("...")]` | Always, such as exports | Information |
| Actions marked `[NoActivityLog]` | Only if they fail | |
| Cron and webhooks (`[ServiceKey]`) | Yes, `"actor":"service"` in the JSON | Information |

**Descriptions are automatic.** `CreateHelpDesk` becomes "Create Help Desk" using Humanizer, which is already a dependency. `[LogActivity("...")]` can override this on an action, but isn't required.

**Module** comes from `EndpointPolicies.For(action).Links` where declared, otherwise from the controller name.

**`ActivityLogFilter` (changed)** stops writing rows and collects, for the middleware:

| Value | Source | Limits |
|---|---|---|
| Record ID, existing record | Route `id` (303 routes use this name), then action arguments, then a body DTO's `Id`, reusing `RecordIds.From` (`RecordAttributes.cs:190`) | Simple values only; name must be `id` or end in `Id`; 64 characters or fewer; up to 20 per row |
| Record ID, new record | `Id` property of the `Ok(entity)` result. This gives the ticket ID in `CreateHelpDesk`, without editing it | Single objects only, not lists |
| Result message | Text of a `BadRequest("...")` result. This captures "Help desk status not found" | 500 characters |
| Validation failure | Field names only, never values, from `InvalidModelStateResponseFactory` (`Program.cs:205`) | |

**Never logged:** request and response bodies, query strings, and the `Authorization` header. `token` on `auth/user-by-token`, `serviceKey` and `sig` all travel in query strings, which is why these are excluded outright.

**Linking rows:** each exception and its activity row share a `requestId`, and the activity row also stores the exception's reference ID. This is how `Ok(SafeErrorMessage(ex))` and `BadRequest` failures can still be identified whatever their HTTP status.

**Background queue:** a bounded `Channel` feeds a hosted service that writes a batch every 2 seconds or 200 rows. It drains on shutdown, and writes to the fallback file on failure.

### 6.5 Business services

**Rule: no logging in every service method.** Exceptions rise to the controller, and the middleware already records the request. Add logging only where a service would otherwise hide a failure.

| File | Lines | Change |
|---|---|---|
| `Messages/WorkflowMessageService.cs` | 597, 641, 704, 758 | Log; keep swallowing |
| `Notifications/NotificationService.cs` | 224 | Log; keep swallowing |
| `Messages/EmailSender.cs` | 370 | Log; remove `ex.Message` from the returned text |
| `PowerBI/EmbedService.cs` | 80, 303, 411, 503 | Replace `Console.WriteLine` with the logger |
| `AzureBlobImage/AzureBlobImageServices.cs` | 184 | None; gains full context automatically |

**No change needed:** `RequirementStatusTrackingService` (roll back and rethrow), `SmtpBuilder` (rethrows), `EfRepository` (wraps and rethrows), `Tokenizer`, `CommonHelper`, `AppFileProvider`, `AadService` (expected failures).

**Note on `EfRepository`:** it puts the whole exception text in `Message`, so `ex.Message` can be very long. `ShortMessage` will therefore use the description or exception type, not `ex.Message`.

### 6.6 Reviewing the logs

The audit screen already filters by user and date, which is the core of an investigation. Small changes make it usable for this purpose, with no new screens:

- **`SecurityAdminService.GetAuditLog`:**
  - Include `Activity:` rows in the "activity" category, alongside older "User activity" rows.
  - Add an "errors" category for rows at Error level and above.
  - Make `search` also match `Id`, so a reference ID finds its row directly.
- **`audit.vue`:** add one "Errors" option to the dropdown. This is the only required web change.
- **Access:** see decision D4.

**SQL for a full timeline:**

```sql
SELECT CreatedOnUtc, ShortMessage, PageUrl, IpAddress,
       JSON_VALUE(FullMessage, '$.siteId')   AS SiteId,
       JSON_VALUE(FullMessage, '$.status')   AS Status,
       JSON_VALUE(FullMessage, '$.recordIds[0]') AS RecordId
FROM   Log
WHERE  UserId = @userId AND CreatedOnUtc BETWEEN @from AND @to
ORDER  BY CreatedOnUtc;
```

Older rows that aren't JSON would break `JSON_VALUE`, so production queries should add `ISJSON(FullMessage) = 1`.

---

## 7. Decisions and open items

### 7.1 Decisions needed

| # | Decision | Recommendation | Trade-off |
|---|---|---|---|
| D1 | Which reads to log? | **Single-record GETs only,** not lists. Controlled by config | Shows what the user opened while keeping `Log` manageable. All reads gives more detail, but this table isn't designed for that volume |
| D2 | Convert about 640 plain-string errors to the standard format centrally? | **Yes, behind a switch,** after testing in Dev | Gives one format everywhere. Check any non-browser callers first |
| D3 | Delete old rows? | **Yes: activity after 90 days, errors and security events after 1 year,** subject to customer or audit requirements | Deleting is permanent. With no retention, the table grows without limit. The job stays off until you confirm |
| D4 | Who can see exception details? | **System Super Admins only.** Site admins see activity for their own site | Stack traces reveal internal details and may quote data |
| D5 | Change `Ok(SafeErrorMessage(ex))` to `BadRequest` at the 2 sites? | **Yes.** Those pages currently treat failures as success | Small behavior change: the user now sees an error. Test both pages |
| D6 | How does traffic reach the API? | *Needs your answer:* direct to IIS, or through a load balancer, CDN or proxy, and at which addresses? | Determines whether `IpAddress` is correct (R1) |
| D7 | Do employees need to be told? | **Confirm with your legal or HR contact** before production | Named activity history is personal data |

### 7.2 Prerequisite

**Commit or branch the current work first.** The security work is uncommitted on `main`, including `SecurityLog.cs`, `ActivityLogFilter.cs` and `Program.cs`, which this plan changes.

### 7.3 Optional, not included

**Showing the exact screen.** The API can't know the user's screen (G16). A small change to `WEB/src/boot/axios.js` could send an `X-Client-Page` header, stored in `ReferrerUrl`. Browser errors would need a further web change and endpoint. Both are left out because this request covers the backend, but page-level detail isn't available without them. I'd add the header; it's about half a day.

---

## 8. Risks

| # | Risk | Mitigation |
|---|---|---|
| R1 | **Wrong IP address.** `UseForwardedHeaders` (`Program.cs:717`) sets no known proxies, so only a proxy on the same machine is trusted. Behind another proxy, every row records the proxy's address | D6; set the real proxy addresses |
| R2 | **Slow searches and table growth.** Site and record search scans `nvarchar(max)` text, and `Log`'s GUID primary key is a poor fit for high insert rates | No list reads; required date ranges; batch writes; retention. If it becomes a real problem, the fix is an index or table, which needs your approval as a schema change |
| R3 | **Existing queries affected by volume.** `GetAuditLog` runs `CountAsync` with text matching | Date range required; check query time in the load test |
| R4 | **Activity lost in a hard crash,** up to about 2 seconds of queued rows | Accepted. Exceptions and security events are written immediately |
| R5 | **Spoofed site ID.** `X-Site-Id` isn't enforced while `SiteGateMode` is Audit | Treat as context. User, IP and time come from the server |
| R6 | **Secrets in logs** | Allow-lists, no bodies or query strings, sanitizer, test scenario 8 |
| R7 | **Constructor changes in 4 services,** to inject `ILogger` | All are scoped services, so DI handles it. Check for circular dependencies at startup |
| R8 | **Log noise from expected errors** | Client cancellations excluded; 4xx rows use Warning |

---

## 9. Complete list of files

### New files (12)

| File | Purpose |
|---|---|
| `Vsky.Api/Middleware/ExceptionHandlingMiddleware.cs` | Central handling of unhandled exceptions |
| `Vsky.Api/Middleware/ActivityLoggingMiddleware.cs` | One activity row per request; request ID |
| `Vsky.Api/Authorization/LogActivityAttribute.cs` | `[LogActivity]` and `[NoActivityLog]` |
| `Vsky.Api/ApiErrors/StandardErrorResultFilter.cs` | Optional string-to-standard conversion (D2) |
| `Vsky.Services/Logging/ILogWriter.cs` | Write contract |
| `Vsky.Services/Logging/LogWriter.cs` | The one database writer, with file fallback |
| `Vsky.Services/Logging/LogDetails.cs` | The `FullMessage` JSON shape |
| `Vsky.Services/Logging/LogSanitizer.cs` | Masking and length limits |
| `Vsky.Services/Logging/RequestLogContext.cs` | Per-request request ID, record IDs, reference ID |
| `Vsky.Services/Logging/ActivityLogQueue.cs` | Bounded queue |
| `Vsky.Services/Logging/ActivityLogBackgroundService.cs` | Batch writer and optional retention (D3) |
| `Vsky.Services/Logging/LoggingOptions.cs` | `ActivityLogging` configuration |

### Existing files: logging and pipeline (11)

| File | Change |
|---|---|
| `Vsky.Api/Program.cs` | Register services; replace `:684-708`; add middleware; validation hook at `:205`; camelCase at `:334` |
| `Vsky.Api/Authorization/ActivityLogFilter.cs` | Collect record IDs and result message; stop writing rows |
| `Vsky.Api/Authorization/SecurityLog.cs` | Write through `LogWriter`; same API and row format; returns an ID even if the write falls back |
| `Vsky.Api/Controllers/BaseController.cs` | `SafeErrorMessage` uses `ILogger`; add `LogHandled` |
| `Vsky.Api/ApiErrors/ApiError.cs` | Add `ReferenceId` |
| `Vsky.Api/Extensions/ServiceExtensions.cs` | Expose the `X-Request-Id` header through CORS |
| `Vsky.Services/Logging/ILogger.cs` | New methods (additive) |
| `Vsky.Services/Logging/DefaultLogger.cs` | Never throws; writes through `LogWriter` |
| `Vsky.Services/SecurityAdmins/SecurityAdminService.cs` | Audit categories and reference ID search |
| `Vsky.Api/appsettings.Development.json` | `ActivityLogging` section |
| `Vsky.Api/appsettings.json` | Same section. **This file is git-ignored,** so each environment needs it set by hand; code defaults apply until then |

### Existing files: controllers (18)

| Controller | Lines | Change |
|---|---|---|
| `AllProjectPlannerController` | 95 | D5 |
| `AuthController` | 495 | Log at Warning level. **Line 222 is unchanged** |
| `CompanyContactController` | 124 | Log empty catch |
| `CompanyController` | 223, 392 | Log empty catch |
| `CronjobController` | 241 | Log empty catch |
| `CustomerFilesController` | 286 | Log empty catch |
| `DailyPlannerController` | 358, 567 | Log; same response |
| `DomainController` | 133, 273 | Log empty catch |
| `ExpenseController` | 458, 697 | Log; remove doubled message text |
| `HelpDeskController` | 698 | Change the label to "Help desk creation failed". Other sites unchanged |
| `LeadUserGroupMappingController` | 65 | Log; same response |
| `ProjectActionItemsController` | 212 | Log empty catch |
| `ProjectModulesController` | 218 | Log; same fallback value |
| `ProjectSwimlaneController` | 92 | D5 |
| `ReportsController` | 87, 103-106, 119, 153-156, 161, 219, 450 | Fix exposure; log |
| `SalesPersonController` | 121 | Log empty catch |
| `ServerController` | 163, 272 | Log empty catch |
| `TagsController` | 140 | Log empty catch |

`ExpenseAdvanceRequestController:598` and `ExpensePurchaseRequestController:479` have the same doubled text as `ExpenseController:697`, so they're included in the review below.

### Existing files: services (4) and web (1)

| File | Change |
|---|---|
| `Vsky.Services/Messages/WorkflowMessageService.cs` | Inject `ILogger`; log 4 catches |
| `Vsky.Services/Notifications/NotificationService.cs` | Inject `ILogger`; log 1 catch |
| `Vsky.Services/Messages/EmailSender.cs` | Inject `ILogger`; log and remove `ex.Message` |
| `Vsky.Services/PowerBI/EmbedService.cs` | Inject `ILogger`; replace `Console.WriteLine` |
| `WEB/src/modules/security-admin/pages/audit.vue` | Add "Errors" option |

### Reviewed, needing no edit

**Controllers (75), covered by the middleware and `SafeErrorMessage`:** Account, AdPostChannel, AdPost, Authorization, Calendar, Candidate, Common, Contact, Customer, Departments, DropDownTypes, DropDowns, EmailNotification, EmployeeLeave, EmployeeOrgStructures, Employees, ExpenseAdvanceRequest\*, ExpenseBankAccount, ExpenseEmployee, ExpensePurchaseRequest\*, ExpenseVendorBankAccounts, HelpDeskTopicsQuestions, InfraAccount, InfraAccountServices, InfraDashboard, InfraDatabase, InfraFTP, InfraProjectInstance, Inventory, Issue, ItemCategory, ItemSubCategoryAttributes, JobCreate, Leads, LeaveCredit, LeaveRules, LeaveSchedule, Modules, ModulesMenus, MovementRegister, Notes, Notification, Person, PersonSitesMapping, ProjectActivities, ProjectDashboard, ProjectModulesUserMapping, ProjectQuestionsAnswers, ProjectReleaseTracking, ProjectTasks, ProjectUserMapping, Projects, ReportUserMapping, Requirement, RequirementDashboard, RequirementDataMapping, RequirementGroup, RequirementStatusTracking, Roles, SOPAssignment, SOPProcess, SOPTemplate, SecurityAdmin, Site, SiteProjectRoles, SitesItemSubCategoryAttributesMapping, SitesItems, TestCase, TestPlan, TimeInTimeOut, TimesheetAISummary, Timesheet, TrainingPortal, Users, WebsiteDemos.

\* One-line message cleanup only, if you want the doubled "An error occurred" text removed there too.

**Other projects and files:**

- **Not changed, by requirement:** `Vsky.Models/Log.cs`, `Vsky.Data/ApplicationDbContext.cs`, `Vsky.Database/*`
- **Not changed:** `Vsky.Core`, `Vsky.Api.Framework`, `EfRepository`, `GlobalVariableMiddleware`, `AuthorizationAuditLog`, `AuthorizationEnforcementFilter`, `SiteScopeActionFilter`, and all other services
- **Module-specific history stays as it is:** `HelpDeskStatusLog`, `IssueStatusChangedLog`, `RequirementChangeLog`, `ProjectTaskStatusLog`, `LeadActivityLogs` and similar. These are business features, not diagnostics

---

## 10. Implementation phases

| Phase | Work | Estimate | Can ship alone? |
|---|---|---|---|
| 1 | `LogWriter`, sanitizer, `DefaultLogger` and `SecurityLog` changes, duplicate protection, enriched exception rows | 2 to 3 days | Yes |
| 2 | Exception middleware, standard error format, exposure fixes | 2 days | Yes |
| 3 | Activity middleware, filter changes, queue, attributes | 3 days | Yes; has a config switch |
| 4 | Catch block fixes in the listed controllers and services | 1 to 2 days | Yes |
| 5 | Audit screen changes, retention (if approved) | 1 day | Yes |
| 6 | Testing and rollout | 2 to 3 days | |

**Configuration:**

```json
"ActivityLogging": {
  "Enabled": true,
  "LogSingleRecordReads": true,
  "LogListReads": false,
  "ExcludedPaths": [ "/auth/refresh", "/swagger", "/profiler" ],
  "QueueCapacity": 10000,
  "FallbackPath": "<a folder outside the deployment>",
  "StandardizeStringErrors": false,
  "RetentionEnabled": false,
  "ActivityRetentionDays": 90,
  "ErrorRetentionDays": 365
}
```

The existing `Authorization:ActivityLogging` flag continues to turn activity rows off, so current rollback instructions stay valid.

---

## 11. Testing

The repo has no automated test project. I recommend adding a small one for duplicate protection, sanitizing and record ID extraction, since mistakes there are silent. The rest is tested by these scenarios:

| # | Scenario | Expected result |
|---|---|---|
| 1 | Force an exception inside `CreateHelpDesk` | **One** error row with user, IP, endpoint, site and stack trace; reference ID shown matches it; description is from line 698 |
| 2 | Force an uncaught exception in a controller with no catch, such as `Roles` | 500 in the standard format; **the reference ID appears in the toast** (G11) |
| 3 | Create a help desk ticket successfully | Activity row with the new ticket's ID |
| 4 | Expired token; denied action; invalid model; unknown URL; rate limit | Activity rows with 401, 403, 400 (field names only), 404, 429 |
| 5 | Stop SQL Server, then force an exception | Client still gets the 500 and reference ID; original exception is in the fallback file under that ID; **no logging error replaces it** |
| 6 | Exception after a pending, unsaved change | The pending change **isn't** saved by logging (G2) |
| 7 | Client cancels a request | No error row |
| 8 | Log in; reset a password; call `user-by-token`; use a service key | **Search `Log` and the fallback file: no passwords, tokens or keys** |
| 9 | Force a Power BI error | Client gets a safe message; details in the log only |
| 10 | Failure in a background email send | Row exists; the user's action still succeeds as it does today |
| 11 | Cron call and Twilio webhook | Rows marked as service; no user |
| 12 | Audit screen: older rows, "Site switched" filter, reference ID search | Older rows still display; existing filters work |
| 13 | Load test, activity logging off vs on | Added latency under 5 ms at the 95th percentile |
| 14 | Production stack trace | Has line numbers, which requires `.pdb` files in the published output |

**Rollout:**

1. Deploy phases 1 and 2 to Dev, then run scenarios 1, 2, 5, 6 and 9.
2. Turn on activity logging with `LogSingleRecordReads: false`, then measure row counts.
3. Turn on single-record reads and watch `Log` growth for 2 to 3 days.
4. Repeat on production.

**Rollback:** set `ActivityLogging:Enabled` to false, or redeploy the previous build. With no schema change, there's nothing to undo in the database, and new rows remain readable by the old code.

---

## 12. Approval checklist

- [ ] Decisions D1 to D7 answered
- [ ] Architecture approved (section 4)
- [ ] `FullMessage` JSON approach accepted for site ID and record ID (section 5)
- [ ] File list approved (section 9)
- [ ] Optional `X-Client-Page` header: include or leave out (section 7.3)
