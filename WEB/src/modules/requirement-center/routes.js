export default [
  {
    path: "/requirement-center",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "requirement-center", component: () => import("modules/requirement-center/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/requirement", "/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Requirement Center" } }
    ]
  }
];
