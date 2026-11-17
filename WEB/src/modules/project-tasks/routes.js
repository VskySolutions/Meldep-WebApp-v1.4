export default [
  {
    path: "/project-tasks",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "tasks", component: () => import("modules/project-tasks/pages/index.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Tasks" } },
      { path: "taskListDetails", name: "taskListDetails", component: () => import("modules/project-tasks/pages/taskListDetails.vue"), meta: { requiresAuth: true, permission: { menu: ["/project", "/project-tasks", "/my-task-and-activities", "/all-project-planner", "/project-targetplan/weeklyplanner", "/project-targetplan/monthlyplanner", "/billing-processing-employee", "/project/assign-users-to-project", "/project-questions-answers/list", "/project-release-tracking/list", "/timesheet", "/my-daily-planner"], level: "view" }, title: "Tasks Grid" } }
    ]
  }
];
