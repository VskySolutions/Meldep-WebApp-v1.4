export default [
  {
    path: "/sites-sharing",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "Share My Tenant", component: () => import("modules/sites-sharing/pages/index.vue"), meta: { requiresAuth: true, title: "Share My Tenant" } }
    ]
  }
];
