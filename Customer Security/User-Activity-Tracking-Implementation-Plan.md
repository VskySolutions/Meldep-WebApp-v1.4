# User Activity Tracking and Issue Investigation: Implementation Plan

**Status:** Awaiting review and approval. No code or database changes have been made.
**Date:** 5 November 2026
**Scope:** API (`API/`), web app (`WEB/`), SQL Server

---

## 1. Summary

The application already logs exceptions and writes, but it can't yet answer the question this work is for: **"What was this user doing when it failed?"**

- Page views and data reads aren't recorded, so the steps before a failure are missing.
- Log rows have no shared ID that ties a request, its activity and its exception together.
- Exception rows have no site, controller or action, and no screen shows them.

**Recommendation:** extend what exists rather than add a logging product.

1. Keep the `Log` table for exceptions and security events, and add structured columns to it.
2. Add a `UserActivityLog` table for high-volume activity, written in the background so requests don't slow down.
3. Record every request in one middleware, because the current action filter can't see many failed requests.
4. Have the web app send page navigation and browser errors.
5. Add investigation screens under Security Administration.

No catch block needs editing for the main exception path: 651 controller catch blocks already call `SafeErrorMessage`, which goes through `SecurityLog`, so improving `SecurityLog` improves all of them.

**Rough effort:** 3 to 4 weeks for one developer, delivered in phases that can each ship separately.

### What an investigation will look like

An admin opens the timeline from the reference ID shown in the user's error message:

| Time (site time) | Type | Page | Action | Record | Result |
|---|---|---|---|---|---|
| 10:41:02 | Sign-in | Login | Login succeeded | | OK |
| 10:41:40 | Page | Issues | Opened page | | |
| 10:41:41 | API | Issues | `GET Issue.GetAll` | | 200, 180 ms |
| 10:42:15 | API | Issues | `GET Issue.GetById` | Issue 8842 | 200, 45 ms |
| 10:43:09 | API | Issues | `PUT Issue.Update` | Issue 8842 | **400, reference a1b2...** |
| | | | `DbUpdateException` and full stack trace (expanded on click) | | |
| 10:43:12 | Browser error | Issues | `TypeError` in the issue form | | |

Each row also holds the user, site, IP address and browser.

---

## 2. Requirements coverage today

| Requirement | Today | Gap | Fixed in |
|---|---|---|---|
| Which user | Partial: `Log.UserId` | Row is lost if its user can't be linked; no username snapshot | Phases 1, 2 |
| Which module or page | **No:** `PageUrl` holds the API path, not the screen | The API doesn't know the user's page | Phases 2, 3 |
| Which record | **No** | Record IDs aren't captured | Phase 2 |
| Actions before the issue | **Writes only** | The 312 `[HttpGet]` actions and POST-based reads are skipped; page views aren't recorded | Phases 2, 3 |
| Which API, controller or method | Writes only, as JSON text | Exception rows don't have it | Phases 1, 2 |
| Exception and stack trace | Mostly: `ex.ToString()` in `FullMessage` | Some service failures are swallowed; no screen shows them | Phases 1, 4 |
| Date and time | Yes: `CreatedOnUtc` | Needs showing in site time | Phase 4 |
| IP address | Yes, **if the proxy setup allows it** | See risk R1 | Phase 0 |
| Site | **No column:** JSON text on some rows only | Exception rows have none | Phases 1, 2 |
| Chronological reconstruction | **No** | No shared request or session ID | Phases 1 to 4 |

---

## 3. Current state: findings

### What can be reused

