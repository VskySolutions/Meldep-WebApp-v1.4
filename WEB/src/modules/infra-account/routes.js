export default [
  {
    path: "/infra-account",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "infra-account", component: () => import("modules/infra-account/pages/index.vue"), meta: { requiresAuth: true, title: "Infra Account" } }
    ]
  }
];
