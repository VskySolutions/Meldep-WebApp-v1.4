export default [
  {
    path: "/infra-dashboard",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "infra-dashboard",
        component: () => import("modules/infra-dashboard/pages/index.vue"),
        meta: { requiresAuth: true, title: "Infrastructure Dashboard" }
      }
    ]
  }
];