| Item | Location | Use |
|---|---|---|
| `SecurityLog`: separate DB context, never throws | `API/Vsky.Api/Authorization/SecurityLog.cs` | Central exception writer; enrich it |
| `SafeErrorMessage` and reference IDs | `API/Vsky.Api/Controllers/BaseController.cs:74` | Already shown to users; becomes the lookup key |
| Global exception handler | `API/Vsky.Api/Program.cs:684` | Catches anything uncaught |
| `ActivityLogFilter` | `API/Vsky.Api/Authorization/ActivityLogFilter.cs` | Narrow it to capturing record IDs |
| `EndpointPolicies` and `RecordIds` | `API/Vsky.Api/Authorization/` | Map an action to its module; read record IDs |
| Security audit screen | `WEB/src/modules/security-admin/pages/audit.vue` | Pattern and home for the new screens |
| Route `meta.title` | `WEB/src/modules/*/routes.js` | Readable page names |
| Empty error hooks, already first in boot order | `WEB/src/boot/error.js` | Browser error capture |
| Tenant headers on every request | `WEB/src/boot/axios.js:57` | Add page and session headers here |

### Problems to fix

| # | Finding | Evidence | Effect on an investigation |
|---|---|---|---|
| F1 | Reads aren't logged | `ActivityLogFilter.cs:41` returns early for GET and View-level actions | Can't see what the user did first |
| F2 | The filter misses failed requests | Authentication (401), authorization denials, invalid models (400), rate limits (429) and unmatched routes (404) all finish before an action filter runs | The failures people report are often the ones missing |
| F3 | No correlation ID | `Log` has no such column | Activity and exceptions can only be matched by guessing from time |
| F4 | Exception rows lack context | `SecurityLog.cs:35` sets only level, messages, user, IP, path and time | Controller, action and site aren't available as data |
| F5 | **Exceptions aren't visible in any screen** | `SecurityAdminService.cs:826` shows only listed security messages, so `Error` rows never appear | A reference ID can only be looked up with SQL |
| F6 | Logs vanish if the database fails | `SecurityLog.cs:53` returns null with no fallback | An outage leaves no record and no reference ID |
| F7 | Swallowed service exceptions | Empty catches at `WorkflowMessageService.cs:597, 641, 704, 758` and `NotificationService.cs:224`; a TODO at `EmailSender.cs:370`; `Console.WriteLine` at `EmbedService.cs:305` | Failed emails and notifications leave no trace |
| F8 | The older logger is unsafe | `DefaultLogger.InsertLog` saves through the request's DB context, which can also save or retry that request's pending changes. It records no user, IP or path. It has 9 call sites | Logging can alter data or fail inside a catch block |
| F9 | Every write adds a synchronous database insert | `SecurityLog.cs:50` | Logging reads this way would slow every page |
| F10 | Site filter searches JSON text; no retention | `SecurityAdminService.cs:830`; already noted in `Security-Implementation-Report.md` item 9 | Screens slow down and the table grows without limit |
| F11 | A real server error can look like a client error | Catch blocks return `BadRequest`, so the status is 400 | Status alone can't identify failures. Link the rows; **don't change the status**, because the web app relies on it |
| F12 | Aborted requests are likely to be logged as errors | Nothing filters them | Cancelled requests would look like faults |

**Checked and ruled out:** I suspected the `Log.UserId` foreign key might reject rows because of how the token stores the user. It doesn't: `sub` holds `user.Id` (`AuthController.cs:152`) and maps to the claim `SecurityLog` reads.

---

## 4. Proposed design

```
Browser                                  API
--------------------                     ----------------------------------------------
router.afterEach (page views)  --+       RequestTrackingMiddleware (outermost)
Vue / window errors            --+-->      assigns RequestId, times the request, and
                                 |         always adds X-Request-Id to the response
POST /client-logs (batched)   <--+           |
                                             v
axios: X-Client-Session,                   Existing pipeline, exception handler, filters
       X-Client-Page          -------->      ActivityLogFilter adds record IDs
                                             SecurityLog adds context to exceptions
                                             |
                       Security events and   |    Activity rows, through a bounded
                       exceptions, written   |    in-memory queue
                       immediately           v
                       Log table <--RequestId-->  UserActivityLog table
                            |                         |
                       file fallback             background batch writer
                            +-----------+-------------+
                                        v
                         Investigation screens and timeline API
```

### Why a separate activity table

