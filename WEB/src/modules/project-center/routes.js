export default [
  {
    path: "/project-center",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "project-center", component: () => import("modules/project-center/pages/index.vue"), meta: { requiresAuth: true, title: "Project Center" } }
    ]
  }
];
