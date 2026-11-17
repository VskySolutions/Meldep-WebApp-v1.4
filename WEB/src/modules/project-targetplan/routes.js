export default [
  {
    path: "/project-targetplan",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "/project-targetplan/weeklyplanner", name: "weeklyplanner", component: () => import("modules/project-targetplan/pages/weeklyPlanner.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Weekly Target Plan" } },
      { path: "/project-targetplan/monthlyplanner", name: "monthlyplanner", component: () => import("modules/project-targetplan/pages/monthlyPlanner.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Monthly Target Plan" } }
    ]
  }
];
