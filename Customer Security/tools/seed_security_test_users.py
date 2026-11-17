"""Creates test accounts in a throwaway copy of the database. It refuses to run on any other database.

Usage: python seed_security_test_users.py <output-json-path>

Test users are cloned from an existing user's Person and AspNetUsers rows. A few real users, one per
common role, also get the test password, so sidebar pages can be crawled with real permissions. All
of this happens only in the copy. The password is random and written only to the output file.
"""
import base64
import hashlib
import json
import os
import secrets
import struct
import subprocess
import sys
import tempfile
import uuid

SERVER = "DESKTOP-M3PJFV7"
DATABASE = "Meldep_Security_Test"
SITE_A = "04086f10-8f09-4e0c-b5c0-c827a244addd"  # Vsky Solutions
SITE_B = "c8c5a74e-c04d-417b-a15b-c422d74731dd"  # Jump Suit Group

# name: list of (site, role). "sec-empty" is created here and has no permission rows at all.
USERS = {
    "sec.empty": [(SITE_A, "sec-empty")],
    "sec.empty2": [(SITE_A, "sec-empty")],
    "sec.employee": [(SITE_A, "employee")],
    "sec.hr": [(SITE_A, "hr")],
    "sec.admin": [(SITE_A, "admin")],
    "sec.admin2": [(SITE_A, "admin")],
    "sec.superadmin": [(SITE_A, "site-super-admin")],
    "sec.adminb": [(SITE_B, "admin")],
    "sec.shared": [(SITE_A, "sec-empty"), (SITE_B, "sec-empty")],
    "sec.norole": [],
}

REAL_USER_ROLES = ["employee", "hr", "manager", "finance", "client", "site-super-admin", "admin", "viewer", "system-super-admin"]


def sql(text, capture=True):
    with tempfile.NamedTemporaryFile("w", suffix=".sql", delete=False, encoding="utf-8") as file:
        file.write("SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;\n" + text)
    try:
        result = subprocess.run(
            ["sqlcmd", "-S", SERVER, "-E", "-C", "-b", "-W", "-h-1", "-s", "|", "-d", DATABASE, "-i", file.name],
            capture_output=capture, text=True, check=False)
    finally:
        os.unlink(file.name)
    if result.returncode != 0:
        raise SystemExit(f"SQL failed:\n{result.stdout}\n{result.stderr}\n{text[:800]}")
    return [line for line in result.stdout.splitlines() if line.strip()]


def identity_hash(password):
    # The .NET 7 default format, so login doesn't try to upgrade the hash and rewrite the user.
    salt = secrets.token_bytes(16)
    iterations = 100000
    key = hashlib.pbkdf2_hmac("sha512", password.encode("utf-8"), salt, iterations, 32)
    header = b"" + struct.pack(">III", 2, iterations, len(salt))
    return base64.b64encode(header + salt + key).decode()


