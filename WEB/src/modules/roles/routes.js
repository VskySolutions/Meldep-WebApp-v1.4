export default [
  {
    path: "/site-roles",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "/site-roles", name: "site-roles", component: () => import("modules/roles/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/site-roles", "/master-roles", "/sites"], level: "view" }, title: "SiteRoles" } },
      { path: "/master-roles", name: "master-roles", component: () => import("modules/roles/pages/masterRoles.vue"), meta: { requiresAuth: true, permission: { menu: ["/master-roles", "/sites"], level: "view" }, title: "MasterRoles" } }
    ]
  }
];