| Option | Verdict | Reason |
|---|---|---|
| Keep everything in `Log` | Rejected | Its GUID clustered key, `nvarchar(max)` columns and JSON searching won't suit tens of thousands of rows a day |
| **New table plus an extended `Log`** | **Recommended** | Compact, indexed activity rows; low-volume exceptions stay in `Log`, and existing reference IDs and the audit screen keep working |
| Serilog, Seq or Application Insights | Not now | Adds cost and a new tool, and admins need a screen tied to users and sites. Can be added later as an extra output |

No new NuGet package is needed.

### Database changes

These follow the existing convention of a re-runnable script. New file: `API/Vsky.Database/schema_activity_tracking.sql`.

**The script must run before the API deploys,** because the API will read these columns from the first request.

**New nullable columns on `Log`:** `RequestId`, `SiteId`, `ControllerName`, `ActionName`, `HttpMethod`, `ExceptionType`, `ClientPage`, `Source` (Server or Browser). New indexes: `RequestId`, and `SiteId, CreatedOnUtc`. Existing rows are unchanged.

**New table `UserActivityLog`:**

| Column | Type | Notes |
|---|---|---|
| `Id` | `bigint identity`, clustered PK | Roughly time-ordered, cheap to insert |
| `OccurredOnUtc` | `datetime2(3)` | |
| `EventType` | `tinyint` | Api, PageView, ClientError, UserAction |
| `RequestId` | `varchar(36)` | Links to `Log.RequestId` |
| `SessionId` | `varchar(36)` | Browser tab session; client-supplied, so for grouping only |
| `ActorType` | `tinyint` | User, Anonymous, Service (cron and webhooks) |
| `UserId`, `UserName` | `nvarchar(450)`, `nvarchar(256)` | **No foreign key,** so history survives user deletion and inserts stay cheap |
| `SiteId` | `nvarchar(450)` | From `X-Site-Id` |
| `IpAddress`, `UserAgent` | `varchar(45)`, `nvarchar(512)` | |
| `ClientPage`, `Module` | `nvarchar(512)`, `nvarchar(200)` | Screen route and menu or module |
| `HttpMethod`, `Path` | `varchar(10)`, `nvarchar(512)` | **Path only, no query string** |
| `ControllerName`, `ActionName` | `nvarchar(128)` each | |
| `RecordType`, `RecordId` | `nvarchar(128)`, `nvarchar(64)` | Primary record |
| `RelatedRecordIds` | `nvarchar(512)` | Capped list for bulk actions |
| `StatusCode`, `DurationMs` | `smallint`, `int` | |
| `ExceptionLogId` | `nvarchar(450)`, nullable | The reference ID in `Log` |
| `Details` | `nvarchar(2000)`, nullable | Short and sanitized |

**Indexes:** `(UserId, OccurredOnUtc DESC)`, `(SiteId, OccurredOnUtc DESC)`, `(RecordType, RecordId, OccurredOnUtc DESC)`, `(RequestId)`, and a filtered index on `ExceptionLogId`.

**Rollback:** drop the table, the indexes and the new columns, after first reverting the API.

### What is never stored

This extends the rule already stated in `SecurityLog.cs`.

| Excluded | Why |
|---|---|
| Request and response bodies | Passwords and personal, HR and finance data |
| Query strings, unless a parameter is on an allow-list | Real examples carry secrets: `token` on `auth/user-by-token`, `serviceKey`, and `sig` on signed uploads |
| `Authorization` and cookie headers | Credentials |
| Record IDs not on the allow-list | Only simple values, named `id` or ending in `Id`, of 64 characters or fewer, are kept |

Client-supplied values (`X-Client-Page`, `X-Site-Id`, user agent, browser error text) are cut to length, stripped of control characters and displayed only as plain text, never through `v-html`.

---

## 5. Implementation phases

### Phase 0: Prerequisites (about 1 day)

