export default [
  {
    path: "/sites",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "sites", component: () => import("modules/sites/pages/index.vue"), meta: { requiresAuth: true, title: "Sites" } },
      // { path: "sitespermissions", name: "sitespermissions", component: () => import("modules/sites/pages/sitePermissions.vue"), meta: { title: "sitespermissions" } },
      { path: "site-menu-role-permissions", name: "site-menu-role-permissions", component: () => import("modules/sites/pages/siteMenuRolePermissions.vue"), meta: { title: "siteMenuRolePermissions" } },
      { path: "site-users", name: "site-users", component: () => import("modules/user-management/pages/index.vue"), meta: { requiresAuth: true, title: "Users" } }
    ]
  }
];
