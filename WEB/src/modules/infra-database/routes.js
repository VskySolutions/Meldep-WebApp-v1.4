export default [
  {
    path: "/infra-database",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "list", name: "infra-database", component: () => import("modules/infra-database/pages/index.vue"), meta: { requiresAuth: true, title: "Infra Database" } }
    ]
  }
];
