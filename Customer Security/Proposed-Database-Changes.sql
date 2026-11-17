/*
  D1 and D2 were approved on 2026-11-04. The script to run is now
  API/Vsky.Database/schema_security_permissions.sql, which is safe to repeat. They are applied only to
  the local Meldep_Live_v4-0 database so far. The D1 and D2 section below is kept for reference.
  D3, D4 and D5 remain proposals and are NOT applied.

  ORIGINAL PROPOSAL:

  D1 and D2 add four new tables. No existing table, column, row or index is changed, and every new
  table is empty, so existing behavior stays the same until an admin creates a group or a grant.

  Why new tables are needed:
    SitesModulesMenusPermissions is keyed by SiteRoleId, with true/false flags. It has no user or
    group column, and "false" means "not granted", so it can't express a user grant, a group or a
    deny. Reusing it would change the meaning of existing rows. LeadUserGroupMapping and
    ReportRoleGroupMapping control which records a user sees, using DropDown values as groups, so
    they aren't suitable for security.

  ViewAccess and ManageAccess values:  0 = Inherit, 1 = Allow, 2 = Deny

  Precedence (already coded in PermissionEvaluator), first match wins, per flag and per menu:
    1. Failed site, module, menu or role checks: deny. Nothing overrides these.
    2. Conflicting duplicate rows for one role and menu: deny.
    3. User Deny.   4. User Allow.   5. Any group Deny.   6. Any role or group Allow.   7. Otherwise deny.

  Every row carries SiteId, so a grant in one site can never apply in another, including for shared
  users. Types match the existing keys: nvarchar(450) IDs and datetime2 dates.

  Run on a copy of the database first, after a backup. Rollback is at the end of this file.
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* ---------- D1: groups ---------- */

CREATE TABLE dbo.SecurityGroups (
    Id            nvarchar(450) NOT NULL CONSTRAINT PK_SecurityGroups PRIMARY KEY,
    SiteId        nvarchar(450) NOT NULL CONSTRAINT FK_SecurityGroups_Sites REFERENCES dbo.Sites (Id),
    Name          nvarchar(200) NOT NULL,
    Description   nvarchar(1000) NULL,
    Active        bit NOT NULL CONSTRAINT DF_SecurityGroups_Active DEFAULT (1),
    Deleted       bit NOT NULL CONSTRAINT DF_SecurityGroups_Deleted DEFAULT (0),
    CreatedById   nvarchar(450) NOT NULL,
    CreatedOnUtc  datetime2 NOT NULL,
    UpdatedById   nvarchar(450) NULL,
    UpdatedOnUtc  datetime2 NULL
);
CREATE UNIQUE INDEX UX_SecurityGroups_Site_Name ON dbo.SecurityGroups (SiteId, Name) WHERE Deleted = 0;

CREATE TABLE dbo.SecurityGroupMembers (
    Id               nvarchar(450) NOT NULL CONSTRAINT PK_SecurityGroupMembers PRIMARY KEY,
    SecurityGroupId  nvarchar(450) NOT NULL CONSTRAINT FK_SecurityGroupMembers_Groups REFERENCES dbo.SecurityGroups (Id),
    UserId           nvarchar(450) NOT NULL CONSTRAINT FK_SecurityGroupMembers_Users REFERENCES dbo.AspNetUsers (Id),
    SiteId           nvarchar(450) NOT NULL CONSTRAINT FK_SecurityGroupMembers_Sites REFERENCES dbo.Sites (Id),
    Deleted          bit NOT NULL CONSTRAINT DF_SecurityGroupMembers_Deleted DEFAULT (0),
    CreatedById      nvarchar(450) NOT NULL,
    CreatedOnUtc     datetime2 NOT NULL,
    UpdatedById      nvarchar(450) NULL,
    UpdatedOnUtc     datetime2 NULL
);
CREATE UNIQUE INDEX UX_SecurityGroupMembers_Group_User ON dbo.SecurityGroupMembers (SecurityGroupId, UserId) WHERE Deleted = 0;
CREATE INDEX IX_SecurityGroupMembers_User_Site ON dbo.SecurityGroupMembers (UserId, SiteId) WHERE Deleted = 0;

CREATE TABLE dbo.SecurityGroupMenuPermissions (
    Id                nvarchar(450) NOT NULL CONSTRAINT PK_SecurityGroupMenuPermissions PRIMARY KEY,
    SecurityGroupId   nvarchar(450) NOT NULL CONSTRAINT FK_SecurityGroupMenuPermissions_Groups REFERENCES dbo.SecurityGroups (Id),
    SiteId            nvarchar(450) NOT NULL CONSTRAINT FK_SecurityGroupMenuPermissions_Sites REFERENCES dbo.Sites (Id),
    SiteModuleMenuId  nvarchar(450) NOT NULL CONSTRAINT FK_SecurityGroupMenuPermissions_Menus REFERENCES dbo.SitesModulesMenus (Id),
    ShowInMenu        bit NOT NULL CONSTRAINT DF_SecurityGroupMenuPermissions_Show DEFAULT (0),
    ViewAccess        tinyint NOT NULL CONSTRAINT DF_SecurityGroupMenuPermissions_View DEFAULT (0)
                      CONSTRAINT CK_SecurityGroupMenuPermissions_View CHECK (ViewAccess IN (0, 1, 2)),
    ManageAccess      tinyint NOT NULL CONSTRAINT DF_SecurityGroupMenuPermissions_Manage DEFAULT (0)
                      CONSTRAINT CK_SecurityGroupMenuPermissions_Manage CHECK (ManageAccess IN (0, 1, 2)),
    Deleted           bit NOT NULL CONSTRAINT DF_SecurityGroupMenuPermissions_Deleted DEFAULT (0),
    CreatedById       nvarchar(450) NOT NULL,
    CreatedOnUtc      datetime2 NOT NULL,
    UpdatedById       nvarchar(450) NULL,
    UpdatedOnUtc      datetime2 NULL
);
CREATE UNIQUE INDEX UX_SecurityGroupMenuPermissions_Group_Menu ON dbo.SecurityGroupMenuPermissions (SecurityGroupId, SiteModuleMenuId) WHERE Deleted = 0;

