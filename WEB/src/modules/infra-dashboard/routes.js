export default [
  {
    path: "/infra-dashboard",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "infra-dashboard",
        component: () => import("modules/infra-dashboard/pages/index.vue"),
        meta: { requiresAuth: true, permission: { menu: ["/infra-account", "/infra-account-services", "/infra-database/list", "/infra-ftp/list", "/infra-project-instance/list", "/server", "/domain"], level: "view" }, title: "Infrastructure Dashboard" }
      }
    ]
  }
];
