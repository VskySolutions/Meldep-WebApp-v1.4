export default [
  {
    path: "/infra-project-instance",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "list", name: "infra-project-instance", component: () => import("modules/infra-project-instance/pages/index.vue"), meta: { requiresAuth: true, title: "Infra Project Instance" } }
    ]
  }
];