1. **Commit or branch the current work first.** The security work is uncommitted on `main`, and this plan changes several of the same files (`Program.cs`, `SecurityLog.cs`, `ActivityLogFilter.cs`).
2. Get the decisions in section 7.
3. Confirm the network path to the API (risk R1).
4. Size the volume from IIS logs: requests per day, and which endpoints poll.

### Phase 1: Complete exception capture (2 to 3 days)

The lowest-risk phase, and useful on its own.

- **Request ID:** add `RequestTrackingMiddleware`, assigning one ID per request and returning it as `X-Request-Id`. Set the header through `Response.OnStarting`, because `UseExceptionHandler` clears response headers.
- **Action details:** add a small step after `UseRouting` that stores the matched controller and action, because the exception handler clears the endpoint.
- **`SecurityLog`:** add the new columns from the request. Create the ID before writing, so **a reference ID still exists if the database is down**.
- **File fallback:** on a database failure, write to a rolling file in `App_Data/logs`.
- **Cancelled requests:** when the client aborted the request, don't record it as an error.
- **`DefaultLogger`:** route its writes through `ISecurityLog`, which removes the risk in F8 without changing its 9 call sites.
- **Swallowed exceptions:** add logging to the catches in F7, keeping their current behavior, and review about 20 controller catches that don't use `SafeErrorMessage`.

### Phase 2: Server activity tracking (4 to 5 days)

- **Middleware:** complete `RequestTrackingMiddleware`. It sits outside the exception handler, so it sees every outcome, including 401, 403, 404, 429 and 500.
- **`ActivityLogFilter`:** change it to collect record IDs from route values, action arguments and body `Id` properties, plus the ID of a newly created record where the result exposes one. The middleware writes the row, so the filter stops writing to `Log`.
- **Link to exceptions:** `SecurityLog` stores the reference ID on the request, so the activity row can point to it. This resolves F11 without changing any status code.
- **Background writer:** a bounded `Channel` feeds a hosted service that inserts in batches every 2 seconds or 200 rows, whichever comes first. It drains on shutdown, falls back to the file on failure, and logs a count if it ever has to drop rows.
- **Retention:** a daily hosted service deletes expired rows in small batches.
- **Audit screen:** point its "User activity" category at the new table, while old `Log` rows age out.
- **Configuration**, in `appsettings.json`:

```json
"ActivityTracking": {
  "Enabled": true,
  "LogReads": true,
  "ExcludedPaths": [ "/auth/refresh", "/swagger", "/profiler" ],
  "QueryStringAllowList": [],
  "QueueCapacity": 10000,
  "RetentionDays": 90,
  "ErrorRetentionDays": 365
}
```

The final `ExcludedPaths` list comes from the volume check in Phase 0. `OPTIONS` requests and static files are skipped unless they fail.

### Phase 3: Web app context (2 to 3 days)

- **`boot/axios.js`:** send `X-Client-Session` (a random ID per tab, in `sessionStorage`) and `X-Client-Page` (the route path without its query string). `http2` gets them too, so anonymous pages are covered.
- **`boot/error.js`:** turn on `app.config.errorHandler`, `window.onerror` and `unhandledrejection`.
- **Page views:** add `router.afterEach` in `router/index.js`, using `route.meta.title` for the page name.
- **New `boot/activityLog.js`:** queue events, then send them every 10 seconds, when the queue reaches 20 events and when the page is hidden.
  - It uses `fetch` with `keepalive`, **not** the shared axios instance, so a logging failure can't show an error toast or trigger logout or redirect handling, and can't log itself in a loop.
  - It removes duplicate errors, caps the queue, and never retries more than once.
- **New `POST /client-logs`:** logged-in users only, with limits on rate, batch size and field length. The server sets the user, IP and time itself and ignores any values the client sends for them.
- **Important user actions:** a small optional `logAction()` for things the API can't see. No global click tracking, because it creates noise and can capture personal data.
- **CORS:** add `WithExposedHeaders("X-Request-Id")` in `ServiceExtensions.cs`.

### Phase 4: Investigation screens (4 to 5 days)

