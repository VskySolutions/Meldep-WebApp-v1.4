/*
  D1 and D2: security groups and individual user permissions.

  Adds four new, empty tables. No existing table, column, row or index changes, so behavior stays
  the same until an admin creates a group or a grant. Safe to run more than once.

  Must run BEFORE deploying the API that uses these tables: the permission check reads them on
  every request, and an API with no tables denies access wherever enforcement is on.

  ViewAccess and ManageAccess: 0 = Inherit, 1 = Allow, 2 = Deny

  Rollback (only after removing the API that reads them):
    DROP TABLE dbo.UserMenuPermissions;
    DROP TABLE dbo.SecurityGroupMenuPermissions;
    DROP TABLE dbo.SecurityGroupMembers;
    DROP TABLE dbo.SecurityGroups;
*/

-- Filtered indexes need these, and sqlcmd doesn't turn QUOTED_IDENTIFIER on by default.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SecurityGroups', N'U') IS NULL
BEGIN
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
END;

IF OBJECT_ID(N'dbo.SecurityGroupMembers', N'U') IS NULL
BEGIN
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
END;

IF OBJECT_ID(N'dbo.SecurityGroupMenuPermissions', N'U') IS NULL
BEGIN
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
END;

IF OBJECT_ID(N'dbo.UserMenuPermissions', N'U') IS NULL
BEGIN
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
        ExpiresOnUtc      datetime2 NULL,
        Deleted           bit NOT NULL CONSTRAINT DF_UserMenuPermissions_Deleted DEFAULT (0),
        CreatedById       nvarchar(450) NOT NULL,
        CreatedOnUtc      datetime2 NOT NULL,
        UpdatedById       nvarchar(450) NULL,
        UpdatedOnUtc      datetime2 NULL
    );
    CREATE UNIQUE INDEX UX_UserMenuPermissions_User_Menu ON dbo.UserMenuPermissions (UserId, SiteModuleMenuId) WHERE Deleted = 0;
    CREATE INDEX IX_UserMenuPermissions_User_Site ON dbo.UserMenuPermissions (UserId, SiteId) WHERE Deleted = 0;
END;

COMMIT TRANSACTION;
