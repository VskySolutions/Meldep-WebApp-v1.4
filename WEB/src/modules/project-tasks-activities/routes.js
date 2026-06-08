export default [
  {
    path: "/project-tasks-activities",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "/my-task-and-activities", name: "my-task-and-activities", component: () => import("modules/project-tasks-activities/pages/my-task-and-activities.vue"), meta: { requiresAuth: true, title: "My Task Activities" } }
    ]
  }
];