New pages under `/security-admin`, built to the `vsky-ui-standards` skill and using the existing `securityShell.vue`.

| Screen | Purpose |
|---|---|
| **Investigate** (main page) | Enter a reference ID, or a user and time range, or a record. Shows the merged timeline from section 1 |
| **Error Log** | Exceptions with filters for date, site, controller, type and user; grouped counts to spot repeat failures; full stack trace on expand |
| **Activity Log** | Searchable, paged list of activity, with Excel export (EPPlus is already included) |

**New API endpoints, on a new `ActivityLogController`:**

- `GET /activity-log`: paged and filtered activity
- `GET /activity-log/timeline`: merges `UserActivityLog` and `Log` for a user and time range
- `GET /activity-log/by-reference/{id}`: finds the exception, then opens the timeline around it
- `GET /activity-log/errors` and `GET /activity-log/errors/{id}`

**Rules:**

- Every endpoint gets an admin endpoint tier, so `EndpointClassificationScan` passes.
- All queries are limited to a date range (7 days by default) and paged.
- Site admins see only their own site; other access follows decision D3.
- Viewing and exporting logs is itself recorded. There are no endpoints to edit or delete log rows.
- Times are stored in UTC and shown in the site's time zone.

### Phase 5: Testing and rollout (2 to 3 days)

The repo has no automated test project. I recommend adding a small one for the rules that are costly to get wrong: redaction, record ID extraction and the queue's behavior when full. Everything else is tested with the scenarios below.

| # | Scenario | Expected result |
|---|---|---|
| 1 | Force an exception in a controller catch block | Row with full context; the on-screen reference ID finds it |
| 2 | Force an uncaught exception | 500 response, with exception and activity rows sharing one `RequestId` |
| 3 | Expired token; denied action; invalid model; unknown URL; rate limit | Activity rows with 401, 403, 400, 404, 429 |
| 4 | Browse, open a record, edit it | Timeline correct, in order, with the record ID |
| 5 | Throw a browser error | Appears in the timeline with its page |
| 6 | Log in; reset a password; call `user-by-token` | **Search both tables and the fallback file: no passwords or tokens** |
| 7 | Stop SQL Server during a request | Response returns; reference ID exists; row is in the fallback file |
| 8 | Recycle the app pool during load | Queue drains; no errors |
| 9 | Site admin on site A looks for site B data | Nothing returned |
| 10 | Load test, tracking off vs on | Agreed limit: added latency under 5 ms at the 95th percentile |
| 11 | Check a production stack trace | Line numbers present, which requires `.pdb` files in the publish output |

**Rollout:**

1. Run the SQL script.
2. Deploy to Dev with `LogReads: false`, then check writes and failures.
3. Turn on `LogReads` and watch volume for 2 to 3 days.
4. Tune `ExcludedPaths`, then repeat on production.

**Rollback:** set `ActivityTracking:Enabled` to false, with no redeploy needed beyond the configuration change.

---

## 6. Risks

| # | Risk | Mitigation |
|---|---|---|
| R1 | **Wrong IP addresses.** `UseForwardedHeaders` (`Program.cs:717`) sets no `KnownProxies` or `KnownNetworks`, so only a proxy on the same machine is trusted. Behind any other proxy or load balancer, every row records the proxy's address. The rate limiter is affected the same way | Confirm the network path in Phase 0 and configure the real proxy addresses |
| R2 | **Database growth.** As an illustration only: 100 users at 300 requests a day is 30,000 rows a day, or very roughly 1 to 2 GB over 90 days with indexes | Measure in Phase 0; exclusions, retention job, compact columns |
| R3 | **Slower requests** | Background queue, no body reading, no extra query in the request path, load test |
| R4 | **Personal data.** A browsing history tied to named employees and IPs is sensitive | No bodies, restricted screens, retention limits, access logged, decision D6 |
| R5 | **Secrets in logs** | Allow-lists rather than block-lists, test scenario 6 |
| R6 | **Sensitive text in exception messages.** Database errors can quote values, such as a duplicate key | Stack traces restricted by D3; EF sensitive data logging stays off |
| R7 | **Activity lost on a hard crash,** up to about 2 seconds of queued rows | Accepted. Exceptions and security events are written immediately, so only routine activity is at risk |
| R8 | **Spoofed client values.** `X-Site-Id` isn't enforced while `SiteGateMode` is Audit, and page and session headers are always client-supplied | Treated as context only. User, IP and time come from the server. Audit evidence should rely on the user and record columns |
| R9 | **Fake browser logs** | Logged-in users only; rate and size limits; `Source` marks them as browser-reported |

