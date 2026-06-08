export default [
  {
    path: "/project-modules",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "project-modules", component: () => import("modules/project-modules/pages/index.vue"), meta: { requiresAuth: true, title: "Project Modules" } }
    ]
  }
];
