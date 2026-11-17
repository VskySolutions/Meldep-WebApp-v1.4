using System;

namespace Vsky.Services.Authorization
{
    public enum AuthorizationMode
    {
        Off,
        Audit,
        Enforce
    }

    /// <summary>
    /// The "Authorization" section of appsettings.json. Every default keeps today's behavior, so
    /// with no section at all the system only logs what it would deny.
    /// </summary>
    public class SecurityAuthorizationOptions
    {
        /// <summary>Audit logs a would-be denial and allows the request. Enforce denies it.</summary>
        public AuthorizationMode Mode { get; set; } = AuthorizationMode.Audit;

        /// <summary>The active-site check has its own switch, because it is enforced first.</summary>
        public AuthorizationMode SiteGateMode { get; set; } = AuthorizationMode.Audit;

        /// <summary>
        /// Applies existing project and owner rules to requests by record ID. Separate because a
        /// link in a notification or email may open a record the user's own list wouldn't show.
        /// </summary>
        public AuthorizationMode RecordScopeMode { get; set; } = AuthorizationMode.Audit;

        /// <summary>Records each write by a logged-in user in the existing log table.</summary>
        public bool ActivityLogging { get; set; } = true;

        /// <summary>Records opening a single record. Off by default, to keep the Log table small.</summary>
        public bool LogRecordAccess { get; set; }

        /// <summary>Records entering a menu module, at most once per user and module every 30 minutes.</summary>
        public bool LogModuleAccess { get; set; }

        /// <summary>
        /// A folder outside the deployment for rows that can't reach the database. If empty, those
        /// rows go only to the application's standard logging.
        /// </summary>
        public string LogFallbackPath { get; set; }

        /// <summary>With Mode = Enforce, the menu links that are enforced so far.</summary>
        public string[] EnforcedLinks { get; set; } = Array.Empty<string>();

        /// <summary>
        /// With Mode = Enforce, enforces every link and denies unclassified actions. This is
        /// separate from an empty EnforcedLinks so a missing list can't lock everyone out.
        /// </summary>
        public bool EnforceAllLinks { get; set; }

        /// <summary>
        /// Treats Show = 1, View = 0, Manage = 0 rows as full access, as they are today. The live
        /// admin screen can't set View or Manage, so these rows mean "not yet configured".
        /// </summary>
        public bool LegacyShowOnlyAccess { get; set; } = true;

        /// <summary>
        /// Also requires Modules.Active, SitesModules.Active and SitesModulesMenus.Active. Keep this
        /// off until the data is corrected: in the audited database every module has Active = 0,
        /// and most site-level rows do too, while today's menu ignores all three.
        /// </summary>
        public bool EnforceSiteLevelActiveFlags { get; set; }

        /// <summary>Shared secret that service callers such as the cron scheduler must send.</summary>
        public string ServiceKey { get; set; }
    }
}
