// siteAdmin is always checked, whatever the rollout mode, because these APIs always require it.
const meta = title => ({ requiresAuth: true, siteAdmin: true, title });

export default [
  {
    path: "/security-admin",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "security-admin", component: () => import("modules/security-admin/pages/overview.vue"), meta: meta("Security Administration") },
      { path: "users", name: "security-admin-users", component: () => import("modules/security-admin/pages/users.vue"), meta: meta("Security Users") },
      { path: "roles", name: "security-admin-roles", component: () => import("modules/security-admin/pages/roles.vue"), meta: meta("Security Roles") },
      { path: "groups", name: "security-admin-groups", component: () => import("modules/security-admin/pages/groups.vue"), meta: meta("Security Groups") },
      { path: "permissions", name: "security-admin-permissions", component: () => import("modules/security-admin/pages/permissions.vue"), meta: meta("Permissions") },
      { path: "site-access", name: "security-admin-site-access", component: () => import("modules/security-admin/pages/siteAccess.vue"), meta: meta("Site Access") },
      { path: "effective", name: "security-admin-effective", component: () => import("modules/security-admin/pages/effective.vue"), meta: meta("Effective Permissions") },
      { path: "audit", name: "security-admin-audit", component: () => import("modules/security-admin/pages/audit.vue"), meta: meta("Security Audit Log") }
    ]
  }
];
