export default [
  {
    path: "/project-tasks-activities",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "/my-task-and-activities", name: "my-task-and-activities", component: () => import("modules/project-tasks-activities/pages/my-task-and-activities.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "My Task Activities" } }
    ]
  }
];