/* ---------- D2: individual users ---------- */

CREATE TABLE dbo.UserMenuPermissions (
    Id                nvarchar(450) NOT NULL CONSTRAINT PK_UserMenuPermissions PRIMARY KEY,
    UserId            nvarchar(450) NOT NULL CONSTRAINT FK_UserMenuPermissions_Users REFERENCES dbo.AspNetUsers (Id),
    SiteId            nvarchar(450) NOT NULL CONSTRAINT FK_UserMenuPermissions_Sites REFERENCES dbo.Sites (Id),
    SiteModuleMenuId  nvarchar(450) NOT NULL CONSTRAINT FK_UserMenuPermissions_Menus REFERENCES dbo.SitesModulesMenus (Id),
    ShowInMenu        bit NOT NULL CONSTRAINT DF_UserMenuPermissions_Show DEFAULT (0),
    ViewAccess        tinyint NOT NULL CONSTRAINT DF_UserMenuPermissions_View DEFAULT (0)
                      CONSTRAINT CK_UserMenuPermissions_View CHECK (ViewAccess IN (0, 1, 2)),
    ManageAccess      tinyint NOT NULL CONSTRAINT DF_UserMenuPermissions_Manage DEFAULT (0)
                      CONSTRAINT CK_UserMenuPermissions_Manage CHECK (ManageAccess IN (0, 1, 2)),
    Reason            nvarchar(500) NULL,
    ExpiresOnUtc      datetime2 NULL,   -- optional: a temporary grant ends by itself
    Deleted           bit NOT NULL CONSTRAINT DF_UserMenuPermissions_Deleted DEFAULT (0),
    CreatedById       nvarchar(450) NOT NULL,
    CreatedOnUtc      datetime2 NOT NULL,
    UpdatedById       nvarchar(450) NULL,
    UpdatedOnUtc      datetime2 NULL
);
CREATE UNIQUE INDEX UX_UserMenuPermissions_User_Menu ON dbo.UserMenuPermissions (UserId, SiteModuleMenuId) WHERE Deleted = 0;
CREATE INDEX IX_UserMenuPermissions_User_Site ON dbo.UserMenuPermissions (UserId, SiteId) WHERE Deleted = 0;

COMMIT TRANSACTION;

/* ---------- Rollback for D1 and D2 (only if approved and applied) ----------
DROP TABLE dbo.UserMenuPermissions;
DROP TABLE dbo.SecurityGroupMenuPermissions;
DROP TABLE dbo.SecurityGroupMembers;
DROP TABLE dbo.SecurityGroups;
*/

/* ---------- D3: menu rows for pages with no menu (data only, review first) ----------
   These pages exist in the app but have no active ModulesMenus row, so View and Manage can't be
   configured for them and they can only require site membership:
     /time-in-time-out/list, /movement-register/list, /help-desk, /leave-yearly-schedule,
     /salesperson, /sites-items, /item, /infra-dashboard, /project-center, /requirement-center,
     /requirement-data-mapping, /requirement-status-tracking, /project-action-items,
     /project-planning, /weekly-timesheet, /weekly-timesheet-approval, /billing-processing

   Some active menus have no matching page and may be related: /break, /time. The business should
   confirm these. Adding a row grants nothing until roles are assigned, so a page must be given to
   its current users before its rule is switched from site membership to menu permission.
   No script is proposed until this list is confirmed.
*/

/* ---------- D4: plaintext credentials (data rewrite plus code, separate project) ----------
   Domain.DatabasePassword, Domain.FtpPassword, Domain.WebsiteLoginPassword, EmailAccount.Password,
   Infra_ProjectInstance_Role_Users.Password, ReportSettings.ApplicationSecret, Server.FtpPassword,
   Server.Password, TwilioEmailSettings.SendGridApiKey, TwilioEmailSettings.TwilioAuthToken

   These are not hashes. The app must read the values back, so they need encryption with a key held
   outside the database. Some must also be shown on screen, which needs a business decision.
*/

/* ---------- D5: users with no valid site (data only, review first) ----------
   Read-only query to list them for review. Run this before enforcing the site rule.

   SELECT u.Id, u.UserName, p.SiteId
   FROM AspNetUsers u
   JOIN PersonSitesMapping p ON p.PersonId = u.PersonId AND p.Deleted = 0
   WHERE u.Deleted = 0 AND u.Active = 1
     AND NOT EXISTS (
       SELECT 1 FROM AspNetUserRoles ur
       JOIN AspNetRoles r ON r.Id = ur.RoleId AND r.Deleted = 0
       JOIN SitesRoles sr ON sr.RoleId = ur.RoleId AND sr.SiteId = ur.SiteId AND sr.Deleted = 0
       WHERE ur.UserId = u.Id AND ur.SiteId = p.SiteId);
*/
