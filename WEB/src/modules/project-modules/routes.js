export default [
  {
    path: "/project-modules",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "project-modules", component: () => import("modules/project-modules/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Project Modules" } }
    ]
  }
];
