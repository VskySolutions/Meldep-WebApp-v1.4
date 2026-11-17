export default [
  {
    path: "/infra-project-instance",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "list", name: "infra-project-instance", component: () => import("modules/infra-project-instance/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/infra-project-instance/list"], level: "view" }, title: "Infra Project Instance" } }
    ]
  }
];