**Out of scope,** noticed during analysis:

- `auth/user-by-token` takes a token in the URL, so it can end up in IIS logs.
- The API targets .NET 7, which is out of support. This plan doesn't depend on upgrading.

---

## 7. Decisions needed from you

| # | Decision | Recommendation | If you choose otherwise |
|---|---|---|---|
| D1 | Track reads as well as writes? | **Yes, all reads,** except polling endpoints | Writes only is far smaller, but loses most of the "what happened before" picture, which is the main objective |
| D2 | How long to keep logs? | **Activity 90 days; exceptions and security events 1 year.** Check against any customer contract or audit requirement | A longer period needs more storage, and possibly archiving |
| D3 | Who can see what? | **Site admins:** activity for their own site, and exception summaries. **System Super Admins:** all sites and full stack traces | Giving site admins stack traces exposes internal details and possibly other data |
| D4 | Record before and after values for field changes? | **Not in this release.** Record who changed which record and when. Existing logs such as `RequirementChangeLog` and `IssueStatusChangedLog` still cover their areas | Adding it for selected entities later takes about 1 week and needs its own privacy review |
| D5 | How does traffic reach the API? | *Needs your answer:* direct to IIS, or through a load balancer, CDN or reverse proxy? If so, what are its addresses? | Determines whether IP addresses can be trusted |
| D6 | Do users need to be told? | **Your or the customer's legal or HR contact should confirm** whether the employee or privacy notice needs updating | This is not a technical decision, and should be settled before production rollout |

---

## 8. Files affected

**New (API):** `Authorization/RequestTrackingMiddleware.cs`, `ActivityQueue.cs`, `ActivityLogWriter.cs` and `ActivityRetentionService.cs` (hosted services), `Controllers/ActivityLogController.cs`, `Vsky.Models/UserActivityLog.cs`, `Vsky.Services/ActivityLogs/` (query service and DTOs), `Vsky.Database/schema_activity_tracking.sql`

**Changed (API):**

- `Program.cs`: registrations and pipeline order
- `SecurityLog.cs`, `ISecurityLog.cs`: enrichment and file fallback
- `ActivityLogFilter.cs`: narrowed to record ID capture
- `DefaultLogger.cs`: writes through `ISecurityLog`
- `Vsky.Models/Log.cs`, `Vsky.Data/ApplicationDbContext.cs`: new columns and table
- `ServiceExtensions.cs`: exposed header
- `SecurityAdminService.cs`: "User activity" category
- `WorkflowMessageService.cs`, `NotificationService.cs`, `EmailSender.cs`, `EmbedService.cs`: logging in catches

**New (web):** `boot/activityLog.js`, and in `modules/security-admin/`: `pages/investigate.vue`, `pages/errorLog.vue`, `pages/activityLog.vue`, plus components

**Changed (web):** `boot/axios.js`, `boot/error.js`, `router/index.js`, `quasar.config.js` (boot list), `modules/security-admin/routes.js`, `securityAdmin.service.js`, `securityShell.vue`

**Not changed:** existing controllers' catch blocks, status codes returned to the web app, and existing feature logs such as `IssueStatusChangedLog`.

---

## 9. Approval

- [ ] Decisions D1 to D6 answered
- [ ] Design approved (section 4)
- [ ] Phases approved, or a subset chosen. Phases 1 and 2 give the core server history; phases 3 and 4 add page context and the screens
