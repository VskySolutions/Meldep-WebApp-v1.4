export default [
  {
    path: "/project",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "project", component: () => import("modules/project/pages/index.vue"), meta: { requiresAuth: true, title: "Projects" } },
      { path: "/project/assign-users-to-project", name: "assign-users-to-project", component: () => import("modules/project/pages/assignUsersToProject.vue"), meta: { requiresAuth: true, title: "Project Security" } },
      { path: "/project/projectTaskNotes", name: "projectTaskNotes", component: () => import("modules/project/pages/projectTaskNotes.vue"), meta: { requiresAuth: true, title: "Project Task Notes" } }
    ]
  }
];
