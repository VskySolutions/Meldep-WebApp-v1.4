export default [
  {
    path: "/infra-account-services",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "infra-account-services", component: () => import("modules/infra-account-services/pages/index.vue"), meta: { requiresAuth: true, title: "Infra Account Services" } }
    ]
  }
];
