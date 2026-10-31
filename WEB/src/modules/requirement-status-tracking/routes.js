export default [
  {
    path: "/requirement-status-tracking",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "requirement-status-tracking", component: () => import("modules/requirement-status-tracking/pages/index.vue"), meta: { requiresAuth: true, title: "Requirement Status Tracking" } }
    ]
  }
];
