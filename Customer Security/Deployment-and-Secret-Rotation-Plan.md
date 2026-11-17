# Deployment and Secret Rotation Plan

This plan doesn't assume whether the application is in production. Treat every environment that holds real users as production. This document contains no secret values.

## 1. Before any deployment

1. Back up the database, the deployed API and Vue folders, and the server's `appsettings.json` and `web.config`.
2. Deploy to a test environment using a copy of the data first.
3. Check whether `API/Vsky.Api/web.config` on the server sets `ASPNETCORE_ENVIRONMENT=Development`. If it does, Swagger and detailed errors are publicly exposed today.
4. Keep Authorization at its defaults for the first deployment, so new rules only log.

5. **Run `API/Vsky.Database/schema_security_permissions.sql` before deploying this API.** The permission check reads the new tables on every request. Without them, signed-in users are denied wherever enforcement is on, and the Vue route guard fails closed in every mode.
6. `appsettings.Development.json` turns on full enforcement, and applies only when `ASPNETCORE_ENVIRONMENT=Development`. Don't copy its values to another environment until the "Before enforcing" list in `Security-Implementation-Report.md` is done.

## 2. Production-sensitive changes

These take effect as soon as the code is deployed.

| Change | Who notices | How to prepare | Rollback |
|---|---|---|---|
| Microsoft login checks the token properly | **All Microsoft-login users, if `MicrosoftLogin` isn't set** | Add `ClientId` and `AllowedTenantIds` first, and deploy API and Vue together | Redeploy the previous version |
| Password reset needs the emailed token | Anyone with an old link in their inbox | Tell users to request a new link. Links last 1 hour | Previous version (this reopens the takeover hole) |
| Forgot-password reply is the same for every email | Users with email notifications off get no message | Tell support | Previous version |
| Role, user, sharing, site and permission changes need an admin | Non-admins who used those screens | Run the query in Section 3 first | Previous version |
| Anonymous leave `GET` and `PUT` now need login | Any external system calling them anonymously (none found in the app) | Check server logs for these calls | Previous version (this reopens the hole) |
| Calendar feed needs login and an Outlook link | Any outside caller, or links on other hosts | Check server logs for `/calendar/list` | Previous version |
| `web.config` no longer sets Development | Swagger disappears, and errors show a reference ID | Confirm nobody relies on Swagger in that environment | Restore the line |
| Rate limit: 60 requests per minute per address on login and public forms | Many users behind one address, or scripts | Check that forwarded client addresses reach the API | Remove `UseRateLimiter` |
| Blocked upload types such as `.exe`, `.html`, `.js` | Anyone who stored those | Confirm with the business | Remove the list |
| New passwords need at least 8 characters | Admins typing short passwords. **Existing passwords keep working** | Tell admins | Restore the setting |
| Tokens issued after this release end when the password changes | Users who change a password on another device | None | Remove the check |
| Exception details are hidden | Support staff who read messages on screen | Use the reference ID to find the row in `Log` | Previous version |
| Activity rows added to `Log` for each write | Database size grows | Watch growth, and plan retention | `Authorization:ActivityLogging=false` |
| Leave approve, decline and forward need approval rights or being the assigned approver | Logged-in users who had neither | Admins and approval-menu holders are unaffected | Previous version |
| Anonymous `project-activities/{id}/timer`, `modules/moduleslist` and `drop-downs-type/list` now need login | Any external caller (the Vue app sends a login token) | Check server logs for anonymous calls | Previous version |
| SOP assistant goes through the API | Users of that page. The URL can be moved to `SopAssistant:WebhookUrl` | Test the assistant after deploying | Previous version |
| Buttons on 78 pages use `v-can` | Nobody until enforcement: then users without Manage don't see Add, Edit or Delete | Review during enforcement | Remove `v-can` |
| Database changes | None are included. Log rows are the only new writes | | |

## 3. Config rollout order

Each step can be undone by changing the value back.

```json
"MicrosoftLogin": { "ClientId": "<from WEB/config env files>", "AllowedTenantIds": [ "<your tenant IDs>" ] },
"Authorization": {
  "Mode": "Audit", "SiteGateMode": "Audit", "RecordScopeMode": "Audit",
  "EnforcedLinks": [], "EnforceAllLinks": false,
  "LegacyShowOnlyAccess": true, "EnforceSiteLevelActiveFlags": false,
  "ActivityLogging": true, "ServiceKey": "<new random value>"
},
"BlobAccess": { "Mode": "Public", "SignatureMinutes": 60 },
"Cors": { "AllowedOrigins": [ "<your Vue app origin>" ] }
```

1. **Deploy with the above.** Review would-be denials for at least one business cycle, including month-end and approvals:
   ```sql
   SELECT CreatedOnUtc, UserId, PageUrl, FullMessage FROM [Log]
   WHERE ShortMessage = 'Authorization audit: would deny' ORDER BY CreatedOnUtc DESC;
   ```
