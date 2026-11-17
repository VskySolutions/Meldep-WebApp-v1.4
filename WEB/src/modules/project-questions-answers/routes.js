export default [
  {
    path: "/project-questions-answers",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "list", name: "project-questions-answers", component: () => import("modules/project-questions-answers/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Project Questions Answers" } }
    ]
  }
];
