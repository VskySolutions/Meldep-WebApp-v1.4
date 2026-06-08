export default [
  {
    path: "/test-case",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "test-case", component: () => import("modules/test-case/pages/index.vue"), meta: { requiresAuth: true, title: "Test Case" } }
    ]
  }
];
