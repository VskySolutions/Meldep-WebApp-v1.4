# Meldep Security Implementation Report

**Updated:** 2026-11-04 (groups, individual permissions and Security Administration)
**Companion documents:** `Industrial-Standard-Application-Security-Implementation-Plan.md`, `Proposed-Database-Changes.sql`, `Deployment-and-Secret-Rotation-Plan.md`

## Summary

Group permissions, individual Allow and Deny, and the Security Administration screens are built and tested with real logins against a copy of the database. Enforcement is on in Development only. Nothing is committed or deployed.

**Not ready for production enforcement:** Show-only role rows still mean full access. See "Before enforcing".

## Requirement status

| Requirement | Status | Default outside Development |
|---|---|---|
| Role permissions | Built, tested | Logs only |
| Group permissions (D1) | Built, tested | Logs only |
| Individual Allow and Deny (D2) | Built, tested, with reason and optional end date | Logs only |
| Precedence: user Deny, user Allow, group Deny, then any Allow | Tested through the API | Logs only |
| Security Administration screens | Built, tested in a browser | Admin checks always enforced |
| Central authorization: all 858 actions have a rule | Confirmed by the startup scan | Logs only, plus admin checks |
| Direct URL and API protection | Tested | No login: blocked. No permission: logged |
| Multi-site isolation | Tested | Logs only |
| Record-level rules | Tested on Issue and Leave only | Logs only |
| Email leave approval | **Fixed on 2026-11-04.** The earlier check let any employee decide | **Enforced** |
| Audit log | Tested: rows are saved and shown per site | **Active** |

## Security Administration URLs

Open from **Settings > Security Administration**, or directly:

| Screen | Path |
|---|---|
| Overview | `/security-admin` |
| Users | `/security-admin/users` |
| Roles | `/security-admin/roles` |
| Groups | `/security-admin/groups` |
| Permissions | `/security-admin/permissions` |
| Site Access | `/security-admin/site-access` |
| Effective Permissions | `/security-admin/effective` |
| Audit Log | `/security-admin/audit` |

Admins can't change their own access, act on a higher-ranked user, or grant what they don't hold. System Super Admin is exempt only from the last rule.

## Verification

Run on `Meldep_Security_Test`, a restored copy with outbound email disabled. Scripts are in `Customer Security/tools`.

| Suite | Result |
|---|---|
| API checks, Audit mode | 107 passed, 0 failed |
| API checks, Enforce mode from `appsettings.Development.json` | 117 passed, 0 failed |
| Workflows: leave, session renewal, revocation, site switch, department create and delete | 30 passed, 0 failed |
| Browser: direct URLs, 404 page, all 8 screens, group and Deny end to end | 63 passed, 1 failed (issue 5 below) |
| Sidebar crawl with Enforce on: 6 real roles, 201 page opens | 0 wrongly denied, 0 failed API calls |

**Not tested**
- Email delivery. Email was disabled, so a notification step failed after the save when forwarding a leave.
- Create and edit flows beyond those listed. The crawl opens pages and doesn't click buttons or save.
- Roles with no suitable real user in the crawl (`manager`, `finance`, `viewer`), because each candidate held several roles.
- A real user's login on `Meldep_Live_v4-0`. Only startup and anonymous requests were checked there.
- Microsoft login with a real token, and the password reset email.

## Before enforcing outside Development

1. **Configure Show-only role rows.** A `1/0/0` row means full Manage. In Vsky Solutions the `employee` role, with 156 holders, has such rows on `/approve-expense`, `/approve-advance-expense`, `/finance-expense`, `/employee` and more, so employees pass the API check for those. Only leave decisions were tightened. Who should hold Manage is a business decision.
2. **Resolve 9 users with no valid site** (7 have no role at all). They can't log in under Enforce. Assign roles on the Site Access screen, or remove them. No roles were assigned automatically, as that would grant access nobody decided on.
3. **Set `Authorization:ServiceKey`.** Without it, cron jobs and inbound webhooks are refused under full enforcement.

## Remaining issues

1. **Accounts sharing an email fail on every signed-in request.** `FindByEmailAsync` in `Program.cs` throws for 6 accounts sharing 3 emails, 4 of them active. This predates the security work. Two of the six also fail at login itself. It needs a data decision on which account stays.
2. **D3:** 17 pages have no menu row and only require site membership, so their 77 write actions can't use View or Manage. Rows were not added, because a checkbox that enforces nothing would mislead admins.
3. **10 sidebar links have no page:** `/module`, `/server`, `/domain`, `/ftp-list`, `/menu`, `/advance-expense-request`, `/master_notifications`, `/master-project-template`, `/health`, `/career`. These predate this work.
4. `/all-project-planner` has no role grant in any site, so under Enforce nobody can use it until an admin grants it.
5. **Dashboard widgets don't handle a 403.** A user with no module permissions sees the dashboard, but widgets raise uncaught errors in the browser console. Real roles in the crawl weren't affected.
6. `Modules.Active` is 0 for all modules, so `EnforceSiteLevelActiveFlags` must stay off.
7. A group never replaces the site check, so a member with no role in the site gets nothing. The Groups and Effective Permissions screens show this.
8. With several API servers, a removed permission can apply for up to 60 seconds on the other servers.
9. Activity rows grow the `Log` table, with no retention job. The audit screen searches JSON text, so it will slow as the table grows.

## Out of scope, as requested

2FA, Azure Blob security and malware scanning were not changed. Also still open from earlier work: encrypting stored credentials (D4), enforcing CSP, the .NET 7 upgrade, stored procedure review and secret rotation.

## Choices made to preserve functionality

| Module | Decision | Why |
|---|---|---|
| Timesheets, daily planner, activities, expenses, reports | By-ID check is site only | Their lists have no effective per-user rule, and approvers rely on seeing others' records |
| Leave by-ID and cancel | Owner, approver, admin or HR | **Inferred: please confirm** |
| Leave approve or decline | Assigned approver, admin, or deliberate Manage on `/approve-leaves` | Matches who the approval list shows |
| Leave forward | HR, the leave's owner, assigned approver, admin, or deliberate Manage on `/forward-leaves` | Matches who the forward list shows. **Inferred: please confirm** |
| Sidebar | Unchanged unless a user has a group or individual rule | The original role query still runs for everyone else |

## Known limitations

- Vue checks are for usability only. The API is the security boundary.
- Tokens issued before this release have no session stamp, so they keep working after a password change until they expire or the JWT key is rotated.
- Active users can renew sessions indefinitely, so only idle users are logged out by a shorter lifetime.
- `MenuLinks.cs` must match `ModulesMenus.Link`. Editing a link in the admin screen stops that rule matching.