def main():
    if sql("SELECT DB_NAME();")[0].strip() != DATABASE:
        raise SystemExit("Not the test copy. Stopping.")

    password = "Sec!" + secrets.token_urlsafe(18)
    password_hash = identity_hash(password)
    now = "SYSUTCDATETIME()"

    source = sql(
        f"SELECT TOP 1 u.Id, u.PersonId FROM AspNetUsers u JOIN PersonSitesMapping p ON p.PersonId = u.PersonId "
        f"WHERE u.Active = 1 AND u.Deleted = 0 AND p.SiteId = '{SITE_A}' AND p.Deleted = 0 AND u.PasswordHash IS NOT NULL ORDER BY u.Id;")[0]
    source_user, source_person = source.split("|")

    sql(f"""
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'sec-empty')
BEGIN
  SELECT TOP 1 * INTO #r FROM AspNetRoles WHERE NormalizedName = 'employee' AND Deleted = 0;
  UPDATE #r SET Id = 'sec-empty-role', Name = 'Sec Empty', NormalizedName = 'sec-empty', ConcurrencyStamp = NEWID();
  INSERT AspNetRoles SELECT * FROM #r;
END;
""")
    for site in (SITE_A, SITE_B):
        sql(f"""
IF NOT EXISTS (SELECT 1 FROM SitesRoles WHERE SiteId = '{site}' AND RoleId = 'sec-empty-role')
BEGIN
  SELECT TOP 1 * INTO #sr FROM SitesRoles WHERE SiteId = '{site}' AND Deleted = 0;
  UPDATE #sr SET Id = NEWID(), RoleId = 'sec-empty-role';
  INSERT SitesRoles SELECT * FROM #sr;
END;""")

    users = {}
    for name, assignments in USERS.items():
        user_id = str(uuid.uuid5(uuid.NAMESPACE_DNS, name + ".user"))
        person_id = str(uuid.uuid5(uuid.NAMESPACE_DNS, name + ".person"))
        email = f"{name}@invalid.localhost"
        sql(f"""
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = '{user_id}')
BEGIN
  SELECT * INTO #p FROM Person WHERE Id = '{source_person}';
  UPDATE #p SET Id = '{person_id}', FirstName = '{name}', LastName = 'Test', PrimaryEmailAddress = '{email}';
  INSERT Person SELECT * FROM #p;
  SELECT * INTO #u FROM AspNetUsers WHERE Id = '{source_user}';
  UPDATE #u SET Id = '{user_id}', PersonId = '{person_id}', UserName = '{email}', NormalizedUserName = UPPER('{email}'),
    Email = '{email}', NormalizedEmail = UPPER('{email}'), EmailConfirmed = 1, PasswordHash = '{password_hash}',
    SecurityStamp = REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), ConcurrencyStamp = NEWID(),
    LockoutEnd = NULL, AccessFailedCount = 0, TwoFactorEnabled = 0, Active = 1, Deleted = 0;
  INSERT AspNetUsers SELECT * FROM #u;
END
ELSE
  UPDATE AspNetUsers SET PasswordHash = '{password_hash}', LockoutEnd = NULL, AccessFailedCount = 0 WHERE Id = '{user_id}';
""")
        sites = {site for site, _ in assignments} or {SITE_A}
        for index, site in enumerate(sorted(sites, key=lambda s: s != SITE_A)):
            shared = 1 if index > 0 else 0
            mapping = str(uuid.uuid5(uuid.NAMESPACE_DNS, f"{name}.{site}.map"))
            sql(f"""
IF NOT EXISTS (SELECT 1 FROM PersonSitesMapping WHERE Id = '{mapping}')
  INSERT PersonSitesMapping (Id, PersonId, SiteId, CreatedOnUtc, CreatedById, IsSharedUser, Deleted, LastUsed)
  VALUES ('{mapping}', '{person_id}', '{site}', {now}, '{user_id}', {shared}, 0, {1 if index == 0 else 0});
""")
        for site, role in assignments:
            sql(f"""
DECLARE @r nvarchar(450) = (SELECT TOP 1 r.Id FROM AspNetRoles r JOIN SitesRoles s ON s.RoleId = r.Id WHERE r.NormalizedName = '{role}' AND r.Deleted = 0 AND s.SiteId = '{site}' AND s.Deleted = 0);
IF @r IS NULL THROW 50000, 'Role {role} is missing in {site}', 1;
IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = '{user_id}' AND RoleId = @r AND SiteId = '{site}')
  INSERT AspNetUserRoles (UserId, RoleId, SiteId) VALUES ('{user_id}', @r, '{site}');
""")
        users[name] = {"id": user_id, "userName": email, "personId": person_id}

    # Real users, one per role, for crawling pages with the permissions real people have.
    real = {}
    for role in REAL_USER_ROLES:
        rows = sql(f"""
SELECT TOP 1 u.Id, u.UserName FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id AND ur.SiteId = '{SITE_A}'
JOIN AspNetRoles r ON r.Id = ur.RoleId AND r.NormalizedName = '{role}'
JOIN PersonSitesMapping p ON p.PersonId = u.PersonId AND p.SiteId = '{SITE_A}' AND p.Deleted = 0
WHERE u.Active = 1 AND u.Deleted = 0
  AND (SELECT COUNT(*) FROM AspNetUserRoles x WHERE x.UserId = u.Id AND x.SiteId = '{SITE_A}') = 1
ORDER BY u.Id;""")
        if rows:
            user_id, user_name = rows[0].split("|")
            sql(f"UPDATE AspNetUsers SET PasswordHash = '{password_hash}', LockoutEnd = NULL, AccessFailedCount = 0 WHERE Id = '{user_id}';")
            real[role] = {"id": user_id, "userName": user_name}

    with open(sys.argv[1], "w", encoding="utf-8") as file:
        json.dump({"password": password, "siteA": SITE_A, "siteB": SITE_B, "users": users, "realUsers": real}, file, indent=1)
    print(f"Seeded {len(users)} test users and {len(real)} real-role users in {DATABASE}: {sorted(real)}")


main()
