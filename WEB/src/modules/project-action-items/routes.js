export default [
  {
    path: "/project-action-items",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "project-action-items", component: () => import("modules/project-action-items/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Project Action Items" } }
    ]
  }
];
