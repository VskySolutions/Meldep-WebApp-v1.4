export default [
  {
    path: "/project-tasks",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "tasks", component: () => import("modules/project-tasks/pages/index.vue"), meta: { requiresAuth: true, title: "Tasks" } },
      { path: "taskListDetails", name: "taskListDetails", component: () => import("modules/project-tasks/pages/taskListDetails.vue"), meta: { requiresAuth: true, title: "Tasks Grid" } }
    ]
  }
];