2. **Site rule.** Fix the users from D5, confirm the mobile app and other clients send `X-Site-Id`, then set `SiteGateMode` to `Enforce`.
3. **Set the View and Manage flags.** Today no row has either set. Have admins configure sensitive modules first, and keep Manage on self-service menus such as leave and timesheets.
4. **Enforce by menu.** `Mode: Enforce`, adding to `EnforcedLinks` one group at a time: admin, then finance and HR, then CRM, with projects, tasks and timesheets last.
5. **Record rules.** Set `RecordScopeMode` to `Enforce` after checking links from notifications and email.
6. **Scheduler and webhooks.** Set `ServiceKey`. The cron scheduler sends it as `X-Service-Key`, and inbound email webhooks may add `?serviceKey=` to their URL. Do this before step 7.
7. **Finish.** `EnforceAllLinks: true`, then `LegacyShowOnlyAccess: false` only when no important row is still `1/0/0`. Turning that off removes access from those rows.
8. **Shorter sessions.** Set `JwtTokenConfig:ValidFor`, for example `"12:00:00"`. Vue renews active sessions automatically, so only idle users log in again. Do this after rotating the JWT key, not during.

## 4. Secure file access (Azure Blob)

Today's containers allow anonymous reads, so anyone with a file URL can open it.

1. Set `BlobAccess:Mode` to `Signed`. File URLs in API responses gain a short-lived signature, and all screens keep working. Stored URLs don't change.
2. Test screens, downloads and the Google file viewer.
3. **Check content that lives outside the API before going further:** images and links in emails already sent, site logos on the login page, and any public-website images. These use unsigned URLs and will stop loading once a container is private.
4. In Azure, turn off anonymous access on one low-risk container, and test. Then do the rest, leaving any container that must stay public (for logos).
5. **Rollback:** turn anonymous access back on in Azure, and set `Mode` to `Public`.

New containers stay public while `Mode` is `Public`, and are created private in `Signed` mode.

## 5. Secret inventory and rotation

No value is listed here. "Exposure" is what was found in this work.

| Secret | Where it is | Exposure | Impact of rotating | Risk |
|---|---|---|---|---|
| JWT signing key | `appsettings.json`: `JwtTokenConfig.SecurityKey` | Real-looking value; forged-login holes existed | **Everyone is logged out once.** Nothing else | High |
| Database password | Commented-out connection string in `appsettings.json`, line 4 | **Printed into an AI session transcript during this work.** Real-looking host and password | Update the connection string, restart | High |
| Azure Storage key | `appsettings.json`: `ConnectionStrings.AzureBlob` | Real-looking key | Uploads fail until updated. Rotate the unused key slot first | High |
| Power BI app secret | `appsettings.json`: `AppSettings.applicationSecret`; also `ReportSettings.ApplicationSecret` | Real-looking value, and stored in a database column | Reports fail until both are updated | High |
| AI assistant key | Was in tracked `env.prod.cjs`, `env.test.cjs` and a comment in `layout.vue`. **Removed from files, but still in git history** | Exposed to anyone with repo access, and possibly built into older bundles | None found: no active code uses it | High |
| Default encryption key | `EncryptionService.cs:92`, tracked in source | **Printed into an AI session transcript during this work** | No code calls `EncryptText` or `DecryptText` today, so low. Confirm no stored data depends on it | Medium |
| Email account passwords | `EmailAccount.Password` | Plaintext in the database | Update the row, and the mail provider | Medium |
| SendGrid and Twilio keys | `TwilioEmailSettings` | Plaintext in the database | Email and help desk stop until updated | Medium |
| Server, FTP and database credentials | `Server`, `Domain`, `Infra_ProjectInstance_Role_Users` | Plaintext in the database, and shown on screen | Rotate on each system, then update the row | Medium |
| Data-protection keys | `API/Vsky.Api/App_Data/` (now git-ignored) | Never committed | Rotating ends outstanding reset links | Low |
| n8n webhook URL | `_sopChatAssistant.vue`, in the browser until this work | Acts as a password to the workflow. Still in git history | Create a new webhook, then set `SopAssistant:WebhookUrl` | Medium |

**Copies to clean up.** `appsettings.json` is git-ignored, but copies are in `bin/`, `obj/` and `publish/` folders on developer machines and build servers.

### Rotation order

Do this in a planned window, with the API and Vue already deployed.

1. **Remove line 4 of `appsettings.json`** (the commented-out connection string).
2. **Database password:** create a new login or password, update the server's config, restart, verify, then disable the old one.
3. **Azure Storage:** switch the app to the secondary key, verify uploads, regenerate the primary key, switch back, then regenerate the secondary.
4. **Power BI secret:** create a new client secret in Azure AD, update both locations, verify a report, delete the old secret.
5. **AI key and n8n webhook:** revoke the old ones at the provider. Create new ones only if still needed.
6. **SendGrid, Twilio and email passwords:** create new ones, update the database rows, send a test email, revoke the old ones.
7. **JWT key last, and out of hours:** generate at least 64 random bytes, update every API server at once, restart. Everyone logs in again, and any forged token stops working. Announce it first.
8. **Review logs** for `mslogin` and `reset-password` activity from before the fix. If abuse can't be ruled out, force password resets. `/auth/user` exposed password hashes anonymously, so plan this for all accounts if the old code was reachable from the internet.

### Keeping secrets safe afterwards

- Keep secrets in Azure Key Vault or server environment variables, not files. ASP.NET Core reads `JwtTokenConfig__SecurityKey`-style variables without any code change.
- Consider purging history for the AI key with `git filter-repo`. This rewrites history for everyone, so plan it separately. Rotation is what actually protects you.
- Never paste real config files into tickets, chats or AI tools.
