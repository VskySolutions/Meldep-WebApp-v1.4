export default [
  {
    path: "/test-plan",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "test-plan", component: () => import("modules/test-plan/pages/index.vue"), meta: { requiresAuth: true, title: "Test Plan" } }
    ]
  }
];
