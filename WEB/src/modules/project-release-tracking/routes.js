export default [
  {
    path: "/project-release-tracking",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "list", name: "project-release-tracking", component: () => import("modules/project-release-tracking/pages/index.vue"), meta: { requiresAuth: true, title: "Project Release Tracking" } }
    ]
  }
];
