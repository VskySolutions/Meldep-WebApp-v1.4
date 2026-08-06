export default [
  {
    path: "/project-action-items",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "project-action-items", component: () => import("modules/project-action-items/pages/index.vue"), meta: { requiresAuth: true, title: "Project Action Items" } }
    ]
  }
];
